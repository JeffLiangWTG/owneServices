using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public partial class LPCOEntryHeaderFilterControl : Customs.Module.EntryHeaderFilterUserControl
	{
		public static class LPCOSchema
		{
			public const string Screening = "Declaration+JE_ScreeningStatus";
			public const string LicenseAuth = "Declaration+JE_EntryAuthorisationDate";
			public const string LicenseStyle = "Declaration+JE_MessageSubType";
			public const string LicenseSubmitted = "Declaration+EntrySubmitDateAsString";
			public const string IssueDate = "Declaration+EntryIssueDateAsString";
		}

		public LPCOEntryHeaderFilterControl() : base()
		{
		}

		public LPCOEntryHeaderFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		protected override void InitialiseGridCore()
		{
			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("2677C6F6-932B-4A24-B2CF-B24194914A0D", "Reference Number"),
						ColumnName = CusEntryHeader.Schema.EntryReferenceNumber,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("FC816B70-2954-4662-8598-A1AF772E7125", "Screening"),
						ColumnName = LPCOSchema.Screening,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("7885A170-2C36-49CA-82F4-1AF00D3D578F", "License Auth. Date"),
						ColumnName = LPCOSchema.LicenseAuth,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("51737CC9-A612-496F-AE49-4CFF7D996CAC", "License Style"),
						ColumnName = LPCOSchema.LicenseStyle,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("39EC69FF-4D1A-4564-A7FC-C869B0E66BEF", "License Submitted"),
						ColumnName = LPCOSchema.LicenseSubmitted,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("7983A717-6414-4DF5-AA2E-1DC0E62286DD", "Issue Date"),
						ColumnName = LPCOSchema.IssueDate,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("CD235AFC-6B0F-4B2B-B4D2-77E72987F768", "Branch Code"),
						ColumnName = Schema.BranchCode,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("CAEEC693-9894-4D01-99CA-4E2E501683E1", "Importer Code"),
						ColumnName = Schema.ImporterCode,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("C36646D1-8206-4A4A-833D-C7B477EFD587", "Supplier Code"),
						ColumnName = Schema.SupplierCode,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
			});
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			SetAvailability();
			SetVisibility();
			ChangeColumns();
		}

		void SetAvailability()
		{
			grid.SetAllAvailability(false);
			grid.SetAvailability(true, availableColumnsForGrid);
		}

		void SetVisibility()
		{
			grid.SetAllColumnsVisible(false);
			grid.SetColumnVisible(true, ColumnNamesInSortOrder.ToArray());
		}

		void ChangeColumns()
		{
			grid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).CaptionResourceString = Res.GetData("C7E3CA41-17C6-4EA7-BF57-37EEB8F3CB24", "License Number");
			grid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryStatus).CaptionResourceString = Res.GetData("C794B600-B896-442B-A838-2B58133D6220", "License Status");
			grid.GetColumnStyle(CusEntryHeader.Schema.EntryHeaderStatusDescription).CaptionResourceString = Res.GetData("59C4F3B0-9DE1-4565-BB3C-91277423583F", "License Status Description");
			grid.GetColumnStyle(CusEntryHeader.Schema.MessageStatusDescription).CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		}

		protected override List<string> ColumnNamesInSortOrder => new List<string>(columnNamesInSortOrder);

		readonly string[] columnNamesInSortOrder = new string[]
		{
				Schema.BranchCode,
				CusEntryHeader.Schema.DeclarationReference,
				CusEntryHeader.Schema.EntryReferenceNumber,
				Schema.ImporterCode,
				Schema.SupplierCode,
				LPCOSchema.Screening,
				CusEntryHeader.Schema.EntryNumber,
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.EntryHeaderStatusDescription,
				CusEntryHeader.Schema.CH_MessageTypeDescription,
				CusEntryHeader.Schema.MessageStatusDescription,
		};

		readonly string[] availableColumnsForGrid = new string[]
		{
			Schema.BranchCode,
			Schema.BranchName,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.EntryReferenceNumber,
			Schema.ImporterCode,
			Schema.ImporterName,
			Schema.SupplierCode,
			Schema.SupplierName,
			LPCOSchema.Screening,
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CH_MessageType,
			CusEntryHeader.Schema.CH_MessageTypeDescription,
			CusEntryHeader.Schema.CH_Status,
			CusEntryHeader.Schema.MessageStatusDescription,
			LPCOSchema.IssueDate,
			LPCOSchema.LicenseAuth,
			LPCOSchema.LicenseStyle,
			LPCOSchema.LicenseSubmitted
		};
	}
}
