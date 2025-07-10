using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC051C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC051CProviderTest : TestCaseWithFactory
	{
		public void TestTransitOperation()
		{
			CombineAssertions("TransitOperation", () =>
			{
				AssertEquals("MorementReferenceNumber", "19AA12345678901230", provider.MovementReferenceNumber);
				AssertEquals("NoReleaseMotivationCode", "CA", provider.NoReleaseMotivationCode);
				AssertEquals("NoReleaseMotivationText", "No Release", provider.NoReleaseMotivationText);
				AssertEquals("DeclarationSubmissionDateAndTime", new ZDateTime(2023, 2, 17), provider.DeclarationSubmissionDateAndTime);
			});
		}

		public void TestCustomsOfficeOfDeparture()
		{
			AssertEquals("CustomsOfficeOfDeparture", "RNALPHN8", provider.CustomsOfficeOfDeparture);
		}

		public void TestPropertiesWhenEmpty()
		{
			CombineAssertions("Should not throw exception when is empty", () =>
			{
				var emptyProvider = new CC051CProvider(new Cc051CType());
				AssertNoExceptionThrown(() => _ = emptyProvider.MovementReferenceNumber);
				AssertNoExceptionThrown(() => _ = emptyProvider.NoReleaseMotivationCode);
				AssertNoExceptionThrown(() => _ = emptyProvider.NoReleaseMotivationText);
				AssertNoExceptionThrown(() => _ = emptyProvider.CustomsOfficeOfDeparture);
				AssertNoExceptionThrown(() => _ = emptyProvider.DeclarationSubmissionDateAndTime);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC051CProvider(CreateStandardProvider());
		}
		CC051CProvider provider;

		public static Cc051CType CreateStandardProvider(string mrn = "19AA12345678901230", string customsOfficeOfDeparture = "RNALPHN8")
		{
			return new Cc051CType
			{
				TransitOperation = new TransitOperationType18
				{
					Mrn = mrn,
					NoReleaseMotivationCode = "CA",
					NoReleaseMotivationText = "No Release",
					DeclarationSubmissionDateAndTime = new DateTime(2023, 2, 17)
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = customsOfficeOfDeparture
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType15
				{
					IdentificationNumber = "IN928",
					TirHolderIdentificationNumber = "TIRHIN928",
					Name = "BOB THE BUILDER",
					Address = new AddressType15
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
