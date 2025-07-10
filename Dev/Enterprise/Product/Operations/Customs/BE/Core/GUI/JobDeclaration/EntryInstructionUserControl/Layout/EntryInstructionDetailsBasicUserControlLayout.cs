using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class EntryInstructionDetailsBasicUserControlLayout : IPanelLayoutProvider
{
	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateEntryInstructionDetailsBasicUserControlLayout());
	PanelLayout layout;

	static PanelLayout CreateEntryInstructionDetailsBasicUserControlLayout()
	{
		var builder = new EntryInstructionDetailsBasicUserControlLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = builder.EUBag;

		builder.AddControlBag(euBag);
		builder.AddColumn();

		builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.SubStyleDropEdit, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.OtherPartiesSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.ToWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(euBag.FromWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.NewOwnerOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

		builder.AddColumn();
		builder.Add(euBag.LocationOfGoodsUserControl, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.RemoverOrganisationControl, alignToControl: commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

		builder.SetVisibility(commonBag.SubStyleDropEdit, x => !(x.IsReExport || x.IsExitSummary), x => x.JobDeclaration.JE_MessageTypeInfo);

		return builder.Build();
	}
}
