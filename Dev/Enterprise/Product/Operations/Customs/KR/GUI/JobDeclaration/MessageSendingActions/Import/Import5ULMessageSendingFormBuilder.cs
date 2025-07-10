using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import5ULMessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = GetEntryNumberColumnStyle().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.FormattedCustomsDisbursementBillNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.Amendment5WNNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.SubmissionDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.AmendmentVersionNoCustoms),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.PaymentAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.RefundType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.RefundCause),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.RefundReason),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(PenaltyRefundRequestMessageSendingObject.TaxOffice),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
				},
			});
			return result.ToArray();
		}
	}
}
