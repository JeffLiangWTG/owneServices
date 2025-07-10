using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class BaseInvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		protected override Customs.GUI.InvoiceLineFilterBusinessObject CreateFilterBusinessObject(Func<Customs.Business.IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
		{
			return DesignModeFinder.IsDesigning ? null : new InvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);
		}

		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Brazil;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Brazil;

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			var declaration = JobDeclaration;
			JI_Calc_InvAmountControl.Visible = declaration != null && (declaration.IsExport || declaration.IsImport);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).ColumnName = JobComInvoiceLine.Schema.FullGoodsDescription;

				var zGuidDropEditColumnStyleInfo = new ZGuidDropEditColumnStyleInfo();
				zGuidDropEditColumnStyleInfo.ColumnName = JobComInvoiceLine.Schema.JI_CEI;
				zGuidDropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
				CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo);

				AddNewColumnForCustomsInvoiceLinesBoundGrid();

				if (DefaultColumnsInOrder.Length > 0)
				{
					CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
					CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumnsInOrder);
					CustomsInvoiceLinesBoundGrid.ReOrderColumns(DefaultColumnsInOrder);
				}
				var declaration = JobDeclaration;
				if (!(declaration is JobDeclaration dec && dec.IsPersistent))
				{
					CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.JI_CEI);
				}
				if (declaration?.IsImport ?? false)
				{
					JI_Calc_CIFConvertToLocalCurrencyControl.CaptionResourceString = Res.GetData("17507CB0-3829-4F43-AA36-7DCFAF68676F", "Customs Value");
				}
			}
		}

		protected virtual void AddNewColumnForCustomsInvoiceLinesBoundGrid()
		{
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

		protected ZCodeFindBoxColumnStyleInfo CreateNewCodeFindBoxColumn(ZString column, ZInt length, ModuleIdentifier module)
		{
			var zCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo.ColumnName = column;
			zCodeFindBoxColumnStyleInfo.ModuleID = module;
			zCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo);
			return zCodeFindBoxColumnStyleInfo;
		}

		protected ZGuidFindBoxColumnStyleInfo CreateNewGuidFindBoxColumn(ZString column, ZInt length, ModuleIdentifier module, ResourceStringData groupName = null)
		{
			var zGuidFindBoxColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			zGuidFindBoxColumnStyleInfo.ColumnName = column;
			zGuidFindBoxColumnStyleInfo.ModuleID = module;
			zGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zGuidFindBoxColumnStyleInfo.GroupName = groupName;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo);
			return zGuidFindBoxColumnStyleInfo;
		}

		protected ZDropEditColumnStyleInfo CreateNewDropEditColumn(ZString column, ZInt length, bool defaultColumn = true, ResourceStringData groupName = null)
		{
			var zDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo.GroupName = groupName;
			zDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo.ColumnName = column;
			zDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zDropEditColumnStyleInfo.IsVisible = defaultColumn;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
			return zDropEditColumnStyleInfo;
		}

		protected ZCalcEditColumnStyleInfo CreateNewCalcEditColumn(ZString column, ZInt length, bool defaultColumn = true, ResourceStringData groupName = null)
		{
			var zCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo.ColumnName = column;
			zCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zCalcEditColumnStyleInfo.IsVisible = defaultColumn;
			zCalcEditColumnStyleInfo.GroupName = groupName;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo);
			return zCalcEditColumnStyleInfo;
		}

		protected virtual void CreateManufacturerAddressColumns()
		{
			var manufacturerOrgPKOrganisationFindBox = new ZOrganisationFindBoxColumnStyleInfo();
			manufacturerOrgPKOrganisationFindBox.BindToList = "Lookups.SupplierList";
			manufacturerOrgPKOrganisationFindBox.CaptionResourceString = ManufacturerCaption;
			manufacturerOrgPKOrganisationFindBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			manufacturerOrgPKOrganisationFindBox.ColumnName = JobComInvoiceLine.Schema.ManufacturerOrgPK;
			manufacturerOrgPKOrganisationFindBox.GroupName = ManufacturerCaption;
			manufacturerOrgPKOrganisationFindBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var manufacturerAddressGuidDropEdit = new ZGuidDropEditColumnStyleInfo();
			manufacturerAddressGuidDropEdit.CaptionResourceString = Res.GetData("9E791492-8D4E-4EBC-835E-04DB05EC0729", "Manufacturer Address");
			manufacturerAddressGuidDropEdit.GroupName = ManufacturerCaption;
			manufacturerAddressGuidDropEdit.ColumnName = JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress;
			manufacturerAddressGuidDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(manufacturerOrgPKOrganisationFindBox);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(manufacturerAddressGuidDropEdit);
		}

		protected void CreateNewCheckBoxColumn(ZString column, ZInt length, bool defaultColumn = true, bool readOnly = false)
		{
			var zCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			zCheckBoxColumnStyleInfo.ColumnName = column;
			zCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zCheckBoxColumnStyleInfo.IsReadOnly = readOnly;
			zCheckBoxColumnStyleInfo.IsVisible = defaultColumn;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo);
		}

		protected ZDateEditColumnStyleInfo CreateNewDateEditColumn(ZString column, ZInt length, bool defaultColumn = true, ResourceStringData groupName = null)
		{
			var zDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			zDateEditColumnStyleInfo.ColumnName = column;
			zDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(length);
			zDateEditColumnStyleInfo.IsVisible = defaultColumn;
			zDateEditColumnStyleInfo.GroupName = groupName;
			zDateEditColumnStyleInfo.DateTimeFormat = ZDateTimePickerFormat.Short;
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo);
			return zDateEditColumnStyleInfo;
		}

		protected static ResourceStringData ManufacturerCaption => Res.GetData("166C9EC0-0C47-4C99-A2CD-4A95B89D0CA9", "Manufacturer");
	}
}
