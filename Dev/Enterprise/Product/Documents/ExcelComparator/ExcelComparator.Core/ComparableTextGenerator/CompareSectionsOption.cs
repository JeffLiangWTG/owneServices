using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Enterprise.ExcelComparator
{
	public class CompareSectionsOption : IComparableTextGeneratorOption
	{
		public CompareSectionsOption()
		{
		}

		public string RunOptionOnWorkSheetContents(string original, int workSheetNumber)
		{
			var modifiedContents = new StringBuilder();
			var contentAsLines = original.Split(new string[] { "\r\n" }, StringSplitOptions.None);

			var configurableSectionsList = new List<ConfigurableSection>();
			ConfigurableSection lastConfigurableSecion = null;
			for (var lineNumber = 0; lineNumber < contentAsLines.Length; lineNumber++)
			{
				var line = contentAsLines[lineNumber];
				var sectionMatch = configurableSectionRegex.Match(line);
				if (sectionMatch.Success)
				{
					lastConfigurableSecion = new ConfigurableSection(sectionMatch.Groups["Section_Name"].Value, lineNumber + 1);
					configurableSectionsList.Add(lastConfigurableSecion);
				}
				else
				{
					if (lastConfigurableSecion != null)
					{
						var dereferencedLine = line;
						var functionWithCellReferenceRegexMatch = functionWithCellReferenceRegex.Match(line);
						if (functionWithCellReferenceRegexMatch.Success)
						{
							var cellReferenceMatches = cellReferenceRegex.Matches(line);
							foreach (Match cellReferenceMatch in cellReferenceMatches)
							{
								int row;
								int.TryParse(cellReferenceMatch.Groups["Row"].Value, out row);
								string column = cellReferenceMatch.Groups["Column"].Value;
								dereferencedLine = dereferencedLine.Replace(cellReferenceMatch.Value, cellReferenceMatch.Result(column + (row - lastConfigurableSecion.StartingRowNumber).ToString()));
							}
						}

						lastConfigurableSecion.AppendStripContents(dereferencedLine);
					}
					else
					{
						modifiedContents.AppendLine(" " + line);
					}
				}
			}

			modifiedContents.AppendLine(ConfigurableSectionsToString(configurableSectionsList));
			return modifiedContents.ToString();
		}

		string ConfigurableSectionsToString(List<ConfigurableSection> configurableSections)
		{
			var formattedString = new StringBuilder();
			configurableSections.Sort((x, y) => x.Name.CompareTo(y.Name));
			foreach (var section in configurableSections)
			{
				formattedString.Append(section.StripContents);
			}
			return formattedString.ToString();
		}

		class ConfigurableSection
		{
			public readonly string Name;
			public int StartingRowNumber;
			readonly StringBuilder stripContents;

			public ConfigurableSection(string sectionName, int row)
			{
				Name = sectionName;
				StartingRowNumber = row;
				stripContents = new StringBuilder();
			}

			public string StripContents
			{
				get
				{
					return (Name + configurableSectionPrefix)
						+ stripContents.ToString()
						+ Name + configurableSectionPostfix;
				}
			}

			public void AppendStripContents(string contents)
			{
				stripContents.AppendLine(" " + contents);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
			const string configurableSectionPrefix = " --- Section Start --------------------------\r\n";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
			const string configurableSectionPostfix = " --- Section End --------------------------\r\n";
		}

		readonly Regex configurableSectionRegex = new Regex(@"\[#ConfigurableSection:.*, (?<Section_Name>.+)\]");
		readonly Regex functionWithCellReferenceRegex = new Regex(@"-\[=.*(?<CellReference>(?<Column>[A-Z]+)(?<Row>[0-9]+)).*\]");
		readonly Regex cellReferenceRegex = new Regex(@"(?<Column>[A-Z]+)(?<Row>[0-9]+)");
	}
}
