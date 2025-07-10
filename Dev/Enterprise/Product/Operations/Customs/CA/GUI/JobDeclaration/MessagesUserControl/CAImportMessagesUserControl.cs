using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAImportMessagesUserControl : Customs.GUI.ImportMessageUserControl
	{
		public CAImportMessagesUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();

			RequiresMergeLabel.AllowOverlap(MainHorizontalSplitContainer);
		}

		protected void EntriesBoundGrid_AfterBind(object sender, EventArgs e)
		{
			EntriesBoundGrid.ListManager.PositionChanged += EntriesBoundGridListManager_PositionChanged;
			EntriesBoundGridListManager_PositionChanged(null, null);
		}

		protected void EntriesBoundGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var caMessagesUserControl = BaseMessageUserControl.HostedControl as MessagesTabUserControl;
			caMessagesUserControl?.MessagesGridListManager_PositionChanged(null, null);

			if (DataSource is JobDeclaration declaration && declaration.IsCADEnabled)
			{
				var listManager = EntriesBoundGrid.ListManager;
				if (listManager != null)
				{
					var entry = listManager.GetCurrent() as CusEntryHeader;
					if (entry != null)
					{
						CADEntryLinesTabUseControl.ChangeVisibilities(entry.IsCAD);
					}
				}
			}
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.SetAvailability(false, CusEntryHeader.Schema.CH_MessageTypeDescription);
			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("E436C151-934C-498E-B791-E8A3A2B590C4", "Entry Status"),
				ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("8BCEC7BD-EAB8-49B6-B170-7C9E4FEBAC0B", "Status", "Msg. Status", "Message Status", ""),
				ColumnName = CusEntryHeader.Schema.CH_Status,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("AC68F284-4F95-437F-A668-E439D2896287", "Customs Value"),
				ColumnName = CusEntryHeader.Schema.CustomsValue,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("C141D40B-62DF-4A26-A5E8-1E85CE753A18", "Transaction Value"),
				ColumnName = CusEntryHeader.Schema.TransactionValue,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("B894B0CA-52A4-4E59-81DC-C8E346FF4914", "Total Payable"),
				ColumnName = CusEntryHeader.Schema.TotalAmountPayable,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("A4C5D549-FB95-4D17-AB5E-D4BA04FCB090", "Duty Amount"),
				ColumnName = CusEntryHeader.Schema.TotalDutyAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				BindToDecimalPlaces = null,
				CaptionResourceString = Res.GetData("3C8049E9-F52A-462B-AA0A-D5AB5934DC7E", "GST Amount"),
				ColumnName = CusEntryHeader.Schema.GSTAmount,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("DA33040A-131A-4364-A555-52FFCDA07138", "Submitted Date"),
				ColumnName = CusEntryHeader.Schema.CH_EntrySubmittedDate,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("D5EB31C1-661C-4064-9918-56ED5F05ECA0", "Customs Office"),
				ColumnName = CusEntryHeader.Schema.EffectivePortOfClearance,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("EC2A81BD-4676-4DE3-A33D-1046D0AD1887", "Accepted Date"),
				ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("CC64D529-7BD3-4319-9787-25FA828FAB1E", "Due Date"),
				ColumnName = CusEntryHeader.Schema.EffectiveValuationDate,
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115)
			});

			var tranNumber = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference);
			tranNumber.Caption = Res.GetString("6cfd3d26-9db0-49eb-84ab-d175111c1dc7", "Transaction Ref.");
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.PackagesCount));
			EntriesBoundGrid.ReOrderColumns(EntriesGridSortOrder);
		}

		string[] EntriesGridSortOrder
		{
			get
			{
				if (entriesGridSortOrder == null)
				{
					entriesGridSortOrder = new[]
					{
						CusEntryHeader.Schema.CH_MessageType,
						CusEntryHeader.Schema.CH_BGMReference,
						CusEntryHeader.Schema.CH_EntryStatus,
						CusEntryHeader.Schema.CH_Status,
						CusEntryHeader.Schema.TransactionValue,
						CusEntryHeader.Schema.CustomsValue,
						CusEntryHeader.Schema.TotalDutyAmount,
						CusEntryHeader.Schema.GSTAmount,
						CusEntryHeader.Schema.TotalAmountPayable,
						CusEntryHeader.Schema.CH_EntrySubmittedDate,
						CusEntryHeader.Schema.EffectivePortOfClearance,
						CusEntryHeader.Schema.CH_EntryReleaseDate,
						CusEntryHeader.Schema.EffectiveValuationDate
					};
				}
				return entriesGridSortOrder;
			}
		}
		string[] entriesGridSortOrder;

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		protected override string MessagesUserControlBindingPath => "CustomsEntryHeaders.MessagesForDisplay";
	}
}
