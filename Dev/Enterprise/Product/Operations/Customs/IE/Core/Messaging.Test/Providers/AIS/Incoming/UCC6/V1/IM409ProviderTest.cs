using System;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM409;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
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
			AssertEquals(new DateTime(2024, 02, 8, 00, 00, 00), provider.DateOfInvalidationDecision);
		}

		public void TestDateOfInvalidationRequest()
		{
			AssertEquals(new DateTime(2024, 03, 9, 00, 00, 00), provider.DateOfInvalidationRequest);
		}

		public void TestDateOfInvalidation()
		{
			AssertEquals(new ZDateTime(2024, 01, 7, 00, 00, 00), provider.DateOfInvalidation);
		}

		public void TestFunctionalErrors()
		{
			AISUCC6V1ProviderTestHelper.AssertFunctionalErrors(new[]
			{
				(ErrorPointer: "ErrorPointer001", ErrorType: "13", ErrorReason: "ER1", ErrorMessage: "Functional Error Message 1", OriginalAttributeValue: "Original Attribute Value 1"),
				(ErrorPointer: "ErrorPointer002", ErrorType: "40", ErrorReason: "ER2", ErrorMessage: "Functional Error Message 2", OriginalAttributeValue: "Original Attribute Value 2")
			}, provider.FunctionalErrors);
		}

		protected override void SetUp()
		{
			provider = new IM409Provider(new Im409()
			{
				Declaration = new DeclarationType()
				{
					DateOfInvalidation = "20240107",
					DateOfInvalidationDecision = "20240208",
					DateOfInvalidationRequest = "20240309",
					InvalidationDecision = true,
					InvalidationInitiatedByCustoms = true,
					InvalidationJustification = "Invalidation Justification",
					Mrn = "12MRN345CDEFG678R9",
					CustomsOffices = new CustomsOfficesType()
					{
						CustomsOfficeLodgement = "IEDUB100"
					}
				},
				FunctionalError = AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeObjects(),
			});
		}
		IM409Provider provider;
	}
}
