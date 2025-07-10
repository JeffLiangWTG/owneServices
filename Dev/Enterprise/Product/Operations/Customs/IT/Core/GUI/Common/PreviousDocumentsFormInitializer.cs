using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Common;

public class PreviousDocumentsFormInitializer
{
	readonly IPreviousDocumentsForm userControl;

	public PreviousDocumentsFormInitializer(IPreviousDocumentsForm userControl)
	{
		this.userControl = Argument.NotNull(userControl, nameof(userControl));
	}

	public void InitializeGridLayout(ZGrid previousDocumentsGrid)
	{
		using (previousDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			var columnsToBeAdded = new ZGridColumnInfo[]
			{
				new ZDropEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_Procedure,
					ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZDropEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_Code,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_SubType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_ReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},

				new ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_Status,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_ReferenceNumber2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
				},

				new ZDateEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_DateOfIssue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},

				new ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_LineNo,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				},

				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_CustomsOffice,
					ModuleID = ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},

				new TariffColumnStyleInfo
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.FormattedTariff,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106),
					GetCountryCode = () => Core.Constants.CountryCodes.Italy,
					GetDataGrouping = () => Core.Constants.CountryCodes.Italy,
					GetTariffType = userControl.GetUniversalTariffType,
					GetEffectiveDate = userControl.GetEffectiveDate
				},

				new ZCalcEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_Quantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87),
					GroupName = Res.GetData("B624CF7B-86E9-4ACB-9F91-D246ADC0C79D", "Net Mass")
				},

				new ZDropEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_UnitOfQuantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78),
					GroupName = Res.GetData("B624CF7B-86E9-4ACB-9F91-D246ADC0C79D", "Net Mass")
				},

				new ZCalcEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_Quantity2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87),
					GroupName = Res.GetData("83F0A0D9-69E3-4276-9F15-F717E8CF342D", "Supplementary Quantity")
				},

				new ZDropEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_UnitOfQuantity2,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58),
					GroupName = Res.GetData("83F0A0D9-69E3-4276-9F15-F717E8CF342D", "Supplementary Quantity")
				},

				new ZCalcEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_Quantity3,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87),
					GroupName = Res.GetData("1B6DCDF6-7592-4ADE-A2B4-E2682FA4955D", "Gross Mass")
				},

				new ZDropEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_UnitOfQuantity3,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					GroupName = Res.GetData("1B6DCDF6-7592-4ADE-A2B4-E2682FA4955D", "Gross Mass")
				},

				new ZCalcEditColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = PreviousDocument.Schema.CSI_PackQty,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87),
				}
			};
			previousDocumentsGrid.ColumnStyles.Clear();
			previousDocumentsGrid.ColumnStyles.AddRange(columnsToBeAdded);
		}
	}

	public void InitializeTariffCodeFindBox(TariffFindBox tariffFindBox)
	{
		tariffFindBox.GetCountryCode = () => Core.Constants.CountryCodes.Italy;
		tariffFindBox.GetDataGrouping = () => Core.Constants.CountryCodes.Italy;
		tariffFindBox.GetTariffType = userControl.GetUniversalTariffType;
		tariffFindBox.GetEffectiveDate = userControl.GetEffectiveDate;
	}
}
