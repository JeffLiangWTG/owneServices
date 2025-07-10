using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class LocalExportSEDDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public LocalExportSEDDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new SEDDetailsLayoutBuilder();
			var bag = SEDDetailsGroupBoxControlBag.Instance;
			builder.AddControlBag(bag);

			builder.AddColumn();
			builder.Add(bag.SupplierGroupBox, ControlWidthClass.Auto);
			builder.Add(bag.ManufacturerGroupBox, ControlWidthClass.Auto);
			builder.Add(bag.ImporterGroupBox, ControlWidthClass.Auto);
			builder.SetCaption(bag.ImporterGroupBox, x => Res.GetData("F481E7C8-1D9C-485A-87BD-376558C720B3", "Importer"));

			return builder.Build();
		}
	}
}
