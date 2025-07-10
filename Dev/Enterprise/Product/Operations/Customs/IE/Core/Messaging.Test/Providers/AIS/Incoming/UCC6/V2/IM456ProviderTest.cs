using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM456ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("12CRN345ABCDE678R9", provider.CustomsRegistrationNumber);
		}

		public void TestBusinessRejectionType()
		{
			AssertEquals("BRT", provider.BusinessRejectionType);
		}

		public void TestRejectionDateAndTime()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.RejectionDateAndTime);
		}

		public void TestRejectionCode()
		{
			AssertEquals("1", provider.RejectionCode);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Rejection Reason", provider.RejectionReason);
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
			provider = new IM456Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM456.Im456
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType44
				{
					Lrn = "LRN001",
					Mrn = "12MRN345ABCDE678R9",
					CustomsRegistrationNumber = "12CRN345ABCDE678R9",
					BusinessRejectionType = "BRT",
					RejectionDateAndTime = new DateTime(2023, 08, 11, 14, 30, 45),
					RejectionCode = "1",
					RejectionReason = "Rejection Reason",
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
		IM456Provider provider;
	}
}
