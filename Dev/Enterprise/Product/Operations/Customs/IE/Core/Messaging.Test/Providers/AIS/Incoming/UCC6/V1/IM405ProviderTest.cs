using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM405;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM405ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestAmendmentRejectionDate()
		{
			AssertEquals(new ZDateTime(2024, 02, 29), provider.AmendmentRejectionDate);
		}

		public void TestAmendmentRejectionMotivationText()
		{
			AssertEquals("Amendment Rejection Motivation Text", provider.AmendmentRejectionMotivationText);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
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
			base.SetUp();
			provider = new IM405Provider(new Im405
			{
				Declaration = new DeclarationType
				{
					AmendmentRejectionDate = "20240229",
					AmendmentRejectionMotivationText = "Amendment Rejection Motivation Text",
					CustomsOffices = { },
					Mrn = "12MRN345CDEFG678R9",
					Parties = { },
					Remarks = "Remarks001"
				},
				FunctionalError = AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeObjects(),
			});
		}
		IM405Provider provider;
	}
}
