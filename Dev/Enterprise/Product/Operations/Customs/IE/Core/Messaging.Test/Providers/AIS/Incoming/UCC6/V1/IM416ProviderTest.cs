using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM416ProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", providerUCC6.AdditionalDeclarationType);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", providerUCC6.LocalReferenceNumber);
		}

		public void TestRejectionDate()
		{
			AssertEquals(new DateTime(2023, 08, 10), providerUCC6.RejectionDate);
		}

		public void TestRejectionMotivationText()
		{
			AssertEquals("UCC6 Rejection Motivation Text", providerUCC6.RejectionMotivationText);
		}

		public void TestHasFunctionalErrors()
		{
			Assert("ProviderUCC6 has Functional Errors", providerUCC6.HasFunctionalErrors);
		}

		public void TestEntryStatus()
		{
			AssertEquals("REJ", providerUCC6.EntryStatus);
		}

		public void TestFunctionalErrors()
		{
			AISUCC6V1ProviderTestHelper.AssertFunctionalErrors(new[]
			{
				(ErrorPointer: "ErrorPointer001", ErrorType: "13", ErrorReason: "ER1", ErrorMessage: "Functional Error Message 1", OriginalAttributeValue: "Original Attribute Value 1"),
				(ErrorPointer: "ErrorPointer002", ErrorType: "40", ErrorReason: "ER2", ErrorMessage: "Functional Error Message 2", OriginalAttributeValue: "Original Attribute Value 2")
			}, providerUCC6.FunctionalErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			providerUCC6 = new IM416Provider(new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM416.Im416
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM416.DeclarationType
				{
					Additionaldeclarationtype = "A",
					Lrn = "LRN001",
					RejectionDate = "20230810",
					RejectionMotivationText = "UCC6 Rejection Motivation Text",
				},
				FunctionalError = AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeObjects(),
			});
		}
		IM416Provider providerUCC6;
	}
}
