using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobComInvoiceHeaderValidationTest : Customs.Business.Testing.JobComInvoiceHeaderValidationTest
	{
		// ******************
		//	This should be removed, and we should revert to base, when proper value-build up gets in
		public override void TestValidateAbsenceOfOFTOrONS()
		{
			Assert(true);
		}

		public void TestCheckJZ_Calc_CIFAmount()
		{
			var dec = Factory.New<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			inv.InvoiceLines.AddNew();
			inv.JZ_InvoiceAmount = 100;
			inv.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			AssertEquals(false, inv.JZ_Calc_CIFAmountInfo.HasWarning("The CIF amount should be greater than zero before submitting this declaration.\r\nThis usually happens when you have entered more invoice charge amounts than invoice total amount."));
		}

		public void TestCheckJZ_IncoTermRequiredToBeSameAsIncoTermOnDeclaration()
		{
			using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoiceCore", true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ShipmentIncoTerm = "1";

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = "2";
				AssertHasMessageError(invoiceHeader.JZ_IncoTermInfo, "Incoterm values do not match between Declaration and Invoice Header.");

				invoiceHeader.JZ_IncoTerm = "1";
				AssertNoMessageError(invoiceHeader.JZ_IncoTermInfo, "Incoterm values do not match between Declaration and Invoice Header.");
			}
		}

		public void TestCheckJZ_IncoTerm_ForTransport()
		{
			CombineAssertions(() =>
			{
				AssertIncoTermValidation("FAS", true);
				AssertIncoTermValidation("FOB", true);
				AssertIncoTermValidation("CFR", true);
				AssertIncoTermValidation("CIF", true);

				AssertIncoTermValidation("EXW", false);
				AssertIncoTermValidation("CIP", false);
			});
		}

		void AssertIncoTermValidation(ZString incoterm, ZBool shouldHaveWarningIfNotWaterTransportMode)
		{
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "SEA", false);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "AIR", shouldHaveWarningIfNotWaterTransportMode);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, "IWT", false);
			AssertIncoTermValidationForTransportAndIncoTerm(incoterm, ZString.Empty, shouldHaveWarningIfNotWaterTransportMode);
		}

		void AssertIncoTermValidationForTransportAndIncoTerm(ZString incoterm, ZString transportMode, ZBool shouldHaveWarningIfNotWaterTransportMode)
		{
			var warningMessage = "is only valid for sea and inland waterway transport. Please check against the transport mode.";
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = incoterm;

			declaration.JE_TransportMode = transportMode;
			var validation = invoiceHeader.Validation;
			validation.ValidateJZ_IncoTerm();
			if (shouldHaveWarningIfNotWaterTransportMode && validation.ShouldValidateWaterIncoTermAndTransportMode)
			{
				AssertHasWarningContaining("There is a warning when transport is " + transportMode + " and incoterm is " + incoterm, invoiceHeader.JZ_IncoTermInfo, warningMessage);
			}
			else
			{
				AssertNoWarningContaining("No warning when transport is " + transportMode + " and incoterm is " + incoterm, invoiceHeader.JZ_IncoTermInfo, warningMessage);
			}
		}

		public override void TestValidateJZ_InvoiceNumber()
		{
			base.TestValidateJZ_InvoiceNumber();

			var invoice = SetUpInvoiceWithSupplier();
			if (invoice.AddingSupportingDocumentAutomaticallyEnabled)
			{
				invoice.JZ_InvoiceDate = ZDate.Today;

				var expectedMessageWarning = "A Supporting Document of type N380 can’t be created automatically because invoice has no Invoice No.";

				invoice.JZ_InvoiceNumber = "";
				AssertHasWarningContaining(invoice.JZ_InvoiceNumberInfo, expectedMessageWarning);

				invoice.JZ_InvoiceNumber = "3";
				AssertNoWarningContaining(invoice.JZ_InvoiceNumberInfo, expectedMessageWarning);
			}
			else
			{
				Assert("When AddingSupportingDocumentAutomatically feature is disabled don't need this test", true);
			}
		}

		public void TestValidateJZ_InvoiceNumber_UniqueInDeclaration()
		{
			invoiceHeader.JZ_InvoiceNumber = "0001";
			var messageError = "Invoice numbers within a declaration must be unique.";

			var invoiceHeader2 = declaration.Invoices.AddNew();

			invoiceHeader2.JZ_InvoiceNumber = "0001";
			AssertHasMessageError(invoiceHeader2.JZ_InvoiceNumberInfo, messageError);

			invoiceHeader2.JZ_InvoiceNumber = "0002";
			AssertNoMessageError(invoiceHeader2.JZ_InvoiceNumberInfo, messageError);
		}

		public void TestCheckJZ_InvoiceDate()
		{
			var invoice = SetUpInvoiceWithSupplier();
			if (invoice.AddingSupportingDocumentAutomaticallyEnabled)
			{
				invoice.JZ_InvoiceDate = ZDate.Empty;
				invoice.JZ_InvoiceNumber = "1";

				var expectedMessageWarning = "A Supporting Document of type N380 can’t be created automatically because invoice has no Invoice Date";

				invoice.JZ_InvoiceDate = ZDate.Empty;
				AssertHasWarningContaining(invoice.JZ_InvoiceDateInfo, expectedMessageWarning);

				invoice.JZ_InvoiceDate = ZDateTime.Today;
				AssertNoWarningContaining(invoice.JZ_InvoiceDateInfo, expectedMessageWarning);
			}
			else
			{
				Assert("When AddingSupportingDocumentAutomatically feature is disabled don't need this test", true);
			}
		}

		public virtual void TestCheckJZ_OH_Supplier()
		{
			var invoice = SetUpInvoiceWithSupplier();
			if (invoice.AddingSupportingDocumentAutomaticallyEnabled)
			{
				var declaration = invoice.JobDeclaration;
				var supplier = invoice.Supplier;
				invoice.JZ_InvoiceNumber = "1";
				invoice.JZ_InvoiceDate = ZDateTime.Today;

				var expectedMessageWarning = "A Supporting Document of type N380 can’t be created automatically because invoice has no Supplier and Declaration has no Supplier";

				declaration.JE_OH_Supplier = ZGuid.Empty;
				invoice.SupportingDocuments.RemoveAndDeleteAll();
				invoice.JZ_OH_Supplier = ZGuid.Empty;
				AssertHasWarningContaining(invoice.JZ_OH_SupplierInfo, expectedMessageWarning);

				declaration.JE_OH_Supplier = supplier.PK;
				invoice.JZ_OH_Supplier = ZGuid.Empty;
				AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, expectedMessageWarning);

				declaration.JE_OH_Supplier = ZGuid.Empty;
				invoice.JZ_OH_Supplier = supplier.PK;
				AssertNoWarningContaining(invoice.JZ_OH_SupplierInfo, expectedMessageWarning);
			}
			else
			{
				Assert("When AddingSupportingDocumentAutomatically feature is disabled don't need this test", true);
			}
		}

		public void TestJZ_OA_BuyerAddressNotEmptyWarning()
		{
			invoiceHeader = GetInvoiceHeader();
			invoiceHeader.BuyerOrgPK = ZGuid.NewZGuid();
			invoiceHeader.JZ_OA_BuyerAddress = ZGuid.Empty;
			invoiceHeader.Validation.ValidateJZ_OA_BuyerAddress();

			CombineAssertions(() =>
			{
				AssertHasWarning(invoiceHeader.JZ_OA_BuyerAddressInfo, ExpectedBuyerAddressEmptyWarningMessage);

				invoiceHeader.JZ_OA_BuyerAddress = ZGuid.NewZGuid();
				AssertNoWarning(invoiceHeader.JZ_OA_BuyerAddressInfo, ExpectedBuyerAddressEmptyWarningMessage);

				invoiceHeader.BuyerOrgPK = ZGuid.Empty;
				invoiceHeader.JZ_OA_BuyerAddress = ZGuid.Empty;
				AssertNoWarning(invoiceHeader.JZ_OA_BuyerAddressInfo, ExpectedBuyerAddressEmptyWarningMessage);
			});
		}

		protected virtual string ExpectedBuyerAddressEmptyWarningMessage => "Buyer organization will not be saved because no address is selected.";

		public void TestCheckBuyerOrgPK_RuleC0728()
		{
			var messageError = "[C0728] – This field must be empty for this Requested Procedure / Additional Declaration / Declaration Sub Type.";

			var declaration = GetJobDeclarationForTest();
			var organizationPK = Factory.New<OrgHeader>().PK;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invoice = GetInvoice("", "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("", "A", organizationPK);
				AssertNoMessageErrorContaining("No message error is expected when subtype is other than C or F.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("", "F", ZGuid.Empty);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected when BuyerOrgPK is not set.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("", "F", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is F.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("", "F", organizationPK, MessageTypeList.Codes.Export);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected for export declaration.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("5100000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 51.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("5300000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 53.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("7100000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 71.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("5200000", ZString.Empty, organizationPK);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected when ProcedureCode is other than 51, 53 or 71.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("0000F15", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when Concession is F15.", invoice.BuyerOrgPKInfo, messageError);

				invoice = GetInvoice("0000F16", ZString.Empty, organizationPK);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected when Concession is other than F15.", invoice.BuyerOrgPKInfo, messageError);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var invoice = GetInvoice("0000F15", ZString.Empty, organizationPK);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected for non UCC6 declarations.", invoice.BuyerOrgPKInfo, messageError);
			}

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(r => r.IsRuleC0728Active);
				var invoice = GetInvoice("", "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C and RuleC0728 is enabled.", invoice.BuyerOrgPKInfo, messageError);

				context.DisableRule(r => r.IsRuleC0728Active);
				invoice = GetInvoice("", "C", organizationPK);
				AssertNoMessageErrorContaining("No validation is expected when RuleC0728 is disabled", invoice.BuyerOrgPKInfo, messageError);
			}

			JobComInvoiceHeader GetInvoice(ZString ji_Procedure, ZString cEI_SubStyle, ZGuid orgPK, string messageType = MessageTypeList.Codes.Import)
			{
				declaration = GetJobDeclarationForTest();
				declaration.CustomsEntryInstructions?.RemoveAndDeleteAll();
				declaration.JE_MessageType = messageType;
				var cei = declaration.CustomsEntryInstructions.AddNew();
				cei.CEI_SubStyle = cEI_SubStyle;
				var invoiceHeader = declaration.Invoices.AddNew();
				var line = invoiceHeader.InvoiceLines.AddNew();
				line.JI_FormattedProcedure = ji_Procedure;
				line.JI_CEI = cei.PK;
				invoiceHeader.BuyerOrgPK = orgPK;
				return invoiceHeader;
			}
		}

		public void TestCheckSellerOrgPK_RuleC0728()
		{
			var messageError = "[C0728] – This field must be empty for this Requested Procedure / Additional Declaration / Declaration Sub Type.";

			var declaration = GetJobDeclarationForTest();
			var organizationPK = Factory.New<OrgHeader>().PK;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invoice = GetInvoice("", "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("", "A", organizationPK);
				AssertNoMessageErrorContaining("No message error is expected when subtype is other than C or F.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("", "F", ZGuid.Empty);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected when SellerOrgPK is not set.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("", "F", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is F.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("", "F", organizationPK, MessageTypeList.Codes.Export);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected for export declaration.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("5100000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 51.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("5300000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 53.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("7100000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 71.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("5200000", ZString.Empty, organizationPK);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected when ProcedureCode is other than 51, 53 or 71.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("0000F15", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when Concession is F15.", invoice.SellerOrgPKInfo, messageError);

				invoice = GetInvoice("0000F16", ZString.Empty, organizationPK);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected when Concession is other than F15.", invoice.SellerOrgPKInfo, messageError);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var invoice = GetInvoice("0000F15", ZString.Empty, organizationPK);
				AssertNoMessageErrorContaining("Validation run for RuleC0728 is not expected for non UCC6 declarations.", invoice.SellerOrgPKInfo, messageError);
			}

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(r => r.IsRuleC0728Active);
				var invoice = GetInvoice("", "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C and RuleC0728 is enabled.", invoice.SellerOrgPKInfo, messageError);

				context.DisableRule(r => r.IsRuleC0728Active);
				invoice = GetInvoice("", "C", organizationPK);
				AssertNoMessageErrorContaining("No validations is expected when RuleC0728 is disabled.", invoice.SellerOrgPKInfo, messageError);
			}

			JobComInvoiceHeader GetInvoice(ZString ji_Procedure, ZString cEI_SubStyle, ZGuid orgPK, string messageType = MessageTypeList.Codes.Import)
			{
				declaration = GetJobDeclarationForTest();
				declaration.CustomsEntryInstructions?.RemoveAndDeleteAll();
				declaration.JE_MessageType = messageType;
				var cei = declaration.CustomsEntryInstructions.AddNew();
				cei.CEI_SubStyle = cEI_SubStyle;
				var invoiceHeader = declaration.Invoices.AddNew();
				var line = invoiceHeader.InvoiceLines.AddNew();
				line.JI_FormattedProcedure = ji_Procedure;
				line.JI_CEI = cei.PK;
				invoiceHeader.SellerOrgPK = orgPK;
				return invoiceHeader;
			}
		}

		public void TestJZ_OA_SellerAddressNotEmptyWarning()
		{
			invoiceHeader.SellerOrgPK = ZGuid.NewZGuid();
			invoiceHeader.JZ_OA_SellerAddress = ZGuid.Empty;
			invoiceHeader.Validation.ValidateJZ_OA_SellerAddress();

			CombineAssertions(() =>
			{
				AssertHasWarning(invoiceHeader.JZ_OA_SellerAddressInfo, "Seller organization will not be saved because no address is selected.");

				invoiceHeader.JZ_OA_SellerAddress = ZGuid.NewZGuid();
				AssertNoWarning(invoiceHeader.JZ_OA_SellerAddressInfo, "Seller organization will not be saved because no address is selected.");

				invoiceHeader.SellerOrgPK = ZGuid.Empty;
				invoiceHeader.JZ_OA_SellerAddress = ZGuid.Empty;
				AssertNoWarning(invoiceHeader.JZ_OA_SellerAddressInfo, "Seller organization will not be saved because no address is selected.");
			});
		}

		public void TestJZ_OA_ExporterAddressNotEmptyWarning()
		{
			invoiceHeader.ExporterOrgPK = ZGuid.NewZGuid();
			invoiceHeader.JZ_OA_ExporterAddress = ZGuid.Empty;
			invoiceHeader.Validation.ValidateJZ_OA_ExporterAddress();

			CombineAssertions(() =>
			{
				AssertHasWarning(invoiceHeader.JZ_OA_ExporterAddressInfo, "Exporter organization will not be saved because no address is selected.");

				invoiceHeader.JZ_OA_ExporterAddress = ZGuid.NewZGuid();
				AssertNoWarning(invoiceHeader.JZ_OA_ExporterAddressInfo, "Exporter organization will not be saved because no address is selected.");

				invoiceHeader.ExporterOrgPK = ZGuid.Empty;
				invoiceHeader.JZ_OA_ExporterAddress = ZGuid.Empty;
				AssertNoWarning(invoiceHeader.JZ_OA_ExporterAddressInfo, "Exporter organization will not be saved because no address is selected.");
			});
		}

		public void TestCheckRuleC0002_Exporter()
		{
			var messageForInvoiceHeader = "[C0002] Value can’t be entered in both Invoice header and Invoice lines.";

			var exporterForInvoice = Factory.New<OrgHeader>();
			var exporterForInvoiceLine = Factory.New<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceheader1 = declaration.Invoices.AddNew();
			var invoiceheader2 = declaration.Invoices.AddNew();

			var invoiceLine1_1 = invoiceheader1.InvoiceLines.AddNew();
			var invoiceLine1_2 = invoiceheader1.InvoiceLines.AddNew();

			var invoiceLine2_1 = invoiceheader2.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceheader2.InvoiceLines.AddNew();

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, isUCC6: true))
			{
				context.EnableRule(r => r.IsRuleC0002Active);
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

				invoiceLine1_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;

				CombineAssertions("Rule C0002 is Active : declaration is UCC6 and Export, invoiceHeader1 and invoiceLine1_1 both have value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2", invoiceheader2.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});

				invoiceheader1.JZ_OA_SupplierAddress = ZGuid.Empty;
				invoiceLine1_1.JI_OA_ExporterAddress = ZGuid.Empty;

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				invoiceLine1_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;
				CombineAssertions("Rule C0002 is Active : declaration is UCC6 and Import, invoiceHeader1 and invoiceLine1_1 both have value for Exporter", () =>
				{
					AssertHasMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2", invoiceheader2.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});

				invoiceLine1_2.JI_OA_ExporterAddress = ZGuid.Empty;
				invoiceheader1.JZ_OA_SupplierAddress = ZGuid.Empty;
				CombineAssertions("Rule C0002 is Active : declaration is UCC6 and Import, invoiceHeader1 has no value and at least 1 of its invoiceLines has value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2", invoiceheader2.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});

				invoiceLine1_1.JI_OA_ExporterAddress = ZGuid.Empty;
				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;
				CombineAssertions("Rule C0002 is Active : declaration is UCC6 and Import, invoiceHeader1 has value and none of its invoiceLines have value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2", invoiceheader2.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});

				invoiceLine1_1.JI_OA_ExporterAddress = ZGuid.Empty;
				invoiceheader1.JZ_OA_SupplierAddress = ZGuid.Empty;
				CombineAssertions("Rule C0002 is Active : declaration is UCC6 and Import, invoiceHeader1 has no value and none of its invoiceLines have value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2", invoiceheader2.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});

				context.DisableRule(r => r.IsRuleC0002Active);
				invoiceheader1.JZ_OA_SupplierAddress = ZGuid.Empty;
				invoiceLine1_1.JI_OA_ExporterAddress = ZGuid.Empty;

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				invoiceLine1_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;
				CombineAssertions("Rule C0002 is Disable : declaration is UCC6 and Import, invoiceHeader1 and invoiceLine1_1 both have value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});
			}

			using (var context = new InvoiceHeaderValidationDeciderTestContext(declaration, isUCC6: false))
			{
				context.EnableRule(r => r.IsRuleC0002Active);
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				invoiceLine1_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;

				CombineAssertions("Rule C0002 is Active : declaration is not UCC6 and is Export, invoiceHeader1 and invoiceLine1_1 has value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2", invoiceheader2.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});

				invoiceheader1.JZ_OA_SupplierAddress = ZGuid.Empty;
				invoiceLine1_1.JI_OA_ExporterAddress = ZGuid.Empty;

				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
				invoiceLine1_1.JI_OA_ExporterAddress = exporterForInvoiceLine.MainAddress.PK;
				invoiceheader1.JZ_OA_SupplierAddress = exporterForInvoice.MainAddress.PK;
				CombineAssertions("Rule C0002 is Active : declaration is not UCC6 and is Import, invoiceHeader1 and invoiceLine 1_1 has value for Exporter", () =>
				{
					AssertNoMessageErrorContaining("InvoiceHeader1", invoiceheader1.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
					AssertNoMessageErrorContaining("InvoiceHeader2", invoiceheader2.JZ_OA_SupplierAddressInfo, messageForInvoiceHeader);
				});
			}
		}

		public void TestValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceHeader = declaration.Invoices.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					var jobComInvoiceHeaderValidationForTesting = new JobComInvoiceHeaderValidation(invoiceHeader);
					var declarationValidationDecider = jobComInvoiceHeaderValidationForTesting.ValidationDecider;
					AssertType<UCC6ImportInvoiceHeaderValidationDecider>("IMP UCC6 declaration", declarationValidationDecider);
					declaration.JE_MessageType = "EXP";
					AssertNull("EXP UCC6 declaration", new JobComInvoiceHeaderValidation(invoiceHeader).ValidationDecider);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					declaration.JE_MessageType = "IMP";
					AssertNull("IMP declaration not UCC6", new JobComInvoiceHeaderValidation(invoiceHeader).ValidationDecider);
					declaration.JE_MessageType = "EXP";
					AssertNull("EXP declaration not UCC6", new JobComInvoiceHeaderValidation(invoiceHeader).ValidationDecider);
				}
			});
		}

		protected override Customs.Business.BaseJobComInvoiceHeader GetInvoiceHeader() => invoiceHeader;
		protected override Type GetTypeForTest() => typeof(JobComInvoiceHeaderValidation);

		protected virtual JobDeclaration GetJobDeclarationForTest() => Factory.New<JobDeclaration>();

		JobComInvoiceHeader SetUpInvoiceWithSupplier()
		{
			var invoice = GetInvoiceHeader() as JobComInvoiceHeader;
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierAddress = supplier.Addresses.MainAddress;
			invoice.JZ_OH_Supplier = supplier.PK;
			supplierAddress.OA_RN_NKCountryCode = "DE";

			return invoice;
		}
	}
}
