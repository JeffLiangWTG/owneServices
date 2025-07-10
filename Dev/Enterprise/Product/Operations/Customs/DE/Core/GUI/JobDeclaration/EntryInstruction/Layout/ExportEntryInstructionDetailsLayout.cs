using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ExportEntryInstructionDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new EntryInstructionDetailsLayoutBuilder();
			var deBag = builder.CommonBag;
			var euBag = EU.GUI.EntryInstructionBasicDetailsControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(deBag.SubStyleDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.StyleDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.DescriptionTextBox, ControlWidthClass.Long);
			builder.Add(euBag.LocationOfGoodsUserControl, widthClass: ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(deBag.PartyConstellationDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.DateForDutyDateEdit, ControlWidthClass.Auto);
			builder.Add(deBag.ExitDateDateEdit, ControlWidthClass.Auto);

			builder.SetCaption(deBag.StyleDropEdit, i => EntryInstructionDetailBasicUserControl.ExportStyleCaption, i => i.CEI_StyleInfo);
			builder.SetCaption(deBag.SubStyleDropEdit, i => EntryInstructionDetailBasicUserControl.ExportSubStyleCaption, i => i.CEI_SubStyleInfo);

			return builder.Build();
		}
	}
}
