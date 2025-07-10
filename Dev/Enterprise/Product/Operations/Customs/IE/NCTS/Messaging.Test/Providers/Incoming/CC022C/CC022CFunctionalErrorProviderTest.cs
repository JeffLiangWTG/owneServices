using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC022CFunctionalErrorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("FunctionalErrorType missing", () => new CC022CFunctionalErrorProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestErrorPointer()
		{
			AssertEquals("EP01", provider.ErrorPointer);
		}

		public void TestErrorCode()
		{
			AssertEquals(AesNctsP5FunctionalErrorCodes.Item26.GetXmlEnumAttributeValue(), provider.ErrorCode);
		}

		public void TestErrorReason()
		{
			AssertEquals("Some reason", provider.ErrorReason);
		}

		public void TestOriginalAttributeValue()
		{
			AssertEquals("Original value 1", provider.OriginalAttributeValue);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC022CFunctionalErrorProvider(new FunctionalErrorType01
			{
				SequenceNumber = "1",
				ErrorPointer = "EP01",
				ErrorCode = AesNctsP5FunctionalErrorCodes.Item26,
				ErrorReason = "Some reason",
				OriginalAttributeValue = "Original value 1"
			});
		}
		CC022CFunctionalErrorProvider provider;
	}
}
