using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5GWDataProvidersTest : XMLMessageTestHelper<GOVCBR5GWDataProvidersTest>
	{
		[TestDate(2019, 10, 04)]
		public void TestWithRealData()
		{
			#region Non BO
			var messageSendingObjectHeader = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			messageSendingObjectHeader.CustomsOffice = "030";
			messageSendingObjectHeader.Department = "83";
			messageSendingObjectHeader.Reason = "긴급화물로 인함.";
			messageSendingObjectHeader.StartDate = new ZDateTime(2019, 10, 06, 15, 00, 00);
			messageSendingObjectHeader.EndDate = new ZDateTime(2019, 10, 06, 16, 00, 00);

			var messageSendingObjectline1 = messageSendingObjectHeader.ExtendedHoursRequestLines.AddNew();
			messageSendingObjectline1.ReferenceNumber = "4163419502388M";
			messageSendingObjectline1.ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
			messageSendingObjectline1.HSDescription = "USED CYLINDER";
			messageSendingObjectline1.CustomsValue = 158;
			messageSendingObjectline1.PackageCount = 9;
			messageSendingObjectline1.TotalWeight = 957;
			messageSendingObjectline1.UQ = "KG";
			messageSendingObjectline1.BondedAreaCode = "03077026";
			messageSendingObjectline1.PayerCompanyName = "한국소화화학품(주)";

			var messageSendingObjectline2 = messageSendingObjectHeader.ExtendedHoursRequestLines.AddNew();
			messageSendingObjectline2.ReferenceNumber = "4163419502375M";
			messageSendingObjectline2.ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
			messageSendingObjectline2.HSDescription = "PHOSPHINE";
			messageSendingObjectline2.CustomsValue = 3249;
			messageSendingObjectline2.PackageCount = 64;
			messageSendingObjectline2.TotalWeight = 3989;
			messageSendingObjectline2.UQ = "KG";
			messageSendingObjectline2.BondedAreaCode = "03077026";
			messageSendingObjectline2.PayerCompanyName = "한국소화화학품(주)";
			#endregion

			#region broker
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "관세법인에이원부산지사";
			broker.OH_IsBroker = true;

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "이성욱엄형수";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var brokerAddress = broker.MainAddress;
			brokerAddress.OA_OH = broker.PK;
			brokerAddress.OA_CompanyNameOverride = broker.OH_FullName;
			brokerAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			brokerAddress.OA_Phone = "051-460-3000";

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = broker.PK;
			#endregion

			Factory.Save();

			var import5GW = new Import5GWCreator().Create(messageSendingObjectHeader);
			var result = new GOVCBR5GWMessageBuilder(import5GW).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5GWDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5GW_Result_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			AssertEquals(EDIMessage.EntryNumberPlaceHolder, import5GW.ApplicationNumber);
			AssertEquals(new ZDateTime(2019, 10, 06, 15, 00, 00), import5GW.StartDateTime);
			AssertEquals(new ZDateTime(2019, 10, 06, 16, 00, 00), import5GW.EndDateTime);
			AssertEquals("030", import5GW.DeclarationCustomsOffice);
			AssertEquals("83", import5GW.DeclarationCustomsDivision);
			AssertEquals("긴급화물로 인함.", import5GW.ApplicationReason);

			AssertNotNull(import5GW.Entries);
			AssertEquals(2, import5GW.Entries.Length);

			var import5GWEntries = import5GW.Entries;
			AssertEquals(ReferenceNumberTypeList.Codes.IMP, import5GWEntries[0].ReferenceNumberType);
			AssertEquals("4163419502388M", import5GWEntries[0].ReferenceNumber);
			AssertEquals("USED CYLINDER", import5GWEntries[0].HSDescription);
			AssertEquals(158m, import5GWEntries[0].TotalCustomsValueInUSD);
			AssertEquals(9m, import5GWEntries[0].TotalPackQty);
			AssertEquals(957m, import5GWEntries[0].TotalGrossWeightInKG);
			AssertEquals("03077026", import5GWEntries[0].BondedAreaCode);
			AssertEquals("한국소화화학품(주)", import5GWEntries[0].PayerCompanyName);

			AssertEquals(ReferenceNumberTypeList.Codes.IMP, import5GWEntries[1].ReferenceNumberType);
			AssertEquals("4163419502375M", import5GWEntries[1].ReferenceNumber);
			AssertEquals("PHOSPHINE", import5GWEntries[1].HSDescription);
			AssertEquals(3249m, import5GWEntries[1].TotalCustomsValueInUSD);
			AssertEquals(64m, import5GWEntries[1].TotalPackQty);
			AssertEquals(3989m, import5GWEntries[1].TotalGrossWeightInKG);
			AssertEquals("03077026", import5GWEntries[1].BondedAreaCode);
			AssertEquals("한국소화화학품(주)", import5GWEntries[1].PayerCompanyName);

			var import5GWDeclarant = import5GW.Declarant;
			AssertNotNull(import5GWDeclarant);
			AssertEquals("관세법인에이원부산지사", import5GWDeclarant.CompanyName);
			AssertEquals("이성욱엄형수", import5GWDeclarant.RepresentativeName);
			AssertEquals("051-460-3000", import5GWDeclarant.PhoneNumber);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		[TestDate(2014, 05, 06)]
		public void TestWithFullData()
		{
			#region Non BO
			var messageSendingObjectHeader = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			messageSendingObjectHeader.CustomsOffice = "010";
			messageSendingObjectHeader.Department = "20";
			messageSendingObjectHeader.Reason = "임시개청/취소사유";
			messageSendingObjectHeader.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 12);
			messageSendingObjectHeader.EndDate = new ZDateTime(2014, 06, 06, 12, 30, 12);

			var messageSendingObjectline = messageSendingObjectHeader.ExtendedHoursRequestLines.AddNew();
			messageSendingObjectline.HSDescription = "품명";
			messageSendingObjectline.CustomsValue = 21;
			messageSendingObjectline.PackageCount = 99;
			messageSendingObjectline.TotalWeight = 99;
			messageSendingObjectline.UQ = "KG";
			messageSendingObjectline.BondedAreaCode = "02010578";
			messageSendingObjectline.PayerCompanyName = "상호";
			messageSendingObjectline.ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
			messageSendingObjectline.ReferenceNumber = "000000000000000";
			#endregion

			#region Declarant
			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_Category = "BUS";
			broker.OH_FullName = "상호";
			broker.OH_IsBroker = true;

			var contact = broker.Contacts.AddNew();
			contact.OC_ContactName = "성명";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			var brokertAddress = broker.MainAddress;
			brokertAddress.OA_OH = broker.PK;
			brokertAddress.OA_CompanyNameOverride = broker.OH_FullName;
			brokertAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			brokertAddress.OA_Phone = "010-0000-0000";

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = broker.PK;
			#endregion
			Factory.Save();

			var import5GW = new Import5GWCreator().Create(messageSendingObjectHeader);
			var result = new GOVCBR5GWMessageBuilder(import5GW).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5GWDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5GW_Result_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}

			AssertEquals(EDIMessage.EntryNumberPlaceHolder, import5GW.ApplicationNumber);
			AssertEquals(new ZDateTime(2014, 05, 06, 12, 30, 12), import5GW.StartDateTime);
			AssertEquals(new ZDateTime(2014, 06, 06, 12, 30, 12), import5GW.EndDateTime);
			AssertEquals("010", import5GW.DeclarationCustomsOffice);
			AssertEquals("20", import5GW.DeclarationCustomsDivision);
			AssertEquals("임시개청/취소사유", import5GW.ApplicationReason);

			AssertNotNull(import5GW.Entries);
			AssertEquals(1, import5GW.Entries.Length);

			var import5GWEntries = import5GW.Entries;
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, import5GWEntries[0].ReferenceNumberType);
			AssertEquals("000000000000000", import5GWEntries[0].ReferenceNumber);
			AssertEquals("품명", import5GWEntries[0].HSDescription);
			AssertEquals(21m, import5GWEntries[0].TotalCustomsValueInUSD);
			AssertEquals(99m, import5GWEntries[0].TotalPackQty);
			AssertEquals(99m, import5GWEntries[0].TotalGrossWeightInKG);
			AssertEquals("02010578", import5GWEntries[0].BondedAreaCode);
			AssertEquals("상호", import5GWEntries[0].PayerCompanyName);

			var import5GWDeclarant = import5GW.Declarant;
			AssertNotNull(import5GWDeclarant);
			AssertEquals("상호", import5GWDeclarant.CompanyName);
			AssertEquals("성명", import5GWDeclarant.RepresentativeName);
			AssertEquals("010-0000-0000", import5GWDeclarant.PhoneNumber);
		}

		public void TestWithNoEntries()
		{
			var messageSendingObjectHeader = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			messageSendingObjectHeader.CustomsOffice = "010";
			messageSendingObjectHeader.Department = "20";
			messageSendingObjectHeader.Reason = "임시개청/취소사유";
			messageSendingObjectHeader.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 12);
			messageSendingObjectHeader.EndDate = new ZDateTime(2014, 06, 06, 12, 30, 12);

			var import5GW = new Import5GWCreator().Create(messageSendingObjectHeader);

			AssertEquals(EDIMessage.EntryNumberPlaceHolder, import5GW.ApplicationNumber);
			AssertEquals(new ZDateTime(2014, 05, 06, 12, 30, 12), import5GW.StartDateTime);
			AssertEquals(new ZDateTime(2014, 06, 06, 12, 30, 12), import5GW.EndDateTime);
			AssertEquals("010", import5GW.DeclarationCustomsOffice);
			AssertEquals("20", import5GW.DeclarationCustomsDivision);
			AssertEquals("임시개청/취소사유", import5GW.ApplicationReason);

			AssertEquals(0, messageSendingObjectHeader.ExtendedHoursRequestLines.Count);
			AssertEquals(0, import5GW.Entries.Length);
		}

		public void TestWithTotalWeight()
		{
			var header = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			header.CustomsOffice = "010";
			header.Department = "20";
			header.Reason = "임시개청/취소사유";
			header.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 12);
			header.EndDate = new ZDateTime(2014, 06, 06, 12, 30, 12);

			var line1 = header.ExtendedHoursRequestLines.AddNew();
			line1.TotalWeight = 1;
			line1.UQ = Core.Constants.Weight.Kilograms;
			var line2 = header.ExtendedHoursRequestLines.AddNew();
			line2.TotalWeight = 2;
			line2.UQ = Core.Constants.Weight.Kilograms;
			var line3 = header.ExtendedHoursRequestLines.AddNew();
			line3.TotalWeight = 3;
			line3.UQ = Core.Constants.Weight.Decitons;

			var import5GW = new Import5GWCreator().Create(header);
			AssertEquals(3, import5GW.Entries.Length);

			AssertEquals(1m, import5GW.Entries[0].TotalGrossWeightInKG);
			AssertEquals(2m, import5GW.Entries[1].TotalGrossWeightInKG);
			AssertEquals(300m, import5GW.Entries[2].TotalGrossWeightInKG);
		}
	}
}
