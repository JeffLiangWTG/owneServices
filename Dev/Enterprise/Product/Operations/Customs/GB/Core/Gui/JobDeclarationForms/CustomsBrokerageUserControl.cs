using System;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			RemoveEntryInstructionDetailsTabPageControls();
		}

		void RemoveEntryInstructionDetailsTabPageControls()
		{
			var chiefDecVisible = (((JobDeclaration)JobDeclaration).JE_ApplicationCode == GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF);
			if (chiefDecVisible)
			{
				RemoveControl(EntryInstructionDetailsTabPage);
			}
		}

		protected override void SetEntryInstructionsTabVisibility()
		{
			EntryInstructionDetailsTabPage.TabRelevant = JobDeclaration.AreMultipleEntryInstructionsAllowed && EntryInstructionsTabVisibleForCountry;
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new MessageUserControl();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new JobDeclarationUserControl();
		}

		protected override BaseCustomsCusContainersUserControl GetContainerUserControl()
		{
			return new CusContainerUserControl();
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

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
				result = new GBImportSupplierHeaderUserControl();
			}
			else
			{
				result = new GBExportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			return new EntryInstructionDetailsUserControl();
		}

		protected override void JobDeclaration_JE_ApplicationCodeChanged(object sender, EventArgs e)
		{
			base.JobDeclaration_JE_ApplicationCodeChanged(sender, e);
			RemoveControl(InvoiceLinesTabPage);
		}
	}
}
