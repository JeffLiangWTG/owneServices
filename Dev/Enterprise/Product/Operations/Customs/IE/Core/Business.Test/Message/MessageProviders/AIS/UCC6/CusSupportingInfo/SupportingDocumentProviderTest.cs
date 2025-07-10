using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class SupportingDocumentProviderTest : DataProviderTestCase<SupportingDocumentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SupportingDocumentProvider(null));
		}

		public void TestProviderInterface()
		{
			Assert(Provider is ISupportingDocument);
		}

		public void TestType()
		{
			AssertEquals("123", Provider.Type);
		}

		public void TestCcQualifier()
		{
			// node should not be populated
			AssertEquals(null, Provider.CcQualifier);
		}

		public void TestReference()
		{
			AssertEquals("REFNO1", Provider.Reference);
		}

		public void TestDocumentLineItemNumber()
		{
			AssertEquals("1", Provider.DocumentLineItemNumber);
		}

		public void TestIssuingAuthorityName()
		{
			AssertEquals("AUTH01", Provider.IssuingAuthorityName);
		}

		public void TestDateOfValidity()
		{
			AssertEquals(ZDateTime.BrettsBirthday, Provider.DateOfValidity);
		}

		protected override SupportingDocumentProvider GetProvider() => new SupportingDocumentProvider(supportingDocument);

		protected override void SetUp()
		{
			supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_Code = "123";
			supportingDocument.CSI_ReferenceNumber = "REFNO1";
			supportingDocument.CSI_ItemNumber = 1;
			supportingDocument.CSI_AdditionalDescription = "AUTH01";
			supportingDocument.CSI_DateOfExpiry = ZDateTime.BrettsBirthday;
		}
		SupportingDocument supportingDocument;
	}
}
