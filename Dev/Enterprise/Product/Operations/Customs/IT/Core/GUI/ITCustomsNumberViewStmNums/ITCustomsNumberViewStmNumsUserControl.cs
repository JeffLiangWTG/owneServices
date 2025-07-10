using System;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.GUI;

public partial class ITCustomsNumberViewStmNumsUserControl : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyUserControl
{
	[Obsolete("Required by the designer on subclass. Please do not use.")]
	public ITCustomsNumberViewStmNumsUserControl()
	{
		InitializeComponent();
	}

	public ITCustomsNumberViewStmNumsUserControl(ITCustomsNumberViewStmNumsWrapperCollection collection)
		: base(collection)
	{
		InitializeComponent();
		AddColumnsToGrid();
	}

	void AddColumnsToGrid()
	{
		using (NumberRangesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			NumberRangesGrid.RemoveFromAvailableColumns(CustomsNumberViewStmNums.Schema.SN_FountainName);

			var yearOfApplicabilityCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo()
			{
				ColumnName = ITCustomsNumberViewStmNumsWrapper.Schema.YearOfApplicability,
				ShowGroupSeparators = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48)
			};
			var appliesToDropEditColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo()
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = ITCustomsNumberViewStmNumsWrapper.Schema.AppliesTo,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73)
			};

			NumberRangesGrid.ColumnStyles.AddRange(new ZGridColumnInfo[] { yearOfApplicabilityCalcEditColumnStyleInfo, appliesToDropEditColumnStyleInfo });

			NumberRangesGrid.ReOrderColumns(
				[
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_Type,
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_TypeDescription,
					ITCustomsNumberViewStmNumsWrapper.Schema.YearOfApplicability,
					ITCustomsNumberViewStmNumsWrapper.Schema.AppliesTo,
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_ValueForDisplay,
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_AvailableNumbers,
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_MinimumValue,
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_Count,
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_MaximumValue,
					ITCustomsNumberViewStmNumsWrapper.Schema.SN_SystemCreateTimeUtc
				]);
		}
	}
}
