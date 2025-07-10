using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.SeaCargo.Test
{
	sealed class SeaCargoProcessorJobTest : TestCaseWithFactory
	{
		public void TestSendChildren()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var job = new SeaCargoProcessorJob(oceanBill);
			Assert(job.SendChildren);
		}

		public void TestReasonWhyNotAcceptable()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var job = new SeaCargoProcessorJob(oceanBill);
			oceanBill.CB_RL_NKPortOfDischarge = "USLAX";
			AssertEquals("The final discharge port is not an Australian port.", job.ReasonWhyNotAcceptable);
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_GB = ZGuid.Empty;
			AssertContains("Valid branch is required.", job.ReasonWhyNotAcceptable);
		}
	}
}
