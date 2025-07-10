using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE426;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU.ImportDeclarationDocument;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument;

public class IDD426Wrapper : DocBaseWrapper, IIDD<IDD426ItemWrapper, IDD426LiquidationWrapper, IDD426DutiesAndTaxWrapper>
{
	public IDD426Wrapper(DeltaIEFREDIMessage message, BusinessObjectFactory factory) : base(message, factory)
	{
		this.message = Argument.NotNull(message, nameof(message));
		this.messageObject = (message.MessageDataObject as IE426MessageDataObject)?.ResponseMessage;
	}

	public static IDD426Wrapper New(DeltaIEFREDIMessage message)
	{
		var messageObject = (message.MessageDataObject as IE426MessageDataObject)?.ResponseMessage;

		return messageObject != null && message.EM_LinkedObject is CusEntryHeader entryHeader
			? new IDD426Wrapper(message, message.Factory)
			: null;
	}

	readonly CC426BType messageObject;
	readonly DeltaIEFREDIMessage message;

	public CusEntryHeader EntryHeader => message.EM_LinkedObject as CusEntryHeader;

	public JobDeclaration Declaration => EntryHeader.Declaration;

	public ZString SheetName => EntryHeader.MovementReferenceNumber.IsEmpty ? EntryHeader.CH_BGMReference : EntryHeader.MovementReferenceNumber;

	public ZString PrintDateTime => ZDateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

	public ZString DeclarationStatus => messageObject.DeclarationStatus.State == "BONAENLEVER" ? (NoResString)"BON A ENLEVER" : messageObject.DeclarationStatus.State;

	public ZString SupervisingCustomsOffice => messageObject.SupervisingCustomsOffice?.ReferenceNumber ?? ZString.Empty;

	protected MGoodsShipmentType07FR GoodsShipment => messageObject.GoodsShipment;

	public ZString TotalItemsCount => (GoodsShipment?.GoodsShipmentItem?.Count ?? 0).ToString();

	public ZString AgreementNumber => GoodsShipment?.AdditionalReference?.FirstOrDefault(x => x.Type == "1DEC")?.ReferenceNumber ?? ZString.Empty;

	public ZString DeclarantName => messageObject.Declarant?.Name ?? ZString.Empty;

	public ZString DeclarantEORI => messageObject.Declarant?.IdentificationNumber ?? ZString.Empty;

	public ZString DeclarantAddress => messageObject.Declarant?.Address?.StreetAndNumber ?? ZString.Empty;

	public ZString DeclarantPostCode => messageObject.Declarant?.Address?.Postcode ?? ZString.Empty;

	public ZString DeclarantCity => messageObject.Declarant?.Address?.City ?? ZString.Empty;

	public ZString DeclarantCountry => messageObject.Declarant?.Address?.Country ?? ZString.Empty;

	public ZString RepresentativeEORI => messageObject.Representative?.IdentificationNumber ?? ZString.Empty;

	public ZString RepresentativeType => messageObject.Representative?.Status ?? ZString.Empty;

	public ZString RepresentativeContactName => messageObject.Representative?.ContactPerson?.Name ?? ZString.Empty;

	public ZString RepresentativeContactPhoneNumber => messageObject.Representative?.ContactPerson?.PhoneNumber ?? ZString.Empty;

	public ZString RepresentativeContactEMail => messageObject.Representative?.ContactPerson?.EMailAddress ?? ZString.Empty;

	public ZString DeclarantContactName => messageObject.Declarant?.ContactPerson?.Name ?? ZString.Empty;

	public ZString DeclarantContactPhoneNumber => messageObject.Declarant?.ContactPerson?.PhoneNumber ?? ZString.Empty;

	public ZString DeclarantContactEMail => messageObject.Declarant?.ContactPerson?.EMailAddress ?? ZString.Empty;

	public ZString AdditionalReferences
	{
		get
		{
			var result = ZString.Empty;
			if (!(GoodsShipment?.AdditionalReference).IsNullOrEmpty())
			{
				result = string.Join(" - ", GoodsShipment.AdditionalReference.Select(p => string.Join(" ", p.Type, p.ReferenceNumber)));
			}

			return result;
		}
	}

	public ZString CRN => messageObject.ImportOperation?.CustomsRegistrationNumber ?? ZString.Empty;

	public DocBaseWrapperCollection<IDD426ItemWrapper> Items => items ??= GetItems();
	DocBaseWrapperCollection<IDD426ItemWrapper> items;

	DocBaseWrapperCollection<IDD426ItemWrapper> GetItems()
	{
		int i = 0;
		var result = new IDDGoodsShipmentWrapper<IDD426ItemWrapper>(Factory);
		GoodsShipment?.GoodsShipmentItem?.ForEach(item => result.Add(IDD426ItemWrapper.New(++i, item, messageObject.DetailedTaxation?.GoodsShipment?.GoodsShipmentItem?.FirstOrDefault(itemFromDetailedTaxation => itemFromDetailedTaxation.DeclarationGoodsItemNumber.Equals(item.DeclarationGoodsItemNumber)), messageObject.GeneralTaxation?.DutiesAndTaxesSummaries, Factory)));
		return result;
	}

	public ZString ItemsCount => Items != null ? Items.Count.ToString() : "0";

	public ZString DeclarationAcceptanceDateTime => ZString.Empty;

	public ZString ReleaseDateTime => ZString.Empty;

	public ZString RelatedRelaseDateTime => ZString.Empty;

	public ZString DeclarationType => ZString.Empty;

	public ZString AdditionalDeclarationType => ZString.Empty;

	public ZString PresentationCustomsOffice => ZString.Empty;

	public ZString GoodsLocationType => ZString.Empty;

	public ZString GoodsLocationAddress => ZString.Empty;

	public ZString GoodsLocationPostCode => ZString.Empty;

	public ZString GoodsLocationCity => ZString.Empty;

	public ZString GoodsLocationCountry => ZString.Empty;

	public ZString SupplierName => ZString.Empty;

	public ZString SupplierEORI => ZString.Empty;

	public ZString SupplierAddress => ZString.Empty;

	public ZString SupplierPostCode => ZString.Empty;

	public ZString SupplierCity => ZString.Empty;

	public ZString SupplierCountry => ZString.Empty;

	public ZString ImporterName => ZString.Empty;

	public ZString ImporterEORI => ZString.Empty;

	public ZString ImporterAddress => ZString.Empty;

	public ZString ImporterPostCode => ZString.Empty;

	public ZString ImporterCity => ZString.Empty;

	public ZString ImporterCountry => ZString.Empty;

	public ZString RepresentativeName => ZString.Empty;

	public ZString ConsigneeName => ZString.Empty;

	public ZString ConsigneeEori => ZString.Empty;

	public ZString SupplyChainActorName => ZString.Empty;

	public ZString SupplyChainActorEORI => ZString.Empty;

	public ZString SupplyChainActorRole => ZString.Empty;

	public ZString AuthorizationType => ZString.Empty;

	public ZString AuthorizationNumber => ZString.Empty;

	public ZString AuthorizationHolder => ZString.Empty;

	public ZString CountryOfDestination => ZString.Empty;

	public ZString CountryOfDispatch => ZString.Empty;

	public ZString IncotermCode => ZString.Empty;

	public ZString IncotermUNLOCO => ZString.Empty;

	public ZString IncotermLocation => ZString.Empty;

	public ZString IncotermCountry => ZString.Empty;

	public ZString IncotermText => ZString.Empty;

	public ZString WarehouseType => ZString.Empty;

	public ZString WarehouseLocation => ZString.Empty;

	public ZString TotalGrossMass => ZString.Empty;

	public ZString IsContainerised => ZString.Empty;

	public ZString ContainerId => ZString.Empty;

	public ZString TransportMeansNationalityAtBorder => ZString.Empty;

	public ZString InlandModeOfTransport => ZString.Empty;

	public ZString ModeOfTransportAtTheBorder => ZString.Empty;

	public ZString IsActiveAtBorder => ZString.Empty;

	public ZString ArrivalTransportMeansIdType => ZString.Empty;

	public ZString SpecialMentions => ZString.Empty;

	public ZString FiscalReferences => ZString.Empty;

	public ZString PreviousDocuments => ZString.Empty;

	public ZString SupportingDocuments => ZString.Empty;

	public ZString TransportDocuments => ZString.Empty;

	public ZString GuaranteeNumber => ZString.Empty;

	public ZString GuaranteeOffice => ZString.Empty;

	public ZString TotalInvoicedAmount => ZString.Empty;

	public ZString TotalInvoicedCurrency => ZString.Empty;

	public ZString GuaranteedAmount => ZString.Empty;

	public ZString UnguaranteedAmount => ZString.Empty;

	public ZString TotalPayableTaxAmount => ZString.Empty;

	public ZString AmountToBeCovered => ZString.Empty;

	public ZString ModeOfPayment => ZString.Empty;

	public ZString MRN => ZString.Empty;

	public ZString LRN => messageObject.ImportOperation.LRN;

	public ZString NatureOfTransaction => ZString.Empty;

	public ZBool IsImputationSheetsCollectionAvailable => false;

	public ZString ReverseChargeVAT => ZString.Empty;

	public ZString VATAmountAI2 => ZString.Empty;
}
