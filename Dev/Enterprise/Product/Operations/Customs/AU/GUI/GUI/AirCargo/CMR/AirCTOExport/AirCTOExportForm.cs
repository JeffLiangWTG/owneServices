using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOExportForm : CMRMessagingForm
	{
		public AirCTOExportForm(AirCTOExportCustomsManifestHeader exportManifestHeader) : base(exportManifestHeader)
		{
			if (exportManifestHeader == null)
			{
				throw new ArgumentNullException(nameof(exportManifestHeader));
			}

			InitializeComponent();
			header = exportManifestHeader;

			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			SetupHooks();
			SetupMenu();
		}

		void SetupHooks()
		{
			this.MainTabPage.BindingOrFirstShown += delegate
			{
				this.exportManifestDetailsUserControl1.Header = header;
			};
		}

		void SetupMenu()
		{
			exportManifestMenu = ExportManifestMenu.GetMenu();
			exportManifestMenu.Header = header;
			this.MainMenu.MenuItems.Add(this.MainMenu.MenuItems.Count - 1, exportManifestMenu);

			header.ED_ManifestTypeInfo.ValueChanged += ED_ManifestTypeInfo_ValueChanged;
			ED_ManifestTypeInfo_ValueChanged(this, EventArgs.Empty);
		}

		void ED_ManifestTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (header.IsCTO)
			{
				exportManifestMenu.Visible = false;
				CTOMenu.Visible = true;
			}
			else if (header.IsMainManifest || header.IsConsolidation)
			{
				exportManifestMenu.Visible = true;
				CTOMenu.Visible = true;
				exportManifestMenu.ShowManifestMenuItems();
			}
			else
			{
				CTOMenu.Visible = false;
				exportManifestMenu.Visible = true;

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
			}
		}

		internal ExportManifestMenu exportManifestMenu;

		internal AirCTOExportMenu CTOMenu
		{
			get
			{
				if (ctoMenu == null)
				{
					ctoMenu = new AirCTOExportMenu((AirCTOExportMessageManager)Manager);
				}
				return ctoMenu;
			}
		}
		AirCTOExportMenu ctoMenu;

		public override string FormCaption
		{
			get { return "Air CTO - Export"; }
		}

		protected override MenuItem GetMessagingMenu()
		{
			return CTOMenu;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (header != null)
				{
					header.ED_ManifestTypeInfo.ValueChanged -= ED_ManifestTypeInfo_ValueChanged;
				}
			}

			base.Dispose(disposing);
		}

		protected override Business.MultiMessageManager GetManager()
		{
			return new AirCTOExportMessageManager((AirCTOExportCustomsManifestHeader)BusinessEntity);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			ContinueWithSave result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes)
			{
				if (!((ExportCustomsManifestHeader)BusinessEntity).MessageManager.SendAnyAmendmentsNeeded(new SendsMessagesToCustomsGUI()))
				{
					result = ContinueWithSave.No;
				}
			}
			return result;
		}

		readonly ExportCustomsManifestHeader header;
	}
}
