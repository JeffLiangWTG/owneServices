using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037CValidityLimitationProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ValidityLimitationType missing", () => new CC037CValidityLimitationProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", 1, provider.SequenceNumber);
		}

		public void TestGuaranteeNotValidIn()
		{
			AssertEquals("GuaranteeNotValidIn", "A", provider.GuaranteeNotValidIn);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037CValidityLimitationProvider(new ValidityLimitationType
			{
				SequenceNumber = "1",
				GuaranteeNotValidIn = "A"
			});
		}
		CC037CValidityLimitationProvider provider;
	}
}
