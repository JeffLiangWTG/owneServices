using System;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5GoodsItemSupportingDocumentLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public Phase5GoodsItemSupportingDocumentLayoutWithGrid()
	{
		Layout = CreateLayout();
	}

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(GoodsItemSupportingDocumentsGridUserControl);

	PanelLayout Layout { get; }

	PanelLayout CreateLayout()
	{
		var builder = new SupportingDocumentLayoutBuilder<Business.NctsSupportingDocument>();
		var commonBag = builder.CommonBag;
		var itBag = SupportingDocumentControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.TypeCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(itBag.YearOfIssueTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.ItemNumberCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ComplementTextBox, ControlWidthClass.Long);

		builder.SetCaption(itBag.YearOfIssueTextBox, _ => SupportingDocumentLayoutCaptions.Instance.YearOfIssueCaption);

		return builder.Build();
	}
}
