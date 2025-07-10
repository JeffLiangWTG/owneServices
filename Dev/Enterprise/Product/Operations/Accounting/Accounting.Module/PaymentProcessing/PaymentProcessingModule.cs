using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module for Payment Processing.
	/// </summary>
	public abstract partial class PaymentProcessingModule : ZFilterGridModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new PaymentProcessingFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new PaymentApprovalCollection(Factory);
		}

		public override bool SupportsWorkflow => true;

		public override string WorkflowType
		{
			get { return string.Empty; }
		}

		protected MenuItem PrintMenuItem;

		#region License and Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected abstract SecurityCheckpoint PostCheckpoint
		{
			get;
		}

		protected abstract SecurityCheckpoint PrintCheckpoint
		{
			get;
		}

		#endregion

		#region Menu Related Methods

		protected MenuItem[] GetReoderedItems(MenuItem[] menuItems)
		{
			List<MenuItem> result = new List<MenuItem>(menuItems);

			RemoveItem(result, NewMenuItem);
			RemoveItem(result, EditMenuItem);
			RemoveItem(result, ViewMenuItem);
			RemoveItem(result, DeleteMenuItem);
			RemoveItem(result, PrintMenuItem);

			result.Add(NewMenuItem);
			result.Add(EditMenuItem);
			result.Add(ViewMenuItem);
			result.Add(DeleteMenuItem);

			return result.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(GetReoderedItems(base.GetNewStandardMenuItems()));
			menuItems.Add(PrintMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.PrintMenuItemText, new EventHandler(PrintPaymentApproval)));
			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			if (PaymentProcessingGUIHelper.IsPaymentAuthorisationRequired())
			{
				var submitForApprovalMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.SubmitForApprovalMenuText, new EventHandler(SubmitForApproval));
				menuItems.Add(submitForApprovalMenuItem);
			}
			else
			{
				var approveForPosting = new ZMenuItem(PaymentProcessingGUIHelper.ApproveForPostingMenuText, new EventHandler(ApproveForPosting));
				menuItems.Add(approveForPosting);
			}

			MenuItem mainAuthoriseMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.AuthorisationMenuText);
			mainAuthoriseMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.AuthoriseMenuText, new EventHandler(AuthorisePaymentApprovals)));
			mainAuthoriseMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.UnAuthoriseMenuText, new EventHandler(UnAuthorisePaymentApprovals)));
			mainAuthoriseMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.RejectItemMenuText, new EventHandler(RejectPaymentApprovals)));
			menuItems.Add(mainAuthoriseMenuItem);

			MenuItem postMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.PostApprovalsMenuText);
			postMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.PostItemMenuText, new EventHandler(PostPaymentApprovals)));
			postMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.AllocateChequeNoItemMenuText, new EventHandler(PopulateChequeNoForPaymentApprovals)));
			postMenuItem.MenuItems.Add(new ZMenuItem(PaymentProcessingGUIHelper.AllocateChequeNoAndPostItemMenuText, new EventHandler(PopulateChequeNoAndPostPaymentApprovals)));
			menuItems.Add(postMenuItem);

			var cancelMenuItem = new ZMenuItem(PaymentProcessingGUIHelper.CancelMenuText, new EventHandler(CancelPaymentApprovals));
			menuItems.Add(cancelMenuItem);

			if (DataTransferMenuItem != null)
			{
				RemoveItem(menuItems, DataTransferMenuItem);
				menuItems.Add(DataTransferMenuItem);
			}

			return menuItems.ToArray();
		}

		void RemoveItem(List<MenuItem> collection, MenuItem item)
		{
			if (item != null && collection.Contains(item))
			{
				collection.Remove(item);
			}
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject != null)
			{
				var factory = new BusinessObjectFactory();
				var approvalToDelete = factory.Load<PaymentApprovalWithAuthorisation>(selectedBusinessObject.PK);

				if (approvalToDelete != null && !approvalToDelete.CanDelete)
				{
					Globals.Message.Show(selectedBusinessObject.ReasonForNotAbleToDelete);
					return null;
				}
			}

			return base.ShowDeleteForm(selectedBusinessObject);
		}

		#endregion

		#region MenuItem EventHandler

		public PaymentProcessingGUIHelper PaymentProcessingGUIHelper => paymentProcessingGUIHelper ?? (paymentProcessingGUIHelper = new PaymentProcessingGUIHelper());
		PaymentProcessingGUIHelper paymentProcessingGUIHelper;

		void SubmitForApproval(object sender, EventArgs e)
		{
			PaymentProcessingGUIHelper.SubmitForApproval(SelectedBusinessObjects);
		}

		void ApproveForPosting(object sender, EventArgs e)
		{
			PaymentProcessingGUIHelper.ApproveForPosting(SelectedBusinessObjects);
		}

		void AuthorisePaymentApprovals(object sender, EventArgs e)
		{
			PaymentProcessingGUIHelper.AuthorisePaymentApprovals(SelectedBusinessObjects);
		}

		void UnAuthorisePaymentApprovals(object sender, EventArgs e)
		{
			PaymentProcessingGUIHelper.UnAuthorisePaymentApprovals(SelectedBusinessObjects);
		}

		void RejectPaymentApprovals(object sender, EventArgs e)
		{
			PaymentProcessingGUIHelper.RejectPaymentApprovals(SelectedBusinessObjects);
		}

		public void PostPaymentApprovals(object sender, EventArgs e)
		{
			if (!PostCheckpoint.IsAllowed)
			{
				PostCheckpoint.ShowError();
			}
			else
			{
				PaymentProcessingGUIHelper.PostPaymentApprovals(SelectedBusinessObjects);
			}
		}

		void PopulateChequeNoForPaymentApprovals(object sender, EventArgs e)
		{
			if (!PostCheckpoint.IsAllowed)
			{
				PostCheckpoint.ShowError();
			}
			else
			{
				PaymentProcessingGUIHelper.PopulateChequeNoForPaymentApprovals(SelectedBusinessObjects);
			}
		}

		void PopulateChequeNoAndPostPaymentApprovals(object sender, EventArgs e)
		{
			if (!PostCheckpoint.IsAllowed)
			{
				PostCheckpoint.ShowError();
			}
			else
			{
				PaymentProcessingGUIHelper.PopulateChequeNoAndPostPaymentApprovals(SelectedBusinessObjects);
			}
		}

		void CancelPaymentApprovals(object sender, EventArgs e)
		{
			PaymentProcessingGUIHelper.CancelPaymentApprovals(SelectedBusinessObjects);
		}

		void PrintPaymentApproval(object sender, EventArgs e)
		{
			if (!PrintCheckpoint.IsAllowed)
			{
				PrintCheckpoint.ShowError();
			}
			else
			{
				PaymentProcessingGUIHelper.PrintPaymentApproval(SelectedBusinessObjects);
			}
		}

		#endregion
	}
}
