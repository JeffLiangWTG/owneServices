using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC060RequestedDocumentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("RequestedDocumentType missing", () => new CC060RequestedDocumentProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", 1, provider.SequenceNumber);
		}

		public void TestDocumentType()
		{
			AssertEquals("Type", "ABC", provider.DocumentType);
		}

		public void TestDescription()
		{
			AssertEquals("Text", "Some text about the document", provider.Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC060RequestedDocumentProvider(new RequestedDocumentType
			{
				SequenceNumber = "1",
				DocumentType = "ABC",
				Description = "Some text about the document"
			});
		}
		CC060RequestedDocumentProvider provider;
	}
}
