using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	sealed class TR060CRequestedDocumentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("RequestedDocumentType missing", () => new TR060CRequestedDocumentProvider(null));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, provider.SequenceNumber);
		}

		public void TestDocumentType()
		{
			AssertEquals("DocumentType", provider.DocumentType);
		}

		public void TestDescription()
		{
			AssertEquals("Description", provider.Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TR060CRequestedDocumentProvider(new CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes.RequestedDocumentType
			{
				SequenceNumber = "1",
				DocumentType = "DocumentType",
				Description = "Description"
			});
		}
		TR060CRequestedDocumentProvider provider;
	}
}
