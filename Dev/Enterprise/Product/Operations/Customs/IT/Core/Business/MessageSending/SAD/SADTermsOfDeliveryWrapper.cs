using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADTermsOfDeliveryWrapper : ITermOfDeliveryGroup
{
	public SADTermsOfDeliveryWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	readonly CusEntryHeader entryHeader;
	JobComInvoiceHeader Invoice => entryHeader.RandomHeader;

	public ZString IncotermCode => Invoice.JZ_IncoTerm;

	public ZString ComplementaryCode => Invoice.ZG_AgreedPlaceCode;

	public ZString ComplementOfInfo => Invoice.JZ_IncoTermPlace;

	public virtual ZString ComplementOfInfoLng => ZString.Empty;
}
