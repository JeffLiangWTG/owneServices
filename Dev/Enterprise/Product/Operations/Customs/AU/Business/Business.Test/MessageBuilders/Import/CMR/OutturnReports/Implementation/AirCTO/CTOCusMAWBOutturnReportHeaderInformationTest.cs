using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusMAWBOutturnReportHeaderInformationTest : CusMAWBBaseOutturnReportHeaderInformationAbstractTest
	{
		public void TestLines()
		{
			Underbond.LinkedObject = MAWB;
			var hAWB = MAWB.ChildBills.AddNew();
			var outturn = Underbond.Outturns.AddNew();
			outturn.Parent = hAWB;
			AssertEquals("Length", 1, HeaderInfo.Lines.Length);
		}

		public void TestDatabaseLines()
		{
			Underbond.LinkedObject = MAWB;
			var hAWB = MAWB.ChildBills.AddNew();
			var outturn = Underbond.Outturns.AddNew();
			outturn.Parent = hAWB;
			AssertEquals("Length", 0, HeaderInfo.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("Length", 1, HeaderInfo.DatabaseLines.Length);
		}

		public void TestPartShipIsMessageLine()
		{
			var cTOCusMAWB = Factory.New<CTOCusMAWB>();
			var partShip = cTOCusMAWB.ChildBills.AddNew().PartShips.AddNew();
			partShip.CG_ArrivalDate = new ZDateTime(2005, 7, 15);
			partShip.CG_FlightNo = "QF123";
			partShip.CG_RL_NKDischargePort = "AUSYD";
			MAWB.CM_ArrivalDate = new ZDateTime(2005, 7, 15);
			MAWB.CM_FlightNo = "QF123";
			MAWB.CM_RL_NKDischargePort = "AUSYD";

			Underbond.LinkedObject = MAWB;
			var outturn = Underbond.Outturns.AddNew();
			outturn.Parent = partShip;
			AssertEquals("Length", 1, HeaderInfo.Lines.Length);
		}

		protected override CusMAWBBase GetCusMAWB() => Factory.New<CTOCusMAWB>();

		protected override CusUnderbondOutturnReportHeaderInformation GetHeaderInfo() => new CTOCusMAWBOutturnReportHeaderInformation(MAWB, Underbond);

		new CTOCusMAWB MAWB => (CTOCusMAWB)base.MAWB;

		CTOCusMAWBOutturnReportHeaderInformation HeaderInfo => (CTOCusMAWBOutturnReportHeaderInformation)GetHeaderInfo();
	}
}
