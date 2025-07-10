using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC571C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC571CProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestReExportNotificationRegistrationDate()
		{
			AssertEquals(new ZDate(2022, 02, 07), provider.ReExportNotificationRegistrationDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC571CProvider(new Cc571C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType25
				{
					Lrn = "LRN",
					Mrn = "MRN",
					ReExportNotificationRegistrationDate = new DateTime(2022, 02, 07, 15, 09, 59)
				}
			});
		}
		CC571CProvider provider;
	}
}
