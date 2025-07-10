using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM433;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM433ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestNotificationRejectionDate()
		{
			var expectedDate = DateTimeProviderHelper.ConvertAisUcc5DateStringToZDateTime("20230101");
			AssertEquals(expectedDate, provider.NotificationRejectionDate);
		}

		public void TestNotificationRejectionReason()
		{
			AssertEquals("Invalid data", provider.NotificationRejectionReason);
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
			provider = new IM433Provider(new Im433
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345ABCDE678R9",
					RejectionDate = "20230101",
					RejectionReason = "Invalid data"
				},
				FunctionalError = AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeObjects(),
			});
		}

		IM433Provider provider;
	}
}
