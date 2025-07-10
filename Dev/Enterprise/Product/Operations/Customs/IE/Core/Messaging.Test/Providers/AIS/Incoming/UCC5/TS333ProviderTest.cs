using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS333Provider))]
	sealed class TS333ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Reject Reason", provider.RejectionReason);
		}

		public void TestRejectionDate()
		{
			AssertEquals(new DateTime(2023, 9, 5, 12, 15, 0), provider.RejectionDate);
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
			provider = new TS333Provider(AISUCC5InterchangeProcessorTestHelper.CreateTS333Object("21IEDUB11A782454R2", new DateTime(2023, 9, 5, 12, 15, 0), "Reject Reason"));
		}
		TS333Provider provider;
	}
}
