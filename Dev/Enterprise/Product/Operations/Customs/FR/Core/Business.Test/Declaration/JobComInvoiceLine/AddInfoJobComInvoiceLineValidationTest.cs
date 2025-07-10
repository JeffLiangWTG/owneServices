using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_CountryOfSupply()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
			AssertHasMessageErrorContaining(invoiceLine.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.ZG_CountryOfSupply = "FR";
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
			AssertNoMessageErrorContaining(invoiceLine.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.ZG_CountryOfSupply = "";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
			AssertNoMessageErrorContaining(invoiceLine.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
			AssertNoMessageErrorContaining(invoiceLine.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckZG_CountryOfSupplyForRuleC0699_N01()
		{
			var message = "[C0699_N01] If Pref. Code starts with 2 or 3 then the field Pref.Origin is mandatory.";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);

			CombineAssertions(() =>
			{
				context.EnableRule(x => x.IsRuleC0699_N01Active);
				invoiceLine.JI_PrimaryPreference = "200";
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertHasMessageError("A message error is expected as JI_PrimaryPreference starts with 2 and ZG_CountryOfSupply is empty.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.EnableRule(x => x.IsRuleC0699_N01Active);
				invoiceLine.JI_PrimaryPreference = "300";
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertHasMessageError("A message error is expected as JI_PrimaryPreference starts with 3 and ZG_CountryOfSupply is empty.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.EnableRule(x => x.IsRuleC0699_N01Active);
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoMessageError("No message error is expected as JI_PrimaryPreference doesn't start with 2 or 3.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.EnableRule(x => x.IsRuleC0699_N01Active);
				invoiceLine.JI_PrimaryPreference = "200";
				invoiceLine.ZG_CountryOfSupply = "CN";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoMessageError("No message error is expected as ZG_CountryOfSupply is not empty.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.DisableRule(x => x.IsRuleC0699_N01Active);
				invoiceLine.JI_PrimaryPreference = "200";
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoMessageError("No message error is expected as rule C0699_N01 is disabled.", invoiceLine.ZG_CountryOfSupplyInfo, message);
			});
		}

		public void TestCheckZG_CountryOfSupplyForRuleC0699_N02()
		{
			var message = "[C0699_N02] Because Pref. Code does not start with 2 or 3, the Pref. Origin country will not be sent in message to Customs.";

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var context = new ImportInvoiceLineValidationTestContext(invoiceLine);

			CombineAssertions(() =>
			{
				context.EnableRule(x => x.IsRuleC0699_N02Active);
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.ZG_CountryOfSupply = "CN";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertHasWarning("A message error is expected as JI_PrimaryPreference starts with 1 and ZG_CountryOfSupply is not empty.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.EnableRule(x => x.IsRuleC0699_N02Active);
				invoiceLine.JI_PrimaryPreference = "200";
				invoiceLine.ZG_CountryOfSupply = "CN";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoWarning("No message error is expected as JI_PrimaryPreference starts with 2 and ZG_CountryOfSupply is not empty.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.EnableRule(x => x.IsRuleC0699_N02Active);
				invoiceLine.JI_PrimaryPreference = "300";
				invoiceLine.ZG_CountryOfSupply = "CN";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoWarning("No message error is expected as JI_PrimaryPreference starts with 3 and ZG_CountryOfSupply is not empty.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.EnableRule(x => x.IsRuleC0699_N02Active);
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.ZG_CountryOfSupply = ZString.Empty;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoWarning("No message error is expected as JI_PrimaryPreference doesn't start with 2 or 3 and ZG_CountryOfSupply is empty.", invoiceLine.ZG_CountryOfSupplyInfo, message);

				context.DisableRule(x => x.IsRuleC0699_N02Active);
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.ZG_CountryOfSupply = "CN";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoMessageError("No message error is expected as rule C0699_N02 is disabled.", invoiceLine.ZG_CountryOfSupplyInfo, message);
			});
		}

		public void TestCheckZG_TariffBypassCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_TariffBypassCode = "X";
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffBypassCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_D;
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffBypassCodeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.EntryInstruction.ZG_BypassCode = ZString.Empty;
			invoiceLine.JI_TariffBypassCode = ZString.Empty;
			invoiceLine.RunPreSaveValidation();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffBypassCodeInfo, "Please set a value");

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_D;
			invoiceLine.RunPreSaveValidation();
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffBypassCodeInfo, "Please set a value");

			invoiceLine.EntryInstruction.ZG_BypassCode = ValuationBypassCodeList.Codes.VBC_A;
			invoiceLine.RunPreSaveValidation();
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffBypassCodeInfo, "Please set a value");
		}

		public void TestZG_TariffBypassReason()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_TariffBypassCode = "X";
			invoiceLine.JI_TariffBypassReason = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffBypassReasonInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_E;
			AssertHasMessageErrorContaining(invoiceLine.JI_TariffBypassReasonInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_TariffBypassReason = "XXX";
			AssertNoMessageErrorContaining(invoiceLine.JI_TariffBypassReasonInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckZG_ValuationMethod()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_ValuationCode = invoiceLine.Lookups.ValuationCodeList[0].Code;
			AssertNoMessageErrorContaining(invoiceLine.JI_ValuationCodeInfo, "list");

			invoiceLine.JI_ValuationCode = "x";
			AssertHasMessageErrorContaining(invoiceLine.JI_ValuationCodeInfo, "list");

			invoiceLine.JI_ValuationCode = ZString.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_ValuationCodeInfo, "list");
		}
	}
}
