using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM882;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM882ProviderTest : TestCase
	{
		IM882Provider provider;

		public void TestMovementReferenceNumber()
		{
			provider = new IM882Provider(new Im882 { ImportOperation = new MCciOperationType882 { Mrn = "12MRN345ABCDE678R9" } });
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestCaseId()
		{
			provider = new IM882Provider(new Im882 { ImportOperation = new MCciOperationType882 { CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ" } });
			AssertEquals("1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ", provider.CaseId);
		}

		public void TestDocumentsUploadRequestCancellationReason()
		{
			provider = new IM882Provider(new Im882 { ImportOperation = new MCciOperationType882 { DocumentsUploadRequestCancellationReason = "Documents Upload Request Cancellation Reason" } });
			AssertEquals("Documents Upload Request Cancellation Reason", provider.DocumentsUploadRequestCancellationReason);
		}
	}
}
