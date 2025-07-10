using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGJobComInvoiceHeaderValueSetStrategy : IValueSetStrategy
	{
		public DeltaGJobComInvoiceHeaderValueSetStrategy(JobComInvoiceHeader invoiceHeader)
		{
			InvoiceHeader = invoiceHeader;
		}

		public JobComInvoiceHeader InvoiceHeader { get; }

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			if (valueThatHasChanged.Name == JobComInvoiceHeader.Schema.JZ_IncoTerm)
			{
				InvoiceHeader.ShouldClearIncoTermPlacesIfNeeded = true;
			}
		}
	}
}
