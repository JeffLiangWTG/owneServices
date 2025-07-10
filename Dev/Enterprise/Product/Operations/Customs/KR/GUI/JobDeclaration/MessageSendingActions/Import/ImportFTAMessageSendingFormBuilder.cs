using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportFTAMessageSendingFormBuilder : ImportAmendmentMessageSendingFormBuilder
	{
		public ImportFTAMessageSendingFormBuilder(ZString messageType)
		{
			this.messageType = messageType;
		}
		readonly ZString messageType;

		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var entryNumberColumnStyle = GetEntryNumberColumnStyle();
			entryNumberColumnStyle[0].IsReadOnly = true;

			var result = new List<ZTextBoxColumnStyleInfo>(entryNumberColumnStyle);
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.LawCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.DepartureDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.DeparturePort),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.CustomsDisbursementBill),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.ManufacturCompanyName),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.ManufacturAddress),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.ManufacturPostCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.DepartureCountry),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.TransshipmentYN),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					IsVisible = false
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.TransshipmentDate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.TransshipmentCountry),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.TransshipmentPort),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.ImporterCompanyName),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(FTAMessageSendingObject.ExporterCompanyName),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsVisible = false
				}
			});

			return result.ToArray();
		}

		public override ZUserControl GetUserControl() => new ImportFTAEntryLinesMessageSendingUserControl();
		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("F8DDD6C7-6372-48D0-A1F3-A33A7709C8C4", "FTA Entry Lines");
		public override ZTabPage[] GetAdditionalTabPages(ZGrid messageSendingObjectsGrid)
		{
			var ftaEntryDetailsUserControl = new ImportFTAMessageDetailsUserControl();
			ftaEntryDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			var ftaEntryDetailsTabPage = new ZTabPage();
			ftaEntryDetailsTabPage.CaptionResourceString = Res.GetData("2D87B267-CE54-4F14-9194-A5BA7AF935F6", "FTA Entry Details");
			ftaEntryDetailsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			ftaEntryDetailsTabPage.Controls.Add(ftaEntryDetailsUserControl);
			
			var ftaEntryLineDetailsUserControl = new ImportFTAEntryLineDetailsUserControl();
			ftaEntryLineDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			var ftaEntryLineDetailsTabPage = new ZTabPage();
			ftaEntryLineDetailsTabPage.CaptionResourceString = Res.GetData("1F15ABDD-2144-4D3B-9908-52E7FDF2273C", "FTA Entry Line Details");
			ftaEntryLineDetailsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			ftaEntryLineDetailsTabPage.Controls.Add(ftaEntryLineDetailsUserControl);

			var result = new ZTabPage[] { ftaEntryDetailsTabPage, ftaEntryLineDetailsTabPage };

			if (messageType == ElectronicDocumentTypeList.Codes._DHR)
			{
				var ftaInvoiceLineDetailsUserControl = new ImportFTAInvoiceLineDetailsUserControl();
				ftaInvoiceLineDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
				var ftaInvoiceLineDetailsTabPage = new ZTabPage();
				ftaInvoiceLineDetailsTabPage.CaptionResourceString = Res.GetData("32AD745B-85FA-4BEF-8E86-140AD599D531", "FTA Invoice Line Details");
				ftaInvoiceLineDetailsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
				ftaInvoiceLineDetailsTabPage.Controls.Add(ftaInvoiceLineDetailsUserControl);

				result = new ZTabPage[] { ftaEntryDetailsTabPage, ftaEntryLineDetailsTabPage, ftaInvoiceLineDetailsTabPage };
			}

			return result;
		}

		public override int[] GetFormSize() => new int[] { 950, 640 };
		public override int Panel1MinSize => Panel1MinSizeForGridUserControl;
		public override int Panel2MinSize => 267;
	}
}
