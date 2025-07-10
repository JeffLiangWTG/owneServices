using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaUnderbondSelectorLine))]
	sealed class SeaUnderbondSelectorLineTest : UnderbondSelectorLineTest
	{
		public void TestUpdateStatuses()
		{
			var consol = Factory.New<CusSCAOceanBill>();
			consol.CB_OceanBill = "OC";
			consol.CB_LloydsIMO = "1234";
			consol.CB_Voyage = "123";
			var container = consol.Containers.AddNew();
			container.CN_ContainerNumber = "CN";
			var house = consol.HouseBills.AddNew();
			container.Pivots.AddNew().CV_CA = house.PK;
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "192K";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "1234";
			outturnHeader.C6_VoyageNum = "123";
			outturnHeader.C6_OutturningPremiseID = "192K";
			outturnHeader.C6_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;

			var line = new SeaUnderbondSelectorLine(underbond);
			line.UpdateStatuses();
			AssertEquals(OutturnStatus.AwaitingResponseFromCustoms, line.OutturnStatus);

			outturnHeader.C6_MessageStatus = CMRBaseStatuses.Codes.AmendmentAccepted;
			line.UpdateStatuses();
			AssertEquals(OutturnStatus.ReadyForScanning, line.OutturnStatus);
		}

		protected override UnderbondSelectorLine GetUnderbondSelectorLine(CusUnderbond cusUnderbond)
		{
			return new SeaUnderbondSelectorLine(cusUnderbond);
		}
	}
}
