using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OutturnLineCollectionExportHelperTest : TestCaseWithFactory
	{
		public void TestExportToScanner()
		{
			using (var tempFile = TempFile.New())
			{
				var collection = SetupTestCollection();
				var helper = new OutturnLineCollectionExportHelper(collection);
				helper.ExportToScanner(tempFile.Filename);

				string content = "";
				using (var reader = new StreamReader(tempFile.Filename))
				{
					content = reader.ReadToEnd();
				}
				AssertEquals(@"
CargoWise ScanCheckManifest,v1.0,Total Consignments:,3
ConsignmentRef,Status,ScannedDateTime,Count
Rederence1,Held,,1
Rederence2,Clear,,2
Rederence3,Held,,3
".Trim(), content.Trim());
			}
		}

		public void TestExportToScannerFileMapper()
		{
			var fileMapper = new OutturnLineCollectionExportHelper.ExportToScannerFileMapper("header 1");
			using (var tempFile = TempFile.New())
			{
				using (var stream = fileMapper.OpenWrite(tempFile.Filename))
				{
				}

				string content = "";
				using (var reader = new StreamReader(tempFile.Filename))
				{
					content = reader.ReadToEnd();
				}

				AssertEquals("header 1\r\n", content);
			}
		}

		OutturnLineCollection SetupTestCollection()
		{
			var cusHAWB = Factory.NewWithValidTestData<CusHAWB>();
			var cusUnderbond = Factory.NewWithValidTestData<CusUnderbond>();

			var line1 = new AirOutturnLine()
			{
				ManifestInfo = new ManifestInformationForTest() { Quantity = 1 },
				ConsignmentRef = "Rederence1",
				Status = "Held",
				Count = 1
			};

			var line2 = new AirOutturnLine()
			{
				ManifestInfo = new ManifestInformationForTest() { Quantity = 2 },
				ConsignmentRef = "Rederence2",
				Status = "Clear",
				Count = 2
			};

			var line3 = new AirOutturnLine()
			{
				ManifestInfo = new ManifestInformationForTest() { Quantity = 3 },
				ConsignmentRef = "Rederence3",
				Status = "Held",
				Count = 3
			};

			var collection = new AirOutturnLineCollection(Factory);
			collection.Add(line1);
			collection.Add(line2);
			collection.Add(line3);

			return collection;
		}

		public void TestExportToScannerError()
		{
			using (var tempFile = TempFile.New())
			{
				var collection = SetupTestCollection();
				var helper = new TestOutturnLineCollectionExportHelper(collection);
				AssertEquals("Some error", helper.ExportToScanner(tempFile.Filename));
			}
		}

		class TestOutturnLineCollectionExportHelper : OutturnLineCollectionExportHelper
		{
			readonly OutturnLineCollection collection;

			public TestOutturnLineCollectionExportHelper(OutturnLineCollection collection)
				: base(collection)
			{
				this.collection = collection;
			}

			protected override ExportWizard GetExportWizard()
			{
				var impl = new ImportCollectionInfoImpl(collection);
				AddCollectionProperties(impl);
				var fileHeader = string.Format("CargoWise ScanCheckManifest,v1.0,Total Consignments:,{0}", collection.Count); // File header
				var fileMapper = new ExportToScannerFileMapper(fileHeader);

				return new ExportWizardTest(impl, null, fileMapper);
			}
		}

		class ExportWizardTest : ExportWizard
		{
			public ExportWizardTest(IExportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
				: base(collectionInfo, settingsStorage, fileMapper)
			{
			}

			protected override bool ExportCollectionCore(IEnumerable<BusinessObject> collection, out string errorMessage)
			{
				errorMessage = "Some error";
				return false;
			}
		}
	}
}
