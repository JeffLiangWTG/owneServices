using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using Enterprise.Customs.IT.Messaging.MessageStructure;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMMessageLine : IMessageContinuation
{
	public IMMessageLine(IIMLine iMLine, IIMHeader iMHeader, ISadMessageSendingObject sadMessageSendingObject)
	{
		this.iMLine = Argument.NotNull(iMLine, nameof(iMLine));
		this.iMHeader = Argument.NotNull(iMHeader, nameof(iMHeader));
		this.sadMessageSendingObject = Argument.NotNull(sadMessageSendingObject, nameof(sadMessageSendingObject));
	}

	readonly IIMLine iMLine;
	readonly IIMHeader iMHeader;
	readonly ISadMessageSendingObject sadMessageSendingObject;

	[MessageLayout(Order = 0)]
	public ISadMessageFixedPart FixedPart => SadMessageFixedPartFactory.GetSadMessageFixedPart
		(isHeader: false
		, sadMessageSendingObject: sadMessageSendingObject
		, messageCode: SadMessageFixedPart.Constants.MessageCode.IM.Continuation
		, annualProgressiveNumber: iMHeader.AnnualProgressiveNumber
		, progressiveNumber: 0);

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	public ZString DeclarationType => ZString.Empty;

	[MessageLayout(Order = 2)]
	public IMLineConsignor Consignor => new IMLineConsignor();

	[MessageLayout(Order = 3)]
	public IMLineConsignee Consignee => new IMLineConsignee();

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString DispatchCountryCode => ZString.Empty;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString DestinationCountryCode => ZString.Empty;

	[MessageLayout(Order = 6)]
	[MessageFieldIntegerRepresentation(7, false)]
	[MessageFieldImportRules("D", "C60", "RN22")]
	[MessageFieldDepositoRules("D", "C60", "RN22")]
	public ZInt? NumberOfPacks => iMLine.Package?.NumberOfPacks;

	[MessageLayout(Order = 7)]
	[MessageFieldImportRules("D", "C55")]
	[MessageFieldDepositoRules("D", "C55")]
	public IMLineContainerCollection Containers => new IMLineContainerCollection(iMLine.Containers);

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 140, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString GoodsDescription => iMLine.GoodsDescription;

	[MessageLayout(Order = 9)]
	public IMLinePackage Package => new IMLinePackage(iMLine.Package);

	[MessageLayout(Order = 10)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	public ZString SensitiveGoodsCode => ZString.Empty;

	[MessageLayout(Order = 11)]
	[MessageFieldDecimalRepresentation(11, 3, false)]
	public ZDecimal? SensitiveQuantity => null;

	[MessageLayout(Order = 12)]
	[MessageFieldIntegerRepresentation(3, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZInt ItemNumber => iMLine.ItemNumber;

	[MessageLayout(Order = 13)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 10, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString CombinedNomenclature => iMLine.CombinedNomenclature;

	[MessageLayout(Order = 14)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public IMLineAdditionalCodeCollection AdditionalCodes => new IMLineAdditionalCodeCollection(iMLine.AdditionalCodes);

	[MessageLayout(Order = 15)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("O")]
	public ZString CountryOfOrigin => iMLine.CountryOfOrigin;

	[MessageLayout(Order = 16)]
	[MessageFieldDecimalRepresentation(16, 5, false)]
	[MessageFieldImportRules("R", "TRN0002")]
	[MessageFieldDepositoRules("R", "TRN0002")]
	public ZDecimal GrossMass => iMLine.GrossMass;

	[MessageLayout(Order = 17)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 3, false)]
	[MessageFieldImportRules("O")]
	public ZString Preferences => iMLine.Preferences;

	[MessageLayout(Order = 18)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Procedure => iMLine.Procedure;

	[MessageLayout(Order = 19)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("R")]
	public IMLineNationalProcedureCollection NationalProcedures => new IMLineNationalProcedureCollection(iMLine.NationalProcedures);

	[MessageLayout(Order = 20)]
	[MessageFieldDecimalRepresentation(16, 5, false)]
	[MessageFieldImportRules("R", "TRN0002")]
	[MessageFieldDepositoRules("R", "TRN0002")]
	public ZDecimal? NetMass => iMLine.NetMass;

	[MessageLayout(Order = 21)]
	[MessageFieldImportRules("O")]
	public IMLineQuotaCollection Quotas
	{
		get
		{
			var lineQuotas = iMLine.Quotas;
			return lineQuotas != null ? new IMLineQuotaCollection(lineQuotas) : null;
		}
	}

	[MessageLayout(Order = 22)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public IMLinePreviousDocument PreviousAdministrativeDocument => new IMLinePreviousDocument(iMLine.PreviousAdministrativeDocument);

	[MessageLayout(Order = 23)]
	[MessageFieldDecimalRepresentation(16, 5, false)]
	[MessageFieldImportRules("D", "CN11", "TRN0002")]
	[MessageFieldDepositoRules("D", "CN11", "TRN0002")]
	public ZDecimal? SupplementaryUnit => iMLine.SupplementaryUnit;

	[MessageLayout(Order = 24)]
	[MessageFieldDecimalRepresentation(17, 2, false, true)]
	[MessageFieldImportRules("R", "TRN0001")]
	public ZDecimal? ItemPriceEuro => iMLine.ItemPriceEuro;

	[MessageLayout(Order = 25)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 1, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString EvaluationMethod => iMLine.EvaluationMethod;

	[MessageLayout(Order = 26)]
	[MessageFieldImportRules("D", "CN12")]
	[MessageFieldDepositoRules("D", "CN12")]
	public IMLineSpecialMentionGroup SpecialMentionGroup => new IMLineSpecialMentionGroup(iMLine.SpecialMentionGroup);

	[MessageLayout(Order = 27)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public IMLineCertificateCollection Certificates => new IMLineCertificateCollection(iMLine.Certificates);

	[MessageLayout(Order = 28)]
	public IMUnitOfMeasureCollection UnitOfMeasure => new IMUnitOfMeasureCollection();

	[MessageLayout(Order = 29)]
	[MessageFieldDecimalRepresentation(16, 2, false)]
	public ZDecimal? EntryPrice => null;

	[MessageLayout(Order = 30)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 500, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString Notes => iMLine.Notes;

	[MessageLayout(Order = 31)]
	[MessageFieldDecimalRepresentation(14, 2, false, true)]
	[MessageFieldImportRules("O")]
	public ZDecimal? AdjustmentInEuro => iMLine.AdjustmentInEuro;

	[MessageLayout(Order = 32)]
	[MessageFieldDecimalRepresentation(17, 2, false, true)]
	[MessageFieldImportRules("R", "TRN0001")]
	[MessageFieldDepositoRules("R", "TRN0001")]
	public ZDecimal? StatisticalValueAmount => iMLine.StatisticalValueAmount;

	[MessageLayout(Order = 33)]
	[MessageFieldBoolRepresentation]
	public ZBool? ExportFromCe => null;

	[MessageLayout(Order = 34)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	public ZString ExportFromCountry => ZString.Empty;

	[MessageLayout(Order = 35)]
	[MessageFieldBoolRepresentation]
	public ZBool? ExportFromCeRepetition => null;

	[MessageLayout(Order = 36)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	public ZString ExportFromCountryRepetition => ZString.Empty;

	[MessageLayout(Order = 37)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public IMLineDutyCollection Duties => new IMLineDutyCollection(iMLine.Duties);

	[MessageLayout(Order = 38)]
	[MessageFieldDecimalRepresentation(17, 2, false, isDecimalPartFixedLength: true)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZDecimal? TotalItemTaxedAmount => iMLine.TotalItemTaxedAmount;

	[MessageLayout(Order = 39)]
	[MessageFieldDecimalRepresentation(17, 2, false, isDecimalPartFixedLength: true)]
	[MessageFieldImportRules("D", "C560", "CN14")]
	[MessageFieldDepositoRules("D", "C560", "CN14")]
	public ZDecimal? GrandTotalTaxedAmount => iMLine.GrandTotalTaxedAmount;
}
