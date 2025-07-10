using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC140C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC140CProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("MRN140", provider.MovementReferenceNumber);
		}

		public void TestRequestOnNonArrivedMovementDate()
		{
			AssertEquals(new DateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Unspecified), provider.RequestOnNonArrivedMovementDate);
		}

		public void TestLimitForResponseDate()
		{
			AssertEquals(new DateTime(1971, 9, 18, 0, 0, 0, DateTimeKind.Unspecified).AddDays(1), provider.LimitForResponseDate);
		}

		public void TestCustomsOfficeOfDeparture()
		{
			AssertEquals("IE987", provider.CustomsOfficeOfDeparture);
		}

		public void TestCustomsOfficeOfEnquiry()
		{
			AssertEquals("IE654", provider.CustomsOfficeOfEnquiry);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC140CProvider(new Cc140CType
			{
				TransitOperation = new TransitOperationType23
				{
					Mrn = "MRN140",
					RequestOnNonArrivedMovementDate = ZDateTime.BrettsBirthday.ToDateTime(),
					LimitForResponseDate = ZDateTime.BrettsBirthday.AddDays(1).ToDateTime(),
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "IE987",
				},
				CustomsOfficeOfEnquiryAtDeparture = new CustomsOfficeOfEnquiryAtDepartureType01
				{
					ReferenceNumber = "IE654",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
				{
					IdentificationNumber = "IN140",
					TirHolderIdentificationNumber = "TIRHIN140",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "IE",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					}
				},
			});
		}
		CC140CProvider provider;
	}
}
