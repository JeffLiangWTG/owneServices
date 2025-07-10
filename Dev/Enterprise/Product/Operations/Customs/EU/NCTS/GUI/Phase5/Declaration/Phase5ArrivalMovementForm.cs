using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5ArrivalMovementForm : ZTemplateForm, IDevToolMessageBuilderMappingPathConfigurator
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public Phase5ArrivalMovementForm()
		{
			InitializeComponent();
		}

		public Phase5ArrivalMovementForm(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			InitializeComponent();
			InitializeTabsLazyCreate();
			InitializeValueChangesEvents();
			AddMessagingMenuItem();
			HookEvents();
			ShowIncidentsTab();
			WorkflowTabPage.Initialize(nctsHeader);
			PlugIns.AddJobInvoicing(nctsHeader.InvoicingSupporter);
			MainTabControl.AddAdditionalTabs((ZBindingSource)BindingSource, LayoutProvider.AdditionalArrivalTabPages);
			MainTabControl.ReorderTabs(LayoutProvider.ReorderArrivalTabPagesNames);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ShowUnloadingTab();
			if (!DesignModeFinder.IsDesigning)
			{
				new TabConfigurationManager(MainMenu, TopLevelTabControl).Enabled = true;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatusInfo.ValueChanged -= ShowUnloadingTab;
				nctsHeader.BH_ExportFlagInfo.ValueChanged -= ShowIncidentsTab;
				UnhookEvents();
			}
			base.Dispose(disposing);
		}

		void ShowUnloadingTab(object sender = null, EventArgs e = null)
		{
			if (nctsHeader.IsUnloadingRemarksTabVisible)
			{
				UnloadingRemarksTabPage.TabVisible = true;
				MainTabControl.SelectedTab = UnloadingRemarksTabPage;
				ToggleUnloadingRemarksTabPageEditableState();
			}
			else
			{
				UnloadingRemarksTabPage.TabVisible = false;
			}
		}

		void ToggleUnloadingRemarksTabPageEditableState()
		{
			UnloadingRemarksTabPage.UpdateEditableIncludingChildren(isEditable: !nctsHeader.IsUnloadingRemarksTabReadOnly, ArrivalUnloadingRemarksTabPageControlsAllowedToRemainEditableAfterSending);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The underlying ZArchitecture method, UpdateEditableIncludingChildren, uses an array where null values indicate absence.")]
		protected virtual string[] ArrivalUnloadingRemarksTabPageControlsAllowedToRemainEditableAfterSending => null;

		void ShowIncidentsTab(object sender = null, EventArgs e = null) => IncidentsTabPage.TabVisible = nctsHeader.IsArrivalEventAvailable;

		void InitializeValueChangesEvents()
		{
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatusInfo.ValueChanged -= ShowUnloadingTab;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatusInfo.ValueChanged += ShowUnloadingTab;
			nctsHeader.BH_ExportFlagInfo.ValueChanged -= ShowIncidentsTab;
			nctsHeader.BH_ExportFlagInfo.ValueChanged += ShowIncidentsTab;
		}

		void AddMessagingMenuItem()
		{
			var messagingMenuItem = new Phase5NctsMessagingMenuItem();
			messagingMenuItem.NctsHeader = nctsHeader;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenuItem);
		}

		readonly NctsHeader nctsHeader;

		public override string FormCaption => nctsHeader.HumanReadableName.ToString();

		protected new NctsHeader DataSource => base.DataSource as NctsHeader;

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;

		void InitializeTabsLazyCreate()
		{
			MessagesTabPage.RunWhenBindingOrFirstShown((s, args) => MessagesTabDynamicUserControl.UserControlType = GetMessagesUserControlType());
		}

		protected virtual Type GetMessagesUserControlType() => typeof(MessagesTabUserControl);

		void HookEvents()
		{
			nctsHeader.RemoveIncidentsWhenIncidentFlagChanged += ShowRemoveIncidentsDialogBox;
		}

		void UnhookEvents()
		{
			nctsHeader.RemoveIncidentsWhenIncidentFlagChanged -= ShowRemoveIncidentsDialogBox;
		}

		void ShowRemoveIncidentsDialogBox(object sender, CancelEventArgs e)
		{
			if (nctsHeader.EnRouteIncidents.Count > 0)
			{
				var result = Globals.Message.Show(Res.GetString("982299E4-B496-4BBA-9A09-A46E6FE670EC", "Incident details will be deleted. Are you sure you want to continue?"), Res.GetString("CA9E4A17-63C1-44A8-92BF-2031EE2C18D0", "Delete Incidents?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				e.Cancel = result == DialogResult.No;
			}
		}

		bool IDevToolMessageBuilderMappingPathConfigurator.CanDisplayMessageBuilderMappingPath => true;
	}
}
