using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.ExportManifest.GUI.Testing
{
	[TestedType(typeof(ExportManifestForm))]
	sealed class ExportManifestFormTest : ZFormBasherTest
	{
		public void TestMenuVisibility()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			using (var form = new ExportManifestForm(header))
			{
				AssertMenuItemVisibility(form, expectManifestItems: true, expectDepartureItems: false, expectHelpItem: true);
				header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
				AssertMenuItemVisibility(form, expectManifestItems: true, expectDepartureItems: false, expectHelpItem: true);
				header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
				AssertMenuItemVisibility(form, expectManifestItems: false, expectDepartureItems: true, expectHelpItem: true);
				header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
				AssertMenuItemVisibility(form, expectManifestItems: true, expectDepartureItems: false, expectHelpItem: true);
				header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
				AssertMenuItemVisibility(form, expectManifestItems: true, expectDepartureItems: false, expectHelpItem: true);
				header.ED_DocumentStatus = "foo";
				header.ED_DepartureReportStatus = "bar";
				header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
				AssertMenuItemVisibility(form, expectManifestItems: true, expectDepartureItems: true, expectHelpItem: false);
			}
		}

		public void TestImportCANsMenuItemVisibility()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = ZString.Empty;
			header.ED_TransportMode = ZString.Empty;
			using (var form = new ExportManifestForm(header))
			{
				var importCANsMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Import CANs");
				AssertNotNull(importCANsMenuItem);
				AssertEquals(ZString.Empty, header.ED_ManifestType);
				AssertEquals(ZString.Empty, header.ED_TransportMode);
				AssertEquals(false, importCANsMenuItem.Visible);
				AssertEquals(false, importCANsMenuItem.Enabled);
				header.ED_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals(false, importCANsMenuItem.Visible);
				AssertEquals(false, importCANsMenuItem.Enabled);
				header.ED_ManifestType = AirManifestTypeList.Codes.ExportMainManifest;
				AssertEquals(false, importCANsMenuItem.Visible);
				AssertEquals(false, importCANsMenuItem.Enabled);
				header.ED_ManifestType = AirManifestTypeList.Codes.ConsolidationExportSubManifest;
				header.ED_TransportMode = ZString.Empty;
				AssertEquals(false, importCANsMenuItem.Visible);
				AssertEquals(false, importCANsMenuItem.Enabled);
				header.ED_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals(true, importCANsMenuItem.Visible);
				AssertEquals(false, importCANsMenuItem.Enabled);
				Factory.Save();
				AssertEquals("Enalbed once saved in database.", true, importCANsMenuItem.Enabled);
				header.ED_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Once the manifest is saved into database, it's always enabled no matter has changes or not.", true, importCANsMenuItem.Enabled);
			}
		}

		public void TestImportCANS()
		{
			// HB1, EXP, SEA, VS100, FL100, 20200401, AU2CO
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_HouseBill = "HB1";
			dec1.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec1.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec1.JE_VesselName = "VS100";
			dec1.JE_VoyageFlightNo = "FL100";
			dec1.JE_ExportDate = new ZDateTime(2020, 04, 01);
			dec1.JE_RL_NKPortOfLoading = "AU2CO";
			// HB2, EXP, SEA, VS100, FL100, 20200401, AU2CO
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_HouseBill = "HB2";
			dec2.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec2.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec2.JE_VesselName = "VS100";
			dec2.JE_VoyageFlightNo = "FL100";
			dec2.JE_ExportDate = new ZDateTime(2020, 04, 01);
			dec2.JE_RL_NKPortOfLoading = "AU2CO";
			// HB3, EXP, AIR, VS100, FL100, 20200401, AU2CO
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_HouseBill = "HB3";
			dec3.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec3.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec3.JE_VesselName = "VS100";
			dec3.JE_VoyageFlightNo = "FL100";
			dec3.JE_ExportDate = new ZDateTime(2020, 04, 01);
			dec3.JE_RL_NKPortOfLoading = "AU2CO";
			// HB4, IMP, SEA, VS100, FL100, 20200401, AU2CO
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_HouseBill = "HB4";
			dec4.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			dec4.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec4.JE_VesselName = "VS100";
			dec4.JE_VoyageFlightNo = "FL100";
			dec4.JE_ExportDate = new ZDateTime(2020, 04, 01);
			dec4.JE_RL_NKPortOfLoading = "AU2CO";
			// HB5, EXP, SEA, VS200, FL100, 20200401, AU2CO
			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_HouseBill = "HB5";
			dec5.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec5.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec5.JE_VesselName = "VS200";
			dec5.JE_VoyageFlightNo = "FL100";
			dec5.JE_ExportDate = new ZDateTime(2020, 04, 01);
			dec5.JE_RL_NKPortOfLoading = "AU2CO";
			// HB6, EXP, SEA, VS100, FL200, 20200401, AU2CO
			var dec6 = Factory.New<JobDeclaration>();
			dec6.JE_HouseBill = "HB6";
			dec6.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec6.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec6.JE_VesselName = "VS100";
			dec6.JE_VoyageFlightNo = "FL200";
			dec6.JE_ExportDate = new ZDateTime(2020, 04, 01);
			dec6.JE_RL_NKPortOfLoading = "AU2CO";
			// HB7, EXP, SEA, VS100, FL100, 20200430, AU2CO
			var dec7 = Factory.New<JobDeclaration>();
			dec7.JE_HouseBill = "HB1";
			dec7.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec7.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec7.JE_VesselName = "VS100";
			dec7.JE_VoyageFlightNo = "FL100";
			dec7.JE_ExportDate = new ZDateTime(2020, 04, 30);
			dec7.JE_RL_NKPortOfLoading = "AU2CO";
			// HB8, EXP, SEA, VS100, FL100, 20200401, NZ2CO
			var dec8 = Factory.New<JobDeclaration>();
			dec8.JE_HouseBill = "HB1";
			dec8.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec8.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec8.JE_VesselName = "VS100";
			dec8.JE_VoyageFlightNo = "FL100";
			dec8.JE_ExportDate = new ZDateTime(2020, 04, 01);
			dec8.JE_RL_NKPortOfLoading = "NZ2CO";
			Factory.Save();
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = AirManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_VesselName = "VS100";
			header.ED_VoyageNumber = "FL100";
			header.ED_DepartureDate = new ZDateTime(2020, 04, 01);
			header.ED_RL_NKPortOfDeparture = "AU2CO";
			AssertEquals("Precondition", 0, header.Lines.Count);
			using (var form = new ExportManifestForm(header))
			{
				var importCANsMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Import CANs");
				importCANsMenuItem.PerformClick();
				var popup = (ZArchitecture.GUI.Internal.EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				var module = popup.Module_ForTest;
				AssertEquals(ModuleIDs.Customs.JobDeclaration, module.ID);
				((ZFilterStripBaseControl)module.filter).FirePerformSearch();
				((ZDisplayGrid)module.DisplayGrid).SelectAllElements();
				popup.ExposedOKButtonForTesting.PerformClick();
				AssertNotNull(header.Lines.Cast<ExportCustomsManifestLines>().FirstOrDefault(x => x.EL_AirWayBill == "HB1"));
				AssertNotNull(header.Lines.Cast<ExportCustomsManifestLines>().FirstOrDefault(x => x.EL_AirWayBill == "HB2"));
				AssertEquals("Air and Import declarations with Vessel(VS100) and Voyage(FL100) and Departure Date(20200401) and Loading Port(AU2CO) are filtered from the collection", 2, header.Lines.Count);
			}
		}

		protected override Form GetFormToBashCore() => new ExportManifestForm(Factory.New<ExportCustomsManifestHeader>());

		void AssertMenuItemVisibility(ExportManifestForm form, bool expectManifestItems, bool expectDepartureItems, bool expectHelpItem)
		{
			var visibleMenuItems = form.exportManifestMenu.MenuItems.OfType<MenuItem>().Where(x => x.Visible).ToArray();
			var idx = 0;
			if (expectManifestItems)
			{
				AssertEquals("&Declare Manifest", visibleMenuItems[idx++].Text);
				AssertEquals("&Withdraw Manifest", visibleMenuItems[idx++].Text);
			}

			AssertEquals("&Reset to original", visibleMenuItems[idx++].Text);
			if (expectDepartureItems)
			{
				AssertEquals("&Declare Departure Report", visibleMenuItems[idx++].Text);
				AssertEquals("&Withdraw Departure Report", visibleMenuItems[idx++].Text);
			}

			if (expectHelpItem)
			{
				AssertEquals("Messaging Problems? Click for HELP.", visibleMenuItems[idx++].Text);
			}

			AssertEquals("-", visibleMenuItems[idx++].Text);
			AssertEquals("&Import Data", visibleMenuItems[idx++].Text);
			AssertEquals("Number of items", idx, visibleMenuItems.Length);
		}
	}
}
