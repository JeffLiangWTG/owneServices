using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM460;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM460ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestControlNotificationDate()
		{
			AssertEquals(new ZDateTime(2024, 02, 20, 23, 59, 00), provider.NotificationDate);
		}

		public void TestTimeLimitForControl()
		{
			AssertEquals(new ZDateTime(2024, 03, 07, 14, 37, 00), provider.TimeLimitForControl);
		}

		public void TestControlTypeCoded()
		{
			AssertEquals("Orange", provider.OverallControlTypeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM460Provider(new Im460
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345CDEFG678R9",
					ControlNotificationDate = "202402202359GMT",
					TimeLimitForControl = "202403071437GMT",
				},
				OverallControlType = new OverAllControlsType
				{
					ControlTypeCoded = "Orange",
				}
			});
		}
		IM460Provider provider;
	}
}

