using System;
using Enterprise.Customs.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.FR.GUI
{
	public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override void JobDeclaration_JE_ApplicationCodeChanged(object sender, EventArgs e)
		{
			base.JobDeclaration_JE_ApplicationCodeChanged(sender, e);
			RemoveUserControlOfEachTabPage();
		}

		#region Create New User Controls for each tab

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new JobDeclarationUserControl();
		}

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			BaseInvoiceLineUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new ImportInvoiceLineUserControl();
			}
			else
			{
				result = new ExportInvoiceLineUserControl();
			}

			return result;
		}

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			BaseCustomsSupplierHeaderUserControl result;

			if (JobDeclaration.IsImport)
			{
				result = new ImportSupplierHeaderUserControl();
			}
			else
			{
				result = new ExportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			return new EntryInstructionDetailsUserControl();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new MessageUserControl(JobDeclaration);
		}

		#endregion
	}
}
