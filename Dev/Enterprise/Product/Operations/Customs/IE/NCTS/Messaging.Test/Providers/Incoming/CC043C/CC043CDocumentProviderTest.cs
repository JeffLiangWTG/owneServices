using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CDocumentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("DocumentType missing", () => new CC043CDocumentProvider(document: null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals((ZShort)1, provider.SequenceNumber);
		}

		public void TestType()
		{
			AssertEquals("DGH", provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("5674", provider.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CDocumentProvider(new TransportDocumentType02
			{
				SequenceNumber = "1",
				Type = "DGH",
				ReferenceNumber = "5674"
			});
		}
		CC043CDocumentProvider provider;
	}
}
