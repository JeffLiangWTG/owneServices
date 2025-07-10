using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class NctsMovementForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public NctsMovementForm()
		{
		}

		public NctsMovementForm(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
			this.nctsHeader = nctsHeader;
			InitializeComponent();
			InitializeTabsLazyCreate();
			InitializeValueChangesEvents();
			AddMessagingMenu();
			ShowMovementTabs();
			WorkflowTabPage.Initialize(nctsHeader);
			AddPlugins();
			ToggleMiscOptionsTabPageVisibility();
			AddCreateArrivalDataFromDepartureMenuOption();
			ToggleStatusTabPageVisibility();
		}

		public void ShowUnloadingTab(bool changeSelectedTab = true)
		{
			if (nctsHeader.IsUnloadingRemarksTabVisible)
			{
				UnloadingRemarksTabPage.TabVisible = true;
				ToggleUnloadingRemarksTabPageEditableState();
				if (changeSelectedTab)
				{
					MainTabControl.SelectedTab = UnloadingRemarksTabPage;
				}
			}
			else
			{
				UnloadingRemarksTabPage.TabVisible = false;
			}
		}

		void ToggleUnloadingRemarksTabPageEditableState()
		{
			UnloadingRemarksTabPage.UpdateEditableIncludingChildren(isEditable: !nctsHeader.IsUnloadingRemarksTabReadOnly);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (nctsHeader.IsArrivalMovement)
			{
				nctsHeader.ArrivalMovementHeader.SynchronizeArrivalAndUnloadingGoodsItems();
			}

			return result;
		}

		public override string FormCaption => nctsHeader != null ? nctsHeader.HumanReadableName.ToString() : base.FormCaption;

		protected virtual void ToggleArrivalNotificationTabPageEditableState()
		{
			ArrivalNotificationTabPage.UpdateEditableIncludingChildren(isEditable: !nctsHeader.IsArrivalTabReadOnly, ControlsAllowedToRemainEditableAfterSending);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The underlying ZArchitecture method, UpdateEditableIncludingChildren, uses an array where null values indicate absence.")]
		protected virtual string[] ControlsAllowedToRemainEditableAfterSending => null;

		protected override void Dispose(bool disposing)
		{
			if (nctsHeader.IsDepartureMovement)
			{
				nctsHeader.BH_FTZMoveInfo.ValueChanged -= IsSafetyAndSecurityInfo_ValueChanged;
				nctsHeader.MovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
			}
			nctsHeader.BH_HeaderTypeInfo.ValueChanged -= BH_HeaderTypeInfo_ValueChanged;
			nctsHeader.EffectiveMessageStatusInfo.ValueChanged -= MessageStatusInfo_ValueChanged;
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected virtual NctsMessagingMenu GetNctsMessageMenu() => new NctsMessagingMenu(this);

		protected override bool SupportsEDocs => true;

		protected virtual Type GetDeclarationDetailsUserControlType() => typeof(DeclarationDetailsTabUserControl);

		protected virtual Type GetSecurityUserControlType() => typeof(SecurityTabUserControl);

		protected virtual Type GetGoodsItemUserControlType() => typeof(NctsGoodsItemsUserControl);

		protected virtual Type GetArrivalNotificationUserControlType() => typeof(NctsArrivalUserControl);

		protected virtual Type GetUnloadingRemarksUserControlType() => typeof(UnloadingRemarksUserControl);

		protected virtual Type GetMiscOptionsUserControlType() => typeof(MiscOptionsUserControl);

		protected virtual Type GetDeclarationStatusUserControlType() => typeof(DeclarationStatusTabUserControl);

		protected virtual Type GetMessagesUserControlType() => typeof(MessagesTabUserControl);

		protected virtual ZBool SupportsMiscOptionsTabPage => false;

		protected virtual ZBool SupportsStatusTabPage => false;

		void AddPlugins()
		{
			PlugIns.AddJobInvoicing(nctsHeader.InvoicingSupporter);
			AddDocDataPlugInIfSupported();
		}

		void AddDocDataPlugInIfSupported()
		{
			var nctsConfiguration = NctsConfiguration.GetConfiguration(nctsHeader.Factory, nctsHeader.BrokerageCountryCode);
			if (nctsConfiguration.DocDataPlugInSupport(nctsHeader))
			{
				PlugIns.Add(ZArchitecture.Modules.ControllerIDs.DocDataPlugIn);
				PlugIns.Add(ZArchitecture.Modules.ControllerIDs.DocumentVisualizer);
			}
		}

		void MessageStatusInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowMovementTabs();
		}

		void StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowMovementTabs();
		}

		public void ShowMovementTabs()
		{
			ShowDepartureDeclarationTabs();
			ShowArrivalTab();
			ShowUnloadingTab();
		}

		void ShowDepartureDeclarationTabs()
		{
			if (nctsHeader.IsDepartureTabVisible)
			{
				MainTabPage.TabVisible = true;
				GoodsItemsTabPage.TabVisible = true;

				ShowHideSecurityTab();
				ToggleDepartureDeclarationRelatedTabsEditableState();
			}
			else
			{
				MainTabPage.TabVisible = false;
				GoodsItemsTabPage.TabVisible = false;
				SecurityTabPage.TabVisible = false;
			}
		}

		void BH_HeaderTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowMovementTabs();
			AddCreateArrivalDataFromDepartureMenuOption();
		}

		void AddCreateArrivalDataFromDepartureMenuOption()
		{
			var menuItems = ActionsMenuItem.MenuItems;
			if (nctsHeader.IsArrivalMovement)
			{
				if (menuItems.Find((NoResString)"Create Arrival Data from Departure", true).FirstOrDefault() == null)
				{
					menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("BE9415C5-9159-46E9-B9A5-E9712F18D623", "Create Arrival Data from Departure"), CreateArrivalDataFromDeparture_Click));
				}
			}
			else
			{
				var menuItem = menuItems.Find((NoResString)"Create Arrival Data from Departure", true).FirstOrDefault();
				if (menuItem != null)
				{
					menuItems.Remove(menuItem);
				}
			}
		}

		void CreateArrivalDataFromDeparture_Click(object sender, EventArgs e)
		{
			var userResult = DialogResult.OK;
			var (matchingResult, departureHeaderFound) = nctsHeader.FindRelevantDepartureRecordForCombinedDepartureAndArrival();

			if (matchingResult == NctsHeader.DepartureRecordFindResult.FoundByMatchingMrn)
			{
				userResult = Globals.Message.ShowConfirmation(
					ResString.GetMultilingualString("C7972EDE-FDA4-41D2-9D6D-E0BA05100F6E", "This arrival does not have an attached departure, but {0} was found with a matching MRN. Use this departure’s data for populating Arrival Goods Items?", departureHeaderFound.BH_JobReference),
					ResString.GetMultilingualString("5BC5549F-A789-4A86-B6D9-A6C23D009552", "Use this departure MRN"),
					ResString.GetMultilingualString("73285AED-F68D-4FF7-BB67-935F4CA3CFF1", "yes"),
					MessageBoxIcon.Question);
			}
			else if (matchingResult == NctsHeader.DepartureRecordFindResult.NothingFound || matchingResult == NctsHeader.DepartureRecordFindResult.Unknown)
			{
				userResult = DialogResult.Cancel;
				Globals.Message.ShowInformation(ResString.GetMultilingualString("CDA214FC-5602-4CC1-81DF-8B7BC6DB22CC", "No matching departure for MRN {0} found.", nctsHeader.MovementReferenceNumber));
			}

			if (userResult == DialogResult.OK)
			{
				nctsHeader.ArrivalMovementHeader.PopulateArrivalGoodsItemsFromDeparture(departureHeaderFound.MovementHeader);
				PopulateCustomsOfficeFromDeparture(departureHeaderFound.MovementHeader, nctsHeader.ArrivalMovementHeader);
			}
		}

		void PopulateCustomsOfficeFromDeparture(NctsDepartureMovementHeader movementHeader, NctsArrivalMovementHeader arrivalMovementHeader)
		{
			if (!nctsHeader.DestinationCustomsOfficeCodeForDeparture.IsEmpty)
			{
				var officeDSA = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
				if (officeDSA == null)
				{
					officeDSA = nctsHeader.CustomsOffices.AddNew();
					officeDSA.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
				}
				officeDSA.CY_Data = nctsHeader.DestinationCustomsOfficeCodeForDeparture;
			}
		}

		void ShowArrivalTab()
		{
			if (nctsHeader.IsArrivalTabVisible)
			{
				ArrivalNotificationTabPage.TabVisible = true;
				ToggleArrivalNotificationTabPageEditableState();
				MainTabControl.SelectedTab = ArrivalNotificationTabPage;
			}
			else
			{
				ArrivalNotificationTabPage.TabVisible = false;
			}
		}

		void ToggleDepartureDeclarationRelatedTabsEditableState()
		{
			var isDepartureTabEditable = !nctsHeader.IsDepartureTabReadOnly;
			MainTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			SecurityTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			MiscOptionsTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			GoodsItemsTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable, new string[] { nameof(FeesGridUserControl.FeesGrid) });
		}

		void ToggleMiscOptionsTabPageVisibility()
		{
			MiscOptionsTabPage.TabVisible = SupportsMiscOptionsTabPage;
		}

		void IsSafetyAndSecurityInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowHideSecurityTab();
		}

		void ShowHideSecurityTab()
		{
			var isSafetyAndSecurity = nctsHeader.BH_FTZMove;
			SecurityTabPage.TabVisible = isSafetyAndSecurity;
			SecurityTabPage.CheckForNotifications = isSafetyAndSecurity;
		}

		void AddMessagingMenu()
		{
			var messagingMenutItem = GetNctsMessageMenu();
			messagingMenutItem.NctsHeader = nctsHeader;
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenutItem);
		}

		void InitializeTabsLazyCreate()
		{
			MainTabPage.RunWhenBindingOrFirstShown((s, args) => DeclarationDetailsTabDynamicUserControl.UserControlType = GetDeclarationDetailsUserControlType());
			SecurityTabPage.RunWhenBindingOrFirstShown((s, args) => SecurityTabDynamicUserControl.UserControlType = GetSecurityUserControlType());
			GoodsItemsTabPage.RunWhenBindingOrFirstShown((s, args) => GoodsItemsTabDynamicUserControl.UserControlType = GetGoodsItemUserControlType());
			ArrivalNotificationTabPage.RunWhenBindingOrFirstShown((s, args) => ArrivalTabDynamicUserControl.UserControlType = GetArrivalNotificationUserControlType());
			UnloadingRemarksTabPage.RunWhenBindingOrFirstShown((s, args) => UnloadingRemarksDynamicUserControl.UserControlType = GetUnloadingRemarksUserControlType());
			MiscOptionsTabPage.RunWhenBindingOrFirstShown((s, args) => MiscOptionsTabDynamicUserControl.UserControlType = GetMiscOptionsUserControlType());
			StatusTabPage.RunWhenBindingOrFirstShown((s, args) => DeclarationStatusTabDynamicUserControl.UserControlType = GetDeclarationStatusUserControlType());
			MessagesTabPage.RunWhenBindingOrFirstShown((s, args) => MessagesTabDynamicUserControl.UserControlType = GetMessagesUserControlType());
		}

		void ToggleStatusTabPageVisibility()
		{
			StatusTabPage.TabVisible = SupportsStatusTabPage;
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DeclarationDetailsTabDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.MainTabPage.SuspendLayout();
			this.DeclarationDetailsTabDynamicUserControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.DeclarationDetailsTabDynamicUserControl);
			// 
			// DeclarationDetailsTabDynamicUserControl
			// 
			this.DeclarationDetailsTabDynamicUserControl.AllowDrop = true;
			this.DeclarationDetailsTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationDetailsTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationDetailsTabDynamicUserControl.Name = "DeclarationDetailsTabDynamicUserControl";
			this.DeclarationDetailsTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 693, true);
			this.DeclarationDetailsTabDynamicUserControl.TabIndex = 1;
			this.DeclarationDetailsTabDynamicUserControl.UserControlType = typeof(DeclarationDetailsTabUserControl);
			this.MainTabPage.PerformLayout();
			this.DeclarationDetailsTabDynamicUserControl.ResumeLayout(true);
			this.DeclarationDetailsTabDynamicUserControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		void GoodsItemsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.GoodsItemsTabDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.GoodsItemsTabPage.SuspendLayout();
			this.GoodsItemsTabDynamicUserControl.SuspendLayout();
			this.GoodsItemsTabPage.Controls.Add(this.GoodsItemsTabDynamicUserControl);
			// 
			// GoodsItemsTabDynamicUserControl
			// 
			this.GoodsItemsTabDynamicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsItemsTabDynamicUserControl, "MovementHeader.GoodsItems");
			this.GoodsItemsTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemsTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemsTabDynamicUserControl.Name = "GoodsItemsTabDynamicUserControl";
			this.GoodsItemsTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 736, true);
			this.GoodsItemsTabDynamicUserControl.TabIndex = 0;
			this.GoodsItemsTabDynamicUserControl.UserControlType = typeof(NctsGoodsItemsUserControl);
			this.GoodsItemsTabPage.PerformLayout();
			this.GoodsItemsTabDynamicUserControl.ResumeLayout(true);
			this.GoodsItemsTabDynamicUserControl.PerformLayout();
			this.GoodsItemsTabPage.ResumeLayout(true);
		}

		void MessagesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MessagesTabDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesTabDynamicUserControl.SuspendLayout();
			this.MessagesTabPage.Controls.Add(this.MessagesTabDynamicUserControl);
			// 
			// MessagesTabDynamicUserControl
			// 
			this.MessagesTabDynamicUserControl.AllowDrop = true;
			this.MessagesTabDynamicUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MessagesTabDynamicUserControl, "Messages");
			this.MessagesTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesTabDynamicUserControl.Name = "MessagesTabDynamicUserControl";
			this.MessagesTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 544, true);
			this.MessagesTabDynamicUserControl.TabIndex = 0;
			this.MessagesTabDynamicUserControl.UserControlType = typeof(MessagesTabUserControl);
			this.MessagesTabPage.PerformLayout();
			this.MessagesTabDynamicUserControl.ResumeLayout(true);
			this.MessagesTabDynamicUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(true);
		}

		void SecurityTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.SecurityTabDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.SecurityTabPage.SuspendLayout();
			this.SecurityTabDynamicUserControl.SuspendLayout();
			this.SecurityTabPage.Controls.Add(this.SecurityTabDynamicUserControl);
			// 
			// SecurityTabDynamicUserControl
			// 
			this.SecurityTabDynamicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityTabDynamicUserControl, ".");
			this.SecurityTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecurityTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecurityTabDynamicUserControl.Name = "SecurityTabDynamicUserControl";
			this.SecurityTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
			this.SecurityTabDynamicUserControl.TabIndex = 0;
			this.SecurityTabDynamicUserControl.UserControlType = typeof(SecurityTabUserControl);
			this.SecurityTabPage.PerformLayout();
			this.SecurityTabDynamicUserControl.ResumeLayout(true);
			this.SecurityTabDynamicUserControl.PerformLayout();
			this.SecurityTabPage.ResumeLayout(true);
		}

		void ArrivalNotificationTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ArrivalTabDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.ArrivalNotificationTabPage.SuspendLayout();
			this.ArrivalTabDynamicUserControl.SuspendLayout();
			this.ArrivalNotificationTabPage.Controls.Add(this.ArrivalTabDynamicUserControl);
			// 
			// ArrivalTabDynamicUserControl
			// 
			this.ArrivalTabDynamicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalTabDynamicUserControl, ".");
			this.ArrivalTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ArrivalTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ArrivalTabDynamicUserControl.Name = "ArrivalTabDynamicUserControl";
			this.ArrivalTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 706, true);
			this.ArrivalTabDynamicUserControl.TabIndex = 0;
			this.ArrivalTabDynamicUserControl.UserControlType = typeof(NctsArrivalUserControl);
			this.ArrivalNotificationTabPage.PerformLayout();
			this.ArrivalTabDynamicUserControl.ResumeLayout(true);
			this.ArrivalTabDynamicUserControl.PerformLayout();
			this.ArrivalNotificationTabPage.ResumeLayout(true);
		}

		void UnloadingRemarksTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.UnloadingRemarksDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.UnloadingRemarksTabPage.SuspendLayout();
			this.UnloadingRemarksDynamicUserControl.SuspendLayout();
			this.UnloadingRemarksTabPage.Controls.Add(this.UnloadingRemarksDynamicUserControl);
			// 
			// UnloadingRemarksDynamicUserControl
			// 
			this.UnloadingRemarksDynamicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingRemarksDynamicUserControl, ".");
			this.UnloadingRemarksDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingRemarksDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.UnloadingRemarksDynamicUserControl.Name = "UnloadingRemarksDynamicUserControl";
			this.UnloadingRemarksDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 706, true);
			this.UnloadingRemarksDynamicUserControl.TabIndex = 0;
			this.UnloadingRemarksDynamicUserControl.UserControlType = typeof(UnloadingRemarksUserControl);
			this.UnloadingRemarksTabPage.PerformLayout();
			this.UnloadingRemarksDynamicUserControl.ResumeLayout(true);
			this.UnloadingRemarksDynamicUserControl.PerformLayout();
			this.UnloadingRemarksTabPage.ResumeLayout(true);
		}

		void MiscOptionsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MiscOptionsTabDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.MiscOptionsTabPage.SuspendLayout();
			this.MiscOptionsTabDynamicUserControl.SuspendLayout();
			this.MiscOptionsTabPage.Controls.Add(this.MiscOptionsTabDynamicUserControl);
			// 
			// MiscOptionsTabDynamicUserControl
			// 
			this.MiscOptionsTabDynamicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MiscOptionsTabDynamicUserControl, ".");
			this.MiscOptionsTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiscOptionsTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MiscOptionsTabDynamicUserControl.Name = "MiscOptionsTabDynamicUserControl";
			this.MiscOptionsTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 544, true);
			this.MiscOptionsTabDynamicUserControl.TabIndex = 0;
			this.MiscOptionsTabDynamicUserControl.UserControlType = typeof(MiscOptionsUserControl);
			this.MiscOptionsTabPage.PerformLayout();
			this.MiscOptionsTabDynamicUserControl.ResumeLayout(true);
			this.MiscOptionsTabDynamicUserControl.PerformLayout();
			this.MiscOptionsTabPage.ResumeLayout(true);
		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
		}

		void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
		}

		void StatusTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DeclarationStatusTabDynamicUserControl = new ZDynamicControlCreationUserControl();
			this.StatusTabPage.SuspendLayout();
			this.DeclarationStatusTabDynamicUserControl.SuspendLayout();
			this.StatusTabPage.Controls.Add(this.DeclarationStatusTabDynamicUserControl);
			// 
			// DeclarationStatusTabDynamicUserControl
			// 
			this.DeclarationStatusTabDynamicUserControl.AllowDrop = true;
			this.DeclarationStatusTabDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationStatusTabDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DeclarationStatusTabDynamicUserControl.Name = "DeclarationStatusTabDynamicUserControl";
			this.DeclarationStatusTabDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 706, true);
			this.DeclarationStatusTabDynamicUserControl.TabIndex = 0;
			this.DeclarationStatusTabDynamicUserControl.UserControlType = typeof(DeclarationStatusTabUserControl);
			this.StatusTabPage.ResumeLayout(false);
			this.StatusTabPage.PerformLayout();
			this.DeclarationStatusTabDynamicUserControl.ResumeLayout(false);
			this.DeclarationStatusTabDynamicUserControl.PerformLayout();
		}

		void InitializeValueChangesEvents()
		{
			if (nctsHeader.IsDepartureMovement)
			{
				nctsHeader.BH_FTZMoveInfo.ValueChanged -= IsSafetyAndSecurityInfo_ValueChanged;
				nctsHeader.BH_FTZMoveInfo.ValueChanged += IsSafetyAndSecurityInfo_ValueChanged;
				nctsHeader.MovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
				nctsHeader.MovementHeader.BM_CustomsStatusInfo.ValueChanged += StatusInfo_ValueChanged;
			}
			nctsHeader.BH_HeaderTypeInfo.ValueChanged -= BH_HeaderTypeInfo_ValueChanged;
			nctsHeader.BH_HeaderTypeInfo.ValueChanged += BH_HeaderTypeInfo_ValueChanged;
			nctsHeader.EffectiveMessageStatusInfo.ValueChanged -= MessageStatusInfo_ValueChanged;
			nctsHeader.EffectiveMessageStatusInfo.ValueChanged += MessageStatusInfo_ValueChanged;
		}
		protected NctsHeader nctsHeader;
	}
}
