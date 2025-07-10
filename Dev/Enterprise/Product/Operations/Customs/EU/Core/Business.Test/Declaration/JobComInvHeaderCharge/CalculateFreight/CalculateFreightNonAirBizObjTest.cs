using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CalculateFreightNonAirBizObj))]
	class CalculateFreightNonAirBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmountAndCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var bizObj = CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);
			AssertEquals("No freight charge is available, Currency should be defaulted to EUR.", "EUR", bizObj.Currency);
			AssertEquals("No freight charge is available, Amount should be 0.", 0m, bizObj.TotalAmount);

			var freightCharge = invoice.Charges.AddNew();
			freightCharge.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			freightCharge.J7_Amount = 100m;
			freightCharge.J7_RX_NKCurrency = "USD";

			var freightCharge2 = invoice.Charges.AddNew();
			freightCharge2.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			freightCharge2.J7_Amount = 200m;
			freightCharge2.J7_RX_NKCurrency = "USD";

			bizObj = CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should be defaulted with common currency code of charges of type OFT found.", "USD", bizObj.Currency);
			AssertEquals("Freight charges share same currency so Amount should be the sum of AFT charges.", 300m, bizObj.TotalAmount);

			var freightCharge3 = invoice.Charges.AddNew();
			freightCharge3.J7_ChargeType = ChargeTypeList.Codes.InternationalFreight;
			freightCharge3.J7_Amount = 400m;
			freightCharge3.J7_RX_NKCurrency = "EUR";

			bizObj = CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should not be defaulted because all charges of type OFT found don't share same currency.", ZString.Empty, bizObj.Currency);
			AssertEquals("Currency could not be determined, Amount should be 0.", 0m, bizObj.TotalAmount);

			freightCharge.J7_RX_NKCurrency = ZString.Empty;
			freightCharge2.J7_RX_NKCurrency = ZString.Empty;
			freightCharge3.J7_RX_NKCurrency = ZString.Empty;

			bizObj = CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);
			AssertEquals("Currency should be defaulted with common currency code of charges of type OFT found, even if empty.", ZString.Empty, bizObj.Currency);
			AssertEquals("Currency could be determined as empty, Amount should be the sum of OFT charges.", 700m, bizObj.TotalAmount);
		}

		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertType<CalculateFreightNonAirBizObj>("Default Type", CalculateFreightNonAirBizObj.New(invoice.Charges, declaration));
				AssertEquals("CalculateFreightNonAirBizObj Has no changes", false, CalculateFreightNonAirBizObj.New(invoice.Charges, declaration).HasChanges);
				AssertNull("Declaration is Null", CalculateFreightNonAirBizObj.New(invoice.Charges, null));
			});
		}

		public void TestDefaultValues()
		{
			var bizObj = (CalculateFreightNonAirBizObj)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("PercentageFreightToEUBorder", 100m, bizObj.PercentageFreightToEUBorder);
				AssertEquals("Currency", Core.Constants.CurrencyCodes.EuropeanUnion, bizObj.Currency);
				AssertEquals("TotalAmount", 0m, bizObj.TotalAmount);
				AssertEquals("HasChanges", false, bizObj.HasChanges);
			});
		}

		public void TestPercentageFreightToEUBorderAndFinalDestinationCalculation()
		{
			var bizObj = (CalculateFreightNonAirBizObj)GetNewBusinessObject();
			bizObj.TotalAmount = 1000m;
			bizObj.PercentageFreightToEUBorder = 70m;
			CombineAssertions(() =>
			{
				AssertEquals("PercentageFreightEUToDestinationCountry (PercentageFreightToEUBorder + PercentageFreightToFinalDestination)", 0m, bizObj.PercentageFreightEUToDestinationCountry);
				AssertEquals("PercentageFreightToFinalDestination (PercentageFreightToEUBorder + PercentageFreightToFinalDestination)", 30m, bizObj.PercentageFreightToFinalDestination);
				AssertEquals("AmountToEUBorder (PercentageFreightToEUBorder + PercentageFreightToFinalDestination)", 700m, bizObj.AmountToEUBorder);
				AssertEquals("AmountToDestinationCountry (PercentageFreightToEUBorder + PercentageFreightToFinalDestination)", 0m, bizObj.AmountToDestinationCountry);
				AssertEquals("AmountToFinalDestination (PercentageFreightToEUBorder + PercentageFreightToFinalDestination)", 300m, bizObj.AmountToFinalDestination);
			});
		}

		public void TestPercentageFreightToEUBorderEUDestinationAndFinalDestinationCalculation()
		{
			var bizObj = (CalculateFreightNonAirBizObj)GetNewBusinessObject();
			bizObj.TotalAmount = 1000m;
			bizObj.PercentageFreightToEUBorder = 70m;
			bizObj.PercentageFreightEUToDestinationCountry = 20m;
			CombineAssertions(() =>
			{
				AssertEquals("PercentageFreightToFinalDestination (PercentageFreightToEUBorder + PercentageFreightEUToDestinationCountry + PercentageFreightToFinalDestination)", 10m, bizObj.PercentageFreightToFinalDestination);
				AssertEquals("AmountToEUBorder (PercentageFreightToEUBorder + PercentageFreightEUToDestinationCountry + PercentageFreightToFinalDestination)", 700m, bizObj.AmountToEUBorder);
				AssertEquals("AmountToDestinationCountry (PercentageFreightToEUBorder + PercentageFreightEUToDestinationCountry + PercentageFreightToFinalDestination)", 200m, bizObj.AmountToDestinationCountry);
				AssertEquals("AmountToFinalDestination (PercentageFreightToEUBorder + PercentageFreightEUToDestinationCountry + PercentageFreightToFinalDestination)", 100m, bizObj.AmountToFinalDestination);
			});
		}

		public void TestDefaultTotalAmountFromExistingRelevantCharges()
		{
			var euIncoTermAndChargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var charge1 = invoice.Charges.AddNew();
			charge1.J7_ChargeType = euIncoTermAndChargeFactory.FreightToEUBorderCode;
			charge1.J7_Amount = 600m;
			var charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = euIncoTermAndChargeFactory.FreightAfterEUBorderCode;
			charge2.J7_Amount = 300m;
			var charge3 = invoice.Charges.AddNew();
			charge3.J7_ChargeType = euIncoTermAndChargeFactory.FreightDomesticCode;
			charge3.J7_Amount = 100m;
			var charge4 = invoice.Charges.AddNew();
			charge4.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			charge4.J7_Amount = 500m;
			var bizObj = (CalculateFreightNonAirBizObj)GetNewBusinessObject();
			AssertEquals("TotalAmount from invoice charges, excluding irrelevant charges", 1000m, bizObj.TotalAmount);
		}

		public void TestChargeTypesToCalculate()
		{
			var bizObj = (CalculateFreightNonAirBizObj)GetNewBusinessObject();
			AssertEquals(CustomsChargeTypeList.Codes.OverseasFreight, bizObj.ChargeTypesToCalculate.Single());
		}

		public void TestCalculate()
		{
			var bizObj = (CalculateFreightNonAirBizObj)GetNewBusinessObject();
			var euIncoTermAndChargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var charge1 = invoice.Charges.AddNew();
			charge1.J7_ChargeType = euIncoTermAndChargeFactory.FreightToEUBorderCode;
			var charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = euIncoTermAndChargeFactory.FreightToEUBorderCode;
			var charge3 = invoice.Charges.AddNew();
			charge3.J7_ChargeType = euIncoTermAndChargeFactory.FreightAfterEUBorderCode;
			var charge4 = invoice.Charges.AddNew();
			charge4.J7_ChargeType = euIncoTermAndChargeFactory.FreightDomesticCode;
			var charge5 = invoice.Charges.AddNew();
			charge5.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;

			CombineAssertions(() =>
			{
				bizObj.TotalAmount = 1000m;
				bizObj.Currency = Core.Constants.CurrencyCodes.Dominica;
				bizObj.PercentageFreightToEUBorder = 70;
				bizObj.PercentageFreightEUToDestinationCountry = 40;
				AssertEquals("Calculation not performed if there's validation error", false, bizObj.Calculate());
				AssertEquals("Existing charges remain", 5, invoice.Charges.Count);
				AssertEquals("Charge1 not deleted", false, charge1.IsDeleted);
				AssertEquals("Charge2 not deleted", false, charge2.IsDeleted);
				AssertEquals("Charge3 not deleted", false, charge3.IsDeleted);
				AssertEquals("Charge4 not deleted", false, charge4.IsDeleted);
				AssertEquals("Charge5 not deleted", false, charge5.IsDeleted);

				bizObj.PercentageFreightEUToDestinationCountry = 20;
				AssertEquals("Calculation performed", true, bizObj.Calculate());
				AssertEquals("1 existing charge + 3 new charges", 4, invoice.Charges.Count);
				AssertEquals("Charge1 deleted", true, charge1.IsDeleted);
				AssertEquals("Charge2 deleted", true, charge2.IsDeleted);
				AssertEquals("Charge3 deleted", true, charge3.IsDeleted);
				AssertEquals("Charge4 deleted", true, charge4.IsDeleted);
				AssertEquals("Charge5 not deleted because code is irrelevant", false, charge5.IsDeleted);

				var chargeFreightToEUBorder = invoice.Charges.OfType<InvoiceCharge>().Single(x => x.J7_ChargeType == euIncoTermAndChargeFactory.FreightToEUBorderCode && x.J7_IsDutiable);
				AssertCharge("Freight to EU Border", chargeFreightToEUBorder, 700m, Core.Constants.CurrencyCodes.Dominica);

				var chargeFreightToDestinationCountry = invoice.Charges.OfType<InvoiceCharge>().Single(x => x.J7_ChargeType == euIncoTermAndChargeFactory.FreightAfterEUBorderCode && !x.J7_IsDutiable && x.J7_IsStatisticalValueApplicable);
				AssertCharge("Freight to Destination Country", chargeFreightToDestinationCountry, 200m, Core.Constants.CurrencyCodes.Dominica);

				var chargeFreightToFinalDestination = invoice.Charges.OfType<InvoiceCharge>().Single(x => x.J7_ChargeType == euIncoTermAndChargeFactory.FreightDomesticCode && !x.J7_IsDutiable && !x.J7_IsStatisticalValueApplicable);
				AssertCharge("Freight to Final Destination", chargeFreightToFinalDestination, 100m, Core.Constants.CurrencyCodes.Dominica);

				bizObj.PercentageFreightEUToDestinationCountry = 30;
				AssertEquals(true, bizObj.Calculate());
				chargeFreightToFinalDestination = invoice.Charges.OfType<InvoiceCharge>().SingleOrDefault(x => x.J7_ChargeType == euIncoTermAndChargeFactory.FreightDomesticCode && !x.J7_IsDutiable && !x.J7_IsStatisticalValueApplicable);
				AssertEquals("No charge created if amount is 0", null, chargeFreightToFinalDestination);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;

		void AssertCharge(ZString message, InvoiceCharge charge, ZDecimal expectedAmount, ZString expectedCurrency)
		{
			AssertEquals($"{message} Amount", expectedAmount, charge.J7_Amount);
			AssertEquals($"{message} Currency", expectedCurrency, charge.J7_RX_NKCurrency);
		}
	}
}
