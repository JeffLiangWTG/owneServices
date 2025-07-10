using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class SeaCargoFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestImportFromCSVToXsd()
		{
			string seaCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SeaCargoCSVExample.txt"));
			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "MSCUSL746555");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));

			GetNewImporter().ImportData(seaCargoExampleFileName, null, SourceInfo.EmptySourceInfo);
			var oceanBill = Factory.LoadTop1<CusSCAOceanBill>(filter);

			AssertOceanBillDetailsAreCorrect(oceanBill);
			AssertHouseBillDetailsAreCorrect(oceanBill);
			AssertContainerDetailsAreCorrect(oceanBill);
		}

		public void TestImportEmptyCSV()
		{
			using (var tempFile = TempFile.New())
			{
				NotificationBuffer notifications = new NotificationBuffer();

				ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "MSCUSL746555");
				GetNewImporter().ImportData(tempFile.Filename, notifications, SourceInfo.EmptySourceInfo);

				string expMessage1 = "Error: Invalid file format (Corrupt or invalid Sea Cargo CSV file. Expected SEAOBL but found no data at line 1.)";

				INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
				AssertEquals("Expected a file format error", 1, fileFormatErrors.Length);
				AssertEquals("Empty File", expMessage1, fileFormatErrors[0].Message);
				AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));
			}
		}

		public void TestImportFromCSVToXsdWithSwappedConsignorPhoneCountry()
		{
			string seaCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SeaCargoCSVExampleWithSwappedPhoneCountry.txt"));
			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "MSCUSL746555");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));

			GetNewImporter().ImportData(seaCargoExampleFileName, null, SourceInfo.EmptySourceInfo);
			var oceanBill = Factory.LoadTop1<CusSCAOceanBill>(filter);

			AssertOceanBillDetailsAreCorrect(oceanBill);
			AssertHouseBillDetailsAreCorrect(oceanBill);
			AssertContainerDetailsAreCorrect(oceanBill);
		}

		public void TestImportFromSeaCargoCSVWithMissingFeilds()
		{
			bool indexOutOfRangeExceptionFired = false;
			try
			{
				string seaCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SeaCargoCSVWithMissingFeilds.txt"));
				GetNewImporter().ImportData(seaCargoExampleFileName, null, SourceInfo.EmptySourceInfo);
			}
			catch (IndexOutOfRangeException)
			{
				indexOutOfRangeExceptionFired = true;
			}
			finally
			{
				Assert(!indexOutOfRangeExceptionFired);
			}
		}

		public void TestInvalidQualifierNotification()
		{
			string seaCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SeaCargoCSVSampleWithCorruptQualifiers.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "MSCUSL746555");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));

			GetNewImporter().ImportData(seaCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);
			string expMessage1 = "Error: Invalid file format (Unknown qualifier detected. Expected SEACNT, SEAHBL or SEAGDS but found SEABBB at line 2.)";
			string expMessage2 = "Error: Invalid file format (Unknown qualifier detected. Expected SEACNT, SEAHBL or SEAGDS but found SEACCC at line 4.)";
			string expMessage3 = "Error: Invalid file format (Unknown qualifier detected. Expected SEACNT, SEAHBL or SEAGDS but found SEADDD at line 6.)";

			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
			AssertEquals("Expected a file format error", 3, fileFormatErrors.Length);
			AssertEquals("Illegal Qualifier on line 2", expMessage1, fileFormatErrors[0].Message);
			AssertEquals("Illegal Qualifier on line 4", expMessage2, fileFormatErrors[1].Message);
			AssertEquals("Illegal Qualifier on line 6", expMessage3, fileFormatErrors[2].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));
		}

		public void TestInvalidMasterbillQualifierNotification()
		{
			string seaCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SeaCargoCSVSampleWithCorruptOBLQualifier.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "MSCUSL746555");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));

			GetNewImporter().ImportData(seaCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);
			string expMessage = "Error: Invalid file format (Corrupt or invalid Sea Cargo CSV file. Expected SEAOBL but found SEAAAA at line 1.)";

			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
			AssertEquals("Expected a file format error", 1, fileFormatErrors.Length);
			AssertEquals("Notification should include format error details", expMessage, fileFormatErrors[0].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));
		}

		public void TestInsufficientFieldsNotification()
		{
			string seaCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SeaCargoCSVSampleInsufficientFields.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "MSCUSL746555");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));

			GetNewImporter().ImportData(seaCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);
			string expHBLCountError = "Error: Invalid file format (Expected at least 8 fields in line 3 but found only 5.)";
			string expCNTCountError = "Error: Invalid file format (Expected at least 37 fields in line 5 but found only 13.)";
			string expGDSCountError = "Error: Invalid file format (Expected at least 20 fields in line 6 but found only 5.)";

			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
			AssertEquals("Expected a file format error", 3, fileFormatErrors.Length);
			AssertEquals("Notification should include format error details", expHBLCountError, fileFormatErrors[0].Message);
			AssertEquals("Notification should include format error details", expCNTCountError, fileFormatErrors[1].Message);
			AssertEquals("Notification should include format error details", expGDSCountError, fileFormatErrors[2].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));
		}

		public void TestNegativePackCount()
		{
			string seaCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SeaCargoCSVSampleNegativePackCount.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "MSCUSL746555");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));

			GetNewImporter().ImportData(seaCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);

			string negativePackCountMsg = "Error: Data type conversion error (The number of packs cannot be negative. Found -331.000 in goods line for House-Bill (439544) / Container(MSCU4604789))";
			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.DataTypeConversionError);
			AssertEquals("Expected a file format error", 1, fileFormatErrors.Length);
			AssertEquals("Notification should include negative pack count error details", negativePackCountMsg, fileFormatErrors[0].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), filter));
		}

		public void TestSupportedFileExtensions()
		{
			var importer = GetNewImporter();
			AssertEquals("All Types Supported (*.*)", FileExtensionType.All, importer.FileExtensionType & FileExtensionType.All);
			AssertEquals("CSV Type Supported (*.CSV)", FileExtensionType.Csv, importer.FileExtensionType & FileExtensionType.Csv);
		}

		public void TestSeaGetValueFromBooleanField()
		{
			var converter = new SeaCargoFlatFileConverter(null, Factory);
			var dataRows = new FlatFileDataRow(4);
			dataRows[0] = "true";
			dataRows[1] = "false";
			dataRows[2] = "";
			dataRows[3] = "test";
			Assert("'true' can be parsed. Return true", converter.GetValueFromBooleanField(dataRows, 0));
			Assert("'false' can be parsed. Return false", !converter.GetValueFromBooleanField(dataRows, 1));
			Assert("empty string can't be parsed. Return false", !converter.GetValueFromBooleanField(dataRows, 2));
			Assert("'test' can't be parsed. Return false", !converter.GetValueFromBooleanField(dataRows, 3));
		}

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}

		protected virtual SeaCargoFlatFileDataImporter GetNewImporter() => new SeaCargoFlatFileDataImporter();

		void AssertOceanBillDetailsAreCorrect(CusSCAOceanBill oceanBill)
		{
			AssertNotNull("No OceanBill is imported", oceanBill);
			AssertEquals("Two HouseBills are expected", 2, oceanBill.HouseBills.Count);
			AssertEquals("Two Containers are expected", 2, oceanBill.Containers.Count);

			AssertEquals("First HouseBill should have one packing", 1, oceanBill.HouseBills[0].Pivot.Count);
			AssertEquals("Second HouseBill should have one packing", 1, oceanBill.HouseBills[1].Pivot.Count);
			AssertEquals("First Container should have one packing", 1, oceanBill.Containers[0].Pivots.Count);
			AssertEquals("Second Container should have one packing", 1, oceanBill.Containers[1].Pivots.Count);

			AssertOceanBillDataImport(oceanBill);
		}

		void AssertOceanBillDataImport(CusSCAOceanBill oceanBill)
		{
			AssertEquals("CB_OceanBill", "MSCUSL746555", oceanBill.CB_OceanBill);
			AssertEquals("CB_MasterHouseBill", "83878", oceanBill.CB_MasterHouseBill);
			AssertEquals("CB_MultiOBLUnpack", ZBool.False, oceanBill.CB_MultiOBLUnpack);
			AssertEquals("CB_VesselName", "MSC MAYA", oceanBill.CB_VesselName);
			AssertEquals("CB_LloydsIMO", "8714190", oceanBill.CB_LloydsIMO);
			AssertEquals("CB_Voyage", "79", oceanBill.CB_Voyage);
			AssertEquals("CB_RL_NKPortOfLoading", "ITSPE", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("CB_RL_NKPortOfDischarge", "AUMEL", oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("CB_RL_NKPortOfFirstArrival", "AUMEL", oceanBill.CB_RL_NKPortOfFirstArrival);
			AssertEquals("CB_DateOfFirstArrival", new ZDateTime(2005, 06, 14), oceanBill.CB_DateOfArrival);
			AssertEquals("CB_PrincipalID", "010114093", oceanBill.CB_PrincipalID);
		}

		void AssertHouseBillDetailsAreCorrect(CusSCAOceanBill oceanBill)
		{
			CusSCAHouse houseBill1, houseBill2;

			//Do not care about order of the housebills in HouseBills collection. 
			//The one that has HWB1 is the first one.
			AssertEquals(false, oceanBill.HouseBills[0].CA_HouseBill.IsEmpty);
			if (oceanBill.HouseBills[0].CA_HouseBill == "439543")
			{
				houseBill1 = oceanBill.HouseBills[0];
				houseBill2 = oceanBill.HouseBills[1];
			}
			else
			{
				houseBill1 = oceanBill.HouseBills[1];
				houseBill2 = oceanBill.HouseBills[0];
			}

			AssertHouseBill1DetailsAreCorrect(houseBill1);
			AssertHouseBill2DetailsAreCorrect(houseBill2);
		}

		void AssertHouseBill1DetailsAreCorrect(CusSCAHouse houseBill1)
		{
			AssertEquals("HouseBill", "439543", houseBill1.CA_HouseBill);
			AssertEquals("MasterHouseBill", "MSCUSL746555", houseBill1.CA_MasterHouseBill);

			AssertEquals("Port of Loading", "ITSPE", houseBill1.CA_RL_NK_PortOfOrigin);
			AssertEquals("Port of Destination", "AUMEL", houseBill1.CA_RL_NK_PortOfDestination);
			AssertEquals("Origin of Goods", "IT", houseBill1.CA_RN_NKGoodsOrigin);
			AssertEquals("PrepaidCollectOther", "PC", houseBill1.CA_PrepaidCollectOther);

			AssertEquals("Consignor Name", "ConsignorName1", houseBill1.CA_ConsignorName);
			AssertEquals("Consignor Address1", "ConsignorAddress11", houseBill1.CA_ConsignorAddress1);
			AssertEquals("Consignor Address2", "ConsignorAddress12", houseBill1.CA_ConsignorAddress2);
			AssertEquals("Consignor Suburb", "Suburb1", houseBill1.CA_ConsignorSuburb);
			AssertEquals("Consignor Postcode", "PCode1", houseBill1.CA_ConsignorPostcode);
			AssertEquals("Consignor Country", "AU", houseBill1.CA_RN_NKConsignorCountryCode);

			AssertEquals("Consignee Name", "ConsigneeName1", houseBill1.CA_ConsigneeName);
			AssertEquals("Consignee Address1", "ConsigneeAddress11", houseBill1.CA_ConsigneeAddress1);
			AssertEquals("Consignee Address1", "ConsigneeAddress12", houseBill1.CA_ConsigneeAddress2);
			AssertEquals("Consignee Suburb", "Suburb3", houseBill1.CA_ConsigneeSuburb);
			AssertEquals("Consignee Postcode", "PCode3", houseBill1.CA_ConsigneePostcode);
			AssertEquals("Consignee Country", "AU", houseBill1.CA_RN_NKConsigneeCountryCode);
			AssertEquals("Consignee Phone", "123456", houseBill1.CA_ConsigneePhone);
			AssertEquals("Consignee Fax", "1234567", houseBill1.CA_ConsigneeFax);

			AssertEquals("Notify Name", "NotifyName1", houseBill1.CA_NotifyName);
			AssertEquals("Notify Address1", "NotifyAddress11", houseBill1.CA_NotifyAddress1);
			AssertEquals("Notify Address2", "NotifyAddress12", houseBill1.CA_NotifyAddress2);
			AssertEquals("Notify Suburb", "Suburb5", houseBill1.CA_NotifySuburb);
			AssertEquals("Notify PostCode", "PCode5", houseBill1.CA_NotifyPostcode);
			AssertEquals("Notify Country", "NZ", houseBill1.CA_RN_NKNotifyCountryCode);
			AssertEquals("Notify Phone", "654321", houseBill1.CA_NotifyPhone);
			AssertEquals("Notify Fax", "7654321", houseBill1.CA_NotifyFax);

			AssertHouseBill1PackingDetailsAreCorrect(houseBill1.Pivot[0]);
		}

		void AssertHouseBill2DetailsAreCorrect(CusSCAHouse houseBill2)
		{
			AssertEquals("439544", houseBill2.CA_HouseBill);
			AssertEquals("MasterHouseBill", "MSCUSL746555", houseBill2.CA_MasterHouseBill);

			AssertEquals("Port of Loading", "ITMIL", houseBill2.CA_RL_NK_PortOfOrigin);
			AssertEquals("Port of Destination", "AUSYD", houseBill2.CA_RL_NK_PortOfDestination);
			AssertEquals("Origin of Goods", "FR", houseBill2.CA_RN_NKGoodsOrigin);
			AssertEquals("PrepaidCollectOther", "CC", houseBill2.CA_PrepaidCollectOther);

			AssertEquals("Consignor Name", "ConsignorName2", houseBill2.CA_ConsignorName);
			AssertEquals("Consignor Address1", "ConsignorAddress21", houseBill2.CA_ConsignorAddress1);
			AssertEquals("Consignor Address2", "ConsignorAddress22", houseBill2.CA_ConsignorAddress2);
			AssertEquals("Consignor Suburb", "Suburb2", houseBill2.CA_ConsignorSuburb);
			AssertEquals("Consignor Postcode", "PCode2", houseBill2.CA_ConsignorPostcode);
			AssertEquals("Consignor Country", "AU", houseBill2.CA_RN_NKConsignorCountryCode);

			AssertEquals("Consignee Name", "ConsigneeName2", houseBill2.CA_ConsigneeName);
			AssertEquals("Consignee Address1", "ConsigneeAddress21", houseBill2.CA_ConsigneeAddress1);
			AssertEquals("Consignee Address1", "ConsigneeAddress22", houseBill2.CA_ConsigneeAddress2);
			AssertEquals("Consignee Suburb", "Suburb4", houseBill2.CA_ConsigneeSuburb);
			AssertEquals("Consignee Postcode", "PCode4", houseBill2.CA_ConsigneePostcode);
			AssertEquals("Consignee Country", "AU", houseBill2.CA_RN_NKConsigneeCountryCode);
			AssertEquals("Consignee Phone", "234567", houseBill2.CA_ConsigneePhone);
			AssertEquals("Consignee Fax", "2345678", houseBill2.CA_ConsigneeFax);

			AssertEquals("Notify Name", "NotifyName2", houseBill2.CA_NotifyName);
			AssertEquals("Notify Address1", "NotifyAddress21", houseBill2.CA_NotifyAddress1);
			AssertEquals("Notify Address2", "NotifyAddress22", houseBill2.CA_NotifyAddress2);
			AssertEquals("Notify Suburb", "Suburb6", houseBill2.CA_NotifySuburb);
			AssertEquals("Notify PostCode", "PCode6", houseBill2.CA_NotifyPostcode);
			AssertEquals("Notify Country", "AU", houseBill2.CA_RN_NKNotifyCountryCode);
			AssertEquals("Notify Phone", "765432", houseBill2.CA_NotifyPhone);
			AssertEquals("Notify Fax", "8765432", houseBill2.CA_NotifyFax);

			AssertHouseBill2PackingDetailsAreCorrect(houseBill2.Pivot[0]);
		}

		void AssertContainerDetailsAreCorrect(CusSCAOceanBill oceanBill)
		{
			CusSCAContainer container1, container2;
			if (oceanBill.Containers[0].CN_ContainerNumber == "MSCU4604788")
			{
				container1 = oceanBill.Containers[0];
				container2 = oceanBill.Containers[1];
			}
			else
			{
				container1 = oceanBill.Containers[1];
				container2 = oceanBill.Containers[0];
			}

			AssertContainer1DetailsAreCorrect(container1);
			AssertContainer2DetailsAreCorrect(container2);
		}

		void AssertContainer1DetailsAreCorrect(CusSCAContainer container1)
		{
			AssertEquals("ContainerNumber", "MSCU4604788", container1.CN_ContainerNumber);
			AssertEquals("Seal", "1362966", container1.CN_SealNumber);
			AssertEquals("Container Mode", "LCL", container1.CN_ContainerMode);
			AssertEquals("TypeofContainer", "GENN", container1.CN_TypeOfContainer);
			AssertEquals("ContainerSizeOrISOCode", "4008", container1.CN_ContainerSizeOrISOCode);
			AssertEquals("ShipperOwnedContainer", false, container1.CN_ShipperOwnedContainer);
		}

		void AssertContainer2DetailsAreCorrect(CusSCAContainer container2)
		{
			AssertEquals("ContainerNumber", "MSCU4604789", container2.CN_ContainerNumber);
			AssertEquals("Seal", "1362967", container2.CN_SealNumber);
			AssertEquals("Container Mode", Core.Constants.ContainerModes.FCL, container2.CN_ContainerMode);
			AssertEquals("TypeofContainer", "GENN", container2.CN_TypeOfContainer);
			AssertEquals("ContainerSizeOrISOCode", "4008", container2.CN_ContainerSizeOrISOCode);
			AssertEquals("ShipperOwnedContainer", true, container2.CN_ShipperOwnedContainer);
		}

		void AssertHouseBill1PackingDetailsAreCorrect(CusSCAPivot packing1)
		{
			AssertEquals("Goods Description", "Goods Description 1", packing1.CV_GoodsDescription);
			AssertEquals("Number Of Packs", 282, packing1.CV_PackageCount);
			AssertEquals("Package Type", "PK", packing1.CV_PackageType);
			AssertEquals("Volume", 28.600m, packing1.CV_Volume);
			AssertEquals("Weight", 2978.000m, packing1.CV_Weight);
			AssertEquals("Weight UQ", "KG", packing1.CV_WeightUQ);
			AssertEquals("Marks & Numbers", "MRS NADIA COPPE, 35 LYNNBURN ROAD  BATESFORD VIC 3221", packing1.CV_MarksAndNumbers);

			AssertEquals("FumigationCert", ZBool.False, packing1.CV_FumigationCert);
			AssertEquals("PerishableGoods", ZBool.True, packing1.CV_PerishableGoods);
			AssertEquals("PersonalEffects", ZBool.True, packing1.CV_PersonalEffects);
			AssertEquals("HazardousGoods", ZBool.False, packing1.CV_HazardousGoods);
			AssertEquals("Timber", ZBool.True, packing1.CV_Timber);
			AssertEquals("IsDocuments", ZBool.False, packing1.CV_IsDocuments);
			AssertEquals("IsSAC", ZBool.True, packing1.CV_IsSAC);
		}

		void AssertHouseBill2PackingDetailsAreCorrect(CusSCAPivot packing2)
		{
			AssertEquals("Goods Description", "Goods Description 2", packing2.CV_GoodsDescription);
			AssertEquals("Number Of Packs", 331, packing2.CV_PackageCount);
			AssertEquals("Package Type", "PK", packing2.CV_PackageType);
			AssertEquals("Volume", 50.000m, packing2.CV_Volume);
			AssertEquals("Weight", 3333.000m, packing2.CV_Weight);
			AssertEquals("Weight UQ", "lb", packing2.CV_WeightUQ);
			AssertEquals("Marks & Numbers", "MRS NADIA COPPE, 35 LYNNBURN ROAD  BATESFORD VIC 3221", packing2.CV_MarksAndNumbers);

			AssertEquals("FumigationCert", ZBool.True, packing2.CV_FumigationCert);
			AssertEquals("PerishableGoods", ZBool.False, packing2.CV_PerishableGoods);
			AssertEquals("PersonalEffects", ZBool.False, packing2.CV_PersonalEffects);
			AssertEquals("HazardousGoods", ZBool.True, packing2.CV_HazardousGoods);
			AssertEquals("Timber", ZBool.False, packing2.CV_Timber);
			AssertEquals("IsDocuments", ZBool.True, packing2.CV_IsDocuments);
			AssertEquals("IsSAC", ZBool.False, packing2.CV_IsSAC);
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.Testing." + fileName;
	}
}
