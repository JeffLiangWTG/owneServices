using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class DocumentProvider : IEMCSDocument
	{
		readonly EMCSDocument document;

		public DocumentProvider(EMCSDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		public ITextAndLanguage Description => description ?? (description = new TextAndLanguageProvider(document.CSI_Description));
		ITextAndLanguage description;

		public ITextAndLanguage Reference => reference ?? (reference = new TextAndLanguageProvider(document.CSI_ReferenceNumber));
		ITextAndLanguage reference;

		public string DocumentType => CachedValueHelper.GetValue(ref documentType, () => document.CSI_SubType);
		CachedValue<string> documentType;

		public string DocumentReference => CachedValueHelper.GetValue(ref documentReference, () => document.CSI_ReferenceNumber);
		CachedValue<string> documentReference;
	}
}
