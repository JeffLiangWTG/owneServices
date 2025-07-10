using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Module
{
	public partial class EntryHeaderFilterUserControl : Customs.Module.EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			grid.ColourDeciding += grid_ColourDeciding;

			FilteredGrid.SetColumnVisible(false, [Schema.AgentsReference, Schema.DateOfArrival, CusEntryHeader.Schema.CH_TotalPaid, Schema.BranchName]);
			FilteredGrid.SetColumnVisible(true, [Schema.ShipmentType, Schema.TransportMode]);
			grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == Schema.SupplierName).CaptionResourceString = Res.GetData("18405289-7D06-47A5-AB68-748D820CCF9A", "Supplier Name");
			grid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == Schema.ImporterName).CaptionResourceString = Res.GetData("059C1D1E-2709-41E6-A973-FCC2903CDB56", "Importer Name");
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("BE6126DD-BA00-4421-97B1-C7F352179ED5", "Declaration Unified Number"),
						ColumnName = CusEntryHeader.Schema.DeclarationUnifiedNumber,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("76A69F3D-78BF-4BED-A181-64F546582932", "CIQ Number"),
						ColumnName = CusEntryHeader.Schema.CIQNumber,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("44C8AA72-69A9-4131-8B0B-0DB9D5D9E5EB", "ACDA Number"),
						ColumnName = CusEntryHeader.Schema.ACDANumber,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("16DA5543-5D50-46AC-B4B6-6989C996DCFA", "CIQ Status"),
						ColumnName = CusEntryHeader.Schema.CIQStatus,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("A8A0C3D8-3193-4099-8084-791D08B2481C", "CIQ Status Desc."),
						ColumnName = CusEntryHeader.Schema.CIQStatusDescription,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("5229DAA6-52D5-4AA2-956A-6C02455BE3D4", "Bill of Lading"),
						ColumnName = CusEntryHeader.Schema.BillOfLading,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("A7B035AB-7102-41D3-A803-C8D5483CDA2C", "CPC"),
						ColumnName = CusEntryHeader.Schema.CustomsProcedureCode,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("6E770B44-7972-446D-949D-FD4E252F26D8", "Customs Procedure"),
						ColumnName = CusEntryHeader.Schema.CustomsProcedureDesc,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZDateEditColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("4A782889-D7F6-4441-BF6F-C85A5530EEA5", "Declaration Date"),
						ColumnName = CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
						DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("75982AC2-A976-4FE1-BF56-6E0D7905E98A", "Branch Code"),
						ColumnName = Schema.BranchCode,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("CADCA7A1-69D7-4BF4-8472-97F827D02BFE", "Supplier"),
						ColumnName = Schema.SupplierCode,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("8E0E4D51-3E64-4411-90F5-E8A3540334C3", "Importer"),
						ColumnName = Schema.ImporterCode,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("5229DAA6-52D5-4AA2-956A-6C02455BE3D3", "Manufacturer"),
						ColumnName = ColumnNames.ManufacturerCode,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("31932E90-5F46-4179-86B6-9B9C3BBB2164", "Manufacturer Name"),
						ColumnName = ColumnNames.ManufacturerName,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("7DAC3B79-2AAE-4E88-BCA0-A81C4BCE4442", "Buyer"),
						ColumnName = ColumnNames.BuyerCode,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("DA63938A-AF8D-40D8-88E4-782A1EE93917", "Buyer Name"),
						ColumnName = ColumnNames.BuyerName,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("DE984507-8884-427E-8F1E-DB80ABB947C2", "Customs Office"),
						ColumnName = ColumnNames.CustomsOffice,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("023629A9-C8B4-4BDD-8F5D-CF14E6CAF1F7", "Office of Entry/Exit"),
						ColumnName = ColumnNames.OfficeOfEntryExit,
						IsReadOnly = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("0EE4C364-0F10-4227-889F-C7DDAAAA9481", "Manual No."),
						ColumnName = ColumnNames.ManualNo,
						IsReadOnly = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("A3C948B1-7750-48D2-A143-49EE0E4AF0C2", "No of Packages"),
						ColumnName = ColumnNames.Packages,
						IsReadOnly = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("6C6AA6E5-15E7-4D93-B6B3-B562639A0251", "Total Weight"),
						ColumnName = ColumnNames.TotalWeight,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

					new ZArchitecture.ZDateEditColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("BDC3E96B-8B4B-4E7E-B988-28378DEA04C1", "Archive Date"),
						ColumnName = CusEntryHeader.Schema.ArchiveDate,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
						DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("8B689A53-F87D-4DB6-BE02-84552BC56DE5", "Archive User"),
						ColumnName = CusEntryHeader.Schema.ArchiveUser,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					},

					new ZArchitecture.ZDateEditColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("0E530F94-DA16-4AC9-A816-F3B2A6CB79DB", "Last Audited Date"),
						ColumnName = ColumnNames.LastAuditedDate,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
						DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("4176AF67-4C5D-4A68-9673-BCF577A573E5", "Last Audited User"),
						ColumnName = ColumnNames.LastAuditedUser,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					},

					new ZArchitecture.ZDateEditColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("3A61B9E4-16E3-4912-BFA7-371776DC3C4E", "Deadline", "Declaration Deadline", ""),
						ColumnName = ColumnNames.DeclarationDeadline,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
						DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("9E080F93-3760-424E-ABFB-C4995E9ABA4D", "Days of Delay", "Days of Delayed Declaration", ""),
						ColumnName = ColumnNames.DaysOfDelayedDeclaration,
						GroupName = Res.GetData("EEB1C687-FE62-49D4-B33F-86B3F238AD5C", "Delayed Declaration"),
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("36963639-4535-4E75-8995-801FDF503E39", "Fee for Delay", "Fee for Delayed Declaration", ""),
						ColumnName = ColumnNames.FeeForDelayedDeclaration,
						GroupName = Res.GetData("EEB1C687-FE62-49D4-B33F-86B3F238AD5C", "Delayed Declaration"),
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					},

					new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("54F91709-98BC-45EE-A4E8-584E21175B10", "Remaining Days", "Remaining Days For Declaration", ""),
						ColumnName = ColumnNames.RemainingDaysForDeclaration,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
					},

					new ZArchitecture.ZCheckBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("C5929EC0-C3A6-11EB-BA2F-0800200C9A66",  "Ready for Co.", "Ready For Complete Declaration", ""),
						ColumnName = ColumnNames.ReadyForCompleteDeclaration,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					},
				});

			grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 146, true);
			grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 325, true);
		}

		protected override List<string> ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new List<string>
					{
						CusEntryHeader.Schema.EntryNumber,
						CusEntryHeader.Schema.DeclarationReference,
						CusEntryHeader.Schema.CH_BGMReference,
						CusEntryHeader.Schema.DeclarationUnifiedNumber,
						CusEntryHeader.Schema.CIQNumber,
						CusEntryHeader.Schema.CH_MessageType,
						CusEntryHeader.Schema.CH_MessageTypeDescription,
						CusEntryHeader.Schema.CH_EntryStatus,
						CusEntryHeader.Schema.EntryHeaderStatusDescription,
						CusEntryHeader.Schema.CH_Status,
						CusEntryHeader.Schema.MessageStatusDescription,
						CusEntryHeader.Schema.CIQStatus,
						CusEntryHeader.Schema.CIQStatusDescription,
						CusEntryHeader.Schema.BillOfLading,
						CusEntryHeader.Schema.CustomsProcedureCode,
						CusEntryHeader.Schema.CustomsProcedureDesc,
						CusEntryHeader.Schema.CH_EntrySubmittedDate,
						CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
						CusEntryHeader.Schema.CH_EntryReleaseDate,
						Schema.BranchCode,
						Schema.SupplierCode,
						Schema.SupplierName,
						Schema.ImporterCode,
						Schema.ImporterName,
						Schema.ShipmentType,
						Schema.TransportMode,
						ColumnNames.CustomsOffice,
						ColumnNames.DeclarationDeadline,
						ColumnNames.OfficeOfEntryExit,
						ColumnNames.ManualNo,
						ColumnNames.Packages,
						ColumnNames.RemainingDaysForDeclaration,
						ColumnNames.ReadyForCompleteDeclaration,
						CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
						CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
					};

					columnNamesInSortOrder = columns;
				}
				return columnNamesInSortOrder;
			}
		}
		List<string> columnNamesInSortOrder;

		public static class ColumnNames
		{
			public const string ManufacturerCode = "Declaration+Manufacturer+OH_Code";
			public const string ManufacturerName = "Declaration+Manufacturer+OH_FullNameTruncated";
			public const string BuyerCode = "Declaration+Buyer+OH_Code";
			public const string BuyerName = "Declaration+Buyer+OH_FullNameTruncated";
			public const string CustomsOffice = "Declaration+JE_CustomsOffice";
			public const string OfficeOfEntryExit = "Declaration+JE_OfficeOfEntryExit";
			public const string ManualNo = "EntryInstruction+CEI_ManualNo";
			public const string Packages = "EntryInstruction+CEI_Packages";
			public const string TotalWeight = "TotalWeight";
			public const string LastAuditedDate = "LastAuditedDate";
			public const string LastAuditedUser = "LastAuditedUser";
			public const string DeclarationDeadline = "Declaration+DeclarationDeadline";
			public const string RemainingDaysForDeclaration = "RemainingDaysForDeclaration";
			public const string DaysOfDelayedDeclaration = "DaysOfDelayedDeclaration";
			public const string FeeForDelayedDeclaration = "FeeForDelayedDeclaration";
			public const string ReadyForCompleteDeclaration = "ReadyForCompleteDeclaration";
		}

		protected void grid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			var entryHeader = (CusEntryHeader)e.ObjectAtRow;
			if (entryHeader.Declaration.JE_ApplicationCode == Customs.Business.DeclarationApplicationCodeList.Codes.Builtin)
			{
				e.Colour = Color.Empty;
				var remainingDays = entryHeader.RemainingDaysForDeclaration;
				if (remainingDays != 0)
				{
					e.Colour = CNCustomsDataRegistry.GetColorByRemainingDays(remainingDays, entryHeader.Declaration.TransportMode);
				}
			}
		}
	}
}
