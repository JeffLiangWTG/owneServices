using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CWNetCoreTest.TestAdapter.Utilities;

namespace CWNetCoreTest.TestAdapter.Test
{
	public class AssemblyResourceProviderTests : TestCaseWithFactory
	{
		const string ResourceDirectory = "Test_CWNetCoreTestAdapterResources";
		static readonly string resourceDirectoryPath = Path.Combine(AppContext.BaseDirectory, ResourceDirectory);

		protected override void SetUp()
		{
			base.SetUp();
			_ = Directory.CreateDirectory(resourceDirectoryPath);
			foreach (var file in Directory.GetFiles(resourceDirectoryPath))
			{
				File.Delete(file);
			}
		}

		protected override void TearDown()
		{
			Directory.Delete(resourceDirectoryPath);
			base.TearDown();
		}

		#region Utilities
		static HashSet<string> RetrieveTestResourceLists(string fileName, Func<string, bool>? filter)
		{
			return new AssemblyResourceProvider().LoadExplicitTestsFromContentFiles(fileName, filter);
		}

		static int TestResourceListsLineCount(string fileName, Func<string, bool>? filter) => RetrieveTestResourceLists(fileName, filter).Count;

		static (int TotalLines, int TestCases, int DuplicateTestCases, int Comments, int EmptyOrWhiteSpaceLines) TestResourceListsDetails(string resourceDirectory)
		{
			var testCases = 0;
			var caseSensitiveDuplicateTestCases = 0;
			var comments = 0;
			var emptyOrWhiteSpaceLines = 0;
			var allTestCases = new List<string>();

			foreach (var file in Directory.GetFiles(resourceDirectory, AssemblyResourceProvider.FileSearchPattern))
			{
				using var stream = new FileStream(file, FileMode.Open);
				{
					if (stream == null)
					{
						throw new InvalidOperationException("Provided Content files did not exist.");
					}

					using var reader = new StreamReader(stream);
					string? line;
					while ((line = reader.ReadLine()) != null)
					{
						var trimmedLine = line.Trim();
						if (string.IsNullOrWhiteSpace(trimmedLine))
						{
							emptyOrWhiteSpaceLines++;
							continue;
						}

						if (trimmedLine.StartsWith('#'))
						{
							comments++;
							continue;
						}

						if (allTestCases.Contains(trimmedLine))
						{
							caseSensitiveDuplicateTestCases++;
							continue;
						}

						testCases++;
						allTestCases.Add(trimmedLine);
					}
				}
			}

			var totalLines = testCases + caseSensitiveDuplicateTestCases + comments + emptyOrWhiteSpaceLines;
			return (totalLines, testCases, caseSensitiveDuplicateTestCases, comments, emptyOrWhiteSpaceLines);
		}

		string CreateExplicitFile(string content = "")
		{
			var filePath = Path.Combine(resourceDirectoryPath, $"{AssemblyResourceProvider.FileName}_{Guid.NewGuid()}.txt");
			File.Create(filePath).Close();

			if (!string.IsNullOrWhiteSpace(content))
			{
				File.WriteAllText(filePath, content);
			}

			return filePath;
		}
		#endregion

		#region LoadExplicitTestsFromContentFiles
		public void TestLoadExplicitTestsFromContentFiles_ResourceNameIsEmpty_ThrowArgumentException()
		{
			_ = AssertExceptionThrown<ArgumentException>(() => new AssemblyResourceProvider().LoadExplicitTestsFromContentFiles(""));
		}

		public void TestLoadExplicitTestsFromContentFiles_ResourceNameIsWhiteSpace_ThrowArgumentException()
		{
			_ = AssertExceptionThrown<ArgumentException>(() => new AssemblyResourceProvider().LoadExplicitTestsFromContentFiles("   "));
		}

		public void TestLoadExplicitTestsFromContentFiles_NoDirectoryFound_ThrowDirectoryNotFoundException()
		{
			const string resourceDirectory = "MyFakeResources";
			var ex = AssertExceptionThrown<DirectoryNotFoundException>(() => new AssemblyResourceProvider().LoadExplicitTestsFromContentFiles(resourceDirectory));

			AssertContains($"Directory not found: {Path.Combine(AppContext.BaseDirectory, resourceDirectory)}", ex.Message);
		}

		public void TestLoadExplicitTestsFromContentFiles_FileFoundButEmpty_ReturnEmptyStringHashSet()
		{
			var filePath = CreateExplicitFile();
			var count = TestResourceListsLineCount(ResourceDirectory, AssemblyResourceProvider.ShouldSkipLine);
			File.Delete(filePath);

			AssertEquals("Explicit test list should be empty.", 0, count);
		}

		public void TestLoadExplicitTestsFromContentFiles_FileFoundWithTwoValidEntries_ReturnStringHashSetWithTwoEntries()
		{
			const string content =
				@"CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestActivateOnlyAccessibleToSupport
CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestAnalysisServerInformationShowsAdminError";

			var filePath = CreateExplicitFile(content);
			var (totalLines, testCases, duplicateTestCases, comments, emptyOrWhiteSpaceLines) = TestResourceListsDetails(ResourceDirectory);

			var count = TestResourceListsLineCount(ResourceDirectory, AssemblyResourceProvider.ShouldSkipLine);
			File.Delete(filePath);

			CombineAssertions("Content files must include specific content", () =>
			{
				AssertEquals("Content file line count, including lines we want to trim.", 2, totalLines);
				AssertEquals("Content file actual comment count.", 0, comments);
				AssertEquals("Content file actual test lines count.", 2, testCases);
				AssertEquals("Content file actual empty lines count.", 0, emptyOrWhiteSpaceLines);
			});

			AssertEquals("Explicit test list should have exactly two elements.", 2, count);
		}

		public void TestLoadExplicitTestsFromContentFiles_ContainsCommentsAndWhiteSpaceAndOneValidEntry_ReturnOneValidEntrySkipOtherLines()
		{
			const string content = @"CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestActivateOnlyAccessibleToSupport

# This is a comment
 # Another comment, but with a gap at the start.
";

			var filePath = CreateExplicitFile(content);
			var (totalLines, testCases, duplicateTestCases, comments, emptyOrWhiteSpaceLines) = TestResourceListsDetails(ResourceDirectory);

			var count = TestResourceListsLineCount(ResourceDirectory, AssemblyResourceProvider.ShouldSkipLine);
			File.Delete(filePath);

			CombineAssertions("Content files must include specific content", () =>
			{
				AssertEquals("Content file line count, including lines we want to trim.", 4, totalLines);
				AssertEquals("Content file actual comment count.", 2, comments);
				AssertEquals("Content file actual test lines count.", 1, testCases);
				AssertEquals("Content file actual empty lines count.", 1, emptyOrWhiteSpaceLines);
			});

			AssertEquals("Resultant Explicit test list should have exactly one element, ignoring lines which are empty/whitespace or comments.", 1, count);
		}

		public void TestLoadExplicitTestsFromContentFiles_ContainsFourValidAndOneInvalidEntriesWithTheSameNameCaseInsensitive_ReturnFourValidEntriesSkipOtherLines()
		{
			const string content = @"CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestActivateOnlyAccessibleToSupport

# This is a comment
CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestActivateOnlyAccessibleToSupport
CargoWise.Bi.Product.Manager.gui.Testing.AnalysisServerControlTest.TestActivateOnlyAccessibleToSupport
CargoWise.Bi.Product.Manager.GUI.Testing.analysisservercontroltest.TestActivateOnlyAccessibleToSupport
CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.testactivateonlyaccessibletosupport
 # Another comment, but with a gap at the start.
";

			var filePath = CreateExplicitFile(content);
			var (totalLines, testCases, duplicateTestCases, comments, emptyOrWhiteSpaceLines) = TestResourceListsDetails(ResourceDirectory);

			var count = TestResourceListsLineCount(ResourceDirectory, AssemblyResourceProvider.ShouldSkipLine);
			File.Delete(filePath);

			CombineAssertions("Content files must include specific content", () =>
			{
				AssertEquals("Content file line count, including lines we want to trim.", 8, totalLines);
				AssertEquals("Content file actual comment count.", 2, comments);
				AssertEquals("Content file actual test lines count.", 4, testCases);
				AssertEquals("Content file duplicate test lines count.", 1, duplicateTestCases);
				AssertEquals("Content file actual empty lines count.", 1, emptyOrWhiteSpaceLines);
			});

			AssertEquals("Resultant Explicit test list should have exactly four elements, ignoring lines which are empty/whitespace or comments.", 4, count);
		}

		public void TestLoadExplicitTestsFromContentFiles_ContainsMultipleLists_ReturnThreeEntries()
		{
			const string content1 = "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestActivateOnlyAccessibleToSupport";
			const string content2 = "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestAnalysisServerInformationShowsAdminError";
			const string content3 = "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestAnalysisClientInformationShowsAdminError";

			var filePath1 = CreateExplicitFile(content1);
			var filePath2 = CreateExplicitFile(content2);
			var filePath3 = CreateExplicitFile(content3);
			var (totalLines, testCases, duplicateTestCases, comments, emptyOrWhiteSpaceLines) = TestResourceListsDetails(ResourceDirectory);

			var count = TestResourceListsLineCount(ResourceDirectory, AssemblyResourceProvider.ShouldSkipLine);
			File.Delete(filePath1);
			File.Delete(filePath2);
			File.Delete(filePath3);

			CombineAssertions("Content files must include specific content", () =>
			{
				AssertEquals("Content file line count, including lines we want to trim.", 3, totalLines);
				AssertEquals("Content file actual comment count.", 0, comments);
				AssertEquals("Content file actual test lines count.", 3, testCases);
				AssertEquals("Content file actual empty lines count.", 0, emptyOrWhiteSpaceLines);
			});

			AssertEquals("Resultant Explicit test list should have exactly three elements, combining three different lists.", 3, count);
		}

		public void TestLoadExplicitTestsFromContentFiles_IgnoresOtherNonExplicitListFiles_ReturnOneEntry()
		{
			const string content1 = "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestActivateOnlyAccessibleToSupport";
			const string content2 = "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestAnalysisServerInformationShowsAdminError";

			var filePath1 = CreateExplicitFile(content1);
			var filePath2 = Path.Combine(resourceDirectoryPath, $"{Guid.NewGuid()}.txt");
			File.Create(filePath2).Close();
			File.WriteAllText(filePath2, content2);

			var (totalLines, testCases, duplicateTestCases, comments, emptyOrWhiteSpaceLines) = TestResourceListsDetails(ResourceDirectory);

			var count = TestResourceListsLineCount(ResourceDirectory, AssemblyResourceProvider.ShouldSkipLine);
			File.Delete(filePath1);
			File.Delete(filePath2);

			CombineAssertions("Content files must include specific content", () =>
			{
				AssertEquals("Content file line count, including lines we want to trim.", 1, totalLines);
				AssertEquals("Content file actual comment count.", 0, comments);
				AssertEquals("Content file actual test lines count.", 1, testCases);
				AssertEquals("Content file actual empty lines count.", 0, emptyOrWhiteSpaceLines);
			});

			AssertEquals("Resultant Explicit test list should have exactly one element, ignorning non-explicit lists.", 1, count);
		}
		#endregion

		#region ShouldSkipLine
		public void TestShouldSkipLine_EmptyAndWhiteSpace_Skipped()
		{
			CombineAssertions("Both empty and whitespace should be skipped.", () =>
			{
				AssertEquals("Empty lines should be skipped.", expected: true, AssemblyResourceProvider.ShouldSkipLine(""));
				AssertEquals("WhiteSpace lines should be skipped.", expected: true, AssemblyResourceProvider.ShouldSkipLine("  "));
			});
		}

		public void TestShouldSkipLine_NonEmptyLine_NotSkipped()
		{
			AssertEquals("Non-empty lines should not be skipped.", expected: false, AssemblyResourceProvider.ShouldSkipLine("NameSpace.Test.Line"));
		}

		public void TestShouldSkipLine_Comment_Skipped()
		{
			CombineAssertions("Lines which start as a comment should be skipped.", () =>
			{
				AssertEquals("Comment starts exactly at position '0' should be skipped.", expected: true, AssemblyResourceProvider.ShouldSkipLine("#"));
				AssertEquals("Comment starts exactly at position '0' after trim, and should be skipped.", expected: true, AssemblyResourceProvider.ShouldSkipLine(" #"));
			});
		}

		public void TestShouldSkipLine_CommentAtLineEnd_NotSkipped()
		{
			AssertEquals("Non-empty lines should not be skipped.", expected: false, AssemblyResourceProvider.ShouldSkipLine("NameSpace.Test.Line # Followed by a comment"));
		}
		#endregion

		#region RemoveCommentFromLine
		public void TestRemoveCommentFromLine_EmptyOrWhiteSpaceLine_ReturnsEmptyString()
		{
			var emptyTestLine = AssemblyResourceProvider.RemoveCommentFromLine("");
			var whiteSpaceTestLine = AssemblyResourceProvider.RemoveCommentFromLine("  ");

			AssertEquals("Returned test line should be an empty string.", string.Empty, emptyTestLine);
			AssertEquals("Returned test line should be an empty string.", string.Empty, whiteSpaceTestLine);
		}

		public void TestRemoveCommentFromLine_JustComment_ReturnsEmptyString()
		{
			var testLine = AssemblyResourceProvider.RemoveCommentFromLine("# This is a comment made on a line.");

			AssertEquals("Returned test line should be an empty string.", string.Empty, testLine);
		}

		public void TestRemoveCommentFromLine_LineWithNoComment_ReturnsSameLine()
		{
			var line = "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestAnalysisServerInformationShowsAdminError";
			var testLine = AssemblyResourceProvider.RemoveCommentFromLine(line);

			AssertEquals("Returned test line should be the same as the input line.", line, testLine);
		}

		public void TestRemoveCommentFromLine_LineWithComment_ReturnsLineWithoutComment()
		{
			var line = "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestAnalysisServerInformationShowsAdminError # This comment is in-line and should be trimmed.";
			var testLine = AssemblyResourceProvider.RemoveCommentFromLine(line);

			AssertEquals("Comment should have been removed from returned test line.", "CargoWise.Bi.Product.Manager.GUI.Testing.AnalysisServerControlTest.TestAnalysisServerInformationShowsAdminError", testLine);
		}
		#endregion
	}
}
