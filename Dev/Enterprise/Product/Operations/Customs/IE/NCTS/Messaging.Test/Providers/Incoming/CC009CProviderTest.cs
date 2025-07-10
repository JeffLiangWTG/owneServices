using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC009C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC009CProviderTest : TestCaseWithFactory
	{
		public void TestLrnAndMrn()
		{
			AssertEquals("LocalReferenceNumber", "LRN009", provider.LocalReferenceNumber);
			AssertEquals("MorementReferenceNumber", "19AA12345678901230", provider.MovementReferenceNumber);
		}

		public void TestCustomsOfficeOfDeparture()
		{
			AssertEquals("CustomsOfficeOfDeparture", "RNALPHN8", provider.CustomsOfficeOfDeparture);
		}

		public void TestInvalidationProperties()
		{
			CombineAssertions("InvalidationProperties", () =>
			{
				AssertEquals("InvalidationDecision", ZBool.True, provider.IsInvalidated);
				AssertEquals("InvalidationRequestDateAndTime", new ZDateTime(2023, 01, 30, 15, 30, 00), provider.InvalidationRequestDateAndTime);
				AssertEquals("InvalidationDecisionDateAndTime", new ZDateTime(2023, 01, 30, 16, 00, 01), provider.InvalidationDecisionDateAndTime);
			});
		}

		public void TestPropertiesWhenEmpty()
		{
			CombineAssertions("Should not throw exception when is empty", () =>
			{
				var emptyProvider = new CC009CProvider(new Cc009CType());
				AssertNoExceptionThrown(() => _ = emptyProvider.LocalReferenceNumber);
				AssertNoExceptionThrown(() => _ = emptyProvider.MovementReferenceNumber);
				AssertNoExceptionThrown(() => _ = emptyProvider.CustomsOfficeOfDeparture);
				AssertNoExceptionThrown(() => _ = emptyProvider.IsInvalidated);
				AssertNoExceptionThrown(() => _ = emptyProvider.InvalidationRequestDateAndTime);
				AssertNoExceptionThrown(() => _ = emptyProvider.InvalidationDecisionDateAndTime);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC009CProvider(CreateStandardProvider());
		}
		CC009CProvider provider;

		public static Cc009CType CreateStandardProvider(string lrn = "LRN009", string mrn = "19AA12345678901230", string customsOfficeOfDeparture = "RNALPHN8", Flag decision = Flag.Item1)
		{
			return new Cc009CType
			{
				TransitOperation = new TransitOperationType03
				{
					Lrn = lrn,
					Mrn = mrn,
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = customsOfficeOfDeparture,
				},
				Invalidation = new InvalidationType01
				{
					Decision = decision,
					RequestDateAndTime = new DateTime(2023, 1, 30, 15, 30, 0),
					DecisionDateAndTime = new DateTime(2023, 1, 30, 16, 0, 1),
					InitiatedByCustoms = Flag.Item1,
					Justification = "Reason for decision",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType13
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
			};
		}
	}
}
