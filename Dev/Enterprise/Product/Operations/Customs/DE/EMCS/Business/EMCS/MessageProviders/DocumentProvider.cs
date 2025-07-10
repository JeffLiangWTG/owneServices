using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class DocumentProvider : IEMCSDocument
	{
		readonly EMCSDocument document;
		readonly DocumentProviderHelper helper;

		public DocumentProvider(EMCSDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
			helper = new DocumentProviderHelper(document);
		}

		public ITextAndLanguage Description => description ?? (description = new TextAndLanguageProvider(document.CSI_Description));
		ITextAndLanguage description;

		public ITextAndLanguage Reference => reference ?? (reference = new TextAndLanguageProvider(document.CSI_ReferenceNumber));
		ITextAndLanguage reference;

		public string DocumentType => helper.DocumentType;

		public string DocumentReference => helper.DocumentReference;
	}
}
