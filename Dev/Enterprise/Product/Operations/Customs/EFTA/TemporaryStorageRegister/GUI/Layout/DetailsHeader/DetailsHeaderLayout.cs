using System;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public sealed class DetailsHeaderLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => declarationDetails.Value;

	readonly Lazy<PanelLayout> declarationDetails = new(CreateDeclarationDetailsLayout);

	static PanelLayout CreateDeclarationDetailsLayout()
	{
		var builder = new DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.ATBNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.PreviousReferenceTypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.StatusDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.ArrvialDateEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PresentationDateEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PreviousReferenceNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.CustomerReferenceTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
