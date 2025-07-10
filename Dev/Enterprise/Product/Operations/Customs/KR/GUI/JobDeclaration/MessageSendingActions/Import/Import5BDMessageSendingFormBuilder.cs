using System.Linq;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import5BDMessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = base.GetAdditionalColumnStyles().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(EarlyReleaseMiscMessageSendingObject.AmendmentReason),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(EarlyReleaseMiscMessageSendingObject.SecurityType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(EarlyReleaseMiscMessageSendingObject.OtherSecurityType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(EarlyReleaseMiscMessageSendingObject.SecurityStartDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(EarlyReleaseMiscMessageSendingObject.SecurityEndDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(EarlyReleaseMiscMessageSendingObject.SecurityAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(EarlyReleaseMiscMessageSendingObject.ReasonForEarlyRemoval),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				}
			});
			return result.ToArray();
		}

		public override int[] GetFormSize() => new int[] { 1050, 460 };
	}
}
