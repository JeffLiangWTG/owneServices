using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM426;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	sealed class IM426ProviderTest : TestCaseWithFactory
	{
		public void TestLRN()
		{
			AssertEquals("LRN", "LRN", provider.LRN);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "MRN", provider.MRN);
		}

		public void TestCustomsRegistrationNumber()
		{
			AssertEquals("CustomsRegistrationNumber", "123", provider.CustomsRegistrationNumber);
		}

		public void TestDeclarationRegistrationDateAndTime()
		{
			AssertEquals("DeclarationRegistrationDateAndTime", new ZDateTime(2023, 8, 17, 14, 22, 4), provider.DeclarationRegistrationDateAndTime);
		}

		public void TestPresentationNotificationDueDate()
		{
			AssertEquals("PresentationNotificationDueDate", new ZDateTime(2023, 7, 11, 12, 23, 7), provider.PresentationNotificationDueDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM426Provider(new Im426
			{
				ImportOperation = new MCciOperationType42
				{
					Lrn = "LRN",
					Mrn = "MRN",
					CustomsRegistrationNumber = "123",
					DeclarationRegistrationDateAndTime = new DateTime(2023, 8, 17, 14, 22, 4),
					PresentationNotificationDueDate = new DateTime(2023, 7, 11, 12, 23, 7)
				}
			});
		}
		IM426Provider provider;
	}
}
