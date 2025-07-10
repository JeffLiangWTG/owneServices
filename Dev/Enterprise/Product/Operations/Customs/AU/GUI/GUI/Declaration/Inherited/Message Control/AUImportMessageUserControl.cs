using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUImportMessageUserControl : ImportMessageUserControl
	{
		public AUImportMessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("033C22EC-F114-4E43-BFD9-5493F6FE7ACB", "Total Quarantine Service Amount"),
				ColumnName = CusEntryHeader.Schema.AQISServicePaymentAmount,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("C54530A5-D6AE-4239-876A-098C84B957EE", "Total Payable"),
				ColumnName = Customs.Business.CusEntryHeader.Schema.TotalAmountPayable,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("5F0BDB47-E518-4FB7-BD5E-07F98DFD677C", "Wood Levy"),
				ColumnName = CusEntryHeader.Schema.WoodLevy,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("A58264E9-A538-4C02-B675-086AA58D48A1", "T&I Amount"),
				ColumnName = CusEntryHeader.Schema.TAndI,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("77FD889A-945F-4E8C-A2E7-A9D7B0D75725", "GST Amount"),
				ColumnName = Customs.Business.CusEntryHeader.Schema.GSTAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("EA4DCFB1-41E6-46AB-B623-28BD999B6B95", "LCT Amount"),
				ColumnName = CusEntryHeader.Schema.LCTAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("13279358-3C99-4DAF-8192-68AB06420A22", "WET Amount"),
				ColumnName = CusEntryHeader.Schema.WETAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("8EB3CA48-AFC3-4275-89BD-9E543415F66A", "Customs Factor"),
				ColumnName = CusEntryHeader.Schema.CustomsFactor,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("B9E7A944-8E3F-491D-B0ED-8C8DE6100A8B", "Import Entry Advice"),
				ColumnName = CusEntryHeader.Schema.ImportEntryAdvice,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("A03DD07D-A0DA-4B24-B9C9-41EDA7BA3B80", "Quarantine Container Charges"),
				ColumnName = CusEntryHeader.Schema.AQISContainerCharges,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(215)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("749FBF69-E057-4173-B924-1067CCF193A9", "Quarantine Processing Charge"),
				ColumnName = CusEntryHeader.Schema.AQISProcessingCharge,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(215)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("F90A0156-D8D6-4F79-84BE-50C0FB2CD91E", "Declaration Processing Charge"),
				ColumnName = CusEntryHeader.Schema.DeclarationProcessingCharge,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(216)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("D26660D5-F7F3-4BE4-9E38-9C4BB78A75AD", "Total Payable Admin"),
				ColumnName = CusEntryHeader.Schema.TotalPayableAdmin,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("F18D039B-5361-443C-BA6C-C0CBA400F4B0", "Other Entry Charges"),
				ColumnName = CusEntryHeader.Schema.OtherEntryCharge,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156)
			});
		}
	}
}
