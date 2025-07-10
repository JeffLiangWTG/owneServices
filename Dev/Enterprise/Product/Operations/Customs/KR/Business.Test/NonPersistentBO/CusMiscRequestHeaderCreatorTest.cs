using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusMiscRequestHeaderCreatorTest : TestCaseWithFactory
	{
		[TestDate(2021, 07, 20)]
		public void TestHeader()
		{
			var extendedHoursRequestHeader = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeader.CustomsOffice = "010";
			extendedHoursRequestHeader.Department = "10";
			extendedHoursRequestHeader.StartDate = new ZDateTime(2021, 07, 18);
			extendedHoursRequestHeader.EndDate = new ZDateTime(2021, 07, 19);
			extendedHoursRequestHeader.Reason = "임시개청 사유입니다.";
			extendedHoursRequestHeader.BranchPK = GlbBranch.CurrentBranch.PK;

			var result = new CusMiscRequestHeaderCreator().Create(extendedHoursRequestHeader);
			AssertEquals("CMR_JobNumber is set when it is saved.", ZString.Empty, result.CMR_JobNumber);
			Factory.Save();

			AssertEquals("01010", result.CMR_CustomsOffice);
			AssertEquals(extendedHoursRequestHeader.MessageType, result.CMR_MessageType);
			AssertEquals(extendedHoursRequestHeader.BranchPK, result.CMR_GB);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, result.CMR_GS_NKBroker);
			AssertEquals("MSC00000001", result.CMR_JobNumber);
			AssertEquals(extendedHoursRequestHeader.StartDate, result.CusEntryNumber.CE_IssueDate);
			AssertEquals(extendedHoursRequestHeader.EndDate, result.CusEntryNumber.CE_ExpiryDate);
			AssertEquals("2021-07-20", result.CMR_RequestDate.ToString(DateFormatType.DateKorean));
			AssertEquals("임시개청 시작일시 : 2021-07-18 00:00" +
						 "\r\n임시개청 종료일시 : 2021-07-19 00:00" +
						 "\r\n임시개청 사유 : 임시개청 사유입니다.", result.CMR_RequestDetails);
		}

		[TestDate(2021, 08, 02)]
		public void Test5SG()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = new FinalPriceReportByDateExtensionHeader(Factory);
			header.CustomsOffice = "010";
			header.GB_Branch = declaration.JE_GB;

			var line1 = header.FinalPriceReportByDateExtensionLines.AddNew();
			line1.ImportDeclarationNumber = "41634195023880M";
			line1.ExtensionDate = new ZDateTime(2021, 08, 02);
			line1.ApplicationReason = "연장신청사유1";

			var line2 = header.FinalPriceReportByDateExtensionLines.AddNew();
			line2.ImportDeclarationNumber = "42634295023880M";
			line2.ExtensionDate = ZDateTime.Empty;
			line2.ApplicationReason = "연장신청사유2";

			var result = new CusMiscRequestHeaderCreator().Create(header);
			AssertEquals("CMR_JobNumber is set when it is saved.", ZString.Empty, result.CMR_JobNumber);
			Factory.Save();

			AssertEquals("010", result.CMR_CustomsOffice);
			AssertEquals(ElectronicDocumentTypeList.Codes._5SG, result.CMR_MessageType);
			AssertEquals(declaration.JE_GB, result.CMR_GB);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, result.CMR_GS_NKBroker);
			AssertEquals("MSC00000001", result.CMR_JobNumber);
			AssertEquals("2021-08-02", result.CMR_RequestDate.ToString(DateFormatType.DateKorean));

			var resultLine = result.RequestLines;
			AssertEquals(2, resultLine.Count);

			AssertEquals("41634195023880M", resultLine[0].CML_EntryNumber);
			AssertEquals(ReferenceNumberTypeList.Codes.IMP, resultLine[0].CML_EntryType);
			AssertEquals("(2021-08-02) 연장신청사유1", resultLine[0].CML_Remarks);

			AssertEquals("42634295023880M", resultLine[1].CML_EntryNumber);
			AssertEquals(ReferenceNumberTypeList.Codes.IMP, resultLine[1].CML_EntryType);
			AssertEquals("(날짜를 입력하지 않았습니다.) 연장신청사유2", resultLine[1].CML_Remarks);
		}

		public void TestEmptyDate()
		{
			var extendedHoursRequestHeader = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			extendedHoursRequestHeader.StartDate = ZDateTime.Empty;
			extendedHoursRequestHeader.EndDate = ZDateTime.Empty;
			extendedHoursRequestHeader.Reason = "임시개청 사유입니다.";

			var result = new CusMiscRequestHeaderCreator().Create(extendedHoursRequestHeader);
			AssertEquals("임시개청 시작일시 : 날짜를 입력하지 않았습니다." +
						 "\r\n임시개청 종료일시 : 날짜를 입력하지 않았습니다." +
						 "\r\n임시개청 사유 : 임시개청 사유입니다.", result.CMR_RequestDetails);
		}

		public void Test5AC()
		{
			var extendedHoursRequestHeader = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);

			var line1 = extendedHoursRequestHeader.ExtendedHoursRequestLines.AddNew();
			line1.ReferenceNumber = "2362520050702X";
			line1.ReferenceNumberType = ReferenceNumberTypeList.Codes.EXP;
			var line2 = extendedHoursRequestHeader.ExtendedHoursRequestLines.AddNew();
			line2.ReferenceNumber = "2362520042782X";
			line2.ReferenceNumberType = ReferenceNumberTypeList.Codes.EXP;

			Setup(line1, line2);

			var result = new CusMiscRequestHeaderCreator().Create(extendedHoursRequestHeader);

			var resultLine = result.RequestLines;
			AssertEquals(2, resultLine.Count);

			AssertEquals("2362520050702X", resultLine[0].CML_EntryNumber);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, resultLine[0].CML_EntryType);
			AssertEquals("신고가격(USD) : 500,000" +
						 "\r\n총포장개수 : 1,234" +
						 "\r\n총중량 : 1,000.100(KG)" +
						 "\r\n수출화주 : 가야티앤아이", resultLine[0].CML_Remarks);

			AssertEquals("2362520042782X", resultLine[1].CML_EntryNumber);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, resultLine[1].CML_EntryType);
			AssertEquals("신고가격(USD) : 5,000" +
						 "\r\n총포장개수 : 111" +
						 "\r\n총중량 : 10.123(KG)" +
						 "\r\n수출화주 : 광화화성", resultLine[1].CML_Remarks);
		}

		public void Test5GW()
		{
			var extendedHoursRequestHeader = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);

			var line1 = extendedHoursRequestHeader.ExtendedHoursRequestLines.AddNew();
			line1.ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
			line1.ReferenceNumber = "2292620002820M";
			var line2 = extendedHoursRequestHeader.ExtendedHoursRequestLines.AddNew();
			line2.ReferenceNumberType = ReferenceNumberTypeList.Codes.CMN;
			line2.ReferenceNumber = "20HDMUA808I00240001";

			Setup(line1, line2);

			var result = new CusMiscRequestHeaderCreator().Create(extendedHoursRequestHeader);

			var resultLine = result.RequestLines;
			AssertEquals(2, resultLine.Count);

			AssertEquals("2292620002820M", resultLine[0].CML_EntryNumber);
			AssertEquals(ReferenceNumberTypeList.Codes.IMP, resultLine[0].CML_EntryType);
			AssertEquals("과세가격(USD) : 500,000" +
						 "\r\n총포장개수 : 1,234" +
						 "\r\n총중량 : 1,000.100(KG)" +
						 "\r\n품명 : ROCKY MOUNTAIN EURO" +
						 "\r\n납세의무자상호 : (주)이알코퍼레이션" +
						 "\r\n(예정)장치장소 : 01206022", resultLine[0].CML_Remarks);

			AssertEquals("20HDMUA808I00240001", resultLine[1].CML_EntryNumber);
			AssertEquals(ReferenceNumberTypeList.Codes.CMN, resultLine[1].CML_EntryType);
			AssertEquals("과세가격(USD) : 5,000" +
						 "\r\n총포장개수 : 111" +
						 "\r\n총중량 : 10.123(KG)" +
						 "\r\n품명 : COFFEE MACHINE" +
						 "\r\n납세의무자상호 : 로투스베이커리즈코리아(주)" +
						 "\r\n(예정)장치장소 : 03077016", resultLine[1].CML_Remarks);
		}

		void Setup(ExtendedHoursRequestLine line1, ExtendedHoursRequestLine line2)
		{
			line1.CustomsValue = 500000m;
			line1.PackageCount = 1234;
			line1.TotalWeight = 1000.1m;
			line1.UQ = Core.Constants.Weight.Kilograms;
			line1.SupplierName = "가야티앤아이";
			line1.HSDescription = "ROCKY MOUNTAIN EURO";
			line1.PayerCompanyName = "(주)이알코퍼레이션";
			line1.BondedAreaCode = "01206022";

			line2.CustomsValue = 5000m;
			line2.PackageCount = 111;
			line2.TotalWeight = 10.123m;
			line2.UQ = Core.Constants.Weight.Kilograms;
			line2.SupplierName = "광화화성";
			line2.HSDescription = "COFFEE MACHINE";
			line2.PayerCompanyName = "로투스베이커리즈코리아(주)";
			line2.BondedAreaCode = "03077016";
		}
	}
}
