using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class AdditionalReferenceProvider : IDocument
{
	public AdditionalReferenceProvider(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo document, int sequence)
	{
		this.document = Argument.NotNull(document, nameof(document));
		this.sequence = sequence;
	}

	readonly EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo document;
	readonly int sequence;

	public int SequenceNumber => sequence;

	public string Type => document.CSI_Code;

	public string ReferenceNumber => document.CSI_ReferenceNumber;
}
