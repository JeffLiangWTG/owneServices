using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	[TestedType(typeof(CSSExporter))]
	public class CSSExporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExport()
		{
			TestHelper.SetValidRegistryForCSSExporterTest();
			IList<BusinessObject> bizoList = GetPopulatedCollectionToSaveAndExport();
			Assert("Precondition - must be at least one element returned from GetPopulatedCollectionToExport()", bizoList.Count > 0);
			Factory.Save();
			Assert("Check if the folder is empty for initial state", Directory.GetFiles(TestHelper.GetExportDirectory).Length == 0);
			Exporter.Export(bizoList, new NotificationBuffer());
			Exporter.Export(bizoList, new NotificationBuffer());
			foreach (var bizo in bizoList)
			{
				int numberOfLogs = 0;
				foreach (StmALog log in bizo.GetLogs().GetAllLogs())
				{
					if (log.SL_SE_NKEvent == "DEX")
					{
						numberOfLogs++;
					}
				}

				AssertEquals("DEX Event Logs", 2, numberOfLogs);
			}

			AssertEquals("Check how many files are exported", 2, Directory.GetFiles(TestHelper.GetExportDirectory).Length);
			Assert("File should have written something. Check that you return the FlatFileRows you are populating in FlatFileConverter.MapExport()", Directory.GetFiles(testHelper.GetExportDirectory)[0].Length > 0);
			Assert("File should have written something. Check that you return the FlatFileRows you are populating in FlatFileConverter.MapExport()", Directory.GetFiles(testHelper.GetExportDirectory)[1].Length > 0);
		}

		protected override void SetUp()
		{
			base.SetUp();
			if (!Directory.Exists(TestHelper.GetExportDirectory))
			{
				Directory.CreateDirectory(TestHelper.GetExportDirectory);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (Directory.Exists(TestHelper.GetExportDirectory))
			{
				Directory.Delete(TestHelper.GetExportDirectory, true);
			}
		}

		IList<BusinessObject> GetPopulatedCollectionToSaveAndExport()
		{
			return TestHelper.GetPopulatedCollection();
		}

		TGETestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new TGETestHelper(Factory));
			}
		}

		TGETestHelper testHelper;
		CSSExporter Exporter
		{
			get
			{
				return exporter ?? (exporter = new CSSExporter(Factory));
			}
		}

		CSSExporter exporter;
	}
}
