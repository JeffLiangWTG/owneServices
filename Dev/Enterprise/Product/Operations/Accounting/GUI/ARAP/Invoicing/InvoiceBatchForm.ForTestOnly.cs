#if DEBUG

using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceBatchForm
	{
		public void FilterControl_PerformSearch_ForTestOnly(object sender, EventArgs e)
		{
			FilterControl_PerformSearch(sender, e);
		}

		public ZPanel NotificationPanel_ForTestOnly
		{
			get { return NotificationPanel; }
			set { NotificationPanel = value; }
		}

		public InvoiceBatchOnFormFilterControl FilterControl_ForTestOnly
		{
			get { return FilterControl; }
			set { FilterControl = value; }
		}

		public ZTextBox BatchNumberTextBox_ForTestOnly
		{
			get { return BatchNumberTextBox; }
			set { BatchNumberTextBox = value; }
		}

		public ZDropEdit TermsDropEdit_ForTestOnly
		{
			get { return TermsDropEdit; }
			set { TermsDropEdit = value; }
		}

		public ZCalcEdit TermDaysCalcEdit_ForTestOnly
		{
			get { return TermDaysCalcEdit; }
			set { TermDaysCalcEdit = value; }
		}

		public ZDateEdit DueDateDateEdit_ForTestOnly
		{
			get { return DueDateDateEdit; }
			set { DueDateDateEdit = value; }
		}

		public ZGuidFindBox DebtorGuidFindBox_ForTestOnly
		{
			get { return DebtorGuidFindBox; }
			set { DebtorGuidFindBox = value; }
		}

		public IZForm ShowSelectedTransaction_ForTestOnly()
		{
			return ShowSelectedTransaction();
		}

		public ContinueWithSave PostSaveProcessing_ForTestOnly(ContinueWithSave baseResult)
		{
			return PostSaveProcessing(baseResult);
		}

		public Core.Forms.ZPostOrCancelButton PostButton_ForTestOnly
		{
			get { return PostButton; }
			set { PostButton = value; }
		}

		public ZGrid BatchInvoiceLinesGrid_ForTestOnly
		{
			get { return BatchInvoiceLinesGrid; }
			set { BatchInvoiceLinesGrid = value; }
		}

		public Core.Forms.ZPostOrCancelButton CloseButton_ForTestOnly
		{
			get { return CloseButton; }
			set { CloseButton = value; }
		}

		public ZCheckedListBox JobTypeCheckedListBox_ForTestOnly
		{
			get { return JobTypeCheckedListBox; }
			set { JobTypeCheckedListBox = value; }
		}

		public ZLabel CancelledBatchLabel_ForTestOnly
		{
			get { return CancelledBatchLabel; }
			set { CancelledBatchLabel = value; }
		}

		public ZLabel NotificationLabel_ForTestOnly
		{
			get { return NotificationLabel; }
			set { NotificationLabel = value; }
		}

		public BusinessObjectFactory LineFactory_ForTestOnly => LineFactory;

		public void FilterControl_ClearButtonClicked_ForTestOnly(object sender, EventArgs e)
		{
			FilterControl_ClearButtonClicked(sender, e);
		}

		public void DoSearch_ForTestOnly(ZQuery query, int count)
		{
			DoSearch(query, count);
		}
	}
}

#endif
