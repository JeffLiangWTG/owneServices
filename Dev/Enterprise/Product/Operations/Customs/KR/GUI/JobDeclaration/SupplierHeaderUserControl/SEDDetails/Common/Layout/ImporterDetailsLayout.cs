using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ImporterDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new SEDDetailsLayoutBuilder();

			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.BuyerGuidFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.BuyerIDTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
