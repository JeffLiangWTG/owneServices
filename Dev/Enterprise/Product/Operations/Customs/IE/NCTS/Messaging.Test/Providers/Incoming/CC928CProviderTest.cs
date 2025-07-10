using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC928C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC928CProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN928", provider.LocalReferenceNumber);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("RNALPHN8", provider.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC928CProvider(new Cc928CType
			{
				TransitOperation = new TransitOperationType26
				{
					Lrn = "LRN928",
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "RNALPHN8",
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType20
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
			});
		}
		CC928CProvider provider;
	}
}
