using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM457;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	sealed class IM457ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("Local Reference Number", "LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("Movement Reference Number", "12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestPresentationNotificationRegistrationDateAndTime()
		{
			AssertEquals("Normal Date Time", new ZDateTime(2023, 9, 19, 16, 50, 24), provider.PresentationNotificationRegistrationDateAndTime);

			provider = new IM457Provider(new Im457 { ImportOperation = new MCciOperationType30 { PresentationNotificationRegistrationDateAndTime = DateTime.MinValue } });
			AssertEquals("DateTime.MinValue", ZDateTime.Empty, provider.PresentationNotificationRegistrationDateAndTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM457Provider(new Im457
			{
				ImportOperation = new MCciOperationType30
				{
					Lrn = "LRN001",
					Mrn = "12MRN345CDEFG678R9",
					PresentationNotificationRegistrationDateAndTime = new DateTime(2023, 9, 19, 16, 50, 24)
				}
			});
		}
		IM457Provider provider;
	}
}
