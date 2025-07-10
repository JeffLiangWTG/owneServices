using System;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FileExtensionFilterBuilderTest : TestCase
	{
		public void TestGetFileExtension()
		{
			FileExtensionFilterBuilderForTesting filterBuilder = new FileExtensionFilterBuilderForTesting(false);
			AssertEquals("Passing in none file extension", "", filterBuilder.GetFileExtension(FileExtensionType.None));

			AssertEquals("Passing in text file", "txt", filterBuilder.GetFileExtension(FileExtensionType.Txt));

			FileExtensionFilterBuilderForTesting.ClientSpecificFileExtension.Value = "ABC";
			AssertEquals("Passing in text even if client specific set returns txt (lowercase)", "txt", filterBuilder.GetFileExtension(FileExtensionType.Txt));
			AssertEquals("Passing in client specific will return client specific (lowercase)", "abc", filterBuilder.GetFileExtension(FileExtensionType.ClientSpecific));

			filterBuilder = new FileExtensionFilterBuilderForTesting(true);
			AssertEquals("Passing in text even if client specific set returns txt (uppercase)", "TXT", filterBuilder.GetFileExtension(FileExtensionType.Txt));
			AssertEquals("Passing in client specific will return client specific (uppercase)", "ABC", filterBuilder.GetFileExtension(FileExtensionType.ClientSpecific));
		}

		public void TestGetFileExtensionClientSpecificWithoutSettingValueReportsDeveloperError()
		{
			ErrorReporter.Clear();
			Assert("No exception in the ErrorReporter", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			try
			{
				FileExtensionFilterBuilderForTesting filterBuilder = new FileExtensionFilterBuilderForTesting();
				filterBuilder.GetFileExtension(FileExtensionType.ClientSpecific);
				AssertEquals("Exception should have been reported because ClientSpecific extension type passed in, but the string is not set", "FileExtensionFilterBuilder.GetFileExtension", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestFilterClauseWithClientSpecific()
		{
			FileExtensionFilterBuilderForTesting.ClientSpecificFileExtension.Value = "ABC";
			FileExtensionFilterBuilderForTesting filterBuilder = new FileExtensionFilterBuilderForTesting();
			AssertEquals("Filter clause empty - no extensions added yet", "", filterBuilder.FilterClause);

			filterBuilder.Add(FileExtensionType.ClientSpecific);
			AssertEquals("Filter builder with client specific file extension", "ABC Files (*.ABC)|*.ABC", filterBuilder.FilterClause);
		}

		public void TestFilterClauseWithClientSpecificAndDescription()
		{
			FileExtensionFilterBuilderForTesting.ClientSpecificFileExtension.Value = "ABC";
			FileExtensionFilterBuilderForTesting.ClientSpecificFileExtensionDescription.Value = "Files of Type ABC (*.ABC)";
			FileExtensionFilterBuilderForTesting filterBuilder = new FileExtensionFilterBuilderForTesting();
			AssertEquals("Filter clause empty - no extensions added yet", "", filterBuilder.FilterClause);

			filterBuilder.Add(FileExtensionType.ClientSpecific);
			AssertEquals("Filter builder with client specific extension AND description", "Files of Type ABC (*.ABC)|*.ABC", filterBuilder.FilterClause);
		}

		public void TestAddFileExtension()
		{
			FileExtensionFilterBuilderForTesting testFilterBuilder = new FileExtensionFilterBuilderForTesting();
			AssertEquals("No file extensions in the list", 0, testFilterBuilder.PublicFileExtensionsList.Count);

			testFilterBuilder.Add(FileExtensionType.Csv);
			AssertEquals("1 file extension in the list", 1, testFilterBuilder.PublicFileExtensionsList.Count);
			AssertEquals("File extension is csv", FileExtensionType.Csv, testFilterBuilder.PublicFileExtensionsList[0]);

			testFilterBuilder.Add(FileExtensionType.Txt | FileExtensionType.ClientSpecific);
			AssertEquals("3 file extensions in the list", 3, testFilterBuilder.PublicFileExtensionsList.Count);
			AssertEquals("1st file extension is csv", true, testFilterBuilder.PublicFileExtensionsList.Contains(FileExtensionType.Csv));
			AssertEquals("2nd file extension is txt", true, testFilterBuilder.PublicFileExtensionsList.Contains(FileExtensionType.Txt));
			AssertEquals("3nd file extension is ClientSpecific", true, testFilterBuilder.PublicFileExtensionsList.Contains(FileExtensionType.ClientSpecific));

			testFilterBuilder.Add(FileExtensionType.None);
			AssertEquals("Still 3 file extensions in the list - None does not get added", 3, testFilterBuilder.PublicFileExtensionsList.Count);
		}

		public void TestRemoveFileExtension()
		{
			FileExtensionFilterBuilderForTesting testFilterBuilder = new FileExtensionFilterBuilderForTesting();
			testFilterBuilder.Add(FileExtensionType.Csv);
			testFilterBuilder.Add(FileExtensionType.Txt);
			AssertEquals("2 file extensions in the list", 2, testFilterBuilder.PublicFileExtensionsList.Count);
			AssertEquals("1st file extension is csv", FileExtensionType.Csv, testFilterBuilder.PublicFileExtensionsList[0]);
			AssertEquals("2nd file extension is txt", FileExtensionType.Txt, testFilterBuilder.PublicFileExtensionsList[1]);

			testFilterBuilder.Remove(FileExtensionType.Csv);
			AssertEquals("1 file extension in the list", 1, testFilterBuilder.PublicFileExtensionsList.Count);
			AssertEquals("File extension is txt", FileExtensionType.Txt, testFilterBuilder.PublicFileExtensionsList[0]);

			testFilterBuilder.Remove(FileExtensionType.Csv);
			AssertEquals("still only 1 file extension in the list", 1, testFilterBuilder.PublicFileExtensionsList.Count);
			AssertEquals("File extension is txt", FileExtensionType.Txt, testFilterBuilder.PublicFileExtensionsList[0]);

			testFilterBuilder.Remove(FileExtensionType.Txt);
			AssertEquals("no file extensions in the list", 0, testFilterBuilder.PublicFileExtensionsList.Count);
		}

		public void TestFilterClause()
		{
			FileExtensionFilterBuilderForTesting testFilterBuilder = new FileExtensionFilterBuilderForTesting();
			testFilterBuilder.Add(FileExtensionType.None);
			AssertEquals("Filter clause", "", testFilterBuilder.FilterClause);

			testFilterBuilder.Add(FileExtensionType.Txt);
			AssertEquals("Filter clause", "Text Files (*.txt)|*.txt", testFilterBuilder.FilterClause);

			testFilterBuilder.Add(FileExtensionType.Csv);
			AssertEquals("Filter clause with 2 values", "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv", testFilterBuilder.FilterClause);

			testFilterBuilder.Remove(FileExtensionType.Txt);
			AssertEquals("Filter clause with one removed", "CSV Files (*.csv)|*.csv", testFilterBuilder.FilterClause);
		}

		public void TestFilterClauseWorksForAllExtensionTypesExceptNone()
		{
			Array fileExtensionTypes = Enum.GetValues(typeof(FileExtensionType));

			foreach (FileExtensionType extension in fileExtensionTypes)
			{
				FileExtensionFilterBuilder builder = new FileExtensionFilterBuilder();
				builder.Add(extension);
				if (extension != FileExtensionType.None)
				{
					AssertNotNullOrEmpty("Filter clause shouldn't return empty when FileExtensionType '" + extension + "' is added. Provide a filter clause for this extension type in the FileExtensionFilterBuilder.", builder.FilterClause);
				}
				else
				{
					AssertEquals("Filter clause shouldn return empty when FileExtensionType 'None' is added.", "", builder.FilterClause);
				}
			}
		}
	}
}
