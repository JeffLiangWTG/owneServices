using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseMessageUserControl : ImportMessageUserControl
	{
		public ImportLicenseMessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
		}
		public static class Schema
		{
			public const string EntryInstructionDescription = "EntryInstruction+CEI_Description";
		}

		protected JobDeclaration Declaration => JobDeclaration as JobDeclaration;

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			if (Declaration != null)
			{
				using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					EntriesBoundGrid.SetAllAvailability(false);
					EntriesBoundGrid.SetAvailability(true, availableColumnsForImportLicense);
				}
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				EntriesBoundGrid.SetAllColumnsVisible(false);
				EntriesBoundGrid.SetColumnVisible(true, defaultColumnsForImportLicense);
			}
		}

		void SetupEntryHeaderColumns()
		{
			using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).CaptionResourceString = Res.GetData("017f56cb-54ca-4beb-9470-629c68cb1631", "Import License");

				EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("22F1992F-37D0-4164-BC9D-01A40A7C0722", "Entry Instruction"),
					ColumnName = Schema.EntryInstructionDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("1b1efafe-b428-4eb5-a610-105da92f1405", "License Submitted Date"),
					ColumnName = CusEntryHeader.Schema.CH_EntrySubmittedDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127)
				},

				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("98f41641-a53a-4aa7-8fcf-79c9302c67bf", "License Issue Date"),
					ColumnName = CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					DateTimeFormat = ZDateTimePickerFormat.Short,
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("f5b9738d-24a9-4fc0-ac62-213b1f84a46c", "License Status"),
					ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("22cd57c7-3c44-46ff-8dbe-c82322e2322f", "License Status Description"),
					ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("46d69cc3-9377-4535-878d-df57f98ea8f9", "Message Status"),
					ColumnName = CusEntryHeader.Schema.CH_Status,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("43e9b83c-7d7d-4b7b-8ac7-17adec91581a", "Message Status Description"),
					ColumnName = CusEntryHeader.Schema.MessageStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("ad6e2e6d-fda9-4c86-87d6-87983ae6da6b", "License Id"),
					ColumnName = CusEntryHeader.Schema.ImportLicenseIdentifier,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("42D4BF1C-6693-4321-BC8F-41298B0C6B0C", "Shipment Expiry Date"),
					ColumnName = CusEntryHeader.Schema.CH_ValidityILShipmentDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("D370A131-7917-45A2-8D2D-A81C261B12B0", "Concession Date"),
					ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZArchitecture.ZDateEditColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("4B22FA02-1BD9-4A37-9AE1-AB96C5DF0927", "Dispatch Expiry Date"),
					ColumnName = CusEntryHeader.Schema.CH_ValidityILDispatchDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.ImportDeclarationNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
			});
			}
		}

		readonly string[] defaultColumnsForImportLicense = new string[]
			{
				Schema.EntryInstructionDescription,
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.ImportLicenseIdentifier,
				CusEntryHeader.Schema.CH_ValidityILShipmentDate,
				CusEntryHeader.Schema.CH_EntryReleaseDate,
				CusEntryHeader.Schema.CH_ValidityILDispatchDate
			};

		readonly string[] availableColumnsForImportLicense = new string[]
			{
				Schema.EntryInstructionDescription,
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_BGMReference,
				CusEntryHeader.Schema.CH_MessageType,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.ImportLicenseIdentifier,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				CusEntryHeader.Schema.CH_Status,
				CusEntryHeader.Schema.MessageStatusDescription,
				CusEntryHeader.Schema.CH_ValidityILShipmentDate,
				CusEntryHeader.Schema.CH_EntryReleaseDate,
				CusEntryHeader.Schema.CH_ValidityILDispatchDate,
				CusEntryHeader.Schema.ImportDeclarationNumber
			};

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);
	}
}
