using System;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC599C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC599CProviderTest : TestCaseWithFactory
	{
		public void TestExportOperation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("LRN", provider.LocalReferenceNumber);
				AssertEquals("MRN", provider.MovementReferenceNumber);
			});
		}

		public void TestExitControlResult()
		{
			AssertEquals("A1", provider.ExitResultCode);
			AssertEquals(new ZDateTime(2020, 12, 22), provider.ExitDate);
			AssertEquals(new ZDateTime(2021, 1, 1), provider.ExitStoppedDate);
			AssertEquals("0", provider.StateofSeals);
		}

		public void TestStatus()
		{
			AssertEquals("EXP", provider.Status);
			cc599c.ExitControlResult.Code = "B1";
			AssertEquals("REF", provider.Status);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cc599c = new Cc599C
			{
				ExportOperation = new ExportOperationType53
				{
					Lrn = "LRN",
					Mrn = "MRN"
				},
				ExitControlResult = new ExitControlResultType02
				{
					Code = "A1",
					ExitDate = new DateTime(2020, 12, 22),
					ExitStoppedDate = new DateTime(2021, 1, 1),
					StateOfSeals = "0"
				}
			};
			provider = new CC599CProvider(cc599c);
		}
		Cc599C cc599c;
		CC599CProvider provider;
	}
}
