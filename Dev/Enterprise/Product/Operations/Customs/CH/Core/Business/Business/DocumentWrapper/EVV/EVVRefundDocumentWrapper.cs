using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public sealed class EVVRefundDocumentWrapper : GenericEVVDocumentWrapper<IEvvRefundProvider>
{
	public static EVVRefundDocumentWrapper New(ICHEDIMessage message, BusinessObjectFactory factory) => new EVVRefundDocumentWrapper(Argument.NotNull(message, nameof(message)), factory);

	EVVRefundDocumentWrapper(ICHEDIMessage message, BusinessObjectFactory factory) : base(message, factory)
	{
		responseData = message.MessageDetail as IEvvRefundProvider;
	}

	public ZString BordereauNumber => responseData?.BordereauNumber ?? ZString.Empty;

	public ZString CustomsOfficer => responseData?.PersonInCharge ?? ZString.Empty;

	public ZString CustomsReference => responseData?.CustomsReference ?? ZString.Empty;

	public ZString TraderReference => responseData?.TraderReference ?? ZString.Empty;

	public ZString VATNumber => responseData?.VATNumber ?? ZString.Empty;

	public ZString VATSuffix => DocumentWrapperHelper.GetVATSuffixText(responseData?.VATSuffix ?? false);

	public ZString AccountNumber => responseData?.Account?.Number ?? ZString.Empty;

	public ZString AccountName => responseData?.Account?.Name ?? ZString.Empty;

	public ZString AccountHolderLine1 => GetAccountHolderLine(0);

	public ZString AccountHolderLine2 => GetAccountHolderLine(1);

	public ZString AccountHolderLine3 => GetAccountHolderLine(2);

	ZString GetAccountHolderLine(int index)
	{
		if (accountHolderLines == null)
		{
			accountHolderLines = (responseData?.AccountHolderAddressLines ?? Enumerable.Empty<string>()).ToArray();
		}
		return index < accountHolderLines.Length ? accountHolderLines[index] : null ?? ZString.Empty;
	}
	string[] accountHolderLines;

	public ZString CorrectionReason => correctionReason ??= RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.EdecTypes.CorrectionReason, ZDateTime.Now, languageCode: DocumentLanguage.GetLanguageCode()).GetDescriptionFromCode(responseData?.CorrectionReason);
	ZString? correctionReason;

	public ZDecimal TotalAmount => (responseData?.TotalAmount ?? ZDecimal.Zero);

	public ZDecimal VATChargedTotalAmount => (responseData.AmountNotCharged ? 0 : TotalAmount);

	public EVVGoodsItemDutyAndTaxesWrapperCollection DutyAndTaxes => dutyAndTaxes ??= EVVGoodsItemDutyAndTaxesWrapperCollection.New(responseData?.DutyAndTaxes, Factory, DocumentLanguage);
	EVVGoodsItemDutyAndTaxesWrapperCollection dutyAndTaxes;

	public EVVLegalAdvisoryWrapper LegalAdvisor => EVVLegalAdvisoryWrapper.New(responseData?.LegalAdvisor, Factory);
}
