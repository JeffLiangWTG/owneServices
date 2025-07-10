using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import5UAMessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = GetEntryNumberColumnStyle().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.DutyPenaltyExemption5UASequenceNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.AmendmentDeclarationDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.AmendmentVersion),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyAmountFrom5FK),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyExemptionReasonCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190)
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyExemptionReason),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyExemptionRequestMessageSendingObject.PenaltyExemptionAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				},
			});
			return result.ToArray();
		}
	}
}
