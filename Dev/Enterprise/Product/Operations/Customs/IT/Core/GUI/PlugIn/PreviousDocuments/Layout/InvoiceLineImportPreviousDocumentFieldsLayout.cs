using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class InvoiceLineImportPreviousDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public InvoiceLineImportPreviousDocumentFieldsLayout()
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
		builder.Add(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto);

		return builder.Build();
	}
}
