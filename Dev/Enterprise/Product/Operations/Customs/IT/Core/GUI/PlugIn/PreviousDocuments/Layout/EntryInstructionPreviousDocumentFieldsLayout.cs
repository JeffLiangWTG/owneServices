using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class EntryInstructionPreviousDocumentFieldsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public EntryInstructionPreviousDocumentFieldsLayout()
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

		builder.SetCaption(euBag.CodeDropEdit, _ => Res.GetData("20CA76C7-65C8-47B7-AB85-E68C45E411B1", "Document"));
		builder.SetCaption(euBag.ReferenceTextBox, _ => Res.GetData("FE28A01A-1830-4350-916A-1C77AC72A4D6", "Number"));
		builder.SetCaption(euBag.Reference2TextBox, _ => Res.GetData("4F8F483D-CA75-43A4-A0EE-D9055133B23D", "MRN"));
		builder.SetCaption(euBag.QuantityCalcDropEdit, _ => Res.GetData("DBDB6603-36F7-42E9-8E68-AAE8FB776F8B", "Mass"));
		builder.SetCaption(euBag.PackageQuantityCalcDropEdit, _ => Res.GetData("42C211EA-01D6-445C-894D-FA291D1F0A9D", "Pkg Qty", "Pkg. Qty", "Package Qty", "Package Quantity"));

		return builder.Build();
	}
}
