using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMMessageHeader : IMessageHeader
{
	public IMMessageHeader(IIMHeader iMHeader, ISadMessageSendingObject sadMessageSendingObject)
	{
		this.iMHeader = Argument.NotNull(iMHeader, nameof(iMHeader));
		this.sadMessageSendingObject = Argument.NotNull(sadMessageSendingObject, nameof(sadMessageSendingObject));
	}

	readonly IIMHeader iMHeader;
	readonly ISadMessageSendingObject sadMessageSendingObject;

	[MessageLayout(Order = 0)]
	public ISadMessageFixedPart FixedPart => SadMessageFixedPartFactory.GetSadMessageFixedPart
		(isHeader: true
		, sadMessageSendingObject: sadMessageSendingObject
		, messageCode: SadMessageFixedPart.Constants.MessageCode.IM.Header
		, annualProgressiveNumber: iMHeader.AnnualProgressiveNumber
		, progressiveNumber: 0);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 6, false)]
	[MessageFieldImportRules("D", "CN91")]
	[MessageFieldDepositoRules("D", "CN91")]
	public ZString AuthorizationNo => iMHeader.AuthorizationNo;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, true)]
	[MessageFieldImportRules("D", "CN91")]
	[MessageFieldDepositoRules("D", "CN91")]
	public ZString AuthorizationCIN => iMHeader.AuthorizationCIN;

	[MessageLayout(Order = 3)]
	public IMHeaderCompanyRegister CompanyRegister => new IMHeaderCompanyRegister(iMHeader.CompanyRegister);

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 8, false)]
	public ZString NoticeNumber => ZString.Empty;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 8, false)]
	public ZString NoticeOrderNumber => ZString.Empty;

	[MessageLayout(Order = 6)]
	[MessageFieldDateDDMMYYRepresentation]
	public ZDate NoticeDate => ZDate.Empty;

	[MessageLayout(Order = 7)]
	[MessageFieldBoolRepresentation]
	[MessageFieldImportRules("R", "R230")]
	[MessageFieldDepositoRules("R", "R230")]
	public ZBool PreClearing => iMHeader.PreClearing;

	[MessageLayout(Order = 8)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public IMHeaderDeclaration Declaration => new IMHeaderDeclaration(iMHeader.Declaration);

	[MessageLayout(Order = 9)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldImportRules("D", "CN8")]
	[MessageFieldDepositoRules("D", "CN8")]
	public ZDate AcceptanceDate => iMHeader.AcceptanceDate;

	[MessageLayout(Order = 10)]
	[MessageFieldBoolRepresentation]
	public ZBool? HeaderDataDeclaredAtArticleLevel => null;

	[MessageLayout(Order = 11)]
	[MessageFieldIntegerRepresentation(5, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZInt TotalItems => iMHeader.TotalItems;

	[MessageLayout(Order = 12)]
	[MessageFieldIntegerRepresentation(7, false)]
	public ZInt? TotalNumberOfPacks => null;

	[MessageLayout(Order = 13)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("D", "CN97")]
	public IMHeaderConsignor Consignor => new IMHeaderConsignor(iMHeader.Consignor);

	[MessageLayout(Order = 14)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 22, false)]
	public ZString ReferenceNumber => ZString.Empty;

	[MessageLayout(Order = 15)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("D", "CN98")]
	public IMHeaderConsignee Consignee => new IMHeaderConsignee(iMHeader.Consignee);

	[MessageLayout(Order = 16)]
	[MessageFieldDecimalRepresentation(14, 2, false, true)]
	[MessageFieldImportRules("O")]
	public ZDecimal? DeliveryCosts => iMHeader.DeliveryCosts;

	[MessageLayout(Order = 17)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public IMHeaderDeclarantTrader Trader => new IMHeaderDeclarantTrader(iMHeader.DeclarantTrader);

	[MessageLayout(Order = 18)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("D", "CN98")]
	public ZString CountryOfDispatch => iMHeader.CountryOfDispatch;

	[MessageLayout(Order = 19)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("D", "CN97")]
	public ZString CountryOfDestination => iMHeader.CountryOfDestination;

	[MessageLayout(Order = 20)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("O")]
	public ZString ProvinceOfDestination => iMHeader.ProvinceOfDestination;

	[MessageLayout(Order = 21)]
	[MessageFieldImportRules("O")]
	public IMHeaderMeansOfTransportOnArrival MeansOfTransportOnArrival => new IMHeaderMeansOfTransportOnArrival(iMHeader.MeansOfTransportOnArrival);

	[MessageLayout(Order = 22)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldImportRules("R")]
	public ZBool? IsContainerizedTransport => iMHeader.IsContainerizedTransport;

	[MessageLayout(Order = 23)]
	[MessageFieldImportRules("R")]
	public IMHeaderTermOfDeliveryGroup TermsOfDelivery => new IMHeaderTermOfDeliveryGroup(iMHeader.TermsOfDelivery);

	[MessageLayout(Order = 24)]
	[MessageFieldImportRules("D", "C10")]
	public IMHeaderMeansOfTransportCrossingBorder MeansOfTransportCrossingBorder => new IMHeaderMeansOfTransportCrossingBorder(iMHeader.MeansOfTransportCrossingBorder);

	[MessageLayout(Order = 25)]
	[MessageFieldImportRules("R")]
	public IMHeaderTransactionData TransactionData => new IMHeaderTransactionData(iMHeader.TransactionData);

	[MessageLayout(Order = 26)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldImportRules("O")]
	public ZString TransportModeAtBorder => iMHeader.TransportModeAtBorder;

	[MessageLayout(Order = 27)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldImportRules("O")]
	public ZString InlandTransportMode => iMHeader.InlandTransportMode;

	[MessageLayout(Order = 28)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 17, false)]
	public ZString GoodsPlaceOfLoading => ZString.Empty;

	[MessageLayout(Order = 29)]
	[MessageFieldImportRules("R")]
	public IMHeaderEntryCustomsOffice EntryCustomsOffice => new IMHeaderEntryCustomsOffice(iMHeader.EntryCustomsOffice);

	[MessageLayout(Order = 30)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public IMHeaderLocationOfGoods LocationOfGoods => new IMHeaderLocationOfGoods(iMHeader.LocationOfGoods);

	[MessageLayout(Order = 31)]
	[MessageFieldImportRules("D", "C558")]
	[MessageFieldDepositoRules("D", "C558")]
	public IMHeaderDeferredPayment DeferredPayment => new IMHeaderDeferredPayment(iMHeader.DeferredPayment);

	[MessageLayout(Order = 32)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("R")]
	public IMHeaderWarehouseIdentification WarehouseIdentification => new IMHeaderWarehouseIdentification(iMHeader.WarehouseIdentification);

	[MessageLayout(Order = 33)]
	[MessageFieldImportRules("D", "CN4")]
	[MessageFieldDepositoRules("D", "CN4")]
	[MessageFieldDateDDMMYYYYRepresentation()]
	public ZDate DateLimitOfTemporaryOperation => iMHeader.DateLimitOfTemporaryOperation;

	[MessageLayout(Order = 34)]
	public IMHeaderPrincipalTrader PrincipalTrader => new IMHeaderPrincipalTrader();

	[MessageLayout(Order = 35)]
	public IMHeaderTransitCustomsOfficeCollection TransitCustomsOffices => new IMHeaderTransitCustomsOfficeCollection();

	[MessageLayout(Order = 36)]
	public IMHeaderGuaranteeCollection Guarantees => new IMHeaderGuaranteeCollection();

	[MessageLayout(Order = 37)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	public ZString DestinationCustomsOffice => ZString.Empty;

	[MessageLayout(Order = 38)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 16, false)]
	public ZString AuthorizedConsigneeId => ZString.Empty;

	[MessageLayout(Order = 39)]
	public IMHeaderSealCollection Seals => new IMHeaderSealCollection();

	[MessageLayout(Order = 40)]
	public IMHeaderControlResult ControlResult => new IMHeaderControlResult();
}
