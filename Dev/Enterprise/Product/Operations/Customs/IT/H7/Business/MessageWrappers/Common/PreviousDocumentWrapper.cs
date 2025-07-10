using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class PreviousDocumentWrapper : IPreviousDocument
{
	public PreviousDocumentWrapper(PreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}

	readonly PreviousDocument previousDocument;

	public int? LineNo => null;

	public int? NumberOfPackages => null;

	public string PackageType => null;

	public decimal? Quantity => null;

	public string DocumentType => previousDocument.CSI_Code;

	public string UnitOfQuantity => null;

	public string ReferenceNumber => previousDocument.CSI_ReferenceNumber;
}
