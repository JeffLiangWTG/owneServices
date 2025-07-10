using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class InvoiceLineTraderJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRuleC0728_ForBuyerDocAddress()
		{
			var messageError = "[C0728] – This field must be empty for this Requested Procedure / Additional Declaration / Declaration Sub Type.";

			var declaration = Factory.New<JobDeclaration>();
			var organizationPK = Factory.New<OrgHeader>().PK;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var line = GetLine(ZString.Empty, "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C.", line.BuyerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine(ZString.Empty, "F", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is F.", line.BuyerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine(ZString.Empty, "A", organizationPK);
				AssertNoNotifications("No message error is expected when subtype is other than C or F.", line.BuyerDocAddress.OrganisationPKInfo);

				line = GetLine(ZString.Empty, ZString.Empty, organizationPK);
				AssertNoNotifications("No message error is expected when Procedure is empty.", line.BuyerDocAddress.OrganisationPKInfo);

				line = GetLine("5123434", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 51.", line.BuyerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("5323434", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 53.", line.BuyerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("7100000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 71.", line.BuyerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("1234567", ZString.Empty, organizationPK);
				AssertNoNotifications("No message error is expected when ProcedureCode is other than 51, 52 or 53", line.BuyerDocAddress.OrganisationPKInfo);

				line = GetLine("1234F15", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when Concession is F15", line.BuyerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("7100000", ZString.Empty, organizationPK, MessageTypeList.Codes.Export);
				AssertNoNotifications("Validation for rule C0728 should not run when declaration is of type export.", line.BuyerDocAddress.OrganisationPKInfo);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var line = GetLine("7100000", ZString.Empty, organizationPK);
				AssertNoNotifications("Validation for rule C0728 should not run for non UCC6 declarations.", line.BuyerDocAddress.OrganisationPKInfo);
			}

			declaration = Factory.New<JobDeclaration>();
			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.DisableRule(r => r.IsRuleC0728Active);
				var line = GetLine(ZString.Empty, "C", organizationPK);
				AssertNoMessageErrorContaining("No validation should run when RuleC0728 is disabled.", line.BuyerDocAddress.OrganisationPKInfo, messageError);

				context.EnableRule(r => r.IsRuleC0728Active);
				line = GetLine(ZString.Empty, "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C and RuleC0728 is enabled.", line.BuyerDocAddress.OrganisationPKInfo, messageError);
			}

			JobComInvoiceLine GetLine(ZString ji_Procedure, ZString cei_SubStyle, ZGuid orgPK, string messageType = MessageTypeList.Codes.Import)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				declaration.CustomsEntryInstructions?.RemoveAndDeleteAll();
				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				var cei = declaration.CustomsEntryInstructions.AddNew();
				line.JI_CEI = cei.PK;
				cei.CEI_SubStyle = cei_SubStyle;
				line.JI_FormattedProcedure = ji_Procedure;
				line.BuyerDocAddress.OrganisationPK = orgPK;
				return line;
			}
		}

		public void TestCheckRuleC0728_ForSellerDocAddress()
		{
			var messageError = "[C0728] – This field must be empty for this Requested Procedure / Additional Declaration / Declaration Sub Type.";

			var declaration = Factory.New<JobDeclaration>();
			var organizationPK = Factory.New<OrgHeader>().PK;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var line = GetLine(ZString.Empty, "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C.", line.SellerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine(ZString.Empty, "F", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is F.", line.SellerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine(ZString.Empty, "A", organizationPK);
				AssertNoNotifications("No message error is expected when subtype is other than C or F.", line.SellerDocAddress.OrganisationPKInfo);

				line = GetLine(ZString.Empty, ZString.Empty, organizationPK);
				AssertNoNotifications("No message error is expected when Procedure is empty.", line.SellerDocAddress.OrganisationPKInfo);

				line = GetLine("5123434", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 51.", line.SellerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("5323434", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 53.", line.SellerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("7100000", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when ProcedureCode is 71.", line.SellerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("1234567", ZString.Empty, organizationPK);
				AssertNoNotifications("No message error is expected when ProcedureCode is other than 51, 52 or 53", line.SellerDocAddress.OrganisationPKInfo);

				line = GetLine("1234F15", ZString.Empty, organizationPK);
				AssertHasMessageError("A message error is expected when Concession is F15", line.SellerDocAddress.OrganisationPKInfo, messageError);

				line = GetLine("7100000", ZString.Empty, organizationPK, MessageTypeList.Codes.Export);
				AssertNoNotifications("Validation for rule C0728 should not run when declaration is of type export.", line.SellerDocAddress.OrganisationPKInfo);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var line = GetLine("7100000", ZString.Empty, organizationPK);
				AssertNoNotifications("Validation for rule C0728 should not run for non UCC6 declarations.", line.SellerDocAddress.OrganisationPKInfo);
			}

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleC0728Active);
				var line = GetLine(ZString.Empty, "C", organizationPK);
				AssertHasMessageError("A message error is expected when subtype is C and RuleC0728 is enabled.", line.SellerDocAddress.OrganisationPKInfo, messageError);

				context.DisableRule(r => r.IsRuleC0728Active);
				line = GetLine(ZString.Empty, "C", organizationPK);
				AssertNoMessageErrorContaining("No validation is expected when RuleC0728 is disabled.", line.SellerDocAddress.OrganisationPKInfo, messageError);
			}

			JobComInvoiceLine GetLine(ZString ji_Procedure, ZString cei_SubStyle, ZGuid orgPK, string messageType = MessageTypeList.Codes.Import)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				declaration.CustomsEntryInstructions?.RemoveAndDeleteAll();
				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				var cei = declaration.CustomsEntryInstructions.AddNew();
				line.JI_CEI = cei.PK;
				cei.CEI_SubStyle = cei_SubStyle;
				line.JI_FormattedProcedure = ji_Procedure;
				line.SellerDocAddress.OrganisationPK = orgPK;
				return line;
			}
		}

		public void TestCheckOrganisationPK_NoValidationOnDuplicatedBuyerDocAddressPerformed_WhenParentDeclarationIsNonUCC6OrExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				SetUpInvoiceWithBuyerOrgPK(declaration);
				var invoiceLine = SetUpInvoiceLineWithValidBuyerDocAddress(declaration);
				invoiceLine.BuyerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 Configuration set", expected: false, declaration.IsUCC6);
				AssertEquals("Prerequisite: Is Import Configuration set", expected: true, declaration.IsImport);
				AssertNoNotifications(invoiceLine.BuyerDocAddress.OrganisationPKInfo);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				SetUpInvoiceWithBuyerOrgPK(declaration);
				var invoiceLine = SetUpInvoiceLineWithValidBuyerDocAddress(declaration);
				invoiceLine.BuyerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 Configuration set", expected: true, declaration.IsUCC6);
				AssertEquals("Prerequisite: Is Import Configuration set", expected: false, declaration.IsImport);
				AssertNoNotifications(invoiceLine.BuyerDocAddress.OrganisationPKInfo);
			}
		}

		public void TestCheckOrganisationPK_AddsMessageError_WhenBuyerDocAddressAndBuyerOrgPKAreBothEntered()
		{
			var messageError = "[R0012] Value can't be entered in both invoice header and invoice lines";
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				SetUpInvoiceWithBuyerOrgPK(declaration);
				var invoiceLine = SetUpInvoiceLineWithValidBuyerDocAddress(declaration);
				invoiceLine.BuyerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 and import Configuration set", expected: true, declaration.IsUCC6AndIsImport);
				AssertHasMessageError(invoiceLine.BuyerDocAddress.OrganisationPKInfo, messageError);
			}
		}

		public void TestCheckOrganisationPK_NoMessageError_WhenBuyerDocAddressEnteredForInvoiceLineOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				var invoiceLine = SetUpInvoiceLineWithValidBuyerDocAddress(declaration);
				invoiceLine.BuyerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 and import Configuration set", expected: true, declaration.IsUCC6AndIsImport);
				AssertNoNotifications(invoiceLine.BuyerDocAddress.OrganisationPKInfo);
			}
		}

		public void TestCheckOrganisationPK_NoMessageError_WhenBuyerOrgPKEnteredForInvoiceOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				SetUpInvoiceWithBuyerOrgPK(declaration);
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.BuyerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 and import Configuration set", expected: true, declaration.IsUCC6AndIsImport);
				AssertNoNotifications(invoiceLine.BuyerDocAddress.OrganisationPKInfo);
			}
		}

		public void TestCheckOrganisationPK_NoValidationOnDuplicatedSellerDocAddressPerformed_WhenParentDeclarationIsNonUCC6OrExport()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				SetUpInvoiceWithValidSellerOrgPK(declaration);
				var invoiceLine = SetUpInvoiceLineWithValidSellerDocAddress(declaration);
				invoiceLine.SellerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 Configuration set", expected: false, declaration.IsUCC6);
				AssertEquals("Prerequisite: Is Import Configuration set", expected: true, declaration.IsImport);
				AssertNoNotifications(invoiceLine.SellerDocAddress.OrganisationPKInfo);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				SetUpInvoiceWithValidSellerOrgPK(declaration);
				var invoiceLine = SetUpInvoiceLineWithValidSellerDocAddress(declaration);
				invoiceLine.SellerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 Configuration set", expected: true, declaration.IsUCC6);
				AssertEquals("Prerequisite: Is Import Configuration set", expected: false, declaration.IsImport);
				AssertNoNotifications(invoiceLine.SellerDocAddress.OrganisationPKInfo);
			}
		}

		public void TestCheckOrganisationPK_AddsMessageError_WhenSellerDocAddressAndSellerOrgPKAreBothEntered()
		{
			var messageError = "[R0012] Value can't be entered in both invoice header and invoice lines";
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				SetUpInvoiceWithValidSellerOrgPK(declaration);
				var invoiceLine = SetUpInvoiceLineWithValidSellerDocAddress(declaration);
				invoiceLine.SellerDocAddress.Validation.ValidateOrganisationPK();
				AssertEquals("Prerequisite: Is UCC6 and import Configuration set", expected: true, declaration.IsUCC6AndIsImport);
				AssertHasMessageError(invoiceLine.SellerDocAddress.OrganisationPKInfo, messageError);
			}
		}

		public void TestCheckOrganisationPK_NoMessageError_WhenSellerDocAddressEnteredForInvoiceLineOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				var invoiceLine = SetUpInvoiceLineWithValidSellerDocAddress(declaration);
				invoiceLine.SellerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 and import Configuration set", expected: true, declaration.IsUCC6AndIsImport);
				AssertNoNotifications(invoiceLine.SellerDocAddress.OrganisationPKInfo);
			}
		}

		public void TestCheckOrganisationPK_NoMessageError_WhenSellerOrgPKEnteredForInvoiceOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				SetUpInvoiceWithValidSellerOrgPK(declaration);
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.SellerDocAddress.Validation.ValidateOrganisationPK();

				AssertEquals("Prerequisite: Is UCC6 and import Configuration set", expected: true, declaration.IsUCC6AndIsImport);
				AssertNoNotifications(invoiceLine.SellerDocAddress.OrganisationPKInfo);
			}
		}

		void SetUpInvoiceWithValidSellerOrgPK(JobDeclaration declaration)
		{
			var invoice = declaration.Invoices.AddNew();
			var organization = Factory.New<OrgHeader>();
			invoice.SellerOrgPK = organization.PK;
		}

		JobComInvoiceLine SetUpInvoiceLineWithValidSellerDocAddress(JobDeclaration declaration)
		{
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.SellerDocAddress.OrganisationPK = orgHeader.PK;
			return invoiceLine;
		}

		void SetUpInvoiceWithBuyerOrgPK(JobDeclaration declaration)
		{
			var invoice = declaration.Invoices.AddNew();
			var organization = Factory.New<OrgHeader>();
			invoice.BuyerOrgPK = organization.PK;
		}

		JobComInvoiceLine SetUpInvoiceLineWithValidBuyerDocAddress(JobDeclaration declaration)
		{
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.BuyerDocAddress.OrganisationPK = orgHeader.PK;
			return invoiceLine;
		}
	}
}
