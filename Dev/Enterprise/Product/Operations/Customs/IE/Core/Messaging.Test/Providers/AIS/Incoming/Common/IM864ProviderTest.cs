using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM864;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM864ProviderTest : TestCase
	{
		IM864Provider provider;

		public void TestMovementReferenceNumber()
		{
			provider = new IM864Provider(new Im864 { ImportOperation = new MCciOperationType864 { Mrn = "12MRN345ABCDE678R9" } });
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestCaseId()
		{
			provider = new IM864Provider(new Im864 { ImportOperation = new MCciOperationType864 { CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ" } });
			AssertEquals("1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ", provider.CaseId);
		}

		public void TestInvalidationRequestCancellationReason()
		{
			provider = new IM864Provider(new Im864 { ImportOperation = new MCciOperationType864 { InvalidationRequestCancellationReason = "Invalidation Request Cancellation Reason" } });
			AssertEquals("Invalidation Request Cancellation Reason", provider.InvalidationRequestCancellationReason);
		}
	}
}
