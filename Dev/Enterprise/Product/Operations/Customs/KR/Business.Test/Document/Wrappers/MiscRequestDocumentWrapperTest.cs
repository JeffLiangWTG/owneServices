using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class MiscRequestDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestCustomsOfficeNameAndDepartmentName()
		{
			var miscRequestHeader = Factory.New<CusMiscRequestHeader>();
			miscRequestHeader.CMR_CustomsOffice = "01";
			var wrapper = new MiscRequestDocumentWrapper(miscRequestHeader, Factory);
			AssertEquals("", wrapper.CustomsOfficeName);
			AssertEquals("", wrapper.DepartmentName);

			miscRequestHeader.CMR_CustomsOffice = "010";
			wrapper = new MiscRequestDocumentWrapper(miscRequestHeader, Factory);
			AssertEquals("서울세관", wrapper.CustomsOfficeName);
			AssertEquals("", wrapper.DepartmentName);

			miscRequestHeader.CMR_CustomsOffice = "0101";
			wrapper = new MiscRequestDocumentWrapper(miscRequestHeader, Factory);
			AssertEquals("서울세관", wrapper.CustomsOfficeName);
			AssertEquals("", wrapper.DepartmentName);

			miscRequestHeader.CMR_CustomsOffice = "01010";
			wrapper = new MiscRequestDocumentWrapper(miscRequestHeader, Factory);
			AssertEquals("서울세관", wrapper.CustomsOfficeName);
			AssertEquals("내륙기지통관과", wrapper.DepartmentName);
		}

		[TestDate(2021, 08, 20)]
		public void TestMiscRequestHeader()
		{
			var wrapper = GetMiscRequestDocumentWrapperForTest(ElectronicDocumentTypeList.Codes._5AC, KRJobMessageTypeList.Codes.Export);
			var requestHeader = wrapper.MiscRequestHeader;

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, requestHeader.CMR_Status);
			requestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalRejected, wrapper.Status);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalRejected, wrapper.StatusDescription);
			AssertEquals(new ZDateTime(2024, 11, 25), wrapper.RequestDate);
			AssertEquals("01010", requestHeader.CMR_CustomsOffice);
			AssertEquals("서울세관", wrapper.CustomsOfficeName);
			AssertEquals("내륙기지통관과", wrapper.DepartmentName);

			AssertEquals(new ZDateTime(2014, 05, 06, 12, 30, 00), wrapper.MiscRequestHeader.CusEntryNumber.CE_IssueDate);
			AssertEquals(new ZDateTime(2014, 05, 09, 12, 30, 00), wrapper.MiscRequestHeader.CusEntryNumber.CE_ExpiryDate);
		}

		public void TestDeclarant()
		{
			var wrapper = GetMiscRequestDocumentWrapperForTest(ElectronicDocumentTypeList.Codes._5AC, KRJobMessageTypeList.Codes.Export);
			AssertEquals("레디코리아", wrapper.Broker.CompanyName);
			AssertEquals("홍길동", wrapper.Broker.RepresentativeName);
			AssertEquals("02354168589", wrapper.Broker.PhoneNumber);
		}

		public void TestFormattedApplicationNumber()
		{
			var wrapper5GW = GetMiscRequestDocumentWrapperForTest(ElectronicDocumentTypeList.Codes._5GW, KRJobMessageTypeList.Codes.Import, "1");
			AssertEquals("11598-개청-12-1000008U", wrapper5GW.FormattedApplicationNumber);

			var wrapper5AC = GetMiscRequestDocumentWrapperForTest(ElectronicDocumentTypeList.Codes._5AC, KRJobMessageTypeList.Codes.Export, "2");
			AssertEquals("11598-12-1000008U", wrapper5AC.FormattedApplicationNumber);
		}

		public void Test5ACMessageData()
		{
			#region export5AC message
			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.CustomsOffice = "010";
			extendedHoursRequestHeaderObj.Department = "20";
			extendedHoursRequestHeaderObj.Reason = "임시개청사유";
			extendedHoursRequestHeaderObj.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 00);
			extendedHoursRequestHeaderObj.EndDate = new ZDateTime(2014, 05, 09, 12, 30, 00);

			var extendedHoursRequestLineObj1 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj1.ReferenceNumber = "6N00221000025X";
			extendedHoursRequestLineObj1.CustomsValue = 1000m;
			extendedHoursRequestLineObj1.PackageCount = 99;
			extendedHoursRequestLineObj1.TotalWeight = 500m;
			extendedHoursRequestLineObj1.UQ = Core.Constants.Weight.Kilograms;
			extendedHoursRequestLineObj1.SupplierName = "수출화주";

			var extendedHoursRequestLineObj2 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj2.ReferenceNumber = "6N00221012345X";
			extendedHoursRequestLineObj2.CustomsValue = 2000m;
			extendedHoursRequestLineObj2.PackageCount = 2;
			extendedHoursRequestLineObj2.TotalWeight = 3000m;
			extendedHoursRequestLineObj2.UQ = Core.Constants.Weight.Grams;
			extendedHoursRequestLineObj2.SupplierName = "수출화주2";
			#endregion
			var message = Factory.New<EDIMessage>();

			var export5AC = new Export5ACCreator().Create(extendedHoursRequestHeaderObj);
			var result = new GOVCBR5ACMessageBuilder(export5AC).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._5AC;
				message.SetEM_MessageTextOrDataSource(stream);
				Factory.Save();
			}

			var entry = Factory.New<CusMiscRequestHeader>();
			entry.Messages.Add(message);

			var wrapper = new MiscRequestDocumentWrapper(entry, Factory);

			AssertNotNull(wrapper.MessageData5AC);
			AssertEquals("임시개청사유", wrapper.MessageData5AC.Reason);
			AssertEquals(2, wrapper.MessageData5AC.ExtendedHoursRequestLines.Count);

			AssertEquals(2, wrapper.MessageData5AC.ExtendedHoursRequestLines.Count);
			AssertEquals("6N00221000025X", wrapper.MessageData5AC.ExtendedHoursRequestLines[0].ReferenceNumber);
			AssertEquals("6N002-21-000025X", wrapper.MessageData5AC.ExtendedHoursRequestLines[0].FormattedReferenceNumber);
			AssertEquals(1000m, wrapper.MessageData5AC.ExtendedHoursRequestLines[0].CustomsValue);
			AssertEquals(2, wrapper.MessageData5AC.ExtendedHoursRequestLines[1].PackageCount);
			AssertEquals(3m, wrapper.MessageData5AC.ExtendedHoursRequestLines[1].TotalWeight);
			AssertEquals("수출화주2", wrapper.MessageData5AC.ExtendedHoursRequestLines[1].SupplierName);
		}

		public void Test5GWMessageData()
		{
			#region import5GW message
			var extendedHoursRequestHeaderObj = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeaderObj.CustomsOffice = "010";
			extendedHoursRequestHeaderObj.Department = "20";
			extendedHoursRequestHeaderObj.Reason = "임시개청사유";
			extendedHoursRequestHeaderObj.StartDate = new ZDateTime(2014, 05, 06, 12, 30, 00);
			extendedHoursRequestHeaderObj.EndDate = new ZDateTime(2014, 05, 09, 12, 30, 00);

			var extendedHoursRequestLineObj1 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj1.ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
			extendedHoursRequestLineObj1.ReferenceNumber = "6N00221000025M";
			extendedHoursRequestLineObj1.HSDescription = "USED CYLINDER";
			extendedHoursRequestLineObj1.CustomsValue = 1000m;
			extendedHoursRequestLineObj1.PackageCount = 99;
			extendedHoursRequestLineObj1.TotalWeight = 500m;
			extendedHoursRequestLineObj1.UQ = Core.Constants.Weight.Kilograms;
			extendedHoursRequestLineObj1.BondedAreaCode = "03077026";
			extendedHoursRequestLineObj1.PayerCompanyName = "한국소화화학품(주)";

			var extendedHoursRequestLineObj2 = extendedHoursRequestHeaderObj.ExtendedHoursRequestLines.AddNew();
			extendedHoursRequestLineObj2.ReferenceNumberType = ReferenceNumberTypeList.Codes.CMN;
			extendedHoursRequestLineObj2.ReferenceNumber = "13CSKAPH01900100001";
			extendedHoursRequestLineObj2.CustomsValue = 2000m;
			extendedHoursRequestLineObj2.PackageCount = 2;
			extendedHoursRequestLineObj2.TotalWeight = 3000m;
			extendedHoursRequestLineObj2.BondedAreaCode = "15310001";
			extendedHoursRequestLineObj2.UQ = Core.Constants.Weight.Grams;
			#endregion
			var message = Factory.New<EDIMessage>();

			var import5GW = new Import5GWCreator().Create(extendedHoursRequestHeaderObj);
			var result = new GOVCBR5GWMessageBuilder(import5GW).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._5GW;
				message.SetEM_MessageTextOrDataSource(stream);
				Factory.Save();
			}

			var entry = Factory.New<CusMiscRequestHeader>();
			entry.Messages.Add(message);

			var wrapper = new MiscRequestDocumentWrapper(entry, Factory);

			AssertNotNull(wrapper.MessageData5GW);
			AssertEquals("임시개청사유", wrapper.MessageData5GW.Reason);

			AssertEquals("6N00221000025M", wrapper.MessageData5GW.ExtendedHoursRequestLines[0].ReferenceNumber);
			AssertEquals(ReferenceNumberTypeList.Codes.IMP, wrapper.MessageData5GW.ExtendedHoursRequestLines[0].ReferenceNumberType);
			AssertEquals("6N002-21-000025M", wrapper.MessageData5GW.ExtendedHoursRequestLines[0].FormattedReferenceNumber);
			AssertEquals("한국소화화학품(주)", wrapper.MessageData5GW.ExtendedHoursRequestLines[0].PayerCompanyName);
			AssertEquals("USED CYLINDER", wrapper.MessageData5GW.ExtendedHoursRequestLines[0].HSDescription);
			AssertEquals(1000m, wrapper.MessageData5GW.ExtendedHoursRequestLines[0].CustomsValue);

			AssertEquals("13CSKAPH01900100001", wrapper.MessageData5GW.ExtendedHoursRequestLines[1].ReferenceNumber);
			AssertEquals(KR.Messaging.ReferenceNumberTypeList.Codes.CMN, wrapper.MessageData5GW.ExtendedHoursRequestLines[1].ReferenceNumberType);
			AssertEquals("13CSKAPH019-0010-0001", wrapper.MessageData5GW.ExtendedHoursRequestLines[1].FormattedReferenceNumber);
			AssertEquals(2, wrapper.MessageData5GW.ExtendedHoursRequestLines[1].PackageCount);
			AssertEquals(3m, wrapper.MessageData5GW.ExtendedHoursRequestLines[1].TotalWeight);
			AssertEquals("15310001", wrapper.MessageData5GW.ExtendedHoursRequestLines[1].BondedAreaCode);
		}

		public void TestImportEntries()
		{
			CreateKREntryHeaderDetailsViewCollection();

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG(new List<ZString> { "11598121000008U", "1234520000045M" });
			var collection = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			AssertEquals(2, collection.Count);

			var wrapper = GetMiscRequestDocumentWrapperForTest(ElectronicDocumentTypeList.Codes._5SG, KRJobMessageTypeList.Codes.Import);
			AssertEquals(1, wrapper.ImportEntries.Count);
			AssertEquals("11598121000008U", wrapper.ImportEntries[0].KEH_EntryNum);
		}

		public void TestFinalPriceReportExtensionHeader_ANT()
		{
			var miscRequestHeader = SetFinalPriceReportExtensionHeaderData("GOVCBR5SH_C.xml", CustomsEntryStatusTypeList.Codes.ANT);
			var wrapper = new MiscRequestDocumentWrapper(miscRequestHeader, Factory);
			var header = wrapper.FinalPriceReportExtensionHeader;
			AssertNotNull(header);
			AssertEquals(1, header.FinalPriceReportByDateExtensionLines.Count);

			AssertEquals(new ZDateTime(2021, 07, 08), header.ApprovalDateFrom5SH);
			AssertEquals("ANT", header.ResultFrom5SH);

			var line = header.FinalPriceReportByDateExtensionLines[0];
			AssertEquals("1234520000045M", line.ImportDeclarationNumber);
			AssertEquals(new ZDateTime(2021, 07, 07), line.EntryReleaseDateFrom5SH);

			AssertEquals(wrapper.ImportEntries[0], line.EntryDetailsView);
			AssertEquals("1234520000045M", line.EntryDetailsView.KEH_EntryNum);
		}

		public void TestFinalPriceReportExtensionHeader_DMS()
		{
			var miscRequestHeader = SetFinalPriceReportExtensionHeaderData("GOVCBR5SH_E.xml", CustomsEntryStatusTypeList.Codes.DMS);
			var wrapper = new MiscRequestDocumentWrapper(miscRequestHeader, Factory);
			var header = wrapper.FinalPriceReportExtensionHeader;
			AssertNotNull(header);
			AssertEquals(1, header.FinalPriceReportByDateExtensionLines.Count);

			AssertEquals(ZDateTime.Empty, header.ApprovalDateFrom5SH);
			AssertEquals("DMS", header.ResultFrom5SH);

			var line = header.FinalPriceReportByDateExtensionLines[0];
			AssertEquals("1234520000045M", line.ImportDeclarationNumber);
			AssertEquals(new ZDateTime(2021, 07, 07), line.EntryReleaseDateFrom5SH);

			AssertEquals(wrapper.ImportEntries[0], line.EntryDetailsView);
			AssertEquals("1234520000045M", line.EntryDetailsView.KEH_EntryNum);
		}

		CusMiscRequestHeader SetFinalPriceReportExtensionHeaderData(string xml, string messageOwner)
		{
			CreateKREntryHeaderDetailsViewCollection();

			var miscRequestHeader = Factory.New<CusMiscRequestHeader>();
			miscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			miscRequestHeader.CMR_RequestDate = ZDateTime.Today;
			miscRequestHeader.CMR_CustomsOffice = "010";
			miscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;

			var miscRequestLine = miscRequestHeader.RequestLines.AddNew();
			miscRequestLine.CML_EntryNumber = "1234520000045M";
			miscRequestLine.CML_EntryType = KRJobMessageTypeList.Codes.Import;

			var finalPriceReportByDateExtensionHeader1 = new FinalPriceReportByDateExtensionHeader(Factory);
			var finalPriceReportByDateExtensionLine1 = finalPriceReportByDateExtensionHeader1.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportByDateExtensionLine1.ImportDeclarationNumber = "1234520000045M";

			var message = miscRequestHeader.Messages.AddNew();
			var import5SG = new Import5SGHeaderCreator().Create(finalPriceReportByDateExtensionHeader1);
			var result = new GOVCBR5SGMessageBuilder(import5SG).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._5SG;
				message.SetEM_MessageTextOrDataSource(stream);
				Factory.Save();
			}

			var fileReader = new TestFileReader(typeof(GOVCBR5SHMessageProcessorTest));
			var messageText5SH = fileReader.GetEmbeddedFileText(TestFilesPath, xml);
			var incomingMessage5SH = miscRequestHeader.Messages.AddNew();
			incomingMessage5SH.EM_MessageType = ElectronicDocumentTypeList.Codes._5SH;
			incomingMessage5SH.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage5SH.EM_MessageText = messageText5SH;
			incomingMessage5SH.EM_ApplicationReference = message.EM_MessageNum;
			incomingMessage5SH.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5SG;
			incomingMessage5SH.EM_MessageOwner = messageOwner;
			Factory.Save();

			return miscRequestHeader;
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";

		void CreateKREntryHeaderDetailsViewCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.EntryNumber = "11598121000008U";
			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "1234520000045M";
			entry2.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalRejected;

			var entryNum = entry1.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryNum = entry2.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
			entryNum.CE_IssueDate = ZDateTime.Today;

			var entryNum934 = entry1.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum934.CE_ExpiryDate = ZDateTime.Today;
			entryNum934 = entry2.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum934.CE_ExpiryDate = ZDateTime.Today;

			Factory.Save();
		}

		MiscRequestDocumentWrapper GetMiscRequestDocumentWrapperForTest(ZString codeType, ZString lineCodeType,  string jobNumber = "1")
		{
			var miscRequestHeader = Factory.New<CusMiscRequestHeader>();
			miscRequestHeader.CMR_CustomsOffice = "01010";
			miscRequestHeader.CMR_MessageType = codeType;
			miscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			miscRequestHeader.CMR_JobNumber = jobNumber;
			miscRequestHeader.CMR_RequestDate = new ZDateTime(2024, 11, 25);
			miscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
			miscRequestHeader.Branch.GB_OH_OrgProxy = broker.PK;

			var miscRequestLine = miscRequestHeader.RequestLines.AddNew();
			miscRequestLine.CML_EntryNumber = "11598121000008U";
			miscRequestLine.CML_EntryType = lineCodeType;

			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_ParentID = miscRequestHeader.PK;
			entryNum.CE_ParentTable = miscRequestHeader.TableName;
			entryNum.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
			entryNum.CE_EntryType = codeType;
			entryNum.CE_EntryNum = "11598121000008U";
			entryNum.CE_IssueDate = new ZDateTime(2014, 05, 06, 12, 30, 00);
			entryNum.CE_ExpiryDate = new ZDateTime(2014, 05, 09, 12, 30, 00);

			Factory.Save();

			return new MiscRequestDocumentWrapper(miscRequestHeader, Factory);
		}
		protected override void SetUp()
		{
			base.SetUp();

			#region CustomsOffice and CustomsDepartment
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			broker = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			TestOrgDataSetUpHelper.AddOrgContact(broker, "홍길동", true);
			var customsAddress = broker.Addresses.AddNew();
			customsAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			TestOrgDataSetUpHelper.AddOrgAddress(customsAddress, "서울특별시 서초구 동광로 41");
			customsAddress.OA_Phone = "02354168589";
			var brokerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(broker, brokerCodes);

			Factory.Save();
			#endregion
		}
		OrgHeader broker;
	}
}
