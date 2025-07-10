using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.CA.GUI.PlugIns.Testing
{
	sealed class RNSCurrentDependentPlugInTest : ZPlugInGenericTest
	{
		public void TestOverrides()
		{
			var container = Factory.New<TallyContainer>();
			container.PackUnpackShipments.AddNew();

			using (var plugin = new RNSCurrentDependentPlugIn(new RNSPlugInSupportTallyWrapper(container)))
			{
				AssertEquals("Name", "RNS/MF", plugin.Name);
				AssertEquals("TopLevelMenu", null, plugin.TopLevelMenu);
				AssertEquals("UserControl", typeof(RNSUserControl), plugin.UserControl.GetType());
			}
		}

		public void TestDataBinding()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var container = Factory.New<TallyContainer>();
			consol.Containers.Add(container);

			var shipment1 = container.PackUnpackShipments.AddNew();
			var shipment2 = container.PackUnpackShipments.AddNew();

			Assert("Precondition:", !container.IsImport());

			using (var form = new TallyFormForTesting(container))
			{
				form.Show();

				var rnsPlugIn = form.ShipmentsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFTallyShipmentsPlugIn);
				var rnsUserControl = rnsPlugIn.UserControl as RNSUserControl;

				Assert("ShipmentRNSUserControl should not be disabled for shipment1", !rnsPlugIn.Enabled);

				container.Consol.JK_RL_NKLoadPort = "USAAA";
				container.Consol.JK_RL_NKDischargePort = "CABBB";
				Assert("Precondition:", container.IsImport());

				Assert("ShipmentRNSUserControl should be enabled for shipment1", rnsPlugIn.Enabled);

				var rnsMessaging11 = (rnsPlugIn.UserControl as RNSUserControl).RNSMessaging;
				AssertEquals("A RNSMessagingBO should be created for shipment1", shipment1, rnsMessaging11.PlugInSupport.Master);

				form.ShipmentsGrid.SelectSingleElement(shipment2);
				var rnsMessaging2 = (rnsPlugIn.UserControl as RNSUserControl).RNSMessaging;
				AssertEquals("A RNSMessagingBO should be created for shipment2", shipment2, rnsMessaging2.PlugInSupport.Master);

				form.ShipmentsGrid.SelectSingleElement(shipment1);
				var rnsMessaging12 = (rnsPlugIn.UserControl as RNSUserControl).RNSMessaging;
				AssertEquals("A RNSMessagingBO should be created for shipment1", shipment1, rnsMessaging12.PlugInSupport.Master);

				AssertSame("The RNSMessagingBOs for shipment1 should be the same one", rnsMessaging11, rnsMessaging12);
			}
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var container = Factory.New<TallyContainer>();
			container.PackUnpackShipments.AddNew();
			container.PackUnpackShipments.AddNew();
			return new RNSCurrentDependentPlugIn(new RNSPlugInSupportTallyWrapper(container));
		}

		sealed class TallyFormForTesting : ZForm
		{
			public TallyFormForTesting(TallyContainer container)
				: base(container)
			{
				ShipmentsGrid = new ZGrid();
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
				ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				BindingSource.SetBindingMember(ShipmentsGrid, "PackUnpackShipments");

				ShipmentsTabControl = new ZTabControl();

				Controls.Add(ShipmentsGrid);
				Controls.Add(ShipmentsTabControl);
			}

			internal ZGrid ShipmentsGrid;
			internal ZTabControl ShipmentsTabControl;

			protected override void OnLoad(System.EventArgs e)
			{
				base.OnLoad(e);

				ShipmentsTabControl.PlugIns.AddCurrentDependentPlugIn(ControllerIDs.Customs.CA.RNSMFTallyShipmentsPlugIn, ShipmentsGrid);
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);

				ShipmentsGrid.Dispose();
				ShipmentsTabControl.Dispose();
			}
		}
	}
}
