using System;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void DocumentEventHandler(object sender, DocumentEventArgs e);

	#region DocumentEventArgs

	public class DocumentEventArgs : EventArgs
	{
		public DocumentEventArgs(StorageDocsBase document)
		{
			this.Document = document;
		}

		public readonly StorageDocsBase Document;
	}

	#endregion
}
