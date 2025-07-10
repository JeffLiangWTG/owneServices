using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ExportSEDDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public ExportSEDDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new SEDDetailsLayoutBuilder();
			var bag = SEDDetailsGroupBoxControlBag.Instance;
			builder.AddControlBag(bag);

			builder.AddColumn();
			builder.Add(bag.CertificateOfOriginGroupBox, ControlWidthClass.Auto);
			builder.Add(bag.ManufacturerGroupBox, ControlWidthClass.Auto);
			builder.Add(bag.ImporterGroupBox, ControlWidthClass.Auto);
			builder.SetCaption(bag.ImporterGroupBox, x => Res.GetData("25C1B079-7887-4A17-9AEF-705C7B472AAE", "Importer"));

			return builder.Build();
		}
	}
}
