using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.Module
{
	public partial class CommissionLinesPreviewPane : ZUserControl
	{
		public CommissionLinesPreviewPane()
		{
			InitializeComponent();

			if (!CommissionLookups.ShouldShowServicesAndSubModules)
			{
				LinesGrid.RemoveFromAvailableColumns("CommissionHeader+CH0_Service", "CommissionHeader+CH0_SubModule");
			}
		}

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupLinesGrid();
		}

		#endregion

		#region DataBinding

		new ViewCommissionLineGrouping CurrentDataItem
		{
			get { return (ViewCommissionLineGrouping)base.CurrentDataItem; }
		}

		#endregion

		#region OpenSourceButton

		void OpenSourceButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Commission);
				controller.ShowEditForm(CurrentDataItem);
			}
			else
			{
				Globals.Message.ShowInformation(
					Res.GetString("6b9d96de-4edb-4b7c-9e35-6586ab4c2361", "Please select a job / transaction to open"),
					Res.GetString("b2a7c1ac-ce9f-44e5-89cb-d5484b3fea46", "Cannot open job / transaction"));
			}
		}

		#endregion

		#region Lines Grid

		public void SetupLinesGrid()
		{
			AddLinesGridContextMenuItems();
		}

		#region View

		protected void ViewLinesGridCurrentSelection()
		{
			var current = LinesGrid.ListManager.GetCurrent() as ViewCommissionLine;
			if (current != null)
			{
				ShowViewForm(current);
				return;
			}

			Globals.Message.ShowInformation(Res.GetString("abb68cad-a7eb-4408-9d15-a9b18690dfcb", "Please select a commission line to view."), CannotViewCommissionLineCaption);
		}

		protected virtual void ShowViewForm(ViewCommissionLine commissionLine)
		{
			ZControllerFactory.Create(ControllerIDs.CommissionLine).ShowViewForm(commissionLine);
		}

		static string CannotViewCommissionLineCaption
		{
			get { return Res.GetString("d1e68182-10fd-4334-b19e-81d51c768f2b", "Unable to view Commission Line"); }
		}

		#endregion

		#region View Commission Agreement

		void ViewLinesGridCurrentSelectionAgreement()
		{
			var current = LinesGrid.ListManager.GetCurrent() as ViewCommissionLine;
			if (current != null)
			{
				var rate = current.CommissionAgreementRecipientRate;
				if (rate != null)
				{
					ShowViewForm(rate);
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("19b4a4f7-f477-482c-a58e-7aebf74b48a0", "Selected commission line does not have a commission agreement."), CannotViewCommissionAgreementCaption);
				}
				return;
			}

			Globals.Message.ShowInformation(Res.GetString("c0902547-db94-4cbb-8a81-48670ede3683", "Please select a commission line that you wish to view commission agreement for."), CannotViewCommissionAgreementCaption);
		}

		protected virtual void ShowViewForm(OrgCommissionAgreementRecipientRate rate)
		{
			((ICommissionAgreementController)ZControllerFactory.Create(ControllerIDs.OrgCommissionAgreement)).ShowEditFormForRate(rate);
		}

		static string CannotViewCommissionAgreementCaption
		{
			get { return Res.GetString("b28302c3-5187-43fb-b84d-b7a0f6bd1bf3", "Unable to view Commission Agreement"); }
		}

		#endregion

		#region View Approval Request

		void ViewLinesGridCurrentSelectionApprovalRequest()
		{
			var current = LinesGrid.ListManager.GetCurrent() as ViewCommissionLine;
			if (current != null)
			{
				var approvalRequest = current.ApprovalRequest;
				if (approvalRequest != null)
				{
					ShowViewForm(approvalRequest);
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("e4c153b3-f018-43c1-9601-433413103512", "Selected commission line does not have an approval request."), CannotViewApprovalRequestCaption);
				}
				return;
			}

			Globals.Message.ShowInformation(Res.GetString("ad045115-912a-4504-ad15-fc6e4c7c2cb7", "Please select a commission line that you wish to view approval request for."), CannotViewApprovalRequestCaption);
		}

		protected virtual void ShowViewForm(AccCommissionApprovalRequest approvalRequest)
		{
			ZControllerFactory.Create(ControllerIDs.CommissionApprovalRequest).ShowEditForm(approvalRequest);
		}

		static string CannotViewApprovalRequestCaption
		{
			get { return Res.GetString("d9d22882-f909-4f13-a7e4-8edad0795ad3", "Unable to view Approval Request"); }
		}

		#endregion

		#region Lines Grid Context Menu

		void AddLinesGridContextMenuItems()
		{
			var viewMenuItem = new ZMenuItem(ResString.GetMultilingualString("615a3922-8f7a-45ec-9e64-a140e2af7412", "&View"), OnViewMenuItemClicked);
			var viewAgreementMenuItem = new ZMenuItem(ResString.GetMultilingualString("f4d6418f-9cee-4d65-8fc2-c4f10f5b09b8", "&View Commission Agreement"), OnViewAgreementMenuItemClicked);
			var viewApprovalRequestMenuItem = new ZMenuItem(ResString.GetMultilingualString("7ec190f9-dced-4e30-8227-1bca981da917", "&View Approval Request"), OnViewApprovalRequestMenuItemClicked);
			var cancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("ce86146d-826c-499c-b12e-91f0ade2d73d", "&Cancel"), OnCancelMenuItemClicked);
			var undoCancelMenuItem = new ZMenuItem(ResString.GetMultilingualString("86c1aa25-006c-4e31-a4ea-0cd2fccc7ae7", "&Undo Cancel"), OnUndoCancelMenuItemClicked);
			cancelMenuItem.Shortcut = Shortcut.Del;

			LinesGrid.ContextMenu.MenuItems.InsertRange(0, new[] { viewMenuItem, viewAgreementMenuItem, viewApprovalRequestMenuItem, cancelMenuItem, undoCancelMenuItem });
		}

		void OnViewMenuItemClicked(object sender, EventArgs e)
		{
			ViewLinesGridCurrentSelection();
		}

		void OnViewAgreementMenuItemClicked(object sender, EventArgs e)
		{
			ViewLinesGridCurrentSelectionAgreement();
		}

		void OnViewApprovalRequestMenuItemClicked(object sender, EventArgs e)
		{
			ViewLinesGridCurrentSelectionApprovalRequest();
		}

		void OnCancelMenuItemClicked(object sender, EventArgs e)
		{
			CancelCurrentSelections();
		}

		void OnUndoCancelMenuItemClicked(object sender, EventArgs e)
		{
			UndoCancelCurrentSelections();
		}

		#endregion

		#region Cancel

		public void CancelCurrentSelections()
		{
			var selectedElements = LinesGrid.SelectedElements.OfType<ViewCommissionLine>();
			if (!selectedElements.Any())
			{
				Globals.Message.ShowInformation(Res.GetString("9a406584-a5cd-42bd-9f2f-548ace8c44b4", "Please select commission line(s) to cancel."), CancelCaption);
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var bulkCancelAction = BulkCancelCommissionLinesAction.NewForDifferentFactory(newFactory, selectedElements);

			var form = new BulkCancelCommissionLinesForm(bulkCancelAction);
			ZFormModaliser.Show(form, ParentForm);
		}

		#endregion

		#region Undo Cancel

		public void UndoCancelCurrentSelections()
		{
			var selectedElements = LinesGrid.SelectedElements.OfType<ViewCommissionLine>().ToList();
			if (!selectedElements.Any())
			{
				Globals.Message.ShowInformation(Res.GetString("a0768f3b-fb03-4ebd-8207-51bdae5f02a8", "Please select commission line(s) to undo cancel."), UndoCancelCaption);
				return;
			}

			var newFactory = new BusinessObjectFactory();
			var bulkCancelAction = BulkUndoCancelCommissionLinesAction.NewForDifferentFactory(newFactory, selectedElements);
			var form = new BulkUndoCancelCommissionLinesForm(bulkCancelAction);
			ZFormModaliser.Show(form, ParentForm);
		}

		#endregion

		#region Event Handlers

		protected void LinesGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				if (LinesGrid.HitTest(e.X, e.Y).Row > -1)
				{
					ViewLinesGridCurrentSelection();
				}
			}
		}

		#endregion

		#endregion

		#region Classes

		public class DisplayGridShowingNotifications : ZDisplayGrid
		{
			protected override bool ShouldShowNotifications
			{
				get { return true; }
			}
		}

		#endregion

		#region Messages

		static string CancelCaption
		{
			get { return Res.GetString("b3baad98-06c3-4b6c-8b77-2dbac95e3401", "Cancel Entity Commission"); }
		}

		static string UndoCancelCaption
		{
			get { return Res.GetString("3d4029e0-14cd-436b-b01d-f4dc70658782", "Undo Cancellation of Commissions"); }
		}

		#endregion

		#region Testing
#if DEBUG

		public ZController LastControllerForTesting;

#endif
		#endregion
	}
}
