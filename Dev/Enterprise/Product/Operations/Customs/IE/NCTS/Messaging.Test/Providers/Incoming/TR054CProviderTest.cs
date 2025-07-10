using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR054C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class TR054CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			var emptyProvider = new TR054CProvider(new Tr054C());
			CombineAssertions(() =>
			{
				AssertEquals("Reference Number", ZString.Empty, emptyProvider.ReferenceNumber);
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("AdviceRequested", ZBool.False, emptyProvider.AdviceRequested);
				AssertEquals("AdviceRequestDateAndTime", ZDateTime.Empty, emptyProvider.AdviceRequestDateAndTime);
			});
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Reference Number", "IE123456", provider.ReferenceNumber);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "21IEDU4EX144268149", provider.MRN);
		}

		public void TestAdviceRequested()
		{
			AssertEquals("AdviceRequested", ZBool.True, provider.AdviceRequested);
		}

		public void TestAdviceRequestDateAndTime()
		{
			AssertEquals("AdviceRequestDateAndTime", new ZDateTime(2023, 1, 1, 10, 15, 30), provider.AdviceRequestDateAndTime);
		}

		TR054CProvider provider;
		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR054CProvider(new Tr054C
			{
				TransitOperation = new TransitOperationType101
				{
					Mrn = "21IEDU4EX144268149",
					AdviceRequested = "1",
					AdviceRequestDateAndTime = new DateTime(2023, 1, 1, 10, 15, 30)
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "IE123456"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType19
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType17
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
			});
		}
	}
}
