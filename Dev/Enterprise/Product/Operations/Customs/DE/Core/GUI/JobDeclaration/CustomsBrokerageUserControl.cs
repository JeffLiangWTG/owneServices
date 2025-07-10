using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.GUI
{
	public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override IBasePackingControl GetPackingUserControl() => JobDeclaration.IsExport ? new ExportCustomsPackingUserControl() : base.GetPackingUserControl();

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

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

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			BaseInvoiceLineUserControl result;
			var declaration = JobDeclaration;
			if (declaration.IsImport)
			{
				result = new ImportInvoiceLineUserControl();
			}
			else if (declaration.IsWarehouseAdjustment)
			{
				result = new WarehouseAdjustmentInvoiceLineUserControl();
			}
			else
			{
				result = new ExportInvoiceLineUserControl();
			}
			return result;
		}

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new EntryMessageUserControl();

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (JobDeclaration != null)
			{
				JobDeclaration.OnPreviousDocumentMasterCSI_ProcedureAboutToChange -= JobDeclaration_OnPreviousDocumentProcedureCodeAboutToChange;

				JobDeclaration.OnInvHeaderZG_AgreedPlaceCodeValueChanged -= JobDeclaration_OnInvHeaderZgAgreedPlaceCodeValueChanged;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (JobDeclaration != null)
			{
				JobDeclaration.OnPreviousDocumentMasterCSI_ProcedureAboutToChange += JobDeclaration_OnPreviousDocumentProcedureCodeAboutToChange;

				JobDeclaration.OnInvHeaderZG_AgreedPlaceCodeValueChanged += JobDeclaration_OnInvHeaderZgAgreedPlaceCodeValueChanged;
			}
		}

		void JobDeclaration_OnInvHeaderZgAgreedPlaceCodeValueChanged(object sender, EventArgs e)
		{
			if (sender is JobComInvoiceHeader jobComInvoiceHeader && jobComInvoiceHeader.IsImport)
			{
				bool HasCharges() => jobComInvoiceHeader.Charges.Count > 0;
				bool HasApportionedCharges() => jobComInvoiceHeader.GroupCharges.Count > 0;
				bool HasGroupCharges() => jobComInvoiceHeader.JobDeclaration.TopGroupInvoice.Charges.Count > 0;
				bool LinesHaveCharges() => jobComInvoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>()
					.Any(l => l.Charges.Count > 0 || l.ApportionedCharges.Count > 0);

				if (HasCharges() || HasApportionedCharges() || HasGroupCharges() || LinesHaveCharges())
				{
					Globals.Message.Show(Res.GetString("b2e29a09-a6c5-4bae-9c36-5d2c3654518c",
						"Incoterm Key has been changed: Please make sure all entered charges against this Invoice are still correct!"));
				}
			}
		}

		void JobDeclaration_OnPreviousDocumentProcedureCodeAboutToChange(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (sender is PreviousDocumentMaster previousDocumentMaster)
			{
				var procedureCode = previousDocumentMaster.CSI_Procedure;
				if (!procedureCode.IsEmpty && new PreviousDocumentConfiguration().ProcedureCodeSupportsMultiplePreviousDocuments(JobDeclaration.IsImport, procedureCode))
				{
					e.Cancel = Globals.Message.Show(
						Res.GetString("6DEF24D1-906B-4122-A7D5-CF903C2D30BE", "The System is about to delete all existing Previous Document lines.\r\nDo you want to continue?"),
						Res.GetString("94B2D4F5-BA95-4A58-B583-953F49F98E8F", "Previous Document Deletion"),
						MessageBoxButtons.OKCancel,
						DialogResult.Cancel) == DialogResult.Cancel;
				}
			}
		}

		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	}
}
