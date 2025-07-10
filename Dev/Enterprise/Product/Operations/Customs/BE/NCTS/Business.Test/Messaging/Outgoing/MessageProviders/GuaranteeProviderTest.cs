using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeProvider))]
	class GuaranteeProviderTest : Customs.Business.Testing.DataProviderTestCase<GuaranteeProvider>
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
			guarantee.PW_BondType = "B";
			AssertEquals("B", provider.GuaranteeType);
		}

		public void TestOtherGuaranteeReference()
		{
			guarantee.PW_BondNumber2 = "Other";
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
			guarantee = header.MovementHeader.Guarantees.AddNew();

			provider = new GuaranteeProvider(guarantee, 1);
		}

		GuaranteeProvider provider;
		NctsGuarantee guarantee;

		protected override GuaranteeProvider GetProvider() => provider;
	}
}
