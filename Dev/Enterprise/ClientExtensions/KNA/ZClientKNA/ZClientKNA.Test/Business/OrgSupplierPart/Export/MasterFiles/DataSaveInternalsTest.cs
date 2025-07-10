using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DataSaveInternalsTest : TestCaseWithFactory
	{
		public void TestDataSaving()
		{
			using (TempFile testFileName = TempFile.New())
			{
				CombineAssertions(() =>
				{
					DataSaver.TestMode = TestTypes.NullBizoCollection;
					DataSaver.ExportData(testFileName.Filename, "Test Data Record");
					AssertEquals("Export failed.", "No Test Data Records to export.", DataSaver.Log[0]);
					DataSaver.TestMode = TestTypes.EmptyBizoCollection;
					DataSaver.ExportData(testFileName.Filename, "Test Data Record");
					AssertEquals("Export failed.", "No Test Data Records to export.", DataSaver.Log[0]);
					DataSaver.TestMode = TestTypes.ValidBizoCollection;
					DataSaver.ExportData(testFileName.Filename, "Test Data Record");
					AssertEquals("Export failed.", true, DataSaver.Log[0].Contains("Test Data Records to export = "));
					AssertEquals("BizosExpectedToExportPlusHeader", DataSaver.Orgs.Count + 1, DataSaver.RunCounters.BizosExpectedToExportPlusHeader);
					AssertEquals("LinesCreated", 0, DataSaver.RunCounters.LinesCreated);
					AssertEquals("BizosExcluded", 0, DataSaver.RunCounters.BizosExcluded);
					AssertEquals("CurrentRow", DataSaver.Orgs.Count + 1, DataSaver.RunCounters.CurrentRow);
					FileInfo exportedFile = new FileInfo(testFileName.Filename);
					AssertEquals("File created", true, exportedFile.Exists);
					AssertEquals("File populated", true, exportedFile.Length > 0);
				});
			}
		}

		public void TestOutputFinalTotals()
		{
			DataSaver.RunCounters.LinesCreated = 2;
			DataSaver.RunCounters.BizosExcluded = 7;
			DataSaver.Log.Clear();
			DataSaver.OutputFinalTotals("TestDataType");
			AssertEquals(1, DataSaver.Log.Count);
			AssertEquals(System.Environment.NewLine + "T O T A L : TestDataTypes created = 2, TestDataTypes excluded = 7" + System.Environment.NewLine, DataSaver.Log[0]);
		}

		DataSaveForTesting DataSaver
		{
			get
			{
				return dataSaver ?? (dataSaver = new DataSaveForTesting());
			}
		}

		DataSaveForTesting dataSaver;
		public class DataSaveForTesting : DataSave
		{
			protected override BusinessObjectCollection NewBusinessObjectCollection()
			{
				BusinessObjectCollection result = null;
				switch (TestMode)
				{
					case TestTypes.EmptyBizoCollection:
						result = new OrgHeaderCollection(Factory);
						break;
					case TestTypes.ValidBizoCollection:
						Orgs = new OrgHeaderCollection(Factory, new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "J"));
						Orgs.Load();
						result = Orgs;
						break;
				}

				return result;
			}

			protected override OCsvLine ProcessDataForThisBizo(BusinessObject bizo)
			{
				return new OCsvLine("DetailLine");
			}

			protected override String[] ColumnHeadings()
			{
				return new String[] { "HeadingLine" };
			}

			internal TestTypes TestMode;
			internal OrgHeaderCollection Orgs;
		}

		internal enum TestTypes
		{
			Unknown,
			NullBizoCollection,
			EmptyBizoCollection,
			ValidBizoCollection
		}
	}
}
