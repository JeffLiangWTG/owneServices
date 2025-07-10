using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.ExcelComparator
{
	public class InsertMissingSectionsOption : IComparableTextGeneratorOption
	{
		public InsertMissingSectionsOption(string filePath)
		{
			this.filePath = filePath;
		}
		readonly string filePath;

		public string RunOptionOnWorkSheetContents(string original, int workSheetNumber)
		{
			if (workSheetNumber <= MissingSections.Count)
			{
				var modifiedContents = new StringBuilder();
				var workSheetMissingSections = MissingSections[workSheetNumber - 1];
				var missingSectionsIndex = 0;
				var originalAsLines = original.Split(new string[] { "\r\n" }, StringSplitOptions.None);
				foreach (var line in originalAsLines)
				{
					if (line.EndsWith(configurableSectionPrefix.Trim()))
					{
						while (missingSectionsIndex < workSheetMissingSections.Length && line.CompareTo(workSheetMissingSections[missingSectionsIndex]) > 0)
						{
							if (!line.StartsWith(workSheetMissingSections[missingSectionsIndex]))
							{
								AppendMissingSection(modifiedContents, workSheetMissingSections[missingSectionsIndex]);
							}
							missingSectionsIndex++;
						}
					}

					modifiedContents.AppendLine(line);
				}
				while (missingSectionsIndex < workSheetMissingSections.Length)
				{
					AppendMissingSection(modifiedContents, workSheetMissingSections[missingSectionsIndex]);
					missingSectionsIndex++;
				}

				return modifiedContents.ToString();
			}
			else
			{
				return original;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void AppendMissingSection(StringBuilder contents, string sectionName)
		{
			contents.Append(sectionName + configurableSectionPrefix);
			contents.AppendLine("  <Section Missing>");
			contents.Append(sectionName + configurableSectionPostfix);
		}

		List<string[]> missingSections;
		List<string[]> MissingSections
		{
			get { return missingSections ?? (missingSections = GetNewMissingSections()); }
		}

		List<string[]> GetNewMissingSections()
		{
			var sections = new List<string[]>();
			using (var excelInterface = new ExcelInterface())
			{
				try
				{
					excelInterface.LoadExcelFileWithoutCallingResDotGetString(filePath);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					return sections;
				}

				try
				{
					foreach (var workSheet in excelInterface.WorkSheets)
					{
						sections.Add(GetWorkSheetSections(workSheet));
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					return sections;
				}
			}
			return sections;
		}

		string[] GetWorkSheetSections(ExcelWorkSheet workSheet)
		{
			var sections = new List<string>();
			var workSheetContents = workSheet.ToString(new CellFormatter());
			var contentAsLines = workSheetContents.Split(new string[] { "\r\n" }, StringSplitOptions.None);

			foreach (var line in contentAsLines)
			{
				var sectionMatch = configurableSectionRegex.Match(line);
				if (sectionMatch.Success)
				{
					sections.Add(sectionMatch.Groups["Section_Name"].Value);
				}
			}
			sections.Sort((x, y) => x.CompareTo(y));
			return sections.ToArray();
		}

		readonly Regex configurableSectionRegex = new Regex(@"\[#ConfigurableSection:.*, (?<Section_Name>.+)\]");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string configurableSectionPrefix = " --- Section Start --------------------------\r\n";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string configurableSectionPostfix = " --- Section End --------------------------\r\n";
	}
}
