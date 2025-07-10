using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoFlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestImportFromCSVToXsd()
		{
			string airCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AirCargoCSVExample.txt"));
			ZQuery filter = new ZQuery(CusMAWBSchema.CM_MAWB, "08143510541");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));

			GetNewImporter().ImportData(airCargoExampleFileName, null, SourceInfo.EmptySourceInfo);
			var masterBill = Factory.LoadTop1<CusMAWB>(filter);

			AssertMasterBillDetailsAreCorrect(masterBill);
			AssertHouseBillDetailsAreCorrect(masterBill);
		}

		public void TestImportFromCSVToXsdWithSwappedConsignorPhoneCountry()
		{
			string airCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AirCargoCSVSampleWithSwappedConsignorPhoneCountry.txt"));
			ZQuery filter = new ZQuery(CusMAWBSchema.CM_MAWB, "08143510541");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));

			GetNewImporter().ImportData(airCargoExampleFileName, null, SourceInfo.EmptySourceInfo);
			var masterBill = Factory.LoadTop1<CusMAWB>(filter);

			AssertMasterBillDetailsAreCorrect(masterBill);
			AssertHouseBillDetailsAreCorrect(masterBill);
		}

		public void TestInvalidQualifierNotification()
		{
			string airCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AirCargoCSVSampleWithCorruptQualifiers.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusMAWBSchema.CM_MAWB, "08143510541");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));

			GetNewImporter().ImportData(airCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);
			string expMessage = "Error: Invalid file format (Unknown qualifier detected. Expected AIRHWB but found AIRXXX at line 2. Expected AIRGDS but found AIRYYY at line 3.)";

			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
			AssertEquals("Expected a file format error", 1, fileFormatErrors.Length);
			AssertEquals("Notification should include format error details", expMessage, fileFormatErrors[0].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));
		}

		public void TestInvalidMasterbillQualifierNotification()
		{
			string airCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AirCargoCSVSampleWithCorruptMBLQualifier.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusMAWBSchema.CM_MAWB, "08143510541");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));

			GetNewImporter().ImportData(airCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);
			string expMessage = "Error: Invalid file format (Corrupt or invalid Air Cargo CSV file. Expected AIRMWB but found AIRAAA at line 1.)";

			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
			AssertEquals("Expected a file format error", 1, fileFormatErrors.Length);
			AssertEquals("Notification should include format error details", expMessage, fileFormatErrors[0].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));
		}

		public void TestInsufficientFieldsNotification()
		{
			string airCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AirCargoCSVSampleInsufficientFields.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusMAWBSchema.CM_MAWB, "08143510541");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));

			GetNewImporter().ImportData(airCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);
			string expHBLCountMessage = "Error: Invalid file format (Expected at least 42 fields in line 4 but found only 13.)";
			string expGDSCountMessage = "Error: Invalid file format (Expected at least 20 fields in line 5 but found only 7.)";

			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
			AssertEquals("Expected a file format error", 2, fileFormatErrors.Length);
			AssertEquals("Notification should include format error details", expHBLCountMessage, fileFormatErrors[0].Message);
			AssertEquals("Notification should include format error details", expGDSCountMessage, fileFormatErrors[1].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));
		}

		[ExpectNoExceptions]
		public void TestEmptyFile()
		{
			string airCargoExampleFileName = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("AirCargoCSVEmpty.txt"));
			NotificationBuffer notifications = new NotificationBuffer();
			ZQuery filter = new ZQuery(CusMAWBSchema.CM_MAWB, "08143510541");
			AssertEquals(0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));

			GetNewImporter().ImportData(airCargoExampleFileName, notifications, SourceInfo.EmptySourceInfo);
			string expMessage = "Error: Invalid file format (Corrupt or invalid Air Cargo CSV file. Expected AIRMWB but found no data at line 1.)";

			INotification[] fileFormatErrors = notifications.GetEventsByType(ErrorType.InvalidFileFormat);
			AssertEquals("Expected a file format error", 1, fileFormatErrors.Length);
			AssertEquals("Notification should include format error details", expMessage, fileFormatErrors[0].Message);
			AssertEquals("No data should have been imported", 0, Factory.GetDatabaseCount(typeof(CusMAWB), filter));
		}

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;

		AirCargoFlatFileDataImporter GetNewImporter() => new AirCargoFlatFileDataImporter();

		void AssertMasterBillDetailsAreCorrect(CusMAWB masterBill)
		{
			AssertNotNull("No MasterBill is imported", masterBill);
			AssertEquals("Two HouseBills are expected", 2, masterBill.ChildBills.Count);

			AssertEquals("MasterHouseBill", "83878", masterBill.CM_MasterHouseBill);
			AssertEquals("FlightNo", "QF026", masterBill.CM_FlightNo);
			AssertEquals("Port of Loading", "ITSPE", masterBill.CM_RL_NKLoadPort);
			AssertEquals("Port of Destination", "AUMEL", masterBill.CM_RL_NKDischargePort);
			AssertEquals("Port of First Arrival", "AUMEL", masterBill.CM_RL_NKFirstArrivalPort);
			AssertEquals("Date of Arrival", new ZDateTime(2005, 06, 14), masterBill.CM_ArrivalDate);
		}

		void AssertHouseBillDetailsAreCorrect(CusMAWB masterBill)
		{
			CusHAWB houseBill1, houseBill2;

			//Do not care about order of the housebills in HouseBills collection. 
			//The one that has HWB1 is the first one.
			AssertEquals(false, masterBill.ChildBills[0].CS_HAWB.IsEmpty);
			if (masterBill.ChildBills[0].CS_HAWB == "HWB1")
			{
				houseBill1 = masterBill.ChildBills[0];
				houseBill2 = masterBill.ChildBills[1];
			}
			else
			{
				houseBill1 = masterBill.ChildBills[1];
				houseBill2 = masterBill.ChildBills[0];
			}

			AssertHouseBill1DetailsAreCorrect(houseBill1);
			AssertHouseBill2DetailsAreCorrect(houseBill2);
		}

		void AssertHouseBill1DetailsAreCorrect(CusHAWB houseBill1)
		{
			AssertEquals("HouseBill", "HWB1", houseBill1.CS_HAWB);
			AssertEquals("MasterHouseBill", "MAST1234", houseBill1.CS_MasterHouseBill);
			AssertEquals("IsMasterHouse", ZBool.False, houseBill1.CS_IsMasterHouse);

			AssertEquals("PortOfOrigin", "ITSPE", houseBill1.CS_RL_NKOrigin);
			AssertEquals("PortOfDestination", "AUMEL", houseBill1.CS_RL_NKDestination);
			AssertEquals("PrepaidCollect", "PC", houseBill1.CS_FreightPrepaidCollect);

			AssertEquals("Consignor Name", "ConsignorName1", houseBill1.CS_ConsignorName);
			AssertEquals("Consignor Address1", "ConsignorAddress11", houseBill1.CS_ConsignorStreet);
			AssertEquals("Consignor Address2", "ConsignorAddress12", houseBill1.CS_ConsignorStreet2);
			AssertEquals("Consignor Suburb", "Suburb1", houseBill1.CS_ConsignorCity);
			AssertEquals("Consignor Postcode", "PCode1", houseBill1.CS_ConsignorPostcode);
			AssertEquals("Consignor Country", "AU", houseBill1.CS_RN_NKConsignorCountry);

			AssertEquals("Consignee Name", "ConsigneeName1", houseBill1.CS_ConsigneeName);
			AssertEquals("Consignee Address1", "ConsigneeAddress11", houseBill1.CS_ConsigneeStreet);
			AssertEquals("Consignee Address1", "ConsigneeAddress12", houseBill1.CS_ConsigneeStreet2);
			AssertEquals("Consignee Suburb", "Suburb3", houseBill1.CS_ConsigneeCity);
			AssertEquals("Consignee Postcode", "PCode3", houseBill1.CS_ConsigneePostcode);
			AssertEquals("Consignee Country", "AU", houseBill1.CS_RN_NKConsigneeCountry);

			AssertEquals("Goods Description", "Goods Description 1", houseBill1.CS_GoodsDescription);
			AssertEquals("PackageCount", (ZShort)282, houseBill1.CS_PiecesManifested);
			AssertEquals("Weight", 2978.000m, houseBill1.CS_Weight);
			AssertEquals("Weight UQ", "KG", houseBill1.CS_WeightUQ);

			AssertEquals("Goods Value", 823.54m, houseBill1.CS_GoodsValue);
			AssertEquals("Goods Currency", "AU", houseBill1.CS_RX_NKGoodsCurrency);

			AssertEquals("IsPersonalEffects", ZBool.True, houseBill1.CS_IsPersonalEffects);
			AssertEquals("IsSelfAssessedClearance", ZBool.True, houseBill1.CS_IsSelfAssessedClearance);
		}

		void AssertHouseBill2DetailsAreCorrect(CusHAWB houseBill2)
		{
			AssertEquals("HouseBill", "HWB2", houseBill2.CS_HAWB);
			AssertEquals("MasterHouseBill", "MAST1234", houseBill2.CS_MasterHouseBill);
			AssertEquals("IsMasterHouse", ZBool.False, houseBill2.CS_IsMasterHouse);

			AssertEquals("PortOfOrigin", "ITMIL", houseBill2.CS_RL_NKOrigin);
			AssertEquals("PortOfDestination", "AUSYD", houseBill2.CS_RL_NKDestination);
			AssertEquals("PrepaidCollect", "PC", houseBill2.CS_FreightPrepaidCollect);

			AssertEquals("Consignor Name", "ConsignorName2", houseBill2.CS_ConsignorName);
			AssertEquals("Consignor Address1", "ConsignorAddress21", houseBill2.CS_ConsignorStreet);
			AssertEquals("Consignor Address2", "ConsignorAddress22", houseBill2.CS_ConsignorStreet2);
			AssertEquals("Consignor Suburb", "Suburb2", houseBill2.CS_ConsignorCity);
			AssertEquals("Consignor Postcode", "PCode2", houseBill2.CS_ConsignorPostcode);
			AssertEquals("Consignor Country", "AU", houseBill2.CS_RN_NKConsignorCountry);

			AssertEquals("Consignee Name", "ConsigneeName2", houseBill2.CS_ConsigneeName);
			AssertEquals("Consignee Address1", "ConsigneeAddress21", houseBill2.CS_ConsigneeStreet);
			AssertEquals("Consignee Address1", "ConsigneeAddress22", houseBill2.CS_ConsigneeStreet2);
			AssertEquals("Consignee Suburb", "Suburb4", houseBill2.CS_ConsigneeCity);
			AssertEquals("Consignee Postcode", "PCode4", houseBill2.CS_ConsigneePostcode);
			AssertEquals("Consignee Country", "AU", houseBill2.CS_RN_NKConsigneeCountry);

			AssertEquals("Goods Description", "Goods Description 2", houseBill2.CS_GoodsDescription);
			AssertEquals("PackageCount", (ZShort)282, houseBill2.CS_PiecesManifested);
			AssertEquals("Weight", 3333.000m, houseBill2.CS_Weight);
			AssertEquals("Weight UQ", "lb", houseBill2.CS_WeightUQ);

			AssertEquals("Goods Value", 30.15m, houseBill2.CS_GoodsValue);
			AssertEquals("Goods Currency", "AU", houseBill2.CS_RX_NKGoodsCurrency);

			AssertEquals("IsPersonalEffects", ZBool.False, houseBill2.CS_IsPersonalEffects);
			AssertEquals("IsSelfAssessedClearance", ZBool.False, houseBill2.CS_IsSelfAssessedClearance);
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.Testing." + fileName;
	}
}
