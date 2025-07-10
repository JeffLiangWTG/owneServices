using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class OrganisationWizardFilterControl : ZFilterStripControl
	{
		int MaxRowsToLoad => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
		readonly LicenceDatabaseRegistrationWizard wizard;
		readonly FilterStripBusinessObject filterStrip;
		public event EventHandler CurrentItemChanged;

		FilteredGridLoader SearchManager => searchManager ?? (searchManager = CreateSearchManager());
		FilteredGridLoader searchManager;

		FilteredGridLoader CreateSearchManager()
			=> new FilteredGridLoader(FilterBusinessObject, new ResultCountMessage(this, MaxRowsToLoad, MaxRowsToLoad), false, ModuleIDs.GlbCompanyCampaignItem, () => new BusinessObjectFactory(), typeof(EDIOrgHeader));

		public OrganisationWizardFilterControl(LicenceDatabaseRegistrationWizard licenceDatabaseRegistrationWizard, FilterStripBusinessObject filterBusinessObject)
			: base(licenceDatabaseRegistrationWizard.OrgHeaderCollection, filterBusinessObject)
		{
			InitializeComponent();

			wizard = licenceDatabaseRegistrationWizard;
			filterStrip = filterBusinessObject;

			PerformSearch += OrganisationWizardFilterControl_PerformSearch;

			FilteredGrid.AfterBind += FilteredGrid_AfterBind;
			FilteredGrid.MouseDoubleClick += FilteredGrid_MouseDoubleClick;
		}

		public void SelectFirstRow()
		{
			if (wizard.OrgHeaderCollection.Any())
			{
				FilteredGrid.UnSelectAll();

				if (FilteredGrid.CurrentRowIndex == 0) // Fire CurrentItemChanged when the row hasn't change
				{
					CurrentItemChanged?.Invoke(FilteredGrid.ListManager.GetCurrent(), null);
				}

				FilteredGrid.CurrentRowIndex = 0;
			}
		}

		void OrganisationWizardFilterControl_PerformSearch(object sender, EventArgs e)
		{
			var collection = wizard.OrgHeaderCollection;
			var factory = SearchManager.GetNewFactory();
			var result = SearchManager.PerformSearch(factory, collection.TypeOfElements, filterStrip.Filter);
			if (result.Type == PerformSearchResultType.Success)
			{
				SearchManager.PushItemsIntoCollection(collection, result, null);
			}

			SelectFirstRow();
		}

		void FilteredGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (FilteredGrid.HitTest(e.X, e.Y).Row > -1)
			{
				var current = FilteredGrid.ListManager.GetCurrent() as EDIOrgHeader;
				if (current != null)
				{
					ZControllerFactory.Create(ClientControllerRegistration.Organisations).ShowEditForm(current);
				}
			}
		}

		void FilteredGrid_AfterBind(object sender, EventArgs e)
		{
			if (FilteredGrid.ListManager != null)
			{
				FilteredGrid.ListManager.CurrentItemChanged -= ListManager_CurrentItemChanged;
				FilteredGrid.ListManager.CurrentItemChanged += ListManager_CurrentItemChanged;
			}
		}

		void ListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			CurrentItemChanged?.Invoke(FilteredGrid.ListManager.GetCurrent() as EDIOrgHeader, e);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (FilteredGrid.ListManager != null)
			{
				FilteredGrid.ListManager.CurrentItemChanged -= ListManager_CurrentItemChanged;
			}
			base.OnHandleDestroyed(e);
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
