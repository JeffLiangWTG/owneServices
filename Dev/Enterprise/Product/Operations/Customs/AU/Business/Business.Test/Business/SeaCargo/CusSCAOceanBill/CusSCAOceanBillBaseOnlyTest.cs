using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetNewCusSCAOceanBillProcessTaskCollection()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>", typeof(ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>), ((IWorkflowProvider)oceanBill).WorkflowItems);
		}

		public void TestDetails()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OBL32423";
			oceanBill.CB_VesselName = "SENORSHIP";
			oceanBill.CB_Voyage = "23523";
			string expectedResult = @"OCEAN BILL DETAILS:
Ocean Bill: OBL32423
Discharge Port: AUBNE
Vessel: SENORSHIP
Voyage: 23523";
			AssertMultilineASCIIEquals("Details should match", expectedResult, oceanBill.Details);
		}

		public void TestShortDescription()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "12345";
			AssertEquals("ShortDescription", "Ocean Bill: 12345", oceanBill.ShortDescription);
		}

		public void TestIsAutoLogged()
		{
			var bizO = Factory.New<CusSCAOceanBill>();
			Assert("IsAutoLogged", bizO.IsAutoAdminBusinessObjectLoggerEnabled);
		}
	}
}
