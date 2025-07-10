using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	sealed class IM460RequestedDocumentsProviderTest : TestCaseWithFactory
	{
		public void TestSequenceNumber()
		{
			AssertEquals("1", provider.SequenceNumber);
		}

		public void TestDocumentType()
		{
			AssertEquals("Y001", provider.DocumentType);
		}

		public void TestCcQualifier()
		{
			AssertEquals("AB", provider.CcQualifier);
		}

		public void TestRequestInformation()
		{
			AssertEquals("Please provide", provider.RequestInformation);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("12345", provider.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM460RequestedDocumentsProvider(new MRequestedDocumentsType02
			{
				SequenceNumber = "1",
				Type = "Y001",
				CcQualifier = "AB",
				Description = "Please provide",
				ReferenceNumber = "12345"
			});
		}
		IM460RequestedDocumentsProvider provider;
	}
}
