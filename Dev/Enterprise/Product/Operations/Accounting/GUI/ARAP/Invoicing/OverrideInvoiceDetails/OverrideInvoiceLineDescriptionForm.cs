using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideInvoiceLineDescriptionForm : OverrideInvoiceDetailsForm
	{
		public OverrideInvoiceLineDescriptionForm(InvoiceLineOverrideForEditingDescriptionAdaptor bo)
			: base(bo)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				SetTransactionLineDefaults();
			}
		}

		protected void SetTransactionLineDefaults()
		{
			var columnsToRemoveIfNotGSTRegistered = BaseInvoicingForm.GetColumnsToRemoveIfNotGSTRegistered();
			for (int i = InvoicesGrid.ColumnStyles.Count - 1; i >= 0; i--)
			{
				var columnStyle = (ZGridColumnInfo)InvoicesGrid.ColumnStyles[i];

				if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered & columnsToRemoveIfNotGSTRegistered.Contains(columnStyle.ColumnName))
				{
					InvoicesGrid.ColumnStyles.Remove(columnStyle);
				}
			}
		}
	}
}
