using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LPCOViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSuffixMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var parentLPCO = declaration.LPCOs.AddNew();
			parentLPCO.CLP_Type = "2000";
			var pgaHeader = invoiceLine.ECCCPGAHeader;
			var lpcoView = new LPCOView(parentLPCO, pgaHeader);
			lpcoView.CLP_HolderType = "xxx";
			lpcoView.Validation.ValidateCLP_AuthorizedPartyContactName();
			AssertHasMessageError(lpcoView.CLP_HolderContactNameInfo, "Please fix the errors on this field in the LPCOs grid on Misc. tab, Document Type: 2000");
		}

		public void TestDocumentType()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "8001", "8001 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.ECCC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");

			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "7001", "7001 DESC", startDate, endDate);
			var attributePGA2 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.CNSC);
			var attributeNameValuesAllowed2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed02 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "EEE");

			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			pgaHeader.CA_WRMProgramInd = YesNoList.Codes.Yes;
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var lpcoView1 = pgaHeader.LPCOViews.AddNew();

			lpcoView1.CLP_Type = "0007";
			AssertHasMessageError(lpcoView1.CLP_TypeInfo, "The selected document type is invalid for the program: Waste Reduction And Management Division");

			lpcoView1.CLP_Type = "8001";
			AssertNoMessageErrors(lpcoView1.CLP_TypeInfo);

			var lpcoView2 = pgaHeader.LPCOViews.AddNew();
			lpcoView2.CLP_Type = "8001";
			AssertHasMessageError(lpcoView2.CLP_TypeInfo, "Type: 8001 is duplicated.");

			var pgaHeader2 = invoiceLine.CNSCPGAHeader;
			pgaHeader2.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpcoView3 = pgaHeader2.LPCOViews.AddNew();
			lpcoView3.CLP_Type = "7000";
			var lpcoView4 = pgaHeader2.LPCOViews.AddNew();
			lpcoView4.CLP_Type = "7001";

			AssertHasMessageError(lpcoView4.CLP_TypeInfo, "One of [7000, 7001] should be added for program: All Programs for CNSC.");

			var lpcoView5 = pgaHeader2.LPCOViews.AddNew();
			lpcoView4.CLP_Type = "7000";
			AssertNoMessageErrors(lpcoView5.CLP_TypeInfo);
		}

		public void TestCheckCA_RefNo()
		{
			pHACHeader.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH01;
			pHACHeader.CA_Category = PHACCategories.Codes.PH01;
			pHACLPCOView.Validation.ValidateCLP_RefNo();
			AssertNoMessageError(pHACLPCOView.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			pHACHeader.CA_Category = PHACCategories.Codes.PH02;
			pHACLPCOView.Validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(pHACLPCOView.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			pHACHeader.CA_ExceptPathogenToxin = true;
			pHACLPCOView.Validation.ValidateCLP_RefNo();
			AssertNoMessageError(pHACLPCOView.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			pHACLPCOView.CLP_RefNo = "12345";
			pHACLPCOView.CLP_Type = "2001";
			pHACLPCOView.Validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(pHACLPCOView.CLP_RefNoInfo, "This license number is not required as it is indicated as exempt.");
		}

		public void TestCheckCA_LPCOHolderType_CheckContactExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpcoview = gacPGAHeader.LPCOViews.AddNew();
			lpcoview.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;
			lpcoview.RunPreSaveValidation();
			AssertHasMessageErrorContaining(lpcoview.CLP_HolderContactNameInfo, OrganisationValidation.AuthorizedPartyContactNameIsRequired);
			AssertHasMessageErrorContaining(lpcoview.CLP_HolderContactEmailInfo, OrganisationValidation.AuthorizedPartyContactEmailIsRequired);
			AssertHasMessageErrorContaining(lpcoview.CLP_HolderContactPhoneInfo, OrganisationValidation.AuthorizedPartyContactPhoneNumberIsRequired);
			lpcoview.CLP_IsHolderOverridden = true;
			lpcoview.CLP_HolderContactName = "Jason";
			lpcoview.CLP_HolderContactEmail = "jason.zhu@wisetechglobal.com";
			lpcoview.CLP_HolderContactPhone = "+ 1 888 - 913 - 4581";
			AssertNoMessageErrorContaining(lpcoview.CLP_HolderContactNameInfo, OrganisationValidation.AuthorizedPartyContactNameIsRequired);
			AssertNoMessageErrorContaining(lpcoview.CLP_HolderContactEmailInfo, OrganisationValidation.AuthorizedPartyContactEmailIsRequired);
			AssertNoMessageErrorContaining(lpcoview.CLP_HolderContactPhoneInfo, OrganisationValidation.AuthorizedPartyContactPhoneNumberIsRequired);

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var ecccPGAHeader = invoiceLine.ECCCPGAHeader;
			var ecccLpcoView = ecccPGAHeader.LPCOViews.AddNew();
			ecccLpcoView.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;
			ecccLpcoView.RunPreSaveValidation();
			AssertHasMessageErrorContaining(ecccLpcoView.CLP_HolderContactNameInfo, OrganisationValidation.AuthorizedPartyContactNameIsRequired);
			AssertHasMessageErrorContaining(ecccLpcoView.CLP_HolderContactEmailInfo, OrganisationValidation.AuthorizedPartyContactEmailIsRequired);
			AssertHasMessageErrorContaining(ecccLpcoView.CLP_HolderContactPhoneInfo, OrganisationValidation.AuthorizedPartyContactPhoneNumberIsRequired);
			ecccLpcoView.CLP_IsHolderOverridden = true;
			ecccLpcoView.CLP_HolderContactName = "Jason";
			ecccLpcoView.CLP_HolderContactEmail = "jason.zhu@wisetechglobal.com";
			ecccLpcoView.CLP_HolderContactPhone = "+ 1 888 - 913 - 4581";
			AssertNoMessageErrorContaining(ecccLpcoView.CLP_HolderContactNameInfo, OrganisationValidation.AuthorizedPartyContactNameIsRequired);
			AssertNoMessageErrorContaining(ecccLpcoView.CLP_HolderContactEmailInfo, OrganisationValidation.AuthorizedPartyContactEmailIsRequired);
			AssertNoMessageErrorContaining(ecccLpcoView.CLP_HolderContactPhoneInfo, OrganisationValidation.AuthorizedPartyContactPhoneNumberIsRequired);
		}

		public void TestCheckCA_LPCOApplicant_CheckContactExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpcoview = gacPGAHeader.LPCOViews.AddNew();
			lpcoview.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;
			lpcoview.RunPreSaveValidation();
			AssertHasMessageErrorContaining(lpcoview.CLP_ApplicantContactNameInfo, OrganisationValidation.ApplicantContactNameIsRequired);
			AssertHasMessageErrorContaining(lpcoview.CLP_ApplicantContactEmailInfo, OrganisationValidation.ApplicantContactEmailIsRequired);
			AssertHasMessageErrorContaining(lpcoview.CLP_ApplicantContactPhoneInfo, OrganisationValidation.ApplicantContactPhoneNumberIsRequired);
			lpcoview.CLP_IsApplicantOverridden = true;
			lpcoview.CLP_ApplicantContactName = "Jason";
			lpcoview.CLP_ApplicantContactEmail = "jason.zhu@wisetechglobal.com";
			lpcoview.CLP_ApplicantContactPhone = "+ 1 888 - 913 - 4581";
			AssertNoMessageErrorContaining(lpcoview.CLP_ApplicantContactNameInfo, OrganisationValidation.ApplicantContactNameIsRequired);
			AssertNoMessageErrorContaining(lpcoview.CLP_ApplicantContactEmailInfo, OrganisationValidation.ApplicantContactEmailIsRequired);
			AssertNoMessageErrorContaining(lpcoview.CLP_ApplicantContactPhoneInfo, OrganisationValidation.ApplicantContactPhoneNumberIsRequired);

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var ecccPGAHeader = invoiceLine.ECCCPGAHeader;
			var ecccLpcoView = ecccPGAHeader.LPCOViews.AddNew();
			ecccLpcoView.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;
			ecccLpcoView.RunPreSaveValidation();
			AssertHasMessageErrorContaining(ecccLpcoView.CLP_ApplicantContactNameInfo, OrganisationValidation.ApplicantContactNameIsRequired);
			AssertHasMessageErrorContaining(ecccLpcoView.CLP_ApplicantContactEmailInfo, OrganisationValidation.ApplicantContactEmailIsRequired);
			AssertHasMessageErrorContaining(ecccLpcoView.CLP_ApplicantContactPhoneInfo, OrganisationValidation.ApplicantContactPhoneNumberIsRequired);
			ecccLpcoView.CLP_IsApplicantOverridden = true;
			ecccLpcoView.CLP_ApplicantContactName = "Jason";
			ecccLpcoView.CLP_ApplicantContactEmail = "jason.zhu@wisetechglobal.com";
			ecccLpcoView.CLP_ApplicantContactPhone = "+ 1 888 - 913 - 4581";
			AssertNoMessageErrorContaining(ecccLpcoView.CLP_ApplicantContactNameInfo, OrganisationValidation.ApplicantContactNameIsRequired);
			AssertNoMessageErrorContaining(ecccLpcoView.CLP_ApplicantContactEmailInfo, OrganisationValidation.ApplicantContactEmailIsRequired);
			AssertNoMessageErrorContaining(ecccLpcoView.CLP_ApplicantContactPhoneInfo, OrganisationValidation.ApplicantContactPhoneNumberIsRequired);
		}

		public void TestCA_TypeValidationForHCPGA()
		{
			pHACLPCOView.CLP_Type = "2001";
			pHACHeader.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH01;
			pHACHeader.CA_Category = PHACCategories.Codes.PH01;
			pHACLPCOView.Validation.ValidateCLP_StartDate();
			AssertNoMessageError(pHACLPCOView.CLP_StartDateInfo, MandatoryValidation.YouHaveNotEntered);
			pHACHeader.CA_Category = PHACCategories.Codes.PH02;
			pHACLPCOView.Validation.ValidateCLP_StartDate();
			AssertHasMessageErrorContaining(pHACLPCOView.CLP_StartDateInfo, MandatoryValidation.YouHaveNotEntered);

			AssertEquals("Show Validation", true, pHACHeader.NeedsDocumentTypeValidation());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var hcPGAHeader = invoiceLine.HCPGAHeader;
			hcPGAHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
			var hcLPCOView = hcPGAHeader.LPCOViews.AddNew();
			hcLPCOView.CLP_Type = "2001";
			hcPGAHeader.CA_IntendedUseCodeAPI = HCIntendedUseCode.Codes.HC13;
			hcPGAHeader.CA_CategoryAPI = HCCategories.Codes.HC01;
			hcLPCOView.Validation.ValidateCLP_EndDate();
			AssertEquals("Show Validation", false, hcPGAHeader.NeedsDocumentTypeValidation());
			AssertNoMessageError(hcLPCOView.CLP_EndDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_LPCOEndDate()
		{
			pHACLPCOView.CLP_Type = "2001";
			pHACHeader.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH01;
			pHACHeader.CA_Category = PHACCategories.Codes.PH01;
			pHACLPCOView.Validation.ValidateCLP_EndDate();
			AssertNoMessageError(pHACLPCOView.CLP_EndDateInfo, MandatoryValidation.YouHaveNotEntered);

			pHACHeader.CA_Category = PHACCategories.Codes.PH02;
			pHACLPCOView.Validation.ValidateCLP_EndDate();
			AssertHasMessageErrorContaining(pHACLPCOView.CLP_EndDateInfo, MandatoryValidation.YouHaveNotEntered);

			pHACHeader.CA_ExceptPathogenToxin = true;
			pHACLPCOView.Validation.ValidateCLP_EndDate();
			AssertNoMessageError(pHACLPCOView.CLP_EndDateInfo, MandatoryValidation.YouHaveNotEntered);

			pHACLPCOView.CLP_RefNo = "123";
			pHACLPCOView.CLP_Type = "2001";
			pHACLPCOView.Validation.ValidateCLP_EndDate();
			AssertHasMessageErrorContaining(pHACLPCOView.CLP_EndDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_LPCOStartDate()
		{
			pHACLPCOView.CLP_Type = "2001";
			pHACHeader.CA_IntendedUseCode = PHACEndUseCodes.Codes.PH01;
			pHACHeader.CA_Category = PHACCategories.Codes.PH01;
			pHACLPCOView.Validation.ValidateCLP_StartDate();
			AssertNoMessageError(pHACLPCOView.CLP_StartDateInfo, MandatoryValidation.YouHaveNotEntered);

			pHACHeader.CA_Category = PHACCategories.Codes.PH02;
			pHACLPCOView.Validation.ValidateCLP_StartDate();
			AssertHasMessageErrorContaining(pHACLPCOView.CLP_StartDateInfo, MandatoryValidation.YouHaveNotEntered);

			pHACHeader.CA_ExceptPathogenToxin = true;
			pHACLPCOView.Validation.ValidateCLP_StartDate();
			AssertNoMessageError(pHACLPCOView.CLP_StartDateInfo, MandatoryValidation.YouHaveNotEntered);

			pHACLPCOView.CLP_RefNo = "123";
			pHACLPCOView.Validation.ValidateCLP_StartDate();
			AssertHasMessageErrorContaining(pHACLPCOView.CLP_StartDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			invoiceLine.CA_PHACInd = YesNoList.Codes.Yes;

			this.cNSCHeader = invoiceLine.CNSCPGAHeader;
			cNSCHeader.LPCOViews.AddNew();
			this.gACCHeader = invoiceLine.GACPGAHeader;
			gACCHeader.LPCOViews.AddNew();
			this.pHACHeader = invoiceLine.PHACPGAHeader;
			this.pHACLPCOView = pHACHeader.LPCOViews.AddNew();
		}
		CNSCPGAHeader cNSCHeader;
		GACPGAHeader gACCHeader;
		LPCOView pHACLPCOView;
		PHACPGAHeader pHACHeader;
		#endregion

	}
}
