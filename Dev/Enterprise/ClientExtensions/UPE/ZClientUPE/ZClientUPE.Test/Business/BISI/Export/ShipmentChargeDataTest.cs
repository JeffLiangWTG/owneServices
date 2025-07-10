using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentChargeDataTest : TestCase
	{
		public void TestConstructor()
		{
			ShipmentChargeData charge = new ShipmentChargeData(ShipmentChargeTypeCode.Security, 56.2m, Core.Constants.CurrencyCodes.Australia);
			AssertEquals("Incorrect gross amount", 56.2m, charge.GrossAmount);
			AssertEquals("Incurrect charge type code", "348", charge.TypeCode);
			AssertEquals("Incorrect charge type code enum", ShipmentChargeTypeCode.Security, charge.TypeCodeEnum);
			charge = new ShipmentChargeData(ShipmentChargeTypeCode.Duty, 99.98m, Core.Constants.CurrencyCodes.Australia);
			AssertEquals("Incorrect gross amount", 99.98m, charge.GrossAmount);
			AssertEquals("Incurrect charge type code", "201", charge.TypeCode);
			AssertEquals("Incorrect charge type code enum", ShipmentChargeTypeCode.Duty, charge.TypeCodeEnum);
		}

		public void TestAddAmount()
		{
			AssertEquals("Pre-conditikon", 10m, Charge.GrossAmount);
			Charge.AddAmount(20m);
			AssertEquals("Incorrect gross amount", 30m, Charge.GrossAmount);
			Charge.AddAmount(40m);
			AssertEquals("Incorrect gross amount", 70m, Charge.GrossAmount);
		}

		public void TestPercentageRate()
		{
			AssertEquals("Incorrect default", 0m, Charge.PercentageRate);
		}

		public void TestCurrencyCode()
		{
			AssertEquals("Incorrect currency code", Core.Constants.CurrencyCodes.Australia, Charge.CurrencyCode);
		}

		#region Implementation
		ShipmentChargeData Charge
		{
			get
			{
				if (fCharge == null)
				{
					fCharge = new ShipmentChargeData(ShipmentChargeTypeCode.Tradegate, 10m, Core.Constants.CurrencyCodes.Australia);
				}

				return fCharge;
			}
		}

		ShipmentChargeData fCharge;
		#endregion
	}
}
