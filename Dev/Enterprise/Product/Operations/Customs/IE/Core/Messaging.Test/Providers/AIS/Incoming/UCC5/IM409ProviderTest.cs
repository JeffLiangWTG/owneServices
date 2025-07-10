using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM409;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	sealed class IM409ProviderTest : TestCaseWithFactory
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
			AISProviderTestHelper.AssertFunctionalErrors(new[]
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
					DateOfInvalidation = "2024-01-07",
					DateOfInvalidationDecision = "2024-02-08",
					DateOfInvalidationRequest = "2024-03-09",
					InvalidationDecision = true,
					InvalidationInitiatedByCustoms = true,
					InvalidationJustification = "Invalidation Justification",
					Mrn = "12MRN345CDEFG678R9",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IEDUB100"
					}
				},
				FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			});
		}
		IM409Provider provider;
	}
}
