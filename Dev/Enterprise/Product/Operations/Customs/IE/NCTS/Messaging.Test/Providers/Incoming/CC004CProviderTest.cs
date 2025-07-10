using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC004C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC004CProviderTest : TestCaseWithFactory
	{
		public void TestMRN()
		{
			AssertEquals("MRN", "Mrn123456", provider.MRN);
		}

		public void TestSubmissionDateAndTime()
		{
			AssertEquals("SubmissionDateAndTime", new ZDateTime(2022, 12, 22), provider.AmendmentSubmissionDateTime);
		}

		public void TestAcceptanceDateAndTime()
		{
			AssertEquals("AcceptanceDateAndTime", new ZDateTime(2022, 5, 2), provider.AmendmentAcceptanceDateTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC004CProvider(new Cc004CType
			{
				TransitOperation = new TransitOperationType01
				{
					Mrn = "Mrn123456",
					AmendmentSubmissionDateAndTime = new DateTime(2022, 12, 22),
					AmendmentAcceptanceDateAndTime = new DateTime(2022, 5, 2),
				}
			});
		}
		CC004CProvider provider;
	}
}
