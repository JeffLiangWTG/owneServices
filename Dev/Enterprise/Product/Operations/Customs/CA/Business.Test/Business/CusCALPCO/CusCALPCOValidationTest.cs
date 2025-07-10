using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCALPCOValidationTest : TestCaseWithFactory
	{
		public void TestCheckCLP_RefNoForCFIA()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;

			var cfia = invoiceLine.CFIAPGAHeader;
			var lpco = cfia.LPCOViews.AddNew().LPCO;
			var validation = lpco.Validation;

			validation.ValidateCLP_RefNo();
			AssertNoNotifications(lpco.CLP_RefNoInfo);

			lpco.CLP_Type = "C00";
			validation.ValidateCLP_RefNo();
			AssertEquals(1, lpco.CLP_RefNoInfo.Notifications.Count());
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, "You have not entered");

			lpco.CLP_Type = "C01";
			validation.ValidateCLP_RefNo();
			AssertEquals(1, lpco.CLP_RefNoInfo.Notifications.Count());
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, "You have not entered");

			lpco.CLP_RefNo = "!!";
			validation.ValidateCLP_RefNo();
			AssertHasWarningContaining(lpco.CLP_RefNoInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());

			lpco.CLP_RefNo = "123-456.s";
			validation.ValidateCLP_RefNo();
			AssertNoWarningContaining(lpco.CLP_RefNoInfo, RegistrationNumberHelper.ShouldBeAlphanumeric.ToString());

			lpco.CLP_RefNo = "A12";
			validation.ValidateCLP_RefNo();
			AssertNoNotifications(lpco.CLP_RefNoInfo);

			lpco.CLP_Type = "C02";
			lpco.CLP_RefNo = ZString.Empty;
			validation.ValidateCLP_RefNo();
			AssertEquals(1, lpco.CLP_RefNoInfo.Notifications.Count());
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, "You have not entered");

			lpco.CLP_RefNo = "A12";
			validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, RegistrationNumberHelper.ShouldBeNumericOnly.ToString());

			lpco.CLP_RefNo = "112";
			validation.ValidateCLP_RefNo();
			AssertNoNotifications(lpco.CLP_RefNoInfo);

			lpco.CLP_RefNo = "11.2";
			validation.ValidateCLP_RefNo();
			AssertNoNotifications(lpco.CLP_RefNoInfo);

			lpco.CLP_Type = "C03";
			lpco.CLP_RefNo = ZString.Empty;
			validation.ValidateCLP_RefNo();
			AssertEquals(1, lpco.CLP_RefNoInfo.Notifications.Count());
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, "You have not entered");

			lpco.CLP_RefNo = "A12";
			validation.ValidateCLP_RefNo();
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, RegistrationNumberHelper.ShouldIndicateY.ToString());

			lpco.CLP_RefNo = YesNoList.Codes.Yes;
			validation.ValidateCLP_RefNo();
			AssertNoNotifications(lpco.CLP_RefNoInfo);
		}

		public void TestCheckCLP_DIFRefNumberOrLocationForCFIA()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;

			var cfia = invoiceLine.CFIAPGAHeader;
			var lpco = cfia.LPCOViews.AddNew().LPCO;
			var validation = lpco.Validation;

			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoNotifications(lpco.CLP_DIFRefNumberOrLocationInfo);

			lpco.CLP_Type = "C00";
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoNotifications(lpco.CLP_DIFRefNumberOrLocationInfo);

			lpco.CLP_Type = "C01";
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.RequireDIFURN.ToString());

			lpco.CLP_DIFRefNumberOrLocation = "AAA";
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.RequireDIFURN.ToString());
			AssertNoMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.NotRequireDIFURN.ToString());

			lpco.CLP_Type = "C02";
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.NotRequireDIFURN.ToString());

			lpco.CLP_DIFRefNumberOrLocation = ZString.Empty;
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoNotifications(lpco.CLP_DIFRefNumberOrLocationInfo);

			lpco.CLP_Type = "C03";
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.RequireDIFURN.ToString());

			lpco.CLP_DIFRefNumberOrLocation = "AAA";
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.RequireDIFURN.ToString());
			AssertNoMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.NotRequireDIFURN.ToString());

			cfia.RN_NKCountryOfSource = Core.Constants.CountryCodes.Australia;
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, RegistrationNumberHelper.NotRequireDIFURN.ToString());

			lpco.CLP_DIFRefNumberOrLocation = ZString.Empty;
			validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoNotifications(lpco.CLP_DIFRefNumberOrLocationInfo);
		}

		public void TestCheckCLP_RefNoWhenCFIA893()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMPORTER";
			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			var license = orgImpAddInfo.SafeFoodLicenses.AddNew();
			license.CY_Code = "SFC";
			license.CY_Data = "SafeFoodLicense";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			var cfia = invoiceLine.CFIAPGAHeader;
			var lpco = cfia.LPCOViews.AddNew().LPCO;

			lpco.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			lpco.CLP_RefNo = "A";
			AssertEquals(1, lpco.Lookups.RefNumbers.Count);
			AssertHasWarning(lpco.CLP_RefNoInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());

			lpco.CLP_RefNo = "SFC";
			AssertNoWarning(lpco.CLP_RefNoInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());

			lpco = declaration.LPCOs.AddNew();
			lpco.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			lpco.CLP_RefNo = "A";
			AssertEquals(1, lpco.Lookups.RefNumbers.Count);
			AssertHasWarning(lpco.CLP_RefNoInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());

			lpco.CLP_RefNo = "SFC";
			AssertNoWarning(lpco.CLP_RefNoInfo, RegistrationNumberHelper.SafeFoodLicenseNotListed.ToString());
		}

		public void TestCheckCLP_RN_NKIssuanceCountryCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_RN_NKIssuanceCountryCodeInfo, "AA", "AM");
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_RN_NKIssuanceCountryCodeInfo, "DD", "ZW");

			cNSCLPCO.CLP_Type = LPCODocumentTypeQualifier.Codes._3004;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cNSCLPCO.CLP_RN_NKIssuanceCountryCodeInfo);
		}

		public void TestCheckCLP_RN_NKSmeltAndPourCountryCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(gACLPCO.CLP_RN_NKSmeltAndPourCountryCodeInfo, "ZZ", "CA");
		}

		public void TestCheckCLP_CFIALPCORefNumberOrLocationWithMaterializedTypeCode()
		{
			var newFactory = new BusinessObjectFactory();
			RegistrationNumberHelperTest.PrepareGlobalData(newFactory);

			var message = "DIF URN must be numeric and should be 14 digits";

			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;
			invoiceLine.CA_HCInd = Customs.Business.YesNoList.Codes.Yes;

			var header = invoiceLine.CFIAPGAHeader;
			var lpco = header.LPCOViews.AddNew();
			lpco.CLP_Type = "1A3";
			lpco.CLP_DIFRefNumberOrLocation = "111";
			lpco.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, message);

			lpco.CLP_Type = "988";
			lpco.CLP_DIFRefNumberOrLocation = "11112222333355";
			lpco.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, message);

			var headerHC = invoiceLine.HCPGAHeader;
			var lpcoHC = headerHC.LPCOViews.AddNew();
			lpcoHC.CLP_Type = LPCODocumentTypeQualifier.Codes._5008;
			lpcoHC.CLP_DIFRefNumberOrLocation = "";
			lpcoHC.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpcoHC.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);

			lpcoHC.CLP_Type = LPCODocumentTypeQualifier.Codes._5038;
			lpcoHC.CLP_DIFRefNumberOrLocation = "";
			lpcoHC.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoMessageErrorContaining(lpcoHC.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCLP_CFIALPCOURNMandatory_HC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = Customs.Business.YesNoList.Codes.Yes;

			var headerHC = invoiceLine.HCPGAHeader;

			var madatoryTypeList = headerHC.GetURNMandatoryDocumentTypes();
			madatoryTypeList.Sort();
			var result = string.Join(",", madatoryTypeList);
			AssertEquals("HC Mandatory Type list - 5008,5009", "5008,5009", result);

			var lpcoHC = headerHC.LPCOViews.AddNew();
			lpcoHC.CLP_Type = LPCODocumentTypeQualifier.Codes._5008;
			lpcoHC.CLP_DIFRefNumberOrLocation = "";
			lpcoHC.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpcoHC.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);

			lpcoHC.CLP_Type = LPCODocumentTypeQualifier.Codes._5038;
			lpcoHC.CLP_DIFRefNumberOrLocation = "";
			lpcoHC.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoMessageErrorContaining(lpcoHC.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);

			lpcoHC.CLP_Type = LPCODocumentTypeQualifier.Codes._5009;
			lpcoHC.CLP_DIFRefNumberOrLocation = "";
			lpcoHC.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertHasMessageErrorContaining(lpcoHC.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);

			lpcoHC.CLP_Type = LPCODocumentTypeQualifier.Codes._5003;
			lpcoHC.CLP_DIFRefNumberOrLocation = "";
			lpcoHC.Validation.ValidateCLP_DIFRefNumberOrLocation();
			AssertNoMessageErrorContaining(lpcoHC.CLP_DIFRefNumberOrLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCLP_CFIALPCOURNMandatory_ECCC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var headerECCC = invoiceLine.ECCCPGAHeader;

			var madatoryTypeList = headerECCC.GetURNMandatoryDocumentTypes();
			madatoryTypeList.Sort();
			var result = string.Join(",", madatoryTypeList);
			AssertEquals("ECCC Mandatory Type list - 8000, 8001, 8010, 8011, 8012", "8000,8001,8010,8011,8012,8020,8021,8022,8023,8030,8031", result);
		}

		public void TestCheckCLP_CFIALPCOURNMandatory_TC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_TCInd = YesNoList.Codes.Yes;

			var headerTC = invoiceLine.TCPGAHeader;
			var madatoryTypeList = headerTC.GetURNMandatoryDocumentTypes();
			madatoryTypeList.Sort();
			var result = string.Join(",", madatoryTypeList);
			AssertEquals("TC Mandatory Type list - 4001, 4002, 4003, 4004, 4005, 4006", "4001,4002,4003,4004,4005,4006", result);
		}

		public void TestCLP_RN_NKOriginCountryCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_RN_NKOriginCountryCodeInfo, "AA", "AM");
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_RN_NKOriginCountryCodeInfo, "DD", "ZW");
		}

		public void TestCLP_RN_NKAuthorizationCountry()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_RN_NKAuthorizationCountryInfo, "AA", "AM");
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_RN_NKAuthorizationCountryInfo, "DD", "ZW");
		}

		public void TestCheckCLP_HolderType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_HolderTypeInfo, "AAA", "IMP");
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_HolderTypeInfo, "BBB", "EXP");
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_HolderTypeInfo, "CCC", "IOR");

			cNSCLPCO.Validation.ValidateCLP_HolderType();
			AssertNoMessageErrorContaining(cNSCLPCO.CLP_HolderTypeInfo, MandatoryValidation.YouHaveNotEntered);

			CombineAssertions(() =>
			{
				var documentTypes = new ZString[]
				{
					LPCODocumentTypeQualifier.Codes._2001,
					LPCODocumentTypeQualifier.Codes._2003,
					LPCODocumentTypeQualifier.Codes._7000,
					LPCODocumentTypeQualifier.Codes._8020,
					LPCODocumentTypeQualifier.Codes._8021,
					LPCODocumentTypeQualifier.Codes._8022,
					LPCODocumentTypeQualifier.Codes._8023
				};

				foreach (var documentType in documentTypes)
				{
					cNSCLPCO.CLP_Type = documentType;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cNSCLPCO.CLP_HolderTypeInfo);
				}
			});
		}

		public void TestCheckCLP_HolderType_CheckHolderExist_InvoiceLine()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpco = gacPGAHeader.LPCOViews.AddNew();
			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertHasMessageErrorContaining(lpco.CLP_HolderTypeInfo, "Importer doesn't exist for this invoice line.");
			AssertHasMessageErrorContaining(lpco.CLP_HolderTypeInfo, "doesn't exist for this invoice line.");

			declaration.JE_OH_Importer = importer.PK;
			lpco.Validation.ValidateCLP_HolderType();
			AssertNoMessageErrorContaining(lpco.CLP_HolderTypeInfo, "Importer doesn't exist for this invoice line.");

			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			AssertNoMessageErrorContaining(lpco.CLP_HolderTypeInfo, "doesn't exist for this invoice line.");
		}

		public void TestCheckCLP_HolderType_CheckHolderExist_Product()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";

			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();

			pivot.CCA_GACIndicator = YesNoList.Codes.Yes;
			var gacPGAHeader = pivot.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpco = gacPGAHeader.LPCOViews.AddNew();
			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertNoMessageErrors(lpco.CLP_HolderTypeInfo);
		}

		public void TestCheckOtherLPCOApplicant()
		{
			var party = Factory.New<OrgHeader>();
			party.OH_FullName = "TestOrgName";

			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;

			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco.CLP_OA_Applicant = ZGuid.Empty;
			lpco.Validation.ValidateCLP_OA_Applicant();
			AssertHasMessageErrorContaining(lpco.CLP_OA_ApplicantInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_OA_Applicant = party.MainAddress.PK;
			AssertNoMessageErrorContaining(lpco.CLP_OA_ApplicantInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(lpco.CLP_OA_ApplicantInfo, "does not specify the Business Number Customs Broker(BRB) or Business Number For Import Export(BRM), please see Organization -> Details -> Config -> Registration Numbers / Codes.");

			lpco.CLP_IsApplicantOverridden = true;
			lpco.CLP_ApplicantName = ZString.Empty;
			AssertHasMessageErrorContaining(lpco.CLP_ApplicantNameInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_ApplicantName = "Lilly";
			AssertNoMessageErrorContaining(lpco.CLP_ApplicantNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckOtherLPCOHolder()
		{
			var party = Factory.New<OrgHeader>();
			party.OH_FullName = "TestOrgName";

			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;

			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco.CLP_OA_Holder = ZGuid.Empty;
			lpco.Validation.ValidateCLP_OA_Holder();
			AssertHasMessageErrorContaining(lpco.CLP_OA_HolderInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_OA_Holder = party.MainAddress.PK;
			AssertNoMessageErrorContaining(lpco.CLP_OA_HolderInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(lpco.CLP_OA_HolderInfo, "does not specify the Business Number Customs Broker(BRB) or Business Number For Import Export(BRM), please see Organization -> Details -> Config -> Registration Numbers / Codes.");

			lpco.CLP_IsHolderOverridden = true;
			lpco.CLP_HolderName = ZString.Empty;
			AssertHasMessageErrorContaining(lpco.CLP_HolderNameInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_HolderName = "Lilly";
			AssertNoMessageErrorContaining(lpco.CLP_HolderNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCLP_ApplicantType_BRBOrBRM()
		{
			var expectedErrorFormat = "The {0} does not specify the Business Number Customs Broker(BRB) or Business Number For Import Export(BRM), please see Organization -> Details -> Config -> Registration Numbers / Codes.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var broker = Factory.New<OrgHeader>();
				broker.OH_FullName = "TestBroker";

				var importer = Factory.New<OrgHeader>();
				importer.OH_FullName = "TestImporter";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				declaration.JE_OH_Importer = importer.PK;

				var company = declaration.EffectiveBranch.Company;
				var originalProxy = company.GC_OH_OrgProxy;

				try
				{
					company.GC_OH_OrgProxy = broker.PK;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

					var pgaHeader = invoiceLine.GACPGAHeader;
					pgaHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

					var lpco = pgaHeader.LPCOViews.AddNew();
					lpco.CLP_ApplicantType = string.Empty;

					AssertNoMessageErrors("Should not has expected error as the applicant is empty.", lpco.CLP_ApplicantTypeInfo);

					lpco.CLP_ApplicantType = "XXX";

					AssertEquals(1, lpco.CLP_ApplicantTypeInfo.Notifications.Count());
					AssertHasMessageError(lpco.CLP_ApplicantTypeInfo, "The code you have selected is not in the list.");

					lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Broker;

					var expectedError = string.Format(expectedErrorFormat, broker.HumanReadableShortcutName);
					AssertHasMessageError("Should has expected error as the broker doesn't have BRB or BRM number.", lpco.CLP_ApplicantTypeInfo, expectedError);

					broker.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "00000000", Core.Constants.CountryCodes.Canada);
					lpco.Validation.ValidateCLP_ApplicantType();

					AssertNoMessageError("Should not has expected error as the broker has BRB or BRM number.", lpco.CLP_ApplicantTypeInfo, expectedError);

					lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;

					expectedError = string.Format(expectedErrorFormat, importer.HumanReadableShortcutName);
					AssertHasMessageError("Should has expected error as the importer doesn't have BRB or BRM number.", lpco.CLP_ApplicantTypeInfo, expectedError);

					importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker, "00000000", Core.Constants.CountryCodes.Canada);
					lpco.Validation.ValidateCLP_ApplicantType();

					AssertNoMessageError("Should not has expected error as the importer has BRB or BRM number.", lpco.CLP_ApplicantTypeInfo, expectedError);
				}
				finally
				{
					company.GC_OH_OrgProxy = originalProxy;
				}
			}
		}

		public void TestCheckCLP_AlternativeQuotaQuantity()
		{
			var error = "value cannot be zero.";

			gACLPCO.CLP_Type = LPCODocumentTypeQualifier.Codes._2005;
			gACLPCO.Validation.ValidateCLP_AlternativeQuotaQuantity();

			AssertHasMessageError(gACLPCO.CLP_AlternativeQuotaQuantityInfo, error);

			gACLPCO.CLP_AlternativeQuotaQuantity = 1m;
			AssertNoMessageError(gACLPCO.CLP_AlternativeQuotaQuantityInfo, error);

			gACLPCO.CLP_Type = LPCODocumentTypeQualifier.Codes._2001;
			gACLPCO.CLP_AlternativeQuotaQuantity = 0m;

			AssertNoMessageError(gACLPCO.CLP_AlternativeQuotaQuantityInfo, error);
		}

		public void TestCheckCLP_ApplicantType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(cNSCLPCO.CLP_ApplicantTypeInfo, "AAA", "IMP");

			CombineAssertions(() =>
			{
				var documentTypes = new ZString[]
				{
					LPCODocumentTypeQualifier.Codes._2001,
					LPCODocumentTypeQualifier.Codes._2003,
					LPCODocumentTypeQualifier.Codes._3001,
					LPCODocumentTypeQualifier.Codes._3002,
					LPCODocumentTypeQualifier.Codes._3003,
				};

				foreach (var documentType in documentTypes)
				{
					cNSCLPCO.CLP_Type = documentType;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cNSCLPCO.CLP_ApplicantTypeInfo);
				}
			});
		}

		public void TestCheckCLP_ApplicantType_CheckHolderExist_InvoiceLine()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpco = gacPGAHeader.LPCOViews.AddNew();
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertHasMessageErrorContaining(lpco.CLP_ApplicantTypeInfo, "Importer doesn't exist for this invoice line.");

			declaration.JE_OH_Importer = importer.PK;
			lpco.Validation.ValidateCLP_ApplicantType();
			AssertNoMessageErrorContaining(lpco.CLP_ApplicantTypeInfo, "Importer doesn't exist for this invoice line.");
		}

		public void TestCheckCLP_ApplicantType_CheckHolderExist_Product()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";

			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();

			pivot.CCA_GACIndicator = YesNoList.Codes.Yes;
			var gacPGAHeader = pivot.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;

			var lpco = gacPGAHeader.LPCOViews.AddNew();
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertNoMessageErrors(lpco.CLP_ApplicantTypeInfo);
		}

		public void TestCheckCLP_RefNo()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");
			newFactory.Save();

			gACLPCO.CLP_Type = "0003";
			AssertEquals("YYY", gACLPCO.CLP_RefNo);

			gACLPCO.CLP_RefNo = "ABC";
			AssertHasMessageError(gACLPCO.CLP_RefNoInfo, "Ref No. should remain : YYY.");
		}

		public void TestCheckCLP_RefNo_MandatoryValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_PermitApplication = false;

			var invoiceHeader = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.CA_GACInd = YesNoList.Codes.Yes;

			var pgaHeader = line.GACPGAHeader;
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var lpco = pgaHeader.LPCOViews.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lpco.CLP_RefNoInfo);

			declaration.CA_PermitApplication = true;
			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2001;

			lpco.Validation.ValidateCLP_RefNo();
			AssertNoMessageError(lpco.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2003;

			lpco.Validation.ValidateCLP_RefNo();
			AssertNoMessageError(lpco.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.CA_PermitApplication = false;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lpco.CLP_RefNoInfo);

			declaration.CA_PermitApplication = true;
			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2004;

			lpco.Validation.ValidateCLP_RefNo();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lpco.CLP_RefNoInfo);
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
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			pgaHeader.CA_WRMProgramInd = YesNoList.Codes.Yes;
			pgaHeader.LPCOViews.RemoveAndDeleteAll();

			var lpco1 = pgaHeader.LPCOViews.AddNew();

			lpco1.CLP_Type = "XXX";
			AssertHasMessageErrorContaining(lpco1.CLP_TypeInfo, ListValidation.InvalidCodeMessageError);

			lpco1.CLP_Type = ZString.Empty;
			AssertHasMessageErrorContaining(lpco1.CLP_TypeInfo, MandatoryValidation.YouHaveNotEntered);

			lpco1.CLP_Type = LPCODocumentTypeQualifier.Codes._8001;
			AssertNoMessageErrors(lpco1.CLP_TypeInfo);
		}

		public void TestCheckCLP_SecondaryRefNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.GACPGAHeader;
			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.Validation.ValidateCLP_SecondaryRefNo();
			AssertNoMessageErrorContaining(lpco.CLP_SecondaryRefNoInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2007;
			lpco.Validation.ValidateCLP_SecondaryRefNo();
			AssertHasMessageErrorContaining(lpco.CLP_SecondaryRefNoInfo, MandatoryValidation.YouHaveNotEntered);

			lpco.CLP_SecondaryRefNo = "AAA";
			AssertNoMessageErrorContaining(lpco.CLP_SecondaryRefNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCLP_LPCOIssueDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.GACPGAHeader;
			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.Validation.ValidateCLP_IssueDate();
			AssertNoMessageErrorContaining(lpco.CLP_IssueDateInfo, MandatoryValidation.YouHaveNotEntered);

			CombineAssertions(() =>
			{
				var documentTypes = new ZString[]
				{
					LPCODocumentTypeQualifier.Codes._3004,
					LPCODocumentTypeQualifier.Codes._2007,
					LPCODocumentTypeQualifier.Codes._8020,
					LPCODocumentTypeQualifier.Codes._8021,
					LPCODocumentTypeQualifier.Codes._8022,
					LPCODocumentTypeQualifier.Codes._8023
				};

				foreach (var documentType in documentTypes)
				{
					lpco.CLP_Type = documentType;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lpco.CLP_IssueDateInfo);
				}
			});
		}

		public void TestCheckCLP_LPCOEndDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			var lpco = pgaHeader.LPCOViews.AddNew();

			CombineAssertions(() =>
			{
				var documentTypes = new ZString[]
				{
					LPCODocumentTypeQualifier.Codes._3004,
					LPCODocumentTypeQualifier.Codes._8020,
					LPCODocumentTypeQualifier.Codes._8021,
					LPCODocumentTypeQualifier.Codes._8022,
					LPCODocumentTypeQualifier.Codes._8023
				};

				foreach (var documentType in documentTypes)
				{
					lpco.CLP_Type = documentType;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lpco.CLP_EndDateInfo);
				}
			});
		}

		public void TestCheckCLP_DIFRefNumberOrLocation()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			RegistrationNumberHelperTest.PrepareGlobalData(Factory);

			var pgaHeader = invoiceLine.ECCCPGAHeader;
			var lpco = pgaHeader.LPCOViews.AddNew();

			CombineAssertions(() =>
			{
				var documentTypes = new ZString[]
				{
					LPCODocumentTypeQualifier.Codes._8020,
					LPCODocumentTypeQualifier.Codes._8021,
					LPCODocumentTypeQualifier.Codes._8022,
					LPCODocumentTypeQualifier.Codes._8023
				};

				foreach (var documentType in documentTypes)
				{
					lpco.CLP_Type = documentType;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(lpco.CLP_DIFRefNumberOrLocationInfo);
				}
			});
			lpco.CLP_Type = "1A3";
			lpco.CLP_DIFRefNumberOrLocation = "abcde123456789";
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, "DIF URN must be numeric and should be 14 digits");

			lpco.CLP_DIFRefNumberOrLocation = "a12345678912345";
			AssertHasMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, "DIF URN must be numeric and should be 14 digits");

			lpco.CLP_DIFRefNumberOrLocation = "12345671234567";
			AssertNoMessageErrorContaining(lpco.CLP_DIFRefNumberOrLocationInfo, "DIF URN must be numeric and should be 14 digits");
		}

		public void TestCheckCLP_UQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = Customs.Business.YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.GACPGAHeader;
			var lpco = pgaHeader.LPCOViews.AddNew();
			lpco.CLP_AlternativeQuotaQuantity = 10.32m;

			ValidationTestHelper.AssertInvalidCodeMessageError(lpco.CLP_AlternativeQuotaUQInfo, "~", "CLT");
		}

		public void TestCheckCLP_HolderContactEmail()
		{
			gACLPCO.CLP_IsHolderOverridden = true;
			gACLPCO.CLP_HolderContactEmail = "111";
			AssertHasMessageError(gACLPCO.CLP_HolderContactEmailInfo, "Holder Contact Email Address is not valid.");

			gACLPCO.CLP_HolderContactEmail = "111@test.com";
			AssertNoMessageErrors(gACLPCO.CLP_HolderContactEmailInfo);
		}

		public void TestCheckCLP_ApplicantContactEmail()
		{
			gACLPCO.CLP_IsApplicantOverridden = true;
			gACLPCO.CLP_ApplicantContactEmail = "111";
			AssertHasMessageError(gACLPCO.CLP_ApplicantContactEmailInfo, "Applicant Contact Email Address is not valid.");

			gACLPCO.CLP_ApplicantContactEmail = "111@test.com";
			AssertNoMessageErrors(gACLPCO.CLP_ApplicantContactEmailInfo);
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
			invoiceLine.CA_CNSCInd = Customs.Business.YesNoList.Codes.Yes;
			invoiceLine.CA_GACInd = Customs.Business.YesNoList.Codes.Yes;
			invoiceLine.CA_PHACInd = Customs.Business.YesNoList.Codes.Yes;
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;

			this.cNSCHeader = invoiceLine.CNSCPGAHeader;
			this.cNSCLPCO = cNSCHeader.LPCOViews.AddNew();
			this.gACCHeader = invoiceLine.GACPGAHeader;
			this.gACLPCO = gACCHeader.LPCOViews.AddNew();
			this.pHACHeader = invoiceLine.PHACPGAHeader;
			pHACHeader.LPCOViews.AddNew();
			this.cFIAHeader = invoiceLine.CFIAPGAHeader;
			cFIAHeader.LPCOViews.AddNew();
		}

		LPCOView cNSCLPCO;
		CNSCPGAHeader cNSCHeader;
		LPCOView gACLPCO;
		GACPGAHeader gACCHeader;
		PHACPGAHeader pHACHeader;
		CFIAPGAHeader cFIAHeader;

		#endregion
	}
}
