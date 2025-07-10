using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.GUI
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.CommercialInvoice.LayoutInvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
			InitialSetupInvoiceChargesGridColumns();
		}

		void InitialSetupInvoiceChargesGridColumns()
		{
			InvoiceChargesGrid.ColumnStyles.Add(CreateIsStatisticalValueApplicableColumn());
			InvoiceChargesGrid.ColumnStyles.Add(CreateIsIncludedInInvoiceAmountColumn());
			InvoiceChargesGrid.ColumnStyles.Add(CreateDistributedByColumn());
			InvoiceChargesGrid.ColumnStyles.Add(CreateFixedRateColumn());
			InvoiceChargesGrid.ColumnStyles.Add(CreateExchangeRateColumn());
		}

		protected override void ChangeInvoiceChargesGridTitlesWhenMessageTypeChanges()
		{
			if (Invoice != null)
			{
				if (Invoice.IsExport)
				{
					ChangeExportInvoiceChargesGridColumns();
				}
				else if (Invoice.IsImport)
				{
					ChangeImportInvoiceChargesGridColumns();
				}
				else
				{
					base.ChangeInvoiceChargesGridTitlesWhenMessageTypeChanges();
					ChangeOtherInvoiceChargesGridColumns();
				}
			}
		}

		void ChangeExportInvoiceChargesGridColumns()
		{
			var visibleAndOrderedColumns = GetVisibleAndOrderedColumnsForExport();
			using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceChargesGrid.SetAllColumnsVisible(false);
				InvoiceChargesGrid.SetColumnVisible(true, visibleAndOrderedColumns);
				InvoiceChargesGrid.ReOrderColumns(visibleAndOrderedColumns);
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_RX_NKCurrency, Res.GetData("26702ED0-E550-40A2-B3FF-C5B1DF831824", "Curr.").Caption);
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsIncludedInITOT, Res.GetData("A392516F-9AC2-4C65-A17D-E5AD93A82F06", "Incl. in  Inv. Lines?").Caption);
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, Res.GetData("004479C6-174C-4435-97C2-7FAA0CF8D15A", "Add to FOB?").Caption);
			}
		}

		static string[] GetVisibleAndOrderedColumnsForExport()
		{
			string[] visibleAndOrdered = {
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceCharge.Schema.J7_DistributeBy,
				JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_IsIncludedInITOT
			};
			return visibleAndOrdered;
		}

		void ChangeImportInvoiceChargesGridColumns()
		{
			var visibleAndOrderedColumns = GetVisibleAndOrderedColumnsForImport();
			using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceChargesGrid.SetAllColumnsVisible(false);
				InvoiceChargesGrid.SetColumnVisible(true, visibleAndOrderedColumns);
				InvoiceChargesGrid.ReOrderColumns(visibleAndOrderedColumns);

				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_RX_NKCurrency, Res.GetData("26702ED0-E550-40A2-B3FF-C5B1DF831824", "Curr.").Caption);
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsIncludedInITOT, Res.GetData("A392516F-9AC2-4C65-A17D-E5AD93A82F06", "Incl. in  Inv. Lines?").Caption);
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsDutiable, Res.GetData("7E1DEF88-5986-40F2-8D3D-98C300C74F8E", "Dutiable").Caption);
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsGSTApplicable, Res.GetData("BB1ACE36-14A1-41FB-855B-1884056D4FAE", "VAT Apply").Caption);
			}
		}

		static string[] GetVisibleAndOrderedColumnsForImport()
		{
			string[] visibleAndOrdered = {
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				InvoiceCharge.Schema.J7_IsGSTApplicable,
				InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				InvoiceCharge.Schema.J7_DistributeBy,
				JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				InvoiceCharge.Schema.J7_ExchangeRate,
				InvoiceCharge.Schema.J7_IsIncludedInITOT
			};
			return visibleAndOrdered;
		}

		void ChangeOtherInvoiceChargesGridColumns()
		{
			var visibleAndOrderedColumns = GetVisibleAndOrderedColumnsForOthers();
			using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceChargesGrid.SetAllColumnsVisible(false);
				InvoiceChargesGrid.SetColumnVisible(true, visibleAndOrderedColumns);
				InvoiceChargesGrid.ReOrderColumns(visibleAndOrderedColumns);

				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_RX_NKCurrency, Res.GetData("D93E3A6B-1DAC-4868-8B92-9F6F513C8D06", "Curr").Caption);
				InvoiceChargesGrid.SetColumnCaption(InvoiceCharge.Schema.J7_IsIncludedInITOT, Res.GetData("092C5C38-2B0F-4FDE-A029-9FCD16B8ECEB", "Included in Lines").Caption);
			}
		}
		static string[] GetVisibleAndOrderedColumnsForOthers()
		{
			string[] visibleAndOrdered = {
				InvoiceCharge.Schema.J7_ChargeType,
				InvoiceCharge.Schema.J7_Amount,
				InvoiceCharge.Schema.J7_RX_NKCurrency,
				InvoiceCharge.Schema.J7_IsDutiable,
				InvoiceCharge.Schema.ChargeCodeDescription,
				InvoiceCharge.Schema.J7_IsGSTApplicable,
				InvoiceCharge.Schema.J7_PrepaidCollect,
				InvoiceCharge.Schema.J7_IsIncludedInITOT
			};
			return visibleAndOrdered;
		}

		internal static ZArchitecture.ZCheckBoxColumnStyleInfo CreateIsStatisticalValueApplicableColumn()
		{
			return new ZArchitecture.ZCheckBoxColumnStyleInfo()
			{
				ColumnName = InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
				CaptionResourceString = Res.GetData("270A3AA6-48E7-4A4C-8C38-2E6CBD4A44D1", "Statistical Value Applicable"),
				ToolTip = Res.GetData("0B1FE0FD-AEF9-4CCF-83F4-EB3EE49BC5D7", "Statistical Value Applicable").Caption,
				IsVisible = true,
				IsReadOnly = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152)
			};
		}

		static ZArchitecture.ZCheckBoxColumnStyleInfo CreateIsIncludedInInvoiceAmountColumn()
		{
			return new ZArchitecture.ZCheckBoxColumnStyleInfo()
			{
				ColumnName = InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
				CaptionResourceString = Res.GetData("D35613C0-AC8E-45B0-947F-30512966E1CE", "Included in Inv. Amt"),
				ToolTip = Res.GetData("DD93BC7E-B6B6-4C06-A563-DC2E559322CD", "Included in Inv. Amt").Caption,
				IsVisible = true,
				IsReadOnly = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152)
			};
		}

		static ZArchitecture.GUI.ZDropEditColumnStyleInfo CreateDistributedByColumn()
		{
			return new ZArchitecture.GUI.ZDropEditColumnStyleInfo()
			{
				ColumnName = InvoiceCharge.Schema.J7_DistributeBy,
				CaptionResourceString = Res.GetData("2974144D-7079-4377-B09A-91526C6F494B", "Distribute By"),
				ToolTip = Res.GetData("30ADAE74-4854-46A2-BA05-1C39B9C31E2F", "Distribute By").Caption,
				IsVisible = true,
				IsReadOnly = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152)
			};
		}

		static ZArchitecture.ZCheckBoxColumnStyleInfo CreateFixedRateColumn()
		{
			return new ZArchitecture.ZCheckBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("834A3829-5679-461C-A824-E2500E69FF15", "Fixed Rate"),
				ColumnName = InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
				GroupName = Res.GetData("F3048888-FEA6-4663-AE4D-637C14761F34", "Fixed Rate"),
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74)
			};
		}

		static ZArchitecture.ZCalcEditColumnStyleInfo CreateExchangeRateColumn()
		{
			return new ZArchitecture.ZCalcEditColumnStyleInfo()
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("0BB757B2-C777-4B6E-BA08-B0A1E19A7EE4", "Exchange Rate"),
				ColumnName = InvoiceCharge.Schema.J7_ExchangeRate,
				Decimals = 6,
				GroupName = Res.GetData("0E1A6BF1-B9CD-4C0B-BF9D-717407CB71C3", "Exchange Rate"),
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102)
			};
		}
	}
}
