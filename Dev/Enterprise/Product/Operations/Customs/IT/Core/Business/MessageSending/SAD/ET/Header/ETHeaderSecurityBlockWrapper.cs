using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETHeaderSecurityBlockWrapper : IETHeaderSecurityBlock
{
	public ETHeaderSecurityBlockWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
	}

	readonly CusEntryHeader entryHeader;
	readonly CusEntryInstruction entryInstruction;

	public ZString SpecificCircumstanceIndicator => ZString.Empty;

	public ZString PlaceOfLoadingCode => ZString.Empty;

	public ZString ConveyanceReferenceNumber => ZString.Empty;

	public ZString PlaceOfUnloadingCode => ZString.Empty;

	public IEnumerable<ZString> TransitCountries => System.Array.Empty<ZString>();

	public ITrader Carrier => new SADEmptyTraderWrapper();

	public ZInt SealsNumber => entryInstruction.ZG_SealsCount;

	public ZString TransportChargesMethodOfPayment
	{
		get
		{
			var methodOfPayments = entryHeader.InvoiceHeaders?.Select(x => x.ZG_TransportChargesMethodOfPayment).Distinct().ToArray() ?? System.Array.Empty<ZString>();
			return methodOfPayments.Length == 1 ? methodOfPayments[0] : ZString.Empty;
		}
	}

	public ZString CommercialReferenceNumber => ZString.Empty;

	public ITrader Consignor => new SADEmptyTraderWrapper();

	public ITrader Consignee => new SADEmptyTraderWrapper();
}
