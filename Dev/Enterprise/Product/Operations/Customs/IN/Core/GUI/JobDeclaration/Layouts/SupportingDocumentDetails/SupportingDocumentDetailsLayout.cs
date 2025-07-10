using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class SupportingDocumentDetailsLayout : IPanelLayoutWithGridProvider
{
	public SupportingDocumentDetailsLayout()
	{
		supportingDocumentLayout = CreateLayout();
	}

	public Type GridUserControlType => typeof(SupportingDocumentGridControl);

	public PanelLayout Layout => supportingDocumentLayout;

	readonly PanelLayout supportingDocumentLayout;

	PanelLayout CreateLayout()
	{
		var builder = new SupportingDocumentDetailsLayoutBuilder<SupportingDocument>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.ImageReferenceNumberTextBox, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.DocumentTypeCodeFindBox, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.IssuingPartyGroupBox, widthClass: ControlWidthClass.LongControl);

		return builder.Build();
	}
}
