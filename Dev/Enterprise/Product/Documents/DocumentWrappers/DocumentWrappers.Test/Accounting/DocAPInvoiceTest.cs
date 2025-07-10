using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAPInvoice))]
	sealed class DocAPInvoiceTest : DocARInvoiceCommonTest
	{
		// Methods
		protected override DocARBaseInvoice GetBaseInvoiceWrapper()
		{
			return DocAPInvoice.New(base.Invoice, base.Factory);
		}

		protected override void TearDown()
		{
		}

		DocAPInvoice GetInvoiceWrapper()
		{
			return DocAPInvoice.New(APInvoice, Factory);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocAPInvoice.New(APInvoice, Factory) };
		}

		protected override TransactionHeader GetWrappedInvoice()
		{
			return Factory.New<APInvoice>();
		}

		public void TestUACostConfirmationHeadingText()
		{
			var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_TransactionNum = "0110112";
			Factory.Save();

			var invoiceWrapper = DocAPInvoice.New(uaInvoice, Factory);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AssertEquals("Cost Confirmation Heading Text", "Test Value!", invoiceWrapper.CostConfirmationHeadingText);
		}

		public void TestInvoiceWithApprovalRequestCostConfirmationHeadingText()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APInvoice>(TestObjectCreator.Creditor1, 100);
			var invoiceWrapper = DocAPInvoice.New(invoice, Factory);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AssertEquals("Test Value!", invoiceWrapper.CostConfirmationHeadingText);

			invoice.MoveFromIncompleteToPayableLedger();
			AssertEquals("Test Value!", invoiceWrapper.CostConfirmationHeadingText);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.CostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Posted Value!");
			AssertEquals("Posted Value!", invoiceWrapper.CostConfirmationHeadingText);

			var creditNote = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APCreditNote>(TestObjectCreator.Creditor1, 100);
			invoiceWrapper = DocAPInvoice.New(creditNote, Factory);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Another Test Value!");
			AssertEquals("Another Test Value!", invoiceWrapper.CostConfirmationHeadingText);

			creditNote.MoveFromIncompleteToPayableLedger();
			AssertEquals("Another Test Value!", invoiceWrapper.CostConfirmationHeadingText);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.CostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Another Posted Value!");
			AssertEquals("Another Posted Value!", invoiceWrapper.CostConfirmationHeadingText);
		}

		public void TestInvoiceCreatedToPreviewRequestCostConfirmationHeadingText()
		{
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AccountingConfigurationRegistry.Instance.CostConfirmationHeadingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Posted Value!");

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var invoiceWrapper = DocAPInvoice.New(invoice, Factory);
			AssertEquals("Posted Value!", invoiceWrapper.CostConfirmationHeadingText);

			invoice.SetContext(BusinessContext.UnapprovedAPInvoiceCreatedForRequestPreview);
			AssertEquals("Test Value!", invoiceWrapper.CostConfirmationHeadingText);
		}

		public void TestUACostConfirmationDocumentTitle()
		{
			var uaInvoice = Factory.NewWithValidTestData<UAInvoice>();
			uaInvoice.AH_TransactionNum = "0110112";
			Factory.Save();

			var invoiceWrapper = DocAPInvoice.New(uaInvoice, Factory);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AssertEquals("Cost Confirmation Document Title", "Test Value!", invoiceWrapper.CostConfirmationDocumentTitle);
		}

		public void TestInvoiceWithApprovalRequestCostConfirmationDocumentTitle()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APInvoice>(TestObjectCreator.Creditor1, 100);
			var invoiceWrapper = DocAPInvoice.New(invoice, Factory);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AssertEquals("Test Value!", invoiceWrapper.CostConfirmationDocumentTitle);

			invoice.MoveFromIncompleteToPayableLedger();
			AssertEquals("Test Value!", invoiceWrapper.CostConfirmationDocumentTitle);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Posted Value!");
			AssertEquals("Posted Value!", invoiceWrapper.CostConfirmationDocumentTitle);

			var creditNote = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APCreditNote>(TestObjectCreator.Creditor1, 100);
			invoiceWrapper = DocAPInvoice.New(creditNote, Factory);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Another Test Value!");
			AssertEquals("Another Test Value!", invoiceWrapper.CostConfirmationDocumentTitle);

			creditNote.MoveFromIncompleteToPayableLedger();
			AssertEquals("Another Test Value!", invoiceWrapper.CostConfirmationDocumentTitle);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Another Posted Value!");
			AssertEquals("Another Posted Value!", invoiceWrapper.CostConfirmationDocumentTitle);
		}

		public void TestInvoiceCreatedToPreviewRequestCostConfirmationDocumentTitle()
		{
			AccountingConfigurationRegistry.Instance.UnapprovedInvoiceCostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test Value!");
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentTitle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Posted Value!");
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var invoiceWrapper = DocAPInvoice.New(invoice, Factory);
			AssertEquals("Posted Value!", invoiceWrapper.CostConfirmationDocumentTitle);

			invoice.SetContext(BusinessContext.UnapprovedAPInvoiceCreatedForRequestPreview);
			AssertEquals("Test Value!", invoiceWrapper.CostConfirmationDocumentTitle);
		}

		public override void TestRecipientTaxIDForEUWithEmptyCode()
		{
			AssertRecipientTaxId(Core.Constants.CountryCodes.Austria, "", "", "Client VAT #:", "", "DEBER");
			AssertRecipientTaxId(Core.Constants.CountryCodes.Netherlands, "", "", "Client VAT #:", "", "DEBER");
		}

		public new void TestRecipientTaxID()
		{
			ZString str = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				SetupCodesForRecipientTaxIdTest(orgProxy, Constants.CountryCodes.UnitedKingdom, Country.GetConsumptionTaxDescription("GB"), "123456", "GBLON");
				orgProxy.LocalBusinessRegNo = "ABC123";
				orgProxy.OH_RL_NKClosestPort = "AUSYD";
				AssertEquals("PreCondition: Local Business Reg Number", "ABC123", orgProxy.LocalBusinessRegNo);

				orgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.VATCode, "VAT12345");
				SetupNewInvoice(orgProxy, false);

				Invoice.Header.OH_RL_NKClosestPort = "AUAMT";
				AssertEquals("Recipient Tax ID Heading when not a South African Company", "Recipient Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when not a South African Company", "ABC123", InvoiceWrapper.RecipientTaxIDNumber);

				Invoice.Header.OH_RL_NKClosestPort = "ZAPRF";
				AssertEquals("Recipient Tax ID Heading for a South African Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a South African Company", orgProxy.LocalBusinessRegNo, InvoiceWrapper.RecipientTaxIDNumber);

				Invoice.Header.OH_RL_NKClosestPort = "TWTOF";
				AssertEquals("Recipient Tax ID Heading for a Taiwan Company", "Client Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Taiwan Company", "VAT12345", InvoiceWrapper.RecipientTaxIDNumber);

				SetupNewInvoice(orgProxy, true);

				Invoice.Header.OH_RL_NKClosestPort = "GBRAY";
				Invoice.Company.OrgProxy.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "GB")), "123456");
				AssertEquals("Recipient Tax ID Heading for a European Union Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a European Union Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				SetupNewInvoice(orgProxy, true);
				Invoice.Header.OH_RL_NKClosestPort = "PHCLB";
				Invoice.Company.OrgProxy.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "PH")), "789012");
				AssertEquals("Recipient Tax ID Heading for a Philippines Company", "TIN:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Philippines Company", "789012", InvoiceWrapper.RecipientTaxIDNumber);

				//India
				string gstRegNumber = "GST1234";
				var header = Factory.New<OrgHeader>();
				header.OH_RL_NKClosestPort = "INBOM";
				header.OH_Code = "OH123";
				var taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.India;
				taxCode.OK_CodeType = "GST";
				taxCode.OK_CustomsRegNo = gstRegNumber;
				Factory.Save();

				APInvoice.AH_OH = header.PK;
				InvoicingLineBase line2 = (InvoicingLineBase)APInvoice.Lines.AddNew();
				AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;
				InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a India Company", "Client GSTIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a India Company", gstRegNumber, InvoiceWrapper.RecipientTaxIDNumber);

				string uinRegNumber = "UIN1234";
				var taxCode1 = header.CustomsCodes.AddNew();
				taxCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
				taxCode1.OK_CodeType = "UIN";
				taxCode1.OK_CustomsRegNo = uinRegNumber;

				Factory.Save();

				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a India Company - if both GST and UIN is present still the system should retrun GST as RecipientTaxID", "Client GSTIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Heading for a India Company - if both GST and UIN is present still the system should retrun GST as RecipientTaxID", gstRegNumber, InvoiceWrapper.RecipientTaxIDNumber);

				taxCode.Delete(); //deleting GST cuscode for India
				Factory.Save();

				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a India Company - in absence of GST, UIN should be retruned as RecipientTaxID", "Client UIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Heading for a India Company - in absence of GST, UIN should be retruned as RecipientTaxID", uinRegNumber, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry((string)str);
			}
		}

		public override void TestPapuaNewGuineaRecipientTaxIDBehaviours()
		{
			Assert("Recipient Tax ID behaviours are not required in AP Invoice for PNG", true);
		}

		protected override void AssertRecipientTaxIdWithDisplayRecipientTaxIDHeading(string expectedTaxIDNumber)
		{
			//This registry is not used here.
		}

		protected override void SetupCodesForRecipientTaxIdTest(OrgHeader header, string countryCode, string cusCodeUsedForVAT, string cusRegNumber, string closestPort)
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			base.SetupCodesForRecipientTaxIdTest(orgProxy, countryCode, cusCodeUsedForVAT, cusRegNumber, closestPort);
			header.OH_RL_NKClosestPort = closestPort;
		}

		public new void TestRecipientTaxID_LoginCountryNotEqualRecipientCountryOfRegistration()
		{
			var storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				var header = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				header.OH_RL_NKClosestPort = "AUSYD";
				header.LocalBusinessRegNo = "ABC123";

				var taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
				taxCode.OK_CustomsRegNo = "123456";

				SetupNewInvoice(header, false);

				//In the Same Country
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				Invoice.Header.OH_RL_NKClosestPort = "AUSYD";

				AssertEquals("Recipient Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("ABC123", InvoiceWrapper.RecipientTaxIDNumber);

				//In Another Country
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
				Invoice.Header.OH_RL_NKClosestPort = "AUSYD";
				Invoice.Header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("ABC123", InvoiceWrapper.RecipientTaxIDNumber);

				//In UK
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				Invoice.Header.OH_RL_NKClosestPort = "AUSYD";
				Invoice.Header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals(string.Empty, InvoiceWrapper.RecipientTaxIDNumber);

				Invoice.Header.OH_RL_NKClosestPort = "GBLON";
				Invoice.Header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("GB123456", InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public new void TestAccountOrgAddress()
		{
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_FullName = "Creditor";

			OrgAddress creditorAPAddress = creditor.Addresses.AddNew(OrgAddressType.Payables, true);
			creditorAPAddress.OA_Address1 = "Creditor AP ADDRESS";

			InvoiceWrapper = GetInvoiceWrapper();
			AssertNull(InvoiceWrapper.AccountOrgAddress);

			Invoice.AH_OH = creditor.PK;
			AssertEquals("Non-Job Related Invoice. Should be creditor's AP Address", creditorAPAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			Job job = Factory.NewJobForTesting<Job>();
			Invoice.AH_JH = job.PK;
			AssertEquals("Job Related Invoice. Should be creditor's AP Address", creditorAPAddress.OA_Address1, InvoiceWrapper.AccountOrgAddress.Address1);

			OrgAddress newAddress = Factory.NewWithValidTestData<OrgAddress>();
			Invoice.AH_OA_InvoiceAddressOverride = newAddress.PK;
			AssertEquals("Should be InvoiceAddressOverride", newAddress.PK, InvoiceWrapper.AccountOrgAddress.OrgAddress.PK);
		}

		[ExpectNoExceptions]
		public new void TestGetICountryComplianceInfo_DifferentCountryCodePassed_WhenRecipientTaxIDHeadingCalled()
		{
			Assert("Not applicable", true);
		}

		public new void TestRecipientTaxIDHeading_GetRecipientTaxIDHeading_NotCalledWhenIsNotTaxed()
		{
			Assert("Not applicable", true);
		}

		public new void TestRecipientTaxIDHeading_GetRecipientTaxIDHeading_NotCalledWhenDisplayRecipientTaxIDIsFalse()
		{
			Assert("Not applicable", true);
		}

		public new void TestRecipientTaxIDHeading_UsesDisplayRecipientTaxIDHeadingRegistryValue()
		{
			Assert("Not applicable", true);
		}

		public new void TestRecipientTaxIDHeading_UsesGetRecipientTaxIDHeading_DisplayRecipientTaxIDHeadingRegistryValueIsNotSet()
		{
			Assert("Not applicable", true);
		}

		public new void TestRecipientTaxIDWithoutTax()
		{
			string str = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				OrgHeader orgProxy = Invoice.Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				orgProxy.LocalBusinessRegNo = "ABC123";
				orgProxy.MiscServ.OM_ARDontShowTaxOnDocs = ZBool.True;

				OrgCusCode orgCusCode = orgProxy.CustomsCodes.AddNew();
				orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Netherlands;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				orgCusCode.OK_CustomsRegNo = "123456";

				orgCusCode = orgProxy.CustomsCodes.AddNew();
				orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				orgCusCode.OK_CustomsRegNo = "654321";

				APInvoice.AH_OH = orgProxy.PK;

				Invoice.Header.OH_RL_NKClosestPort = "NLLME";
				orgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.VATCode, "123456");
				AssertEquals("PreCondition: Local VAT Code", "123456", orgProxy.LocalVATCode);
				AssertEquals("PreCondition: is not taxed", ZBool.True, orgProxy.MiscServ.OM_ARDontShowTaxOnDocs);

				InvoicingLineBase line2 = (InvoicingLineBase)APInvoice.Lines.AddNew();
				AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);

				AssertEquals("Recipient Tax ID Heading when Current Company is Netherlands", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Netherlands", "123456", InvoiceWrapper.RecipientTaxIDNumber);

				Invoice.Header.OH_RL_NKClosestPort = "AUSYD";
				orgProxy.SetCustomsCode(OrgCusCode.CodeTypes.VATCode, Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")), "654321");
				AssertEquals("PreCondition: Local VAT Code", "654321", orgProxy.LocalVATCode);
				AssertEquals("Recipient Tax ID Heading when Current Company is Australia", "Recipient Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Australia", "ABC123", InvoiceWrapper.RecipientTaxIDNumber);

				line2.AL_AT = ZGuid.Empty;
				InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
				Factory.ClearCachedValue<ZBool>(APInvoice.PK.ToStringKey());
				AssertEquals("Recipient Tax ID Heading when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(str);
			}
		}

		public new void TestRecipientTaxIDForZA()
		{
			ZString oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				InvoiceWrapper = GetInvoiceWrapper();
				AssertEquals("Recipient Tax ID Heading before setup", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number before setup", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);

				ZString taxNumber = "123456789";
				ZString orgCode = "ORGCOD";

				SetupNewInvoice(GlbCompany.CurrentCompany.OrgProxy, false);

				OrgHeader debtor = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				debtor.OH_Code = orgCode;
				debtor.OH_IsDebtor = new ZBool(true);
				debtor.CompanyData.SetARTaxApplicable(ZBool.True);
				debtor.PrimaryRegistrationNumber.Number = taxNumber;

				Invoice.Header.OH_RL_NKClosestPort = "ZAPRF";
				AssertEquals("Recipient Tax ID Heading", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID", taxNumber, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = oldCountry;
			}
		}

		public void TestAPLinesForInvoiceIsIndependentOfARInvoiceRollupOrGroupSetting()
		{
			SetUpInvoiceWrapperForRollUp();

			InvoicingLineBase line1 = AddOriginChargeToInvoice();
			InvoicingLineBase line2 = AddCustomsChargeToInvoice();
			var invoiceWrapper = (DocARInvoiceCommon)DocAPInvoice.New(InvoicingBase, Factory);
			invoiceWrapper = DocAPInvoice.New(Invoice, Factory);
			DocARInvoiceLineCollection linesForInvoice = invoiceWrapper.LinesForInvoice;
			linesForInvoice.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, linesForInvoice.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", linesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, linesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", linesForInvoice[0].OSTaxDisplay);
			AssertEquals("LinesForInvoice should come from Lines directly", invoiceWrapper.Lines[0], linesForInvoice[0]);

			//Second Line
			AssertEquals("Description", "Origin", linesForInvoice[1].LineDescription);
			AssertEquals("Amount", 150.00M, linesForInvoice[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", linesForInvoice[1].OSTaxDisplay);
			AssertEquals("LinesForInvoice should come from Lines directly", invoiceWrapper.Lines[1], linesForInvoice[1]);

			SetUpOrganisationForRollUpAll();
			linesForInvoice = invoiceWrapper.LinesForInvoice;
			linesForInvoice.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 2, linesForInvoice.Count);

			//First Line
			AssertEquals("Description", "Customs Charge", linesForInvoice[0].LineDescription);
			AssertEquals("Amount", 200.00M, linesForInvoice[0].OSExTaxAmount);
			AssertEquals("GST", "10.11%=20.22 **", linesForInvoice[0].OSTaxDisplay);
			AssertEquals("LinesForInvoice should come from Lines directly", invoiceWrapper.Lines[0], linesForInvoice[0]);

			//Second Line
			AssertEquals("Description", "Origin", linesForInvoice[1].LineDescription);
			AssertEquals("Amount", 150.00M, linesForInvoice[1].OSExTaxAmount);
			AssertEquals("GST", "10.11%=15.17 *", linesForInvoice[1].OSTaxDisplay);
			AssertEquals("LinesForInvoice should come from Lines directly", invoiceWrapper.Lines[1], linesForInvoice[1]);
		}

		public void TestSupplierTaxIDWithoutTax()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG";
				header.MiscServ.OM_ARDontShowTaxOnDocs = ZBool.True;

				OrgCusCode taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Netherlands;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Netherlands);
				taxCode.OK_CustomsRegNo = "123456";

				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Australia);
				taxCode.OK_CustomsRegNo = "654321";

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Netherlands);
				AssertEquals("PreCondition: RawTaxRegistrationNumber", "123456", header.RawTaxRegistrationNumber);
				AssertEquals("PreCondition: is not taxed", ZBool.True, header.MiscServ.OM_ARDontShowTaxOnDocs);

				APInvoice.AH_OH = header.PK;

				InvoicingLineBase line2 = (InvoicingLineBase)APInvoice.Lines.AddNew();
				AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				InvoiceWrapper = GetInvoiceWrapper();

				AssertEquals("Supplier Tax ID Heading when Current Company is Netherlands", "Client VAT #:", InvoiceWrapper.SupplierTaxIDHeading);
				AssertEquals("Supplier Tax ID Number when Current Company is Netherlands", "NL123456", InvoiceWrapper.SupplierTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

				AssertEquals("RawTaxRegistrationNumber is stiil unchanged because the CodeForTaxRegistration has been already created", "123456", header.RawTaxRegistrationNumber);
				AssertEquals("Supplier Tax ID Heading when Current Company is Australia", ZString.Empty, InvoiceWrapper.SupplierTaxIDHeading);
				AssertEquals("Supplier Tax ID Number when Current Company is Australia", ZString.Empty, InvoiceWrapper.SupplierTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestSupplierTaxIDForNewZealand()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.NewZealand))
			using (AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<OrgHeader>();
				header.OH_RL_NKClosestPort = Constants.CountryCodes.Australia;

				var code = header.CustomsCodes.AddNew();
				code.OK_RN_NKCodeCountry = Constants.CountryCodes.NewZealand;
				code.OK_CodeType = "GST";
				code.OK_CustomsRegNo = "12345678";

				APInvoice.AH_OH = header.PK;
				InvoiceWrapper = GetInvoiceWrapper();

				APInvoice.IsSelfBillingInvoice = false;
				AssertEquals("Supplier Tax ID Heading when Current Company is NewZealand", "", InvoiceWrapper.SupplierTaxIDHeading);
				AssertEquals("Supplier Tax ID Number when Current Company is NewZealand", "", InvoiceWrapper.SupplierTaxIDNumber);

				APInvoice.IsSelfBillingInvoice = true;
				AssertEquals("Supplier Tax ID Heading when Current Company is NewZealand", "", InvoiceWrapper.SupplierTaxIDHeading);
				AssertEquals("Supplier Tax ID Number when Current Company is NewZealand", "", InvoiceWrapper.SupplierTaxIDNumber);

				AssertEquals(false, InvoiceWrapper.IsTaxed);
				var line1 = (InvoicingLineBase)APInvoice.Lines.AddNew();
				line1.AL_AC = TestObjectCreator.FRT.PK;
				line1.AL_AT = TestObjectCreator.GST1.PK;
				line1.AL_OSExTaxAmount = 500m;
				InvoiceWrapper = GetInvoiceWrapper();
				Factory.ClearCachedValue<ZBool>(APInvoice.PK.ToStringKey());
				AssertEquals(true, InvoiceWrapper.IsTaxed);
				AssertEquals("Supplier Tax ID Heading when Current Company is NewZealand", "GST #:", InvoiceWrapper.SupplierTaxIDHeading);
				AssertEquals("Supplier Tax ID Number when Current Company is NewZealand", "12345678", InvoiceWrapper.SupplierTaxIDNumber);
			}
		}

		public void TestSupplierAddress()
		{
			var aPOrg = Factory.New<OrgHeader>();
			aPOrg.OH_Code = "AP_ORG TEST";
			aPOrg.MainAddress.OA_Address1 = "AP_ORG Address";
			aPOrg.OH_FullName = "AP ORG";

			var contact = aPOrg.Contacts.AddNew();
			contact.OC_ContactName = "Jamie";
			OrgDocument doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Payables.Code;

			APInvoice.AH_OH = aPOrg.PK;
			Factory.Save();
			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals(InvoiceWrapper.Organisation.PostalAddressExcludeCountryIfSame, InvoiceWrapper.SupplierNameAddress);

			OrgAddress payablesAddress = aPOrg.Addresses.AddNew();
			payablesAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			payablesAddress.OA_OH = aPOrg.PK;
			payablesAddress.OA_Address1 = "AP_ORG Payables Address";
			Factory.Save();
			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals(InvoiceWrapper.SupplierNameAddress, "AP ORG\nAP_ORG PAYABLES ADDRESS");
		}

		public new void TestIsTaxed()
		{
			var org = GlbCompany.CurrentCompany.OrgProxy;
			org.OH_Code = "TESTORG1";
			APInvoice.AH_OH = org.PK;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("IsTaxed:1", false, InvoiceWrapper.IsTaxed);

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("IsTaxed:2", false, InvoiceWrapper.IsTaxed);

			line1.AL_OSExTaxAmount = 123.00M;
			line1.AL_OSTaxAmount = 35.30M;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_OSExTaxAmount = 234.00M;
			line2.AL_OSTaxAmount = 57.40M;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("IsTaxed:3", false, InvoiceWrapper.IsTaxed);

			line1.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(APInvoice, (invoice, factory) => DocAPInvoice.New(invoice, factory));
			AssertEquals("IsTaxed:4", true, InvoiceWrapper.IsTaxed);
		}

		public new void TestIsTaxedIsCachedInFactory()
		{
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			Assert(!APInvoice.IsInDatabase);
			AssertEquals(false, APInvoice.IsTaxed);
			AssertEquals(APInvoice.IsTaxed, InvoiceWrapper.IsTaxed);

			var line = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			line.AL_OSExTaxAmount = 123.00M;
			line.AL_OSTaxAmount = 35.30M;
			line.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("IsTaxed should be true as it calculates based on line when invoice is NOT in database", true, APInvoice.IsTaxed);
			AssertEquals(APInvoice.IsTaxed, InvoiceWrapper.IsTaxed);

			Factory.Save();
			Assert(APInvoice.IsInDatabase);
			Factory.ClearCachedValue<ZBool>("IsTaxed:" + APInvoice.PK.ToStringKey());
			Factory.GetCachedValue("IsTaxed:" + APInvoice.PK.ToStringKey(), () => ZBool.False);
			AssertEquals("IsTaxed should be false as it reads from the cache when transaction is in database", false, APInvoice.IsTaxed);
			AssertEquals(APInvoice.IsTaxed, InvoiceWrapper.IsTaxed);
		}

		public void TestIsTaxedIgnoreARDontShowTaxOnDocs()
		{
			OrgHeader orgProxy = Invoice.Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			orgProxy.LocalBusinessRegNo = "ABC123";
			orgProxy.MiscServ.OM_ARDontShowTaxOnDocs = ZBool.True;

			var org = GlbCompany.CurrentCompany.OrgProxy;
			org.OH_Code = "TESTORG1";
			APInvoice.AH_OH = org.PK;

			InvoicingLineBase line1 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)InvoicingBase.Lines.AddNew();
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);

			line1.AL_OSExTaxAmount = 123.00M;
			line1.AL_OSTaxAmount = 35.30M;
			line1.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_OSExTaxAmount = 234.00M;
			line2.AL_OSTaxAmount = 57.40M;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceWrapper = RecreateTestingInvoiceDocWrapper(APInvoice, (invoice, factory) => DocAPInvoice.New(invoice, factory));
			AssertEquals("IsTaxed should be true even if ARDontShowTaxOnDocs is true", true, InvoiceWrapper.IsTaxed);
		}

		public new void TestJobInvoiceNumber()
		{
			APInvoice.AH_TransactionNum = "1234";
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Must be equal transaction number", "1234", InvoiceWrapper.JobInvoiceNumber);
		}

		public new void TestMessage()
		{
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Invoice message", "This is a Self Billed / Recipient Issued Invoice.", InvoiceWrapper.Message);

			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			AssertEquals("Invoice and Disbursement message", AccountingConfigurationRegistry.Instance.DisbursementMessage.Value, APInvoiceWrapper.Message.ToString());

			Invoice.AH_TransactionCategory = "";
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			AssertEquals("Credit note message", "This is a Self Billed / Recipient Issued Credit Note.", APInvoiceWrapper.Message);

			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			AssertEquals("Adjustment note message", "This is a Self Billed / Recipient Issued Adjustment Note.", APInvoiceWrapper.Message);
		}

		public new void TestNonTaxAdjustmentNoteTitle()
		{
			APInvoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Recipient Created / Self Billed Adjustment Note", InvoiceWrapper.DocumentTitle);

			AssertEquals("Please return a copy of this recipient created / self billed adjustment note with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestNonTaxDisbursementInvoiceTitle()
		{
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			APInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Recipient Created / Self Billed Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestNonTaxDisbursementInvoiceTitleWithAllNotReportableChargeCodes()
		{
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var notReportTaxRate = Factory.New<AccTaxRate>();
			notReportTaxRate.AT_Code = "NOTREPORT";
			notReportTaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = notReportTaxRate.PK;
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = notReportTaxRate.PK;
			APInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Recipient Created / Self Billed Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestNonTaxInvoiceTitle()
		{
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Recipient Created / Self Billed Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestNonTaxInvoiceTitleWithNullTaxAndNotReportableChargeCodes()
		{
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = ZGuid.Empty;
			var notReportTaxRate = Factory.New<AccTaxRate>();
			notReportTaxRate.AT_Code = "NOTREPORT";
			notReportTaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = notReportTaxRate.PK;
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Recipient Created / Self Billed Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);

			notReportTaxRate.AT_Type = AccTaxRate.Types.Exempt;
			AssertEquals("Recipient Created / Self Billed Tax Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed tax invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);

			notReportTaxRate.AT_Type = AccTaxRate.Types.ExcludedFromTheTaxBase;
			AssertEquals("Recipient Created / Self Billed Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestNonTaxInvoiceTitleWithAllNotReportableChargeCodes()
		{
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			var notReportTaxRate = Factory.New<AccTaxRate>();
			notReportTaxRate.AT_Code = "NOTREPORT";
			notReportTaxRate.AT_Type = AccTaxRate.Types.NotReportable;
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = notReportTaxRate.PK;
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = notReportTaxRate.PK;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, Factory);
			AssertEquals("Recipient Created / Self Billed Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestTaxAdjustmentNoteTitle()
		{
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.AdjustmentNote;
			APInvoice.AH_TransactionType = TransactionTypes.AdjustmentNote;
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = TestObjectCreator.GST1.PK;
			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals("Recipient Created / Self Billed Tax Adjustment Note", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed tax adjustment note with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestTaxInvoiceTitle()
		{
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = TestObjectCreator.GST1.PK;
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals("Recipient Created / Self Billed Tax Invoice", InvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed tax invoice with your payment if paying by cheque", InvoiceWrapper.InvoiceFooterMessage);
		}

		public new void TestTaxDisbursementInvoiceTitle()
		{
			APInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			APInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			((InvoicingLineBase)InvoicingBase.Lines.AddNew()).AL_AT = TestObjectCreator.GST1.PK;
			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals("Recipient Created / Self Billed Tax Invoice", APInvoiceWrapper.DocumentTitle);
			AssertEquals("Please return a copy of this recipient created / self billed tax invoice with your payment if paying by cheque", APInvoiceWrapper.InvoiceFooterMessage);
		}

		public void TestLinesForCostConfirmationSummary_RollupByChargeCode()
		{
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentRollupSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DocRollUpConstants.CostConfirmationDocumentRollupSettingsCodes.ChargeCode);

			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S00001000");
			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S00001001");
			ForwardingShipment shipment3 = TestObjectCreator.CreateShipment("S00001002");
			InvoiceWrapper = (DocAPInvoice)GetBaseInvoiceWrapper();
			Invoice = (TransactionHeader)InvoiceWrapper.WrappedObject;
			Invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			JobHeader job1 = GetInvoiceJob(shipment1, Invoice);
			JobHeader job2 = GetInvoiceJob(shipment2, Invoice);
			JobHeader job3 = GetInvoiceJob(shipment3, Invoice);
			Factory.Save();

			InvoicingLineBase line1 = AddOriginChargeToInvoice("ORG", 1);
			InvoicingLineBase line2 = AddDestinationChargeToInvoice("DST", 2);
			InvoicingLineBase line3 = AddFreightChargeToInvoice("FRC", 3);
			InvoicingLineBase line4 = AddOriginChargeToInvoice("ORG", 4);
			InvoicingLineBase line5 = AddDestinationChargeToInvoice("DST", 5);
			InvoicingLineBase line6 = AddCommentChargeToInvoice("CMT", 6);
			InvoicingLineBase line7 = AddOriginChargeToInvoice("ORG", 7);
			InvoicingLineBase line8 = AddDestinationChargeToInvoice("DST", 8);
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice("NGP", 9);
			InvoicingLineBase line10 = AddFreightChargeToInvoice("FRC", 109);
			InvoicingLineBase line11 = AddOriginChargeToInvoice("ORG", 11);
			InvoicingLineBase line12 = AddDestinationChargeToInvoice("DST", 12);
			InvoicingLineBase line13 = AddFreightChargeToInvoice("FRC", 13);
			InvoicingLineBase line14 = AddCommentChargeToInvoice("CMT", 14);

			line1.AL_JH = job1.PK;
			line2.AL_JH = job1.PK;
			line3.AL_JH = job2.PK;
			line4.AL_JH = job3.PK;
			line5.AL_JH = job2.PK;
			line6.AL_JH = job3.PK;
			line7.AL_JH = job1.PK;
			line8.AL_JH = job1.PK;
			line9.AL_JH = job3.PK;
			line10.AL_JH = job1.PK;
			line11.AL_JH = ZGuid.Empty;
			line12.AL_JH = job2.PK;
			line13.AL_JH = job3.PK;
			line14.AL_JH = job1.PK;

			AssertEquals("Precondition: Count of lines", 14, InvoiceWrapper.Lines.Count);

			DocARInvoiceLineCollection lines = InvoiceWrapper.LinesForCostConfirmationSummary;

			lines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 6, lines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 0M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 0M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 0M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 0M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Destination", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 800.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 80.88M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 880.88M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "80.88 **,*****,*******,***********", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 80.88M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 800.0M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Freight", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 750.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 75.84M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 825.84M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "75.84 ***,*********,************", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 75.84M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 750.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 25.28M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=25.28 ********", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Origin", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 600.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 60.68M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 660.68M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "60.68 *,****,******,**********", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 60.68M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 600.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("All lines must be tested.", lines.Count, lineNumber);
		}

		public void TestLinesForCostConfirmationSummary_RollupByJob()
		{
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentRollupSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DocRollUpConstants.CostConfirmationDocumentRollupSettingsCodes.Job);

			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S00001000");
			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S00001001");
			ForwardingShipment shipment3 = TestObjectCreator.CreateShipment("S00001002");
			InvoiceWrapper = (DocAPInvoice)GetBaseInvoiceWrapper();
			Invoice = (TransactionHeader)InvoiceWrapper.WrappedObject;
			Invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			JobHeader job1 = GetInvoiceJob(shipment1, Invoice);
			JobHeader job2 = GetInvoiceJob(shipment2, Invoice);
			JobHeader job3 = GetInvoiceJob(shipment3, Invoice);
			Factory.Save();

			InvoicingLineBase line1 = AddOriginChargeToInvoice("ORG", 1);
			InvoicingLineBase line2 = AddDestinationChargeToInvoice("DST", 2);
			InvoicingLineBase line3 = AddFreightChargeToInvoice("FRC", 3);
			InvoicingLineBase line4 = AddOriginChargeToInvoice("ORG", 4);
			InvoicingLineBase line5 = AddDestinationChargeToInvoice("DST", 5);
			InvoicingLineBase line6 = AddCommentChargeToInvoice("CMT", 6);
			InvoicingLineBase line7 = AddOriginChargeToInvoice("ORG", 7);
			InvoicingLineBase line8 = AddDestinationChargeToInvoice("DST", 8);
			InvoicingLineBase line9 = AddNotGroupedChargeToInvoice("NGP", 9);
			InvoicingLineBase line10 = AddFreightChargeToInvoice("FRC", 109);
			InvoicingLineBase line11 = AddOriginChargeToInvoice("ORG", 11);
			InvoicingLineBase line12 = AddDestinationChargeToInvoice("DST", 12);
			InvoicingLineBase line13 = AddFreightChargeToInvoice("FRC", 13);
			InvoicingLineBase line14 = AddCommentChargeToInvoice("CMT", 14);

			line1.AL_JH = job1.PK;
			line2.AL_JH = job1.PK;
			line3.AL_JH = job2.PK;
			line4.AL_JH = job3.PK;
			line5.AL_JH = job2.PK;
			line6.AL_JH = job3.PK;
			line7.AL_JH = job1.PK;
			line8.AL_JH = job1.PK;
			line9.AL_JH = job3.PK;
			line10.AL_JH = job1.PK;
			line11.AL_JH = ZGuid.Empty;
			line12.AL_JH = job2.PK;
			line13.AL_JH = job3.PK;
			line14.AL_JH = job1.PK;

			AssertEquals("Precondition: Count of lines", 14, InvoiceWrapper.Lines.Count);

			DocARInvoiceLineCollection lines = InvoiceWrapper.LinesForCostConfirmationSummary;

			lines.Sort("LineDescription", ListSortDirection.Ascending);

			AssertEquals("Count of lines", 7, lines.Count);

			int lineNumber = 0;

			AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 0M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 0M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Comment charge", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 0M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 0M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 0M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 0M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 0M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Job: S00001000", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 950.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 96.06M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 1046.06M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "96.06 *,**,******,*******,*********", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 96.06M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 950.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Job: S00001001", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 650.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 65.72M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 715.72M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "65.72 ***,*****,***********", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 65.72M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 650.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Job: S00001002", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 400.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 40.45M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 440.45M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "40.45 ****,************", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 40.45M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 400.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Not grouped charge", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 250.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 25.28M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 275.28M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=25.28 ********", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 25.28M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 250.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("Description", "Origin", lines[lineNumber].LineDescription);
			AssertEquals("OSExTaxAmount", 150.00M, lines[lineNumber].OSExTaxAmount);
			AssertEquals("OSTaxAmount", 15.17M, lines[lineNumber].OSTaxAmount);
			AssertEquals("OSAmount", 165.17M, lines[lineNumber].OSAmount);
			AssertEquals("GST", "10.11%=15.17 **********", lines[lineNumber].OSTaxDisplay);
			AssertEquals("GSTVAT", 15.17M, lines[lineNumber].GSTVAT);
			AssertEquals("LineAmount", 150.00M, lines[lineNumber].LineAmount);
			lineNumber++;

			AssertEquals("All lines must be tested.", lines.Count, lineNumber);
		}

		public void TestApprovalRequestIDForInvoiceRelatedRequest()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var wrapper = DocGenericTransactionHeader.New(apInvoice, Factory, null);

			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(apInvoice);
			request.XP_ReasonDescription = "Test";
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			request.XP_RequestID = "ABC12345";

			AssertEquals("ABC12345", wrapper.ApprovalRequestID);
		}

		public void TestApprovalRequestIDForJobRelatedRequest()
		{
			var jobPK = ZGuid.NewZGuid();
			var invoiceCharges = new APInvoiceCharges("Creditor1", "INV1", jobPK, "JH", null);
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeJobRelated(invoiceCharges, jobPK, "JH");
			request.XP_ReasonDescription = "Test";
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			request.XP_RequestID = "ABC12345";

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_TransactionNum = "INV1";
			apInvoice.AH_JH = jobPK;

			var wrapper = DocGenericTransactionHeader.New(apInvoice, Factory, null);
			AssertEquals("ABC12345", wrapper.ApprovalRequestID);
		}

		public void TestApprovalRequestIDForConsolRelatedRequest()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_TransactionNum = "INV1";
			apInvoice.AH_JH = ZGuid.Empty;

			ApportionmentListing app = new ApportionmentListing(Factory, consol);
			var cost = app.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Enterprise.Environment.Env.Registry.FreightChargeCode;
			cost.E6_OH_Creditor = TestObjectCreator.LocalClient.PK;
			cost.E6_RX_NKCurrency = "AUD";
			cost.E6_ExchangeRate = 1m;
			cost.E6_OSCostAmount = 12.00m;
			cost.E6_InvoiceNum = "INV1";
			cost.E6_AH_APInvoice = apInvoice.PK;

			var apInvoiceCharges = new APInvoiceCharges(TestObjectCreator.LocalClient.OH_Code, cost.E6_InvoiceNum, ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.AddRange(cost.Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, cost.ApportionmentCharges.GetPKs())));

			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.XP_ParentID = consol.PK;
			request.XP_ParentTableCode = consol.TablePrefix;
			request.XP_ReasonDescription = "Test";
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			request.XP_RequestID = "ABC12345";
			request.InitializeJobRelated(apInvoiceCharges, consol.PK, consol.TablePrefix);

			var wrapper = DocGenericTransactionHeader.New(apInvoice, Factory, null);
			AssertEquals("ABC12345", wrapper.ApprovalRequestID);
		}

		#region Implementation

		protected override void AssertRecipientTaxIDCoreForMalaysia(AccTaxRate taxRate1, AccTaxRate taxRate2)
		{
			Assert("Should not affect AP.", true);
		}

		void SetupNewInvoice(OrgHeader header, bool inNewFactory)
		{
			BusinessObjectFactory factory;
			if (inNewFactory)
			{
				factory = new BusinessObjectFactory();
				Invoice = factory.New<APInvoice>();
			}
			else
			{
				factory = Factory;
			}
			APInvoice.AH_OH = header.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)APInvoice.Lines.AddNew();
			AccTaxRate rate = factory.LoadTop1<AccTaxRate>(new ZQuery());
			line2.AL_AT = rate.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			InvoiceWrapper = DocAPInvoice.New(APInvoice, factory);
		}

		APInvoice APInvoice
		{
			get { return Invoice as APInvoice; }
		}

		new DocAPInvoice InvoiceWrapper;

		DocAPInvoice APInvoiceWrapper
		{
			get { return (DocAPInvoice)base.InvoiceWrapper; }
		}

		#endregion
	}
}
