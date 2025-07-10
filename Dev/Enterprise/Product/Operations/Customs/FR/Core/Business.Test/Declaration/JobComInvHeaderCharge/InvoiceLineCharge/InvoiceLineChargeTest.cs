using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	class InvoiceLineChargeTest : EU.Business.Declaration.Testing.InvoiceLineChargeTest
	{
		public new void TestLookups()
		{
			AssertType<InvoiceLineChargeLookups>(Factory.New<InvoiceLineCharge>().Lookups);
		}

		public new void TestValidation()
		{
			AssertType<InvoiceLineChargeValidation>(Factory.New<InvoiceLineCharge>().Validation);
		}

		public void TestJ7_ChargeType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.EntryInstruction.ZG_BypassCode = ValuationBypassCodeList.Codes.VBC_F;
			var charge = invoiceLine.Charges.AddNew();

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;

			CombineAssertions("NOT CBR", () =>
			{
				AssertEquals("no modification declaration ZG_bypasscode", ValuationBypassCodeList.Codes.VBC_F, invoiceLine.EntryInstruction.ZG_BypassCode);
			});

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge;
			CombineAssertions("CBR", () =>
			{
				AssertEquals("modification declaration ZG_bypasscode", ValuationBypassCodeList.Codes.VBC_J, invoiceLine.EntryInstruction.ZG_BypassCode);
			});
		}

		public new void TestUpdateJ7_IsIncludedInInvoiceWhenChargeTypeIsEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AdjustmentCharge;
			AssertChargeFlags(charge, false, false, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge;
			AssertChargeFlags(charge, true, true, false, false, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge;
			AssertChargeFlags(charge, true, true, false, false, false);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge;
			AssertChargeFlags(charge, false, false, true, true, true);

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge;
			AssertChargeFlags(charge, false, false, true, true, true);
		}

		void AssertChargeFlags(InvoiceLineCharge charge, bool isIncludedInInvoice, bool j7_IsIncludedInITOT, bool j7_IsDutiable, bool j7_IsStatisticalValueApplicable, bool j7_IsGSTApplicable)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Included In Invoice", isIncludedInInvoice, !charge.J7_IsNotIncludedInInvoice);
				AssertEquals("Included In Invoice Line", j7_IsIncludedInITOT, charge.J7_IsIncludedInITOT);
				AssertEquals("Dutiable", j7_IsDutiable, charge.J7_IsDutiable);
				AssertEquals("Statable", j7_IsStatisticalValueApplicable, charge.J7_IsStatisticalValueApplicable);
				AssertEquals("VATible", j7_IsGSTApplicable, charge.J7_IsGSTApplicable);
			});
		}
	}
}
