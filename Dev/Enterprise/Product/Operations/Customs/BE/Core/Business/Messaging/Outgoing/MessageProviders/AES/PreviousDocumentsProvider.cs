using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class PreviousDocumentsProvider : IDocument
{
	public PreviousDocumentsProvider(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument document, int sequence)
	{
		this.document = Argument.NotNull(document, nameof(document));
		SequenceNumber = sequence;
	}

	readonly EU.Business.Declaration.MultiLineAddInfos.PreviousDocument document;

	public int SequenceNumber { get; }

	public string Type => document.CSI_Code;

	public string ReferenceNumber => document.CSI_ReferenceNumber;
}
