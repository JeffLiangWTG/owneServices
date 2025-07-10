using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.SeaCargo.Testing
{
	[TestedType(typeof(AUCustomsSeaCargoController))]
	sealed class AUCustomsSeaCargoControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AUCustomsSeaCargoController testController = new AUCustomsSeaCargoController();
			AssertEquals(typeof(ForwardingConsol), testController.TypeOfTopLevelBusinessObject);
		}

		[ExpectNoExceptions]
		public void TestSecurityCheckPointWhenWorkflowIsAccessedOnSeaCargo()
		{
			var consol = Factory.New<ForwardingConsol>();
			var seaCargoJob = Factory.New<CusSCAOceanBill>();
			seaCargoJob.CB_ParentId = consol.PK;
			seaCargoJob.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			var controller = new AUCustomsSeaCargoController();
			using (ConsolForm form = (ConsolForm)controller.ShowEditForm(seaCargoJob))
			{
				form.Show();
				var mainTabControl = GetZTabControl(form.Controls);
				var workflowTabPage = mainTabControl.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x is MasterFiles.GUI.ZWorkflowTabPage);
				AssertNotNull(workflowTabPage);
				mainTabControl.SelectedTab = workflowTabPage;
			}
		}

		public void TestGetLoadedBusinessEntityInLocalFactory()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Factory.Save();
			var testController = new AUCustomsSeaCargoControllerForTest();
			AssertEquals("Enterprise.Freight.Forwarding.Business.ForwardingConsol", testController.GetLoadedBusinessEntityInLocalFactory(oceanBill).ToString());
		}

		public void TestGetForm()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var testController = new AUCustomsSeaCargoControllerForTest();
			using (var testForm = ((ZControllerInternals)testController).GetForm(consol))
			{
				AssertEquals("PlugInIDToSelectOnLoaded", ControllerIDs.Customs.AU.SeaCargo, ((ConsolForm)testForm).PlugInIDToSelectOnLoaded);
				AssertEquals("Form Type", typeof(ConsolForm), testForm.GetType());
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.SeaCargo;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var consol = base.GetBusinessObjectWithoutValidationErrors() as ForwardingConsol;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			Factory.Save();
			return consol;
		}

		ZTabControl GetZTabControl(Control.ControlCollection controls)
		{
			var tabControls = controls.OfType<ZTabControl>();
			if (tabControls.Count() == 1)
			{
				return tabControls.FirstOrDefault();
			}

			foreach (Control control in controls)
			{
				var result = GetZTabControl(control.Controls);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		sealed class AUCustomsSeaCargoControllerForTest : AUCustomsSeaCargoController
		{
			public AUCustomsSeaCargoControllerForTest()
			{
			}

			internal new IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity) => base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}
	}
}
