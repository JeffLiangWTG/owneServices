using System;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI;

public partial class SupportingInformationControl : EU.GUI.SupportingInformationControl
{
	public SupportingInformationControl()
	{
		InitializeComponent();
		AdditionalDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitAdditionalDocumentsUserControl());
		SetupTabsOrder();
	}

	protected virtual Type GetAdditionalDocumentsUserControlType() => typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid);

	protected override Type GetAdditionalInfosUserControlType() => (Declaration?.IsUCC6AndIsExport ?? false)
																		? typeof(EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid)
																		: base.GetAdditionalInfosUserControlType();

	protected override Type GetSupportingDocumentsUserControlType() => (Declaration?.IsImport ?? false)
																		? typeof(ImportSupportingDocumentsUserControl)
																		: typeof(ExportSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

	protected override Type GetGuaranteesUserControlType() => typeof(ESGuaranteesUserControl);

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		var captionResourceString = (Declaration?.IsUCC6AndIsExport ?? false)
											? Res.GetData("DE96B289-0065-4886-AA93-7B82BA856C45", "Additional Documents")
											: Res.GetData("855743B2-5181-4164-915F-E08ED2816973", "[44] Additional Info");
		AdditionalInfoTabPage.CaptionResourceString = captionResourceString;
		AdditionalInfoTabPage.Text = captionResourceString.Caption;
		if (Declaration?.IsUCC6AndIsExport ?? false)
		{
			AdditionalDocumentsTabPage.TabVisible = false;
		}
	}

	void InitAdditionalDocumentsUserControl()
	{
		additionalDocumentsUserControl.UserControlType = GetAdditionalDocumentsUserControlType();
		additionalDocumentsUserControl.HostedControlCreated += (sender, args) =>
		{
			var hostedControl = additionalDocumentsUserControl.HostedControl;
			if (hostedControl is EU.GUI.PlugIn.ISupportingInfoUserControls)
			{
				EU.GUI.PlugIn.SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(hostedControl, "", SupportingInfoColumnLayoutContext, GetAdditionalInfosColumnWidths());
			}
		};
	}

	void SetupTabsOrder()
	{
		var index = SupportingInformationTabControl.TabPages.IndexOf(SupportingDocumentTabPage);
		SupportingInformationTabControl.TabPages.Insert(AdditionalDocumentsTabPage, index + 1);
	}

	JobDeclaration Declaration => (JobDeclaration)CurrentDataItem;
}
