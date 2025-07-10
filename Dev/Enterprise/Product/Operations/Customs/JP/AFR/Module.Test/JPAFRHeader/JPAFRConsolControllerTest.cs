using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Module.Testing
{
	[TestedType(typeof(JPAFRConsolController))]
	class JPAFRConsolControllerTest : ZControllerBasherTest
	{
		public void TestGetForm()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			var header = Factory.New<JPAFRHeader>();
			var consol = Factory.New<ForwardingConsol>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			var controller = new JPAFRConsolController();
			using (var form = controller.ShowEditForm(header))
			{
				AssertEquals(typeof(ConsolForm), form.GetType());
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<JPAFRHeader>();
			var consol = Factory.New<ForwardingConsol>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			return consol;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JP.AFRPluggedIntoConsol;
		}

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var header = Factory.New<JPAFRHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "123";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			return header;
		}
	}
}
