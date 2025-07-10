using System;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.GUI
{
	#region Interface Definition

	public interface IGraphicalDisplay
	{
		void ShowFile(IPreviewableDocument filename, StorageDocsBase document, int defaultPage);
		void Close();
		void BringToFront();

		event RotateEventHandler Rotation;
		event PageDeleteEventHandler PageDelete;
		event PageReorderEventHandler PageReorder;
		event PageCopyEventHandler PageCopy;
		event PageCutEventHandler PageCut;
		event PagePasteEventHandler PagePaste;
		event PageInsertEventHandler PageInsert;
		event PageCutEventHandler MoveToNewDocument;
		event PageDroppedEventHandler PageDropped;
		event EventHandler Closed;
	}

	#endregion

	#region Custom Event Argument Defintions

	#region RotateEventArgs

	public class RotateEventArgs : EventArgs
	{
		public readonly bool Clockwise;
		public readonly int PageNumber;

		public RotateEventArgs(bool clockwise, int pageNumber) : base()
		{
			this.Clockwise = clockwise;
			this.PageNumber = pageNumber;
		}
	}

	#endregion

	#region PageDeleteEventArgs

	public class PageDeleteEventArgs : EventArgs
	{
		public readonly int[] DeletedPages;

		public PageDeleteEventArgs(int pageDeleted) : base()
		{
			DeletedPages = new int[1];
			DeletedPages[0] = pageDeleted;
		}

		public PageDeleteEventArgs(int[] pagesDeleted) : base()
		{
			DeletedPages = pagesDeleted;
		}
	}

	#endregion

	#region PageInsertEventArgs

	public class PageInsertEventArgs : EventArgs
	{
		public readonly int InsertAfterIndex;
		public readonly string[] Files;

		public PageInsertEventArgs(string[] files, int insertAfterIndex) : base()
		{
			this.Files = files;
			this.InsertAfterIndex = insertAfterIndex;
		}
	}

	#endregion

	#region PageReorderEventArgs

	public class PageReorderEventArgs : EventArgs
	{
		public readonly int[] NewOrder;
		public readonly int[] PagesToMove;
		public readonly int DestIndex;

		public PageReorderEventArgs(int[] newOrder, int[] pagesToMove, int destIndex) : base()
		{
			this.NewOrder = newOrder;
			this.PagesToMove = pagesToMove;
			this.DestIndex = destIndex;
		}
	}

	#endregion

	#region PageCopyEventArgs

	public class PageCopyEventArgs : EventArgs
	{
		public readonly int[] PagesCopied;

		public PageCopyEventArgs(int[] pagesCopied) : base()
		{
			this.PagesCopied = pagesCopied;
		}
	}

	#endregion

	#region PageCutEventArgs

	public class PageCutEventArgs : EventArgs
	{
		public readonly int[] PagesCut;
		public readonly bool IsDragDrop;

		public PageCutEventArgs(int[] pagesCut, bool isDragDrop) : base()
		{
			this.PagesCut = pagesCut;
			this.IsDragDrop = isDragDrop;
		}

		public PageCutEventArgs(int[] pagesCut) : this(pagesCut, false)
		{
		}
	}

	#endregion

	#region PagePasteEventArgs

	public class PagePasteEventArgs : EventArgs
	{
		public readonly int PasteAfterIndex;
		public readonly bool IsDragDrop;

		public PagePasteEventArgs(int pasteAfterIndex, bool isDragDrop) : base()
		{
			this.PasteAfterIndex = pasteAfterIndex;
			this.IsDragDrop = isDragDrop;
		}

		public PagePasteEventArgs(int pasteAfterIndex) : this(pasteAfterIndex, false)
		{
		}
	}

	#endregion

	#region PageDroppedEventArgs

	public class PageDroppedEventArgs : EventArgs
	{
		public readonly int PasteAfterIndex;
		public readonly SerializableEDocCollection Data;

		public PageDroppedEventArgs(SerializableEDocCollection data, int pasteAfterIndex)
		{
			this.Data = data;
			this.PasteAfterIndex = pasteAfterIndex;
		}
	}

	#endregion

	#endregion

	#region Custom Event Delegates

	public delegate void RotateEventHandler(object sender, RotateEventArgs e);
	public delegate void PageDeleteEventHandler(object sender, PageDeleteEventArgs e);
	public delegate void PageInsertEventHandler(object sender, PageInsertEventArgs e);
	public delegate void PageReorderEventHandler(object sender, PageReorderEventArgs e);
	public delegate void PageCopyEventHandler(object sender, PageCopyEventArgs e);
	public delegate void PageCutEventHandler(object sender, PageCutEventArgs e);
	public delegate void PagePasteEventHandler(object sender, PagePasteEventArgs e);
	public delegate void PageDroppedEventHandler(object sender, PageDroppedEventArgs e);

	#endregion
}
