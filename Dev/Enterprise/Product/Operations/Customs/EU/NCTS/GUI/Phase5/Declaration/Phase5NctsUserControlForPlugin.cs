using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5NctsUserControlForPlugin : ZUserControl, ISupportMultipleResourceStringDataSupporter
	{
		[Obsolete("Just for the Designer")]
		public Phase5NctsUserControlForPlugin()
		{
			InitializeComponent();
		}

		public Phase5NctsUserControlForPlugin(NctsHeader nctsMovement)
		{
			InitializeComponent();
			this.nctsMovement = nctsMovement;
			HookEvents();
			ShowMovementTabs();
		}
		public NctsHeader nctsMovement;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			CreateAndBindChildControls(dataSource, dataMember);
			InitializeValueChangedEvents();
			nctsMovement.SynchronizeMonetaryValue();
		}

		void CreateAndBindChildControls(object dataSource, string dataMember)
		{
			if (nctsMovement.IsDepartureTabVisible)
			{
				CreateAndBindDeclarationDetailsUserControl(dataSource, dataMember);
				CreateAndBindServicesUserControl(dataSource, dataMember);
				CreateAndBindTransportAndPackagingUserControl(dataSource, dataMember);
				CreateAndBindHouseConsignmentsUserControl(dataSource, dataMember);
				CreateAndBindMiscOptionsUserControlIfSupported(dataSource, dataMember);
			}
			if (nctsMovement.IsArrivalTabVisible)
			{
				CreateAndBindArrivalUserControl(dataSource, dataMember);
				CreateAndBindIncidentsUserControl(dataSource, dataMember);
			}
			if (nctsMovement.IsUnloadingRemarksTabVisible)
			{
				CreateAndBindUnloadingRemarksUserControl(dataSource, dataMember);
			}
			CreateAndBindMessagesUserControl(nctsMovement);
		}

		void InitializeValueChangedEvents()
		{
			if (nctsMovement.IsDepartureMovement)
			{
				nctsMovement.MovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
				nctsMovement.MovementHeader.BM_CustomsStatusInfo.ValueChanged += StatusInfo_ValueChanged;
			}

			nctsMovement.BH_ExportFlagInfo.ValueChanged -= ShowIncidentsTab;
			nctsMovement.BH_ExportFlagInfo.ValueChanged += ShowIncidentsTab;
			nctsMovement.EffectiveMessageStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
			nctsMovement.EffectiveMessageStatusInfo.ValueChanged += StatusInfo_ValueChanged;
		}

		protected virtual ZUserControl GetDeclarationDetailsTabUserControl() => new Phase5DeclarationDetailsTabUserControl();

		protected virtual ZUserControl GetNctsArrivalUserControl() => new Phase5ArrivalNotificationTabUserControl();

		protected virtual ZUserControl GetIncidentsUserControl() => new Phase5EventTabUserControl();

		protected virtual ZUserControl GetUnloadingRemarksUserControl() => new Phase5UnloadingRemarksTabUserControl();

		protected virtual ZUserControl GetMessagesUserControl() => new MessagesTabUserControl();

		protected virtual ZUserControl GetMiscOptionsUserControl() => new Phase5DeclarationMiscTabUserControl();

		protected virtual ZUserControl GetDeclarationServicesUserControl() => new Phase5DeclarationServicesTabUserControl();

		protected virtual ZUserControl GetTransportAndPackagingUserControl() => new Phase5TransportAndPackagingTabUserControl();

		protected virtual ZUserControl GetHouseConsignmentsUserControl() => new HouseConsignmentsTabUserControl();

		protected virtual ZBool SupportsMiscOptionsTabPage => nctsMovement.Configuration.MiscTabPageSupport(nctsMovement);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				if (nctsMovement != null)
				{
					if (nctsMovement.IsDepartureMovement)
					{
						nctsMovement.MovementHeader.BM_CustomsStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
					}
					nctsMovement.BH_ExportFlagInfo.ValueChanged -= ShowIncidentsTab;
					UnhookEvents();
					nctsMovement.EffectiveMessageStatusInfo.ValueChanged -= StatusInfo_ValueChanged;
				}
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
			if (nctsMovement.IsDepartureTabVisible)
			{
				ToggleDepartureDeclarationRelatedTabsEditableState();
				ServicesTabPage.TabVisible = true;
				TransportAndPackagingTabPage.TabVisible = true;
				HouseConsignmentsTabPage.TabVisible = true;
			}
			else
			{
				DepartureDeclarationTabPage.TabVisible = false;
				ServicesTabPage.TabVisible = false;
				TransportAndPackagingTabPage.TabVisible = false;
				HouseConsignmentsTabPage.TabVisible = false;
				MiscOptionsTabPage.TabVisible = false;
			}
		}

		void ToggleDepartureDeclarationRelatedTabsEditableState()
		{
			var isDepartureTabEditable = !nctsMovement.IsDepartureTabReadOnly;
			DepartureDeclarationTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
			MiscOptionsTabPage.UpdateEditableIncludingChildren(isDepartureTabEditable);
		}

		void ShowArrivalTab()
		{
			if (nctsMovement.IsArrivalTabVisible)
			{
				ArrivalNotificationTabPage.TabVisible = true;
				ShowIncidentsTab();
				ToggleArrivalNotificationTabPageEditableState();
				MainTabControl.SelectedTab = ArrivalNotificationTabPage;
			}
			else
			{
				ArrivalNotificationTabPage.TabVisible = false;
				IncidentsTabPage.TabVisible = false;
			}
		}

		void ShowIncidentsTab(object sender = null, EventArgs e = null) => IncidentsTabPage.TabVisible = nctsMovement.IsArrivalEventAvailable;

		void HookEvents()
		{
			nctsMovement.RemoveIncidentsWhenIncidentFlagChanged += ShowRemoveIncidentsDialogBox;
		}

		void UnhookEvents()
		{
			nctsMovement.RemoveIncidentsWhenIncidentFlagChanged -= ShowRemoveIncidentsDialogBox;
		}

		void ShowRemoveIncidentsDialogBox(object sender, CancelEventArgs e)
		{
			if (nctsMovement.EnRouteIncidents.Count > 0)
			{
				var result = Globals.Message.Show(Res.GetString("D3CC57F1-0D7A-4CAD-881E-A1446CDD66AA", "Incident details will be deleted. Are you sure you want to continue?"),
													Res.GetString("030EEE3A-49C4-4AD7-8623-E85FDD114CF9", "Delete Incidents?"),
													MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				e.Cancel = result == DialogResult.No;
			}
		}

		void ToggleArrivalNotificationTabPageEditableState()
		{
			ArrivalNotificationTabPage.UpdateEditableIncludingChildren(isEditable: !nctsMovement.IsArrivalTabReadOnly, ControlsAllowedToRemainEditableAfterSending);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The underlying ZArchitecture method, UpdateEditableIncludingChildren, uses an array where null values indicate absence.")]
		protected virtual string[] ControlsAllowedToRemainEditableAfterSending => null;

		public void ShowUnloadingTab(bool changeSelectedTab = true)
		{
			if (nctsMovement.IsUnloadingRemarksTabVisible)
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
			UnloadingRemarksTabPage.UpdateEditableIncludingChildren(isEditable: !nctsMovement.IsUnloadingRemarksTabReadOnly);
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

		void CreateAndBindMessagesUserControl(NctsHeader header)
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

			BindingSource.SetBindingMember(MessagesUserControl,
				header.IsDepartureMovement
					? $"{nameof(NctsHeader.MovementHeader)}.{nameof(NctsDepartureMovementHeader.MessagesForDisplay)}"
					: $"{nameof(NctsHeader.Messages)}");
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

		void CreateAndBindIncidentsUserControl(object dataSource, string dataMember)
		{
			if (IncidentsUserControl == null)
			{
				IncidentsUserControl = GetIncidentsUserControl();
				BindingSource.SetBindingMember(this.IncidentsUserControl, "EnRouteIncidents");
				IncidentsTabPage.Controls.Add(IncidentsUserControl);
				IncidentsUserControl.AllowDrop = true;
				IncidentsUserControl.Dock = DockStyle.Fill;
				IncidentsUserControl.Name = "IncidentsUserControl";
			}

			IncidentsUserControl.SetDataBinding(dataSource, dataMember);
		}

		void CreateAndBindHouseConsignmentsUserControl(object dataSource, string dataMember)
		{
			if (HouseConsignmentsUserControl == null)
			{
				HouseConsignmentsUserControl = GetHouseConsignmentsUserControl();
				BindingSource.SetBindingMember(HouseConsignmentsUserControl, "Bills");
				HouseConsignmentsTabPage.Controls.Add(HouseConsignmentsUserControl);
				HouseConsignmentsUserControl.AllowDrop = true;
				HouseConsignmentsUserControl.Dock = DockStyle.Fill;
				HouseConsignmentsUserControl.Name = "HouseConsignmentsUserControl";
			}

			HouseConsignmentsUserControl.SetDataBinding(dataSource, dataMember);
		}

		void CreateAndBindTransportAndPackagingUserControl(object dataSource, string dataMember)
		{
			if (TransportAndPackagingUserControl == null)
			{
				TransportAndPackagingUserControl = GetTransportAndPackagingUserControl();
				TransportAndPackagingTabPage.Controls.Add(TransportAndPackagingUserControl);
				TransportAndPackagingUserControl.AllowDrop = true;
				TransportAndPackagingUserControl.Dock = DockStyle.Fill;
				TransportAndPackagingUserControl.Name = "TransportAndPackagingUserControl";
			}

			TransportAndPackagingUserControl.SetDataBinding(dataSource, dataMember);
		}

		void CreateAndBindServicesUserControl(object dataSource, string dataMember)
		{
			if (ServicesUserControl == null)
			{
				ServicesUserControl = GetDeclarationServicesUserControl();
				ServicesTabPage.Controls.Add(ServicesUserControl);
				ServicesUserControl.AllowDrop = true;
				ServicesUserControl.Dock = DockStyle.Fill;
				ServicesUserControl.Name = "ServicesUserControl";
			}

			ServicesUserControl.SetDataBinding(dataSource, dataMember);
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

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => nctsMovement;
	}
}
