using System;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;

namespace Enterprise.Customs.KR.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		System.ComponentModel.Container components;

		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
			MainTabControl.Controls.Remove(InvoiceGroupingTabPage);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		JobDeclaration Declaration => (JobDeclaration)base.JobDeclaration;

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new JobDeclarationUserControl();
		}

		protected override void JobDeclaration_JE_MessageTypeChanged(object sender, EventArgs e)
		{
			base.JobDeclaration_JE_MessageTypeChanged(sender, e);
			SetEntryInstructionsTabVisibility();
		}

		protected override void RemoveUserControlOfEachTabPage()
		{
			base.RemoveUserControlOfEachTabPage();
			RemoveControl(PackingTabPage);
		}

		public override bool EntryInstructionsTabVisibleForCountry => Declaration.IsEntryInstructionRelevant;

		#region Create New User Controls for each tab

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			BaseCustomsSupplierHeaderUserControl result;

			if (Declaration.IsImport)
			{
				result = new ImportSupplierHeaderUserControl();
			}
			else if (Declaration.IsLocalExport)
			{
				result = new LocalExportSupplierHeaderUserControl();
			}
			else
			{
				result = new ExportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;

			if (Declaration.IsImport)
			{
				result = new ImportInvoiceLineUserControl();
			}
			else if (Declaration.IsLocalExport)
			{
				result = new LocalExportInvoiceLineUserControl();
			}
			else
			{
				result = new ExportInvoiceLineUserControl();
			}

			return result;
		}

		protected override IBasePackingControl GetPackingUserControl() => Declaration.IsImport ? new ImportCustomsPackingUserControl() : base.GetPackingUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			if (Declaration.IsDeclarationIntegrated)
			{
				return base.GetMessageUserControl();
			}
			else if (Declaration.IsImport)
			{
				return new ImportMessageUserControl();
			}
			else if (Declaration.IsLocalExport)
			{
				return new LocalExportMessageUserControl();
			}
			else
			{
				return new ExportMessageUserControl();
			}
		}

		#endregion

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			return new EntryInstructionDetailsUserControl();
		}
	}
}
