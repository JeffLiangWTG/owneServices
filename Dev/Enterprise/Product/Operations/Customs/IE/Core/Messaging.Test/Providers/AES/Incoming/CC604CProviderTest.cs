using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC604C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	sealed class CC604CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestAmendmentSubmissionDateAndTime()
		{
			AssertEquals(new ZDateTime(2022, 10, 07, 15, 09, 59), provider.AmendmentSubmissionDateTime);
		}

		public void TestAmendmentAcceptanceDateAndTime()
		{
			AssertEquals(new ZDateTime(2022, 10, 10, 15, 09, 59), provider.AmendmentAcceptanceDateTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC604CProvider(new Cc604C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType56
				{
					Mrn = "MRN",
					AmendmentDateAndTime = new DateTime(2022, 10, 07, 15, 09, 59),
					AmendmentAcceptanceDateAndTime = new DateTime(2022, 10, 10, 15, 09, 59),
				}
			});
		}
		CC604CProvider provider;
	}
}
