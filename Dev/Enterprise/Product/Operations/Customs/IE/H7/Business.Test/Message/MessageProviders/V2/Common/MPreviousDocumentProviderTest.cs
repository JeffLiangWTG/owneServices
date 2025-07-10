using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class MPreviousDocumentProviderTest : DataProviderTestCase<MPreviousDocumentProvider>
	{
		public void TestDateOfAcceptance()
		{
			AssertEquals("Date Of Acceptance", DateTime.MinValue, mPreviousDocumentProvider.DateOfAcceptance);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", mPreviousDocumentProvider.CcQualifier);
		}

		public void TestType()
		{
			AssertEquals("Type", "Code", mPreviousDocumentProvider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Reference Number", "Description", mPreviousDocumentProvider.Reference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "Description";

			mPreviousDocumentProvider = new MPreviousDocumentProvider(supportingInfo);
		}
		CusSupportingInfo supportingInfo;
		MPreviousDocumentProvider mPreviousDocumentProvider;

		protected sealed override MPreviousDocumentProvider GetProvider()
		{
			return mPreviousDocumentProvider;
		}
	}
}
