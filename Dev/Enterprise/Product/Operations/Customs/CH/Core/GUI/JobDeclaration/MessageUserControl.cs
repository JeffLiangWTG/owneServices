using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class MessageUserControl : Customs.GUI.ImportMessageUserControl
{
	public MessageUserControl()
	{
		InitializeComponent();
		ReorderTabPages();
		SetupGridColumns();
		BindingSource.SetBindingMember(EComMessageUserControl, "CustomsEntryHeaders.EComMessages");
		RequiresMergeLabel.AllowOverlap(MainHorizontalSplitContainer);
	}

	protected void SetupGridColumns()
	{
		if (!DesignMode)
		{
			ReOrderEntryHeaderColumns();
			ReOrderEntryLinesColumns();
		}
	}

	protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

	protected override string MessagesUserControlBindingPath => "CustomsEntryHeaders.NonEComMessages";

	void ReorderTabPages()
	{
		EntryLinesMessagesTabControl.ReorderTabPages(MessageTabPage, EComMessageTabPage, EntryHeaderChargesTabPage, EntryLinesTabPage);
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();
		EComMessageTabPage.TabVisible = JobDeclaration?.IsImport ?? false;
		EntryHeaderChargesTabPage.TabVisible = JobDeclaration?.IsImport ?? false;
	}

	#region Entry Header Grid columns

	void ReOrderEntryHeaderColumns()
	{
		using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			EntriesBoundGrid.RemoveFromAvailableColumns(CusEntryHeader.Schema.EntryNumber);

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("2533A69A-2FDA-4760-82D3-9FC04E9B514F", "Selection Result"),
				ColumnName = CusEntryHeader.Schema.SelectionResult,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("DF4A0FA5-EB68-434B-9A1C-D7C61F58833E", "Selection Result Desc."),
				ColumnName = CusEntryHeader.Schema.SelectionResultDescription,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("9ED3A222-8D25-4A9B-8B56-D79995F0FE36", "Access Code"),
				ColumnName = CusEntryHeader.Schema.AccessCode,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
			});

			var totalDutyAmount = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.TotalDutyAmount);
			if (totalDutyAmount != null)
			{
				totalDutyAmount.CaptionResourceString = Res.GetData("6A48D157-71B7-4B38-A0B3-7CF6C50CA242", "Duty");
			}

			var vatAmount = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.GSTAmount);
			if (vatAmount != null)
			{
				vatAmount.CaptionResourceString = Res.GetData("60AF0B14-3AE8-427C-BBC7-32EEF3CC28EA", "VAT");
			}

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("41DD304D-074F-4567-A772-C230840594F0", "Acceptance Date"),
				ColumnName = nameof(CusEntryHeader.MovementReferenceNumberIssueDate),
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("69EA065C-7C19-4786-92B1-08DB28B70D95", "Activation Deadline"),
				ColumnName = nameof(CusEntryHeader.MovementReferenceNumberExpiryDate),
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			EntriesBoundGrid.SetColumnMandatory(CusEntryHeader.Schema.CH_MessageType, false);
			EntriesBoundGrid.SetColumnMandatory(CusEntryHeader.Schema.CH_MessageTypeDescription, false);

			EntriesBoundGrid.SetAllColumnsVisible(false);
			EntriesBoundGrid.SetColumnVisible(true, defaultColumnsForEntryHeaderGrid);
			EntriesBoundGrid.ReOrderColumns(defaultColumnsForEntryHeaderGrid);
		}
	}

	readonly string[] defaultColumnsForEntryHeaderGrid = new[]
	{
			CusEntryHeader.Schema.MovementReferenceNumber,
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CH_Status,
			CusEntryHeader.Schema.MessageStatusDescription,
			CusEntryHeader.Schema.CH_PhaseStatus,
			CusEntryHeader.Schema.PhaseStatusDescription,
			CusEntryHeader.Schema.SelectionResult,
			CusEntryHeader.Schema.SelectionResultDescription,
			CusEntryHeader.Schema.CH_EntrySubmittedDate,
			CusEntryHeader.Schema.CH_EntryReleaseDate,
			CusEntryHeader.Schema.AccessCode,
			CusEntryHeader.Schema.PackagesCount,
			CusEntryHeader.Schema.TotalDutyAmount,
			CusEntryHeader.Schema.GSTAmount,
			nameof(CusEntryHeader.ConfirmedDuty),
			nameof(CusEntryHeader.ConfirmedVAT),
			nameof(CusEntryHeader.EComMessageStatusDescription),
			nameof(CusEntryHeader.MovementReferenceNumberIssueDate),
			nameof(CusEntryHeader.MovementReferenceNumberExpiryDate),
	};

	#endregion

	#region Entry Lines Grid columns

	void ReOrderEntryLinesColumns()
	{
		using (EntryLineGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			var lineStatus = EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.LineSubmissionStatusDescription);
			if (lineStatus != null)
			{
				lineStatus.CaptionResourceString = Res.GetData("7DB8297A-2420-47C5-93D5-BD8094BDBB84", "Line Status");
			}

			var dutyAmount = EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.DutyAmount);
			if (dutyAmount != null)
			{
				dutyAmount.CaptionResourceString = Res.GetData("97A61EC5-A99A-4301-8968-0C2488804CD3", "Total Duty Tax");
			}

			var vatAmount = EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.GSTVATAmount);
			if (vatAmount != null)
			{
				vatAmount.CaptionResourceString = Res.GetData("99D5A40C-5F67-430E-907F-C512EE94425B", "VAT Amount");
			}

			var statisticalValue = EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_StatisticalValue);
			if (statisticalValue != null)
			{
				statisticalValue.CaptionResourceString = Res.GetData("EC18A0C8-3002-4071-892F-9517D08170A7", "Statistical Value");
				statisticalValue.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			}

			EntryLineGrid.SetAllColumnsVisible(false);
			EntryLineGrid.SetColumnVisible(true, defaultColumnsForEntryLinesGrid);
			EntryLineGrid.ReOrderColumns(defaultColumnsForEntryLinesGrid);
		}
	}

	readonly string[] defaultColumnsForEntryLinesGrid = new string[]
	{
			CusEntryLine.Schema.CL_LineNumber,
			CusEntryLine.Schema.LineSubmissionStatusDescription,
			CusEntryLine.Schema.FormattedTariff,
			CusEntryLine.Schema.EffectiveDescription,
			CusEntryLine.Schema.DutyAmount,
			CusEntryLine.Schema.GSTVATAmount,
			nameof(CusEntryLine.ConfirmedDuty),
			nameof(CusEntryLine.ConfirmedVAT),
			CusEntryLine.Schema.CL_CustomsValue,
			CusEntryLine.Schema.CL_StatisticalValue
	};

	#endregion
}
