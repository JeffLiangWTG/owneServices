using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
{
	public ExportInvoiceLineUserControl()
	{
		InitializeComponent();
		HideControlsInLineDetailsTab();
		AddRemarksPageAfterAdditionalInfoPageInLineDetailsTabControl();
	}

	protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalInfo_44;

	protected override string[] GetDefaultColumnsForGrid()
	{
		if (IsUcc6Export)
		{
			return GetDefaultColumnsForGridForUcc6Export();
		}

		var columnsForGrid = base.GetDefaultColumnsForGrid().ToList();

		AddOriginStateColumnNextToCountryOfOrigin();

		return columnsForGrid.ToArray();

		void AddOriginStateColumnNextToCountryOfOrigin()
		{
			var countryOfOriginIndex = columnsForGrid.IndexOf(JobComInvoiceLine.Schema.JI_CountryOfOrigin);
			columnsForGrid.Insert(++countryOfOriginIndex, JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin);
		}
	}

	string[] GetDefaultColumnsForGridForUcc6Export() => new[]
	{
		JobComInvoiceLine.Schema.JI_LineNo,
		JobComInvoiceLine.Schema.JI_CEI,
		JobComInvoiceLine.Schema.EntryInstructionDescription,
		JobComInvoiceLine.Schema.JI_FormattedProcedure,
		JobComInvoiceLine.Schema.JI_CountryOfOrigin,
		JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
		JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
		AddInfoJobComInvoiceLine.Schema.ZG_CountryOfDestination,
		JobComInvoiceLine.Schema.JI_PartNo,
		JobComInvoiceLine.Schema.JI_FormattedTariff,
		JobComInvoiceLine.Schema.JI_Description,
		JobComInvoiceLine.Schema.JI_SupplementaryCode1,
		JobComInvoiceLine.Schema.JI_SupplementaryCode2,
		AddInfoJobComInvoiceLine.Schema.ZG_PortTaxRate,
		AddInfoJobComInvoiceLine.Schema.ZG_CusNumber,
		JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
		JobComInvoiceLine.Schema.JI_LinePrice,
		JobComInvoiceLine.Schema.JI_Weight,
		JobComInvoiceLine.Schema.JI_WeightUQ,
		JobComInvoiceLine.Schema.JI_NetWeight,
		JobComInvoiceLine.Schema.JI_NetWeightUQ,
		JobComInvoiceLine.Schema.JI_CustomsQuantity,
		JobComInvoiceLine.Schema.JI_CustomsUnitQty,
		JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
		JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
		JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
		JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
		JobComInvoiceLine.Schema.JI_Volume,
		JobComInvoiceLine.Schema.JI_VolumeUQ,
		JobComInvoiceLine.Schema.JI_BondedWhsQuantity,
		JobComInvoiceLine.Schema.JI_BondedWhsUnitQty,
		JobComInvoiceLine.Schema.JI_PreviousEntryNumber,
		JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber,
		JobComInvoiceLine.Schema.EntryReferenceNumber,
		JobComInvoiceLine.Schema.MergedLineNumber,
	};

	protected override void AddColumnsToGrid()
	{
		base.AddColumnsToGrid();

		var columns = new List<ZGridColumnInfo>
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_StateOrRegionOfOrigin,
				CaptionResourceString = Res.GetData("2746F924-4F2F-43C9-962E-9144D7176BDE", "[34b] Orig. Prov."),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			}
		};

		if (IsUcc6Export)
		{
			columns.AddRange(new ZGridColumnInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = AddInfoJobComInvoiceLine.Schema.ZG_PortTaxRate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
					GroupName = Res.GetData("2746F924-4F2F-43C9-962E-9144D7176B14", "Third Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
					GroupName = Res.GetData("2746F924-4F2F-43C9-962E-9144D7176B14", "Third Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				}
			});
		}

		CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(columns);
	}

	public new JobComInvoiceLine CurrentInvoiceLine => (JobComInvoiceLine)base.CurrentInvoiceLine;

	protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLineLayoutSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType()
		=> IsUcc6Export ? typeof(LayoutUcc6ExportPreviousDocumentsUserControl) : typeof(PreviousDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType()
		=> IsUcc6Export ? typeof(Ucc6ExportInvLineAddInfoUserControlWithGrid) : typeof(AdditionalInfosUserControl);

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override ZBool HasDifferentPanelLayout => ZBool.True;

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();
		SetRemarksTabPageVisibility();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		AdjustAdditionalInfoTabOrder();
	}

	void AdjustAdditionalInfoTabOrder()
	{
		if (!IsUcc6Export)
		{
			return;
		}

		LineDetailTabControl.TabPages.Remove(AdditionalInfosTabPage);
		var previousDocumentTabIndex = LineDetailTabControl.TabPages.IndexOf(PreviousDocumentsTabPage);
		LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, previousDocumentTabIndex + 1);
	}

	void AddRemarksPageAfterAdditionalInfoPageInLineDetailsTabControl()
	{
		var additionalInfosTabPageIndex = LineDetailTabControl.TabPages.IndexOf(AdditionalInfosTabPage);
		LineDetailTabControl.TabPages.Insert(RemarksTabPage, additionalInfosTabPageIndex + 1);
	}

	void HideControlsInLineDetailsTab()
	{
		PrincipalsRepresentativeGroupBox.Visible = false;
	}

	void SetRemarksTabPageVisibility()
	{
		RemarksTabPage.TabVisible = !IsUcc6Export;
	}

	bool IsUcc6Export
	{
		get
		{
			var jobDeclaration = CurrentDataItem as JobDeclaration ?? JobDeclaration as JobDeclaration;
			return jobDeclaration?.IsUCC6AndIsExport ?? false;
		}
	}

	protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => declaration?.IsUCC6AndIsExport ?? false ? EU.GUI.CaptionProvider.PreviousDocuments : base.GetPreviousDocumentsTabPageCaption(declaration);
}
