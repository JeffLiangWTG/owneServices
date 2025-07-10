using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC574C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC574CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestAmendmentSubmissionDateAndTime()
		{
			AssertEquals(new ZDateTime(2022, 02, 07, 15, 09, 59), provider.AmendmentDateAndTime);
		}

		public void TestAmendmentAcceptanceDateAndTime()
		{
			AssertEquals(new ZDateTime(2022, 02, 07, 15, 09, 59), provider.AmendmentAcceptanceDateAndTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC574CProvider(new Cc574C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType56
				{
					Mrn = "MRN",
					AmendmentDateAndTime = new DateTime(2022, 02, 07, 15, 09, 59),
					AmendmentAcceptanceDateAndTime = new DateTime(2022, 02, 07, 15, 09, 59),
				}
			});
		}
		CC574CProvider provider;
	}
}
