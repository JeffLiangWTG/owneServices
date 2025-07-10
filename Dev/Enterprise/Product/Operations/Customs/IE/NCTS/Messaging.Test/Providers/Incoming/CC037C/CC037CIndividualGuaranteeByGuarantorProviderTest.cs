using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037CIndividualGuaranteeByGuarantorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("IndividualGuaranteeByGuarantorType missing", () => new CC037CIndividualGuaranteeByGuarantorProvider(null));
			});
		}

		public void TestGuaranteeAmount()
		{
			AssertEquals("GuaranteeAmount", 8M, provider.GuaranteeAmount);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", "EUR", provider.Currency);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CIndividualGuaranteeByGuarantorProvider(new IndividualGuaranteeByGuarantorType
			{
				GuaranteeAmount = 8,
				Currency = "EUR",
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "FD123TYU",
				},
				CustomsOfficeOfDestination = new CustomsOfficeOfDestinationType02
				{
					ReferenceNumber = "UJ321YHN",
				}
			});
		}
		CC037CIndividualGuaranteeByGuarantorProvider provider;
	}
}
