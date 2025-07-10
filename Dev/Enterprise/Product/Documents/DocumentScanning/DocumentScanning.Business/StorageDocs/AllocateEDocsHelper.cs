namespace Enterprise.DocumentScanning.Business
{
	public static class AllocateEDocsHelper
	{
		public static PreviewableDocumentType GetPreviewableDocumentType(StorageDocsBase document)
		{
			if (document == null || document.SC_ImageData.IsEmpty)
			{
				return PreviewableDocumentType.NoneFile;
			}

			return document is TempStorageDocs ? PreviewableDocumentType.TempFile : PreviewableDocumentType.eDocsFile;
		}
	}

	public enum PreviewableDocumentType
	{
		NoneFile = 0,
		TempFile,
		eDocsFile
	}
}
