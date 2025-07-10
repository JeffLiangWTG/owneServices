using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS.Messaging.Testing;

namespace Enterprise.Customs.GB.CDS.Messaging.Calculators.Testing
{
	public class H7DeclarationFreightChargeCalculatorTests : TestCaseWithFactory
	{
		GroupInvoiceChargeCollection<Business.Declaration.GroupInvoiceCharge> groupHeaderCharges;
		Business.Declaration.CusEntryHeader entryHeader;

		public void TestCurrency()
		{
			setupGroupHeaderCharge("AFT", 10m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			setupGroupHeaderCharge("OFT", 10m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			setupGroupHeaderCharge("ONS", 10m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			var freightChargeAmount = new H7DeclarationFreightChargeCalculator(entryHeader).CalculateFreightChargeAmount();
			AssertEquals("EUR", freightChargeAmount.Currency);
			AssertEquals(30.00m, freightChargeAmount.Amount);

			CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
			helper.SetExchangeRate(Core.Constants.CurrencyCodes.EuropeanUnion, 1.181m, ZDateTime.Today);
			setupGroupHeaderCharge("ONS", 10m, Core.Constants.CurrencyCodes.UnitedKingdom, "FUA");
			freightChargeAmount = new H7DeclarationFreightChargeCalculator(entryHeader).CalculateFreightChargeAmount();
			AssertEquals("GBP", freightChargeAmount.Currency);
			AssertEquals(35.41m, freightChargeAmount.Amount);
		}

		public void TestNoExceptionWithEmptyCurrency()
		{
			setupGroupHeaderCharge(ChargesProvider.AirFreightCode, 1m, null, ApportionmentTypeList.Codes.FullApportionment);
			setupGroupHeaderCharge(CustomsChargeTypeList.Codes.OverseasFreight, 3m, null, ApportionmentTypeList.Codes.FullApportionment);
			setupGroupHeaderCharge(CustomsChargeTypeList.Codes.OverseasInsurance, 5m, null, ApportionmentTypeList.Codes.FullApportionment);
			AssertNoExceptionThrown(() =>
			{
				_ = new H7DeclarationFreightChargeCalculator(entryHeader).CalculateFreightChargeAmount();
			});
		}

		public void TestFullApportionment()
		{
			setupGroupHeaderCharge("AFT", 10m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			setupGroupHeaderCharge("OFT", 10m, Core.Constants.CurrencyCodes.EuropeanUnion, "PAA");
			setupGroupHeaderCharge("ONS", 10m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			var freightChargeAmount = new H7DeclarationFreightChargeCalculator(entryHeader).CalculateFreightChargeAmount();
			AssertEquals("EUR", freightChargeAmount.Currency);
			AssertEquals(30.00m, freightChargeAmount.Amount);
		}

		public void TestIncludedCharges()
		{
			setupGroupHeaderCharge("AFT", 11m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			setupGroupHeaderCharge("OFT", 11m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			setupGroupHeaderCharge("ONS", 11m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			setupGroupHeaderCharge("ONS", 11m, Core.Constants.CurrencyCodes.EuropeanUnion, "PAA");

			setupGroupHeaderCharge("CPA", 2.1m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");
			setupGroupHeaderCharge("MAC", 2.1m, Core.Constants.CurrencyCodes.EuropeanUnion, "FUA");

			var freightChargeAmount = new H7DeclarationFreightChargeCalculator(entryHeader).CalculateFreightChargeAmount();
			AssertEquals("EUR", freightChargeAmount.Currency);
			AssertEquals(44.00m, freightChargeAmount.Amount);
		}

		public void TestNoCharges()
		{
			var freightChargeAmount = new H7DeclarationFreightChargeCalculator(entryHeader).CalculateFreightChargeAmount();
			AssertEquals("GBP", freightChargeAmount.Currency);
			AssertEquals(0.00m, freightChargeAmount.Amount);
		}

		void setupGroupHeaderCharge(string chargeCode, decimal amount, string currencyCode, string chargeApportionment)
		{
			groupHeaderCharges.AddNew(chargeCode, amount, currencyCode).J7_FullOrPartialApportionment = chargeApportionment;
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var declaration = entryHeader.Declaration;
			declaration.JE_DeclarationType = ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration;  // H7
			declaration.JE_MessageSubType = "IM";
			entryHeader.EntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.SuperReducedDataSetDeclaration;
			entryHeader.EntryInstruction.CEI_SubStyle = "Y";
			groupHeaderCharges = declaration.JobComInvoiceGroupHeaders[0].Charges;
			groupHeaderCharges.RemoveAndDeleteAll();
		}
	}
}
