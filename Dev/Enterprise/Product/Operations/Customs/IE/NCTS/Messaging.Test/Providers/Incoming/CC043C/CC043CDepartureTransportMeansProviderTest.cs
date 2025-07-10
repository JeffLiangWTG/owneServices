using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CDepartureTransportMeansProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("DepartureTransportMeansType missing", () => new CC043CDepartureTransportMeansProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals((ZShort)3, provider.SequenceNumber);
		}

		public void TestTypeOfIdentification()
		{
			AssertEquals("Type", provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("7890", provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals("IE", provider.Nationality);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CDepartureTransportMeansProvider(new DepartureTransportMeansType02
			{
				SequenceNumber = "3",
				TypeOfIdentification = "Type",
				IdentificationNumber = "7890",
				Nationality = "IE"
			});
		}
		CC043CDepartureTransportMeansProvider provider;
	}
}
