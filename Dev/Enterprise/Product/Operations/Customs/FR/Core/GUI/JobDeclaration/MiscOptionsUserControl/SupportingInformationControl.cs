using System;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.GUI;

public partial class SupportingInformationControl : EU.GUI.SupportingInformationControl
{
	public SupportingInformationControl()
	{
		InitializeComponent();
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(PlugIn.SupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PlugIn.PreviousDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => IsUCC6Import
		? typeof(AdditionalInfosUserControlWithGrid)
		: typeof(PlugIn.AdditionalInfosUserControl);

	protected override void OnCurrentDataItemChanged(EventArgs e)
	{
		base.OnCurrentDataItemChanged(e);
		var captionResourceString = CaptionProvider.AdditionalInfoTabPageCaption(IsUCC6Import);
		AdditionalInfoTabPage.CaptionResourceString = captionResourceString;
		AdditionalInfoTabPage.Text = captionResourceString.Caption;
	}

	bool IsUCC6Import => CurrentDataItem is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6AndIsImport;
}
