using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.BE.Business;

[CodeAlive("Provider will be called from message builder")]
public class CC515CDataProvider : AESMessageHeaderProvider, ICC515CDataProvider
{
	public CC515CDataProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
		this.instruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader) + "." + nameof(entryHeader.EntryInstruction));
	}

	readonly CusEntryInstruction instruction;

	public override string MessageType => Constants.BECMessageTypes.Outgoing.CC515C;

	public IReadOnlyCollection<IAuthorization> Authorizations => authorizations ?? (authorizations =
		instruction.CusAuthorizationUsages
			.Select((x, i) => new AuthorizationProvider(x, i + 1)).ToArray<IAuthorization>());
	IReadOnlyCollection<IAuthorization> authorizations;

	public string CustomsOfficeOfPresentationReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfPresentationReferenceNumber, () =>
		declaration.CustomsOffices.OfType<EU.Business.EuOfficeCode>().FirstOrDefault(o => o.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation && !o.CY_Data.IsEmpty)?.CY_Data
		?? string.Empty);
	CachedValue<string> customsOfficeOfPresentationReferenceNumber;

	public string CustomsOfficeOfExportReferenceNumber => declaration.JE_CustomsOffice;

	public string CustomsOfficeOfExitDeclaredReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfExitDeclaredReferenceNumber, () =>
		declaration.CustomsOffices.OfType<EU.Business.EuOfficeCode>().FirstOrDefault(o => o.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit && !o.CY_Data.IsEmpty)?.CY_Data
		?? string.Empty);
	CachedValue<string> customsOfficeOfExitDeclaredReferenceNumber;

	public string SupervisingCustomsOfficeReferenceNumber => CachedValueHelper.GetValue(ref supervisingCustomsOfficeReferenceNumber, () =>
		declaration.CustomsOffices.OfType<EU.Business.EuOfficeCode>().FirstOrDefault(o => o.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.SupervisingOffice && !o.CY_Data.IsEmpty)?.CY_Data
		?? string.Empty);
	CachedValue<string> supervisingCustomsOfficeReferenceNumber;

	public IParty Exporter => exporter ?? (exporter = new PartyProvider(declaration.ExporterDocAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty exporter;

	public IParty Declarant => declarant ?? (declarant = new PartyWithContactProvider(declaration.DeclarantAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty declarant;

	public IAESRepresentative Representative => representative ?? (representative = RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType));
	IAESRepresentative representative;

	public ICurrencyExchange CurrencyExchange => currencyExchange ?? (currencyExchange = declaration.Invoices.Count > 0 ? new CurrencyExchangeProvider(declaration.Invoices[0]) : null);
	ICurrencyExchange currencyExchange;

	public string DeferredPayment => declaration.JE_DefermentAccountNumber;

	public IGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = new GoodsShipmentProvider(entryHeader));
	IGoodsShipment goodsShipment;
}
