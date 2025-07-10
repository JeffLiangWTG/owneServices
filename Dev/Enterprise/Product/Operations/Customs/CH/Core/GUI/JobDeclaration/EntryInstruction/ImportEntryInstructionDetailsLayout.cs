using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ImportEntryInstructionDetailsLayout : IPanelLayoutProvider
{
	PanelLayout EntryInstructionType { get; }

	public PanelLayout Layout => EntryInstructionType;

	public ImportEntryInstructionDetailsLayout()
	{
		EntryInstructionType = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
		var chBag = EntryInstructionsDetailsControlBag.Instance;
		var commonBag = EntryInstructionBasicDetailsControlBag.Instance;

		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(commonBag.DetailsLabel, widthClass: ControlWidthClass.LongNoCaption);
		builder.Add(commonBag.StyleDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.SubStyleDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);
		builder.Add(chBag.ReasonDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.AssessmentDateEdit, ControlWidthClass.Long);

		return builder.Build();
	}
}
