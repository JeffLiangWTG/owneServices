using Enterprise.Customs.ES.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class DocumentManager9015
	{
		public DocumentManager9015(JobDeclaration declaration, IDocument9015MessageBoxProvider guiProvider)
		{
			this.declaration = declaration;
			this.guiProvider = guiProvider;
		}

		readonly JobDeclaration declaration;
		readonly IDocument9015MessageBoxProvider guiProvider;

		public void Remove9015DocumentsIfNeeded()
		{
			foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
			{
				if (entryHeader.EntryHas9015SupportingDocuments() && !entryHeader.Is9015SupportingDocumentNeeded())
				{
					AskAboutRemovingDocuments(entryHeader);
				}
			}
		}

		void AskAboutRemovingDocuments(CusEntryHeader entryHeader)
		{
			if (guiProvider.AskIfShouldRemove9015Documents(entryHeader.CH_BGMReference))
			{
				entryHeader.Remove9015Documents();
			}
		}
	}
}
