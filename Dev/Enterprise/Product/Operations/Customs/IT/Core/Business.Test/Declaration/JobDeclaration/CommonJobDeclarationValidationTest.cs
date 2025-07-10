using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class CommonJobDeclarationValidationTest : EU.Business.Declaration.Testing.JobDeclarationValidationTest
{
	public void TestCheckJE_OA_RepresentativeNotEqualToDeclarant()
	{
		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
		var expectedErrorMessage = "Representative must be different from Declarant";

		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_OA_DeclarantAddress = orgHeader1.MainAddress.PK;
		declaration.JE_OA_Representative = orgHeader1.MainAddress.PK;

		var validation = declaration.Validation;
		var representativeInfo = declaration.JE_OA_RepresentativeInfo;

		declaration.JE_MessageType = "EXP";
		validation.ValidateJE_OA_Representative();
		AssertNoMessageError("When declaration is not UCC6, no error message is expected for Representative equal to Declarant", representativeInfo, expectedErrorMessage);

		declaration.JE_MessageType = "IMP";
		AssertRepresentativeDifferentFromDeclarant_UCC6();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			AssertRepresentativeDifferentFromDeclarant_UCC6();
		}

		void AssertRepresentativeDifferentFromDeclarant_UCC6()
		{
			declaration.JE_OA_DeclarantAddress = orgHeader1.MainAddress.PK;
			declaration.JE_OA_Representative = orgHeader1.MainAddress.PK;

			validation.ValidateJE_OA_Representative();
			AssertHasMessageError($"When declaration is UCC6 {declaration.JE_MessageType}, error message is expected for Representative equal to Declarant", representativeInfo, expectedErrorMessage);

			declaration.JE_OA_Representative = orgHeader2.MainAddress.PK;
			validation.ValidateJE_OA_Representative();
			AssertNoMessageError("When Declarant and Representative are different, no error message is expected about Representative equal to Declarant", representativeInfo, expectedErrorMessage);

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_OA_Representative = ZGuid.Empty;
			validation.ValidateJE_OA_Representative();
			AssertNoMessageError("When Declarant and Representative are both empty, no error message is expected", representativeInfo, expectedErrorMessage);
		}
	}

	public void TestCheckJE_OA_RepresentativeNotEmptyWhenRepTypeIsFilled()
	{
		var representative = Factory.NewWithValidTestData<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		declaration.JE_DeclarantType = string.Empty;

		var validation = declaration.Validation;
		var representativeInfo = declaration.JE_OA_RepresentativeInfo;

		declaration.JE_MessageType = "EXP";
		validation.ValidateJE_OA_Representative();
		AssertNoMessageErrorContaining("When declaration is not UCC6, no error message is expected for Empty representative, even if Rep. Type is filled", representativeInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_MessageType = "IMP";
		AssertRepresentativeCannotBeEmptyIfRepTypeIsFilled();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			AssertRepresentativeCannotBeEmptyIfRepTypeIsFilled();
		}

		void AssertRepresentativeCannotBeEmptyIfRepTypeIsFilled()
		{
			declaration.JE_DeclarantType = "ARG";
			declaration.JE_OA_Representative = Guid.Empty;
			validation.ValidateJE_OA_Representative();
			AssertHasMessageErrorContaining($"When declaration is UCC6 {declaration.JE_MessageType}, error message is expected for empty Representative", representativeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OA_Representative = representative.MainAddress.PK;
			validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When Representative is filled, no error message is expected", representativeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = string.Empty;
			declaration.JE_OA_Representative = Guid.Empty;
			validation.ValidateJE_OA_Representative();
			AssertNoMessageErrorContaining("When Rep Type is not filled and Representative is empty, no error message is expected", representativeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestCheckJE_DeclarantTypeNotEmptyWhenRepresentativeIsFilled()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();

		var validation = declaration.Validation;
		var declarantTypeInfo = declaration.JE_DeclarantTypeInfo;

		declaration.JE_DeclarantType = string.Empty;
		declaration.JE_OA_Representative = ZGuid.Empty;
		validation.ValidateJE_DeclarantType();
		AssertHasMessageErrorContaining("When declaration is not UCC6, error message is expected for empty DeclarantType even with empty Representative", declarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_MessageType = "IMP";
		AssertDeclarantTypeCannotBeEmptyWhenRepresentaiveIsFilled();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "EXP";
			AssertDeclarantTypeCannotBeEmptyWhenRepresentaiveIsFilled();
		}

		void AssertDeclarantTypeCannotBeEmptyWhenRepresentaiveIsFilled()
		{
			declaration.JE_DeclarantType = string.Empty;
			declaration.JE_OA_Representative = orgHeader.MainAddress.PK;
			validation.ValidateJE_DeclarantType();
			AssertHasMessageErrorContaining($"When declaration is UCC6 {declaration.JE_MessageType}, error message is expected for empty DeclarantType", declarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = "ARG";
			validation.ValidateJE_DeclarantType();
			AssertNoMessageErrorContaining("When Rep. Type is filled, no error message is expected", declarantTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_DeclarantType = string.Empty;
			declaration.JE_OA_Representative = Guid.Empty;
			validation.ValidateJE_DeclarantType();
			AssertNoMessageErrorContaining("When Rep Type is not filled and Representative is empty, no error message is expected", declarantTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public override void TestCheckJE_LocationOfGoods()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var authorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: orgHeader.PK, "999999", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, ITCusAuthorisationRuleTypeList.Codes.Location, "MYLOC1");
		CusAuthorisationRuleTestHelper.AddAuthorisationRule(authorisationHeader, "UND", "UNDEFINED");

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = orgHeader.PK;

		declaration.ZG_AuthorisationNumber = "";
		declaration.JE_LocationOfGoods = "MYLOC1";
		declaration.JE_LocationQualifier = "FC";
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

		declaration.ZG_AuthorisationNumber = "111111";
		declaration.JE_LocationQualifier = "LB";
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

		declaration.ZG_AuthorisationNumber = "999999";
		declaration.JE_LocationOfGoods = "";
		AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_LocationOfGoods = "MYLOC1";
		AssertNoMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_LocationOfGoods = "UND";
		AssertHasMessageErrorContaining(declaration.JE_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckJE_ContainerMode()
	{
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		supplier.OH_IsConsignor = true;

		declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
		AssertNoNotifications(declaration.JE_ContainerModeInfo);

		declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
		AssertHasMessageErrorContaining(declaration.JE_ContainerModeInfo, "Container mode contains a containerized type, at least one Container record must be entered.");

		var container = declaration.CusContainers.AddNew();
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;

		AssertEquals(1, declaration.CusContainers.Count);
		AssertNoNotifications(declaration.JE_ContainerModeInfo);
	}

	public override void TestCheckJE_CustomsOffice()
	{
		base.TestCheckJE_CustomsOffice();
		var error = "Customs Office should be 8 alphanumeric characters.";

		declaration.JE_CustomsOffice = "ZBC";
		AssertHasError(declaration.JE_CustomsOfficeInfo, error);

		declaration.JE_CustomsOffice = "IT12312%";
		AssertHasError(declaration.JE_CustomsOfficeInfo, error);

		declaration.JE_CustomsOffice = "IT123128";
		AssertNoError(declaration.JE_CustomsOfficeInfo, error);
	}

	public void TestCheckJE_DeclarantTypeForImportDeclaration()
	{
		var declarantAndImporter = Factory.NewWithValidTestData<OrgHeader>();
		var declarantButNotImporter = Factory.NewWithValidTestData<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = declarantAndImporter.PK;
		declaration.JE_DeclarantType = "SEL";

		declaration.JE_OA_DeclarantAddress = declarantAndImporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OA_DeclarantAddress = declarantButNotImporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertHasMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OH_Importer = declarantAndImporter.PK;
		declaration.JE_DeclarantType = "DIR";

		declaration.JE_OA_DeclarantAddress = declarantAndImporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OA_DeclarantAddress = declarantButNotImporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OH_Importer = declarantAndImporter.PK;
		declaration.JE_DeclarantType = "IND";

		declaration.JE_OA_DeclarantAddress = declarantAndImporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OA_DeclarantAddress = declarantButNotImporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");
	}

	public void TestCheckJE_DeclarantTypeForExportDeclaration()
	{
		var declarantAndExporter = Factory.NewWithValidTestData<OrgHeader>();
		var declarantButNotExporter = Factory.NewWithValidTestData<OrgHeader>();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_OH_Supplier = declarantAndExporter.PK;
		declaration.JE_DeclarantType = "SEL";

		declaration.JE_OA_DeclarantAddress = declarantAndExporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OA_DeclarantAddress = declarantButNotExporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertHasMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OH_Supplier = declarantAndExporter.PK;
		declaration.JE_DeclarantType = "DIR";

		declaration.JE_OA_DeclarantAddress = declarantAndExporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OA_DeclarantAddress = declarantButNotExporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OH_Supplier = declarantAndExporter.PK;
		declaration.JE_DeclarantType = "IND";

		declaration.JE_OA_DeclarantAddress = declarantAndExporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");

		declaration.JE_OA_DeclarantAddress = declarantButNotExporter.MainAddress.PK;
		declaration.Validation.ValidateJE_DeclarantType();
		AssertNoMessageErrorContaining(declaration.JE_DeclarantTypeInfo, "There is no coherence between Representative Type and selected Declarant");
	}

	public void TestCheckJE_CustomsProfile()
	{
		var declarantWithOneNode = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithOneNode.OH_Code = "BB";
		var declarantWithTwoNodes = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithTwoNodes.OH_Code = "CC";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "XXXX").AppendAccountDetail("BB-XXXX", "BB")
			.AppendAccount("22222222222-001", "YYYY").AppendAccountDetail("CC-YYYY", "CC")
			.AppendAccount("33333333333-001", "ZZZZ").AppendAccountDetail("CC-ZZZZ", "CC")
			.Build();

		declaration.JE_OA_DeclarantAddress = declarantWithOneNode.MainAddress.PK;
		declaration.JE_CustomsProfile = "BB-XXXX";
		AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_CustomsProfile = "CC-YYYY";
		AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_CustomsProfile = "";
		AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_OA_DeclarantAddress = declarantWithTwoNodes.MainAddress.PK;
		declaration.JE_CustomsProfile = "BB-XXXX";
		AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_CustomsProfile = "CC-YYYY";
		AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_CustomsProfile = "CC-ZZZZ";
		AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

		declaration.JE_CustomsProfile = "";
		AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
	}

	[TestDate(2020, 02, 01)]
	[UseSnapshotProtection]
	public void TestCheckJE_CustomsProfileWarningWhenRemainingNumbersAreLessThanThreshold()
	{
		string taxNumber = "11111111111";
		const string warningMessage = "The available Number Range for range type R, account 11111111111 is less than 3. Create a new Number Range in Companies or expand the existing one";

		var tempFactory = new BusinessObjectFactory();
		var company = tempFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var numberViewStmNum1 = ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, taxNumber, minimumValue: 1, maximumValue: 1000, currentValue: 998);
		tempFactory.Save();

		var query = new ZQuery(ViewStmNumsSchema.SN_Name, numberViewStmNum1.SN_Name);
		query.AddToFilter(ViewStmNumsSchema.SN_Owner, numberViewStmNum1.SN_Owner);
		numberViewStmNum1 = Factory.LoadTop1<CustomsNumberViewStmNums>(query);
		numberViewStmNum1.Provider = company.CustomsNumberProvider;
		numberViewStmNum1.FirstNumberRange.SNR_ThresholdRunOutWarning = 10;
		company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		var declarant1 = Factory.NewWithValidTestData<OrgHeader>();
		declarant1.OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "AAAA")
			.AppendAccountDetail("DEC1-AAAA", "DEC1")
			.Build();

		declaration.JE_MessageType = "IMP";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_CustomsProfile = "DEC1-AAAA";
			AssertHasWarningContaining(declaration.JE_CustomsProfileInfo, warningMessage);
		}

		declaration.JE_CustomsProfile = ZString.Empty;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_CustomsProfile = "DEC1-AAAA";
			AssertNoWarningContaining(declaration.JE_CustomsProfileInfo, warningMessage);
		}

		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, taxNumber, minimumValue: 1, maximumValue: 1000, currentValue: 1);
		Factory.Save();

		declaration.JE_CustomsProfile = "DEC1-AAAA";
		AssertNoWarnings(declaration.JE_CustomsProfileInfo);
	}

	[TestDate(2020, 01, 01)]
	public void TestCheckJE_CustomsProfileCannotRetrievePanNumberRange()
	{
		var messageError = ValidationCaptions.ProgressiveAnnualNumber.CannotRetrievePanNumberRangeForSelectedNode;
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.New<OrgHeader>().OH_Code = "DEC2";
		Factory.New<OrgHeader>().OH_Code = "DEC3";
		Factory.New<OrgHeader>().OH_Code = "DEC4";
		Factory.New<OrgHeader>().OH_Code = "DEC5";
		Factory.Save();

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
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

		AssertForNonUcc6Declaration(messageError);
		AssertForUcc6Declaration(messageError);
	}

	public void TestJE_GS_NKCusAgent()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var staff1 = Factory.NewWithValidTestData<GlbStaff>();
		staff1.GS_Code = "ST1";
		staff1.GS_FullName = "STAFF1 FULL NAME";
		var staff1Wrapper = GlbStaffWrapper.Get(staff1);
		staff1Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		staff1Wrapper.PasswordCollection.AddNew().GP_UserID = "5678";

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("11111111111-002", "5678").AppendAccountDetail("5678-DEC1", "DEC1")
			.Build();

		Factory.Save();

		using (TemporarilySetIsUCC6ForDeclaration(declaration, isUCC6: false))
		{
			CombineAssertions("When IsUCC6 = false", () =>
			{
				declaration.JE_MessageType = "IMP";
				AssertEquals("Pre-Cond: IsUCC6", false, declaration.IsUCC6);

				declaration.JE_CustomsProfile = "9999";
				declaration.JE_GS_NKCusAgent = "";
				AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_GS_NKCusAgent = "XXX";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_CustomsProfile = "1234-DEC1";
				declaration.JE_GS_NKCusAgent = "";
				AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_GS_NKCusAgent = "XXX";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_GS_NKCusAgent = "ST1";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_CustomsProfile = "5678-DEC1";
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		using (TemporarilySetIsUCC6ForDeclaration(declaration, isUCC6: true))
		{
			CombineAssertions("When IsUCC6 = true", () =>
			{
				declaration.JE_MessageType = "IMP";
				AssertEquals("Pre-Cond: IsUCC6", true, declaration.IsUCC6);

				declaration.JE_CustomsProfile = "9999";
				declaration.JE_GS_NKCusAgent = "";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_GS_NKCusAgent = "XXX";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_CustomsProfile = "1234-DEC1";
				declaration.JE_GS_NKCusAgent = "";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_GS_NKCusAgent = "XXX";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_GS_NKCusAgent = "ST1";
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_CustomsProfile = "5678-DEC1";
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, ListValidation.InvalidCodeMessageError);
			});
		}
	}

	public void TestJE_VesselName()
	{
		var expectedMessage = "Field exceeds the maximum allowed length in the declaration message (27 characters).";

		declaration.JE_VesselName = "TRUCK";
		AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, expectedMessage);

		declaration.JE_VesselName = "TRUCK ZZZZZZZZZZZZZZZZZZZZZZZZZZZ";
		AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, expectedMessage);

		var expectedMessageWhenIsSea = "Fields (Vessel + Voyage) exceeds the maximum allowed length in the declaration message (27 characters).";
		declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
		declaration.JE_VoyageFlightNo = "0123456789";
		declaration.JE_VesselName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, expectedMessageWhenIsSea);

		declaration.JE_VoyageFlightNo = "01";
		declaration.JE_VesselName = "ABCDEFGHIJKLMNOPQRS";
		AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, expectedMessageWhenIsSea);

		declaration.JE_VoyageFlightNo = "0123456789";
		AssertHasMessageErrorContaining(declaration.JE_VesselNameInfo, expectedMessageWhenIsSea);

		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		declaration.JE_VoyageFlightNo = "0123456789";
		declaration.JE_VesselName = "";
		AssertNoMessageErrorContaining(declaration.JE_VesselNameInfo, expectedMessage);
	}

	public override void TestCheckJE_OA_DeclarantAddress()
	{
		base.TestCheckJE_OA_DeclarantAddress();

		var declarant = Factory.NewWithValidTestData<OrgHeader>();

		declaration.JE_MessageType = "EXP";
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
		{
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var declarantAddress = declarant.MainAddress;

			var expectedCompanyNameWarningMessage = "Declarant Company Name is longer than 35 characters, it will be truncated in the message.";
			declarant.OH_FullName = "A0001";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoWarningContaining(declaration.JE_OA_DeclarantAddressInfo, expectedCompanyNameWarningMessage);

			declarant.OH_FullName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasWarningContaining(declaration.JE_OA_DeclarantAddressInfo, expectedCompanyNameWarningMessage);

			var expectedAddressWarningMessage = "Declarant Address is longer than 35 characters, it will be truncated in the message.";
			declarantAddress.OA_Address1 = "";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoWarningContaining(declaration.JE_OA_DeclarantAddressInfo, expectedAddressWarningMessage);

			declarantAddress.OA_Address2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasWarningContaining(declaration.JE_OA_DeclarantAddressInfo, expectedAddressWarningMessage);

			var expectedCityWarningMessage = "Declarant City is longer than 35 characters, it will be truncated in the message.";
			declarantAddress.OA_City = "Milan";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoWarningContaining(declaration.JE_OA_DeclarantAddressInfo, expectedCityWarningMessage);

			declarantAddress.OA_City = "Llanfairpwllgwyngyllgogerychwyrndrobwllllantysilio";
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			AssertHasWarningContaining(declaration.JE_OA_DeclarantAddressInfo, expectedCityWarningMessage);

			declaration.JE_MessageType = "IMP";
			declarant.OH_FullName = "".PadRight(75, 'A');
			declaration.Validation.ValidateJE_OA_DeclarantAddress();
			var expectedCompanyNameWarningMessageForImport = "Declarant Company Name is longer than 70 characters, it will be truncated in the message.";
			AssertHasWarningContaining(declaration.JE_OA_DeclarantAddressInfo, expectedCompanyNameWarningMessageForImport);
		}
	}

	public void TestCheckMessageVersion()
	{
		CombineAssertions("Message version visible", () =>
		{
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = "BLT";
			AssertEquals("Pre-Cond: Message version Visible", true, declaration.IsMessageVersionApplicable);
			declaration.MessageVersion = "TXT";
			AssertNoMessageErrorContaining("When Message version visible and valid", declaration.MessageVersionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("When Message version visible and valid", declaration.MessageVersionInfo, ListValidation.InvalidCodeMessageError.ToString());

			declaration.MessageVersion = ZString.Empty;
			AssertHasMessageErrorContaining("When Message version visible and empty", declaration.MessageVersionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.MessageVersion = "ABX";
			AssertNoMessageErrorContaining("When Message version visible and invalid", declaration.MessageVersionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("When Message version visible and invalid", declaration.MessageVersionInfo, ListValidation.InvalidCodeMessageError.ToString());
		});

		CombineAssertions("Message version not visible", () =>
		{
			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = "ITF";
			AssertEquals("Pre-Cond: Message Version not visible", false, declaration.IsMessageVersionApplicable);
			declaration.MessageVersion = "TXT";
			AssertNoMessageErrorContaining("When Message version not visible and valid", declaration.MessageVersionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("When Message version not visible and valid", declaration.MessageVersionInfo, ListValidation.InvalidCodeMessageError.ToString());

			declaration.MessageVersion = ZString.Empty;
			AssertNoMessageErrorContaining("When Message version not visible and empty", declaration.MessageVersionInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.MessageVersion = "ABX";
			AssertNoMessageErrorContaining("When Message version not visible and invalid", declaration.MessageVersionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("When Message version not visible and invalid", declaration.MessageVersionInfo, ListValidation.InvalidCodeMessageError.ToString());
		});
	}

	[TestDate(2020, 01, 01)]
	public void TestCheckHasMauCertificateIfApplicable()
	{
		var expectedMessage = "The Node you selected has no MAU Certificate. Please add it in Company>Brokerage for your company.";
		var declarantWithTwoNodes = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithTwoNodes.OH_Code = "CC";
		Factory.Save();

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("22222222222-001", "YYYY").AppendAccountDetail("CC-YYYY", "CC")
			.AppendAccount("33333333333-001", "ZZZZ").AppendAccountDetail("CC-ZZZZ", "CC")
		.Build();

		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, ZDate.Today.Year, "22222222222", currentValue: 20000);
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, 2019, "33333333333", currentValue: 30000);

		declaration.JE_OA_DeclarantAddress = declarantWithTwoNodes.MainAddress.PK;

		var companyWrapper = GlbCompanyWrapper.Get(company);
		var mauPassword = companyWrapper.PasswordCollection.AddNew();
		mauPassword.GP_UserID = "CC-ZZZZ";
		Factory.Save();

		using (TemporarilySetIsUCC6ForDeclaration(declaration, true))
		{
			CombineAssertions("When IsUCC6 = true", () =>
			{
				AssertEquals("Pre:IsUcc6", true, declaration.IsUCC6);

				declaration.JE_CustomsProfile = ZString.Empty;
				AssertNoMessageErrorContaining("When CustomsProfile empty", declaration.JE_CustomsProfileInfo, expectedMessage);

				declaration.JE_CustomsProfile = "CC-ZZZZ";
				AssertNoMessageErrorContaining("When MAU Certificate added", declaration.JE_CustomsProfileInfo, expectedMessage);

				declaration.JE_CustomsProfile = "CC-YYYY";
				AssertHasMessageErrorContaining("When MAU Certificate not added", declaration.JE_CustomsProfileInfo, expectedMessage);
			});
		}

		using (TemporarilySetIsUCC6ForDeclaration(declaration, false))
		{
			CombineAssertions("When IsUCC6 = false", () =>
			{
				AssertEquals("Pre:IsUcc6", false, declaration.IsUCC6);

				declaration.JE_CustomsProfile = ZString.Empty;
				AssertNoMessageErrorContaining("When CustomsProfile empty", declaration.JE_CustomsProfileInfo, expectedMessage);

				declaration.JE_CustomsProfile = "CC-ZZZZ";
				AssertNoMessageErrorContaining("When MAU Certificate added", declaration.JE_CustomsProfileInfo, expectedMessage);

				declaration.JE_CustomsProfile = "CC-YYYY";
				AssertNoMessageErrorContaining("When MAU Certificate not added", declaration.JE_CustomsProfileInfo, expectedMessage);
			});
		}
	}

	IDisposable TemporarilySetIsUCC6ForDeclaration(JobDeclaration declaration, bool isUCC6) => ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	public void TestValidatePowerOfAttorneyOptOutBase()
	{
		var organization = Factory.New<OrgHeader>();

		const string expectedWarning = "There is no written authority to act against this organization.";

		using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Latvia))
		{
			var latviaDeclaration = Factory.New<EU.Business.Declaration.JobDeclaration>();

			latviaDeclaration.JE_OH_Importer = organization.PK;
			latviaDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			latviaDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertHasWarningContaining("No warning for Import", latviaDeclaration.JE_DeclarantTypeInfo, expectedWarning);
		}

		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_OH_Importer = organization.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertNoWarningContaining("No warning for Import", declaration.JE_DeclarantTypeInfo, expectedWarning);

			declaration.JE_OH_Supplier = organization.PK;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertNoWarningContaining("No warning for not UCC6 Export", declaration.JE_DeclarantTypeInfo, expectedWarning);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertNoWarningContaining("No warning for UCC6 Export", declaration.JE_DeclarantTypeInfo, expectedWarning);
			}
		});
	}

	protected void AssertListValidationAddsWarningNotification(ZPropertyInfo portPtyInfo)
	{
		portPtyInfo.SetValueFromString("X");

		CombineAssertions("Invalid Port", () =>
		{
			AssertHasWarning(portPtyInfo, "This port code is invalid. Please check against the transport mode and shipment type.");
			AssertNoMessageErrorContaining(portPtyInfo, "This port code is invalid. Please check against the transport mode and shipment type.");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	new JobDeclaration declaration;

	void AssertForNonUcc6Declaration(string messageError)
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsProfile = ZString.Empty;
			AssertNoMessageErrorContaining("When Node is not selected, error message should not appear", declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "XXXX";
			AssertHasMessageErrorContaining("An invalid node is selected. PAN can't be retrieved", declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "AAAA-DEC1";
			AssertHasMessageErrorContaining("A valid node is selected, but the number range is not configured. PAN can't be retrieved",
				declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "BBBB-DEC2";
			AssertHasMessageErrorContaining(
				"A valid node is selected and the number range is configured but the maximum value has been reached. PAN can't be retrieved",
				declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "CCCC-DEC3";
			AssertNoMessageErrorContaining("A valid node is selected and the number range is configured. PAN can be retrieved",
				declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "DDDD-DEC4";
			AssertHasMessageErrorContaining("A valid node is selected but it has no number range available for current/last year. PAN can't be retrieved",
				declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "EEEE-DEC5";
			AssertNoMessageErrorContaining(
				"A valid node is selected. It has no number range available for current year, but it has a number range for last year. PAN can be retrieved (the number range will be cloned)",
				declaration.JE_CustomsProfileInfo, messageError);
		}
	}

	void AssertForUcc6Declaration(string messageError)
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_CustomsProfile = "AAAA-DEC1";
			AssertNoMessageErrorContaining("[Ucc6] A valid node is selected, but the number range is not configured. No error", declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_MessageType = "EXP";
			declaration.JE_CustomsProfile = "BBBB-DEC2";
			AssertNoMessageErrorContaining("[Ucc6] A valid node is selected and the number range is configured but the maximum value has been reached. No error", declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "CCCC-DEC3";
			AssertNoMessageErrorContaining("[Ucc6] A valid node is selected and the number range is configured. No error.", declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "DDDD-DEC4";
			AssertNoMessageErrorContaining("[Ucc6] A valid node is selected but it has no number range available for current/last year. No error.", declaration.JE_CustomsProfileInfo, messageError);

			declaration.JE_CustomsProfile = "EEEE-DEC5";
			AssertNoMessageErrorContaining("[Ucc6] A valid node is selected. It has no number range available for current year, but it has a number range for last year. No error.", declaration.JE_CustomsProfileInfo, messageError);
		}
	}
}
