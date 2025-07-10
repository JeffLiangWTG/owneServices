using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.DocumentScanning.GUI.Res;
using ResString = Enterprise.DocumentScanning.GUI.ResString;

namespace Enterprise.DocumentScanning.OCR
{
	public partial class OCRResultGridForm : ZChildForm
	{
		public OCRResultGridForm()
		{
			InitializeComponent();

			SetupContextMenuExtra();
			MainStatusBar.Text = Res.GetString("a164219d-8fde-4cea-a477-d68d9513c243", "Right click on column headings to map columns.");
		}

		void SetupContextMenuExtra()
		{
			ContextMenu gridMenu = OCRResultBoundGrid.ContextMenu;
			gridMenu.Popup += new EventHandler(OnMenuPopup);

			MenuItem curItem;

			curItem = new ZMenuItem(ResString.GetMultilingualString("OCRResultsGridForm|MapColumn", "Map Column"));
			gridMenu.MenuItems.Add(curItem);

			MenuItem subMenuRoot = curItem;

			curItem = new ZMenuItem(ResString.GetMultilingualString("OCRResultsGridForm|PartNo", "Part No."), new EventHandler(OnSetColumnMapping));
			subMenuRoot.MenuItems.Add(curItem);

			curItem = new ZMenuItem(ResString.GetMultilingualString("OCRResultsGridForm|Description", "Description"), new EventHandler(OnSetColumnMapping));
			subMenuRoot.MenuItems.Add(curItem);

			curItem = new ZMenuItem(ResString.GetMultilingualString("OCRResultsGridForm|Quantity", "Quantity"), new EventHandler(OnSetColumnMapping));
			subMenuRoot.MenuItems.Add(curItem);

			curItem = new ZMenuItem(ResString.GetMultilingualString("OCRResultsGridForm|TotalValue", "Total Value"), new EventHandler(OnSetColumnMapping));
			subMenuRoot.MenuItems.Add(curItem);
		}

		Point fLastMenuPopupPoint = Point.Empty;

		void OnMenuPopup(object sender, EventArgs e)
		{
			fLastMenuPopupPoint = Control.MousePosition;
		}

		void OnSetColumnMapping(object sender, EventArgs e)
		{
			MenuItem curItem = (MenuItem)sender;

			Point curPos = fLastMenuPopupPoint;
			Point clientMousePos = OCRResultBoundGrid.PointToClient(curPos);
			int columnIndex = OCRResultGridForm.GetColumnUnderMousePosition(OCRResultBoundGrid, clientMousePos);

			if (columnIndex >= 0)
			{
				ZGridColumn col = OCRResultBoundGrid.Columns[columnIndex];
				string curColumnName = col.ColumnStyle.MappingName;

				SetGridColumnTitle(this.OCRResultBoundGrid, curColumnName, curItem.Text);
				OCRResultBoundGrid.Refresh();
			}
		}

		public string GetGridColumnTitle(ZGrid aGrid, string aColumnName)
		{
			return aGrid.Columns[aColumnName].ColumnStyle.HeaderText;
		}

		void SetGridColumnTitle(ZGrid aGrid, string aColumnName, string aDescription)
		{
			aGrid.Columns[aColumnName].ColumnStyle.HeaderText = aDescription;
		}

		void SetDefaultColumns()
		{
			for (int i = 0; i < fResultData.NumColumns; i++)
			{
				string curColumnName = OCRResultGridData.GetColumnName(i);

				ZTextBoxColumnStyleInfo colInfo = new ZTextBoxColumnStyleInfo(curColumnName, 100);
				colInfo.CharacterCasing = CharacterCasing.Normal;
				colInfo.IsMandatory = false;
				colInfo.IsReadOnly = false;
				colInfo.IsVisible = true;

				OCRResultBoundGrid.Columns.Add(colInfo);
				string defaultColumnTitle = "";
				SetGridColumnTitle(OCRResultBoundGrid, curColumnName, defaultColumnTitle);
			}
		}

		OCRResultGridData fResultData;

		public void SynchData(OCRResultGridData aResultData)
		{
			fResultData = aResultData;

			SetDefaultColumns();

			OCRResultBoundGrid.SetDataBinding(fResultData.TheDataSet, fResultData.TheDataSet.Tables[0].TableName);
			DataView dv = OCRResultGridForm.GetViewFromGrid(OCRResultBoundGrid, true);
			dv.ListChanged += new ListChangedEventHandler(ListChangedEvent);

			this.OCRResultBoundGrid.RemoveAction = RemoveAction.NoRemovePossible;
		}

		void ListChangedEvent(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded)
			{
				int curRowIndex = e.NewIndex;
				DataView dv = OCRResultGridForm.GetViewFromGrid(OCRResultBoundGrid, true);
				DataRow curRow = dv[curRowIndex].Row;

				if (curRow[OCRResultTable.PK] == DBNull.Value)
				{
					curRow[OCRResultTable.PK] = Guid.NewGuid();
				}
			}
		}

		void RowPositionChanged(object sender, EventArgs e)
		{
			fResultData.ResultTable.Columns[OCRResultTable.PK].DefaultValue = Guid.NewGuid();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			fResultData.TheDataSet.AcceptChanges();
		}

		void ACancelButton_Click(object sender, EventArgs e)
		{
			fResultData.TheDataSet.RejectChanges();
		}

		static DataView GetViewFromGrid(ZGrid aGrid, bool handleErrors)
		{
			DataView dv = null;

			BindingManagerBase bmb = aGrid.ListManager;
			CurrencyManager cm = bmb as CurrencyManager;
			if (cm != null)
			{
				dv = cm.List as DataView;
			}

			if (handleErrors && (dv == null))
			{
				throw new OdysseyException("Could not get view for grid <" + aGrid.GetType().ToString() + ">");
			}

			return dv;
		}

		static int GetColumnUnderMousePosition(ZGrid aGrid, Point clientMousePos)
		{
			DataGrid.HitTestInfo hTInfo = aGrid.HitTest(clientMousePos);
			if ((hTInfo.Type == DataGrid.HitTestType.ColumnHeader) ||
				(hTInfo.Type == DataGrid.HitTestType.Cell))
			{
				return hTInfo.Column;
			}

			return -1;
		}
	}
}
