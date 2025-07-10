using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CustomsValuationWrapperTest : Customs.Business.Testing.DataProviderTestCase<CustomsValuationWrapper>
	{
		public void TestValuationMethod()
		{
			AssertEquals("ValuationMethod should be equal to JI_ValuationCode.", Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1, Provider.ValuationMethod);
		}

		public void TestAdditionsAndDeductions()
		{
			AssertType<Collection<CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces.IAdditionsAndDeductions>>("AdditionsAndDeductions should be a Collection of CustomsValuationWrapper.", Provider.AdditionsAndDeductions);
			AssertContainsExactElementsInAnyOrder("There should be 2 AdditionsAndDeductions elements as both Charges and Apportioned Charges should be merged by charge type.", new string[] { "AK|132USD", "CA|264USD", "BA|8USD", "AB|97USD", "CZ|2USD" }, Provider.AdditionsAndDeductions.Select(x => x.Code + "|" + x.Amount.ToString() + x.Currency));

			var wrapper = CustomsValuationWrapper.New(Factory.New<CusEntryLine>());
			AssertEquals("There should be 0 AdditionsAndDeductions.", 0, wrapper.AdditionsAndDeductions.Count);

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			wrapper = CustomsValuationWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("When the conditions of Nat_146 are met, transport charge BA and AK will not be calculated", new string[] { "CA|264USD", "AB|97USD", "CZ|2USD" }, wrapper.AdditionsAndDeductions.Select(x => x.Code + "|" + x.Amount.ToString() + x.Currency));

			invoiceHeader.JZ_IncoTerm = "";
			invoiceLine.Charges.Cast<InvoiceLineCharge>().First(x => x.J7_ChargeType == "ONS").J7_IsIncludedInITOT = true;
			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes._1277;
			wrapper = CustomsValuationWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("If supplementary code uses '1277', transport charge CA and CZ will not be calculated", new string[] { "AK|132USD", "BA|8USD", "AB|97USD" }, wrapper.AdditionsAndDeductions.Select(x => x.Code + "|" + x.Amount.ToString() + x.Currency));

			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._2;
			wrapper = CustomsValuationWrapper.New(entryLine);
			AssertContainsExactElementsInAnyOrder("If Valuation Method is other than 1, then only calculate codes AK / BA/ AN / BG / BC / CA / CZ.", new string[] { "AK|132USD", "CA|264USD", "BA|8USD", "CZ|2USD" }, wrapper.AdditionsAndDeductions.Select(x => x.Code + "|" + x.Amount.ToString() + x.Currency));
		}

		protected override CustomsValuationWrapper GetProvider()
		{
			var referenceDataHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateCusMapType("CHG", "OUT", "CW1 Charge Codes to Customs codes", true);
			referenceDataHelper.CreateCusMap("CHG", "COM", "AB", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "FR");
			referenceDataHelper.CreateCusMap("CHG", "CPA", "CZ", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "FR");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryLine = (CusEntryLine)entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_ValuationCode = Enterprise.MasterFiles.Business.Customs.EU.ValuationMethodList.Codes._1;

			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "COM";
			charge.J7_Amount = 1m;
			charge.J7_RX_NKCurrency = "USD";

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "CPA";
			charge2.J7_Amount = 2m;
			charge2.J7_RX_NKCurrency = "USD";

			var transportCharge1 = invoiceLine.Charges.AddNew();
			transportCharge1.J7_ChargeType = "OFT";
			transportCharge1.J7_Amount = 4m;
			transportCharge1.J7_IsDutiable = true;
			transportCharge1.J7_IsIncludedInITOT = false;
			transportCharge1.J7_RX_NKCurrency = "USD";

			var transportCharge2 = invoiceLine.Charges.AddNew();
			transportCharge2.J7_ChargeType = "ONS";
			transportCharge2.J7_Amount = 8m;
			transportCharge2.J7_IsDutiable = false;
			transportCharge2.J7_IsIncludedInITOT = true;
			transportCharge2.J7_RX_NKCurrency = "USD";

			var invalidCharge = invoiceLine.Charges.AddNew();
			invalidCharge.J7_ChargeType = "ABC";
			invalidCharge.J7_Amount = 16m;
			invalidCharge.J7_RX_NKCurrency = "USD";

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "COM";
			apportionedCharge1.J7_Amount = 32m;
			apportionedCharge1.J7_RX_NKCurrency = "USD";

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "COM";
			apportionedCharge2.J7_Amount = 64m;
			apportionedCharge2.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge1.J7_ChargeType = "CNE";
			transportApportionedCharge1.J7_Amount = 128m;
			transportApportionedCharge1.J7_IsDutiable = true;
			transportApportionedCharge1.J7_IsIncludedInITOT = false;
			transportApportionedCharge1.J7_RX_NKCurrency = "USD";

			var transportApportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			transportApportionedCharge2.J7_ChargeType = "CEI";
			transportApportionedCharge2.J7_Amount = 256m;
			transportApportionedCharge2.J7_IsDutiable = false;
			transportApportionedCharge2.J7_IsIncludedInITOT = false;
			transportApportionedCharge2.J7_RX_NKCurrency = "USD";

			var invalidApportionedCharge = invoiceLine.ApportionedCharges.AddNew();
			invalidApportionedCharge.J7_ChargeType = "ABC";
			invalidApportionedCharge.J7_Amount = 512m;
			invalidApportionedCharge.J7_RX_NKCurrency = "USD";

			return CustomsValuationWrapper.New(entryLine);
		}

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
	}
}
