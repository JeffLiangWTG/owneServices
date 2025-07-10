using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM405ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestAmendmentRejectionDate()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.AmendmentRejectionDate);
		}

		public void TestAmendmentRejectionMotivationText()
		{
			AssertEquals("Amendment Rejection Motivation Text", provider.AmendmentRejectionMotivationText);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
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
			provider = new IM405Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM405.Im405
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType405
				{
					Mrn = "12MRN345CDEFG678R9",
					AmendmentRejectionDate = new DateTime(2023, 08, 10, 14, 30, 45),
					AmendmentRejectionMotivationText = "Amendment Rejection Motivation Text",
					Remarks = "Remarks001",
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
		IM405Provider provider;
	}
}
