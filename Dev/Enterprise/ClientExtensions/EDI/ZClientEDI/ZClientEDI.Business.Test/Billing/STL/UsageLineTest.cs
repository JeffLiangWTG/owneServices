using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(UsageLine))]
	internal class UsageLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetAmounts_Rounding()
		{
			var usageLine1 = new UsageLine(Factory);
			usageLine1.PriceCurrency = "AUD";
			usageLine1.SetAmounts(2.555m, 1.555m);

			var usageLine2 = new UsageLine(Factory);
			usageLine2.PriceCurrency = "TWD";
			usageLine2.SetAmounts(2.555m, 1.555m);

			AssertEquals(2.56m, usageLine1.UnadjustedPreDiscountAmount);
			AssertEquals(1.56m, usageLine1.UnadjustedPostDiscountAmount);

			AssertEquals(2.56m, usageLine2.UnadjustedPreDiscountAmount);
			AssertEquals(2m, usageLine2.UnadjustedPostDiscountAmount);
		}

		public void TestSetAmounts_Rounding_ThreeDecimal()
		{
			var priceItem = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			priceItem.L7_Category = "WTA";
			priceItem.L7_Code = "W00";
			priceItem.L7_Price = 0.123456789m;

			var list = new CodeDescriptionPairList();
			list.AddPair("WTA", ".");
			EDIDataRegistry.Instance.ProductsWithThreeDecimalBillingSummary.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var usageLine1 = new UsageLine(Factory, ZDateTime.UtcNow, ZDateTime.UtcNow, "", priceItem, null, null, null);
			usageLine1.FormatOptions = BillingConstants.FormatOptions.GetOptionsByProduct("WTA", Factory);
			usageLine1.PriceCurrency = "AUD";
			usageLine1.Price = 0.45679m;
			usageLine1.PriceItem = priceItem;
			usageLine1.SetAmounts(1.123456789m, 2.987654321m);

			AssertEquals(1.123m, usageLine1.PreDiscountAmount);
			AssertEquals(2.988m, usageLine1.PostDiscountAmount);

			AssertEquals(1.123m, usageLine1.UnadjustedPreDiscountAmount);
			AssertEquals(2.988m, usageLine1.UnadjustedPostDiscountAmount);

			AssertEquals("0.457", usageLine1.PriceText);
			AssertEquals("1.123", usageLine1.PreDiscountAmountText);
			AssertEquals("2.988", usageLine1.PostDiscountAmountText);

			usageLine1.AdjustAmounts(1.99m);
			AssertEquals(2.235m, usageLine1.PreDiscountAmount);
			AssertEquals(5.946m, usageLine1.PostDiscountAmount);
			AssertEquals("0.457", usageLine1.PriceText);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UsageLine(Factory);
		}

		#endregion
	}
}
