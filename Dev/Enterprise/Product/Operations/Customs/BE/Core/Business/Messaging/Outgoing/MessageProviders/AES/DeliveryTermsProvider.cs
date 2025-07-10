using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;
using JobComInvoiceHeader = Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader;

namespace Enterprise.Customs.BE.Business;

public class DeliveryTermsProvider : IDeliveryTerms
{
	public DeliveryTermsProvider(JobComInvoiceHeader invoiceHeader)
	{
		this.invoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
	}
	readonly JobComInvoiceHeader invoiceHeader;

	public string IncotermCode => invoiceHeader.IncoTerm;

	public string UNLocode => CachedValueHelper.GetValue(ref unlocodeCached, () => new RefUNLOCO.Loader(invoiceHeader.Factory).Load(invoiceHeader.ZG_AgreedPlaceCode)?.Code);
	CachedValue<string> unlocodeCached;

	public string Location => Country is not null ? invoiceHeader.JZ_IncoTermPlace : null;

	public string Country => CachedValueHelper.GetValue(ref countryCached, () => new RefCountry.Loader(invoiceHeader.Factory).LoadForCountry(invoiceHeader.ZG_AgreedPlaceCode)?.Code);
	CachedValue<string> countryCached;

	public string Text => invoiceHeader.IncoTermsAgreedPlace;
}
