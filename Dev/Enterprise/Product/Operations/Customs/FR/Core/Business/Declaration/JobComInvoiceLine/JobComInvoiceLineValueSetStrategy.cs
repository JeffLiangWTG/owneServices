using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceLineValueSetStrategy : EU.Business.Declaration.JobComInvoiceLineValueSetStrategy
	{
		public JobComInvoiceLineValueSetStrategy(JobComInvoiceLine invoiceLine)
			: base()
		{
			this.invoiceLine = invoiceLine;
		}
		protected readonly JobComInvoiceLine invoiceLine;

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);
			switch (valueThatHasChanged.Name)
			{
				case JobComInvoiceLine.Schema.JI_Procedure:
					OnProcedureChanged();
					break;
			}
		}

		protected virtual void OnProcedureChanged()
		{
			var dec = invoiceLine.Declaration;

			if (dec != null)
			{
				if (dec.IsImport)
				{
					if (invoiceLine.RequiresVATNumberDocument)
					{
						dec.ManageVATSupportingDocuments();
					}
					invoiceLine.ManageEndUseN990OrC990Document();
				}
			}
		}
	}
}
