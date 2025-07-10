using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class SupplierDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new SEDDetailsLayoutBuilder();

			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.SupplierGuidFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.SupplierUnipassIDTextBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
