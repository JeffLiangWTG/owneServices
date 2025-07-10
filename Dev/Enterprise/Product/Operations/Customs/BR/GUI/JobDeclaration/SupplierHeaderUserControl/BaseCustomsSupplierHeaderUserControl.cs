using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class BaseCustomsSupplierHeaderUserControl : Customs.GUI.DeclarationInvoiceHeaderUserControl
	{
		public BaseCustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
		}

		protected override string ColumnTitleWhenExportForDutiable => base.ColumnTitleWhenImportForDutiable;

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (InvoiceHeadersBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ResetSupplierAddressColumns();

				if (DefaultColumnsInOrder.Length > 0)
				{
					InvoiceHeadersBoundGrid.SetAllColumnsVisible(false);
					InvoiceHeadersBoundGrid.SetColumnVisible(true, DefaultColumnsInOrder);
					InvoiceHeadersBoundGrid.ReOrderColumns(DefaultColumnsInOrder);
				}
			}
		}

		string[] DefaultColumnsInOrder => defaultColumnsInOrder ?? (defaultColumnsInOrder = GetDefaultColumnsInOrderCore().ToArray());
		string[] defaultColumnsInOrder;

		protected virtual IEnumerable<string> GetDefaultColumnsInOrderCore() => new List<string>();

		void ResetSupplierAddressColumns()
		{
			var supplierAddressColumns = GetSupplierAddressColumns();
			if (supplierAddressColumns != null)
			{
				var supplierColumnInfo = InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OH_Supplier);
				if (supplierColumnInfo != null)
				{
					InvoiceHeadersBoundGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.JZ_OH_Supplier);

					var index = InvoiceHeadersBoundGrid.ColumnStyles.IndexOf(supplierColumnInfo);

					foreach (var column in supplierAddressColumns)
					{
						InvoiceHeadersBoundGrid.ColumnStyles.Insert(index++, column);
					}
				}
				else
				{
					InvoiceHeadersBoundGrid.ColumnStyles.AddRange(supplierAddressColumns);
				}
			}
		}

		protected virtual ZGridColumnInfo[] GetSupplierAddressColumns() => null;

		protected void CreateNewDropEditColumn(ZString column, ZInt length, ResourceStringData caption = null, bool defaultColumn = false)
		{
			var zDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo.CaptionResourceString = caption;
			zDropEditColumnStyleInfo.ColumnName = column;
			zDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zDropEditColumnStyleInfo.IsVisible = defaultColumn;
			InvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
		}

		protected void CreateNewTextBoxColumn(ZString column, ZInt length, bool defaultColumn = false, ResourceStringData groupName = null)
		{
			var zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo.GroupName = groupName;
			zTextBoxColumnStyleInfo.ColumnName = column;
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zTextBoxColumnStyleInfo.IsVisible = defaultColumn;
			InvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
		}

		protected void CreateNewCalcEditColumn(ZString column, ZInt length, bool defaultColumn = false)
		{
			var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo.ColumnName = column;
			zCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zCalcEditColumnStyleInfo.IsVisible = defaultColumn;
			InvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
		}
	}
}
