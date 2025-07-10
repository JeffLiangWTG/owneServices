using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class PreviousDocumentProviderTest : DataProviderTestCase<PreviousDocumentProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("PreviousDocument missing", () => new PreviousDocumentProvider(null));
			});
		}

		public void TestType()
		{
			AssertEquals(string.Empty, Provider.Type);
			previousDocument.CSI_Code = "123";
			AssertEquals("123", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals(string.Empty, Provider.Reference);
			previousDocument.CSI_ReferenceNumber = "REFERENCE";
			AssertEquals("REFERENCE", Provider.Reference);
		}

		public void TestComplementOfInformation()
		{
			AssertEquals(string.Empty, Provider.ComplementOfInformation);
			previousDocument.CSI_ReferenceNumber2 = "REFERENCE 2";
			AssertEquals("REFERENCE 2", Provider.ComplementOfInformation);
		}

		protected override PreviousDocumentProvider GetProvider() => new PreviousDocumentProvider(previousDocument);

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = Factory.New<PreviousDocument>();
		}
		PreviousDocument previousDocument;
	}
}
