using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class LicenceDatabaseRegistrationWizardFilterControl : ZFilterStripControl
	{
		int MaxRowsToLoad => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
		readonly LicenceDatabaseNonDependentCollection licenceDatabaseCollection;
		readonly FilterStripBusinessObject filterStrip;
		const string SetMasterOrgsMenuItemText = "Set Master Orgs";

		FilteredGridLoader SearchManager => searchManager ?? (searchManager = CreateSearchManager());
		FilteredGridLoader searchManager;

		FilteredGridLoader CreateSearchManager()
			=> new FilteredGridLoader(FilterBusinessObject, new ResultCountMessage(this, MaxRowsToLoad, MaxRowsToLoad), false, ModuleIDs.GlbCompanyCampaignItem, () => licenceDatabaseCollection.Factory, typeof(LicenceDatabase));

		public LicenceDatabaseRegistrationWizardFilterControl(LicenceDatabaseNonDependentCollection licenceDatabaseCollection, FilterStripBusinessObject filterBusinessObject)
			: base(licenceDatabaseCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetupContextMenu();
			PerformSearch += LicenceDatabaseRegistrationWizardFilterControl_PerformSearch;
			this.licenceDatabaseCollection = licenceDatabaseCollection;
			filterStrip = filterBusinessObject;

			FilteredGrid.MouseDoubleClick += FilteredGrid_MouseDoubleClick;

			ToolStrip.AllowOutsideOfParent();
		}

		void LicenceDatabaseRegistrationWizardFilterControl_PerformSearch(object sender, EventArgs e)
		{
			var factory = SearchManager.GetNewFactory();
			var result = SearchManager.PerformSearch(factory, licenceDatabaseCollection.TypeOfElements, filterStrip.Filter);
			if (result.Type == PerformSearchResultType.Success)
			{
				SearchManager.PushItemsIntoCollection(licenceDatabaseCollection, result, null);
			}

			if (licenceDatabaseCollection.Any())
			{
				FilteredGrid.CurrentRowIndex = 0;
			}
		}

		void FilteredGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (FilteredGrid.HitTest(e.X, e.Y).Row > -1)
			{
				var current = FilteredGrid.ListManager.GetCurrent() as LicenceDatabase;
				if (current != null)
				{
					ZControllerFactory.Create(ClientControllerRegistration.LicenceDatabase).ShowEditForm(current);
				}
			}
		}

		void SetupContextMenu()
		{
			Grid.ContextMenu.MenuItems.Add("-");
			Grid.ContextMenu.MenuItems.Add(new ZMenuItem(SetMasterOrgsMenuItemText, SetMasterOrgs) { Name = SetMasterOrgsMenuItemText });
			Grid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var setMasterOrgsMenuItem = Grid.ContextMenu.MenuItems.FindByName(SetMasterOrgsMenuItemText);
			setMasterOrgsMenuItem.Enabled = Grid.SelectedElements.OfType<LicenceDatabase>().Any(x => x.LD_OH_WebAccessOrg.IsEmpty);
		}

		void SetMasterOrgs(object sender, EventArgs e)
		{
			if (Grid.SelectedRowCount > 0)
			{
				var databasesUpdatedCount = 0;
				var databasesSkippedCount = 0;
				var threshold = EDIDataRegistry.Instance.LicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold.Value;

				foreach (var database in Grid.SelectedElements.OfType<LicenceDatabase>().ToArray())
				{
					if (database.LD_OH_WebAccessOrg.IsEmpty)
					{
						var topSuggestion = database.TopOrgSuggestion;
						if (topSuggestion != null && topSuggestion.LDS_TotalScore >= threshold)
						{
							database.LD_OH_WebAccessOrg = topSuggestion.LDS_OH;
							databasesUpdatedCount++;
							continue;
						}
					}
					databasesSkippedCount++;
				}

				Globals.Message.ShowInformation($"{databasesUpdatedCount} Database(s) successfully updated. {databasesSkippedCount} Database(s) skipped.");
			}
			else
			{
				Globals.Message.ShowError("Please select at least one row");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			SearchManager?.Dispose();
			base.Dispose(disposing);
		}
	}
}
