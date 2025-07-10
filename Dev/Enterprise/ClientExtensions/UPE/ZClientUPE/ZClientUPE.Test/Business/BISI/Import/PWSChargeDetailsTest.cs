using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class PWSChargeDetailsTest : TestCase
	{
		public void TestChargeDescription()
		{
			AssertEquals("FREIGHT", ChargeDetails.ChargeDescription);
		}

		public void TestTaxableAmount()
		{
			AssertEquals(45m, ChargeDetails.TaxableAmount);
		}

		public void TestNonTaxableAmount()
		{
			AssertEquals(238m, ChargeDetails.NonTaxableAmount);
		}

		public void TestDiscount()
		{
			AssertEquals(10.5m, ChargeDetails.Discount);
		}

		public void TestNettAmount()
		{
			AssertEquals(350.25m, ChargeDetails.NettAmount);
		}

		#region Implementation
		PWSChargeDetails ChargeDetails
		{
			get
			{
				if (fChargeDetails == null)
				{
					fChargeDetails = new PWSChargeDetails(TestChargeDataString);
				}

				return fChargeDetails;
			}
		}

		PWSChargeDetails fChargeDetails;
		const string TestChargeDataString = "FREIGHT                         45.00           238.00            10.50           350.25";
		#endregion
	}
}
