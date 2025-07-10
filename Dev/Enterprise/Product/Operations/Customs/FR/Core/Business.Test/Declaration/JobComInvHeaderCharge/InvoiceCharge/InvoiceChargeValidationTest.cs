using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class InvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var parent = Factory.New<InvoiceCharge>();
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestValidationOfJ7_ChargeType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = IncoTerms.DeliveredDutyPaid;
			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge;
			AssertNoWarnings("Should has no warning", charge.J7_ChargeTypeInfo);

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = IncoTerms.DeliveredAtPlaceUnloaded;
			charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge;
			AssertHasWarning("Should has this warning", charge.J7_ChargeTypeInfo, "IDO charge only applies to INCO Term DDP.");

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = IncoTerms.DeliveredAtPlaceUnloaded;
			charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AdjustmentCharge;
			AssertNoWarnings("Should has no warning", charge.J7_ChargeTypeInfo);
		}

		public void TestValidationOfJ7_ChargeType_RuleNat237()
		{
			var message = "Rule Nat_237 - This charge type has already been entered at this level";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = IncoTerms.DeliveredDutyPaid;
			var charge = invoice.Charges.AddNew();

			using (var context = new JobComInvoiceHeaderValidationDeciderTestContext(invoice, isUCC6: true))
			{
				context.EnableRule(x => x.IsRuleNAT_237Active);
				charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				AssertNoMessageErrorContaining("No duplication within the Charge Type of this Invoice => no error.", charge.J7_ChargeTypeInfo, message);

				var invoice2 = declaration.Invoices.AddNew();
				var charge2 = invoice2.Charges.AddNew();
				charge2.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				AssertNoMessageErrorContaining("No duplication within the Charge Type of this Invoice => no error.", charge2.J7_ChargeTypeInfo, message);

				var charge3 = invoice.Charges.AddNew();
				charge3.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				AssertHasMessageErrorContaining("there is duplicate within the Charge Type of this Invoice => error.", charge3.J7_ChargeTypeInfo, message);
			}

			using (var context = new JobComInvoiceHeaderValidationDeciderTestContext(invoice, isUCC6: false))
			{
				context.DisableRule(x => x.IsRuleNAT_237Active);
				charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				var charge3 = invoice.Charges.AddNew();
				charge3.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				AssertNoMessageErrorContaining("there is duplicate within the Charge Type of this Invoice but rule 237 is inactive => no error.", charge3.J7_ChargeTypeInfo, message);
			}
		}

		public void TestValidationOfJ7_ChargeType_RuleNat154()
		{
			var message = "Rule Nat_154 - This charge type has already been entered at Invoice Line level.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			using (var context = new JobComInvoiceHeaderValidationDeciderTestContext(invoice, isUCC6: true))
			{
				context.EnableRule(x => x.IsRuleNAT_154Active);
				charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				AssertNoMessageErrorContaining("This charge type has not been set at invoiceLine level => no error.",
					charge.J7_ChargeTypeInfo, message);

				var invoiceLine = invoice.InvoiceLines.AddNew();
				var charge2 = invoiceLine.Charges.AddNew();
				charge2.J7_ChargeType = FRCustomsChargeTypeList.Codes.ContainersAndPackingCharge;
				charge.Validation.ValidateAll();
				AssertNoMessageErrorContaining("Cut has not been set at invoiceLine level => no error.",
					charge.J7_ChargeTypeInfo, message);

				charge2.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				charge.Validation.ValidateAll();
				AssertHasMessageErrorContaining("Cut has been set at invoiceLine level too => error.",
					charge.J7_ChargeTypeInfo, message);

				charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
				charge2.J7_ChargeType = FRCustomsChargeTypeList.Codes.AirInsuranceCostsCharge;
				charge.Validation.ValidateAll();
				AssertNoMessageErrorContaining(
					"there is duplicate within the Charge Type of this Invoice and its InvoiceLine but these are transport charges => no error.",
					charge.J7_ChargeTypeInfo, message);
			}

			using (var context = new JobComInvoiceHeaderValidationDeciderTestContext(invoice, isUCC6: false))
			{
				context.DisableRule(x => x.IsRuleNAT_154Active);
				charge.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				var invoiceLine = invoice.InvoiceLines.AddNew();
				var charge2 = invoiceLine.Charges.AddNew();
				charge2.J7_ChargeType = FRCustomsChargeTypeList.Codes.Cut;
				charge.Validation.ValidateAll();
				AssertNoMessageErrorContaining(
					"Cut has been set at invoiceLine level too but rule 154 is disable => no error.",
					charge.J7_ChargeTypeInfo, message);
			}
		}
	}
}
