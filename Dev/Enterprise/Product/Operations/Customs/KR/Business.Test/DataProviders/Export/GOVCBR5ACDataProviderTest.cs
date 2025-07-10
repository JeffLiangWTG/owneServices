using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ACDataProvidersTest : XMLMessageTestHelper<GOVCBR5ACDataProvidersTest>
	{
		[TestDate(2021, 03, 03)]
		public void TestWithRealData()
		{
			#region IOrganization
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "레디코리아";
			broker.OH_IsBroker = true;

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "김환태";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var brokerAdddress = broker.MainAddress;
			brokerAdddress.OA_CompanyNameOverride = broker.OH_FullName;
			brokerAdddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			brokerAdddress.OA_Phone = "020-548-7636";

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = broker.PK;
			#endregion
			Factory.Save();

			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.CustomsOffice = "010";
			extendedHoursRequestHeaderObj.Department = "10";
			extendedHoursRequestHeaderObj.Reason = "개청";
			extendedHoursRequestHeaderObj.StartDate = new ZDateTime(2021, 03, 03, 18, 00, 00);
			extendedHoursRequestHeaderObj.EndDate = new ZDateTime(2021, 03, 03, 23, 00, 00);

			var extendedHoursRequestLineObj1 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj1.ReferenceNumber = "2362520050702X";
			extendedHoursRequestLineObj1.CustomsValue = 1150;
			extendedHoursRequestLineObj1.PackageCount = 1;
			extendedHoursRequestLineObj1.TotalWeight = 1650;
			extendedHoursRequestLineObj1.SupplierName = "금산유통";
			extendedHoursRequestLineObj1.UQ = "KG";

			var extendedHoursRequestLineObj2 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj2.ReferenceNumber = "2362520042782X";
			extendedHoursRequestLineObj2.CustomsValue = 0;
			extendedHoursRequestLineObj2.PackageCount = 0;
			extendedHoursRequestLineObj2.SupplierName = "(주)라이트워크";
			extendedHoursRequestLineObj2.UQ = "KG";

			#region Test Serialisation With Xml 
			var export5AC = new Export5ACCreator().Create(extendedHoursRequestHeaderObj);
			var result = new GOVCBR5ACMessageBuilder(export5AC).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5ACDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5AC_Result_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
			#endregion

			#region Test Set Check with Non-BO hedaer
			AssertEquals(EDIMessage.EntryNumberPlaceHolder, export5AC.ApplicationNumber);
			AssertEquals(new ZDateTime(2021, 03, 03, 18, 00, 00), export5AC.StartDateTime);
			AssertEquals(new ZDateTime(2021, 03, 03, 23, 00, 00), export5AC.EndDateTime);
			AssertEquals("010", export5AC.DeclarationCustomsOffice);
			AssertEquals("10", export5AC.DeclarationCustomsDivision);
			AssertEquals("개청", export5AC.ApplicationReason);
			#endregion

			#region Test Set Check with Non-BO Lines
			AssertEquals(2, export5AC.Entries.Length);

			var line1 = export5AC.Entries[0];
			AssertEquals("2362520050702X", line1.ReferenceNumber);
			AssertEquals(1150m, line1.TotalCustomsValueInUSD);
			AssertEquals(1m, line1.TotalPackQty);
			AssertEquals(1650m, line1.TotalGrossWeightInKG);
			AssertEquals("금산유통", line1.SupplierName);

			var line2 = export5AC.Entries[1];
			AssertEquals("2362520042782X", line2.ReferenceNumber);
			AssertEquals(0m, line2.TotalCustomsValueInUSD);
			AssertEquals(0m, line2.TotalPackQty);
			AssertEquals("(주)라이트워크", line2.SupplierName);
			#endregion

			var export5ACDeclarant = export5AC.Declarant;
			AssertNotNull(export5ACDeclarant);
			AssertEquals("레디코리아", export5ACDeclarant.CompanyName);
			AssertEquals("김환태", export5ACDeclarant.RepresentativeName);
			AssertEquals("020-548-7636", export5ACDeclarant.PhoneNumber);
		}
		[TestDate(2014, 05, 06)]
		public void TestWithFullData()
		{
			#region IOrganization
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "상호";
			broker.OH_IsBroker = true;

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "성명";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var brokerAdddress = broker.MainAddress;
			brokerAdddress.OA_CompanyNameOverride = broker.OH_FullName;
			brokerAdddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			brokerAdddress.OA_Phone = "020-0000-0000";

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = broker.PK;
			#endregion
			Factory.Save();

			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.CustomsOffice = "010";
			extendedHoursRequestHeaderObj.Department = "20";
			extendedHoursRequestHeaderObj.Reason = "임시개청사유";
			extendedHoursRequestHeaderObj.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 00);
			extendedHoursRequestHeaderObj.EndDate = new ZDateTime(2014, 05, 09, 12, 30, 00);

			var extendedHoursRequestLineObj = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj.ReferenceNumber = "0000000000";
			extendedHoursRequestLineObj.CustomsValue = 1000;
			extendedHoursRequestLineObj.PackageCount = 99;
			extendedHoursRequestLineObj.TotalWeight = 500;
			extendedHoursRequestLineObj.SupplierName = "수출화주";
			extendedHoursRequestLineObj.UQ = "KG";

			#region Test Serialisation With Xml 
			var export5AC = new Export5ACCreator().Create(extendedHoursRequestHeaderObj);
			var result = new GOVCBR5ACMessageBuilder(export5AC).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5ACDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5AC_Result_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
			#endregion

			#region Test Set Check with Non-BO hedaer
			AssertEquals(EDIMessage.EntryNumberPlaceHolder, export5AC.ApplicationNumber);
			AssertEquals(new ZDateTime(2014, 05, 06, 12, 30, 00), export5AC.StartDateTime);
			AssertEquals(new ZDateTime(2014, 05, 09, 12, 30, 00), export5AC.EndDateTime);
			AssertEquals("010", export5AC.DeclarationCustomsOffice);
			AssertEquals("20", export5AC.DeclarationCustomsDivision);
			AssertEquals("임시개청사유", export5AC.ApplicationReason);
			#endregion

			#region Test Set Check with Non-BO Lines
			AssertEquals(1, export5AC.Entries.Length);

			var line = export5AC.Entries[0];
			AssertEquals("0000000000", line.ReferenceNumber);
			AssertEquals(1000m, line.TotalCustomsValueInUSD);
			AssertEquals(99m, line.TotalPackQty);
			AssertEquals(500m, line.TotalGrossWeightInKG);
			AssertEquals("수출화주", line.SupplierName);
			#endregion

			var export5ACDeclarant = export5AC.Declarant;
			AssertNotNull(export5ACDeclarant);
			AssertEquals("상호", export5ACDeclarant.CompanyName);
			AssertEquals("성명", export5ACDeclarant.RepresentativeName);
			AssertEquals("020-0000-0000", export5ACDeclarant.PhoneNumber);
		}

		public void TestWithNoEntries()
		{
			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.CustomsOffice = "010";
			extendedHoursRequestHeaderObj.Department = "20";
			extendedHoursRequestHeaderObj.Reason = "임시개청사유";
			extendedHoursRequestHeaderObj.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 00);
			extendedHoursRequestHeaderObj.EndDate = new ZDateTime(2014, 05, 09, 12, 30, 00);

			var export5AC = new Export5ACCreator().Create(extendedHoursRequestHeaderObj);

			AssertEquals(EDIMessage.EntryNumberPlaceHolder, export5AC.ApplicationNumber);
			AssertEquals(new ZDateTime(2014, 05, 06, 12, 30, 00), export5AC.StartDateTime);
			AssertEquals(new ZDateTime(2014, 05, 09, 12, 30, 00), export5AC.EndDateTime);
			AssertEquals("010", export5AC.DeclarationCustomsOffice);
			AssertEquals("20", export5AC.DeclarationCustomsDivision);
			AssertEquals("임시개청사유", export5AC.ApplicationReason);

			AssertEquals(0, extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.Count);
			AssertEquals(0, export5AC.Entries.Length);
		}

		public void TestEmptyDateFormat()
		{
			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.StartDate = ZDateTime.Empty;
			extendedHoursRequestHeaderObj.EndDate = ZDateTime.Empty;

			var export5AC = new Export5ACCreator().Create(extendedHoursRequestHeaderObj);
			AssertEquals(EDIMessage.EntryNumberPlaceHolder, export5AC.ApplicationNumber);
			AssertEquals(ZDateTime.Empty, export5AC.StartDateTime);
			AssertEquals(ZDateTime.Empty, export5AC.EndDateTime);

			extendedHoursRequestHeaderObj.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 00);
			extendedHoursRequestHeaderObj.EndDate = new ZDateTime(2014, 05, 09, 12, 30, 00);

			export5AC = new Export5ACCreator().Create(extendedHoursRequestHeaderObj);
			AssertEquals(new ZDateTime(2014, 05, 06, 12, 30, 00), export5AC.StartDateTime);
			AssertEquals(new ZDateTime(2014, 05, 09, 12, 30, 00), export5AC.EndDateTime);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing";
	}
}
