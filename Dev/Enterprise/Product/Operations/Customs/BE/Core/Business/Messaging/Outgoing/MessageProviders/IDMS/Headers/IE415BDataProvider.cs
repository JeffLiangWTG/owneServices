using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.BE.Business;

[CodeAlive("Provider will be called from message builder")]
public class IE415BDataProvider : IMPMessageHeaderProvider, IIe415BDataProvider
{
	public IE415BDataProvider(BEJobDeclarationMessageSendingObject messageSendingAction) : base(messageSendingAction)
	{
	}

	public override string MessageType => Constants.BECMessageTypes.Outgoing.IE415B;

	public IGoodsShipment GoodsShipment => goodsShipment ??= new GoodsShipmentProvider(entryHeader);
	IGoodsShipment goodsShipment;

	public IImportOperation ImportOperation => importOperation ??= new ImportOperationProvider(entryHeader);
	IImportOperation importOperation;

	public IReadOnlyCollection<IAuthorization> Authorisations => authorizations ??= entryHeader.EntryInstruction.CusAuthorizationUsages.Select((x, i) => new AuthorizationProvider(x, i + 1)).ToArray<IAuthorization>();
	IReadOnlyCollection<IAuthorization> authorizations;

	public string CustomsOfficeOfPresentationReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfPresentationReferenceNumber, () => declaration.CustomsOffices.Where(co => co.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation).FirstOrDefault()?.CY_Data);
	CachedValue<string> customsOfficeOfPresentationReferenceNumber;

	public string SupervisingCustomsOfficeReferenceNumber => CachedValueHelper.GetValue(ref supervisingCustomsOfficeReferenceNumber, () => declaration.CustomsOffices.Where(o => o.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.SupervisingCustomsOffice && !o.CY_Data.IsEmpty).FirstOrDefault()?.CY_Data);
	CachedValue<string> supervisingCustomsOfficeReferenceNumber;

	public IParty Importer => importer ??= new PartyProvider(declaration.ImporterAddress);
	IParty importer;

	public IParty Declarant => declarant ??= new PartyWithContactProvider(declaration.Declarant);
	IParty declarant;

	public string PersonProvidingAGuaranteeIdentificationNumber => CachedValueHelper.GetValue(ref personProvidingAGuaranteeIdentificationNumber, () => declaration.DefermentPartyDocAddress?.Address?.GetEORI());
	CachedValue<string> personProvidingAGuaranteeIdentificationNumber;

	public string PersonPayingCustomsDutyIdentificationNumber => CachedValueHelper.GetValue(ref personPayingCustomsDutyIdentificationNumber, () => declaration.DutyPayer?.GetEORI());
	CachedValue<string> personPayingCustomsDutyIdentificationNumber;

	public IParty Representative => new PartyWithContactProvider(declaration.Representative);
	public IParty representative;

	public IReadOnlyCollection<IGuarantee> Guarantees => guarantees ??= entryHeader.EntryInstruction?.Guarantees.Cast<CusBondDetail>().Select(g => g.PW_BondType).Distinct().Select((bondType, i) => new GuaranteeProvider(entryHeader.EntryInstruction, i + 1, bondType)).ToArray<IGuarantee>();
	IReadOnlyCollection<IGuarantee> guarantees;

	public string InternalCurrencyUnit => CachedValueHelper.GetValue(ref internalCurrencyUnit, () => declaration.Invoices.FirstOrDefault()?.JZ_RX_NKInvoice_Currency);
	CachedValue<string> internalCurrencyUnit;

	public IReadOnlyCollection<IDeferredPayment> DeferredPayments => deferredPayments ??= declaration.JE_DefermentAccountNumber.IsEmpty ? Array.Empty<IDeferredPayment>() : new IDeferredPayment[] { new DeferredPaymentProvider(1, declaration.JE_DefermentAccountNumber) };
	IReadOnlyCollection<IDeferredPayment> deferredPayments;

	public IPostalCharges PostalCharges => throw new NotImplementedException();
}
