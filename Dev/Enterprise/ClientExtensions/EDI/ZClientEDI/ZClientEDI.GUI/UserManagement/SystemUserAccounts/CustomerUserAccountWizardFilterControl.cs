using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class CustomerUserAccountWizardFilterControl : ZFilterStripControl
	{
		int MaxRowsToLoad => SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
		readonly EdiCustomerUserAccountCollection ediCustomerUserAccountCollection;
		public readonly FilterStripBusinessObject filterStrip;

		public FilteredGridLoader SearchManager => searchManager ?? (searchManager = CreateSearchManager());
		FilteredGridLoader searchManager;

		FilteredGridLoader CreateSearchManager()
			=> new FilteredGridLoader(FilterBusinessObject, new ResultCountMessage(this, MaxRowsToLoad, MaxRowsToLoad), false, ModuleIDs.GlbCompanyCampaignItem, () => new BusinessObjectFactory(), typeof(EdiCustomerUserAccount));

		public CustomerUserAccountWizardFilterControl(EdiCustomerUserAccountCollection ediCustomerUserAccountCollection, FilterStripBusinessObject filterBusinessObject)
			: base(ediCustomerUserAccountCollection, filterBusinessObject)
		{
			InitializeComponent();

			SetupContextMenu();
			PerformSearch += CustomerUserAccountWizardFilterControl_PerformSearch;
			this.ediCustomerUserAccountCollection = ediCustomerUserAccountCollection;
			filterStrip = filterBusinessObject;

			FilteredGrid.MouseDoubleClick += FilteredGrid_MouseDoubleClick;

			ToolStrip.AllowOutsideOfParent();
		}

		void CustomerUserAccountWizardFilterControl_PerformSearch(object sender, EventArgs e)
		{
			var filter = filterStrip.Filter;
			filter.MaximumRows = MaxRowsToLoad;
			var result = SearchManager.PerformSearch(ediCustomerUserAccountCollection.Factory, typeof(EdiCustomerUserAccount), filter);

			SearchManager.PushItemsIntoCollection(ediCustomerUserAccountCollection, result, null);

			if (ediCustomerUserAccountCollection.Any())
			{
				FilteredGrid.CurrentRowIndex = 0;
			}
		}

		protected override void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			base.OnSearchPerformed(showError, didSearch, isManualSearch, form);

			if (ediCustomerUserAccountCollection.Any())
			{
				FilteredGrid.CurrentRowIndex = 0;
			}
		}

		void FilteredGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			//if (FilteredGrid.HitTest(e.X, e.Y).Row > -1)
			//{
			//	var current = FilteredGrid.ListManager.GetCurrent() as LicenceDatabase;
			//	if (current != null)
			//	{
			//		ZControllerFactory.Create(ClientControllerRegistration.edi).ShowEditForm(current);
			//	}
			//}
		}

		const string MergeUserAccountMenuItemText = "Amalgamate User ID's into same Contact/Person";
		const string ClearDerStatusMenuItemText = "Clear 'DER' (Distinct Email Required) Status";
		const string AuditData = "Audit Data";

		void SetupContextMenu()
		{
			Grid.ContextMenu.MenuItems.Add("-");
			Grid.ContextMenu.MenuItems.Add(new ZMenuItem(MergeUserAccountMenuItemText, MergeToSameContact) { Name = MergeUserAccountMenuItemText });
			Grid.ContextMenu.MenuItems.Add(new ZMenuItem(ClearDerStatusMenuItemText, ClearDerStatus) { Name = ClearDerStatusMenuItemText });
			Grid.ContextMenu.MenuItems.Add(new ZMenuItem(AuditData, OpenAuditDataForm) { Name = AuditData });
			Grid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var mergeUserAccountMenuItem = Grid.ContextMenu.MenuItems.FindByName(MergeUserAccountMenuItemText);
			mergeUserAccountMenuItem.Enabled = Grid.SelectedRowCount == 2;

			var clearDerStatusMenuItem = Grid.ContextMenu.MenuItems.FindByName(ClearDerStatusMenuItemText);
			clearDerStatusMenuItem.Enabled = Grid.SelectedRowCount > 0;
		}

		void MergeToSameContact(object sender, EventArgs e)
		{
			if (Grid.CurrentRowIndex >= 0 && Grid.SelectedRowCount == 2)
			{
				var sourceUserAccount = (EdiCustomerUserAccount)Grid.List[Grid.CurrentRowIndex];
				var targetUserAccount = Grid.SelectedElements.OfType<EdiCustomerUserAccount>().Single(x => x != sourceUserAccount);

				if (sourceUserAccount.WebAccessContact == null)
				{
					Globals.Message.ShowError("The User Account is not linked to a valid Contact");
					return;
				}

				if (Globals.Message.Show(
$@"System User ID: '{targetUserAccount.EUA_UserID}' will be moved to Contact '{sourceUserAccount.WebAccessContact.OC_ContactName}'.
If these are two Person records, the Person records will also be merged with pending verification statuses being reset.
Do you wish to continue? ",
					MergeUserAccountMenuItemText, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					CustomerUserAccountRelationshipStatusResolver.MergeToSameContact(sourceUserAccount.PK, targetUserAccount.PK);
					sourceUserAccount.Reload();
					targetUserAccount.Reload();
					Globals.Message.Show("System User moved.");
				}
			}
			else
			{
				Globals.Message.ShowError("Please select two rows");
			}
		}

		void ClearDerStatus(object sender, EventArgs e)
		{
			if (Grid.SelectedRowCount > 0)
			{
				var selectedAccounts = Grid.SelectedElements.OfType<EdiCustomerUserAccount>();

				if (selectedAccounts.Any(x => !x.EUA_ContactRelationshipStatus.EqualsIgnoringCase(ContactRelationshipStatusList.Codes.DistinctEmailRequired)))
				{
					Globals.Message.ShowError($"The User Account(s) selected must have the status 'DER'");
					return;
				}

				CustomerUserAccountRelationshipStatusResolver.ClearRelationshipStatus(selectedAccounts.Select(x => x.PK));
				selectedAccounts.ForEach((x) => x.Reload());
				Globals.Message.Show("Status 'DER' cleared.");
			}
			else
			{
				Globals.Message.ShowError("Please select at least one row");
			}
		}

		void OpenAuditDataForm(object sender, EventArgs e)
		{
			if (Grid.SelectedRowCount > 0)
			{
				var userAccount = (EdiCustomerUserAccount)Grid.List[Grid.CurrentRowIndex];
				ZFormModaliser.ShowDialogAndDispose(new SystemUserAccountsAuditForm(userAccount), this.ParentForm);
			}
			else
			{
				Globals.Message.ShowError("Please select a row");
			}
		}
	}
}
