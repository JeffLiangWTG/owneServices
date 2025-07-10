using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CSealProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("SealType missing", () => new CC043CSealProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals((ZShort)1, provider.SequenceNumber);
		}

		public void TestIdentifier()
		{
			AssertEquals("3456", provider.Identifier);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CSealProvider(new SealType04
			{
				SequenceNumber = "1",
				Identifier = "3456"
			});
		}
		CC043CSealProvider provider;
	}
}
