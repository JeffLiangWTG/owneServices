using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentItemValuationInformationProvider : IGoodsShipmentItemTypeValuationInformation
	{
		public IM413AndIM415GoodsShipmentItemValuationInformationProvider(CusEntryLine entryLine, EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryLine = entryLine;
			entryLineWrapper = new EntryLineWrapper(entryLine, entryHeaderWrapper);
			randomInvoiceLine = entryLineWrapper.RandomInvoiceLine;
		}
		readonly EntryLineWrapper entryLineWrapper;
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine randomInvoiceLine;

		public IReadOnlyCollection<IAdditionsAndDeductions> AdditionsDeductions => additionsDeductions ??= AISMessageProviderHelper.GetAdditionsAndDeductions(entryLine.InvoiceLines.Cast<JobComInvoiceLine>());
		IReadOnlyCollection<IAdditionsAndDeductions> additionsDeductions;

		public string ValuationIndicator => valuationIndicator ??= AISMessageProviderHelper.GetValuationIndicator(entryLineWrapper.RandomInvoiceHeader, randomInvoiceLine);
		string valuationIndicator;

		public decimal ItemAmount => itemAmount ??= entryLine.TotalLinePrice.Amount;
		decimal? itemAmount;

		public string ValuationMethod => randomInvoiceLine.JI_ValuationCode;

		public string Preference => randomInvoiceLine.JI_PrimaryPreference;

		public IValueType Value => null;

		public ITransportCostsType TransportCosts => null;
	}
}
