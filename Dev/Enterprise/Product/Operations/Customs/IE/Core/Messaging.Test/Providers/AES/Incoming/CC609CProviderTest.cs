using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC609C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	sealed class CC609CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestInvalidationRequestDateTime()
		{
			AssertEquals(new ZDateTime(2022, 10, 07, 15, 09, 59), provider.InvalidationRequestDateTime);
		}

		public void TestInvalidationDecisionDateTime()
		{
			AssertEquals(new ZDateTime(2022, 10, 10, 15, 09, 59), provider.InvalidationDecisionDateTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC609CProvider(new Cc609C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType30
				{
					Mrn = "MRN",
					InvalidationRequestDateAndTime = new DateTime(2022, 10, 07, 15, 09, 59),
					InvalidationDecisionDateAndTime = new DateTime(2022, 10, 10, 15, 09, 59),
				}
			});
		}
		CC609CProvider provider;
	}
}
