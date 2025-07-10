using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETMessageLine : IMessageContinuation
{
	public ETMessageLine(IETLine iETLine, IETHeader iETHeader, ISadMessageSendingObject sadMessageSendingObject)
	{
		this.iETLine = Argument.NotNull(iETLine, nameof(iETLine));
		this.iETHeader = Argument.NotNull(iETHeader, nameof(iETHeader));
		this.sadMessageSendingObject = Argument.NotNull(sadMessageSendingObject, nameof(sadMessageSendingObject));
	}

	readonly IETLine iETLine;
	readonly IETHeader iETHeader;
	readonly ISadMessageSendingObject sadMessageSendingObject;

	[MessageLayout(Order = 0)]
	public ISadMessageFixedPart FixedPart => SadMessageFixedPartFactory.GetSadMessageFixedPart
		(isHeader: false
		, sadMessageSendingObject: sadMessageSendingObject
		, messageCode: SadMessageFixedPart.Constants.MessageCode.ET.Continuation
		, annualProgressiveNumber: iETHeader.AnnualProgressiveNumber
		, progressiveNumber: 0);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	[MessageFieldExportWithTransitRules("D", "C45")]
	[MessageFieldTransitRules("D", "C45")]
	public ZString DeclarationType => iETLine.DeclarationType;

	[MessageLayout(Order = 2)]
	[MessageFieldExportRules("D", "R10")]
	[MessageFieldExportWithTransitRules("D", "R10")]
	[MessageFieldTransitRules("D", "R10")]
	[MessageFieldInternationalRoadTransportsRules("D", "R10")]
	public ETLineConsignor Consignor => new ETLineConsignor(iETLine.Consignor);

	[MessageLayout(Order = 3)]
	[MessageFieldExportRules("O", "R11")]
	[MessageFieldExportWithTransitRules("O", "R11")]
	[MessageFieldTransitRules("D", "R11", "C1")]
	[MessageFieldInternationalRoadTransportsRules("D", "R11", "C1")]
	public ETLineConsignee Consignee => new ETLineConsignee(iETLine.Consignee);

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("D", "C578")]
	[MessageFieldExportWithTransitRules("D", "C135")]
	[MessageFieldTransitRules("D", "C135")]
	public ZString DispatchCountryCode => iETLine.DispatchCountryCode;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("D", "C568", "TR9121")]
	[MessageFieldExportWithTransitRules("D", "C140")]
	[MessageFieldTransitRules("D", "C140")]
	[MessageFieldInternationalRoadTransportsRules("D", "C140")]
	public ZString DestinationCountryCode => iETLine.DestinationCountryCode;

	[MessageLayout(Order = 6)]
	[MessageFieldExportWithTransitRules("D", "C186", "C187")]
	[MessageFieldTransitRules("D", "C186", "C187")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C187")]
	public ETLineSecurityBlockConsignor SecurityBlockConsignor => new ETLineSecurityBlockConsignor(iETLine.SecurityBlock.Consignor);

	[MessageLayout(Order = 7)]
	[MessageFieldExportWithTransitRules("D", "C186", "C188")]
	[MessageFieldTransitRules("D", "C186", "C188")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C188")]
	public ETLineSecurityBlockConsignee SecurityBlockConsignee => new ETLineSecurityBlockConsignee(iETLine.SecurityBlock.Consignee);

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("D", "C186")]
	[MessageFieldTransitRules("D", "C186")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186")]
	public ZString TransportChargesMethodOfPayment => iETLine.SecurityBlock.TransportChargesMethodOfPayment;

	[MessageLayout(Order = 9)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 70, false)]
	[MessageFieldExportRules("D", "C567", "R876")]
	[MessageFieldExportWithTransitRules("D", "C186", "C547", "R876")]
	[MessageFieldTransitRules("D", "C186", "C547", "R876")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186", "C547", "R876")]
	public ZString CommercialReferenceNumber => iETLine.SecurityBlock.CommercialReferenceNumber;

	[MessageLayout(Order = 10)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ETLinePackageCollection Packages => new ETLinePackageCollection(iETLine.Packages);

	[MessageLayout(Order = 11)]
	[MessageFieldExportRules("D", "C55")]
	[MessageFieldExportWithTransitRules("D", "C55")]
	[MessageFieldTransitRules("D", "C55")]
	[MessageFieldInternationalRoadTransportsRules("D", "C55")]
	public ETLineContainerCollection Containers => new ETLineContainerCollection(iETLine.Containers);

	[MessageLayout(Order = 12)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 280, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString GoodsDescription => iETLine.GoodsDescription;

	[MessageLayout(Order = 13)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString GoodsDescriptionLng => ZString.Empty;

	[MessageLayout(Order = 14)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	public ZString SensitiveGoodsCode => ZString.Empty;

	[MessageLayout(Order = 15)]
	[MessageFieldDecimalRepresentation(11, 3, false)]
	public ZDecimal? SensitiveGoodsQuantity => null;

	[MessageLayout(Order = 16)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("D", "C186")]
	[MessageFieldTransitRules("D", "C186")]
	[MessageFieldInternationalRoadTransportsRules("D", "C186")]
	public ZString UNDangerousGoodsCode => iETLine.SecurityBlock.UNDangerousGoodsCode;

	[MessageLayout(Order = 17)]
	[MessageFieldIntegerRepresentation(3, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZInt ItemNumber => iETLine.ItemNumber;

	[MessageLayout(Order = 18)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString CombinedNomenclature => iETLine.CombinedNomenclature;

	[MessageLayout(Order = 19)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	public ZString TaricCode => ZString.Empty;

	[MessageLayout(Order = 20)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ETLineAdditionalCodeCollection AdditionalCodes => new ETLineAdditionalCodeCollection(iETLine.AdditionalCodes);

	[MessageLayout(Order = 21)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("D", "CN34")]
	[MessageFieldExportWithTransitRules("D", "CN34")]
	[MessageFieldTransitRules("D", "CN34")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN34")]
	public ZString CountryOfOrigin => iETLine.CountryOfOrigin;

	[MessageLayout(Order = 22)]
	[MessageFieldDecimalRepresentation(13, 5, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZDecimal GrossMass => iETLine.GrossMass;

	[MessageLayout(Order = 23)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 4, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Procedure => iETLine.Procedure;

	[MessageLayout(Order = 24)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	public ETLineNationalProcedureCollection NationalProcedures => new ETLineNationalProcedureCollection(iETLine.NationalProcedures);

	[MessageLayout(Order = 25)]
	[MessageFieldDecimalRepresentation(13, 5, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("O", "C91")]
	[MessageFieldInternationalRoadTransportsRules("O", "C91")]
	public ZDecimal? NetMass => iETLine.NetMass;

	[MessageLayout(Order = 26)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("D", "CN90")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN90")]
	public ETLinePreviousDocument PreviousAdministrativeDocument => new ETLinePreviousDocument(iETLine.PreviousAdministrativeDocument);

	[MessageLayout(Order = 27)]
	[MessageFieldDecimalRepresentation(13, 5, false)]
	[MessageFieldExportRules("D", "CN11")]
	[MessageFieldExportWithTransitRules("D", "CN11")]
	[MessageFieldTransitRules("D", "CN11")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN11")]
	public ZDecimal? SupplementaryUnit => iETLine.SupplementaryUnit;

	[MessageLayout(Order = 28)]
	public ETLineSpecialMentionGroup SpecialMentions => new ETLineSpecialMentionGroup(iETLine.SpecialMentionGroup);

	[MessageLayout(Order = 29)]
	[MessageFieldExportRules("D", "C567")]
	[MessageFieldExportWithTransitRules("D", "C547")]
	[MessageFieldTransitRules("D", "C547")]
	[MessageFieldInternationalRoadTransportsRules("R", "C903")]
	public ETLineCertificateCollection Certificates => new ETLineCertificateCollection(iETLine.Certificates);

	[MessageLayout(Order = 30)]
	public ETLineSpecialMentionInfoAdditionalInformation AdditionalInformation => new ETLineSpecialMentionInfoAdditionalInformation(iETLine.SpecialMentionGroup.AdditionalInformation);

	[MessageLayout(Order = 31)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 26, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString ComplementOfInformation => iETLine.ComplementOfInformation;

	[MessageLayout(Order = 32)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString ComplementOfInformationLng => iETLine.ComplementOfInformationLng;

	[MessageLayout(Order = 33)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 500, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString Notes => iETLine.Notes;

	[MessageLayout(Order = 34)]
	[MessageFieldDecimalRepresentation(17, 2, false, true)]
	[MessageFieldExportRules("R", "C28", "TRN0001")]
	[MessageFieldExportWithTransitRules("R", "C28", "TRN0001")]
	public ZDecimal? StatisticalValueAmount => iETLine.StatisticalValueAmount;

	[MessageLayout(Order = 35)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	public ZString StatisticalValueCurrency => ZString.Empty;

	[MessageLayout(Order = 36)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ETLineDutyCollection Duties => new ETLineDutyCollection(iETLine.Duties);

	[MessageLayout(Order = 37)]
	[MessageFieldDecimalRepresentation(15, 2, false, true)]
	[MessageFieldExportRules("D", "C559")]
	[MessageFieldExportWithTransitRules("D", "C559")]
	[MessageFieldTransitRules("D", "C559")]
	[MessageFieldInternationalRoadTransportsRules("D", "C559")]
	public ZDecimal? TotalItemTaxesAmount => iETLine.TotalItemTaxedAmount;

	[MessageLayout(Order = 38)]
	[MessageFieldDecimalRepresentation(15, 2, false, true)]
	[MessageFieldExportRules("D", "C560", "CN14")]
	[MessageFieldExportWithTransitRules("D", "C560", "CN14")]
	[MessageFieldTransitRules("D", "C560", "CN14")]
	[MessageFieldInternationalRoadTransportsRules("D", "C560", "CN14")]
	public ZDecimal? GrandTotalTaxesAmount => iETLine.GrandTotalTaxedAmount;
}
