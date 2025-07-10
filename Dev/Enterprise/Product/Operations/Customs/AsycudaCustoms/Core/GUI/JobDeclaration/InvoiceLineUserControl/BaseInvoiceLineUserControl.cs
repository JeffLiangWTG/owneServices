using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class BaseInvoiceLineUserControl : Customs.GUI.GeneralCountryInvoiceLineUserControl
	{
		public BaseInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			AddColumnsToGrid();
			ReorderAndChangeInvoiceLinesGridVisibility();
		}

		protected override ResourceStringData GetJI_Calc_GSTConvertToLocalCurrencyControlCaption()
		{
			return JI_Calc_GSTConvertToLocalCurrencyControl.CaptionResourceString.Format(Core.Constants.Customs.CusEntryFeeTypes.VAT);
		}

		void AddColumnsToGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PreviousEntryNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZGuidDropEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CEI,
					ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CEI_Description,
					IsReadOnly = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},

				new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_Procedure,
					ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_PrimaryPreference,
					ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					GroupName = Res.GetData("BaseInvoiceLineUserControl|cfa450ab-8502-4340-a000-df0eb99d7c33", "Additional Quantity 1"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					GroupName = Res.GetData("BaseInvoiceLineUserControl|cfa450ab-8502-4340-a000-df0eb99d7c33", "Additional Quantity 1"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30)
				},

				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
					GroupName = Res.GetData("BaseInvoiceLineUserControl|ab1d04b6-1ee7-47ed-acef-f7ef5060bfb0", "Additional Quantity 2"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					GroupName = Res.GetData("BaseInvoiceLineUserControl|ab1d04b6-1ee7-47ed-acef-f7ef5060bfb0", "Additional Quantity 2"),
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30)
				},

				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_ZZF_NKTaxType,
					ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				}
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = CharacterCasing.Upper,
				ColumnName = nameof(JobComInvoiceLine.VehicleVIN),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});
		}

		void ReorderAndChangeInvoiceLinesGridVisibility()
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumnsForGrid());
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(DefaultColumnsForGrid());
			}
		}

		protected string[] DefaultColumnsForGrid() => defaultColumnsForGrid ?? (defaultColumnsForGrid = GetDefaultColumnsForGrid());
		string[] defaultColumnsForGrid;

		protected string[] GetDefaultColumnsForGrid()
		{
			var columns = new List<string>()
			{
				JobComInvoiceLine.Schema.JI_LineNo,
				JobComInvoiceLine.Schema.JI_Calc_Invoice,
				JobComInvoiceLine.Schema.JI_CEI,
				JobComInvoiceLine.Schema.JI_CEI_Description,
				JobComInvoiceLine.Schema.JI_PartNo,
				JobComInvoiceLine.Schema.JI_Procedure,
				JobComInvoiceLine.Schema.JI_CC,
				JobComInvoiceLine.Schema.JI_Tariff,
				JobComInvoiceLine.Schema.JI_PrimaryPreference,
				JobComInvoiceLine.Schema.JI_InvoiceQuantity,
				JobComInvoiceLine.Schema.JI_InvoiceUQ,
				JobComInvoiceLine.Schema.JI_CustomsQuantity,
				JobComInvoiceLine.Schema.JI_CustomsUnitQty,
				JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
				JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
				JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
				JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
				JobComInvoiceLine.Schema.JI_LinePrice,
				JobComInvoiceLine.Schema.JI_ZZF_NKTaxType,
				JobComInvoiceLine.Schema.JI_Description,
				JobComInvoiceLine.Schema.JI_CountryOfOrigin,
				JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
				JobComInvoiceLine.Schema.JI_Weight,
				JobComInvoiceLine.Schema.JI_WeightUQ,
				JobComInvoiceLine.Schema.JI_NetWeight,
				JobComInvoiceLine.Schema.JI_NetWeightUQ,
				JobComInvoiceLine.Schema.JI_Volume,
				JobComInvoiceLine.Schema.JI_VolumeUQ,
				JobComInvoiceLine.Schema.JI_OrderNumber,
				JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
				JobComInvoiceLine.Schema.JI_PartAttrib1,
				JobComInvoiceLine.Schema.JI_PartAttrib2,
				JobComInvoiceLine.Schema.JI_PartAttrib3,
				JobComInvoiceLine.Schema.JI_SerialNumber
			};

			columns.AddRange(new[] {
				JobComInvoiceLine.Schema.UnitPrice,
				JobComInvoiceLine.Schema.JI_CustomAttrib1,
				JobComInvoiceLine.Schema.JI_CustomAttrib2,
				JobComInvoiceLine.Schema.JI_CustomAttrib3,
				JobComInvoiceLine.Schema.JI_CustomAttrib4,
				JobComInvoiceLine.Schema.JI_CustomAttrib5,
				JobComInvoiceLine.Schema.JI_CustomAttrib6,
				JobComInvoiceLine.Schema.JI_CustomTextBlob1,
				JobComInvoiceLine.Schema.JI_PreviousEntryNumber,
				JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
				nameof(JobComInvoiceLine.VehicleVIN)
			});

			return columns.ToArray();
		}

		public BaseApplicationBusinessProvider ApplicationBusinessProvider => BaseApplicationBusinessProvider.GetApplicationBusinessProvider(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		protected override ZString UniversalTariffType => ApplicationBusinessProvider?.UniversalTariffType ?? base.UniversalTariffType;

		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl() => new BaseInvoiceLineChargesUserControl();

		#region Dynamic Layout

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new InvoiceLineDetailsLayouts();

		#endregion
	}
}
