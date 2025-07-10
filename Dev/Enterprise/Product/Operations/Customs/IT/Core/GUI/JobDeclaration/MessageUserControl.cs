using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class MessageUserControl : EU.GUI.MessageUserControl
{
	public MessageUserControl()
	{
		InitializeComponent();
		SetUpEntryHeaderColumns();
		SetUpEntryLinesColumns();
		SetUpEntryLinesMessagesTabControlPages();

		requestToCustomsMenuItem = EntriesBoundGrid.ContextMenu.MenuItems.Add(Res.GetString("44D6E06C-376B-4AE1-B02F-7EAAE44D8CCF", "Request to Customs"));

		ResetCancelledEntryContextMenu.Initialize();
		ManualReleaseContextMenu.Initialize();
		AmendmentContextMenu.Initialize();
		Ucc6EFStatusRequestGridContextMenuComponent.Initialize();
		ReleaseProspectusRequestGridContextMenuComponent.Initialize();
		AccountingSummaryRequestGridContextMenuComponent.Initialize();
		AccountingSummaryDownloadGridContextMenu.Initialize();
		IvistoRequestGridContextMenuComponent.Initialize();
		Ucc6EADRequestGridContextMenuComponent.Initialize();
		Eur1RequestGridContextMenuComponent.Initialize();
		SummaryProspectusRequestGridContextMenuComponent.Initialize();
		SummaryProspectusDownloadGridContextMenuComponent.Initialize();
	}

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();
		SetAllM2LinesTabPageVisibility();
	}

	protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new EntryLineAdditionalDataUserControl();

	protected override IPanelLayoutProvider GetNewEntryDetailsPanelLayout() => new EntryDetailsLayout();

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

	void SetUpEntryHeaderColumns()
	{
		var expectedVisibleColumnsInOrder = new Dictionary<ZInt, ZString>()
		{
			{ 0, CusEntryHeader.Schema.EntryTypeFriendlyName },
			{ 1, CusEntryHeader.Schema.CH_BGMReference },
			{ 2, "CusEntryNumber+CE_IssueDate" },
			{ 3,CusEntryHeader.Schema.CH_IncoTerm },
			{ 4, CusEntryHeader.Schema.PackagesCount },
			{ 5, CusEntryHeader.Schema.InvoiceAmount },
			{ 6, CusEntryHeader.Schema.InvoiceAmountCurrency },
			{ 7, CusEntryHeader.Schema.CH_FreightAdjustment },
			{ 8, CusEntryHeader.Schema.Duty },
			{ 9, CusEntryHeader.Schema.VAT },
			{ 10, CusEntryHeader.Schema.CH_Status },
			{ 11, CusEntryHeader.Schema.MessageStatusDescription },
			{ 12, CusEntryHeader.Schema.CH_EntryStatus },
			{ 13, CusEntryHeader.Schema.EntryHeaderStatusDescription },
			{ 14, CusEntryHeader.Schema.CH_MessageType },
			{ 15, CusEntryHeader.Schema.CH_MessageTypeDescription },
			{ 16, CusEntryHeader.Schema.EntryNumber },
			{ 17, "CusEntryNumber+CE_EntryLineReference" },
			{ 18, CusEntryHeader.Schema.CH_EntrySubmittedDate },
			{ 19, CusEntryHeader.Schema.MovementReferenceNumber },
			{ 20, CusEntryHeader.Schema.ReleaseCode },
			{ 21, CusEntryHeader.Schema.CH_EntryReleaseDate },
		}.ToImmutableDictionary();

		var cachedColumnStyles = EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
		EntriesBoundGrid.ColumnStyles.Clear();

		AddVisibleExpectedColumns(expectedVisibleColumnsInOrder, cachedColumnStyles);
		AddInvisibleNotExpectedColumns(expectedVisibleColumnsInOrder, cachedColumnStyles);
	}

	void AddVisibleExpectedColumns(ImmutableDictionary<ZInt, ZString> expectedVisibleColumnsInOrder, ZGridColumnInfo[] cachedColumnStyles)
	{
		foreach (var expectedColumn in expectedVisibleColumnsInOrder)
		{
			var columnStyle = GetColumnInfo(expectedColumn.Value, cachedColumnStyles);
			if (columnStyle != null)
			{
				columnStyle.IsVisible = true;
				EntriesBoundGrid.ColumnStyles.Add(columnStyle);
			}
		}
	}

	void AddInvisibleNotExpectedColumns(ImmutableDictionary<ZInt, ZString> expectedVisibleColumnsInOrder, ZGridColumnInfo[] cachedColumnStyles)
	{
		var notExpectedColumnsFromBase = cachedColumnStyles.Where(x => !expectedVisibleColumnsInOrder.ContainsValue(x.ColumnName)).ToArray();
		foreach (var notExpectedColumn in notExpectedColumnsFromBase)
		{
			notExpectedColumn.IsVisible = false;
		}
		EntriesBoundGrid.ColumnStyles.AddRange(notExpectedColumnsFromBase);
	}

	ZGridColumnInfo GetColumnInfo(ZString columnName, ZGridColumnInfo[] columnStyleList)
	{
		var columnStyle = columnStyleList.SingleOrDefault(x => x.ColumnName == columnName);
		if (columnStyle == null)
		{
			switch (columnName)
			{
				case CusEntryHeader.Schema.CH_IncoTerm:
					columnStyle = new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						ColumnName = CusEntryHeader.Schema.CH_IncoTerm,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78)
					};
					break;

				case CusEntryHeader.Schema.InvoiceAmount:
					columnStyle = new ZArchitecture.ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						ColumnName = CusEntryHeader.Schema.InvoiceAmount,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103)
					};
					break;

				case CusEntryHeader.Schema.InvoiceAmountCurrency:
					columnStyle = new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("50525C99-2B4F-4FAC-987A-47B4C108B5B1", "Invoice Currency"),
						ColumnName = CusEntryHeader.Schema.InvoiceAmountCurrency,
						IsReadOnly = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105)
					};
					break;

				case CusEntryHeader.Schema.CH_FreightAdjustment:
					columnStyle = new ZArchitecture.ZCalcEditColumnStyleInfo
					{
						BindToDecimalPlaces = null,
						ColumnName = CusEntryHeader.Schema.CH_FreightAdjustment,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(119)
					};
					break;

				case "CusEntryNumber+CE_EntryLineReference":
					columnStyle = new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("0FD78C75-87DF-41F8-9F32-2320B699CA85", "Customs Office"),
						ColumnName = "CusEntryNumber+CE_EntryLineReference",
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105)
					};
					break;

				case CusEntryHeader.Schema.ReleaseCode:
					columnStyle = new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("81380FBB-4ADF-4ABA-97EF-4787D8A10C57", "Release Code"),
						ColumnName = CusEntryHeader.Schema.ReleaseCode,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					};
					break;

				default:
					break;
			}
		}
		return columnStyle;
	}

	void SetUpEntryLinesColumns()
	{
		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.ZG_LinesValue
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.LinesValueInInvoiceCurrency
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.ZG_AdjustmentAmount
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = "ProcedureCodeWithoutConcession",
			CaptionResourceString = Res.GetData("14524215-2969-48D0-AF47-C43999A1A5B3", "CPC"),
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = "CountryOfOriginCode",
			CaptionResourceString = Res.GetData("356FDB58-B0A8-4BE2-BC53-1342EE526951", "Origin"),
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = "PreferenceCode",
			CaptionResourceString = Res.GetData("61EA69A8-03E4-427A-9706-DE03306BE910", "Preference"),
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = "ValuationMethod",
			CaptionResourceString = Res.GetData("759FBFA3-F021-4E9F-93D8-2D334AE2C866", "Valuation Code"),
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = CusEntryLine.Schema.PackageType,
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = CusEntryLine.Schema.SteelType,
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = "QuotaOrderNumber",
			CaptionResourceString = Res.GetData("F9028D57-BC00-40B7-B2AB-A58546E099F7", "Quota"),
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = CusEntryLine.Schema.ReleaseCode
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			ColumnName = CusEntryLine.Schema.ReleaseDate
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.EffectiveGrossWeightKg,
			IsVisible = true,
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.EffectiveNetWeightKg,
			IsVisible = true,
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.EffectiveCustomsWeightKg,
			IsVisible = true,
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.EffectiveSupplementaryQuantity,
			IsVisible = true,
		});

		EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			BindToDecimalPlaces = null,
			ColumnName = CusEntryLine.Schema.NumberOfPackages,
			IsVisible = true,
		});

		EntryLineGrid.ReOrderColumns(
			[
				CusEntryLine.Schema.CL_LineNumber,
				CusEntryLine.Schema.LineSubmissionStatusDescription,
				CusEntryLine.Schema.FormattedTariff,
				CusEntryLine.Schema.EffectiveDescription,
				CusEntryLine.Schema.EffectiveGrossWeightKg,
				CusEntryLine.Schema.EffectiveNetWeightKg,
				CusEntryLine.Schema.EffectiveCustomsWeightKg,
				CusEntryLine.Schema.EffectiveSupplementaryQuantity,
				CusEntryLine.Schema.NumberOfPackages,
				CusEntryLine.Schema.DutyAmount,
				CusEntryLine.Schema.CL_DutyPercent,
				CusEntryLine.Schema.GSTVATAmount,
				CusEntryLine.Schema.GSTVATDeferred,
				CusEntryLine.Schema.CL_CustomsValue,
				CusEntryLine.Schema.CL_StatisticalValue,
				CusEntryLine.Schema.ZG_LinesValue,
				CusEntryLine.Schema.LinesValueInInvoiceCurrency,
				CusEntryLine.Schema.ZG_AdjustmentAmount,
				CusEntryLine.Schema.ProcedureCodeWithoutConcession,
				CusEntryLine.Schema.CountryOfOriginCode,
				CusEntryLine.Schema.PreferenceCode,
				CusEntryLine.Schema.ValuationMethod,
				CusEntryLine.Schema.PackageType,
				CusEntryLine.Schema.SteelType,
				CusEntryLine.Schema.QuotaOrderNumber,
				CusEntryLine.Schema.ReleaseCode,
				CusEntryLine.Schema.ReleaseDate
			]);
	}

	void SetUpEntryLinesMessagesTabControlPages()
	{
		var expectedTabPagesInOrder = new Dictionary<ZInt, ZTabPage>()
		{
			{ 0, NewEntryDetailsTabPage },
			{ 1, EntryLinesTabPage },
			{ 2, AllM2LinesTabPage },
			{ 3, MessageTabPage },
		}.ToImmutableDictionary();

		EntryLinesMessagesTabControl.OrderTabPages(expectedTabPagesInOrder);
	}

	protected override void SetEntryAsFailedFromTransmissionCore(object sender, EventArgs ev)
	{
		int entries = 0;

		if (EntriesBoundGrid.SelectedElements.Length == 0)
		{
			Globals.Message.Show(Res.GetString("7762779B-DD5E-473C-8763-40A89FAE2000", "Please select a row first"));
		}
		else
		{
			var selectedCusEntryHeaders = EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>();
			var notAllowedEntryHeader = selectedCusEntryHeaders.FirstOrDefault(header => !header.EntryStatusHasHigherPriorty(ITEntryStatusList.Codes.Registered));
			if (notAllowedEntryHeader != null)
			{
				Globals.Message.Show(Res.GetString("446AD746-C02B-4D7C-979E-9A6F35084089", "You cannot set an Entry with entry status {0} as failed for transmission", notAllowedEntryHeader.CH_EntryStatus));
			}
			else
			{
				var headersWithRecendIdoc = selectedCusEntryHeaders.Where(header => HasRecentIdoc(header));
				Customs.Business.ISendsMessagesToCustoms messageInitiator;
				ZString message, caption, confirmationPrompt, confirmationString;
				if (headersWithRecendIdoc.Any())
				{
					messageInitiator = headersWithRecendIdoc.First().Declaration.MessageInitiator;
					message = Res.GetString("56B4B28F-D732-4C1E-AD51-AEC2DDE1D815", "The last message for one or more Entries was sent less than half an hour ago, there could be a delay in the response.Are you really sure you want to set the Entry as failed for transmission ?");
				}
				else
				{
					messageInitiator = selectedCusEntryHeaders.First().Declaration.MessageInitiator;
					message = Res.GetString("1D6F08D7-7F83-4B35-8113-7FCAC8D61F3A", "Are you sure you want to set this Entry as Failed from Transmission?");
				}

				caption = Res.GetString("63CD1AC3-B8B8-4CD6-BEEC-63BE09F5B127", "Failed from Transmission");
				confirmationPrompt = Res.GetString("C5ECCEB5-9C43-4799-B665-ECFC7E903A1A", "If you are absolutely sure you want to set this Entry as Failed From Transmission, please type:");
				confirmationString = Res.GetString("2A801067-C2DB-4BD5-8015-1D587A26716B", "confirm");

				var result = Globals.Message.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, ZMessageBoxIcon.Warning);
				if (result == ZDialogResult.OK)
				{
					EntriesBoundGrid.SelectedElements.Cast<CusEntryHeader>().ForEach(header =>
					{
						header.SetAsFailedFromTransmission();
						entries++;
					});

					if (entries == 1)
					{
						Globals.Message.Show(Res.GetString("6CC12424-E33A-4307-B788-171912DAE17A", "One Entry was set to Failed from Transmission"));
					}
					else
					{
						Globals.Message.Show(Res.GetString("00F95B79-8595-4815-B3CC-FD4F24038997", "{0} Entries were set to Failed from Transmission", entries.ToString(Culture.Current)));
					}
				}
			}
		}
	}

	bool HasRecentIdoc(CusEntryHeader header)
	{
		var recentIdoc = header.Messages.GetLastMessageByType(SADConstants.CustomsInterchangeType.IdocR);
		return recentIdoc != null && ((ZDateTime.Now - recentIdoc.EM_MessageDateTime).TotalMinutes <= 30);
	}

	CusEntryHeader CurrentEntryHeaderFromListManager => (CusEntryHeader)(EntriesBoundGrid.ListManager?.GetCurrent() ?? EntriesBoundGrid.List?.Cast<BusinessObject>().FirstOrDefault());

	void EntriesBoundGridOnAfterBind(object sender, EventArgs e)
	{
		EntriesBoundGrid.ListManager.PositionChanged += ListManagerOnPositionChanged;
		ListManagerOnPositionChanged(null, null);
	}

	void ListManagerOnPositionChanged(object sender, EventArgs eventArgs)
	{
		var listManager = EntriesBoundGrid.ListManager;
		if (listManager != null)
		{
			if (listManager.Count > 0)
			{
				var currentEntryFromListManager = CurrentEntryHeaderFromListManager;
				if (currentEntryFromListManager != null)
				{
					bool hasChanged = currentEntryHeader != currentEntryFromListManager;
					if (hasChanged)
					{
						currentEntryHeader = currentEntryFromListManager;
					}
				}
			}
			else
			{
				currentEntryHeader = null;
			}
		}
	}

	CusEntryHeader currentEntryHeader;

	void SetAllM2LinesTabPageVisibility()
	{
		var declaration = CurrentDataItem;
		AllM2LinesTabPage.TabVisible = declaration != null && !declaration.IsUCC6;
	}

	protected override void Dispose(bool disposing)
	{
		ResetCancelledEntryContextMenu.Dispose();
		ManualReleaseContextMenu.Dispose();
		AmendmentContextMenu.Dispose();
		AccountingSummaryDownloadGridContextMenu.Dispose();
		Ucc6EFStatusRequestGridContextMenuComponent.Dispose();
		ReleaseProspectusRequestGridContextMenuComponent.Dispose();
		AccountingSummaryRequestGridContextMenuComponent.Dispose();
		IvistoRequestGridContextMenuComponent.Dispose();
		Ucc6EADRequestGridContextMenuComponent.Dispose();
		Eur1RequestGridContextMenuComponent.Dispose();
		SummaryProspectusRequestGridContextMenuComponent.Dispose();
		SummaryProspectusDownloadGridContextMenuComponent.Dispose();
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	ResetCancelledEntryGridContextMenuItemComponent ResetCancelledEntryContextMenu => resetCancelledEntryContextMenu ?? (resetCancelledEntryContextMenu = new ResetCancelledEntryGridContextMenuItemComponent(EntriesBoundGrid));
	ResetCancelledEntryGridContextMenuItemComponent resetCancelledEntryContextMenu;

	ManualReleaseGridContextMenuItemComponent ManualReleaseContextMenu => manualReleaseContextMenu ?? (manualReleaseContextMenu = new ManualReleaseGridContextMenuItemComponent(EntriesBoundGrid));
	ManualReleaseGridContextMenuItemComponent manualReleaseContextMenu;

	AmendmentGridContextMenuItemComponent AmendmentContextMenu => amendmentContextMenu ?? (amendmentContextMenu = new AmendmentGridContextMenuItemComponent(EntriesBoundGrid));
	AmendmentGridContextMenuItemComponent amendmentContextMenu;

	ReleaseProspectusRequestGridContextMenuComponent ReleaseProspectusRequestGridContextMenuComponent => releaseProspectusRequestGridContextMenuComponent ??= new ReleaseProspectusRequestGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	ReleaseProspectusRequestGridContextMenuComponent releaseProspectusRequestGridContextMenuComponent;

	AccountingSummaryRequestGridContextMenuComponent AccountingSummaryRequestGridContextMenuComponent => accountingSummaryRequestGridContextMenuComponent ??= new AccountingSummaryRequestGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	AccountingSummaryRequestGridContextMenuComponent accountingSummaryRequestGridContextMenuComponent;

	Ucc6EFStatusRequestGridContextMenuComponent Ucc6EFStatusRequestGridContextMenuComponent => ucc6EFStatusRequestGridContextMenuComponent ??= new Ucc6EFStatusRequestGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	Ucc6EFStatusRequestGridContextMenuComponent ucc6EFStatusRequestGridContextMenuComponent;

	AccountingSummaryDownloadGridContextMenuComponent AccountingSummaryDownloadGridContextMenu => accountingSummaryDownloadGridContextMenu ??= new(EntriesBoundGrid, requestToCustomsMenuItem);
	AccountingSummaryDownloadGridContextMenuComponent accountingSummaryDownloadGridContextMenu;

	IvistoRequestGridContextMenuComponent IvistoRequestGridContextMenuComponent => ivistoRequestGridContextMenuComponent ??= new IvistoRequestGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	IvistoRequestGridContextMenuComponent ivistoRequestGridContextMenuComponent;

	Ucc6EADRequestGridContextMenuComponent Ucc6EADRequestGridContextMenuComponent => ucc6EADRequestGridContextMenuComponent ??= new Ucc6EADRequestGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	Ucc6EADRequestGridContextMenuComponent ucc6EADRequestGridContextMenuComponent;

	Eur1RequestGridContextMenuComponent Eur1RequestGridContextMenuComponent => eur1RequestGridContextMenuComponent ??= new Eur1RequestGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	Eur1RequestGridContextMenuComponent eur1RequestGridContextMenuComponent;

	SummaryProspectusRequestGridContextMenuComponent SummaryProspectusRequestGridContextMenuComponent => summaryProspectusRequestGridContextMenuComponent ??= new SummaryProspectusRequestGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	SummaryProspectusRequestGridContextMenuComponent summaryProspectusRequestGridContextMenuComponent;

	SummaryProspectusDownloadGridContextMenuComponent SummaryProspectusDownloadGridContextMenuComponent => summaryProspectusDownloadGridContextMenuComponent ??= new SummaryProspectusDownloadGridContextMenuComponent(EntriesBoundGrid, requestToCustomsMenuItem);
	SummaryProspectusDownloadGridContextMenuComponent summaryProspectusDownloadGridContextMenuComponent;

	readonly MenuItem requestToCustomsMenuItem;
}
