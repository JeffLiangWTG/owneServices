using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed partial class Phase5DepartureMovementsTabGridUserControl : ZUserControl
	{
		public Phase5DepartureMovementsTabGridUserControl()
		{
			InitializeComponent();

			resetMovementMenuItem = new ZMenuItem(ResString.GetMultilingualString("3014CFC3-1A0D-4AF3-A6E1-0B18A11D399F", "Reset Movement for Retransmission"), ResetMovementForRetransmission_Click);

			InitializeMenuItems();
		}

		NctsDepartureMovementHeader DepartureMovementHeader
		{
			get { return (NctsDepartureMovementHeader)CurrentDataItem; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			MovementsGrid.AfterBind -= new EventHandler(MovementsGrid_AfterBind);
			if (dataSource != null)
			{
				MovementsGrid.AfterBind += new EventHandler(MovementsGrid_AfterBind);
			}
			base.SetDataBinding(dataSource, dataMember);
			var phase5DepartureMovementForm = TopLevelControl as Phase5DepartureMovementForm;
			if (!(phase5DepartureMovementForm?.DepartureMovementsTabUserControl?.ProcessTemplateCustomFieldsControl?.IsDisposed ?? true))
			{
				phase5DepartureMovementForm.DepartureMovementsTabUserControl.ProcessTemplateCustomFieldsControl.ForceBindingIncludingParents();
			}

			if (DepartureMovementHeader != null)
			{
				HookCustomFields();
			}
		}

		void MovementsGrid_AfterBind(object sender, EventArgs e)
		{
			var currentNctsDepartureMovementHeader = (NctsDepartureMovementHeader)MovementsGrid.ListManager?.GetCurrent();
			int indexOfCurrentHouseBill = MovementsGrid.List == null ? -1 : MovementsGrid.List.IndexOf(currentNctsDepartureMovementHeader);
			if (indexOfCurrentHouseBill >= 0)
			{
				HookMovementsGridEvents();
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			UpdateGridColumnLayout();
		}

		#region HookCustomFields

		void HookCustomFields()
		{
			if (!customFieldsHooked && DataSource?.MovementHeader is NctsDepartureMovementHeader moveHeader)
			{
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(MovementsGrid, moveHeader.Header?.DepartureMovementHeaders, WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor, true);
				customFieldsHooked = true;
			}
		}
		bool customFieldsHooked;

		#endregion

		void HookMovementsGridEvents()
		{
			if (MovementsGrid != null && MovementsGrid.ListManager != null)
			{
				MovementsGrid.ListManager.CurrentChanged += new EventHandler(MovementsGrid_CurrentChanged);
			}
		}

		void UnHookMovementsGridEvents()
		{
			if (MovementsGrid != null && MovementsGrid.ListManager != null)
			{
				MovementsGrid.ListManager.CurrentChanged -= new EventHandler(MovementsGrid_CurrentChanged);
			}
		}

		void MovementsGrid_CurrentChanged(object sender, EventArgs e)
		{
			var currentNctsDepartureMovementHeader = (NctsDepartureMovementHeader)MovementsGrid.ListManager?.GetCurrent();
			if (currentNctsDepartureMovementHeader != null)
			{
				int indexOfCurrenDepartureMovementHeader = MovementsGrid.List == null ? -1 : MovementsGrid.List.IndexOf(currentNctsDepartureMovementHeader);
				if (indexOfCurrenDepartureMovementHeader >= 0)
				{
					currentNctsDepartureMovementHeader.BM_InBondEntryType = DataSource?.MovementHeader?.BM_InBondEntryType ?? ZString.Empty;
					currentNctsDepartureMovementHeader.BM_AdditionalDeclarationType = DataSource?.MovementHeader?.BM_AdditionalDeclarationType ?? ZString.Empty;
					currentNctsDepartureMovementHeader.IsSimplifiedNctsProcedure = DataSource?.MovementHeader?.IsSimplifiedNctsProcedure ?? ZBool.False;
					currentNctsDepartureMovementHeader.BM_RL_NKDestinationPort = DataSource?.MovementHeader?.BM_RL_NKDestinationPort ?? ZString.Empty;
					MovementsGrid.Select(indexOfCurrenDepartureMovementHeader);
				}
			}
		}

		new NctsHeader DataSource => base.DataSource as NctsHeader;

		void UpdateGridColumnLayout()
		{
			var layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode);
			MovementsGrid.ApplyGridColumnLayout(layoutProvider.GetDepartureMovementGridColumnLayout());
		}

		void InitializeMenuItems()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				MovementsGrid.ContextMenu.MenuItems.Add(resetMovementMenuItem);
				MovementsGrid.ContextMenu.Popup += ContextMenu_Popup;
			}
		}

		void ResetMovementForRetransmission_Click(object sender, EventArgs e)
		{
			var currentNctsDepartureMovementHeader = (NctsDepartureMovementHeader)MovementsGrid.ListManager?.GetCurrent();

			if (currentNctsDepartureMovementHeader != null)
			{
				var result = currentNctsDepartureMovementHeader.ResetMovementForRetransmission();

				if (result.IsUpdated)
				{
					Globals.Message.Show(result.Message);
				}
				else
				{
					Globals.Message.ShowWarning(result.Message);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && MovementsGrid.ContextMenu != null)
			{
				MovementsGrid.ContextMenu.Popup -= ContextMenu_Popup;
			}

			if (disposing)
			{
				UnHookMovementsGridEvents();
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			var currentNctsDepartureMovementHeader = (NctsDepartureMovementHeader)MovementsGrid.ListManager?.GetCurrent();
			resetMovementMenuItem.Visible = currentNctsDepartureMovementHeader?.IsDepartureRetransmissionAllowed ?? false;
		}

		readonly ZMenuItem resetMovementMenuItem;
	}
}
