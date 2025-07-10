using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class ImportPreviousDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ImportPreviousDocumentFieldsLayout()
	{
		Layout = CreatePreviousDocumentFieldsLayout();
	}

	static PanelLayout CreatePreviousDocumentFieldsLayout()
	{
		var builder = new EU.GUI.PlugIn.PreviousDocumentsFieldsLayoutBuilder<Business.Declaration.PreviousDocument>();

		var euBag = EU.GUI.PlugIn.PreviousDocumentsFieldsControlBag.Instance;
		builder.AddControlBag(euBag);

		builder.AddColumn();
		builder.Add(euBag.ProcedureDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.CodeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Long);
		builder.Add(euBag.Reference2TextBox, ControlWidthClass.Long);
		builder.Add(euBag.IssueDateEdit, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(euBag.LineNoCalcEdit, ControlWidthClass.Auto);
		builder.Add(euBag.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.Quantity3CalcDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);

		builder.SetCaption(euBag.CodeDropEdit, _ => Res.GetData("F1FABB9E-BD8E-4E3B-9510-9DECD39CD250", "Document"));
		builder.SetCaption(euBag.ReferenceTextBox, _ => Res.GetData("F11D66C6-5644-479B-A997-ED6C1BAFFA15", "Number"));
		builder.SetCaption(euBag.QuantityCalcDropEdit, _ => Res.GetData("AF552462-7673-45D9-8EF3-BAA17D000CAC", "Net Mass"));

		return builder.Build();
	}
}
