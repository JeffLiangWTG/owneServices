using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationExtraChargesProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationChargesProvider()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "BTH", "Currency Codes Mapping", true);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Currency, "USD", "220", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			var invLine = Factory.New<JobComInvoiceLine>();

			var chargeEngineering = invLine.Charges.AddNew();
			chargeEngineering.J7_ChargeType = ImportCustomsChargeTypeList.Codes.EngineeringProjects;
			chargeEngineering.J7_RX_NKCurrency = "USD";
			chargeEngineering.J7_Amount = 200m;
			chargeEngineering.J7_ExchangeRate = 2m;

			var chargeCommission = invLine.Charges.AddNew();
			chargeCommission.J7_ChargeType = ImportCustomsChargeTypeList.Codes.CommissionsBrokerage;
			chargeCommission.J7_RX_NKCurrency = "USD";
			chargeCommission.J7_Amount = 300m;
			chargeCommission.J7_ExchangeRate = 2m;

			List<JobComInvCharge> charges = new List<JobComInvCharge>();
			charges.Add(chargeEngineering);
			charges.Add(chargeCommission);

			var declarationCharges = DeclarationExtraChargeProvider.New(charges);

			AssertEquals("ChargeCode", "07", declarationCharges.ChargeCode);
			AssertEquals("CurrencyCode", "220", declarationCharges.CurrencyCode);
			AssertEquals("Amount", 500m, declarationCharges.Amount);
			AssertEquals("AmountInLocalCurrency", 359.71m, declarationCharges.AmountInLocalCurrency);
		}
	}
}
