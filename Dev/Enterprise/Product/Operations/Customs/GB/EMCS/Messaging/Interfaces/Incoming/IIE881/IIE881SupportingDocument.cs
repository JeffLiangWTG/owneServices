using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE881SupportingDocument
	{
		ITextAndLanguage SupportingDocumentDescription { get; }
		ITextAndLanguage ReferenceOfSupportingDocument { get; }
		IReadOnlyCollection<byte> ImageOfDocument { get; }
		ZString SupportingDocumentType { get; }
	}
}
