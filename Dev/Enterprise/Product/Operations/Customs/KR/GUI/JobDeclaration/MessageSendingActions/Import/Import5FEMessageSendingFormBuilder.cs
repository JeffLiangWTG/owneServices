using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import5FEMessageSendingFormBuilder : ImportAmendmentMessageSendingFormBuilder
	{
		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = new List<ZTextBoxColumnStyleInfo>();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationMessageSendingObject.FormattedEntryNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.SubmissionDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentVersion),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					IsMandatory = true,
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsMandatory = true,
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.DeclarantType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsMandatory = true,
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.ReasonCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsMandatory = true,
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.AmendmentReason),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsMandatory = true,
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.FaultParty),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsMandatory = true,
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.FaultPartyOtherDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.TotalAmendedItemsCount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.TotalAmendedTaxCount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.BeforeTotalDutyTaxAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.AfterTotalDutyTaxAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.DutyTaxDifference),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.BeforeCustomsValue),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.AfterCustomsValue),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.CustomsValueDifference),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.DutyPenaltyCause),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.ApplyDutyPenaltyReduction),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.TaxPenaltyCause),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionIndicator),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsMandatory = true
				},
				new ZDropEditColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionReasonCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170)
				},
				new ZMultiLineTextBoxColumnInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionReason),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionReqSequence),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyExemptionAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZCodeFindBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.PenaltyPaymentReasonCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = nameof(JobDeclarationAmendmentMessageSendingObject.RefundRequestSubmissionYN),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
				}
			});
			return result.ToArray();
		}

		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingGrid)
		{
			messageSendingGrid.ListManagerListChanged += MessageSendingGrid_ListManagerListChanged;

			var entryDetailsUserControl = new ImportAmendmentEntryDetailsUserControl();
			entryDetailsUserControl.Dock = DockStyle.Fill;

			var entryDetailsTabPage = new ZTabPage();
			entryDetailsTabPage.CaptionResourceString = Res.GetData("0A226E41-2756-4ECD-B381-21DD26E2CE2D", "Entry Details");
			entryDetailsTabPage.Dock = DockStyle.Fill;
			entryDetailsTabPage.Controls.Add(entryDetailsUserControl);

			var dutyTaxDetailsUserControl = new Import5FEMessageDutyTaxDetailsUserControl();
			dutyTaxDetailsUserControl.Dock = DockStyle.Fill;

			var dutyTaxDetailsTabPage = new ZTabPage();
			dutyTaxDetailsTabPage.CaptionResourceString = Res.GetData("4B82E101-1E5B-4305-B325-2E52D72F7D5B", "Duty && Tax Details");
			dutyTaxDetailsTabPage.Dock = DockStyle.Fill;
			dutyTaxDetailsTabPage.Controls.Add(dutyTaxDetailsUserControl);

			var refundRequestUserControl = new Import5FEMessageRefundRequestUserControl();
			refundRequestUserControl.Dock = DockStyle.Fill;

			refundRequestTabPage = new ZTabPage();
			refundRequestTabPage.Name = "RefundRequestTabPage";
			refundRequestTabPage.CaptionResourceString = Res.GetData("E883BD13-B8C1-43A7-8550-8F1746AE58CF", "Refund Request");
			refundRequestTabPage.Dock = DockStyle.Fill;
			refundRequestTabPage.Controls.Add(refundRequestUserControl);

			return [entryDetailsTabPage, dutyTaxDetailsTabPage, refundRequestTabPage];
		}

		void MessageSendingGrid_ListManagerListChanged(object sender, ZGrid.ListManagerListChangedEventArgs e)
		{
			RemoveEventsFromMessageSendingGridManager();
			var grid = (ZGrid)sender;
			messageSendingGridManager = grid.ListManager;
			messageSendingGridManager.CurrentChanged += SendingObjectsGrid_CurrentFocusedRowChanged;
			foreach (JobDeclarationAmendmentMessageSendingObject sendingObject in messageSendingGridManager.List)
			{
				sendingObject.RefundRequestSubmissionYNInfo.ValueChanged += RefundRequestYN_ValueChanged;
			}
			UpdateRefundRequestTabVisibility();
			grid.Disposed += Grid_Disposed;
		}
		CurrencyManager messageSendingGridManager;

		void Grid_Disposed(object sender, EventArgs e)
		{
			RemoveEventsFromMessageSendingGridManager();
		}

		void SendingObjectsGrid_CurrentFocusedRowChanged(object sender, EventArgs e)
		{
			UpdateRefundRequestTabVisibility();
		}

		void RemoveEventsFromMessageSendingGridManager()
		{
			if (messageSendingGridManager != null)
			{
				messageSendingGridManager.CurrentChanged -= SendingObjectsGrid_CurrentFocusedRowChanged;

				foreach (JobDeclarationAmendmentMessageSendingObject sendingObject in messageSendingGridManager.List)
				{
					sendingObject.RefundRequestSubmissionYNInfo.ValueChanged -= RefundRequestYN_ValueChanged;
				}
			}
		}
		void RefundRequestYN_ValueChanged(object sender, EventArgs e)
		{
			UpdateRefundRequestTabVisibility();
		}

		void UpdateRefundRequestTabVisibility()
		{
			if (messageSendingGridManager != null)
			{
				var amendmentSendingObject = messageSendingGridManager.GetCurrent() as JobDeclarationAmendmentMessageSendingObject;
				if (amendmentSendingObject != null && refundRequestTabPage != null)
				{
					refundRequestTabPage.TabVisible = amendmentSendingObject.RefundRequestSubmissionYN == YesNoList.Codes.Yes;
				}
			}
		}

		ZTabPage refundRequestTabPage;

		public override ZUserControl GetUserControl() => new AmendedItemsUserControl();

		public override int Panel1MinSize => 250;
		public override int Panel2MinSize => 360;
		public override int[] GetFormSize() => new int[] { 1200, 725 };
	}
}
