using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class SupplierBuyerLinkGroupChargesPopulatorTest : TestCaseWithFactory
	{
		public void TestPopulateCharges()
		{
			var invoice1 = groupInvoiceHeader.AllJobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoice2 = groupInvoiceHeader.AllJobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 200m;
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var charge1 = groupInvoiceHeader.Charges.AddNew();
			charge1.J7_ChargeType = FRCustomsChargeTypeList.Codes.Additions71Charge;
			charge1.J7_IsCalculated = true;

			var populator = new SupplierBuyerLinkGroupChargesPopulator(groupInvoiceHeader);
			populator.PopulateCharges();

			Assert(!groupInvoiceHeader.Charges.Contains(charge1));
			AssertEquals(1, groupInvoiceHeader.Charges.Count);
			var charge = groupInvoiceHeader.Charges.Cast<GroupInvoiceCharge>().FirstOrDefault();
			AssertEquals(27.19m, charge.J7_Amount);
			AssertEquals("EUR", charge.J7_RX_NKCurrency);
			AssertEquals(FRCustomsChargeTypeList.Codes.InsuranceCostsCharge, charge.J7_ChargeType);

			invoice2.JZ_InvoiceAmount = 300m;
			AssertEquals(1, groupInvoiceHeader.Charges.Count);
			AssertEquals(37.19m, charge.J7_Amount);
			AssertEquals("EUR", charge.J7_RX_NKCurrency);
			AssertEquals(FRCustomsChargeTypeList.Codes.InsuranceCostsCharge, charge.J7_ChargeType);

			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(1, groupInvoiceHeader.Charges.Count);
			AssertEquals(40m, charge.J7_Amount);
			AssertEquals("USD", charge.J7_RX_NKCurrency);
			AssertEquals(FRCustomsChargeTypeList.Codes.InsuranceCostsCharge, charge.J7_ChargeType);
		}

		public void TestPopulateCharges_NoExceptionWhenInvoice_CurrencyIsNull()
		{
			var invoice1 = groupInvoiceHeader.AllJobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 100m;
			invoice1.JZ_RX_NKInvoice_Currency = "XXX";

			var charge1 = groupInvoiceHeader.Charges.AddNew();
			charge1.J7_ChargeType = FRCustomsChargeTypeList.Codes.Additions71Charge;
			charge1.J7_IsCalculated = true;

			var populator = new SupplierBuyerLinkGroupChargesPopulator(groupInvoiceHeader);
			AssertNoExceptionThrown(() => populator.PopulateCharges());

			var charge = groupInvoiceHeader.Charges.Cast<GroupInvoiceCharge>().FirstOrDefault();
			AssertEquals(Core.Constants.CurrencyCodes.EuropeanUnion, charge.J7_RX_NKCurrency);
		}

		protected override void SetUp()
		{
			base.SetUp();

			link = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			link.OL_InsuranceUplift = 10m;
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = link.OL_OH_Supplier;
			declaration.JE_OH_Buyer = link.OL_OH_Buyer;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Australia;
			groupInvoiceHeader = declaration.TopGroupInvoice;
		}
		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupInvoiceHeader;
		OrgSupplierBuyerLink link;
	}
}
