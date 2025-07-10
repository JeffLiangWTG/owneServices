using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ExportEntryInstructionDetailsLayout : IPanelLayoutProvider
{
	PanelLayout EntryInstructionType { get; }

	public PanelLayout Layout => EntryInstructionType;

	public ExportEntryInstructionDetailsLayout()
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
		builder.Add(chBag.ProcedureCodeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.StyleDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.SubStyleDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.NextProcedureDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.PartialDeliveryCheckBox, ControlWidthClass.Long);
		builder.Add(chBag.TransportChargesMethodOfPaymentDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

		builder.SetVisibility(commonBag.SubStyleDropEdit, h => h.JobDeclaration.IsExportDeclarationActivation, h => h.JobDeclaration.JE_MessageTypeInfo);

		return builder.Build();
	}
}
