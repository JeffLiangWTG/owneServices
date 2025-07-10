using System;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class GraphicalDisplayForm : ZChildForm, IGraphicalDisplay
	{
		readonly GraphicalDisplayControl graphicalDisplayControl;

		public GraphicalDisplayForm(bool readOnly)
		{
			InitializeComponent();

			graphicalDisplayControl = GetNewGraphicalDisplayControl(readOnly);
			graphicalDisplayControl.Rotation += new RotateEventHandler(OnRotate);
			graphicalDisplayControl.PageDelete += new PageDeleteEventHandler(OnPageDelete);
			graphicalDisplayControl.PageReorder += new PageReorderEventHandler(OnPageReorder);
			graphicalDisplayControl.PageCopy += new PageCopyEventHandler(OnPageCopy);
			graphicalDisplayControl.PageCut += new PageCutEventHandler(OnPageCut);
			graphicalDisplayControl.PagePaste += new PagePasteEventHandler(OnPagePaste);
			graphicalDisplayControl.PageDropped += new PageDroppedEventHandler(OnPageDropped);
			graphicalDisplayControl.PageInsert += new PageInsertEventHandler(OnPageInsert);
			graphicalDisplayControl.MoveToNewDocument += new PageCutEventHandler(OnMoveToNewDocument);

			this.Controls.Add(graphicalDisplayControl);
		}

		public StorageDocsBase Document
		{
			get { return graphicalDisplayControl.Document; }
			set { graphicalDisplayControl.Document = value; }
		}

		public override string FormCaption
		{
			get { return (Document != null) ? Res.GetString("18f875da-c6b6-444f-900d-cb9b9192185e", "eDoc Viewer - {0}", Document.SC_DescMultilingual) : Res.GetString("77e8b2d9-8310-445a-a05a-6323b39becb5", "eDoc Viewer"); }
		}

		#region IGraphicalDisplay (Proxy) Implementation

		public event PageDeleteEventHandler PageDelete;
		public event PageReorderEventHandler PageReorder;
		public event RotateEventHandler Rotation;
		public event PageCopyEventHandler PageCopy;
		public event PageCutEventHandler PageCut;
		public event PagePasteEventHandler PagePaste;
		public event PageDroppedEventHandler PageDropped;
		public event PageInsertEventHandler PageInsert;
		public event PageCutEventHandler MoveToNewDocument;

		public void ShowFile(IPreviewableDocument file, StorageDocsBase document, int pageNumber)
		{
			graphicalDisplayControl.ShowFile(file, document, pageNumber);
			this.Document = document;
		}

		protected override void OnClosed(EventArgs e)
		{
			graphicalDisplayControl.Close();
			graphicalDisplayControl.Dispose();
			base.OnClosed(e);
		}

		void OnRotate(object sender, RotateEventArgs e)
		{
			Rotation?.Invoke(this, e);
		}

		void OnPageDelete(object sender, PageDeleteEventArgs e)
		{
			PageDelete?.Invoke(this, e);
		}

		void OnPageReorder(object sender, PageReorderEventArgs e)
		{
			PageReorder?.Invoke(this, e);
		}

		void OnPageCopy(object sender, PageCopyEventArgs e)
		{
			PageCopy?.Invoke(this, e);
		}

		void OnPageCut(object sender, PageCutEventArgs e)
		{
			PageCut?.Invoke(this, e);
		}

		void OnPagePaste(object sender, PagePasteEventArgs e)
		{
			PagePaste?.Invoke(this, e);
		}

		void OnPageDropped(object sender, PageDroppedEventArgs e)
		{
			PageDropped?.Invoke(this, e);
		}

		void OnPageInsert(object sender, PageInsertEventArgs e)
		{
			PageInsert?.Invoke(this, e);
		}

		void OnMoveToNewDocument(object sender, PageCutEventArgs e)
		{
			MoveToNewDocument?.Invoke(this, e);
		}

		#endregion

		protected virtual GraphicalDisplayControl GetNewGraphicalDisplayControl(bool readOnly)
		{
			return new GraphicalDisplayControl(readOnly);
		}

#if DEBUG

		internal GraphicalDisplayControl GraphicalDisplayControlForTesting
		{
			get { return graphicalDisplayControl; }
		}

		internal void PagePasteForTesting() => OnPagePaste(this, new PagePasteEventArgs(1));
#endif
	}
}
