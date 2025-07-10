using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class PreviousDocumentExtendedProvider : IPreviousDocumentExtended
{
	public PreviousDocumentExtendedProvider(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument document, int sequence)
	{
		this.document = Argument.NotNull(document, nameof(document));
		SequenceNumber = sequence;
	}

	readonly EU.Business.Declaration.MultiLineAddInfos.PreviousDocument document;

	public int SequenceNumber { get; }

	public string Type => document.CSI_Code;

	public string ReferenceNumber => document.CSI_ReferenceNumber;

	public int? GoodsItemNumber => document.CSI_ItemNumber;

	public string TypeOfPackages => document.CSI_PackType;

	public int? NumberOfPackages => document.CSI_PackQty;

	public string MeasurementUnitAndQualifier => document.CSI_UnitOfQuantity;

	public decimal? Quantity => document.CSI_Quantity;

	public string ComplementOfInformation => ZString.Empty;

	public string GoodsItemIdentifier => document.CSI_ItemNumber.ToString();
}
