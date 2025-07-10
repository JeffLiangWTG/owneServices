using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC025CHouseConsignmentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("HouseConsignmentType missing", () => new CC025CHouseConsignmentProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestReleaseType()
		{
			AssertEquals("2", provider.ReleaseType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC025CHouseConsignmentProvider(new HouseConsignmentType02
			{
				SequenceNumber = "1",
				ReleaseType = "2"
			});
		}
		CC025CHouseConsignmentProvider provider;
	}
}
