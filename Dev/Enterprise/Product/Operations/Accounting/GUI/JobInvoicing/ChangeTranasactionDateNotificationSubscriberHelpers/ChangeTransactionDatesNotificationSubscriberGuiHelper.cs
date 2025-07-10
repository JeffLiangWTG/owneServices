using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class ChangeTransactionDatesNotificationSubscriberGuiHelper : NotificationSubscriberGuiHelper
	{
		public ChangeTransactionDatesNotificationSubscriberGuiHelper()
		{
		}

		protected override YesNoYesAllNoAllMessageBoxResult ShowYesNoAllMessageBox(string message, string caption)
		{
			YesNoYesAllNoAllMessageBoxResult yesNoAllResult;
			using (ChangeTransactionDatesMessageBox dialog = GetNewMessageBox())
			{
				dialog.ShowYesNoAllButtons = true;
				ZFormModaliser.ShowDialogWithoutDispose(dialog);
				yesNoAllResult = dialog.YesNoAllResult;
			}

			if (yesNoAllResult == YesNoYesAllNoAllMessageBoxResult.YesToAll)
			{
				ChangeTransactionDateToAll = ChangeTransactionDateBusinessObject;
			}
			return yesNoAllResult;
		}

		protected ChangeTransactionDatesBusinessObject ChangeTransactionDateToAll
		{ get; private set; }

		protected override DialogResult ShowMessageBox(string message, string caption, MessageBoxButtons buttons, DialogResult defaultResult)
		{
			DialogResult dialogResult;
			using (ChangeTransactionDatesMessageBox dialog = GetNewMessageBox())
			{
				dialog.ShowYesNoAllButtons = false;
				dialogResult = ZFormModaliser.ShowDialogWithoutDispose(dialog);
			}
			return dialogResult;
		}

		protected virtual ChangeTransactionDatesMessageBox GetNewMessageBox()
		{
			return new ChangeTransactionDatesMessageBox(ChangeTransactionDateBusinessObject);
		}

		public void SetChangeTransactionDateBusinessObject(ChangeTransactionDatesBusinessObject value)
		{
			ChangeTransactionDateBusinessObject = value;

			if (ChangeTransactionDateToAll != null)
			{
				ChangeTransactionDateBusinessObject.PopulateValuesFrom(ChangeTransactionDateToAll);
			}
		}

		protected ChangeTransactionDatesBusinessObject ChangeTransactionDateBusinessObject
		{ get; private set; }

		public ZDateTime TransactionDate
		{
			get { return ChangeTransactionDateBusinessObject != null ? ChangeTransactionDateBusinessObject.InvoiceDate : ZDateTime.Empty; }
		}

		public ZDateTime PostDate
		{
			get { return ChangeTransactionDateBusinessObject != null ? ChangeTransactionDateBusinessObject.PostDate : ZDateTime.Empty; }
		}

		public ZString RevenueRecognitionDates
		{
			get { return ChangeTransactionDateBusinessObject != null ? ChangeTransactionDateBusinessObject.RevenueRecognitionDates : ZString.Empty; }
		}
	}
}
