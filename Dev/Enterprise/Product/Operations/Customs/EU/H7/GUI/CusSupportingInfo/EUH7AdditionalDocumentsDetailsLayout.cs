using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public sealed class EUH7AdditionalDocumentsDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new EU.GUI.PlugIn.AdditionalInformationDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.KindDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.FullTypeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ReferenceTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
