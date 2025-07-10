using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class CertificateOfOriginDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new SEDDetailsLayoutBuilder();

			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.COODeterminationRuleDropEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.COOIssueStatusDropEdit, ControlWidthClass.Auto);
			builder.Add(controlBag.COOLabelLocationDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
