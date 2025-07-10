using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PackingSynchroniserTest : Customs.Business.Testing.PackingSynchroniserTest<JobDeclaration>
	{
		public void TestSynchronisePackageMarksAndNumbersWithMaxLength35()
		{
			var caDeclaration = declaration;
			caDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			caDeclaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			caDeclaration.ShipmentSynchroniser.SetEnabled(true, false);
			caDeclaration.ShipmentSynchroniser.Synchronise(true);
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 17;
			packLine.JL_MarksAndNumbers = "RED BLUE GREEN";

			AssertEquals("RED BLUE GREEN", caDeclaration.Packages[0].CW_MarksAndNos);

			packLine.JL_MarksAndNumbers = "012345678901234567890123456789ABCDEFGH";
			AssertEquals("35 characters", "012345678901234567890123456789ABCDE", caDeclaration.Packages[0].CW_MarksAndNos);
		}

		protected override JobDeclaration GetDeclarationPackingRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Import;
			return result;
		}

		protected override JobDeclaration GetDeclarationPackingNotRelevant()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Export;
			return result;
		}

		public override void TestSychroniseCore()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "12345678";
			consol.JK_RL_NKLoadPort = "KRBUS";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CRUX123456";
			consol.Shipments.Add(shipment);
			declaration.CusContainers.AddNew().CO_ContainerNumber = container.JC_ContainerNum;
			var containerised = shipment.OuterPackLines.AddNew();
			containerised.JL_PackageCount = 100;
			containerised.JL_F3_NKPackType = "PKG";
			containerised.JL_JC = container.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.ShipmentSynchroniser.Synchronise(true);
			var package = (Package)declaration.Packages[0];
			AssertEquals("Pack UQ", "PK", package.CW_PackType);

			declaration.CA_ServiceOption = "";
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Pack UQ", "PKG", package.CW_PackType);
		}
	}
}
