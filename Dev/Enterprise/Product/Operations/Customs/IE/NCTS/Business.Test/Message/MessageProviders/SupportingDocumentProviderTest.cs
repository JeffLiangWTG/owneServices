using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class SupportingDocumentProviderTest : DataProviderTestCase<SupportingDocumentProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("SupportingDocument missing", () => new SupportingDocumentProvider(null));
			});
		}

		public void TestType()
		{
			AssertEquals(string.Empty, Provider.Type);
			supportingDocument.CSI_Code = "123";
			AssertEquals("123", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals(string.Empty, Provider.Reference);
			supportingDocument.CSI_ReferenceNumber = "REFERENCE";
			AssertEquals("REFERENCE", Provider.Reference);
		}

		public void TestComplementOfInformation()
		{
			AssertEquals(string.Empty, Provider.ComplementOfInformation);
			supportingDocument.CSI_ReferenceNumber2 = "REFERENCE 2";
			AssertEquals("REFERENCE 2", Provider.ComplementOfInformation);
		}

		public void TestLineItemNumber()
		{
			AssertEquals(0, Provider.LineItemNumber);
			supportingDocument.CSI_ItemNumber = 1;
			AssertEquals(1, Provider.LineItemNumber);
		}

		protected override SupportingDocumentProvider GetProvider() => new SupportingDocumentProvider(supportingDocument);

		protected override void SetUp()
		{
			base.SetUp();
			supportingDocument = Factory.New<SupportingDocument>();
		}
		SupportingDocument supportingDocument;
	}
}
