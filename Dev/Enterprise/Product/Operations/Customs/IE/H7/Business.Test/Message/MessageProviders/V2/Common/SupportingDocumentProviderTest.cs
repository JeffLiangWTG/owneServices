using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class SupportingDocumentProviderTest : DataProviderTestCase<SupportingDocumentProvider>
	{
		public void TestType()
		{
			AssertEquals("Type", "Code", supportingDocumentProvider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Reference Number", "Description", supportingDocumentProvider.Reference);
		}

		public void TestDocumentLineItemNumber()
		{
			AssertNull("Document Line ItemNumber", supportingDocumentProvider.DocumentLineItemNumber);
		}

		public void TestIssuingAuthorityName()
		{
			AssertNull("Issuing Authority Name", supportingDocumentProvider.IssuingAuthorityName);
		}

		public void TestDateOfValidity()
		{
			AssertEquals("Date Of Validity", DateTime.MinValue, supportingDocumentProvider.DateOfValidity);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", supportingDocumentProvider.CcQualifier);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "Description";

			supportingDocumentProvider = new SupportingDocumentProvider(supportingInfo);
		}
		CusSupportingInfo supportingInfo;
		SupportingDocumentProvider supportingDocumentProvider;

		protected sealed override SupportingDocumentProvider GetProvider()
		{
			return supportingDocumentProvider;
		}
	}
}
