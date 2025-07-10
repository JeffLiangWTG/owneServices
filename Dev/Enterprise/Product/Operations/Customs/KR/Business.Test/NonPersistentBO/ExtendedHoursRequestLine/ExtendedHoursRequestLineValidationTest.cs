using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExtendedHoursRequestLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestWithMessageErrorIfNotEnteredInReferenceNumberType()
		{
			var parent5GW = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			var parentLine5GW = parent5GW.ExtendedHoursRequestLines.AddNew();
			parentLine5GW.ReferenceNumberType = ZString.Empty;
			AssertHasMessageErrorContaining(parentLine5GW.ReferenceNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);

			parentLine5GW.ReferenceNumberType = ReferenceNumberTypeList.Codes.IMP;
			AssertNoMessageErrorContaining(parentLine5GW.ReferenceNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);

			var parent5AC = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine5AC = parent5AC.ExtendedHoursRequestLines.AddNew();
			parentLine5AC.ReferenceNumberType = ZString.Empty;
			AssertNoMessageErrorContaining(parentLine5AC.ReferenceNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithMessageErrorIfNotEnteredInHSDescription()
		{
			var parent5GW = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			var parentLine5GW = parent5GW.ExtendedHoursRequestLines.AddNew();
			parentLine5GW.HSDescription = ZString.Empty;
			AssertHasMessageErrorContaining(parentLine5GW.HSDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			parentLine5GW.HSDescription = "Test";
			AssertNoMessageErrorContaining(parentLine5GW.HSDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			var parent5AC = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine5AC = parent5AC.ExtendedHoursRequestLines.AddNew();
			parentLine5AC.HSDescription = ZString.Empty;
			AssertNoMessageErrorContaining(parentLine5AC.HSDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithMessageErrorIfNotEnteredInPayerCompanyName()
		{
			var parent5GW = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			var parentLine5GW = parent5GW.ExtendedHoursRequestLines.AddNew();
			parentLine5GW.PayerCompanyName = ZString.Empty;
			AssertHasMessageErrorContaining(parentLine5GW.PayerCompanyNameInfo, MandatoryValidation.YouHaveNotEntered);

			parentLine5GW.PayerCompanyName = "Test";
			AssertNoMessageErrorContaining(parentLine5GW.PayerCompanyNameInfo, MandatoryValidation.YouHaveNotEntered);

			var parent5AC = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine5AC = parent5AC.ExtendedHoursRequestLines.AddNew();
			parentLine5AC.PayerCompanyName = ZString.Empty;
			AssertNoMessageErrorContaining(parentLine5AC.PayerCompanyNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithMessageErrorIfNotEnteredInBondedAreaCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "11111111", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var parent5GW = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			var parentLine5GW = parent5GW.ExtendedHoursRequestLines.AddNew();
			parentLine5GW.BondedAreaCode = ZString.Empty;
			AssertHasMessageErrorContaining(parentLine5GW.BondedAreaCodeInfo, MandatoryValidation.YouHaveNotEntered);

			parentLine5GW.BondedAreaCode = "Test";
			AssertHasMessageErrorContaining(parentLine5GW.BondedAreaCodeInfo, ListValidation.InvalidCodeMessageError);

			parentLine5GW.BondedAreaCode = "11111111";
			AssertNoMessageErrors(parentLine5GW.BondedAreaCodeInfo);

			var parent5AC = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine5AC = parent5AC.ExtendedHoursRequestLines.AddNew();
			parentLine5AC.BondedAreaCode = ZString.Empty;
			AssertNoMessageErrorContaining(parentLine5AC.BondedAreaCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithMessageErrorIfNotEnteredInUQ()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			var parentLine = parent.ExtendedHoursRequestLines.AddNew();
			parentLine.UQ = ZString.Empty;
			AssertHasMessageErrorContaining(parentLine.UQInfo, MandatoryValidation.YouHaveNotEntered);

			parentLine.UQ = "KG";
			AssertNoMessageErrorContaining(parentLine.UQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithMessageErrorIfIsNegativeInPackageCount()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine = parent.ExtendedHoursRequestLines.AddNew();
			parentLine.PackageCount = -1;
			AssertHasMessageErrorContaining(parentLine.PackageCountInfo, MandatoryValidation.ValueCannotBeNegative);

			parentLine.PackageCount = 1;
			AssertNoMessageErrorContaining(parentLine.PackageCountInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestWithMessageErrorIfIsNegativeInCustomsValue()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine = parent.ExtendedHoursRequestLines.AddNew();
			parentLine.CustomsValue = -1m;
			AssertHasMessageErrorContaining(parentLine.CustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);

			parentLine.CustomsValue = 1m;
			AssertNoMessageErrorContaining(parentLine.CustomsValueInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestWithMessageErrorIfIsNegativeInTotalWeight()
		{
			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine = parent.ExtendedHoursRequestLines.AddNew();
			parentLine.TotalWeight = -1m;
			AssertHasMessageErrorContaining(parentLine.TotalWeightInfo, MandatoryValidation.ValueCannotBeNegative);

			parentLine.TotalWeight = 1m;
			AssertNoMessageErrorContaining(parentLine.TotalWeightInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestWithWarringAboutReferenceNumber()
		{
			var requestHeader = Factory.New<CusMiscRequestHeader>();
			requestHeader.CMR_JobNumber = "AAA001";
			requestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
			requestHeader.CMR_CustomsOffice = "01020";
			requestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
			requestHeader.CMR_RequestDate = ZDateTime.Today;
			requestHeader.CMR_Status = "OAC";
			requestHeader.CMR_GS_NKBroker = GlbStaff.CurrentUser.GS_Code;

			var requestLine = requestHeader.RequestLines.AddNew();
			requestLine.CML_EntryNumber = "6N00221000025X";
			requestLine.CML_EntryType = KRJobMessageTypeList.Codes.Export;
			Factory.Save();

			var parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5AC, GlbCompany.CurrentCompany.PK);
			var parentLine = parent.ExtendedHoursRequestLines.AddNew();
			parentLine.FormattedReferenceNumber = "6N0022-10-00025X";
			AssertHasWarningContaining(parentLine.FormattedReferenceNumberInfo, "There is another 5AC request, AAA001 which has this entry number. Please check it.");

			parentLine.FormattedReferenceNumber = "6N0022-10-00123X";
			AssertNoWarningContaining(parentLine.FormattedReferenceNumberInfo, "There is another 5AC request, AAA001 which has this entry number. Please check it.");

			var reauestHeader5GW = Factory.New<CusMiscRequestHeader>();
			reauestHeader5GW.CMR_JobNumber = "BBB001";
			reauestHeader5GW.CMR_MessageType = ElectronicDocumentTypeList.Codes._5GW;
			reauestHeader5GW.CMR_CustomsOffice = "01020";
			reauestHeader5GW.CMR_GB = GlbBranch.CurrentBranch.PK;
			reauestHeader5GW.CMR_RequestDate = ZDateTime.Today;
			reauestHeader5GW.CMR_Status = "OAC";
			reauestHeader5GW.CMR_GS_NKBroker = GlbStaff.CurrentUser.GS_Code;

			var requestLine5GW = reauestHeader5GW.RequestLines.AddNew();
			requestLine5GW.CML_EntryNumber = "22926200828220M";
			requestLine5GW.CML_EntryType = KRJobMessageTypeList.Codes.Import;
			Factory.Save();

			parent = new ExtendedHoursRequestHeader(Factory, ElectronicDocumentTypeList.Codes._5GW, GlbCompany.CurrentCompany.PK);
			parentLine = parent.ExtendedHoursRequestLines.AddNew();
			parentLine.FormattedReferenceNumber = "229262-00-828220M";

			AssertHasWarningContaining(parentLine.FormattedReferenceNumberInfo, "There is another 5GW request, BBB001 which has this entry number. Please check it.");

			parentLine.FormattedReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(parentLine.FormattedReferenceNumberInfo, "Please enter an entry number. If you don't have a specific entry number, then please enter 'NO'");

			parentLine.FormattedReferenceNumber = "NO";
			AssertNoErrors(parentLine.FormattedReferenceNumberInfo);
		}
	}
}
