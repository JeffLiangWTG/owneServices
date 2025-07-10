using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class LocalExportLoadingCompletionMessageSendingFormBuilder : MessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = new List<ZTextBoxColumnStyleInfo>(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationLoadingCompletionMessageSendingObject.CustomsReceiptNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationLoadingCompletionMessageSendingObject.CustomsOffice),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationLoadingCompletionMessageSendingObject.Department),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationLoadingCompletionMessageSendingObject.StevedoresCompany),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationLoadingCompletionMessageSendingObject.LoadingDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
				}
			});

			return result.ToArray();
		}
		public override ZUserControl GetUserControl() => new StevedoresUserControl();
		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("9B5E3411-4144-4654-A5F3-CE48AA7E3DEB", "Stevedores");
		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid) => null;
		public override int[] GetFormSize() => new int[] { 900, 550 };

		public override int Panel1MinSize => Panel1MinSizeForGridUserControl;
	}
}
