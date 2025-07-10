using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FinalPriceExtensionRequestNewLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new FinalPriceExtensionRequestNewLayoutsBuilder();
			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(controlBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(controlBag.BranchGuidFindBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
