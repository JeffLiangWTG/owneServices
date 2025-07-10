using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentScanning.GUI
{
	internal class ZSplitResultGrid : ZGrid
	{
		public ZSplitResultGrid()
		: base()
		{
			StorageDocImageViewer = new StorageDocsViewer();
			StorageDocImageViewer.Initialise(this, null);
			RegisterEventHandlers();
		}

		internal readonly StorageDocsViewer StorageDocImageViewer;
		internal Point LastClickPoint;

		void RegisterEventHandlers()
		{
			DoubleClick += new EventHandler(Grid_DoubleClick);
		}

		#region Properties

		[Browsable(false)]
		public DocumentSplitResultInfo CurrentElementAtMousePosition
		{
			get { return GetElementAtPosition(LastClickPoint); }
		}

		[Browsable(false)]
		public DocumentSplitResultInfo CurrentElement
		{
			get
			{
				DocumentSplitResultInfo bizO = null;

				if (CurrentRowIndex >= 0 && CurrentRowIndex < ListManager.Count)
				{
					bizO = (DocumentSplitResultInfo)ListManager.List[CurrentRowIndex];
				}
				return bizO;
			}
		}

		#endregion

		#region Context Menu

		void SaveMouseCoordinates()
		{
			LastClickPoint = PointToClient(MousePosition);
		}

		protected override void SetupContextMenu()
		{
			base.SetupContextMenu();
			AddSeparator(ContextMenu);
			AddViewMenuItem(ContextMenu);
			AddSeparator(ContextMenu);
		}

		void AddMenuItem(ContextMenu menu, eDocMenuItem menuItem)
		{
			menu.MenuItems.Add(menuItem);
		}

		void AddSeparator(ContextMenu menu)
		{
			if (menu.MenuItems[menu.MenuItems.Count - 1].Text != Constants.SeparatorMenuText)
			{
				AddMenuItem(menu, new SeparatorMenuItem());
			}
		}

		void AddViewMenuItem(ContextMenu menu)
		{
			AddMenuItem(menu, new ViewMenuItem(new EventHandler(Grid_ViewFile)));
		}

		protected override void HookContextMenu()
		{
			base.HookContextMenu();
			ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			SaveMouseCoordinates();
			UpdateContextMenuElements(CurrentElementAtMousePosition);
		}

		internal void UpdateContextMenuElements(DocumentSplitResultInfo elementAtMouse)
		{
			foreach (var item in ContextMenu.MenuItems.OfType<eDocMenuItem>())
			{
				item.UpdateVisibility(GetWrappedFile(elementAtMouse), SelectedElements, false, true);
			}
		}

		public DocumentSplitResultInfo GetElementAtPosition(Point p)
		{
			DocumentSplitResultInfo bizO = null;
			DataGrid.HitTestInfo info = HitTest(p);

			if (info.Type == DataGrid.HitTestType.Cell || info.Type == DataGrid.HitTestType.RowHeader && info.Row != -1)
			{
				bizO = (DocumentSplitResultInfo)ListManager.List[info.Row];
			}

			return bizO;
		}

		#endregion

		#region MenuEvents

		#region View

		void Grid_DoubleClick(object sender, EventArgs e)
		{
			SaveMouseCoordinates();
			FireDoubleClickedIfApplicable();
		}

		protected void FireDoubleClickedIfApplicable()
		{
			if (HitTest(LastClickPoint).Row >= 0)
			{
				DoubleClicked();
			}
		}

		protected virtual void DoubleClicked()
		{
			foreach (DocumentSplitResultInfo element in SelectedElements)
			{
				View(element);
			}
		}

		void Grid_ViewFile(object sender, EventArgs e)
		{
			View(CurrentElementAtMousePosition);
		}

		protected void View(DocumentSplitResultInfo file)
		{
			var doc = GetWrappedFile(file);
			StorageDocImageViewer.View(doc, ReadOnly);
		}

		StorageDocsBase GetWrappedFile(DocumentSplitResultInfo info)
		{
			StorageDocsBase file = null;

			if (info != null)
			{
				DocumentFactory factory = new DocumentFactoryProvider().GetFactory(info.Factory);
				if (info.SplitManager.DocumentToSplit as StorageFile != null)
				{
					file = factory.New<StorageFile>();
				}
				else if (info.SplitManager.DocumentToSplit as StorageDocs != null)
				{
					file = factory.New<StorageDocs>();
				}
				file.SC_FileName = info.DocumentName;
				file.SC_DataType = info.SplitManager.DocumentToSplit.SC_DataType;
				file.SC_ImageData = info.DocumentData;
			}

#if DEBUG
			CurrentFile = file;
#endif

			return file;
		}

#if DEBUG
		public StorageDocsBase CurrentFile
		{
			get;
			set;
		}
#endif

		#endregion

		#endregion
	}
}
