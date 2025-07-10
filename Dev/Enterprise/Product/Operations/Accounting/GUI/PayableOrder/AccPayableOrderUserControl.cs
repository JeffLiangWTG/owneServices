using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class AccPayableOrderUserControl : ZUserControl
	{
		public AccPayableOrderUserControl()
		{
			InitializeComponent();
			this.AddressWithContactControl.ContactInfoTabVisible = false;
			GSTInclusiveCheckBox.Visible = GlbCompany.CurrentCompany.GC_IsGSTRegistered;

			orderLinesTotalByProduct.AllowOutsideOfParent();
			AddressWithContactControl.AllowOverlap(BottomTabControl);
			APH_ReadyForDeliveryBoundDateEdit.AllowOverlap(GSTInclusiveCheckBox);
		}

		void BottomTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == this.ProductSummaryTab)
			{
				Order.ProductQuantitySummary.Populate();
			}
		}

		void DispositionExplainButton_Click(object sender, EventArgs e)
		{
			if (!Order.APH_DispositionInfo.HasErrors())
			{
				var hintText = GetHintForDisposition(Order.APH_Disposition);
				Globals.Message.ShowInformation(hintText, Order.APH_Disposition);
			}
		}

		MultilingualString GetHintForDisposition(string disposition)
		{
			MultilingualString result;
			switch (disposition)
			{
				case Constants.PayableOrderDisposition.OrderIncomplete:
					result = ResString.GetMultilingualString("70301b51-4c04-44bf-9b58-90888d23b365", "The Order currently has no Order Lines.  Once Order Lines are added or subsequent changes are saved to Charge Allocation, Quantity, Item Price or Line Price, the Order will be 'Pending Approval'.");
					break;
				case Constants.PayableOrderDisposition.PendingApproval:
					result = ResString.GetMultilingualString("84baf8fc-05d1-49a8-9941-2d4cce791019", "Changes to the Order Lines were made that need approval.  Action 'Approve' will update the Disposition to 'Order to be Placed' or 'AP Invoice to be Posted' as relevant");
					break;
				case Constants.PayableOrderDisposition.OrderToBePlaced:
					result = ResString.GetMultilingualString("31105df5-4ad0-4266-8aaa-a5eaa532791a", "The Order was approved for the first time or after subsequent changes to the Order Line details. Please deliver the Booking Request after each Approval to ensure the Supplier can Confirm any amendments. Simply updating the Confirmation- or Expected DLV Dates will bypass the 'Order to be Placed' Disposition if required.");
					break;
				case Constants.PayableOrderDisposition.PendingConfirmation:
					result = ResString.GetMultilingualString("300fee47-a229-4a69-b4fb-f17137fcd66a", "Enter the Confirmation Number/Date after the Supplier confirmed receipt of the latest Booking Request.");
					break;
				case Constants.PayableOrderDisposition.ExpectedDLVPending:
					result = ResString.GetMultilingualString("6940b84a-9811-4ebd-bbf1-2b46121a2db1", "Updates to the 'Expected DLV Date' changes the Disposition to 'Delivery in Progress' for Approved Orders. 'Ready for DLV' creates an ETD event and 'Expected DLV Date' creates an ETA event.");
					break;
				case Constants.PayableOrderDisposition.DeliveryInProgress:
					result = ResString.GetMultilingualString("8a1243eb-f271-4fa6-a952-06908d3e8b3a", "Reducing the Quantity Remaining on the Order Lines changes the Disposition to 'AP Invoice to be Posted'.");
					break;
				case Constants.PayableOrderDisposition.APInvoiceToBePosted:
					result = ResString.GetMultilingualString("b628ca0e-c379-4bbb-8a11-2517698e6c91", "Enter the Supplier's Invoice Number and Invoice Date to 'Post the AP Invoice' in the Actions menu. It is advised that the Quantity Invoiced and Quantity Received reconciles before posting the AP Invoice as only one AP Invoice may be posted per Order and related Split Orders. If a second Invoice is received simply 'Create a New Order' for the remaining Order Lines or reverse the posted invoice in the Payables Transactions module to enable another 'AP Invoice to be Posted' from the Order.");
					break;
				case Constants.PayableOrderDisposition.PendingGoodsReceivedAudit:
					result = ResString.GetMultilingualString("7773bf81-d190-44df-92ee-92a61bc3d496", "Any differences between the Quantity Ordered, Quantity Received and Quantity Invoiced should be accounted for by a 'Goods Received Note'. Once the AP Invoice has been posted and a 'Record Audited' event has been created the Disposition will be 'Complete' to ensure no changes can be made thereafter.");
					break;
				case Constants.PayableOrderDisposition.Complete:
					result = ResString.GetMultilingualString("2aa780e9-0a01-4cb0-b545-fc3abe1c743d", "Whilst the Order is in a 'Complete' Disposition, no changes can be made to it.  Canceling the ADT event changes the Disposition back to 'Pending Goods Received Audit'.");
					break;
				default:
					result = ResString.GetMultilingualString("4266682c-7974-467c-8e06-2cae105ede21", "A blank or invalid disposition was specified.");
					break;
			}
			return result;
		}
	}
}
