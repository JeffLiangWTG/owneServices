using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceHeaderValuePostProcessingStrategy
	{
		readonly JobComInvoiceHeader invoiceHeader;

		public JobComInvoiceHeaderValuePostProcessingStrategy(JobComInvoiceHeader jobComInvoiceHeader)
		{
			invoiceHeader = jobComInvoiceHeader;
		}

		protected void ValuePostProcessCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			if (valueThatHasChanged.Name == JobComInvoiceHeader.Schema.ZG_AgreedPlaceCode)
			{
				UpdateGroupCharges();
			}
		}

		protected void UpdateGroupCharges()
		{
			(invoiceHeader.JobDeclaration?.TopGroupInvoice)?.UpdateCharges();

			invoiceHeader.Charges.MarkAsNeedingValidation();

			if (invoiceHeader.JobDeclaration != null)
			{
				invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().ForEach((x) => { x.Charges.MarkAsNeedingValidation(); });
			}
		}
	}
}
