using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public class EntryInstructionDetailsLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ??= CreateLayout();
	PanelLayout layout;

	PanelLayout CreateLayout()
	{
		var builder = new EntryInstructionDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var aeBag = EntryInstructionDetailsControlBag.Instance;
		builder.AddControlBag(aeBag);

		builder.AddColumn();
		builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(aeBag.TradeTypeDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.OtherPartiesSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(aeBag.ToWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(aeBag.ToWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(aeBag.FromWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(aeBag.FromWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.NewOwnerOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(commonBag.AssessmentDateEdit, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.RemoverOrganisationControl, alignToControl: commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(aeBag.DeclarationPurposeDropEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(aeBag.DeclarationPurposeDetailsTextBox, widthClass: ControlWidthClass.Auto);

		return builder.Build();
	}
}
