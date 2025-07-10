using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CalculateFreightNonAirBizObj))]
	sealed class CalculateFreightNonAirBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNew_InvoiceCharges()
		{
			AssertType<CalculateFreightNonAirBizObj>(EU.Business.Declaration.CalculateFreightNonAirBizObj.New(invoice.Charges, declaration));
		}

		public void TestNew_GroupInvoiceCharges()
		{
			var invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			AssertType<CalculateFreightNonAirBizObj>(EU.Business.Declaration.CalculateFreightNonAirBizObj.New(invoiceGroupHeader.Charges, declaration));
		}

		public void TestGetChargeTypesToCalculate()
		{
			var euIncoTermAndChargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var bizObj = (CalculateFreightNonAirBizObj)GetNewBusinessObject();
			AssertContainsExactElementsInAnyOrder(new ZString[] { euIncoTermAndChargeFactory.FreightToEUBorderCode, euIncoTermAndChargeFactory.FreightAfterEUBorderCode }, bizObj.ChargeTypesToCalculate.ToArray());
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
			charge4.J7_Amount = 2000m;
			charge4.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
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
				AssertEquals("2 existing charge + 2 new charges", 3, invoice.Charges.Count);
				AssertEquals("Charge1 deleted", true, charge1.IsDeleted);
				AssertEquals("Charge2 deleted", true, charge2.IsDeleted);
				AssertEquals("Charge3 deleted", true, charge3.IsDeleted);
				AssertEquals("Charge4 deleted", true, charge4.IsDeleted);
				AssertEquals("Charge5 not deleted because code is irrelevant", false, charge5.IsDeleted);

				var chargeFreightToEUBorder = invoice.Charges.Cast<InvoiceCharge>().Single(x => x.J7_ChargeType == euIncoTermAndChargeFactory.FreightToEUBorderCode && x.J7_IsDutiable);
				AssertCharge("Freight to EU Border", chargeFreightToEUBorder, 700m, Core.Constants.CurrencyCodes.Dominica);

				var chargeFreightToDestinationCountry = invoice.Charges.Cast<InvoiceCharge>().Single(x => x.J7_ChargeType == euIncoTermAndChargeFactory.FreightAfterEUBorderCode && !x.J7_IsDutiable && x.J7_IsStatisticalValueApplicable);
				AssertCharge("Freight to Destination Country", chargeFreightToDestinationCountry, 200m, Core.Constants.CurrencyCodes.Dominica);

				var chargeFreightToFinalDestination = invoice.Charges.Cast<InvoiceCharge>().SingleOrDefault(x => x.J7_ChargeType == euIncoTermAndChargeFactory.FreightDomesticCode && !x.J7_IsDutiable && !x.J7_IsStatisticalValueApplicable);
				AssertNull("No Freight to Final Destination is created", chargeFreightToFinalDestination);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => EU.Business.Declaration.CalculateFreightNonAirBizObj.New(invoice.Charges, declaration);

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
