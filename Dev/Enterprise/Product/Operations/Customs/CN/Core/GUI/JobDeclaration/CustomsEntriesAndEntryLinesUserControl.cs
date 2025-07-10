using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class CustomsEntriesAndEntryLinesUserControl : Customs.GUI.ImportMessageUserControl
	{
		public CustomsEntriesAndEntryLinesUserControl()
		{
			InitializeComponent();

			SuspendLayout();

			AddEntryHeaderColumns();
			SetEntryHeaderColumns();

			AddEntryLineColumns();
			ResetTabOrder();

			ResumeLayout(false);
			PerformLayout();
		}

		void ResetTabOrder()
		{
			EntryLinesMessagesTabControl.TabPages.Remove(EntryHeaderTabPage);
			EntryLinesMessagesTabControl.TabPages.Insert(EntryHeaderTabPage, 1);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();

			if (CurrentDataItem != null)
			{
				if (CurrentDataItem.IsImport)
				{
					EntryLineGrid.AddToAvailableColumns(CusEntryLine.Schema.GSTVATAmount, CusEntryLine.Schema.ExciseAmount);
				}
				else
				{
					EntryLineGrid.RemoveFromAvailableColumns(CusEntryLine.Schema.GSTVATAmount, CusEntryLine.Schema.ExciseAmount);
				}
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			if (CurrentDataItem != null)
			{
				if (CurrentDataItem.IsImport)
				{
					TotalGSTVATAmountCalcEdit.Visible = true;
					TotalExciseAmountCalcEdit.Visible = true;
					TotalAntiDumpingAmountCalcEdit.Visible = true;
					TotalCountervailingAmountCalcEdit.Visible = true;
				}
				else
				{
					TotalGSTVATAmountCalcEdit.Visible = false;
					TotalExciseAmountCalcEdit.Visible = false;
					TotalAntiDumpingAmountCalcEdit.Visible = false;
					TotalCountervailingAmountCalcEdit.Visible = false;
				}
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			AddEntryHeaderContextMenus();
		}

		public static class Constant
		{
			public const string DeclarationDeadline = "Declaration+DeclarationDeadline";
		}

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		#region Entries Grid columns

		void AddEntryHeaderColumns()
		{
			var referenceColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_BGMReference);
			if (referenceColumn != null)
			{
				referenceColumn.CaptionResourceString = Res.GetData("734F3A42-E50D-413F-95DD-EA131A93AB51", "Local Reference Number");
				referenceColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			}

			var entryNumberColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber);
			if (entryNumberColumn != null)
			{
				entryNumberColumn.CaptionResourceString = Res.GetData("F246E393-A36F-443A-B8F3-1DAD4A8B0BF7", "Entry Number");
				entryNumberColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			}

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F01B82EF-E9F1-4590-9F9F-ADFF144A0601", "Pre Entry Number"),
				ColumnName = CusEntryHeader.Schema.PreEntryNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F67D4234-E7A1-4D5F-B5B7-44273FA802C0", "Declaration Unified Number"),
				ColumnName = CusEntryHeader.Schema.DeclarationUnifiedNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
			});

			var messageTypeColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageType);
			if (messageTypeColumn != null)
			{
				messageTypeColumn.IsMandatory = true;
				messageTypeColumn.GroupName = Res.GetData("0FD6C218-2E35-4294-BB33-70E15F657F9E", "Message Type");
			}

			var messageTypeDescColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_MessageTypeDescription);
			if (messageTypeDescColumn != null)
			{
				messageTypeDescColumn.GroupName = Res.GetData("0FD6C218-2E35-4294-BB33-70E15F657F9E", "Message Type");
			}

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6380BAD6-E8D8-4B3F-BCFE-D7A73BF409E5", "Entry Status"),
				ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
				GroupName = Res.GetData("9151FB19-0A1B-4468-A931-CA269EA45AE6", "Entry Status"),
				IsMandatory = true,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("992DEAAC-B461-422B-A8C0-5DBDCDD571E9", "Entry Status Desc."),
				ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
				GroupName = Res.GetData("9151FB19-0A1B-4468-A931-CA269EA45AE6", "Entry Status"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("12333522-3808-40E6-B051-A387B3F55693", "Package Type"),
				ColumnName = CusEntryHeader.Schema.CEI_PackageUQ,
				GroupName = Res.GetData("D3BF1760-1F8C-4374-B9E3-6F58606E8624", "Package Type"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("A68C5204-85B3-4C22-972A-5991E586FA90", "Package Type Desc."),
				ColumnName = CusEntryHeader.Schema.XC_PackageUQDescription,
				GroupName = Res.GetData("D3BF1760-1F8C-4374-B9E3-6F58606E8624", "Package Type"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("EF1B28B2-5CB6-4DFF-9C2C-C0BFCCA5D866", "Manual No"),
				ColumnName = CusEntryHeader.Schema.ManualNo,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("5765E3BB-FF79-4053-BC26-4C4D4E1E7739", "Vessel"),
				ColumnName = CusEntryHeader.Schema.VesselName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("7403F08E-88BF-4FA2-8AED-94E2401D231F", "Voyage"),
				ColumnName = CusEntryHeader.Schema.Voyage,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("362F8F11-4F16-42DA-A09B-936E7D69ABF8", "Bill of Lading"),
				ColumnName = CusEntryHeader.Schema.BillOfLading,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("BF2DA965-7DDE-4929-8110-72BEF1812320", "Customs Procedure"),
				ColumnName = CusEntryHeader.Schema.CustomsProcedureDesc,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("166ba6d8-026d-e381-412c-caaf3dd99f3b", "Incoterm"),
				ColumnName = CusEntryHeader.Schema.IncoTermDesc,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("7156BF88-348F-4B8E-B517-4B96D4B5F94F", "Count Of Lines"),
				ColumnName = CusEntryHeader.Schema.CountOfLines,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("ECEDEF50-3DC8-4DB1-8699-322F3EFB0971", "Invoice Numbers"),
				ColumnName = CusEntryHeader.Schema.InvoiceNumbers,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("C8AA43B2-C9F6-4F3D-A038-4DBE3CBCDCBE", "Contract Numbers"),
				ColumnName = CusEntryHeader.Schema.ContractNo,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("AE4A1470-7039-49AF-A6C0-82126DD59B8A", "Count Of Containers"),
				ColumnName = CusEntryHeader.Schema.NumberOfContainers,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("9E1FC212-BE0B-49BC-8783-9E8ECD15B49D", "Freight Fee Mark Desc."),
				ColumnName = CusEntryHeader.Schema.FreightFeeMarkDesc,
				GroupName = Res.GetData("CCE19205-CA9C-4AC7-8B16-8E249BFFCC48", "Freight Fee"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("5A02E6CD-DC7E-4887-B6BD-650C200DA6C0", "Freight Fee Amount"),
				ColumnName = CusEntryHeader.Schema.FreightFeeAmount,
				GroupName = Res.GetData("CCE19205-CA9C-4AC7-8B16-8E249BFFCC48", "Freight Fee"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("40CC6814-5ED7-4EAD-B114-9CDD540E13D1", "Freight Fee Currency"),
				ColumnName = CusEntryHeader.Schema.FreightFeeCurrencyCode,
				GroupName = Res.GetData("CCE19205-CA9C-4AC7-8B16-8E249BFFCC48", "Freight Fee"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("1E29F8B4-E4CB-4407-ACA3-E83CB6295633", "Gross Weight (KG)"),
				ColumnName = CusEntryHeader.Schema.GrossWeightInKG,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("2A01A83E-C59F-4FEC-9EE7-4446F376ACBE", "Net Weight (KG)"),
				ColumnName = CusEntryHeader.Schema.NetWeightInKG,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("95891626-E756-4557-89A1-0131B3FEEE66", "Entry Submitted Date"),
				ColumnName = CusEntryHeader.Schema.CH_EntrySubmittedDate,
				IsMandatory = true,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F17898C3-C669-406F-A707-4CC71CB384D7", "Release Date"),
				ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("4F749090-781E-41FE-986F-64A39EF689E4", "Declaration Date"),
				ColumnName = CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("94743BFB-0F7B-4DA0-A60E-96433FE3C248", "Status"),
				ColumnName = CusEntryHeader.Schema.CH_Status,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("3AF2613C-D401-4FD1-9649-0792F90ABFFE", "Status Desc."),
				ColumnName = CusEntryHeader.Schema.MessageStatusDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6EB205AC-AE6B-478D-A2A5-0297C2509E60", "CIQ Number"),
				ColumnName = CusEntryHeader.Schema.CIQNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("321913df-e5f1-4bd2-9e9a-e12ed585e73a", "CIQ Status"),
				ColumnName = CusEntryHeader.Schema.CIQStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("8a162ddb-cf5a-4f7e-ae83-37a31a7ad7fc", "CIQ Status Desc."),
				ColumnName = CusEntryHeader.Schema.CIQStatusDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("BC35E74A-70EF-4B4C-96BE-6AE279BAAA01", "Archive Date"),
				ColumnName = CusEntryHeader.Schema.ArchiveDate,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("84254C9E-03E1-427A-A7CE-4B91716E9D0C", "Archive User"),
				ColumnName = CusEntryHeader.Schema.ArchiveUser,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				ColumnName = nameof(CusEntryHeader.LastAuditedDate),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = nameof(CusEntryHeader.LastAuditedUser),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("098111FD-79C5-4789-8077-CA90A74FAF6C", "Deadline", "Declaration Deadline", ""),
				ColumnName = Constant.DeclarationDeadline,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("02B16099-F073-439D-B93B-1DC09720A5AE", "Days of Delay", "Days of Delayed Declaration", ""),
				ColumnName = nameof(CusEntryHeader.DaysOfDelayedDeclaration),
				GroupName = Res.GetData("5928CC3D-A830-48E7-97FF-ABA2EFF2FBD7", "Delayed Declaration"),
				IsReadOnly = true,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("8E8CDA61-B0A3-4586-905D-3A4E825EFDA3", "Fee for Delay", "Fee for Delayed Declaration", ""),
				ColumnName = nameof(CusEntryHeader.FeeForDelayedDeclaration),
				GroupName = Res.GetData("5928CC3D-A830-48E7-97FF-ABA2EFF2FBD7", "Delayed Declaration"),
				IsReadOnly = true,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200)
			});
		}

		void SetEntryHeaderColumns()
		{
			using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				EntriesBoundGrid.ReOrderColumns(ColumnNamesInSortOrder);
				EntriesBoundGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
				EntriesBoundGrid.SetColumnVisible(true, DefaultColumnsForGrid);
			}
		}

		string[] hiddenColumns;

		string[] HiddenColumns => hiddenColumns ?? (hiddenColumns = new string[]
		{
			CusEntryHeader.Schema.PreEntryNumber,
			CusEntryHeader.Schema.ManualNo,
			CusEntryHeader.Schema.VesselName,
			CusEntryHeader.Schema.Voyage,
			CusEntryHeader.Schema.BillOfLading,
			CusEntryHeader.Schema.CountOfLines,
			CusEntryHeader.Schema.ContractNo,
			CusEntryHeader.Schema.NumberOfContainers,
			CusEntryHeader.Schema.FreightFeeMarkDesc,
			CusEntryHeader.Schema.FreightFeeAmount,
			CusEntryHeader.Schema.FreightFeeCurrencyCode,
			CusEntryHeader.Schema.GrossWeightInKG,
			CusEntryHeader.Schema.NetWeightInKG,
			CusEntryHeader.Schema.ArchiveDate,
			CusEntryHeader.Schema.ArchiveUser,
			nameof(CusEntryHeader.LastAuditedDate),
			nameof(CusEntryHeader.LastAuditedUser),
		});

		string[] columnNamesInSortOrder;

		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new List<string>();
					columns.AddRange(DefaultColumnsForGrid);
					columns.AddRange(HiddenColumns);

					columnNamesInSortOrder = columns.ToArray();
				}
				return columnNamesInSortOrder;
			}
		}

		string[] defaultColumnsForGrid;

		string[] DefaultColumnsForGrid => defaultColumnsForGrid ?? (defaultColumnsForGrid = new[]
		{
			CusEntryHeader.Schema.CH_BGMReference,
			CusEntryHeader.Schema.DeclarationUnifiedNumber,
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.CIQNumber,
			CusEntryHeader.Schema.CH_MessageType,
			CusEntryHeader.Schema.CH_MessageTypeDescription,
			CusEntryHeader.Schema.CH_Status,
			CusEntryHeader.Schema.MessageStatusDescription,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CIQStatus,
			CusEntryHeader.Schema.CIQStatusDescription,
			CusEntryHeader.Schema.PackagesCount,
			CusEntryHeader.Schema.CEI_PackageUQ,
			CusEntryHeader.Schema.XC_PackageUQDescription,
			CusEntryHeader.Schema.CustomsProcedureDesc,
			CusEntryHeader.Schema.IncoTermDesc,
			CusEntryHeader.Schema.InvoiceNumbers,
			CusEntryHeader.Schema.CH_EntrySubmittedDate,
			CusEntryHeader.Schema.MovementReferenceNumberIssueDate,
			CusEntryHeader.Schema.CH_EntryReleaseDate
		});

		#endregion

		void AddEntryHeaderContextMenus()
		{
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(new ZMenuItem("-"));
			EntriesBoundGrid.SetColumnWidth(CusEntryHeader.Schema.EntryNumber, 130);

			var sendAcdAgrMessageMenu = new ZMenuItem(ResString.GetMultilingualString("717F1FC5-90D3-40BC-81EA-B9C2189D0F6A",
				"Send Agreement of Customs Declaration Agent Message"), SendACDAMessage);
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(sendAcdAgrMessageMenu);

			var showEntryNumbersUpdateFormMenu = new ZMenuItem(ResString.GetMultilingualString("5D7570F4-9901-42E7-9320-589B8349819E",
				"Update Entry Numbers"), ShowEntryNumbersUpdateForm);
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(showEntryNumbersUpdateFormMenu);

			var generateCustomsInvoiceAndPurchaseOrderMenu = new ZMenuItem(ResString.GetMultilingualString("58E6B86E-DAE5-445F-8A4D-C38AE43FE40D",
				"Generate Attachments"), GenerateAttachments);
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(generateCustomsInvoiceAndPurchaseOrderMenu);

			var exportExcelForSingleWindowImportMenu = new ZMenuItem(ResString.GetMultilingualString("BC174719-B7E6-46D0-9751-E66E0F101D3C",
				"Export Excel for Single Window Importing"), ExportExcelForSingleWindow);
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(exportExcelForSingleWindowImportMenu);

			var archiveRecordsMenu = new ZMenuItem(ResString.GetMultilingualString("0E1E8BD7-74E3-4574-A3DF-AF02003D5D8F", "Archive Entry"), ArchiveEntries);
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(archiveRecordsMenu);

			var writeToLogMenuItem = new ZMenuItem(auditEntryCaption, ShowAuditForm);
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(writeToLogMenuItem);
			EntriesBoundGrid.ContextMenu?.MenuItems.Add(new ZMenuItem("-"));
		}

		string auditEntryCaption => Res.GetString("BDC68F4F-226C-432B-AA08-7211110D5610", "Audit Entry");

		void AddEntryLineColumns()
		{
			var manualItemNoColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var tariffCodeColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var ciqTariffCodeColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var nameOfGoodsColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var specModelColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var tradeQuantityColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var tradeUnitQtyDescColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var totalPriceColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var unitPriceColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var currencyDescColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var goodOriginColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var finalDestColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var dutyModeColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var customsSuperConditionsColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var inspectionSuperConditionsColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var customsValueColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var dutyAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var exciseAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var vatAmountColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var certOfOriginColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var tradeAgreementCodeColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var certOfOriginCountryColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var itemNoOnCertOfOriginColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var certOfOriginTypeColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();

			manualItemNoColumnStyleInfo.CaptionResourceString = Res.GetData("6604B4D3-CDE4-4C26-AC49-0A35555D0AFB", "Manual Item No.");
			manualItemNoColumnStyleInfo.ColumnName = "ProductManualNo";
			manualItemNoColumnStyleInfo.Decimals = 0;
			manualItemNoColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			tariffCodeColumnStyleInfo.CaptionResourceString = Res.GetData("0EF3E26F-58FD-46E3-880D-6BE717B6EB79", "Tariff Code");
			tariffCodeColumnStyleInfo.ColumnName = "CL_AdValoremTariff";
			tariffCodeColumnStyleInfo.IsMandatory = true;
			tariffCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			ciqTariffCodeColumnStyleInfo.CaptionResourceString = Res.GetData("11588311-b7c3-4288-8070-8c76f1748c08", "CIQ", "CIQ Code", "CIQ Tariff Supplement Code");
			ciqTariffCodeColumnStyleInfo.ColumnName = "CIQSupplementCode";
			ciqTariffCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			nameOfGoodsColumnStyleInfo.CaptionResourceString = Res.GetData("F6FE8712-41B9-4678-B4FC-7C368BBD2D80", "Name of Goods");
			nameOfGoodsColumnStyleInfo.ColumnName = "NameOfGoods";
			nameOfGoodsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			specModelColumnStyleInfo.CaptionResourceString = Res.GetData("03e425ea-95f2-4187-bd4e-a665ca5a980c", "Spec & Model", "Specification & Model", "Specification And Model of Goods");
			specModelColumnStyleInfo.ColumnName = "GoodsSpecModel";
			specModelColumnStyleInfo.IsVisible = false;
			specModelColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tradeQuantityColumnStyleInfo.ColumnName = "TradeQuantity";
			tradeQuantityColumnStyleInfo.Decimals = 5;
			tradeQuantityColumnStyleInfo.GroupName = Res.GetData("187CCE8F-D8DD-428D-92CD-5466EBD8FEC4", "Invoice Qty");
			tradeQuantityColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			tradeUnitQtyDescColumnStyleInfo.ColumnName = "TradeUnitQtyDesc";
			tradeUnitQtyDescColumnStyleInfo.GroupName = Res.GetData("187CCE8F-D8DD-428D-92CD-5466EBD8FEC4", "Invoice Qty");
			tradeUnitQtyDescColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			totalPriceColumnStyleInfo.CaptionResourceString = Res.GetData("ACFACAB1-3424-456A-9974-A66CDD6C13CB", "Total Price");
			totalPriceColumnStyleInfo.ColumnName = "TotalPrice";
			totalPriceColumnStyleInfo.GroupName = Res.GetData("d99deea4-b323-49da-bb5b-a805af71a125", "Price");
			totalPriceColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			unitPriceColumnStyleInfo.CaptionResourceString = Res.GetData("61eceb06-4a9e-4ca5-8782-0b824e908844", "Unit Price");
			unitPriceColumnStyleInfo.ColumnName = "UnitPrice";
			unitPriceColumnStyleInfo.GroupName = Res.GetData("d99deea4-b323-49da-bb5b-a805af71a125", "Price");
			unitPriceColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			currencyDescColumnStyleInfo.CaptionResourceString = Res.GetData("A69EF0BB-CAF2-40B9-B786-3EADCF7310F3", "Currency");
			currencyDescColumnStyleInfo.ColumnName = "CurrencyDesc";
			currencyDescColumnStyleInfo.GroupName = Res.GetData("d99deea4-b323-49da-bb5b-a805af71a125", "Price");
			currencyDescColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			goodOriginColumnStyleInfo.CaptionResourceString = Res.GetData("0f4cc686-9fa6-473b-bc2f-a691a782c4cf", "Goods Origin");
			goodOriginColumnStyleInfo.ColumnName = "GoodsOriginName";
			goodOriginColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			finalDestColumnStyleInfo.CaptionResourceString = Res.GetData("1c5a3fb1-0b88-4758-8367-ea3fee1fb221", "Final Dest.");
			finalDestColumnStyleInfo.ColumnName = "GoodsDestName";
			finalDestColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			dutyModeColumnStyleInfo.CaptionResourceString = Res.GetData("7b093146-d922-4e68-8a3f-f64e6055018f", "Duty Mode");
			dutyModeColumnStyleInfo.ColumnName = "DutyModeDesc";
			dutyModeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			customsSuperConditionsColumnStyleInfo.CaptionResourceString = Res.GetData("4748B88A-7992-437C-9571-D7EDD0D02DD7", "Customs Supervision Conditions");
			customsSuperConditionsColumnStyleInfo.ColumnName = "CustomsSupervisionConditions";
			customsSuperConditionsColumnStyleInfo.IsVisible = false;
			customsSuperConditionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			inspectionSuperConditionsColumnStyleInfo.CaptionResourceString = Res.GetData("4CB8DB13-F1F9-4250-8907-04DD9293F41F", "Inspection Supervision Conditions");
			inspectionSuperConditionsColumnStyleInfo.ColumnName = "InspectionSupervisionConditions";
			inspectionSuperConditionsColumnStyleInfo.IsVisible = false;
			inspectionSuperConditionsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			customsValueColumnStyleInfo.CaptionResourceString = Res.GetData("7befde6c-08d0-465f-a747-6348ef5d3e03", "Customs Value");
			customsValueColumnStyleInfo.ColumnName = "CL_CustomsValue";
			customsValueColumnStyleInfo.Decimals = 0;
			customsValueColumnStyleInfo.IsVisible = false;
			customsValueColumnStyleInfo.IsReadOnly = true;
			customsValueColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			dutyAmountColumnStyleInfo.CaptionResourceString = Res.GetData("b580022d-2170-4e33-801e-b3ddb86068d4", "Duty Amount");
			dutyAmountColumnStyleInfo.ColumnName = "DutyAmount";
			dutyAmountColumnStyleInfo.IsVisible = false;
			dutyAmountColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			exciseAmountColumnStyleInfo.CaptionResourceString = Res.GetData("a0eb32a0-f323-41c4-8309-ac81fdf3c250", "Excise Amount");
			exciseAmountColumnStyleInfo.ColumnName = "ExciseAmount";
			exciseAmountColumnStyleInfo.IsVisible = false;
			exciseAmountColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			vatAmountColumnStyleInfo.CaptionResourceString = Res.GetData("4cb3772a-3ce6-4f89-9f72-7c4bd0ee866d", "VAT Amount");
			vatAmountColumnStyleInfo.ColumnName = "GSTVATAmount";
			vatAmountColumnStyleInfo.IsVisible = false;
			vatAmountColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			certOfOriginColumnStyleInfo.CaptionResourceString = Res.GetData("0EF3E26F-58FD-46E3-880D-6BE717B6EB71", "Certificate Of Origin");
			certOfOriginColumnStyleInfo.ColumnName = nameof(CusEntryLine.CertOfOriginNumber);
			certOfOriginColumnStyleInfo.IsVisible = false;
			certOfOriginColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			tradeAgreementCodeColumnStyleInfo.CaptionResourceString = Res.GetData("0EF3E26F-58FD-46E3-880D-6BE717B6EB72", "Preferential Code");
			tradeAgreementCodeColumnStyleInfo.ColumnName = nameof(CusEntryLine.TradeAgreementCode);
			tradeAgreementCodeColumnStyleInfo.IsVisible = false;
			tradeAgreementCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			certOfOriginCountryColumnStyleInfo.CaptionResourceString = Res.GetData("0EF3E26F-58FD-46E3-880D-6BE717B6EB73", "Origin Under FTA");
			certOfOriginCountryColumnStyleInfo.ColumnName = nameof(CusEntryLine.CertOfOriginCountry);
			certOfOriginCountryColumnStyleInfo.IsVisible = false;
			certOfOriginCountryColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			itemNoOnCertOfOriginColumnStyleInfo.CaptionResourceString = Res.GetData("0EF3E26F-58FD-46E3-880D-6BE717B6EB74", "Item No of COO");
			itemNoOnCertOfOriginColumnStyleInfo.ColumnName = nameof(CusEntryLine.ItemNoOnCertOfOrigin);
			itemNoOnCertOfOriginColumnStyleInfo.IsVisible = false;
			itemNoOnCertOfOriginColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			certOfOriginTypeColumnStyleInfo.CaptionResourceString = Res.GetData("0EF3E26F-58FD-46E3-880D-6BE717B6EB75", "COO Type");
			certOfOriginTypeColumnStyleInfo.ColumnName = nameof(CusEntryLine.CertOfOriginType);
			certOfOriginTypeColumnStyleInfo.IsVisible = false;
			certOfOriginTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			EntryLineGrid.ColumnStyles.Add(manualItemNoColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(tariffCodeColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(ciqTariffCodeColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(nameOfGoodsColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(specModelColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(tradeQuantityColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(tradeUnitQtyDescColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(unitPriceColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(totalPriceColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(currencyDescColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(goodOriginColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(finalDestColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(dutyModeColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(customsSuperConditionsColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(inspectionSuperConditionsColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(customsValueColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(dutyAmountColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(exciseAmountColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(vatAmountColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(certOfOriginColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(tradeAgreementCodeColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(certOfOriginCountryColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(itemNoOnCertOfOriginColumnStyleInfo);
			EntryLineGrid.ColumnStyles.Add(certOfOriginTypeColumnStyleInfo);
		}

		void SendACDAMessage(object sender, EventArgs e)
		{
			ProcessSingleEntryHeader(entryHeader =>
			{
				if (new AcdAgrMessageSender(Globals.Message).SendAndSave(entryHeader))
				{
					Globals.Message.Show(Res.GetString("3F451ECA-D267-4078-831A-67EB2B0845A5", "Agreement of Customs Declaration Agent Message message queued for sending."));
				}
			});
		}

		void ShowEntryNumbersUpdateForm(object sender, EventArgs e)
		{
			ProcessSingleEntryHeader(
				entryHeader =>
				{
					var entryNumbersBO = new EntryNumbersBO(entryHeader);
					var entryNumbersUpdateForm = new EntryNumbersUpdateForm(entryNumbersBO);
					ZFormModaliser.ShowDialogAndDispose(entryNumbersUpdateForm);
				});
		}

		void GenerateAttachments(object sender, EventArgs e)
		{
			ProcessSingleEntryHeader(
				entryHeader =>
				{
					var messages = new CusEntryHeaderAttachmentGenerator(entryHeader).GenerateAttachments();
					if (!messages.IsEmpty)
					{
						Globals.Message.ShowInformation(messages);
					}
				});
		}

		void ShowAuditForm(object sender, EventArgs e)
		{
			ProcessSingleEntryHeader(
				entryHeader =>
				{
					WriteToLogFormInvoker.ShowWriteToLogForm(
						entryHeader,
						entryHeader,
						entryHeader.HumanReadableName,
						Env.Security.CustomsDeclarationAudit,
						auditEntryCaption,
						ZString.Empty,
						new BusinessObjectLoggerOptions(),
						Globals.Message.ShowInformation,
						(succeeded) => RefreshLastAuditedInfos(succeeded, entryHeader)
					);
				}
			);
		}

		void RefreshLastAuditedInfos(bool succeeded, CusEntryHeader entryHeader)
		{
			if (succeeded)
			{
				entryHeader.LastAuditedDateInfo.RefreshBinding();
				entryHeader.LastAuditedUserInfo.RefreshBinding();
			}
		}

		void ProcessSingleEntryHeader(Action<CusEntryHeader> processDelegate)
		{
			if ((ParentForm as ZForm).PreSaveDeclaration(JobDeclaration))
			{
				var entryHeaders = EntriesBoundGrid.GetSelectedElements<CusEntryHeader>();
				if (entryHeaders != null && entryHeaders.Length == 1)
				{
					processDelegate(entryHeaders[0]);
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("5D591789-445F-430F-ACAB-D1A1368F61D0", "Please select one and only one Entry first."));
				}
			}
		}

		void ArchiveEntries(object sender, EventArgs e)
		{
			ProcessMultipleEntryHeaders(entryHeaders =>
			{
				var security = Env.Security.CustomsDeclarationArchive;
				if (security == null || !security.IsAllowed)
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("CC9FE251-AD1F-4A63-A8E2-39DB085AE39A", "You do not have the security rights to archive entries.\r\nPlease contact your system administrator to grant you the following security right:{0}", security.DisplayTextPathToSecurityRight));
				}
				else
				{
					foreach (var entryHeader in entryHeaders)
					{
						entryHeader.AddRecordArchivedLog();
					}
					Globals.Message.ShowInformation(ResString.GetMultilingualString("259FBE9D-A96E-439F-B6E3-A3B6E4F4A02E", "All selected entries have been archived."));
				}
			});
		}

		void ExportExcelForSingleWindow(object sender, EventArgs e)
		{
			ProcessMultipleEntryHeaders(entryHeaders =>
			{
				var shouldContinueWithExport = true;

				var parent = new CNJobDeclarationMessageSendingObjectParent(JobDeclaration);
				parent.SendingObjectsCollection.Cast<CNJobDeclarationMessageSendingObject>().ForEach(x => x.ShouldSend = entryHeaders.Contains(x.Header));

				var messageErrors = parent.BizObjValidationMessageErrors;
				if (!messageErrors.IsEmpty)
				{
					var caption = ResString.GetMultilingualString("F064879F-C772-4B55-8D0F-35E4146230BE", "Export excel with message errors");

					if (parent.SecurityCheckpointToSendWithMessageError.IsAllowed)
					{
						shouldContinueWithExport = Globals.Message.Show(messageErrors, caption, MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes;
					}
					else
					{
						Globals.Message.ShowError(messageErrors, caption);

						shouldContinueWithExport = false;
					}
				}

				if (shouldContinueWithExport)
				{
					var messages = new ZStringBuilder();
					messages.Append(new ExportExcelForSingleWindowHelper(entryHeaders).ExportAndSaveAsEDocs());

					if (messages.Length > 0)
					{
						messages.Append(ResString.GetMultilingualString("6bc08cda-aa7a-4995-8428-ed975f1de126", "You can find the excel files on the eDocs tab under each Entry.\r\nIf you cannot find, please click 'Refresh' button."));
						Globals.Message.ShowInformation(messages.ToStringWithNewLineBetweenAppends());
					}
				}
			});
		}

		void ProcessMultipleEntryHeaders(Action<IEnumerable<CusEntryHeader>> processDelegate)
		{
			if ((ParentForm as ZForm).PreSaveDeclaration(JobDeclaration))
			{
				var entryHeaders = EntriesBoundGrid.GetSelectedElements<CusEntryHeader>();
				if (entryHeaders != null && entryHeaders.Length > 0)
				{
					processDelegate(entryHeaders);
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("31A18CC7-0812-460B-8B2C-ACE249233815", "Please select one or more Entries first."));
				}
			}
		}

		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);
	}
}
