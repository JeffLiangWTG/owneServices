using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM416;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	class IM416ProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDeclarationType()
		{
			AssertEquals("B", provider.AdditionalDeclarationType);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN002", provider.LocalReferenceNumber);
		}

		public void TestRejectionDate()
		{
			AssertEquals(new ZDate(2024, 02, 21), provider.RejectionDate);
		}

		public void TestRejectionMotivationText()
		{
			AssertEquals("UCC5 Rejection Motivation Text", provider.RejectionMotivationText);
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
			provider = new IM416Provider(new Im416
			{
				Declaration = new DeclarationType()
				{
					AdditionalDeclarationType12 = "B",
					Lrn25 = "LRN002",
					RejectionDate = "20240221",
					RejectionMotivationText = "UCC5 Rejection Motivation Text",
				},
				FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			});
		}
		IM416Provider provider;
	}
}
