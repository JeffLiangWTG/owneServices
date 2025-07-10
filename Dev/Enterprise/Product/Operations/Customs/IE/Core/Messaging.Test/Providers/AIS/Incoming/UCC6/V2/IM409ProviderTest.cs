using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM409ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestInvalidationDecision()
		{
			AssertEquals(true, provider.InvalidationDecision);
		}

		public void TestInvalidationInitiatedByCustoms()
		{
			AssertEquals(true, provider.InvalidationInitiatedByCustoms);
		}

		public void TestInvalidationJustification()
		{
			AssertEquals("Invalidation Justification", provider.InvalidationJustification);
		}

		public void TestDateOfInvalidationDecision()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.DateOfInvalidationDecision);
		}

		public void TestDateOfInvalidationRequest()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.DateOfInvalidationRequest);
		}

		public void TestDateOfInvalidation()
		{
			AssertEquals(new DateTime(2023, 08, 09, 14, 30, 45), provider.DateOfInvalidation);
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
			provider = new IM409Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM409.Im409
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType409
				{
					Mrn = "12MRN345CDEFG678R9",
					InvalidationDecision = true,
					InvalidationInitiatedByCustoms = true,
					InvalidationJustification = "Invalidation Justification",
					DateOfInvalidationDecision = new DateTime(2023, 08, 11, 14, 30, 45),
					DateOfInvalidationRequest = new DateTime(2023, 08, 10, 14, 30, 45),
					DateOfInvalidation = new DateTime(2023, 08, 09, 14, 30, 45),
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
		IM409Provider provider;
	}
}
