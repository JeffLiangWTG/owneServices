using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class AISDocumentsUploadAddInfoGridLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; } = CreateLayout();

		static PanelLayout CreateLayout()
		{
			var builder = new AISDocumentsUploadAddInfoGridLayoutBuilder<UploadDocumentsSendingAction>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.AddInfosGrid, ControlWidthClass.Auto);
			builder.Add(commonBag.AddInfosIM483Grid, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
