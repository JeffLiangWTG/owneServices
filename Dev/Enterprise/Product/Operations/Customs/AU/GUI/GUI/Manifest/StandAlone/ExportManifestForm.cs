using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	public partial class ExportManifestForm : ZForm
	{
		public ExportManifestForm()
		{
			InitializeComponent();
		}

		public ExportManifestForm(ExportCustomsManifestHeader header) : base(header)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl);
			exportManifestDeclarationUserControl.Header = header;
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			this.header = header;
			SetupMenu();
			SetupHooks();
		}

		readonly ExportCustomsManifestHeader header;
		protected internal ExportManifestMenu exportManifestMenu;
		MenuItem importCANsMenuItem;

		protected override ZTabControl TopLevelTabControl => exportManifestDeclarationUserControl.DepartureReportDetailsTabControl;

		public override string FormCaption
		{
			get
			{
				string result = "Export Manifest";
				if (header != null && !header.ED_BGMReference.IsEmpty)
				{
					result += " - " + header.ED_BGMReference;
				}

				return result;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && !header.MessageManager.SendAnyAmendmentsNeeded(new SendsMessagesToCustomsGUI()))
			{
				result = ContinueWithSave.No;
			}
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				header.ED_ManifestTypeInfo.ValueChanged -= ED_ManifestTypeInfo_ValueChanged;
				header.ED_TransportModeInfo.ValueChanged -= ED_TransportModeInfo_ValueChanged;
				header.Factory.Saved -= HeaderFactoryOnSaved;
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		void SetupMenu()
		{
			exportManifestMenu = ExportManifestMenu.GetMenu();
			exportManifestMenu.Header = header;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.Count - 1, exportManifestMenu);

			importCANsMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, Res.GetString("{3508EE3E-5428-4D21-8F71-FD0F4ECE5814}", "Import CANs"), ImportCANsMenuItem_Click);
			SetImportCANsMenuItemVisibility();
		}

		void SetupHooks()
		{
			header.ED_ManifestTypeInfo.ValueChanged += ED_ManifestTypeInfo_ValueChanged;
			ED_ManifestTypeInfo_ValueChanged(this, EventArgs.Empty);

			header.ED_TransportModeInfo.ValueChanged += ED_TransportModeInfo_ValueChanged;
			ED_TransportModeInfo_ValueChanged(this, EventArgs.Empty);

			if (!header.IsInDatabase)
			{
				header.Factory.Saved += HeaderFactoryOnSaved;
			}
		}

		void ED_ManifestTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (header.IsOld)
			{
				exportManifestMenu.ShowOld();
			}
			else if (header.IsDeparture)
			{
				exportManifestMenu.ShowDepartureMenuItems();
			}
			else
			{
				exportManifestMenu.ShowManifestMenuItems();
			}

			SetImportCANsMenuItemVisibility();
		}

		void ED_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetImportCANsMenuItemVisibility();
		}

		void HeaderFactoryOnSaved(BusinessObjectFactory factory, bool savedsuccessfully)
		{
			if (savedsuccessfully)
			{
				SetImportCANsMenuItemVisibility();
				header.Factory.Saved -= HeaderFactoryOnSaved;
			}
		}

		void SetImportCANsMenuItemVisibility()
		{
			importCANsMenuItem.Visible = !header.ED_TransportMode.IsEmpty && header.ED_ManifestType == AirManifestTypeList.Codes.ConsolidationExportSubManifest;
			importCANsMenuItem.Enabled = header.IsInDatabase;
		}

		void ImportCANsMenuItem_Click(object sender, EventArgs e)
		{
			var filteredCollection = new JobDeclarationCollection(header.Factory);
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Shipment Type", "Property", new ZString(Common.Shared.SharedJobMessageTypeList.Codes.Export), isRemovable: false));

			var flightOrVoyage = header.IsSea ? header.ED_VoyageNumber : header.ED_FlightNumber;
			var vessel = header.ED_VesselName;
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Vessel and Flight/Voyage #", "Property", flightOrVoyage, flightOrVoyage.IsEmpty || vessel.IsEmpty));
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Vessel and Flight/Voyage #", "NkProperty", vessel, flightOrVoyage.IsEmpty || vessel.IsEmpty));

			var departureDate = header.ED_DepartureDate;
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Departure at Loading Port", "PropertySearch", ModuleDateFilter.SpecifiedDateRange, departureDate.IsEmpty));
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Departure at Loading Port", "Property1", departureDate, departureDate.IsEmpty));
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Departure at Loading Port", "Property2", departureDate, departureDate.IsEmpty));

			var loadingPort = header.ED_RL_NKPortOfDeparture;
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Consol Load/Discharge", "Property1", loadingPort, loadingPort.IsEmpty));
			filteredCollection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Transport Mode", "Property", header.ED_TransportMode, isRemovable: true));

			var chooser = new ZRecordChooser<JobDeclaration>(ModuleIDs.Customs.JobDeclaration, filteredCollection);
			chooser.ShowModal(this, header.CreateLinesFromJobDeclarations);
		}
	}
}
