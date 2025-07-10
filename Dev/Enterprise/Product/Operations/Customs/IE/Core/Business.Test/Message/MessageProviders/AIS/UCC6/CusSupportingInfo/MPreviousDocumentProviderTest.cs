using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class MPreviousDocumentProviderTest : DataProviderTestCase<MPreviousDocumentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new MPreviousDocumentProvider(null));
		}

		public void TestProviderInterface()
		{
			Assert(Provider is IMPreviousDocument);
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

		public void TestDateOfAcceptance()
		{
			AssertEquals(ZDateTime.BrettsBirthday, Provider.DateOfAcceptance);
		}

		protected override MPreviousDocumentProvider GetProvider() => new MPreviousDocumentProvider(previousDocument);

		protected override void SetUp()
		{
			previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Code = "123";
			previousDocument.CSI_ReferenceNumber = "REFNO1";
			previousDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
		}
		PreviousDocument previousDocument;
	}
}
