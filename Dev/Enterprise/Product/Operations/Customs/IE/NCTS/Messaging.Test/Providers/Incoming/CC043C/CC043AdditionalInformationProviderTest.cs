using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043AdditionalInformationProvider))]
	sealed class CC043AdditionalInformationProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("additionalInformation missing", () => new CC043AdditionalInformationProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("Sequence Number", "1", provider.SequenceNumber);
		}
		public void TestCode()
		{
			AssertEquals("Code", "TEST123", provider.Code);
		}
		public void TestText()
		{
			AssertEquals("Text", "Sample Text", provider.Text);
		}

		protected override void SetUp()
		{
			provider = new CC043AdditionalInformationProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes.AdditionalInformationType02()
			{
				Code = "TEST123",
				SequenceNumber = "1",
				Text = "Sample Text"
			});
		}
		CC043AdditionalInformationProvider provider;
	}
}
