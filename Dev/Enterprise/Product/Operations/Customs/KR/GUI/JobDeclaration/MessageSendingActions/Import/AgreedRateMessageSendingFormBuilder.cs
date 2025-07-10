using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class AgreedRateMessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		public override ZUserControl GetUserControl() => new AgreedRateMessageDetailsUserControl();

		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("0CA2BF9F-2A2B-45C0-9AAC-B7E7B98A0DCF", "Entry Lines");

		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = base.GetAdditionalColumnStyles().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(AgreedRateMessageSendingObject.DeclarationDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(AgreedRateMessageSendingObject.PreferenceCodeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(AgreedRateMessageSendingObject.DutyRate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
				}
			});

			return result.ToArray();
		}

		public override int[] GetFormSize() => new int[] { 700, 550 };
		public override int Panel1MinSize => Panel1MinSizeForGridUserControl;
	}
}
