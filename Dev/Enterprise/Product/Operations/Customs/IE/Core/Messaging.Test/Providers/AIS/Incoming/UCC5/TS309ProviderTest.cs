using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS309Provider))]
	sealed class TS309ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestInvalidationDecision()
		{
			AssertEquals(true, provider.InvalidationDecision);
		}

		public void TestInvalidationInitiatedByCustoms()
		{
			AssertEquals(false, provider.InvalidationInitiatedByCustoms);
		}

		public void TestInvalidationJustification()
		{
			AssertEquals("justification", provider.InvalidationJustification);
		}

		public void TestDateOfInvalidationDecision()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.DateOfInvalidationDecision);
		}

		public void TestDateOfInvalidationRequest()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.DateOfInvalidationRequest);
		}

		public void TestDateOfInvalidation()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.DateOfInvalidation);
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
			base.SetUp();
			provider = new TS309Provider(AISUCC5InterchangeProcessorTestHelper.CreateTS309Object("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 15, 0)));
		}
		TS309Provider provider;
	}
}
