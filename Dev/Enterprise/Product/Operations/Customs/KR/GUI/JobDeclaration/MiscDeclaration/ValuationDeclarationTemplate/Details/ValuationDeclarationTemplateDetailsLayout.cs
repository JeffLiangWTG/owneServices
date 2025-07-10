using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ValuationDeclarationTemplateDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new ValuationDeclarationTemplateLayoutBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.ValuationCodeDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.DepartmentCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.PONoTextBox, ControlWidthClass.Long);
			builder.Add(controlBag.PODateEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.ServiceCodeFindBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
