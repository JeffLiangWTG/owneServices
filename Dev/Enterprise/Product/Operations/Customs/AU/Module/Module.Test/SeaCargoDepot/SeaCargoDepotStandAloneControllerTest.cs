using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoDepotStandAloneController))]
	sealed class SeaCargoDepotStandAloneControllerTest : ZControllerBasherTest
	{
		public void TestPlugin()
		{
			SeaCargoDepotStandAloneController controller = new SeaCargoDepotStandAloneController();
			TallyContainer container = Factory.New<TallyContainer>();
			TallyOutturnHeader header = Factory.New<TallyOutturnHeader>();
			TallyOutturn outturn = header.Outturns.AddNew();
			CFSTallyContainerWrapper wrapper = CFSTallyContainerWrapper.Load(container);
			wrapper.Outturns.Add(outturn);
			using (ZPlugIn plugin = controller.GetPlugInInternal(container))
			{
				AssertNotNull(plugin);
				AssertEquals(typeof(SeaCargoDepotOutturnPlugin), plugin.GetType());
				SeaCargoDepotOutturnPlugin scdPlugin = (SeaCargoDepotOutturnPlugin)plugin;
				AssertEquals(header, scdPlugin.BusinessEntity);
			}
		}

		public void TestCheckPoints()
		{
			AssertEquals(Env.Security.AUCustomsSCADepotModify, new SeaCargoDepotStandAloneController().CheckPointForDeleteExposedForTest);
			AssertEquals(Env.Security.AUCustomsSCADepotModify, new SeaCargoDepotStandAloneController().CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.AUCustomsSCADepotModify, new SeaCargoDepotStandAloneController().CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.AUCustomsSCADepot, new SeaCargoDepotStandAloneController().CheckPointForViewExposedForTest);
		}

		public void TestSeaCargoDepotModuleID()
		{
			var controller = new SeaCargoDepotStandAloneController();
			AssertEquals(ModuleIDs.Customs.AU.SeaCargoOutturnBills, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			SeaCargoDepotStandAloneController controller = new SeaCargoDepotStandAloneController();
			AssertEquals("Should return a CusOutturnHeader type", typeof(CusOutturnHeader), controller.TypeOfTopLevelBusinessObject);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController;

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}
	}
}
