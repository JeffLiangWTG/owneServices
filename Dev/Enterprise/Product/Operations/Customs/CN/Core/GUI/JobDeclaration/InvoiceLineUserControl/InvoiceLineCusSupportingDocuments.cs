using System.Collections.Generic;
using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI
{
	public partial class InvoiceLineCusSupportingDocumentsUserControl : CusSupportingDocumentsUserControl
	{
		public InvoiceLineCusSupportingDocumentsUserControl()
		{
			InitializeComponent();
			AddColumnsForGrid();
		}
		void AddColumnsForGrid()
		{
			var zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo()
			{
				BindToDecimalPlaces = null,
				ColumnName = "CSI_LineNo",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			};

			CusSupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);

			CusSupportingDocumentsGrid.ReOrderColumns(ColumnNamesInSortOrder);
		}

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (fcolumnNamesInSortOrder == null)
				{
					var columns = new List<string>();
					columns.Add(CusSupportingDocument.Schema.DocumentType);
					columns.Add(CusSupportingDocument.Schema.CSI_ReferenceNumber);
					columns.Add(CusSupportingDocument.Schema.CSI_LineNo);
					fcolumnNamesInSortOrder = columns.ToArray();
				}
				return fcolumnNamesInSortOrder;
			}
		}
		string[] fcolumnNamesInSortOrder;
	}
}
