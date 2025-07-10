using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS.DevTools.RefCusProcedures.Testing
{
	class CreateRefCusProcedures : TestCaseWithFactory
	{
		[TestDate(2018, 10, 15, 15, 10, 0)]
		public void TestFakeImports()
		{
			new GeneratorWithListStyleCrossReference().GenerateLatestRefCusProcedureListSoBrendonCanInsertIt();
		}

		[TestDate(2018, 11, 27, 15, 10, 0)]
		public void TestFakeInventoryImports()
		{
			new GeneratorWithMatrixStyleCrossReference().GenerateLatestRefCusProcedureListSoBrendonCanInsertIt();
		}

		[TestDate(2018, 11, 27, 15, 10, 0)]
		public void TestFakeInventoryExports()
		{
			new GeneratorForInventoryExports().GenerateLatestRefCusProcedureListSoBrendonCanInsertIt();
		}

		[TestDate(2019, 3, 13, 15, 10, 0)]
		public void TestFakeExports()
		{
			new GeneratorForExports().GenerateLatestRefCusProcedureListSoBrendonCanInsertIt();
		}

		class GeneratorWithListStyleCrossReference
		{
			public void GenerateLatestRefCusProcedureListSoBrendonCanInsertIt()
			{
				// ReadFile the README.txt file

				var allPrexfixes = ReadTabSeparatedEmbeddedDataFile("Prefix.txt");  // Definitions of all possible 4-char prefixes (e.g. 4071) with descriptions. 
				var allSuffixes = ReadTabSeparatedEmbeddedDataFile(suffixFileName); // Definitions of all possible 3-char suffixes (e.g. 000) with descriptions.
				var allCombinations = ReadTabSeparatedEmbeddedDataFileForCrossReference(crossReferenceFileName);    // List of which 4-char prefixes are allowed with which 3-char suffixes. 
				var allGroups = ReadTabSeparatedEmbeddedDataFile("Category.txt");           // List of which 4-char prefixes can be used with (or rather, define) declaration categories. e.g. 4000 = H1, H5, 
				var combinations = TurnMultipleDelimitedValuesIntoFurtherList(allCombinations);
				var groups = TurnMultipleDelimitedValuesIntoFurtherList(allGroups);
				var totalSqlForBrendon = new List<string>();
				MakeSqlUpdatesOrInserts(totalSqlForBrendon, allPrexfixes, allSuffixes, combinations, groups);
				AppendDeleteAllAtStartThenKnownUpdateRules(totalSqlForBrendon); // Creates a delete statement to flush data, and then makes a set of known edits to the new data
				AssertAllLinesAreEqual(ReadFile(expectedInsertResults), totalSqlForBrendon);
			}

			protected virtual void AppendDeleteAllAtStartThenKnownUpdateRules(List<string> sqlLines)
			{
				var fixVatF45 = Invariant($"update {TableName}  set {RefCusProcedure.Schema.ZZ6_CalculateVAT} = 0  where {RefCusProcedure.Schema.ZZ6_Concession} = 'F45' and  {RefCusProcedure.Schema.ZZ6_ZZZ_NKDataGrouping} = 'CDS'");
				var fixDutyC30 = Invariant($"update {TableName}  set {RefCusProcedure.Schema.ZZ6_CalculateDuty} = 0  where {RefCusProcedure.Schema.ZZ6_Concession} = 'C30' and  {RefCusProcedure.Schema.ZZ6_ZZZ_NKDataGrouping} = 'CDS'");
				sqlLines.Add("");
				sqlLines.Add(fixVatF45);
				sqlLines.Add(fixDutyC30);
			}

			void AssertAllLinesAreEqual(IList<string> expectedLines, IList<string> actualLines)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Number of SQL lines", expectedLines.Count, actualLines.Count);
					if (expectedLines.Count == actualLines.Count)
					{
						for (var index = 0; index < actualLines.Count; index++)
						{
							AssertMultilineASCIIEquals($"Line {index + 1} should be equal", expectedLines[index].TrimEnd(), actualLines[index].TrimEnd());
						}
					}
				});
			}

			const string TableName = nameof(RefCusProcedure);

			void MakeSqlUpdatesOrInserts(List<string> allEdits, Dictionary<string, string> definitionOfPrefixes, Dictionary<string, string> definitionsOfSuffixes, Dictionary<string, List<string>> combinationsOfPrefixAndSuffix, Dictionary<string, List<string>> categoriesWithPrefixes)
			{
				var now = ZDateTime.Now.ToString("yyyy-MM-dd HH:mm");

				foreach (var combo in combinationsOfPrefixAndSuffix)
				{
					var groupsList = GetAllowedDeclarationGroupsForGivenPrefix(combo, categoriesWithPrefixes);
					foreach (var suffix in combo.Value)
					{
						var fourCharPrefix = combo.Key.Trim();
						var requested = fourCharPrefix.Substring(0, 2);
						var previous = fourCharPrefix.Substring(2, 2);
						var concession = suffix;
						var description = definitionOfPrefixes[fourCharPrefix.Replace("'", "")] + " -- " + definitionsOfSuffixes[suffix].Replace("'", "");
						if (allEdits.Count > 0)
						{
							allEdits.Add("");
							allEdits.Add("");
						}
						var statementBlock = Invariant($@"print '{requested}{previous}{concession}'
													if (exists(select null from {TableName} where ZZ6_ProcedureCode = '{requested}' and  ZZ6_PreviousProcedureCode = '{previous}' and  ZZ6_Concession = '{concession}' and ZZ6_ZZZ_NKDataGrouping = 'CDS'))
														begin
															Print '		Updating'
															UPDATE {TableName}
															SET ZZ6_Category = '',
																ZZ6_Description  = '{description}',
																ZZ6_ShipmentType = '{impOrExp}',
																ZZ6_Group = '{groupsList}',
																ZZ6_StartDate = '{now}',
																ZZ6_EndDate  = '2079-06-06 23:59'
															WHERE ZZ6_ProcedureCode = '{requested}' and  ZZ6_PreviousProcedureCode = '{previous}' and  ZZ6_Concession = '{concession}' and ZZ6_ZZZ_NKDataGrouping = 'CDS'
														end
													else
														begin
															Print '		Inserting'
															INSERT INTO {TableName}
															(
																{RefCusProcedure.Schema.PK}, ZZ6_Category, ZZ6_ProcedureCode, ZZ6_PreviousProcedureCode, ZZ6_Concession,
																ZZ6_Description, ZZ6_ZZZ_NKDataGrouping, ZZ6_ShipmentType, ZZ6_Group, ZZ6_StartDate, ZZ6_EndDate
															)
															VALUES
															(
																newid(), '', '{requested}', '{previous}', '{concession}',
																'{description}', 'CDS', '{impOrExp}', '{groupsList}', '{now}', '2079-06-06 23:59'
															)
														end");
						allEdits.AddRange(Regex.Split(statementBlock, "\r\n"));
					}
				}
			}

			string GetAllowedDeclarationGroupsForGivenPrefix(KeyValuePair<string, List<string>> combo, Dictionary<string, List<string>> categories)
			{
				var prefix = combo.Key;
				var hits = new List<string>();
				foreach (var pair in categories)
				{
					foreach (var subPair in pair.Value)
					{
						if (subPair.Trim() == prefix.Trim())
						{
							hits.Add(pair.Key.Trim());
							continue;
						}
					}
				}
				return string.Join(",", hits.ToArray());
			}

			Dictionary<string, List<string>> TurnMultipleDelimitedValuesIntoFurtherList(Dictionary<string, string> allCombinations)
			{
				var results = new Dictionary<string, List<string>>();
				foreach (var pair in allCombinations)
				{
					var suffixes = pair.Value.Split(new[] { '\t', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					results.Add(pair.Key, suffixes.ToList());
				}
				return results;
			}

			protected virtual Dictionary<string, string> ReadTabSeparatedEmbeddedDataFileForCrossReference(string filename)
			{
				return ReadTabSeparatedEmbeddedDataFile(filename);
			}

			Dictionary<string, string> ReadTabSeparatedEmbeddedDataFile(string filename)
			{
				var allData = ReadFile(filename);
				var results = new Dictionary<string, string>();
				foreach (var row in allData)
				{
					var elements = row.Split(new[] { '\t' }, 2);
					var key = elements[0].Trim();
					var value = elements[1].Trim();
					if (results.ContainsKey(key))
					{
						results[key] = results[key] + "," + value;
					}
					else
					{
						results.Add(key, value);
					}
				}
				return results;
			}

			protected string[] ReadFile(string filename)
			{
				var assembly = GetType().Assembly;
				var foundPath = assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(".DevTools.RefCusProcedure." + filename));
				using (var inStream = assembly.GetManifestResourceStream(foundPath))
				{
					var content = new StreamReader(inStream).ReadToEnd().TrimEnd();
					return Regex.Split(content, "\r\n");
				}
			}

			protected virtual string suffixFileName => "Suffix.txt";
			protected virtual string crossReferenceFileName => "CrossReference.txt";
			protected virtual string impOrExp => "IMP";
			protected virtual string expectedInsertResults => "ExpectedInserts20181219.txt";
		}

		class GeneratorWithMatrixStyleCrossReference : GeneratorWithListStyleCrossReference
		{
			protected override string suffixFileName => "SuffixInventory.txt";
			protected override string crossReferenceFileName => "CrossReferenceInventoryImport.txt";
			protected override string expectedInsertResults => "ExpectedInsertsInventoryImports20181127.txt";

			protected override void AppendDeleteAllAtStartThenKnownUpdateRules(List<string> sqlLines)
			{
			}

			protected override Dictionary<string, string> ReadTabSeparatedEmbeddedDataFileForCrossReference(string filename)
			{
				var allData = ReadFile(filename);
				var results = new Dictionary<string, string>();
				var isFirstRow = true;
				var tickMark = GetTickMarkCharacter();
				var suffixIdentifiersFromFirstRow = new Dictionary<int, string>();
				foreach (var row in allData)
				{
					var elements = row.Split(new[] { '\t' });
					if (isFirstRow)
					{
						isFirstRow = false;
						for (int i = 0; i < elements.Length; i++)
						{
							suffixIdentifiersFromFirstRow[i] = elements[i];
						}
						continue;
					}
					var currentAndPreviousProc = elements[0].Trim();
					var listOfSuffixes = new List<string>();
					for (int i = 1; i < elements.Length; i++)
					{
						if (elements[i] == tickMark)
						{
							listOfSuffixes.Add(suffixIdentifiersFromFirstRow[i]);
						}
					}

					var value = string.Join(",", listOfSuffixes.ToArray());
					if (results.ContainsKey(currentAndPreviousProc))
					{
						results[currentAndPreviousProc] = results[currentAndPreviousProc] + "," + value;
					}
					else
					{
						results.Add(currentAndPreviousProc, value);
					}
				}
				return results;
			}

			protected virtual string GetTickMarkCharacter()
			{
				return "ü";
			}
		}

		class GeneratorForInventoryExports : GeneratorWithMatrixStyleCrossReference
		{
			protected override string crossReferenceFileName => "CrossReferenceInventoryExport.txt";
			protected override string impOrExp => "EXP";
			protected override string expectedInsertResults => "ExpectedInsertsInventoryExports20181127.txt";
		}

		class GeneratorForExports : GeneratorWithMatrixStyleCrossReference
		{
			protected override string suffixFileName => "SuffixExports.txt";
			protected override string crossReferenceFileName => "CrossReferenceExport.txt";
			protected override string impOrExp => "EXP";
			protected override string expectedInsertResults => "ExpectedInsertsExports20190313.txt";
			protected override string GetTickMarkCharacter() => "Y";
		}
	}
}
