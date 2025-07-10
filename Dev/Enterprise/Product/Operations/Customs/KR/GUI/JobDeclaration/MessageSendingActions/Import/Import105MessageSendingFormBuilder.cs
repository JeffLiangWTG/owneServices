using System.Collections.Generic;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import105MessageSendingFormBuilder : ImportAmendmentMessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = new List<ZTextBoxColumnStyleInfo>(GetEntryNumberColumnStyle());

			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentVersion,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentTypeDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.LawCodeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.CustomsDisbursementBillNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = JobDeclarationMiscMessageSendingObjectCore.Schema.AmendmentReason,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsMandatory = true,
				}
			});

			return result.ToArray();
		}

		public override ZUserControl GetUserControl() => new AmendedItemsUserControl();
		public override int Panel1MinSize => Panel1MinSizeForGridUserControl;
	}
}
