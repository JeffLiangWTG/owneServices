using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM431ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestLodgementOfSupplementaryDeclarationStartDate()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), provider.LodgementOfSupplementaryDeclarationStartDate);
		}

		public void TestLodgementOfSupplementaryDeclarationExpiryDate()
		{
			AssertEquals(new DateTime(2023, 08, 11, 14, 30, 45), provider.LodgementOfSupplementaryDeclarationExpiryDate);
		}

		public void TestTimerExpiryInformation()
		{
			AssertEquals("Timer Expiry Information", provider.TimerExpiryInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM431Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM431.Im431
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType01
				{
					Lrn = "LRN001",
					Mrn = "12MRN345CDEFG678R9",
				},
				TimerExpiryForSupplementaryDeclaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MTimerExpiryType
				{
					LodgementOfSupplementaryDeclarationStartDate = new DateTime(2023, 08, 10, 14, 30, 45),
					LodgementOfSupplementaryDeclarationExpiryDate = new DateTime(2023, 08, 11, 14, 30, 45),
					TimerExpiryInformation = "Timer Expiry Information",
				},
			});
		}
		IM431Provider provider;
	}
}
