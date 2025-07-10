using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class PrinterSelectionForm : ZChildForm
	{
		public PrinterSelectionForm(DeliveryInstructions instructions)
			: base(instructions)
		{
			this.Instructions = instructions;
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			ChooseDefaultPrinter();
		}

		readonly DeliveryInstructions Instructions;

		#region Default Printer

		void ChooseDefaultPrinter()
		{
			for (int i = 0; i < PrintersGrid.ListManager.List.Count; i++)
			{
				StmPrintQueue queue = (StmPrintQueue)PrintersGrid.ListManager.List[i];
				if (queue.PK == Instructions.PrinterDelivery.PrintQueuePK)
				{
					PrintersGrid.Select(i);
					break;
				}
			}
		}

		#endregion

		#region GUI Setup

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region Printer Validation

		void OKButton_Click(object sender, System.EventArgs e)
		{
			ValidateAndSetPrinter();
		}

		void PrintersGrid_DoubleClick(object sender, System.EventArgs e)
		{
			ValidateAndSetPrinter();
		}

		void ValidateAndSetPrinter()
		{
			if (PrintersGrid.SelectedRowCount == 0)
			{
				Globals.Message.ShowError(Res.GetString("2bb5710f-8d66-4a2c-a7cd-d64ecf56b78c", "Please select a printer to use."), Res.GetString("951105ad-6949-4f8b-b90d-ca9ddbe802a4", "Select Printer"));
			}
			else
			{
				StmPrintQueue printQueue = (StmPrintQueue)PrintersGrid.SelectedElements[0];

				if (printQueue.SQ_QueueDeleted.IsValid)
				{
					Globals.Message.ShowError(Res.GetString("af8dbe73-953d-44ab-9417-872943061a09", "The printer you have selected is not currently installed. Please see your system administrator."), Res.GetString("b94e0e2b-9e66-4159-af52-8aeb22dbe56b", "Printer Not Installed"));
				}
				else
				{
					if (!printQueue.IsPrintAllowed)
					{
						Globals.Message.ShowError(Res.GetString("2ec88e87-0486-4c03-85a2-57bb8ba45218", "You do not have the security rights to print to the selected printer.\r\nPlease see your system administrator if you want to obtain the security rights for this printer."), Res.GetString("7cf9a4ff-6213-42d5-a63c-11ffc111c60e", "Printer Access Denied"));
					}
					else
					{
						Instructions.PrinterDelivery.PrintQueuePK = printQueue.PK;
						DialogResult = DialogResult.OK;
					}
				}
			}
		}

		#endregion
	}
}
