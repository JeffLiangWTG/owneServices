using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationDiscountChargesProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationDiscountChargesProvider()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.Currency, "BTH", "Currency Codes Mapping", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.Currency, "USD", "220", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			var invLine = Factory.New<JobComInvoiceLine>();

			var charge1 = invLine.Charges.AddNew();
			charge1.J7_ChargeType = ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry;
			charge1.J7_RX_NKCurrency = "USD";
			charge1.J7_Amount = 50m;
			charge1.J7_ExchangeRate = 2m;

			var charge2 = invLine.Charges.AddNew();
			charge2.J7_ChargeType = ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry;
			charge2.J7_RX_NKCurrency = "USD";
			charge2.J7_Amount = 300m;
			charge2.J7_ExchangeRate = 2m;

			List<JobComInvCharge> charges = new List<JobComInvCharge>();
			charges.Add(charge1);
			charges.Add(charge2);

			var discountCharge = DeclarationDiscountChargeProvider.New(charges);

			AssertEquals("ChargeCode", "01", discountCharge.ChargeCode);
			AssertEquals("CurrencyCode", "220", discountCharge.CurrencyCode);
			AssertEquals("Amount", 350m, discountCharge.Amount);
			AssertEquals("AmountInLocalCurrency", 251.80m, discountCharge.AmountInLocalCurrency);
		}
	}
}

