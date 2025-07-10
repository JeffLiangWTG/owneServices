using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ECCCPGAHeaderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCA_AOSConformity()
		{
			PrepareRefData();

			var org01 = Factory.New<OrgHeader>();
			org01.OH_Code = "TESTORG01";
			var address01 = org01.MainAddress;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_AOSConformity();
			AssertHasMessageErrorContaining(eccc.CA_AOSConformityInfo, NotEntered);

			eccc.CA_AOSConformity = "ME01";
			validation.ValidateCA_AOSConformity();
			AssertNoMessageErrorContaining(eccc.CA_AOSConformityInfo, NotEntered);

			eccc.CA_AOSConformity = "ME02";
			validation.ValidateCA_AOSConformity();
			AssertNoMessageErrorContaining(eccc.CA_AOSConformityInfo, NotEntered);

			eccc.CA_AOSConformity = "ME05";
			validation.ValidateCA_AOSConformity();
			AssertHasMessageErrorContaining(eccc.CA_AOSConformityInfo, NotInList);

			eccc.CA_AOSConformity = "ME03";
			validation.ValidateCA_AOSConformity();
			AssertNoMessageErrorContaining(eccc.CA_AOSConformityInfo, NotInList);
			AssertHasMessageErrorContaining(eccc.CA_AOSConformityInfo, "LPCO Document Types 8030 and 8031 are required when Affirmation of Statement Conformity = ME03");
			var lpco1 = eccc.LPCOViews.AddNew();
			lpco1.CLP_Type = LPCODocumentTypeQualifier.Codes._8030;
			validation.ValidateCA_AOSConformity();
			AssertHasMessageErrorContaining(eccc.CA_AOSConformityInfo, "LPCO Document Types 8030 and 8031 are required when Affirmation of Statement Conformity = ME03");
			var lpco2 = eccc.LPCOViews.AddNew();
			lpco2.CLP_Type = LPCODocumentTypeQualifier.Codes._8031;
			validation.ValidateCA_AOSConformity();
			AssertNoMessageErrorContaining(eccc.CA_AOSConformityInfo, "LPCO Document Types 8030 and 8031 are required when Affirmation of Statement Conformity = ME03");

			eccc.CA_AOSConformity = "ME04";
			declaration.JE_OH_Importer = org01.PK;
			Factory.Save();

			validation.ValidateCA_AOSConformity();
			AssertHasMessageErrorContaining(eccc.CA_AOSConformityInfo, "Declaration's Importer should contain a ECC registration number.");

			declaration.DocAddresses.FindOrCreateWithDocAddressType(address01.PK, MasterFiles.Integration.DocAddressType.ImporterOfRecord);
			Factory.Save();

			validation.ValidateCA_AOSConformity();
			AssertHasMessageErrorContaining(eccc.CA_AOSConformityInfo, "Declaration's Importer of Record should contain a ECC registration number.");

			org01.CustomsCodes.AddNew(MasterFiles.Business.OrgCusCode.CACodeTypes.ECCCAuthorizationNumber, "ECC01", Core.Constants.CountryCodes.Canada);
			Factory.Save();

			validation.ValidateCA_AOSConformity();
			AssertNoMessageErrorContaining(eccc.CA_AOSConformityInfo, "Declaration's Importer should contain a ECC registration number.");
			AssertNoMessageErrorContaining(eccc.CA_AOSConformityInfo, "Declaration's Importer of Record should contain a ECC registration number.");
		}

		public void TestCheckCA_AOSReplacement()
		{
			PrepareRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_AOSReplacement();
			AssertNoMessageErrorContaining(eccc.CA_AOSReplacementInfo, NotEntered);
			AssertNoMessageErrorContaining(eccc.CA_AOSReplacementInfo, NotInList);

			eccc.CA_ReplacementEngines = true;
			validation.ValidateCA_AOSReplacement();
			AssertHasMessageErrorContaining(eccc.CA_AOSReplacementInfo, NotEntered);

			eccc.CA_AOSReplacement = "ME05";
			validation.ValidateCA_AOSReplacement();
			AssertNoMessageErrorContaining(eccc.CA_AOSReplacementInfo, NotEntered);

			eccc.CA_AOSReplacement = "ME04";
			validation.ValidateCA_AOSReplacement();
			AssertHasMessageErrorContaining(eccc.CA_AOSReplacementInfo, NotInList);

			eccc.CA_AOSReplacement = "ME06";
			validation.ValidateCA_AOSReplacement();
			AssertNoMessageErrorContaining(eccc.CA_AOSReplacementInfo, NotInList);

			eccc.CA_AOSConformity = "ME01";
			validation.ValidateCA_AOSReplacement();
			AssertNoMessageErrorContaining(eccc.CA_AOSReplacementInfo, InvalidAOSReplacementCode);

			eccc.CA_AOSConformity = "ME02";
			validation.ValidateCA_AOSReplacement();
			AssertHasMessageErrorContaining(eccc.CA_AOSReplacementInfo, InvalidAOSReplacementCode);
		}

		public void TestCheckCA_AOSEvidence()
		{
			PrepareRefData();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_AOSEvidence();
			AssertNoMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotEntered);
			AssertNoMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotInList);

			eccc.CA_AOSConformity = "ME02";
			validation.ValidateCA_AOSEvidence();
			AssertHasMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotEntered);

			eccc.CA_AOSEvidence = "ME07";
			validation.ValidateCA_AOSEvidence();
			AssertNoMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotEntered);

			eccc.CA_AOSEvidence = "ME05";
			validation.ValidateCA_AOSEvidence();
			AssertNoMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotEntered);
			AssertHasMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotInList);

			eccc.CA_AOSConformity = ZString.Empty;
			eccc.CA_ReplacementEngines = true;
			eccc.CA_AOSReplacement = "ME06";
			eccc.CA_AOSEvidence = ZString.Empty;
			validation.ValidateCA_AOSEvidence();
			AssertNoMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotEntered);

			eccc.CA_AOSEvidence = "ME08";
			validation.ValidateCA_AOSEvidence();
			AssertNoMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotEntered);
			AssertNoMessageErrorContaining(eccc.CA_AOSEvidenceInfo, NotInList);
		}

		public void TestCheckCA_AOSRetention()
		{
			PrepareRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_AOSRetention();
			AssertNoMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotEntered);
			AssertNoMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotInList);

			eccc.CA_AOSConformity = "ME02";
			validation.ValidateCA_AOSRetention();
			AssertHasMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotEntered);

			eccc.CA_AOSRetention = "ME09";
			validation.ValidateCA_AOSRetention();
			AssertNoMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotEntered);

			eccc.CA_AOSRetention = "ME05";
			validation.ValidateCA_AOSRetention();
			AssertNoMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotEntered);
			AssertHasMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotInList);

			eccc.CA_AOSConformity = ZString.Empty;
			eccc.CA_ReplacementEngines = true;
			eccc.CA_AOSReplacement = "ME06";
			eccc.CA_AOSRetention = ZString.Empty;
			validation.ValidateCA_AOSRetention();
			AssertNoMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotEntered);

			eccc.CA_AOSRetention = "ME10";
			validation.ValidateCA_AOSRetention();
			AssertNoMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotEntered);
			AssertNoMessageErrorContaining(eccc.CA_AOSRetentionInfo, NotInList);
		}

		public void TestCheckCA_AlternativeStandardOfEngineClass()
		{
			PrepareRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_AlternativeStandardOfEngineClass();
			AssertNoMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotEntered);
			AssertNoMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotInList);

			eccc.CA_EngineClass = "EC15";
			validation.ValidateCA_AlternativeStandardOfEngineClass();
			AssertHasMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotEntered);

			eccc.CA_EngineClass = "EC16";
			validation.ValidateCA_AlternativeStandardOfEngineClass();
			AssertHasMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotEntered);

			eccc.CA_EngineClass = "EC0A";
			validation.ValidateCA_AlternativeStandardOfEngineClass();
			AssertHasMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotEntered);

			eccc.CA_EngineClass = "EC0B";
			validation.ValidateCA_AlternativeStandardOfEngineClass();
			AssertHasMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotEntered);

			eccc.CA_AlternativeStandardOfEngineClass = "EC01";
			validation.ValidateCA_AlternativeStandardOfEngineClass();
			AssertNoMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotEntered);
			AssertHasMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotInList);

			eccc.CA_AlternativeStandardOfEngineClass = "EC0E";
			validation.ValidateCA_AlternativeStandardOfEngineClass();
			AssertNoMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotEntered);
			AssertNoMessageErrorContaining(eccc.CA_AlternativeStandardOfEngineClassInfo, NotInList);
		}

		public void TestCheckCA_EvaporativeFamily()
		{
			PrepareRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_EvaporativeFamily();
			AssertNoMessageErrorContaining(eccc.CA_EvaporativeFamilyInfo, NotEntered);

			eccc.CA_EngineClass = "EC0C";
			validation.ValidateCA_EvaporativeFamily();
			AssertHasMessageErrorContaining(eccc.CA_EvaporativeFamilyInfo, NotEntered);

			eccc.CA_EngineClass = "EC0D";
			validation.ValidateCA_EvaporativeFamily();
			AssertHasMessageErrorContaining(eccc.CA_EvaporativeFamilyInfo, NotEntered);

			eccc.CA_EvaporativeFamily = "EVA";
			AssertNoMessageErrorContaining(eccc.CA_EvaporativeFamilyInfo, NotEntered);
		}

		public void TestCheckCA_OA_EngineLocation()
		{
			PrepareRefData();

			var org01 = Factory.New<OrgHeader>();
			org01.OH_Code = "TESTORG01";
			var address01 = org01.MainAddress;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_OA_EngineLocation();
			AssertNoMessageErrorContaining(eccc.CA_OA_EngineLocationInfo, NotEntered);

			eccc.CA_AOSConformity = "ME04";
			validation.ValidateCA_OA_EngineLocation();
			AssertHasMessageErrorContaining(eccc.CA_OA_EngineLocationInfo, NotEntered);

			eccc.CA_AOSConformity = ZString.Empty;
			eccc.CA_AOSReplacement = "ME06";
			validation.ValidateCA_OA_EngineLocation();
			AssertHasMessageErrorContaining(eccc.CA_OA_EngineLocationInfo, NotEntered);

			eccc.CA_OA_EngineLocation = address01.PK;
			validation.ValidateCA_OA_EngineLocation();
			AssertNoMessageErrorContaining(eccc.CA_OA_EngineLocationInfo, NotEntered);
		}

		public void TestCheckCA_OA_EvidenceOfConformityLocation()
		{
			PrepareRefData();

			var org01 = Factory.New<OrgHeader>();
			org01.OH_Code = "TESTORG01";
			var address01 = org01.MainAddress;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			var eccc = invoiceLine.ECCCPGAHeader;
			eccc.CA_VEEProgramInd = YesNoList.Codes.Yes;
			eccc.CA_ProcessCode = ProcessCodes.Codes.XE02;
			var validation = eccc.AddInfoValidation;
			Factory.Save();

			validation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertHasMessageErrorContaining(eccc.CA_OA_EvidenceOfConformityLocationInfo, NotEntered);

			eccc.CA_OA_EvidenceOfConformityLocation = address01.PK;
			validation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertNoMessageErrorContaining(eccc.CA_OA_EvidenceOfConformityLocationInfo, NotEntered);
		}

		public void TestCheckCA_BulkReporting()
		{
			header.CA_NationalMark = false;
			header.CA_BulkReporting = false;

			var messageError = "Only ONE of National Emissions Mark, Bulk Reporting Approval or Non-Commercial Import can be ticked";
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_NationalMark = true;
			header.CA_BulkReporting = true;
			AssertHasMessageError(header.CA_BulkReportingInfo, messageError);
			header.CA_BulkReporting = false;
			header.CA_NonCommercialImport = true;
			AssertNoMessageError(header.CA_BulkReportingInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_BulkReportingInfo, messageError);
			header.CA_NationalMark = false;
			header.CA_BulkReporting = true;
			header.CA_NonCommercialImport = true;
			AssertHasMessageError(header.CA_BulkReportingInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_BulkReportingInfo, messageError);
			header.CA_BulkReporting = false;
			header.CA_NonCommercialImport = true;
			AssertNoMessageError(header.CA_BulkReportingInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_BulkReportingInfo, messageError);

			header.CA_VEEProgramInd = YesNoList.Codes.No;
			header.CA_NationalMark = true;
			header.CA_BulkReporting = true;
			AssertNoMessageError(header.CA_BulkReportingInfo, messageError);
		}

		public void TestCheckCA_NationalMark()
		{
			var messageError = "Only ONE of National Emissions Mark, Bulk Reporting Approval or Non-Commercial Import can be ticked";
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_NationalMark = true;
			header.CA_BulkReporting = true;
			AssertHasMessageError(header.CA_NationalMarkInfo, messageError);
			header.CA_BulkReporting = false;
			header.CA_NonCommercialImport = true;
			AssertHasMessageError(header.CA_NationalMarkInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_NationalMarkInfo, messageError);
			header.CA_NationalMark = false;
			header.CA_BulkReporting = true;
			header.CA_NonCommercialImport = true;
			AssertNoMessageError(header.CA_NationalMarkInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_NationalMarkInfo, messageError);
			header.CA_BulkReporting = false;
			header.CA_NonCommercialImport = true;
			AssertNoMessageError(header.CA_NationalMarkInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_NationalMarkInfo, messageError);

			header.CA_VEEProgramInd = YesNoList.Codes.No;
			header.CA_NonCommercialImport = true;
			header.CA_BulkReporting = true;
			AssertNoMessageError(header.CA_NationalMarkInfo, messageError);
		}

		public void TestCheckCA_NonCommercialImport()
		{
			var messageError = "Only ONE of National Emissions Mark, Bulk Reporting Approval or Non-Commercial Import can be ticked";
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_NationalMark = true;
			header.CA_BulkReporting = true;
			AssertNoMessageError(header.CA_NonCommercialImportInfo, messageError);
			header.CA_BulkReporting = false;
			header.CA_NonCommercialImport = true;
			AssertHasMessageError(header.CA_NonCommercialImportInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_NonCommercialImportInfo, messageError);
			header.CA_NationalMark = false;
			header.CA_BulkReporting = true;
			header.CA_NonCommercialImport = true;
			AssertHasMessageError(header.CA_NonCommercialImportInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_NonCommercialImportInfo, messageError);
			header.CA_BulkReporting = false;
			header.CA_NonCommercialImport = true;
			AssertNoMessageError(header.CA_NonCommercialImportInfo, messageError);
			header.CA_NonCommercialImport = false;
			AssertNoMessageError(header.CA_NonCommercialImportInfo, messageError);

			header.CA_VEEProgramInd = YesNoList.Codes.No;
			header.CA_NonCommercialImport = true;
			header.CA_BulkReporting = true;
			AssertNoMessageError(header.CA_NonCommercialImportInfo, messageError);
		}

		public void TestCheckCA_EngineFamilyName()
		{
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_ProcessCode = ProcessCodes.Codes.XE03;
			header.CA_EngineFamilyName = ZString.Empty;
			AssertHasMessageError(header.CA_EngineFamilyNameInfo, NotEntered);
			header.CA_EngineFamilyName = "BOB";
			AssertNoMessageError(header.CA_EngineFamilyNameInfo, NotEntered);
			header.CA_ProcessCode = ProcessCodes.Codes.XE04;
			header.CA_EngineFamilyName = ZString.Empty;
			AssertNoMessageError(header.CA_EngineFamilyNameInfo, NotEntered);
			header.CA_VEEProgramInd = YesNoList.Codes.No;
			header.CA_ProcessCode = ProcessCodes.Codes.XE03;
			header.CA_EngineFamilyName = ZString.Empty;
			AssertNoMessageError(header.CA_EngineFamilyNameInfo, NotEntered);
		}

		public void TestCheckCA_EngineModelYear_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_EngineModelYearInfo, "0234", "2017");
		}

		public void TestCheckCA_EngineModelYear_Mandatory()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				header.AddInfoValidation.ValidateCA_EngineModelYear();
				AssertNoMessageErrorContaining("XE01", header.CA_EngineModelYearInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var processCode in new[] { ProcessCodes.Codes.XE02, ProcessCodes.Codes.XE03 })
				{
					header.CA_ProcessCode = processCode;
					header.AddInfoValidation.ValidateCA_EngineModelYear();
					AssertHasMessageErrorContaining(processCode, header.CA_EngineModelYearInfo, MandatoryValidation.YouHaveNotEntered);
				}

				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.AddInfoValidation.ValidateCA_EngineModelYear();
				AssertHasWarningContaining("XE04", header.CA_EngineModelYearInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_EngineModelYear = "2020";
				AssertNoMessageErrorContaining("Entered", header.CA_EngineModelYearInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_ModelOfMachine_ProcessCodes()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC20;
				header.AddInfoValidation.ValidateCA_ModelOfMachine();
				AssertHasMessageErrorContaining("XE02", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC23;
				header.AddInfoValidation.ValidateCA_ModelOfMachine();
				AssertHasMessageErrorContaining("XE03", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.AddInfoValidation.ValidateCA_ModelOfMachine();
				AssertNoMessageErrorContaining("Not XE02 or 03", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_ModelOfMachine_EngineClass()
		{
			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			header.CA_EngineClass = ECCCProductCategories.Codes.EC20;
			header.AddInfoValidation.ValidateCA_ModelOfMachine();
			AssertHasMessageErrorContaining("EC20", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_EngineClass = ECCCProductCategories.Codes.EC22;
			header.AddInfoValidation.ValidateCA_ModelOfMachine();
			AssertNoMessageErrorContaining("Not EC20 or 16", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_EngineClass = ECCCProductCategories.Codes.EC16;
			header.AddInfoValidation.ValidateCA_ModelOfMachine();
			AssertHasMessageErrorContaining("EC16", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_ModelOfMachine = "LG4571";
			AssertNoMessageErrorContaining("Entered", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_ProcessCode = ProcessCodes.Codes.XE03;
			header.CA_EngineClass = ECCCProductCategories.Codes.EC23;
			header.CA_ModelOfMachine = ZString.Empty;
			AssertHasMessageErrorContaining("EC23", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_EngineClass = ECCCProductCategories.Codes.EC27;
			header.AddInfoValidation.ValidateCA_ModelOfMachine();
			AssertNoMessageErrorContaining("Not EC23", header.CA_ModelOfMachineInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_ModelOfEngine()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				header.AddInfoValidation.ValidateCA_ModelOfEngine();
				AssertNoMessageErrorContaining("XE01", header.CA_ModelOfEngineInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var processCode in new[] { ProcessCodes.Codes.XE02, ProcessCodes.Codes.XE03 })
				{
					header.CA_ProcessCode = processCode;
					header.AddInfoValidation.ValidateCA_ModelOfEngine();
					AssertHasMessageErrorContaining(processCode, header.CA_ModelOfEngineInfo, MandatoryValidation.YouHaveNotEntered);
				}

				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.AddInfoValidation.ValidateCA_ModelOfEngine();
				AssertHasWarningContaining("XE04", header.CA_ModelOfEngineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ModelOfEngine = "STRAIGHT 6";
				AssertNoMessageErrorContaining("Entered", header.CA_ModelOfEngineInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_MakeOfMachine_ProcessCode()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC20;
				header.AddInfoValidation.ValidateCA_MakeOfMachine();
				AssertHasMessageErrorContaining("XE02", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_MakeOfMachine = "PHILLIPS";
				AssertNoMessageErrorContaining("Entered", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC23;
				header.CA_MakeOfMachine = ZString.Empty;
				AssertHasMessageErrorContaining("XE03", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.AddInfoValidation.ValidateCA_MakeOfMachine();
				AssertNoMessageErrorContaining("XE04", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_MakeOfMachine_EngineClass()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC16;
				header.AddInfoValidation.ValidateCA_MakeOfMachine();
				AssertHasMessageErrorContaining("EC16", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC26;
				header.AddInfoValidation.ValidateCA_MakeOfMachine();
				AssertNoMessageErrorContaining("Not EC16 or 20", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC20;
				header.AddInfoValidation.ValidateCA_MakeOfMachine();
				AssertHasMessageErrorContaining("EC20", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC23;
				header.AddInfoValidation.ValidateCA_MakeOfMachine();
				AssertHasMessageErrorContaining("EC23", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC29;
				header.AddInfoValidation.ValidateCA_MakeOfMachine();
				AssertNoMessageErrorContaining("Not EC23", header.CA_MakeOfMachineInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_MakeOfEngine()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				header.AddInfoValidation.ValidateCA_MakeOfEngine();
				AssertNoMessageErrorContaining("XE01", header.CA_MakeOfEngineInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var processCode in new[] { ProcessCodes.Codes.XE02, ProcessCodes.Codes.XE03 })
				{
					header.CA_ProcessCode = processCode;
					header.AddInfoValidation.ValidateCA_MakeOfEngine();
					AssertHasMessageErrorContaining(processCode, header.CA_MakeOfEngineInfo, MandatoryValidation.YouHaveNotEntered);
				}

				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.AddInfoValidation.ValidateCA_MakeOfEngine();
				AssertHasWarningContaining("XE04", header.CA_MakeOfEngineInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_MakeOfEngine = "GM";
				AssertNoMessageErrorContaining("Entered", header.CA_MakeOfEngineInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_EngineManufacturer()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				header.AddInfoValidation.ValidateCA_EngineManufacturer();
				AssertNoMessageErrorContaining("XE01", header.CA_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

				foreach (var processCode in new[] { ProcessCodes.Codes.XE02, ProcessCodes.Codes.XE03 })
				{
					header.CA_ProcessCode = processCode;
					header.AddInfoValidation.ValidateCA_EngineManufacturer();
					AssertHasMessageErrorContaining(processCode, header.CA_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
				}

				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.AddInfoValidation.ValidateCA_EngineManufacturer();
				AssertHasWarningContaining("XE04", header.CA_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_EngineManufacturer = "LG";
				AssertNoMessageErrorContaining("Entered", header.CA_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestIsCA_EngineManufacturerRequired_ForECCC()
		{
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
			header.CA_Incomplete = true;
			header.CA_EngineClass = ECCCProductCategories.Codes.EC14;
			header.AddInfoValidation.ValidateCA_EngineManufacturer();
			AssertHasMessageErrorContaining("With Engine Class", header.CA_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_EngineClass = "";
			header.AddInfoValidation.ValidateCA_EngineManufacturer();
			AssertNoMessageErrorContaining("Without Engine Class", header.CA_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_MachineManufacturer_ProcessCode()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC16;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertHasMessageErrorContaining("XE02", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertHasMessageErrorContaining("XE03", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertNoMessageErrorContaining("Not XE02 or 03", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_MachineManufacturer_EngineClass()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.CA_EngineClass = ECCCProductCategories.Codes.EC16;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertHasMessageErrorContaining("EC16", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC20;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertHasMessageErrorContaining("EC20", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC22;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertNoMessageErrorContaining("Not EC16 or EC20", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_MachineManufacturer_ManufacturerAddress()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertHasMessageErrorContaining("Empty Manufacturer Address", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);

				invoice.JZ_OA_ManufacturerAddress = Factory.New<OrgHeader>().MainAddress.PK;
				header.AddInfoValidation.ValidateCA_MachineManufacturer();
				AssertNoMessageErrorContaining("Address Entered", header.CA_MachineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_EngineClass_List()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.CA_VehicleClass = ECCCProductCategories.Codes.EC05;
				header.CA_VEEProgramInd = YesNoList.Codes.No;
				header.CA_EngineClass = "EC14";
				AssertNoMessageError("Program Id No", header.CA_EngineClassInfo, ListValidation.InvalidCodeMessageError);
				header.CA_VEEProgramInd = YesNoList.Codes.Yes;
				ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_EngineClassInfo, "EC14", "EC15");
			});
		}

		public void TestCheckCA_EngineClass_Mandatory()
		{
			CombineAssertions(() =>
			{
				header.CA_VEEProgramInd = YesNoList.Codes.No;
				header.AddInfoValidation.ValidateCA_EngineClass();
				AssertNoMessageErrorContaining("Program ID No", header.CA_EngineClassInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_VEEProgramInd = YesNoList.Codes.Yes;
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.AddInfoValidation.ValidateCA_EngineClass();
				AssertHasMessageErrorContaining("XE02", header.CA_EngineClassInfo, MandatoryValidation.YouHaveNotEntered);
				header.CA_EngineClass = "EC21";
				AssertNoMessageErrorContaining("Entered", header.CA_EngineClassInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.CA_EngineClass = ZString.Empty;
				AssertHasMessageErrorContaining("XE03", header.CA_EngineClassInfo, MandatoryValidation.YouHaveNotEntered);
				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				header.AddInfoValidation.ValidateCA_EngineClass();
				AssertNoMessageErrorContaining("Not XE02 or 03", header.CA_EngineClassInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_VehicleClass_ListValidation()
		{
			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_VehicleClassInfo, "EC25", "EC05");
		}

		public void TestCheckCA_VehicleClass_Mandatory()
		{
			CombineAssertions(() =>
			{
				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				header.AddInfoValidation.ValidateCA_VehicleClass();
				AssertHasMessageErrorContaining("XE01", header.CA_VehicleClassInfo, MandatoryValidation.YouHaveNotEntered);
				header.CA_VehicleClass = "EC05";
				AssertNoMessageErrorContaining("Vehicle Class Entered", header.CA_VehicleClassInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_VehicleClass = ZString.Empty;
				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.AddInfoValidation.ValidateCA_VehicleClass();
				AssertHasMessageErrorContaining("XE04", header.CA_VehicleClassInfo, MandatoryValidation.YouHaveNotEntered);
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.AddInfoValidation.ValidateCA_VehicleClass();
				AssertNoMessageErrorContaining("Not XE01 or 04", header.CA_VehicleClassInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_ProcessCode_Mandatory()
		{
			CombineAssertions(() =>
			{
				header.CA_VEEProgramInd = YesNoList.Codes.No;
				header.AddInfoValidation.ValidateCA_ProcessCode();
				AssertNoMessageErrorContaining("Program Indicator N", header.CA_ProcessCodeInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_VEEProgramInd = YesNoList.Codes.Yes;
				header.AddInfoValidation.ValidateCA_ProcessCode();
				AssertHasMessageErrorContaining("Program Indicator Y", header.CA_ProcessCodeInfo, MandatoryValidation.YouHaveNotEntered);

				header.CA_ProcessCode = ProcessCodes.Codes.XE01;
				AssertNoMessageErrorContaining("Process Code entered", header.CA_ProcessCodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCA_ProcessCode_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_ProcessCodeInfo, "AAAA", ProcessCodes.Codes.XE01);
		}

		public void TestCheckCA_ProcessCode_NeedsStatement_TransitionAndCode()
		{
			SetupRequiredNeedsStatement();
			CombineAssertions(() =>
			{
				header.AddInfoValidation.ValidateCA_ProcessCode();
				AssertHasMessageErrorContaining("All false", header.CA_ProcessCodeInfo, NeedStatementsMessageWarning);
				header.CA_Transition = true;
				header.AddInfoValidation.ValidateCA_ProcessCode();
				AssertHasMessageErrorContaining("Transition true", header.CA_ProcessCodeInfo, NeedStatementsMessageWarning);
				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.CA_Transition = true;
				header.AddInfoValidation.ValidateCA_ProcessCode();
				AssertNoMessageErrorContaining("Process Code XE02 and Transition", header.CA_ProcessCodeInfo, NeedStatementsMessageWarning);
			});
		}

		public void TestCheckCA_ProcessCode_NeedsStatement_NationalMark()
		{
			AssertProcessCodeNeedsStatement(header.CA_NationalMarkInfo);
		}

		public void TestCheckCA_ProcessCode_NeedsStatement_EPACertified()
		{
			AssertProcessCodeNeedsStatement(header.CA_EPACertifiedInfo);
		}

		public void TestCheckCA_ProcessCode_NeedsStatement_CanadaUnique()
		{
			AssertProcessCodeNeedsStatement(header.CA_CanadaUniqueInfo);
		}

		public void TestCheckCA_ProcessCode_NeedsStatement_Incomplete()
		{
			AssertProcessCodeNeedsStatement(header.CA_IncompleteInfo);
		}

		public void TestCheckCA_ProcessCode_NeedsStatement_BulkReporting()
		{
			AssertProcessCodeNeedsStatement(header.CA_BulkReportingInfo);
		}

		public void TestCheckCA_SourceOfSpecimen()
		{
			header.CA_WENProgramInd = "Y";
			header.AddInfoValidation.ValidateCA_SourceOfSpecimen();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.CA_SourceOfSpecimenInfo);

			header.CA_SourceOfSpecimen = "EC35";
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_SourceOfSpecimenInfo, "EC01", "EC35");
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_SourceOfSpecimenInfo, "EC02", "EC43");
		}

		public void TestCheckCA_Sex()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_SexInfo, "F", "Female");
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_SexInfo, "M", "Male");
		}

		public void TestCheckCA_LifeStage()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_LifeStageInfo, "prog", "Propagate");
			ValidationTestHelper.AssertInvalidCodeMessageError(header.CA_LifeStageInfo, "juv", "Juvenile");
		}

		public void TestCheckCA_ScientificName()
		{
			header.CA_WENProgramInd = "Y";
			header.CA_ComplianceDeclaration = true;
			header.AddInfoValidation.ValidateCA_ScientificName();
			AssertHasMessageErrorContaining(header.CA_ScientificNameInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_ScientificName = "MAT";
			AssertNoMessageErrorContaining(header.CA_ScientificNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCA_IntendedUseCode()
		{
			header.CA_WENProgramInd = "Y";
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_IntendedUseCode = "050.007";
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			header.CA_IntendedUseCode = "EC01";
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertNoMessageErrorContaining(header.CA_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			header.CA_WRMProgramInd = "Y";
			header.CA_IntendedUseCode = "EC01";
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertNoMessageErrorContaining(header.CA_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			header.CA_IntendedUseCode = "050.007";
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertNoMessageErrorContaining(header.CA_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			header.CA_IntendedUseCode = "XX";
			header.AddInfoValidation.ValidateCA_IntendedUseCode();
			AssertHasMessageErrorContaining(header.CA_IntendedUseCodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCA_Incomplete_ProcessCode()
		{
			SetupIncompleteVehicle();
			header.CA_EngineClass = ECCCProductCategories.Codes.EC24;
			CombineAssertions(() =>
			{
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("XE01", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_ProcessCode = ProcessCodes.Codes.XE02;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertNoMessageErrorContaining("Not XE01 or 04 and not Incomplete Engine Class", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC21;
				header.CA_ProcessCode = ProcessCodes.Codes.XE03;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("XE03 with Incomplete Engine Class", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_Incomplete = true;
				AssertNoMessageErrorContaining("Is Incomplete", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_ProcessCode = ProcessCodes.Codes.XE04;
				header.CA_Incomplete = false;
				AssertHasMessageErrorContaining("XE04", header.CA_IncompleteInfo, IncompleteVehicleMessageError);
			});
		}

		public void TestCheckCA_Incomplete_VehicleClass()
		{
			SetupIncompleteVehicle();
			header.CA_EngineClass = ECCCProductCategories.Codes.EC27;
			CombineAssertions(() =>
			{
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("EC13", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_VehicleClass = ECCCProductCategories.Codes.EC28;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertNoMessageErrorContaining("Not EC13 or 32", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_VehicleClass = ECCCProductCategories.Codes.EC32;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("EC32", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_VEEProgramInd = YesNoList.Codes.No;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertNoMessageErrorContaining("Program Indicator N", header.CA_IncompleteInfo, IncompleteVehicleMessageError);
			});

			SetupIncompleteVehicle();
			header.CA_EngineClass = ECCCProductCategories.Codes.EC27;
			CombineAssertions(() =>
			{
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("EC32", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_VehicleClass = ECCCProductCategories.Codes.EC28;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertNoMessageErrorContaining("Not EC32", header.CA_IncompleteInfo, IncompleteVehicleMessageError);
			});
		}

		public void TestCheckCA_Incomplete_EngineClass()
		{
			SetupIncompleteVehicle();
			header.CA_VehicleClass = ECCCProductCategories.Codes.EC15;
			CombineAssertions(() =>
			{
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("EC21", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC33;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("EC33", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC34;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertHasMessageErrorContaining("EC34", header.CA_IncompleteInfo, IncompleteVehicleMessageError);

				header.CA_EngineClass = ECCCProductCategories.Codes.EC23;
				header.AddInfoValidation.ValidateCA_Incomplete();
				AssertNoMessageErrorContaining("Not EC21, 33 or 34", header.CA_IncompleteInfo, IncompleteVehicleMessageError);
			});
		}

		public void TestCheckCA_PowerRatingUQ()
		{
			header.AddInfoValidation.ValidateCA_PowerRatingUQ();
			AssertNoMessageErrorContaining(header.CA_PowerRatingUQInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_EnginePowerRating = 5m;
			header.AddInfoValidation.ValidateCA_PowerRatingUQ();
			AssertHasMessageErrorContaining(header.CA_PowerRatingUQInfo, MandatoryValidation.YouHaveNotEntered);

			header.CA_PowerRatingUQ = "XX";
			header.AddInfoValidation.ValidateCA_PowerRatingUQ();
			AssertHasMessageErrorContaining(header.CA_PowerRatingUQInfo, NotInList);
		}

		public void TestCheckCA_MachineManufacturer_ValidateMandatory()
		{
			CreateDataForMessageSending();
			header.CA_ProcessCode = ProcessCodes.Codes.XE03;

			header.CA_MachineManufacturer = orgAddress.PK;

			AssertNoMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a name");
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_MachineManufacturer();
			AssertNoMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.Header.OH_FullName = ZString.Empty;
			header.AddInfoValidation.ValidateCA_MachineManufacturer();
			AssertNoMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_MachineManufacturer();
			AssertHasMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a name");

			AssertNoMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered an address line 1");
			orgAddress.OA_Address1 = ZString.Empty;
			header.AddInfoValidation.ValidateCA_MachineManufacturer();
			AssertHasMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered an address line 1");

			AssertNoMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a city");
			orgAddress.OA_City = ZString.Empty;
			header.AddInfoValidation.ValidateCA_MachineManufacturer();
			AssertHasMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a city");

			AssertNoMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a country");
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			header.AddInfoValidation.ValidateCA_MachineManufacturer();
			AssertHasMessageErrorContaining(header.CA_MachineManufacturerInfo, "You have not entered a country");
			AssertHasMessageErrorContaining(header.CA_MachineManufacturerInfo, "Manufacturer");
		}

		public void TestCheckCA_OA_EngineLocation_ValidateMandatory()
		{
			CreateDataForMessageSending();
			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_AOSConformity = AffirmationOfStatementCodes.Codes.ME04;

			header.CA_OA_EngineLocation = orgAddress.PK;

			AssertNoMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a name");
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EngineLocation();
			AssertNoMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.Header.OH_FullName = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EngineLocation();
			AssertNoMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EngineLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a name");

			AssertNoMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered an address line 1");
			orgAddress.OA_Address1 = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EngineLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered an address line 1");

			AssertNoMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a city");
			orgAddress.OA_City = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EngineLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a city");

			AssertNoMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a country");
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EngineLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EngineLocationInfo, "You have not entered a country");
			AssertHasMessageErrorContaining(header.CA_OA_EngineLocationInfo, "Engine Location");
		}

		public void TestCheckCA_OA_EvidenceOfConformityLocation_ValidateMandatory()
		{
			CreateDataForMessageSending();
			header.CA_ProcessCode = ProcessCodes.Codes.XE02;
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;

			header.CA_OA_EvidenceOfConformityLocation = orgAddress.PK;

			AssertNoMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a name");
			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertNoMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = "ORGDEF";
			orgAddress.Header.OH_FullName = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertNoMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a name");

			orgAddress.OA_CompanyNameOverride = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a name");

			AssertNoMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered an address line 1");
			orgAddress.OA_Address1 = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered an address line 1");

			AssertNoMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a city");
			orgAddress.OA_City = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a city");

			AssertNoMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a country");
			orgAddress.OA_RN_NKCountryCode = ZString.Empty;
			header.AddInfoValidation.ValidateCA_OA_EvidenceOfConformityLocation();
			AssertHasMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "You have not entered a country");
			AssertHasMessageErrorContaining(header.CA_OA_EvidenceOfConformityLocationInfo, "Evidence of Conformity Location");
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

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = "Y";
			header = invoiceLine.ECCCPGAHeader;
		}
		ECCCPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		void SetupRequiredNeedsStatement()
		{
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_NationalMark = false;
			header.CA_EPACertified = false;
			header.CA_CanadaUnique = false;
			header.CA_Incomplete = false;
			header.CA_BulkReporting = false;
			header.CA_Transition = false;
			header.CA_ProcessCode = ProcessCodes.Codes.XE03;
		}

		void SetupIncompleteVehicle()
		{
			header.CA_VEEProgramInd = YesNoList.Codes.Yes;
			header.CA_Incomplete = false;
			header.CA_VehicleClass = ECCCProductCategories.Codes.EC32;
			header.CA_EngineClass = ECCCProductCategories.Codes.EC21;
			header.CA_ProcessCode = ProcessCodes.Codes.XE01;
		}

		void PrepareRefData()
		{
			var date = ZDateTime.Today;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var codeType01 = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement;
			// With attribute - AOSConformity
			var cusCode = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME01", "ME01", startDate, endDate);
			var cusCode01 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME02", "ME02", startDate, endDate);
			var cusCode02 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME03", "ME03", startDate, endDate);
			var cusCode03 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME04", "ME04", startDate, endDate);
			var attributeType01 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSConformity, "AOSConformity", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			var attribute01 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode01.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			var attribute02 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode02.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			var attribute03 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode03.PK, attributeType01.ZXE_Name, YesNoList.Codes.Yes);
			// With attribute - AOSReplacement
			var cusCode04 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME05", "ME05", startDate, endDate);
			var cusCode05 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME06", "ME06", startDate, endDate);
			var attributeType02 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSReplacement, "AOSReplacement", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute04 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode04.PK, attributeType02.ZXE_Name, YesNoList.Codes.Yes);
			var attribute05 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode05.PK, attributeType02.ZXE_Name, YesNoList.Codes.Yes);
			// With attribute - AOSEvidence
			var cusCode06 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME07", "ME07", startDate, endDate);
			var cusCode07 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME08", "ME08", startDate, endDate);
			var attributeType03 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSEvidence, "AOSEvidence", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute06 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode06.PK, attributeType03.ZXE_Name, YesNoList.Codes.Yes);
			var attribute07 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode07.PK, attributeType03.ZXE_Name, YesNoList.Codes.Yes);
			// With attribute - AOSRetention
			var cusCode08 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME09", "ME09", startDate, endDate);
			var cusCode09 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME10", "ME10", startDate, endDate);
			var cusCode10 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType01, "ME11", "ME11", startDate, endDate);
			var attributeType04 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.AOSRetention, "AOSRetention", codeType01, Core.Constants.CountryCodes.Canada);
			var attribute08 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode08.PK, attributeType04.ZXE_Name, YesNoList.Codes.Yes);
			var attribute09 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode09.PK, attributeType04.ZXE_Name, YesNoList.Codes.Yes);
			var attribute10 = helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode10.PK, attributeType04.ZXE_Name, YesNoList.Codes.Yes);

			// ECACS
			var codeType02 = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCAlternativeStandardConformityStatements;
			var cusCode11 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType02, "EC0E", "EC0E", startDate, endDate);
			var cusCode12 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType02, "EC0F", "EC0F", startDate, endDate);
			Factory.Save();
		}

		const string NotEntered = "You have not entered a value.";
		const string NotInList = "The code you have selected is not in the list.";
		const string InvalidAOSReplacementCode = "Affirmation of Statement Replacement code ME05 or ME06 can only be provided if Affirmation of Statement Conformity selection includes ME01";

		void AssertProcessCodeNeedsStatement(ZPropertyInfo propertyInfo)
		{
			SetupRequiredNeedsStatement();
			CombineAssertions(() =>
			{
				header.AddInfoValidation.ValidateCA_ProcessCode();
				AssertHasMessageErrorContaining("false", header.CA_ProcessCodeInfo, NeedStatementsMessageWarning);
				propertyInfo.Value = ZBool.True;
				header.AddInfoValidation.ValidateCA_ProcessCode();
				AssertNoMessageErrorContaining("true", header.CA_ProcessCodeInfo, NeedStatementsMessageWarning);
			});
		}

		const string NeedStatementsMessageWarning = "At least one of compliance statements must be provided.";
		const string IncompleteVehicleMessageError = "Incomplete Vehicles or Engines should be provided.";
	}
}
