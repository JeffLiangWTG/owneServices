using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class ArrivalDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<ArrivalDocumentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ArrivalDocumentProvider(null));
		}

		public void TestSequenceNumber_StatusNEW()
		{
			doc.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc.CSI_LineNo = 4;
			AssertEquals(4, Provider.SequenceNumber);
		}

		public void TestSequenceNumber_StatusNotNEW()
		{
			doc.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			doc.CSI_LineNo = 4;
			AssertEquals(4, Provider.SequenceNumber);
		}

		public void TestType_StatusNEW()
		{
			doc.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc.CSI_Code = "ABC";
			AssertEquals("ABC", Provider.Type);
		}

		public void TestType_StatusNotNEW()
		{
			doc.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			doc.CSI_Code = "ABC";
			AssertEquals(string.Empty, Provider.Type);
		}

		public void TestReferenceNumber_StatusNEW()
		{
			doc.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc.CSI_ReferenceNumber = "123";
			AssertEquals("123", Provider.ReferenceNumber);
		}

		public void TestReferenceNumber_StatusNotNEW()
		{
			doc.CSI_Status = NctsUnloadedStateList.Codes.MIS;
			doc.CSI_ReferenceNumber = "123";
			AssertEquals(string.Empty, Provider.ReferenceNumber);
		}

		protected override ArrivalDocumentProvider GetProvider() => new ArrivalDocumentProvider(doc);

		protected override void SetUp()
		{
			base.SetUp();
			doc = Factory.New<CusSupportingInfo>();
		}
		CusSupportingInfo doc;
	}
}
