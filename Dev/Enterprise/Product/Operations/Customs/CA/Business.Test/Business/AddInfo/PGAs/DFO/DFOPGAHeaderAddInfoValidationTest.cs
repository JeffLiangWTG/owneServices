using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DFOPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_OA_HarvestingParty()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;

			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			pgaHeader.CA_OA_HarvestingParty = address.PK;

			AssertHasMessageError(pgaHeader.CA_OA_HarvestingPartyInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = org.Contacts.AddNew();
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";

			pgaHeader.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertNoMessageError(pgaHeader.CA_OA_HarvestingPartyInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(pgaHeader.CA_OA_HarvestingPartyInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "15850503354";
			pgaHeader.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertNoMessageError(pgaHeader.CA_OA_HarvestingPartyInfo, OrganisationValidation.ContactPhoneNumberIsRequired);
		}

		public void TestCheckCA_GeneticModificationDescription()
		{
			AssertMandatoryValidationOnABIProgram(AutoDFOPGAHeaderAddInfo.Schema.CA_GeneticModificationDescription);
		}

		public void TestCheckCA_GenusOrSpecies()
		{
			AssertMandatoryValidationOnABIProgram(AutoDFOPGAHeaderAddInfo.Schema.CA_GenusOrSpecies);
			AssertMandatoryValidationOnAISProgram(AutoDFOPGAHeaderAddInfo.Schema.CA_GenusOrSpecies);
			AssertMandatoryValidationOnTTPProgram(AutoDFOPGAHeaderAddInfo.Schema.CA_GenusOrSpecies);
		}

		public void TestCheckCA_SpeciesCode()
		{
			var pgaHeader = CreateNewHeader();
			ValidationTestHelper.AssertInvalidCodeMessageError(pgaHeader.CA_SpeciesCodeInfo, "XXXX", DFOScientificNames.Codes.FO11);
		}

		public void TestCheckCA_Commission()
		{
			var pgaHeader = CreateNewHeader();
			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(pgaHeader.CA_CommissionInfo, "XXXX", DFOCommissionList.Codes.FO13);

			AssertMandatoryValidationOnTTPProgram(AutoDFOPGAHeaderAddInfo.Schema.CA_Commission);
		}

		public void TestCheckCA_CommonNameCode()
		{
			var pgaHeader = CreateNewHeader();
			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(pgaHeader.CA_CommonNameCodeInfo, "XXXX", DFOCommonNameCodes.Codes.FO21);
		}

		public void TestCA_Category()
		{
			var pgaHeader = CreateNewHeader();

			pgaHeader.CA_ABIProgramInd = "";
			pgaHeader.CA_HasGeneticModification = false;
			pgaHeader.CA_TTPProgramInd = "Y";
			pgaHeader.AddInfoValidation.ValidateCA_Category();
			AssertNoMessageErrorContaining(pgaHeader.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);

			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_HasGeneticModification = true;
			pgaHeader.CA_Category = "";
			AssertHasMessageErrorContaining(pgaHeader.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);
			pgaHeader.CA_Category = "XXX";
			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeMessageError(pgaHeader.CA_CategoryInfo, "XXX", DFOProductCategories.Codes.FO01);

			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.No;
			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.Yes;

			pgaHeader.AddInfoValidation.ValidateCA_Category();
			AssertNoMessageError(header.CA_CategoryInfo, MandatoryValidation.YouHaveNotEntered);
			pgaHeader.CA_Category = "XX";
			pgaHeader.AddInfoValidation.ValidateCA_Category();
			AssertNoMessageError(header.CA_CategoryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCA_Direction()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SpeciesCode = DFOScientificNames.Codes.FO11;

			Customs.Business.Testing.ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(pgaHeader.CA_DirectionInfo, "XXX", DFOPGADirections.Codes.FO01);
		}

		public void TestLifeState_ABI()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_HasGeneticModification = true;

			pgaHeader.CA_LifeStagePropagate = false;
			pgaHeader.CA_LifeStageEmbryo = false;
			pgaHeader.CA_LifeStageJuvenile = false;
			pgaHeader.CA_LifeStageAdult = false;
			pgaHeader.CA_LifeStageLive = false;
			pgaHeader.CA_LifeStageDead = false;

			pgaHeader.AddInfoValidation.ValidateAll();

			var expectedErrorMessage = "If the commodity being imported has been genetically modified or genetically engineered, the life stage must be provided.";

			AssertHasMessageError(pgaHeader.CA_LifeStagePropagateInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageEmbryoInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageJuvenileInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageAdultInfo, expectedErrorMessage);

			AssertNoMessageError(pgaHeader.CA_LifeStageDeadInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_LifeStageLiveInfo, expectedErrorMessage);
		}

		public void TestLifeState_AIS()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SpeciesCode = DFOScientificNames.Codes.FO11;

			pgaHeader.CA_LifeStagePropagate = false;
			pgaHeader.CA_LifeStageEmbryo = false;
			pgaHeader.CA_LifeStageJuvenile = false;
			pgaHeader.CA_LifeStageAdult = false;
			pgaHeader.CA_LifeStageLive = false;
			pgaHeader.CA_LifeStageDead = false;

			pgaHeader.AddInfoValidation.ValidateAll();

			var expectedErrorMessage = "The life stage must be provided in this field if the commodity being imported is non-eviscerated Asian Carp, Zebra or Quagga Mussels as prohibited under the AIS Regulations.";

			AssertHasMessageError(pgaHeader.CA_LifeStagePropagateInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageEmbryoInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageJuvenileInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageAdultInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageDeadInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageLiveInfo, expectedErrorMessage);
		}

		public void TestLifeState_TTP()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.Yes;

			pgaHeader.CA_LifeStagePropagate = false;
			pgaHeader.CA_LifeStageEmbryo = false;
			pgaHeader.CA_LifeStageJuvenile = false;
			pgaHeader.CA_LifeStageAdult = false;
			pgaHeader.CA_LifeStageLive = false;
			pgaHeader.CA_LifeStageDead = false;

			pgaHeader.AddInfoValidation.ValidateAll();

			var expectedErrorMessage = "The life stage of the commodity being imported must be provided for each commodity line with one of the life stage codes.";

			AssertHasMessageError(pgaHeader.CA_LifeStagePropagateInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageEmbryoInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageJuvenileInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageAdultInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_LifeStageDeadInfo, expectedErrorMessage);

			AssertNoMessageError(pgaHeader.CA_LifeStageLiveInfo, expectedErrorMessage);
		}

		public void TestSex_ABI()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_HasGeneticModification = true;

			pgaHeader.CA_SexMale = false;
			pgaHeader.CA_SexFemale = false;
			pgaHeader.CA_SexOther = false;
			pgaHeader.CA_SexSterile = false;
			pgaHeader.CA_SexHermaphrodite = false;
			pgaHeader.CA_SexUnknown = false;

			pgaHeader.AddInfoValidation.ValidateAll();

			var expectedErrorMessage = "If the commodity being imported has been genetically modified or genetically engineered, the sex of the commodity must be provided.";

			AssertHasMessageError(pgaHeader.CA_SexFemaleInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexMaleInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexOtherInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexSterileInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexHermaphroditeInfo, expectedErrorMessage);

			AssertNoMessageError(pgaHeader.CA_SexUnknownInfo, expectedErrorMessage);
		}

		public void TestSex_AIS()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SpeciesCode = DFOScientificNames.Codes.FO11;

			pgaHeader.CA_SexMale = false;
			pgaHeader.CA_SexFemale = false;
			pgaHeader.CA_SexOther = false;
			pgaHeader.CA_SexSterile = false;
			pgaHeader.CA_SexHermaphrodite = false;
			pgaHeader.CA_SexUnknown = false;

			pgaHeader.AddInfoValidation.ValidateAll();

			var expectedErrorMessage = "The sex of the commodity must be provided in this field if the commodity being imported is non-eviscerated Asian Carp, Zebra or Quagga Mussels as prohibited under the AIS Regulations.";

			AssertHasMessageError(pgaHeader.CA_SexFemaleInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexMaleInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexOtherInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexSterileInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_SexHermaphroditeInfo, expectedErrorMessage);

			AssertNoMessageError(pgaHeader.CA_SexUnknownInfo, expectedErrorMessage);
		}

		public void TestIntendedUseCode_ABI()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_HasGeneticModification = true;

			pgaHeader.CA_IUF = false;
			pgaHeader.CA_IUA = false;
			pgaHeader.CA_IURAD = false;
			pgaHeader.CA_IUSCP = false;
			pgaHeader.CA_IUEA = false;
			pgaHeader.CA_IUO = false;
			pgaHeader.CA_IUE = false;
			pgaHeader.CA_IUOTH = false;
			pgaHeader.CA_IUS = false;
			pgaHeader.CA_IUAIS = false;

			pgaHeader.AddInfoValidation.ValidateAll();

			var expectedErrorMessage = "If the commodity being imported has been genetically modified or genetically engineered, the coded description of how a product will be used within Canada must be provided.";

			AssertHasMessageError(pgaHeader.CA_IUFInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUAInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IURADInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUSCPInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUEAInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUOInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUEInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUOTHInfo, expectedErrorMessage);

			AssertNoMessageError(pgaHeader.CA_IUSInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_IUAISInfo, expectedErrorMessage);
		}

		public void TestIntendedUseCode_AIS()
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_SpeciesCode = DFOScientificNames.Codes.FO11;

			pgaHeader.CA_IUF = false;
			pgaHeader.CA_IUA = false;
			pgaHeader.CA_IURAD = false;
			pgaHeader.CA_IUSCP = false;
			pgaHeader.CA_IUEA = false;
			pgaHeader.CA_IUO = false;
			pgaHeader.CA_IUE = false;
			pgaHeader.CA_IUOTH = false;
			pgaHeader.CA_IUS = false;
			pgaHeader.CA_IUAIS = false;

			pgaHeader.AddInfoValidation.ValidateAll();

			var expectedErrorMessage = "If the declaration includes goods prohibited for importation under the Aquatic Invasive Species Regulations, then one of the Intended End Use codes must be provided indicating the purpose of the importation.";

			AssertNoMessageError(pgaHeader.CA_IUFInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_IUAInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_IURADInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_IUSCPInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_IUEAInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_IUOInfo, expectedErrorMessage);
			AssertNoMessageError(pgaHeader.CA_IUOTHInfo, expectedErrorMessage);

			AssertHasMessageError(pgaHeader.CA_IUEInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUSInfo, expectedErrorMessage);
			AssertHasMessageError(pgaHeader.CA_IUAISInfo, expectedErrorMessage);
		}

		public void TestCheckCA_OA_HarvestingParty_ValidateMandatory()
		{
			CreateDataForMessageSending();

			header.CA_OA_HarvestingParty = orgAddress.PK;

			AssertNoMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a name");
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertNoMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.Header.OH_FullName = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertNoMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertHasMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a name");

			AssertNoMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered an address line 1");
			orgAddress.OA_Address1 = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertHasMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered an address line 1");

			AssertNoMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a city");
			orgAddress.OA_City = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertHasMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a city");

			AssertNoMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a country");
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_HarvestingParty();
			AssertHasMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "You have not entered a country");
			AssertHasMessageErrorContaining(header.CA_OA_HarvestingPartyInfo, "Harvesting Party");
		}

		public void TestCheckCA_OA_Processor_ValidateMandatory()
		{
			CreateDataForMessageSending();

			header.CA_OA_Processor = orgAddress.PK;

			AssertNoMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a name");
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_Processor();
			AssertNoMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.Header.OH_FullName = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_Processor();
			AssertNoMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_Processor();
			AssertHasMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a name");

			AssertNoMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered an address line 1");
			orgAddress.OA_Address1 = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_Processor();
			AssertHasMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered an address line 1");

			AssertNoMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a city");
			orgAddress.OA_City = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_Processor();
			AssertHasMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a city");

			AssertNoMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a country");
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_Processor();
			AssertHasMessageErrorContaining(header.CA_OA_ProcessorInfo, "You have not entered a country");
			AssertHasMessageErrorContaining(header.CA_OA_ProcessorInfo, "Processor");
		}

		void CreateDataForMessageSending()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "ORGABC";
			orgAddress = orgHeader.MainAddress;
			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.OA_Address1 = "Address 1";
			orgAddress.OA_Address2 = "Address 2";
			orgAddress.OA_City = "HONGKONG";
			orgAddress.OA_RN_NKCountryCode = "HK";
		}
		OrgAddress orgAddress;

		void AssertMandatoryValidationOnABIProgram(string propertyName)
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_ABIProgramInd = YesNoList.Codes.Yes;
			pgaHeader.CA_HasGeneticModification = true;

			var propertyInfo = pgaHeader.FindPropertyInfo(propertyName);
			AssertNotNull(propertyInfo);

			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);
		}

		void AssertMandatoryValidationOnAISProgram(string propertyName)
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_AISProgramInd = YesNoList.Codes.Yes;

			var propertyInfo = pgaHeader.FindPropertyInfo(propertyName);
			AssertNotNull(propertyInfo);

			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);
		}

		void AssertMandatoryValidationOnTTPProgram(string propertyName)
		{
			var pgaHeader = CreateNewHeader();
			pgaHeader.CA_TTPProgramInd = YesNoList.Codes.Yes;

			var propertyInfo = pgaHeader.FindPropertyInfo(propertyName);
			AssertNotNull(propertyInfo);

			Customs.Business.Testing.ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);
		}

		DFOPGAHeader CreateNewHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invocieHeader = declaration.Invoices.AddNew();

			var invoiceLine = (JobComInvoiceLine)invocieHeader.InvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			invoiceLine.CA_TradeName = string.Empty;

			return invoiceLine.DFOPGAHeader;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = YesNoList.Codes.Yes;
			header = invoiceLine.DFOPGAHeader;
		}
		DFOPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
