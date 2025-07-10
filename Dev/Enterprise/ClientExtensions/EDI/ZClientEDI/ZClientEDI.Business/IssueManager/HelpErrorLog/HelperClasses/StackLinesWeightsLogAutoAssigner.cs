using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class StackLinesWeightsLogAutoAssigner : IIssueAssignmentCalculator
	{
		readonly HelpErrorLogStackLineExtractor stackLineExtractor = new HelpErrorLogStackLineExtractor();

		public IssueAssignment GetAssignment(EdiHelpErrorLog log, DataFormatter formatter)
		{
			formatter.CreateTable("Issue Information", new List<string> { "Name", "Value" });

			formatter.AddRowToTable("Issue Information", new List<string> { "PK", log.PK.ToString() });
			formatter.AddRowToTable("Issue Information", new List<string> { "ExceptionMessage", log.ExceptionMessageFirstLine });

			IssueAssignment assignment = null;
			var stackLines = GetLogStackLines(log, formatter);

			formatter.CreateTable("Stackline Weights", new List<string> { "Weight", "Count", "Count Relevance", "Position", "Position Relevance",
																		  "Assembly", "Type", "Method", "Parameters", "Stackline" });
			formatter.OrderTableBy("Stackline Weights", "Weight", ascending: false);

			formatter.CreateTable("Assembly Paths", new List<string> { "Assembly", "Assembly Source Path", "Matching Responsibility Path", "Matching Assignment" });

			var stackLineWeights = CalculateStackLineWeights(log.Factory, stackLines, formatter);
			var assemblyWeights = stackLineWeights.Where(weight => !string.IsNullOrEmpty(weight.StackLine.Assembly))
													.GroupBy(weight => weight.StackLine.Assembly)
													.Select(g => new { Assembly = g.Key, Weight = g.Max(a => a.Weight), Ignored = g.First().Ignored })
													.OrderByDescending(a => a.Weight).ToArray();
			var assignmentCandidates = (from assemblyWeight in assemblyWeights
										let assemblySourcePath = GetAssemblySourcePath(assemblyWeight.Assembly)
										let candidateAssignment = !string.IsNullOrEmpty(assemblySourcePath) ? GetAssignmentForSourcePath(assemblySourcePath, assemblyWeight.Assembly, formatter) : null
										where candidateAssignment != null
										select new AssignmentCandidate
										{
											Weight = assemblyWeight.Weight,
											Assembly = assemblyWeight.Assembly,
											Assignment = candidateAssignment,
											Ignored = assemblyWeight.Ignored
										}).ToList(); // this could impact performance since we no longer defer execution for when we need an element but when AI is enabled
													 // this data will be serialised and enumerated anyway so it should not matter much

			foreach (var stackLineWeight in stackLineWeights)
			{
				formatter.AddRowToTable("Stackline Weights", new List<string> { stackLineWeight.Weight.ToString("0.###") ?? "null",
																				stackLineWeight.StackLineCount.ToString(),
																				stackLineWeight.FrequencyRelevance.ToString("0.###"),
																				stackLineWeight.PositionNumber.ToString(),
																				stackLineWeight.PositionRelevance.ToString("0.###"),
																				stackLineWeight.StackLine.Assembly ?? "null",
																				stackLineWeight.StackLine.Type ?? "null",
																				stackLineWeight.StackLine.Method ?? "null",
																				stackLineWeight.StackLine.Parameters ?? "null",
																				stackLineWeight.StackLine.FullStackLine ?? "null" });
			}

			formatter.CreateTable("Assignment Candidates", new List<string> { "Weight", "Assembly", "Assignment" });
			formatter.OrderTableBy("Assignment Candidates", "Weight", ascending: false);

			foreach (var candidate in assignmentCandidates)
			{
				formatter.AddRowToTable("Assignment Candidates", new List<string> { candidate.Weight.ToString("0.###"), candidate.Assembly, candidate.Assignment.ToString() });
			}

			var hightestAssignmentCandidate = assignmentCandidates?.FirstOrDefault();
			formatter.LogMessage($"Team assignment: {hightestAssignmentCandidate?.Assignment}");

			if (MachineLearningTeamAssignmentRetriever.IsEnabled)
			{
				formatter.LogMessage("Machine learning team assignment is enabled, getting assignment...");
				assignment = MachineLearningTeamAssignmentRetriever.GetIssueAssignmentAsync(stackLines, assignmentCandidates, log).Result;

				if (assignment == null)
				{
					formatter.LogMessage("Machine learning team assignment returned a null assignment, falling back to using first assignment candidate");
					assignment = hightestAssignmentCandidate?.Assignment;
				}
			}
			else
			{
				formatter.LogMessage("Machine learning team assignment is disabled, choosing first assignment candidate as assignment");
				if (hightestAssignmentCandidate?.Ignored ?? false)
				{
					formatter.LogMessage("Ignored weight stack line is the highest assignment candidate, can't give a reasonable assignment candidate.");
					return null;
				}
				assignment = hightestAssignmentCandidate?.Assignment;
			}

			return assignment;
		}

		public MachineLearningTeamAssignmentRetriever MachineLearningTeamAssignmentRetriever { get; set; } = new MachineLearningTeamAssignmentRetriever();

		StackLine[] GetLogStackLines(EdiHelpErrorLog log, DataFormatter formatter)
		{
			var occurrence = log.Factory.LoadTop1<HelpErrorLogOccurrence>(new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, log.PK) { OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_EXEDateTime + " DESC" });
			if (occurrence != null)
			{
				formatter.LogMessage($"Found occurrence, extracting stacklines...");

				formatter.CreateTable("Occurrence Information", new List<string> { "Name", "Value" });

				formatter.AddRowToTable("Occurrence Information", new List<string> { "PK", occurrence.PK.ToString() });
				formatter.AddRowToTable("Occurrence Information", new List<string> { "ExceptionDateTime", occurrence.HO_ExceptionDateTime.ToLongTimeString() });
				formatter.AddRowToTable("Occurrence Information", new List<string> { "ExceptionID", occurrence.HO_ExceptionID });
				formatter.AddRowToTable("Occurrence Information", new List<string> { "SessionID", occurrence.SessionIdAsText });

				return GetStackLinesFromLog(occurrence.HO_XMLData).Reverse().ToArray();
			}
			else
			{
				formatter.LogMessage("No occurences found, using empty stackline array.");
				return Array.Empty<StackLine>();
			}
		}

		IEnumerable<StackLine> GetStackLinesFromLog(string logXML)
		{
			try
			{
				return stackLineExtractor.ReadStackLines(logXML);
			}
			catch (XmlException)
			{
				// invalid XML
				return Enumerable.Empty<StackLine>();
			}
		}

		static List<StackLineWeight> CalculateStackLineWeights(BusinessObjectFactory factory, StackLine[] stackLines, DataFormatter formatter)
		{
			formatter.LogMessage("Calculating stackline weights...");

			var stackLineCounts = LoadHelpErrorStackLineCounts(factory, stackLines);
			var stackLineCountLookup = stackLineCounts.ToDictionary(c => (string)c.HSL_StackLine, StringComparer.OrdinalIgnoreCase);
			var stackLineWeights = new List<StackLineWeight>(stackLines.Length);
			var totalWeight = 0D;

			var stackLineCountMax = 1;

			if (stackLineCounts.Length > 0)
			{
				stackLineCountMax = stackLineCounts.Max(c => c.HSL_Count);
				formatter.LogMessage($"Using Stackline count max of: {stackLineCountMax}");
			}
			else
			{
				formatter.LogMessage("No matching stacklines found, using default max count of 1.");
			}

			var frequencyNormalizer = Math.Log(stackLineCountMax + 2);

			formatter.LogMessage($"Using frequency normalizer: {frequencyNormalizer:0.###}");

			formatter.LogMessage($"Total number of stacklines: {stackLines.Length}");

			for (var i = 0; i < stackLines.Length; i++)
			{
				var stackLine = stackLines[i];
				stackLineCountLookup.TryGetValue(stackLine.FullStackLine, out var stackLineCount);
				var stackLineOccurrences = stackLineCount?.HSL_Count ?? 1;
				var frequencyRelevance = 1 - (Math.Log(stackLineOccurrences + 1) / frequencyNormalizer);
				var positionNumber = i + 1;

				var positionRelevance = (double)positionNumber / stackLines.Length;
				var weight = 0d;
				var isMatchIgnoredRegex = IsMatchIgnoredStackLineRegex(stackLine.FullStackLine, formatter);
				if (!isMatchIgnoredRegex)
				{
					weight = positionRelevance * frequencyRelevance;
				}

				if (string.IsNullOrEmpty(stackLine.Assembly) && !string.IsNullOrEmpty(stackLineCount?.HSL_Assembly))
				{
					stackLine.Assembly = stackLineCount.Assembly;
					stackLine.Method = stackLineCount.Method;
				}

				stackLineWeights.Add(new StackLineWeight()
				{
					FrequencyRelevance = frequencyRelevance,
					PositionNumber = positionNumber,
					PositionRelevance = positionRelevance,
					StackLineCount = stackLineOccurrences,
					StackLine = stackLine,
					Weight = weight,
					Ignored = isMatchIgnoredRegex,
				});
				totalWeight += weight;
			}

			formatter.LogMessage($"Total weight of stacklines: {totalWeight:0.###}");

			StackLineAssemblyLookup.UpdateMissingAssemblyInformation(stackLines);
			foreach (var weight in stackLineWeights)
			{
				weight.Weight /= totalWeight;
			}

			return stackLineWeights;
		}

		static bool IsMatchIgnoredStackLineRegex(string fullStackLine, DataFormatter formatter)
		{
			var stackLineRegexCollection = EDIDataRegistry.Instance.IgnoredExceptionStackLineRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var combinedRegex = stackLineRegexCollection.Select(e => e as ExceptionKeyRegex)
				.WhereNotNull()
				.Select(e => e.Regex)
				.Where(regex => !string.IsNullOrEmpty(regex))
				.Aggregate(string.Empty, (agg, cur) => string.IsNullOrEmpty(agg) ? cur : $"({agg})|({cur})");

			var isMatchRegex = !string.IsNullOrEmpty(fullStackLine) && !string.IsNullOrEmpty(combinedRegex) && Regex
				.Matches(fullStackLine, combinedRegex)
				.Cast<Match>().Any(match => match.Success);
			if (isMatchRegex)
			{
				formatter.LogMessage($"Stack Line: {fullStackLine} match lower height exception stack line regex, the stack line weight will be set as 0");
			}
			return isMatchRegex;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "External database")]
		static IssueAssignment GetAssignmentForSourcePath(string sourcePath, string assembly, DataFormatter formatter)
		{
			if (!sourcePath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				sourcePath += "/";
			}

			var query = @"SELECT TOP 1 ST_Path, ST_Product, ST_Product_Area, ST_Module FROM SourceTreeResponsibility";

			if (sourcePath.StartsWith("https://github.com", StringComparison.OrdinalIgnoreCase))
			{
				query += @" WHERE @SourcePath like ST_Path +'?path=%'
						OR @SourcePath LIKE ST_Path +'/%'
						OR (@SourcePath LIKE ST_Path + '.%' AND ST_Path NOT LIKE 'https://github.com/%/%.%')";
			}
			else
			{
				query += " WHERE @SourcePath like ST_Path + '/%' OR @SourcePath like ST_Path + '?path=/%'";
			}

			query += " ORDER BY LEN(ST_Path) DESC";

			IssueAssignment result = null;
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var command = connection.Command(query))
			{
				command.AddParameter("@SourcePath", SqlDbType.NVarChar, sourcePath);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						result = new IssueAssignment(reader["ST_Product"] as string, reader["ST_Product_Area"] as string, reader["ST_Module"] as string);

						formatter.AddRowToTable("Assembly Paths", new List<string> { assembly, sourcePath, reader["ST_Path"] as string, result.ToString() });
					}
					else
					{
						formatter.AddRowToTable("Assembly Paths", new List<string> { assembly, sourcePath, "Not found", "Not found" });
					}
				}
			}
			return result;
		}

		static string GetAssemblySourcePath(string assembly)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var command = connection.Command("SELECT PA_SourcePath FROM PublishedAssemblies WHERE PA_AssemblyName = @Assembly;"))
			{
				command.AddParameter("@Assembly", SqlDbType.NVarChar, assembly);
				return command.ExecuteScalar() as string;
			}
		}

		static HelpErrorStackLineCount[] LoadHelpErrorStackLineCounts(BusinessObjectFactory factory, StackLine[] stackLines)
		{
			return factory.Load<HelpErrorStackLineCount>(new ZQuery(HelpErrorStackLineCountSchema.HSL_StackLine, stackLines.Select(s => s.FullStackLine).Distinct(StringComparer.OrdinalIgnoreCase)));
		}

		public class AssignmentCandidate
		{
			public double Weight { get; set; }
			public string Assembly { get; set; }
			[JsonIgnore]
			public bool Ignored { get; set; }
			public IssueAssignment Assignment { get; set; }
		}
	}

	class StackLineWeight
	{
		public double FrequencyRelevance { get; set; }

		public int PositionNumber { get; set; }

		public double PositionRelevance { get; set; }

		public StackLine StackLine { get; set; }

		public int StackLineCount { get; set; }

		public double Weight { get; set; }

		public bool Ignored { get; set; }

		public override string ToString()
		{
			return StackLine.FullStackLine + " - " + Weight.ToString(CultureInfo.InvariantCulture);
		}
	}
}
