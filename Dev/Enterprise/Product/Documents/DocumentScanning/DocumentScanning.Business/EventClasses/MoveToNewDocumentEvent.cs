using System;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void MoveToNewDocumentEventHandler(object sender, MoveToNewDocumentEventArgs e);

	public class MoveToNewDocumentEventArgs : EventArgs
	{
		public MoveToNewDocumentEventArgs(SerializableEDocCollection eDocs)
		{
			SerializableEDocs = eDocs;
		}

		public readonly SerializableEDocCollection SerializableEDocs;
	}
}
