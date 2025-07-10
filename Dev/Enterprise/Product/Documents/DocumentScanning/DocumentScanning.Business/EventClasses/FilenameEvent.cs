using System;

namespace Enterprise.DocumentScanning.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
	public delegate void FilenameEventHandler(StorageDocsBase storageDoc, FilenameEventArgs e);
	public delegate void OverwriteOrCreateNewEventHandler(object sender, FilenameEventArgs args);

	public enum FileAction
	{
		Overwrite,
		CreateNew,
		None,
		AskForUserInput
	}

	public class FilenameEventArgs : EventArgs
	{
		public FilenameEventArgs(string filename)
		{
			Filename = filename;
		}

		public readonly string Filename;

		public FileAction Action
		{
			get { return fAction; }
			set { fAction = value; }
		}

		FileAction fAction = FileAction.None;
	}
}
