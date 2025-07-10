using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(TS316Provider))]
	sealed class TS316ProviderTest : TestCaseWithFactory
	{
		public void TestLRN()
		{
			AssertEquals("LRN123", provider.LRN);
		}

		public void TestRejectionDate()
		{
			AssertEquals(new DateTime(2023, 9, 5), provider.RejectionDate);
		}

		public void TestRejectionMotivationText()
		{
			AssertEquals("reason", provider.RejectionMotivationText);
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
			provider = new TS316Provider(AISUCC5InterchangeProcessorTestHelper.CreateTS316Object("LRN123", new DateTime(2023, 9, 5, 12, 15, 0)));
		}
		TS316Provider provider;
	}
}
