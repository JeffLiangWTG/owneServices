using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class GuaranteeProviderTest : DataProviderTestCase<GuaranteeProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeProvider(null, ZInt.Zero));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestGuaranteeType()
		{
			AssertEquals("B", provider.GuaranteeType);
		}

		public void TestOtherGuaranteeReference()
		{
			AssertEquals("Other", provider.OtherGuaranteeReference);
		}

		public void TestGuaranteeReferences()
		{
			AssertNotNull(provider.GuaranteeReferences);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var guarantee = header.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = "B";
			guarantee.PW_BondNumber = "BondNr";
			guarantee.PW_BondNumber2 = "Other";

			provider = new GuaranteeProvider(guarantee, 1);
		}

		GuaranteeProvider provider;

		protected override GuaranteeProvider GetProvider() => provider;
	}
}
