using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM933ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("23IE999912345678", provider.MovementReferenceNumber);
		}

		public void TestAmendmentRejectionDate()
		{
			AssertEquals(new DateTime(2023, 09, 22, 14, 30, 45), provider.NotificationRejectionDate);
		}

		public void TestAmendmentRejectionMotivationText()
		{
			AssertEquals("Rejection Reason", provider.NotificationRejectionReason);
		}

		public void TestFunctionalErrors()
		{
			AISUCC6ProviderTestHelper.AssertFunctionalErrors(new[]
			{
				(SequenceNumber: "1", ErrorPointer: "ErrorPointer001", ErrorCode: "13", ErrorReason: "ER1", Remarks: "Functional Error Remarks 1", OriginalAttributeValue: "Original Attribute Value 1"),
				(SequenceNumber: "2", ErrorPointer: "ErrorPointer002", ErrorCode: "52", ErrorReason: "ER2", Remarks: "Functional Error Remarks 2", OriginalAttributeValue: "Original Attribute Value 2")
			}, provider.FunctionalErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM933Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM933.Im933
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType433()
				{
					Mrn = "23IE999912345678",
					RejectionDate = new DateTime(2023, 09, 22, 14, 30, 45),
					RejectionReason = "Rejection Reason"
				},
				FunctionalError = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
					{
						SequenceNumber = "1",
						ErrorPointer = "ErrorPointer001",
						ErrorCode = "13",
						ErrorReason = "ER1",
						Remarks = "Functional Error Remarks 1",
						OriginalAttributeValue = "Original Attribute Value 1",
					},
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
					{
						SequenceNumber = "2",
						ErrorPointer = "ErrorPointer002",
						ErrorCode = "52",
						ErrorReason = "ER2",
						Remarks = "Functional Error Remarks 2",
						OriginalAttributeValue = "Original Attribute Value 2",
					},
				}
			});
		}
		IM933Provider provider;
	}
}
