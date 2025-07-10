using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			SetupGridColumns();
		}
		public static class CaptionStrings
		{
			public static string CountryOfOrigin => Res.GetString("4EAF83F8-1A50-461B-BDF3-B3A547E9066E", "Origin");
		}

		protected override ZBool DynamicLayoutApplied => true;

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new InvoiceLineDetailsLayout();

		protected override void OnLoad(EventArgs e)
		{
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(DefaultColumns);
				base.OnLoad(e);
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (CustomsInvoiceLinesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				CustomsInvoiceLinesBoundGrid.ReOrderColumns(DefaultColumns);
				CustomsInvoiceLinesBoundGrid.SetAllColumnsVisible(false);
				CustomsInvoiceLinesBoundGrid.SetColumnVisible(true, DefaultColumns);
			}
		}

		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Import;

		void SetupGridColumns()
		{
			CustomsInvoiceLinesBoundGrid.SetColumnWidth(JobComInvoiceLine.Schema.JI_CountryOfOrigin, 45);
			CustomsInvoiceLinesBoundGrid.SetColumnCaption(JobComInvoiceLine.Schema.JI_CountryOfOrigin, CaptionStrings.CountryOfOrigin);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_PrimaryPreference,
					Width = 80,
					IsVisible = true,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_PrimaryPreference", "Preference"),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_PreferenceDocNumber,
					Width = 80,
					IsVisible = true,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_PreferenceDocNumber", "Preference Doc.#"),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|69086792-4C38-451F-BDD2-354161BFA549", "Statistical Qty"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsVisible = false,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_CustomsSecondQuantity", "Statistical Qty"),
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|69086792-4C38-451F-BDD2-354161BFA549", "Statistical Qty"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
					IsVisible = false,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_CustomsSecondUnitQty", "Statistical UQ"),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|5B1A726D-5D28-44A8-AF41-03B715C1513C", "Additional Qty"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_CustomsThirdQuantity", "Additional Qty"),
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|5B1A726D-5D28-44A8-AF41-03B715C1513C", "Additional Qty"),
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
					IsVisible = false,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_CustomsThirdUnitQty", "Additional UQ"),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_BondedWhsQuantity,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|30A78D35-4A76-4B8B-ACE9-5E9E859A771B", "Countable Qty"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_BondedWhsQuantity", "Countable Qty"),
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_BondedWhsUnitQty,
					GroupName = Res.GetData("ImportInvoiceLineUserControl|30A78D35-4A76-4B8B-ACE9-5E9E859A771B", "Countable Qty"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(30),
					IsVisible = false,
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_BondedWhsUnitQty", "Countable UQ"),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = JobComInvoiceLine.Schema.JI_ZZF_NKTaxType,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					CaptionResourceString = Res.GetData("IL.ImportInvoiceLineUserControl.JI_ZZF_NKTaxType", "VAT Code"),
				},
			});
		}

		string[] DefaultColumns => defaultColumns ??= new[] {
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.JI_Calc_Invoice,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_Tariff,
			JobComInvoiceLine.Schema.JI_InvoiceQuantity,
			JobComInvoiceLine.Schema.JI_InvoiceUQ,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_LinePrice,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			JobComInvoiceLine.Schema.JI_PrimaryPreference,
		};
		string[] defaultColumns;
	}
}
