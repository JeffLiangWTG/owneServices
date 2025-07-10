using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.CA.Business.Testing
{
	class ImportJobComInvoiceHeaderValidationTest : CommonImportJobComInvoiceHeaderValidationTest
	{
		public void TestCheckJZ_NoOfPacks()
		{
			var message = "Merge has not occurred.\r\nPlease ensure this invoice or every line must have packages ticked and each amount must be greater than zero.\r\nThen save the job and try again.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();

			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();
			var package = packageGroup.Packages.AddNew();
			package.CW_PackQty = 20;

			var invoice = declaration.Invoices.AddNew();
			invoice.PackagesPivot.RemoveAndDeleteAll();

			invoice.JZ_NoOfPacks = 0;

			declaration.CA_RequiresMerge = true;
			invoice.Validation.ValidateJZ_NoOfPacks();
			AssertHasMessageError("Should have the message as the total package count of invoice is 0.", invoice.JZ_NoOfPacksInfo, message);

			invoice.JZ_NoOfPacks = 3;

			declaration.CA_RequiresMerge = true;
			invoice.Validation.ValidateJZ_NoOfPacks();
			AssertNoMessageError("Should not have the message as the total package count of invoice is not 0.", invoice.JZ_NoOfPacksInfo, message);

			invoice.JZ_NoOfPacks = 0;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;

			declaration.CA_RequiresMerge = true;
			invoice.Validation.ValidateJZ_NoOfPacks();
			AssertNoMessageError("Should not have the message as the declaration does not support package on invoice or invoice line.", invoice.JZ_NoOfPacksInfo, message);

			message = "Please ensure this invoice or every line must have packages ticked and each amount must be greater than zero.";

			invoice.JZ_NoOfPacks = 0;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			declaration.CA_RequiresMerge = false;
			invoice.Validation.ValidateJZ_NoOfPacks();
			AssertHasMessageError("Should have the message as the total package count of invoice is not 0 and the declaration doesn't require merge.", invoice.JZ_NoOfPacksInfo, message);
		}

		public void TestNoExceptionThrownWhenValidateJZ_InvoiceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			groupInvoice.JZ_GroupInvoice = ZBool.True;
			groupInvoice.JZ_JE = declaration.PK;
			groupInvoice.JZ_InvoiceNumber = "All Invoices";
			groupInvoice.JZ_InvoiceDate = new ZDate(2020, 7, 17);
			groupInvoice.JZ_GB = Guid.Empty;

			var invoiceCA = Factory.New<JobComInvoiceHeader>();
			invoiceCA.JZ_InvoiceNumber = "CA1707";
			invoiceCA.JZ_JE = declaration.PK;
			invoiceCA.JZ_JZ_GroupInvoiceFK = groupInvoice.PK;
			invoiceCA.JZ_StandAloneInvoiceDirection = "IMP";
			invoiceCA.JZ_GB = GlbBranch.CurrentBranch.PK;
			invoiceCA.JZ_IncoTerm = "FOB";

			AssertNoExceptionThrown("CA Should not make an exception in ShouldCheckDuplicate.", () => invoiceCA.Validation.ValidateJZ_InvoiceNumber());
			ITemplateCopyable template = invoiceCA;
			AssertNoExceptionThrown("CA Should not make an exception in ShouldCheckDuplicate.", () => template.TemplateCopy());
		}

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestValidateJZ_InvoiceNumberForDuplicateNotOnIM2()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "ORG1";

			var org2 = OrgHeader.New(Factory);
			org2.OH_Code = "ORG2";

			var baseDec = Factory.NewWithValidTestData<JobDeclaration>();
			baseDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			baseDec.JE_DeclarationReference = "B00000001";
			baseDec.JE_OH_Supplier = org1.PK;
			baseDec.CA_K84AccountingDate = new ZDateTime(2016, 01, 01);
			var header = baseDec.Invoices.AddNew();
			header.JZ_InvoiceNumber = "INVNO";
			Factory.Save();
			AssertEquals("Does not have warning", false, header.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining("This invoice number already exists in job(s): "));

			var copied1 = baseDec.GetNewCopyToB2Declaration();
			copied1.CA_B2AcceptedDate = new ZDateTime(2016, 01, 01);
			Factory.Save();
			var header1 = copied1.Invoices.First();
			AssertEquals("INVNO", header1.JZ_InvoiceNumber);
			AssertEquals("Does not have warning", false, header1.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining("This invoice number already exists in job(s): B00000001"));

			var copied2 = baseDec.GetNewCopyToB2Declaration();
			var header2 = copied2.Invoices.First();
			AssertEquals("INVNO", header2.JZ_InvoiceNumber);
			AssertEquals("Does not have warning", false, header2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining("This invoice number already exists in job(s): B00000001"));
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			invoiceHeader.Validation.ValidateJZ_InvoiceNumber();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.JZ_InvoiceNumber = "123456";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			var testDec1 = JobDeclaration.New(Factory);
			testDec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec1.JE_DeclarationReference = "B00000001";
			var invoice1 = testDec1.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "111";
			AssertNoNotifications(invoice1.JZ_InvoiceNumberInfo);

			var testDec2 = JobDeclaration.New(Factory);
			testDec2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDec2.JE_DeclarationReference = "B00000002";
			var invoice2 = testDec2.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "222";
			AssertNoNotifications(invoice2.JZ_InvoiceNumberInfo);

			Factory.Save();

			var testDec3 = JobDeclaration.New(Factory);
			testDec3.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDec3.JE_DeclarationReference = "B00000003";
			var invoice3 = testDec3.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "222";
			var messageError = "This LVS ID has already been used on job(s): B00000002";
			AssertHasWarningContaining(invoice3.JZ_InvoiceNumberInfo, messageError);
			invoice3.JZ_InvoiceNumber = "111";
			AssertNoWarningContaining(invoice3.JZ_InvoiceNumberInfo, messageError);
			AssertNoMessageErrors(invoice3.JZ_InvoiceNumberInfo);
			var invoice4 = testDec3.Invoices.AddNew();
			invoice4.JZ_InvoiceNumber = "111";
			messageError = "This LVS ID has already been used on this job";
			AssertHasMessageErrorContaining(invoice4.JZ_InvoiceNumberInfo, messageError);
			invoice4.JZ_InvoiceNumber = "333";
			AssertNoMessageErrorContaining(invoice4.JZ_InvoiceNumberInfo, messageError);
			AssertNoMessageErrors(invoice4.JZ_InvoiceNumberInfo);

			var testDec5 = JobDeclaration.New(Factory);
			testDec5.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			testDec5.JE_DeclarationReference = "B00000005";
			var invoice5 = testDec5.Invoices.AddNew();
			invoice5.Validation.ValidateJZ_InvoiceNumber();
			AssertHasMessageErrorContaining(invoice5.JZ_InvoiceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			invoice5.JZ_InvoiceNumber = "222";
			messageError = "This LVS ID has already been used on job(s): B00000002";
			AssertHasWarningContaining(invoice5.JZ_InvoiceNumberInfo, messageError);
			invoice5.JZ_InvoiceNumber = "555";
			AssertNoNotifications(invoice5.JZ_InvoiceNumberInfo);

			var testDec6 = JobDeclaration.New(Factory);
			testDec6.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			testDec6.JE_DeclarationReference = "B00000006";
			var invoice6 = testDec5.Invoices.AddNew();
			invoice6.JZ_InvoiceNumber = "222";
			messageError = "This LVS ID has already been used on job(s): B00000002";
			AssertHasWarningContaining(invoice6.JZ_InvoiceNumberInfo, messageError);
			invoice6.JZ_InvoiceNumber = "666";
			AssertNoNotifications(invoice6.JZ_InvoiceNumberInfo);

			var testDec7 = Factory.NewWithValidTestData<JobDeclaration>();
			testDec7.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec7.CA_ServiceOption = "IID";
			testDec7.JE_DeclarationReference = "B00000001";
			var header = testDec7.Invoices.AddNew();
			header.JZ_InvoiceNumber = "INVNO";
			var release = testDec7.ReleaseStatuses.AddNew();
			release.RL_CargoControlNumber = "00001";
			var release2 = testDec7.ReleaseStatuses.AddNew();
			release2.RL_CargoControlNumber = "00002";
			header.Validation.ValidateJZ_InvoiceNumber();
			AssertHasMessageErrorContaining(header.JZ_InvoiceNumberInfo, "Please enter at least one CCN in the Cargo Control Numbers grid when there are multiple CCNs entered on the Packing tab > Cargo Control Numbers");

			var headerCCN = header.CargoControlNumbersList.AddNew();
			headerCCN.J2_ReferenceNumber = "00001";
			header.Validation.ValidateJZ_InvoiceNumber();
			AssertNoMessageErrorContaining(header.JZ_InvoiceNumberInfo, "Please enter at least one CCN in the Cargo Control Numbers grid when there are multiple CCNs entered on the Packing tab > Cargo Control Numbers");

			testDec7.ReleaseStatuses.RemoveAndDelete(testDec7.ReleaseStatuses.First());
			AssertEquals(1, testDec7.ReleaseStatuses.Count);
			header.CargoControlNumbersList.DeleteAll();
			header.Validation.ValidateJZ_InvoiceNumber();
			AssertNoMessageErrorContaining(header.JZ_InvoiceNumberInfo, "Please enter at least one CCN in the Cargo Control Numbers grid when there are multiple CCNs entered on the Packing tab > Cargo Control Numbers");
		}

		public void TestCheckJZ_OH_Buyer()
		{
			var orgHeader1 = new DeclarationTestHelper(Factory, false).CreateOrganisation("A", "CAAAA").PK;
			var orgHeader2 = new DeclarationTestHelper(Factory, false).CreateOrganisation("B", "CABBB").PK;

			invoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;

			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			invoiceHeader.JobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceHeader.JobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceHeader.JobDeclaration.JE_MessageSubType = LowValueShipmentsTypes.Codes.TotalConsolidation;
			invoiceHeader.JZ_OH_Buyer = orgHeader1;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceHeader.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceHeader.JZ_OH_Buyer = orgHeader1;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, MandatoryValidation.YouHaveNotEntered);

			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvsJob.JE_MessageSubType = LowValueShipmentsTypes.Codes.ConsolidationByImporter;
			lvsJob.JE_OH_Importer = orgHeader1;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoiceHeader, lvsJob);
			CAAddressValidatorTest.AssertMainAddressUsesCAAddressValidationIfOrgSpecified(invoiceHeader.JZ_OH_BuyerInfo, "Purchaser Main");
			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, "should match the header importer");
			invoiceHeader.JZ_OH_Buyer = orgHeader2;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, "should match the header importer");
			invoiceHeader.JZ_OH_Buyer = orgHeader1;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_OH_BuyerInfo, "should match the header importer");
		}

		public virtual void TestCheckJZ_OH_BuyerWithOrgOnCreditHold()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = ZBool.True;
			org.CompanyData.OB_AROnCreditHold = ZBool.True;
			Factory.Save();
			Assert("IsCreditOnHold", org.CreditChecker.IsCreditOnHold());

			var lvxJob = Factory.NewWithValidTestData<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var lvxInvoice = lvxJob.LVXInvoiceHeader;

			lvxInvoice.JZ_OH_Buyer = org.PK;
			lvxInvoice.Validation.ValidateJZ_OH_Buyer();
			AssertHasWarningContaining(lvxInvoice.JZ_OH_BuyerInfo, "TestOrg is on Credit Hold.");
		}

		public void TestCheckJZ_RN_NKDefaultOrigin()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.JZ_RN_NKDefaultOriginInfo, "11", Constants.CountryCodes.Canada);
		}

		public void TestCheckJZ_InvoiceDate()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceHeader.JZ_InvoiceDateInfo, ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.JZ_InvoiceDateInfo);

			var now = ZDateTime.Now;
			invoiceHeader.JZ_InvoiceDate = now.AddDays(2);
			AssertHasWarning(invoiceHeader.JZ_InvoiceDateInfo, ImportJobComInvoiceHeaderValidation.InvoiceDateIsInThefuture);
			invoiceHeader.JZ_InvoiceDate = now.AddDays(-2);
			AssertNoWarning(invoiceHeader.JZ_InvoiceDateInfo, ImportJobComInvoiceHeaderValidation.InvoiceDateIsInThefuture);

			invoiceHeader.JZ_ValuationDateOverride = now.AddDays(-4);
			invoiceHeader.JZ_InvoiceDate = now.AddDays(-2);
			AssertHasWarning(invoiceHeader.JZ_InvoiceDateInfo, ImportJobComInvoiceHeaderValidation.InvoiceDateCannotBeGreaterThanDirectShipmentDate);
			invoiceHeader.JZ_InvoiceDate = now.AddDays(-6);
			AssertNoWarning(invoiceHeader.JZ_InvoiceDateInfo, ImportJobComInvoiceHeaderValidation.InvoiceDateCannotBeGreaterThanDirectShipmentDate);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.Validation.ValidateJZ_InvoiceDate();
			});
		}

		public void TestCheckJZ_Weight()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_WeightInfo, MandatoryValidation.ValueCannotBeZero);
			invoiceHeader.JZ_Weight = -123m;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			invoiceHeader.JZ_Weight = 123m;
			AssertNoMessageErrors(invoiceHeader.JZ_WeightInfo);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entry = Factory.New<CusEntryHeaderTestHelper>();
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry.CustomsValueExposed = JobComInvoiceHeader.VFDLimit;
			declaration.CustomsEntryHeaders.Add(entry);
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceHeader.JZ_Weight = 123m;
			AssertNoMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			invoiceHeader.JZ_Weight = 0m;
			AssertHasMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertNoMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertHasMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			entry.CustomsValueExposed = 2499;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertNoMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			entry.CustomsValueExposed = 2501;
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.PuertoRico;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertHasMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.UnitedStatesMinorIslands;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertHasMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.VirginIslands;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertHasMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.Mexico;
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertNoMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			invoiceHeader.CA_TradeZone = "10";
			invoiceHeader.Validation.ValidateJZ_Weight();
			AssertHasMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);
			invoiceHeader.JZ_Weight = 123m;
			AssertNoMessageError(invoiceHeader.JZ_WeightInfo, ImportJobComInvoiceHeaderValidation.GrossWeightRequired);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.Validation.ValidateJZ_Weight();
			});
		}

		public void TestCheckJZ_WeightUQ()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.JZ_WeightUQInfo, "12", "KG");
			invoiceHeader.JZ_Weight = 123m;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_WeightUQInfo);
			invoiceHeader.JZ_Weight = 0m;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_WeightUQ = ZString.Empty;
			AssertNoMessageErrors(invoiceHeader.JZ_WeightUQInfo);
		}

		public override void TestValidateAbsenceOfOFTOrONS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			invoice.RunPreSaveValidation();
			AssertEquals(false, invoice.JZ_Calc_CIFAmountInfo.HasNotifications());
		}

		public override void TestCheckJZ_ValuationDateOverride()
		{
			base.TestCheckJZ_ValuationDateOverride();
			var messageError = "Direct Shipment Date, which is required when no ATD is entered on the Declaration tab.";
			var declaration = base.declaration;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			base.declaration.JE_ExportDate = ZDateTime.Empty;
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;
			AssertEquals("only 1 message error", 1, invoiceHeader.JZ_ValuationDateOverrideInfo.Notifications.Count());
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);

			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AppraisalQualityData;
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);
			declaration.CA_AssesmentOption = AssessmentOptions.Codes.AQtoFollow;
			invoiceHeader.Validation.ValidateJZ_ValuationDateOverride();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			declaration.JE_ExportDate = ZDateTime.Now;
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);

			messageError = "Direct Shipment Date, which is required for LVS calculations.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_ExportDate = ZDateTime.Empty;
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_ValuationDateOverrideInfo, messageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(5);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(6);
			AssertNoMessageError(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.DirectShipmentDateCannotBeGreaterThanRelaseDate);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddDays(5);
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(6);
			AssertHasMessageError(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.DirectShipmentDateCannotBeGreaterThanRelaseDate);

			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Now.AddDays(4);
			AssertNoMessageError(invoiceHeader.JZ_ValuationDateOverrideInfo, ImportJobComInvoiceHeaderValidation.DirectShipmentDateCannotBeGreaterThanRelaseDate);

			var now = ZDateTime.Now;
			invoiceHeader.JZ_ValuationDateOverride = now.AddDays(-4);
			invoiceHeader.JZ_InvoiceDate = now.AddDays(-2);
			AssertHasWarning(invoiceHeader.JZ_InvoiceDateInfo, ImportJobComInvoiceHeaderValidation.InvoiceDateCannotBeGreaterThanDirectShipmentDate);
			invoiceHeader.JZ_InvoiceDate = now.AddDays(-6);
			AssertNoWarning(invoiceHeader.JZ_InvoiceDateInfo, ImportJobComInvoiceHeaderValidation.InvoiceDateCannotBeGreaterThanDirectShipmentDate);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.Validation.ValidateJZ_ValuationDateOverride();
			});
		}

		public void TestCheckJZ_ValuationDateOverride_MandatoryValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			AssertNoMessageErrors(invoiceHeader.JZ_ValuationDateOverrideInfo);

			invoiceHeader.CA_RL_NKLastPort = "AUSYD";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_ValuationDateOverrideInfo);

			invoiceHeader.CA_RL_NKLastPort = string.Empty;

			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.CA_GACInd = Enterprise.Customs.Business.YesNoList.Codes.Yes;

			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceHeader.JZ_ValuationDateOverrideInfo);
		}

		public void TestValidateTotalValueForDutyForLVS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var taxOrFee = helper.CreateTaxOrFee(Enterprise.Customs.Universal.Constants.RefCusTaxOrFeeTypes.Deminimus, 1650, Core.Constants.CountryCodes.Canada, new ZDateTime(2013, 01, 07), new ZDateTime(2079, 06, 06));
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;

			var messageError = "Total value for duty of Low Value Shipment should not exceed 1650 CAD, but it is 2000 CAD. If the entered amount is wrong then correct it and then run apportionment from the brokerage menu (if available) or press the Calculate Duty button if using the wizard.";
			invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today;
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.CA_CustomsValueOvr = true;
			line.CA_CustomsValue = 2000;
			invoiceHeader.Validation.ValidateAll();
			AssertHasError(invoiceHeader.TotalValueForDutyInfo, messageError);

			line.CA_CustomsValue = 1000;
			invoiceHeader.Validation.ValidateAll();
			AssertNoError(invoiceHeader.TotalValueForDutyInfo, messageError);

			messageError = "Total value for duty of Low Value Shipment should not exceed 1650 CAD, but it is 3500 CAD. If the entered amount is wrong then correct it and then run apportionment from the brokerage menu (if available) or press the Calculate Duty button if using the wizard.";
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 01, 05);
			line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.CA_CustomsValueOvr = true;
			line.CA_CustomsValue = 2500;
			invoiceHeader.Validation.ValidateAll();
			AssertNoError(invoiceHeader.TotalValueForDutyInfo, messageError);

			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2013, 01, 08);
			invoiceHeader.Validation.ValidateAll();
			AssertHasError(invoiceHeader.TotalValueForDutyInfo, messageError);
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;

			var invoice = declaration.LVXInvoiceHeader;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoice.JZ_InvoiceAmountInfo);

			invoice.JZ_InvoiceAmount = 100.121m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_LinePrice = 100.124m;

			AssertNoMessageError(invoice.JZ_InvoiceAmountInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);
			AssertNoMessageError(invoice.JZ_Calc_BalanceInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			invoiceLine.JI_LinePrice = 100.128m;
			declaration.ResumeApportionment();

			AssertHasMessageError(invoice.JZ_InvoiceAmountInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);
			AssertNoMessageError(invoice.JZ_Calc_BalanceInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			declaration.IsSimplifiedLVSMode = true;
			invoiceLine.JI_LinePrice = 100.124m;
			declaration.ResumeApportionment();

			AssertNoMessageError(invoice.JZ_InvoiceAmountInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);
			AssertNoMessageError(invoice.JZ_Calc_BalanceInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			invoiceLine.JI_LinePrice = 100.128m;
			declaration.ResumeApportionment();

			AssertHasMessageError(invoice.JZ_InvoiceAmountInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);
			AssertNoMessageError(invoice.JZ_Calc_BalanceInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);

			invoice.JZ_InvoiceAmount = 0m;
			declaration.ResumeApportionment();

			AssertNoMessageError(invoice.JZ_InvoiceAmountInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);
			AssertNoMessageError(invoice.JZ_Calc_BalanceInfo, Enterprise.Customs.Business.InvoiceHeaderValidation.UnbalancedInvoiceMessage);
		}

		public void TestCheckJZ_NetWeight()
		{
			invoiceHeader.JZ_NetWeight = -1m;
			AssertHasErrorContaining(invoiceHeader.JZ_NetWeightInfo, "greater than or equal to 0.");
			invoiceHeader.JZ_NetWeight = 1m;
			AssertNoErrorContaining(invoiceHeader.JZ_NetWeightInfo, "greater than or equal to 0.");
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.JZ_NetWeightUQInfo, "??", "KG");
			invoiceHeader.JZ_NetWeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.JZ_NetWeightUQInfo, "must be entered.");
			invoiceHeader.JZ_NetWeightUQ = "T";
			AssertNoMessageErrorContaining(invoiceHeader.JZ_NetWeightUQInfo, "must be entered.");
			invoiceHeader.JZ_NetWeight = 0m;
			invoiceHeader.JZ_NetWeightUQ = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceHeader.JZ_NetWeightUQInfo, "must be entered.");
		}

		public void TestCheckJZ_OA_ManufacturerAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "123ABC";
			org.OH_RL_NKClosestPort = "CABLO";
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_CompanyNameOverride = "Company";
			org.MainAddress.OA_PostCode = ZString.Empty;
			invoiceHeader.ManufacturerOrgPK = org.PK;
			invoiceHeader.JZ_OA_ManufacturerAddress = org.MainAddress.PK;
			invoiceHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "Manufacturer");
			AssertNoWarning(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			org.OH_FullName = "123ABC– ";
			invoiceHeader.Validation.ValidateAll();
			AssertHasWarning(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			org.MainAddress.OA_PostCode = "A2B2C2";
			invoiceHeader.Validation.ValidateAll();
			AssertNoMessageErrorContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "Manufacturer");

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertNoMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_HCInd = Customs.Business.YesNoList.Codes.Yes;
			var hcPgaHeader = invoiceLine.HCPGAHeader;
			hcPgaHeader.CA_PESProgramInd = Customs.Business.YesNoList.Codes.Yes;
			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertHasMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = org.Contacts.AddNew();
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";

			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertNoMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "15850503354";

			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertNoMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			hcPgaHeader.CA_PESProgramInd = Customs.Business.YesNoList.Codes.No;
			hcPgaHeader.CA_CPRProgramInd = Customs.Business.YesNoList.Codes.Yes;

			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertNoWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertHasWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactEmailIsRecommended);

			contact.OC_Phone = "";
			contact.OC_Email = "test@test.com";

			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertHasWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertNoWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactEmailIsRecommended);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.Validation.ValidateJZ_OA_ManufacturerAddress();
			});

			standAloneInvoice.JZ_OA_ManufacturerAddress = org.MainAddress.PK;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.Validation.ValidateJZ_OA_ManufacturerAddress();
			});
		}

		public void TestCheckJZ_OA_ManufacturerAddressForOGDTC()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var mDocumentaryAddress = manufacturer.Addresses.AddNew();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OA_ManufacturerAddress = mDocumentaryAddress.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDTC = true;
			invoiceHeader.Validation.ValidateAll();
			AssertNoMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "Manufacturer is required for Transport Canada (Tires) shipments");
			invoiceHeader.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			AssertHasMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "Manufacturer is required for Transport Canada (Tires) shipments");
			declaration.CA_OGDTC = false;
			invoiceHeader.Validation.ValidateAll();
			AssertNoMessageError(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "Manufacturer is required for Transport Canada (Tires) shipments");
		}

		protected override ZString GetMessageType()
		{
			return JobMessageTypeList.Codes.Import;
		}

		protected override Type GetTypeForTest()
		{
			return typeof(ImportJobComInvoiceHeaderValidation);
		}
	}
}
