using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public sealed class InvoiceLineAdditionalInformationDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new AdditionalInformationDetailsLayoutBuilder();
			var euBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(euBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.FullTypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.ReferenceTextBox, ControlWidthClass.Long);
			builder.Add(euBag.DescriptionTextBox, ControlWidthClass.Long);
			builder.Add(euBag.DetailTextBox, ControlWidthClass.Long);
			builder.Add(euBag.CurrencyDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.AmountCalcEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
