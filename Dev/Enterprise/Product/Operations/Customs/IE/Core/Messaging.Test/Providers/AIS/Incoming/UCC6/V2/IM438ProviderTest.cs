using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM438ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestRequestDate()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.RequestDate);
		}

		public void TestExpirationDate()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.ExpirationDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM438Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM438.Im438
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType01
				{
					Lrn = "LRN001",
					Mrn = "12MRN345ABCDE678R9",
				},
				ReminderDetails = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MReminderDetailsType
				{
					RequestDate = new DateTime(2023, 08, 10, 14, 30, 45),
					ExpirationDate = new DateTime(2023, 08, 11, 14, 30, 45),
				}
			});
		}
		IM438Provider provider;
	}
}
