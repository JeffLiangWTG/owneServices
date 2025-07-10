using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
abstract class NctsHeaderValidationTest : NctsDepartureValidationTest
{
	public void TestCheckAuthorization()
	{
		var header = Factory.NewDepartureNctsHeader();
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var requirement = header.DocAddresses.FindOrCreateWithRequirement(header.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = orgHeader.MainAddress.PK;

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, header.Consignor.OrganisationPK, "1111111", ZDate.Today.AddDays(-10), ZDate.Today.AddDays(10));

		header.Authorization = ZString.Empty;
		AssertNoMessageErrors(header.AuthorizationInfo);

		header.Authorization = "1111111";
		AssertNoMessageErrors(header.AuthorizationInfo);

		header.Authorization = "yyyyyyy";
		AssertHasMessageError(header.AuthorizationInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckRepresentationType()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		ValidationTestHelper.AssertInvalidCodeMessageError(header.RepresentationTypeInfo, "~", RepresentationTypeList.Codes._1Self);
	}

	public void TestCheckDeclarantAddressPK()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.RepresentationType = RepresentationTypeList.Codes._1Self;
		var validation = header.Validation as NctsHeaderValidation;
		AssertNotNull("PRE-CONDITION: Validation is Phase4", validation);

		validation.ValidateDeclarantAddressPK();
		AssertNoMessageErrors(header.DeclarantAddressPKInfo);
		header.RepresentationType = RepresentationTypeList.Codes._2Direct;
		validation.ValidateDeclarantAddressPK();
		AssertHasMessageError(header.DeclarantAddressPKInfo, "For the selected Rep. Type this field is mandatory.");
		header.RepresentationType = RepresentationTypeList.Codes._3Indirect;
		validation.ValidateDeclarantAddressPK();
		AssertHasMessageError(header.DeclarantAddressPKInfo, "For the selected Rep. Type this field is mandatory.");
		header.DeclarantAddressPK = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
		AssertNoMessageErrors(header.DeclarantAddressPKInfo);
	}

	public void TestCheckSubscriberMandatoryValidation()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.Subscriber = "XXX";
		AssertNoMessageErrorContaining("Empty Subscriber", nctsHeader.SubscriberInfo, MandatoryValidation.YouHaveNotEntered);

		nctsHeader.Subscriber = ZString.Empty;
		AssertHasMessageErrorContaining("Filled Subscriber", nctsHeader.SubscriberInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckSubscriberListValidation()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";

		var staff1 = Factory.New<GlbStaff>();
		staff1.GS_Code = "ST1";
		var staff1Wrapper = IT.Business.GlbStaffWrapper.Get(staff1);
		staff1Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-A", "DEC1")
			.Build();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_CustomsProfile = ZString.Empty;
		nctsHeader.Subscriber = "ABC";
		AssertHasMessageErrorContaining("Inexistent Subscriber", nctsHeader.SubscriberInfo, ListValidation.InvalidCodeMessageError.ToString());

		nctsHeader.BH_CustomsProfile = "1234-A";
		nctsHeader.Subscriber = "ST1";
		AssertNoMessageErrorContaining("Valid Subscriber", nctsHeader.SubscriberInfo, ListValidation.InvalidCodeMessageError.ToString());

		nctsHeader.Subscriber = "ST2";
		AssertHasMessageErrorContaining("Invalid Subscriber for selected node", nctsHeader.SubscriberInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckBH_CustomsProfileMandatoryValidation()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_CustomsProfile = "1234";
		AssertNoMessageErrorContaining("Empty Node", nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

		nctsHeader.BH_CustomsProfile = ZString.Empty;
		AssertHasMessageErrorContaining("Empty Node", nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBH_CustomsProfileListValidation()
	{
		var declarantAA = Factory.New<OrgHeader>();
		declarantAA.OH_Code = "AA";
		var declarantBB = Factory.New<OrgHeader>();
		declarantBB.OH_Code = "BB";
		Factory.Save();

		var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();
		new AccountCollectionTestBuilder(currentCompanyPk)
			.AppendAccount("11111111111-001", "XXXX").AppendAccountDetail("XXXX-AA", "AA")
			.AppendAccount("11111111111-002", "YYYY").AppendAccountDetail("YYYY-BB", "BB")
			.Build();

		CustomsProfileListTestHelper.ClearCustomsProfilesLookupsCache(Factory, currentCompanyPk);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.DeclarantAddressPK = ZGuid.Empty;
		nctsHeader.BH_CustomsProfile = "XXXX-AA";
		AssertNoMessageErrorContaining("Empty Declarant - valid node", nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError.ToString());
		nctsHeader.BH_CustomsProfile = "YYYY-BB";
		AssertNoMessageErrorContaining("Empty Declarant - valid node", nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError.ToString());
		nctsHeader.BH_CustomsProfile = "1234";
		AssertHasMessageErrorContaining("Empty Declarant - invalid node", nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError.ToString());

		nctsHeader.DeclarantAddressPK = declarantAA.MainAddress.PK;
		nctsHeader.BH_CustomsProfile = "XXXX-AA";
		AssertNoMessageErrorContaining("Filled Declarant - valid node", nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError.ToString());
		nctsHeader.BH_CustomsProfile = "YYYY-BB";
		AssertHasMessageErrorContaining("Filled Declarant - invalid node", nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError.ToString());
		nctsHeader.BH_CustomsProfile = "1234";
		AssertHasMessageErrorContaining("Filled Declarant - invalid node", nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	[TestDate(2020, 02, 01)]
	[UseSnapshotProtection]
	public void TestCheckBH_CustomsProfileWarningWhenRemainingNumbersAreLessThanThreshold()
	{
		const string taxNumber = "11111111111";
		var currentCompanyPK = GlbCompany.CurrentCompany.PK;
		var tempFactory = new BusinessObjectFactory();
		var company = tempFactory.Load<GlbCompany>(currentCompanyPK);
		var numberViewStmNum1 = ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, taxNumber, minimumValue: 1, maximumValue: 1000, currentValue: 998);
		tempFactory.Save();

		var query = new ZQuery(ViewStmNumsSchema.SN_Name, numberViewStmNum1.SN_Name);
		query.AddToFilter(ViewStmNumsSchema.SN_Owner, numberViewStmNum1.SN_Owner);
		numberViewStmNum1 = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
		numberViewStmNum1.Provider = company.CustomsNumberProvider;
		numberViewStmNum1.FirstNumberRange.SNR_ThresholdRunOutWarning = 10;
		company = Factory.Load<GlbCompany>(currentCompanyPK);

		var declarantAA = Factory.New<OrgHeader>();
		declarantAA.OH_Code = "AA";
		Factory.Save();

		new AccountCollectionTestBuilder(currentCompanyPK)
			.AppendAccount("11111111111-001", "AAAA")
			.AppendAccountDetail("AAAA-AA", "AA")
			.Build();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_CustomsProfile = "AAAA-AA";
		AssertHasWarningContaining(nctsHeader.BH_CustomsProfileInfo, "The available Number Range for range type R, account 11111111111 is less than 3. Create a new Number Range in Companies or expand the existing one.");

		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, taxNumber, minimumValue: 1, maximumValue: 1000, currentValue: 1);
		Factory.Save();

		nctsHeader.Validation.ValidateBH_CustomsProfile();
		AssertNoWarnings(nctsHeader.BH_CustomsProfileInfo);
	}

	[TestDate(2020, 01, 01)]
	public void TestCheckBH_CustomsProfileCannotRetrievePanNumberRange()
	{
		var messageError = ValidationCaptions.ProgressiveAnnualNumber.CannotRetrievePanNumberRangeForSelectedNode;

		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.New<OrgHeader>().OH_Code = "DEC2";
		Factory.New<OrgHeader>().OH_Code = "DEC3";
		Factory.New<OrgHeader>().OH_Code = "DEC4";
		Factory.New<OrgHeader>().OH_Code = "DEC5";
		Factory.Save();

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "AAAA").AppendAccountDetail("AAAA-DEC1", "DEC1")
			.AppendAccount("22222222222-001", "BBBB").AppendAccountDetail("BBBB-DEC2", "DEC2")
			.AppendAccount("33333333333-001", "CCCC").AppendAccountDetail("CCCC-DEC3", "DEC3")
			.AppendAccount("44444444444-001", "DDDD").AppendAccountDetail("DDDD-DEC4", "DEC4")
			.AppendAccount("55555555555-001", "EEEE").AppendAccountDetail("EEEE-DEC5", "DEC5")
			.Build();

		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "22222222222", currentValue: 999999);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "33333333333", currentValue: 10000);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2018, "44444444444", currentValue: 10000);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2019, "55555555555", currentValue: 10000);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.BH_CustomsProfile = ZString.Empty;
		AssertNoMessageErrorContaining("When Node is not selected, error message should not appear", nctsHeader.BH_CustomsProfileInfo, messageError);

		nctsHeader.BH_CustomsProfile = "XXXX";
		AssertHasMessageErrorContaining("An invalid node is selected. PAN can't be retrieved", nctsHeader.BH_CustomsProfileInfo, messageError);

		nctsHeader.BH_CustomsProfile = "AAAA-DEC1";
		AssertHasMessageErrorContaining("A valid node is selected, but the number range is not configured. PAN can't be retrieved", nctsHeader.BH_CustomsProfileInfo, messageError);

		nctsHeader.BH_CustomsProfile = "BBBB-DEC2";
		AssertHasMessageErrorContaining("A valid node is selected and the number range is configured but the maximum value has been reached. PAN can't be retrieved", nctsHeader.BH_CustomsProfileInfo, messageError);

		nctsHeader.BH_CustomsProfile = "CCCC-DEC3";
		AssertNoMessageErrorContaining("A valid node is selected and the number range is configured. PAN can be retrieved", nctsHeader.BH_CustomsProfileInfo, messageError);

		nctsHeader.BH_CustomsProfile = "DDDD-DEC4";
		AssertHasMessageErrorContaining("A valid node is selected but it has no number range available for current/last year. PAN can't be retrieved", nctsHeader.BH_CustomsProfileInfo, messageError);

		nctsHeader.BH_CustomsProfile = "EEEE-DEC5";
		AssertNoMessageErrorContaining("A valid node is selected. It has no number range available for current year, but it has a number range for last year. PAN can be retrieved (the number range will be cloned)", nctsHeader.BH_CustomsProfileInfo, messageError);
	}

	public void TestCheckBH_CustomsProfileCannotRetrievePanNumberRange_Phase5()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var companyWrapper = IT.Business.GlbCompanyWrapper.Get(currentCompany);

			var accountDetail = companyWrapper.PasswordCollection.AddNew();
			accountDetail.GP_UserID = "1111";
			accountDetail.GP_Name = "AA";
			accountDetail.GP_MailBoxID = "11111111111-001";
			currentCompany.Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var organizationAA = Factory.New<OrgHeader>();
			organizationAA.OH_Code = "AA";
			var address = organizationAA.Addresses.AddNew();
			nctsHeader.Principal.E2_OA_Address = address.PK;

			nctsHeader.BH_CustomsProfile = "1111";
			AssertNoMessageErrors("Despite PAN can't be retrieved, no validation in Phase 5 on this", nctsHeader.BH_CustomsProfileInfo);
		}
	}

	public void TestCheckBH_RL_NKImportLoadPortWithInBondEntryTypeEqualsTIR()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;

		nctsHeader.BH_RL_NKImportLoadPort = ZString.Empty;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertHasMessageErrorContaining(nctsHeader.BH_RL_NKImportLoadPortInfo, ValidationCaptions.NctsHeader.CountryOfDispatchIsequired);

		nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertNoMessageErrorContaining(nctsHeader.BH_RL_NKImportLoadPortInfo, ValidationCaptions.NctsHeader.CountryOfDispatchIsequired);
	}

	public void TestCheckBH_RL_NKImportLoadPortWithSingleGoodItem()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		nctsHeader.BH_RL_NKImportLoadPort = ZString.Empty;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertHasMessageErrorContaining("When Country of Dispatch is not available at Header level and Goods items are empty", nctsHeader.BH_RL_NKImportLoadPortInfo, "You have not entered a Dispatch Country");
		var nctsDesc = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDesc.BY_RN_NKCountryOfDispatch = ZString.Empty;
		nctsHeader.BH_RL_NKImportLoadPort = ZString.Empty;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertHasMessageErrorContaining("When Country of Dispatch is not available at Goods Item nor Header level", nctsHeader.BH_RL_NKImportLoadPortInfo, "You have not entered a Dispatch Country");

		nctsDesc.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Italy;
		nctsHeader.BH_RL_NKImportLoadPort = ZString.Empty;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertNoMessageErrorContaining("When Country of Dispatch is available at Goods Item level", nctsHeader.BH_RL_NKImportLoadPortInfo, "You have not entered a Dispatch Country");
		nctsDesc.BY_RN_NKCountryOfDispatch = ZString.Empty;
		nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertNoMessageErrorContaining("When Country of Dispatch is not available at Header level", nctsHeader.BH_RL_NKImportLoadPortInfo, "You have not entered a Dispatch Country");

		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		var nctsDescWithValidValue = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
		nctsDescWithValidValue.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertHasMessageErrorContaining("When Country of Dispatch at Goods Item and Header level are different", nctsHeader.BH_RL_NKImportLoadPortInfo, "Dispatch Country in the Declaration is different from Dispatch Country in Goods Items");

		nctsDescWithValidValue.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertNoMessageErrorContaining("When Country of Dispatch at Goods Item and Header level are equal", nctsHeader.BH_RL_NKImportLoadPortInfo, "Dispatch Country in the Declaration is different from Dispatch Country in Goods Items");
	}

	public void TestCheckBH_RL_NKImportLoadPortWithMultipleGoodItems()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsDesc = nctsHeader.MovementHeader.GoodsItems.AddNew();

		nctsDesc.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		var nctsDescTwoWithValidValue = nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsDescTwoWithValidValue.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.UnitedKingdom;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertNoMessageErrorContaining(nctsHeader.BH_RL_NKImportLoadPortInfo, "Dispatch Country in the Declaration is different from Dispatch Country in Goods Items");

		nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertHasMessageErrorContaining(nctsHeader.BH_RL_NKImportLoadPortInfo, "Dispatch Country in the Declaration is different from Dispatch Country in Goods Items");

		nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertNoMessageErrorContaining(nctsHeader.BH_RL_NKImportLoadPortInfo, "Dispatch Country in the Declaration is different from Dispatch Country in Goods Items");

		nctsHeader.MovementHeader.GoodsItems.DeleteAll();
		nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsHeader.MovementHeader.GoodsItems.AddNew();
		nctsHeader.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
		nctsHeader.Validation.ValidateBH_RL_NKImportLoadPort();
		AssertNoMessageErrorContaining(nctsHeader.BH_RL_NKImportLoadPortInfo, "Dispatch Country in the Declaration is different from Dispatch Country in Goods Items");
	}

	public void TestValidateConsignorMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		AssertTraderMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage(nameof(goodsItem.Consignor), nctsHeader.Consignor, goodsItem.Consignor);
	}

	public void TestValidateConsigneeMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

		AssertTraderMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage(nameof(goodsItem.Consignee), nctsHeader.Consignee, goodsItem.Consignee);
	}

	public void TestConditionC001CountrySpecific()
	{
		var expectedMessageError = "Consignee Trader is required for goods destined for NCTS contracting parties.";

		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "DE";
		nctsHeader.Consignor.OrganisationPK = GetNewOrgHeaderPK();
		var consginee = nctsHeader.Consignee;
		consginee.OrganisationPK = ZGuid.Empty;
		var goodsItem1 = nctsHeader.MovementHeader.GoodsItems.AddNew();
		var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();

		goodsItem1.Consignee.OrganisationPK = GetNewOrgHeaderPK();
		goodsItem2.Consignee.OrganisationPK = GetNewOrgHeaderPK();

		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.StandardOneSupplierOneImporter;
		consginee.Validation.ValidateOrganisationPK();
		AssertHasMessageErrorContaining("When ParticipantType is STD, C001 on Consignee is enable", consginee.OrganisationPKInfo, expectedMessageError);

		nctsHeader.MovementHeader.ParticipantType = NctsParticipantTypeList.Codes.GroupageManySuppliersAndManyImporters;
		consginee.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining("When ParticipantType is GRP, C001 on Consignee is disable", consginee.OrganisationPKInfo, expectedMessageError);

		ZGuid GetNewOrgHeaderPK() => Factory.New<OrgHeader>().PK;
	}

	void AssertTraderMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupage(string traderName, JobDocAddress traderHeaderLevel, JobDocAddress traderGoodsItem)
	{
		var expectedMessageError = "To declare Consignor and Consignee at Goods Items level (Participants = GRP), Consignor and Consignee must be empty in Departure Declaration Header TAB";
		traderGoodsItem.OrganisationPK = Factory.New<OrgHeader>().PK;

		traderHeaderLevel.OrganisationPK = Factory.New<OrgHeader>().PK;
		AssertHasMessageErrorContaining($"When declaration is GRP and {traderName} is declared on both Header and Goods Item level", traderHeaderLevel.OrganisationPKInfo, expectedMessageError);

		traderHeaderLevel.OrganisationPK = ZGuid.Empty;
		traderGoodsItem.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining($"When declaration is GRP and {traderName} is declared only at Goods Item level", traderHeaderLevel.OrganisationPKInfo, expectedMessageError);
	}

	protected override void SetPlaceOfLoading(EU.NCTS.Business.NctsDepartureMovementHeader movementHeader, ZString value)
	{
		movementHeader.BM_PlaceOfLoading = value;
	}

	protected override string NctsDeclarationTypeListCountryCode => Core.Constants.CountryCodes.Italy;

	protected override BusinessObject GetNewBusinessObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return nctsHeader;
	}
}
