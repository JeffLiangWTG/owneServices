using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class JobDeclarationSynchroniserTest : EU.Business.Declaration.Testing.JobDeclarationSynchroniserTest
	{
		public void TestDefaultJobDeclarationContainerMode()
		{
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Code)).Code;

			var consolidationContainerModes = new[]
			{
				ContainerModes.BuyersConsol,
				ContainerModes.FCL,
				ContainerModes.Groupage
			};

			var shipmentContainerModes = new[]
			{
				ContainerModes.BuyersConsol,
				ContainerModes.LCL
			};

			foreach (var consolidationContainerMode in consolidationContainerModes)
			{
				foreach (var shipmentContainerMode in shipmentContainerModes)
				{
					var consol = Factory.New<ForwardingConsol>();
					consol.JK_RL_NKLoadPort = "GBANY";
					consol.JK_RL_NKDischargePort = localPort;
					consol.JK_ConsolMode = consolidationContainerMode;

					var shipment = consol.Shipments.AddNew();
					shipment.JS_PackingMode = shipmentContainerMode;

					var declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_JS = shipment.PK;
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.ShipmentSynchroniser.Synchronise();
					AssertEquals("When the customs declaration is created using shipment and shipment is LCL or BCM and consolidation is FCL or GRP or BCN, default value of JE_ContainerMode is CNT.", ContainerModes.Containerised, declaration.JE_ContainerMode);
				}
			}

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKLoadPort = "GBANY";
			consol2.JK_RL_NKDischargePort = localPort;
			consol2.JK_ConsolMode = ContainerModes.BuyersConsol;

			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_PackingMode = ContainerModes.Liquid;

			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			declaration2.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration2.ShipmentSynchroniser.Synchronise();
			AssertEquals("If the condition is not met, keep the previous default logic.", ContainerModes.Liquid, declaration2.JE_ContainerMode);

			consol2.JK_ConsolMode = ContainerModes.Liquid;
			shipment2.JS_PackingMode = ContainerModes.LCL;
			declaration2.ShipmentSynchroniser.Synchronise();
			AssertEquals("If the condition is not met, keep the previous default logic.", ContainerModes.LCL, declaration2.JE_ContainerMode);
		}
	}
}
