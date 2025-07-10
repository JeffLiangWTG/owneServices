using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class GoodsShipmentTypeValuationInformationProvider : IGoodsShipmentTypeValuationInformation
	{
		public GoodsShipmentTypeValuationInformationProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.invoiceHeader = entryHeaderWrapper.RandomInvoiceHeader;
			this.entryHeader = entryHeaderWrapper.EntryHeader;
		}
		readonly JobComInvoiceHeader invoiceHeader;
		readonly CusEntryHeader entryHeader;

		public IDeliveryTerms DeliveryTerms => CachedValueHelper.GetValue(ref deliveryTerms, () => DeliveryTermsProvider.New(invoiceHeader));
		CachedValue<IDeliveryTerms> deliveryTerms;

		public IReadOnlyCollection<IAdditionsAndDeductions> AdditionsDeductions => additionsAndDeductions ??= entryHeader.InvoiceLines.Cast<JobComInvoiceLine>().GetAdditionsAndDeductions();
		IReadOnlyCollection<IAdditionsAndDeductions> additionsAndDeductions;
	}
}
