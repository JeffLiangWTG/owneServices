using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ManufacturerDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => layout;

		static PanelLayout CreateLayout()
		{
			var builder = new SEDDetailsLayoutBuilder();

			var controlBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(controlBag.ManufacturerAddressControl, ControlWidthClass.Auto);
			builder.Add(controlBag.ManufacturerGuidFindBox, ControlWidthClass.Auto);
			builder.Add(controlBag.ManufacturerUnipassIDTextBox, ControlWidthClass.Auto);
			builder.Add(controlBag.ManufacturerIPCCodeFindBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
