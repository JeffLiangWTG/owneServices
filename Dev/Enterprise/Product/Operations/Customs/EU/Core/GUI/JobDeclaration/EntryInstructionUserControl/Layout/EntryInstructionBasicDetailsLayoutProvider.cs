using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class EntryInstructionBasicDetailsLayoutProvider : IPanelLayoutProvider
	{
		#region IPanelLayoutProvider

		PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		PanelLayout layout;

		#endregion

		PanelLayout CreateLayout()
		{
			var builder = new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
			var commonBag = builder.CommonBag;
			var euBag = builder.EUBag;

			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.DetailsLabel, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.StyleDropEdit, widthClass: ControlWidthClass.Auto);
			builder.Add(euBag.LocationOfGoodsUserControl, widthClass: ControlWidthClass.Auto);

			builder.Add(commonBag.OtherPartiesSeparatorUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(euBag.ToWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(euBag.ToWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(euBag.FromWarehouseLabel, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(euBag.FromWarehouseUserControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.NewOwnerOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.RemoverOrganisationControl, alignToControl: commonBag.BondHolderOrganisationControl, widthClass: ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.SubStyleDropEdit, alignToControl: commonBag.StyleDropEdit, widthClass: ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
