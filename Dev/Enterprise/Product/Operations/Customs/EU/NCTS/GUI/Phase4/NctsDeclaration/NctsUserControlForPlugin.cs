using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class NctsUserControlForPlugin : ZUserControl, ISupportMultipleResourceStringDataSupporter
	{
		[Obsolete("Just for the Designer")]
		public NctsUserControlForPlugin()
		{
			InitializeComponent();
		}

		public NctsUserControlForPlugin(NctsHeader nctsMovement)
		{
			InitializeComponent();
			this.nctsMovement = nctsMovement;
			ShowMovementTabs();
		}
		protected readonly NctsHeader nctsMovement;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			CreateAndBindChildControls(dataSource, dataMember);
			InitializeValueChangedEvents();
		}

		void CreateAndBindChildControls(object dataSource, string dataMember)
		{
			if (nctsMovement != null)
			{
				if (nctsMovement.IsDepartureTabVisible)
				{
					CreateAndBindDeclarationDetailsUserControl(dataSource, dataMember);
					CreateAndBindGoodsItemsUserControl();
					CreateAndBindSecurityUserControl(dataSource, dataMember);
				}
				if (nctsMovement.IsArrivalTabVisible)
				{
					CreateAndBindArrivalUserControl(dataSource, dataMember);
				}
				if (nctsMovement.IsUnloadingRemarksTabVisible || ShouldCreateUnloadingRemarksUserControl)
				{
					CreateAndBindUnloadingRemarksUserControl(dataSource, dataMember);
				}
			}
			CreateAndBindMessagesUserControl();
			CreateAndBindMiscOptionsUserControlIfSupported(dataSource, dataMember);
			CreateAndBindDeclarationStatusUserControlIfSupported(dataSource, dataMember);
		}

		protected virtual bool ShouldCreateUnloadingRemarksUserControl => false;

		void InitializeValueChangedEvents()
		{
			if (nctsMovement != null)
			{
				if (nctsMovement.IsDepartureMovement)
				{
					nctsMovement.BH_FTZMoveInfo.ValueChanged -= BH_FTZMoveInfo_ValueChanged;
					nctsMovement.BH_FTZMoveInfo.ValueChanged += BH_FTZMoveInfo_ValueChanged;
					nctsMovement.MovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
					nctsMovement.MovementHeader.BM_CustomsStatusInfo.ValueChanged += StatusInfo_ValueChanged;
				}
				nctsMovement.EffectiveMessageStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
				nctsMovement.EffectiveMessageStatusInfo.ValueChanged += StatusInfo_ValueChanged;
			}
		}

		protected virtual DeclarationDetailsTabUserControl GetDeclarationDetailsTabUserControl() => new DeclarationDetailsTabUserControl();

		protected virtual NctsGoodsItemsUserControl GetNctsGoodsItemsUserControl() => new NctsGoodsItemsUserControl();

		protected virtual NctsArrivalUserControl GetNctsArrivalUserControl() => new NctsArrivalUserControl();

		protected virtual UnloadingRemarksUserControl GetUnloadingRemarksUserControl() => new UnloadingRemarksUserControl();

		protected virtual MessagesTabUserControl GetMessagesUserControl() => new MessagesTabUserControl();

		protected virtual SecurityTabUserControl GetSecurityUserControl() => new SecurityTabUserControl();

		protected virtual MiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

		protected virtual DeclarationStatusTabUserControl GetDeclarationStatusUserControl() => new DeclarationStatusTabUserControl();

		protected virtual ZBool SupportsMiscOptionsTabPage => ZBool.False;

		protected virtual ZBool SupportsStatusTabPage => ZBool.False;

		protected override void Dispose(bool disposing)
		{
			if (nctsMovement != null)
			{
				if (nctsMovement.IsDepartureMovement)
				{
					nctsMovement.BH_FTZMoveInfo.ValueChanged -= BH_FTZMoveInfo_ValueChanged;
					nctsMovement.MovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
				}
				nctsMovement.EffectiveMessageStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
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
			if (nctsMovement?.IsDepartureTabVisible ?? false)
			{
				ShowHideSecurityTab();
				ToggleDepartureDeclarationRelatedTabsEditableState();
			}
			else
			{
				DepartureDeclarationTabPage.TabVisible = false;
				GoodsItemsTabPage.TabVisible = false;
				SecurityTabPage.TabVisible = false;
			}
		}

		void ToggleDepartureDeclarationRelatedTabsEditableState()
		{
			var isDepartureTabEditable = !(nctsMovement?.IsDepartureTabReadOnly ?? true);
			DepartureDeclarationTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			GoodsItemsTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable, new string[] { nameof(FeesGridUserControl.FeesGrid) });
			SecurityTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			MiscOptionsTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
		}

		void BH_FTZMoveInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowHideSecurityTab();
		}

		void ShowHideSecurityTab()
		{
			SecurityTabPage.TabVisible = nctsMovement?.BH_FTZMove ?? false;
		}

		void ShowArrivalTab()
		{
			if (nctsMovement?.IsArrivalTabVisible ?? false)
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

		void ToggleArrivalNotificationTabPageEditableState()
		{
			ArrivalNotificationTabPage.UpdateEditableIncludingChildren(isEditable: !(nctsMovement?.IsArrivalTabReadOnly ?? true), ControlsAllowedToRemainEditableAfterSending);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The underlying ZArchitecture method, UpdateEditableIncludingChildren, uses an array where null values indicate absence.")]
		protected virtual string[] ControlsAllowedToRemainEditableAfterSending => null;

		public void ShowUnloadingTab(bool changeSelectedTab = true)
		{
			if (nctsMovement?.IsUnloadingRemarksTabVisible ?? false)
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
			UnloadingRemarksTabPage.UpdateEditableIncludingChildren(isEditable: !(nctsMovement?.IsUnloadingRemarksTabReadOnly ?? true));
		}

		void CreateAndBindDeclarationDetailsUserControl(object dataSource, string dataMember)
		{
			if (DeclarationDetailsTabUserControl == null)
			{
				DeclarationDetailsTabUserControl = GetDeclarationDetailsTabUserControl();
				DeclarationDetailsTabUserControl.AllowDrop = true;
				DeclarationDetailsTabUserControl.AutoSize = true;
				DeclarationDetailsTabUserControl.Dock = DockStyle.Fill;
				DeclarationDetailsTabUserControl.Name = "DeclarationDetailsTabUserControl";
				DepartureDeclarationTabPage.Controls.Add(DeclarationDetailsTabUserControl);
			}
			DeclarationDetailsTabUserControl.SetDataBinding(dataSource, dataMember);
		}

		void CreateAndBindGoodsItemsUserControl()
		{
			if (NctsGoodsItemsUserControl == null)
			{
				NctsGoodsItemsUserControl = GetNctsGoodsItemsUserControl();
				GoodsItemsTabPage.Controls.Add(NctsGoodsItemsUserControl);
				NctsGoodsItemsUserControl.AllowDrop = true;
				NctsGoodsItemsUserControl.Dock = DockStyle.Fill;
				NctsGoodsItemsUserControl.Name = "NctsGoodsItemsUserControl";
			}
			BindingSource.SetBindingMember(NctsGoodsItemsUserControl, "MovementHeader.GoodsItems");
		}

		void CreateAndBindMessagesUserControl()
		{
			if (MessagesUserControl == null)
			{
				MessagesUserControl = GetMessagesUserControl();
				MessagesTabPage.Controls.Add(MessagesUserControl);
				MessagesUserControl.AllowDrop = true;
				MessagesUserControl.BackColor = SystemColors.Control;
				MessagesUserControl.Dock = DockStyle.Fill;
				MessagesUserControl.Name = "MessagesUserControl";
			}
			BindingSource.SetBindingMember(MessagesUserControl, nameof(NctsHeader.Messages));
		}

		void CreateAndBindSecurityUserControl(object dataSource, string dataMember)
		{
			if (SecurityTabUserControl == null)
			{
				SecurityTabUserControl = GetSecurityUserControl();
				SecurityTabPage.Controls.Add(SecurityTabUserControl);
				SecurityTabUserControl.AllowDrop = true;
				SecurityTabUserControl.Dock = DockStyle.Fill;
				SecurityTabUserControl.Name = "SecurityTabUserControl";
			}
			SecurityTabUserControl.SetDataBinding(dataSource, dataMember);
		}

		void CreateAndBindArrivalUserControl(object dataSource, string dataMember)
		{
			if (NctsArrivalUserControl == null)
			{
				NctsArrivalUserControl = GetNctsArrivalUserControl();
				ArrivalNotificationTabPage.Controls.Add(NctsArrivalUserControl);
				NctsArrivalUserControl.AllowDrop = true;
				NctsArrivalUserControl.Dock = DockStyle.Fill;
				NctsArrivalUserControl.Name = "NctsArrivalUserControl";
			}

			NctsArrivalUserControl.SetDataBinding(dataSource, dataMember);
		}

		void CreateAndBindUnloadingRemarksUserControl(object dataSource, string dataMember)
		{
			if (UnloadingRemarksUserControl == null)
			{
				UnloadingRemarksUserControl = GetUnloadingRemarksUserControl();
				UnloadingRemarksTabPage.Controls.Add(UnloadingRemarksUserControl);
				UnloadingRemarksUserControl.AllowDrop = true;
				UnloadingRemarksUserControl.Dock = DockStyle.Fill;
				UnloadingRemarksUserControl.Name = "UnloadingRemarksUserControl";
			}
			UnloadingRemarksUserControl.SetDataBinding(dataSource, dataMember);
		}

		void CreateAndBindMiscOptionsUserControlIfSupported(object dataSource, string dataMember)
		{
			if (SupportsMiscOptionsTabPage)
			{
				if (MiscOptionsUserControl == null)
				{
					MiscOptionsUserControl = GetMiscOptionsUserControl();
					MiscOptionsTabPage.Controls.Add(MiscOptionsUserControl);
					MiscOptionsUserControl.AllowDrop = true;
					MiscOptionsUserControl.Dock = DockStyle.Fill;
					MiscOptionsUserControl.Name = "MiscOptionsUserControl";
				}
				MiscOptionsUserControl.SetDataBinding(dataSource, dataMember);
			}
			MiscOptionsTabPage.TabVisible = SupportsMiscOptionsTabPage;
		}

		void CreateAndBindDeclarationStatusUserControlIfSupported(object dataSource, string dataMember)
		{
			if (SupportsStatusTabPage)
			{
				if (DeclarationStatusUserControl == null)
				{
					DeclarationStatusUserControl = GetDeclarationStatusUserControl();
					StatusTabPage.Controls.Add(DeclarationStatusUserControl);
					DeclarationStatusUserControl.AllowDrop = true;
					DeclarationStatusUserControl.Dock = DockStyle.Fill;
					DeclarationStatusUserControl.Name = "DeclarationStatusUserControl";
				}
				DeclarationStatusUserControl.SetDataBinding(dataSource, dataMember);
			}
			StatusTabPage.TabVisible = SupportsStatusTabPage;
		}

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => nctsMovement;
	}
}
