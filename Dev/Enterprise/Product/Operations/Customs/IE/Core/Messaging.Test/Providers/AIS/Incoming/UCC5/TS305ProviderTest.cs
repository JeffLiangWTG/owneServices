using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS305Provider))]
	sealed class TS305ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestAmendmentRejectionDate()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.AmendmentRejectionDate);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Test Reason", provider.RejectionReason);
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
			provider = new TS305Provider(AISUCC5InterchangeProcessorTestHelper.CreateTS305Object("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 15, 0), "Test Reason"));
		}
		TS305Provider provider;
	}
}
