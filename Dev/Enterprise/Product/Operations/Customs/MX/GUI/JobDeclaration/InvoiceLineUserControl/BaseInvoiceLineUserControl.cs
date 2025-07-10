using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.MX.GUI
{
	public partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		protected override bool UseUniversalTariff => false;

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CreateNewGuidDropEditColumn(JobComInvoiceLine.Schema.JI_CEI, ZDropEdit.ShowInDropDownList.OnlyShowCode);
				CreateNewMultiLineTextBoxColumn(JobComInvoiceLine.Schema.Observations, 300, 200);

				AddNewColumnForCustomsInvoiceLinesBoundGrid();

				if (DefaultColumnsInOrder.Length > 0)
				{
					CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
					CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumnsInOrder);
					CustomsInvoiceLinesBoundGrid.ReOrderColumns(DefaultColumnsInOrder);
				}

				if (!(JobDeclaration is JobDeclaration dec && dec.IsPersistent))
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.JI_CEI);
				}
			}
		}

		protected virtual void AddNewColumnForCustomsInvoiceLinesBoundGrid()
		{
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber, 80);
		}

		string[] DefaultColumnsInOrder => defaultColumnsInOrder ?? (defaultColumnsInOrder = GetDefaultColumnsInOrderCore().ToArray());
		string[] defaultColumnsInOrder;

		protected virtual IEnumerable<string> GetDefaultColumnsInOrderCore() => new List<string>();

		protected ZTextBoxColumnStyleInfo CreateNewTextBoxColumn(ZString column, ZInt length, ResourceStringData caption = null, bool defaultColumn = true, bool readOnly = false, ResourceStringData groupName = null)
		{
			var zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo.CaptionResourceString = caption;
			zTextBoxColumnStyleInfo.GroupName = groupName;
			zTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo.ColumnName = column;
			zTextBoxColumnStyleInfo.IsReadOnly = readOnly;
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zTextBoxColumnStyleInfo.IsVisible = defaultColumn;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
			return zTextBoxColumnStyleInfo;
		}

		protected void CreateNewGuidDropEditColumn(ZString column, ShowInDropDownList showInDropDown, bool defaultColumn = true)
		{
			var zGuidDropEditColumnStyleInfo = new ZGuidDropEditColumnStyleInfo();
			zGuidDropEditColumnStyleInfo.ColumnName = column;
			zGuidDropEditColumnStyleInfo.ShowInDropDown = showInDropDown;
			zGuidDropEditColumnStyleInfo.IsVisible = defaultColumn;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo);
		}

		protected void CreateNewMultiLineTextBoxColumn(ZString column, ZInt minimumEditControlWidth, ZInt length, bool defaultColumn = true)
		{
			var zMultiLineTextBoxColumnInfo = new ZMultiLineTextBoxColumnInfo();
			zMultiLineTextBoxColumnInfo.ColumnName = column;
			zMultiLineTextBoxColumnInfo.MinimumEditControlWidth = minimumEditControlWidth;
			zMultiLineTextBoxColumnInfo.Width = length;
			zMultiLineTextBoxColumnInfo.IsVisible = defaultColumn;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo);
		}
	}
}
