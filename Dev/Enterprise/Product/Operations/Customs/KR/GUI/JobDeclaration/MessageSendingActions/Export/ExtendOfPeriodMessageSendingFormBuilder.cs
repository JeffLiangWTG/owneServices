using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ExtendOfPeriodMessageSendingFormBuilder : MessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = new List<ZTextBoxColumnStyleInfo>(GetEntryNumberColumnStyle());
			result.AddRange(GetCommonAmendmentColumnStyleInfo());

			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.FaultParty,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsMandatory = true
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.ReasonCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsMandatory = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObject.Schema.CurrentDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObject.Schema.NewDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					IsMandatory = true
				}
			});

			return result.ToArray();
		}

		public override ZUserControl GetUserControl() => null;
		public override ResourceStringData GetUserControlGroupBoxCaption() => null;
		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid) => null;
		public override int[] GetFormSize() => new int[] { 960, 460 };
	}
}
