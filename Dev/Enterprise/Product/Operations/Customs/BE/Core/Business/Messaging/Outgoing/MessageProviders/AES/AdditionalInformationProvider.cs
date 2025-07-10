using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class AdditionalInformationProvider : IAdditionalInformation
{
	public AdditionalInformationProvider(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo document, int sequence)
	{
		this.document = Argument.NotNull(document, nameof(document));
		SequenceNumber = sequence;
	}

	readonly EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo document;

	public int SequenceNumber { get; }

	public string Code => document.CSI_Code;

	public string Text => document.CSI_Description;
}
