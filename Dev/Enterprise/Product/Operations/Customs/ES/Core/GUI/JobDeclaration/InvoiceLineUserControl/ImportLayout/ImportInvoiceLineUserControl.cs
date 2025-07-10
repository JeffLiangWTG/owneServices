using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
{
	public ImportInvoiceLineUserControl()
	{
		InitializeComponent();
		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		AdditionalDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitAdditionalDocumentsUserControl());
	}

	bool IsUCC6AndIsImport => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6AndIsImport;

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

	protected override Type GetOrganizationsUserControlType() => typeof(ImportInvoiceLineOrganizationsUserControl);

	Type GetAdditionalDocumentsUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

	protected override Type GetAdditionalInfosUserControlType() => IsUCC6AndIsImport
			? typeof(AdditionalInfosUserControlWithGrid)
			: base.GetAdditionalInfosUserControlType();

	protected override Type GetValuationIndicatorsUserControlType() => typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl);

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		var additionalProcedure = NewLineDetailsTabPage.FindSingleOrDefault<EU.GUI.AdditionalProcedureCodesUserControl>(x => x.Name == "AdditionalProcedureCodesUserControl");
		if (additionalProcedure != null)
		{
			additionalProcedure.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("57D9C55A-07F7-4F4C-8501-DA6CA56C9322", "[37.2] Nat./UE Reg.");
		}
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetupTabsOrder();
	}

	void InitAdditionalDocumentsUserControl()
	{
		additionalDocumentsUserControl.UserControlType = GetAdditionalDocumentsUserControlType();
		additionalDocumentsUserControl.HostedControlCreated += (sender, args) =>
		{
			var hostedControl = additionalDocumentsUserControl.HostedControl;
			if (hostedControl is EU.GUI.PlugIn.ISupportingInfoUserControls)
			{
				EU.GUI.PlugIn.SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(hostedControl, "FilteredInvoiceLines", SupportingInfoColumnLayoutContext, GetAdditionalDocumentsColumnWidths());
			}
		};
	}

	protected virtual EU.GUI.PlugIn.ColumnWidth[] GetAdditionalDocumentsColumnWidths()
	{
		return new EU.GUI.PlugIn.ColumnWidth[4]
		{
			new EU.GUI.PlugIn.ColumnWidth("CSI_Code", 47),
			new EU.GUI.PlugIn.ColumnWidth("CSI_Description", 77),
			new EU.GUI.PlugIn.ColumnWidth("CSI_RN_NKCountryCode", 142),
			new EU.GUI.PlugIn.ColumnWidth("CSI_NctsExportFromEC", 89)
		};
	}

	protected const string SupportingInfoColumnLayoutContext = "DEC";

	void SetupTabsOrder()
	{
		if (!IsUCC6AndIsImport)
		{
			var indexSupportingDocumentsTabPage = LineDetailTabControl.TabPages.IndexOf(SupportingDocumentsTabPage);
			LineDetailTabControl.TabPages.Insert(AdditionalDocumentsTabPage, indexSupportingDocumentsTabPage + 1);
		}
		
		var indexPackagesPivotTabPage = LineDetailTabControl.TabPages.IndexOf(PackagesPivotTabPage);
		LineDetailTabControl.TabPages.Insert(VehiclesTabPage, indexPackagesPivotTabPage + 1);
	}

	ICommonInvoiceDataProvider ESDeclaration => (ICommonInvoiceDataProvider)JobDeclaration;
	public new JobComInvoiceLine CurrentInvoiceLine => (JobComInvoiceLine)base.CurrentInvoiceLine;

	public override Customs.Business.ICommonInvoiceDataProvider JobDeclaration
	{
		get => base.JobDeclaration;
		set
		{
			UnhookJobDeclarationEvents(ESDeclaration);
			base.JobDeclaration = value;
			HookJobDeclarationEvents(ESDeclaration);
		}
	}

	void HookJobDeclarationEvents(ICommonInvoiceDataProvider declaration)
	{
		if (declaration != null)
		{
			declaration.ZG_DestinationStateInfo.ValueChanged += ZG_DestinationStateInfo_ValueChanged;

			ZG_DestinationStateInfo_ValueChanged(this, EventArgs.Empty);
		}
	}

	void ZG_DestinationStateInfo_ValueChanged(object sender, EventArgs e)
	{
		ChangeLineCalculationsLabels();
	}

	void UnhookJobDeclarationEvents(ICommonInvoiceDataProvider declaration)
	{
		if (declaration != null)
		{
			declaration.ZG_DestinationStateInfo.ValueChanged -= ZG_DestinationStateInfo_ValueChanged;
		}
	}

	void ChangeLineCalculationsLabels()
	{
		var destinationStateIsCanaryIsland = ESDeclaration?.DestinationStateIsCanaryIsland ?? false;

		JI_Calc_GSTConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = destinationStateIsCanaryIsland
																								? Res.GetString("ImportInvoiceLineUserControl|59FC7D4E-D461-452F-9151-1CECA2DEFFF6", "IGIC")
																								: Res.GetString("ImportInvoiceLineUserControl|764415CD-5066-4644-A5D5-BB5294DE620D", "VAT");
		JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption = destinationStateIsCanaryIsland
																											? Res.GetString("ImportInvoiceLineUserControl|C1312BE8-7344-4163-8AE8-917A7DFA3D4C", "Def. IGIC")
																											: Res.GetString("ImportInvoiceLineUserControl|F3242672-C80D-48EE-833D-76A288CBB729", "Def. VAT");
		ValueForGstVatLocalCurrencyControl.CaptionResourceString = destinationStateIsCanaryIsland
																	? Res.GetData("ImportInvoiceLineUserControl|8929A984-E4B6-4483-823A-F43B7AD3F386", "IGIC Value")
																	: Res.GetData("ImportInvoiceLineUserControl|E9B9437B-7B5F-407E-A53E-F33A84C50915", "VAT Value");
		VATAdditionsLocalCurrencyControl.CaptionResourceString = destinationStateIsCanaryIsland
																? Res.GetData("ImportInvoiceLineUserControl|E9FADEC6-7B88-4675-B209-C45C32B0B165", "IGIC Additions")
																: Res.GetData("ImportInvoiceLineUserControl|A742C3D9-96B2-4ADF-BE20-13F04E305F41", "VAT Additions");
	}

	protected override bool IsJI_ZZF_NKTaxTypeVisible => false;
	protected override bool IsJI_ValuationMarkupVisible => false;
	protected override bool IsZG_CommercialReferenceVisible => true;

	protected override Type GetSupportingDocumentsUserControlType() => typeof(ImportSupportingDocumentsUserControl);
}
