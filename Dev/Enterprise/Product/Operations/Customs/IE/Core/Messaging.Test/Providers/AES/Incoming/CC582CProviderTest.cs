using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC582C;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	sealed class CC582CProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestResponseDateLimit()
		{
			AssertEquals(new ZDate(2020, 12, 22), provider.ResponseDateLimit);
		}

		public void TestNonExitedExportRequest()
		{
			AssertEquals(new ZDate(2022, 11, 04), provider.NonExitedExportRequestDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC582CProvider(new Cc582C
			{
				ExportOperation = new CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes.ExportOperationType27
				{
					Mrn = "MRN",
					LimitForResponseDate = new DateTime(2020, 12, 22, 15, 09, 59),
					RequestOnNonExitedExportDate = new DateTime(2022, 11, 04, 15, 09, 59),
				}
			});
		}
		CC582CProvider provider;
	}
}
