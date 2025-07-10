using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

sealed class CC513CDataProvider : AESMessageHeaderProvider, ICC513CDataProvider
{
	public CC513CDataProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	public override string MessageType => Constants.BECMessageTypes.Outgoing.CC513C;

	public string CustomsOfficeOfPresentationReferenceNumber => customsOfficeOfPresentation ?? ((declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfPresentation) is EuOfficeCode office) ? customsOfficeOfPresentation = office.CY_Data : null);
	string customsOfficeOfPresentation;

	public string CustomsOfficeOfExportReferenceNumber => customsOfficeOfExport ?? (customsOfficeOfExport = declaration.JE_CustomsOffice);
	string customsOfficeOfExport;

	public string CustomsOfficeOfExitReferenceNumber => customsOfficeOfExit ?? (declaration.CustomsOffices.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfExit) is EuOfficeCode office ? customsOfficeOfExit = office.CY_Data : null);
	string customsOfficeOfExit;

	public string DeferredPayment => deferredPayment ?? (deferredPayment = declaration.JE_DefermentAccountNumber);
	string deferredPayment;

	public IReadOnlyCollection<IAuthorization> Authorisation => authorizations ?? (authorizations = entryHeader.EntryInstruction.CusAuthorizationUsages.Cast<EU.Business.CusAuthorizationUsage>().Select((x, i) => new AuthorizationProvider(x, i + 1)).ToArray<IAuthorization>());
	IReadOnlyCollection<IAuthorization> authorizations;

	public IParty Exporter => CachedValueHelper.GetValue(ref exporter, () => new PartyProvider(declaration.ExporterDocAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	CachedValue<IParty> exporter;

	public IParty Declarant => declarant ?? (declarant = new PartyWithContactProvider(declaration.DeclarantAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty declarant;

	public IAESRepresentative Representative => representative ?? (representative = RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IAESRepresentative representative;

	public ICurrencyExchange CurrencyExchange => currencyExchange ?? (currencyExchange = declaration.Invoices.Count > 0 ? new CurrencyExchangeProvider(declaration.Invoices[0]) : null);
	ICurrencyExchange currencyExchange;

	public IConsignment Consignment => null;

	public IGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = new GoodsShipmentProvider(entryHeader));
	IGoodsShipment goodsShipment;
}
