using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public abstract class GenericEVVDocumentWrapper<TProvider> : BaseEVVDocumentWrapper
	where TProvider : class, IEvvCommonProvider
{
	protected GenericEVVDocumentWrapper(ICHEDIMessage message, BusinessObjectFactory factory) : base(message, factory)
	{
		responseData = message.MessageDetail as TProvider;
	}

	protected TProvider responseData;

	public ZString DocumentLanguage => documentLanguage ??= Factory.GetDocumentLanguage(responseData?.DocumentLanguage);
	ZString? documentLanguage;

	public ZString DocumentType => responseData?.DocumentType ?? ZString.Empty;

	public ZString DocumentNumber => responseData?.DocumentNumber ?? ZString.Empty;

	public ZString DocumentTitle => responseData?.DocumentTitle ?? ZString.Empty;

	public ZString DocumentVersion => responseData?.DocumentVersion ?? ZString.Empty;

	public ZDateTime DocumentDateTime => responseData?.DocumentDateTime.DateTime ?? ZDateTime.Empty;

	protected override ZString DocumentFilenameCore => $"e-dec_receiptResponse_receipt_{responseData.DocumentType}_{responseData.DocumentNumber}_{responseData.DocumentVersion}_{responseData.RequestorTraderIdentificationNumber}";

	public ZString CustomsOfficeName => responseData?.CustomsOfficeName ?? ZString.Empty;

	public ZString CustomsOfficeNumber => responseData?.CustomsOfficeNumber ?? ZString.Empty;

	public ZString CustomsOfficePhoneNumber => responseData?.CustomsOfficePhoneNumber ?? ZString.Empty;

	public ZString CustomsOfficeAddressSupplement1 => responseData?.CustomsOfficeAddressSupplement1 ?? ZString.Empty;

	public ZString CustomsOfficeAddressSupplement2 => responseData?.CustomsOfficeAddressSupplement2 ?? ZString.Empty;

	public ZString CustomsOfficeStreet => responseData?.CustomsOfficeStreet ?? ZString.Empty;

	public ZString CustomsOfficePostalCode => responseData?.CustomsOfficePostalCode ?? ZString.Empty;

	public ZString CustomsOfficeCity => responseData?.CustomsOfficeCity ?? ZString.Empty;

	public ZString CustomsOfficeCountry => responseData?.CustomsOfficeCountry ?? ZString.Empty;
}
