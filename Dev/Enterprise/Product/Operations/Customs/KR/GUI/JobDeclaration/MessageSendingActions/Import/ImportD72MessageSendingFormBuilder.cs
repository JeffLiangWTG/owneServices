using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportD72MessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		public override ZUserControl GetUserControl() => new ImportD72MessageDetailsUserControl();

		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("6C074AAC-81E7-4448-AFB9-8F7502303799", "Entry Lines");

		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = base.GetAdditionalColumnStyles().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(ExtendReExportDateMessageSendingObject.CurrentReExportScheduledDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(ExtendReExportDateMessageSendingObject.NewReExportDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(ExtendReExportDateMessageSendingObject.ReasonDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
				}
			});

			return result.ToArray();
		}

		public override int[] GetFormSize() => new int[] { 820, 600 };
		public override int Panel1MinSize => Panel1MinSizeForMultiUserControls;
	}
}
