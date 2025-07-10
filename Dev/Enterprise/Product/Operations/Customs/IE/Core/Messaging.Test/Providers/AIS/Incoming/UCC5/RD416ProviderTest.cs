using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD416;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	[TestedType(typeof(RD416Provider))]
	sealed class RD416ProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new RD416Provider(null));
		}

		public void TestApplicationReferenceId()
		{
			AssertEquals("Abcd123aisdu3eh2u8", provider.ApplicationReferenceId);
		}

		public void TestRejectionDate()
		{
			AssertEquals(new DateTime(2023, 08, 02), provider.RejectionDate);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Test Reason", provider.RejectionReason);
		}

		public void TestApplicant()
		{
			AssertEquals("XY", provider.Applicant);
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
			provider = new RD416Provider(GenerateMessage());
		}

		RD416Provider provider;

		Rd416 GenerateMessage()
		{
			return new Rd416
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u8",
					RejectionDate = "20230802",
					RejectionReason = "Test Reason",
					Applicant = "XY",
				},
				FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			};
		}
	}
}
