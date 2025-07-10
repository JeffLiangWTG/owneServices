using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class AISDocumentsUploadAddInfoGridControlBag : ControlBag
	{
		AISDocumentsUploadAddInfoGridControlBag()
		{
			AddInfosGrid = RegisterControl(nameof(AISDocumentsUploadAddInfoGridUserControl.AddInfosGrid));
			AddInfosIM483Grid = RegisterControl(nameof(AISDocumentsUploadAddInfoGridUserControl.AddInfosIM483Grid));
		}

		public static AISDocumentsUploadAddInfoGridControlBag Instance => instance ?? (instance = new AISDocumentsUploadAddInfoGridControlBag());

		[ThreadStatic]
		static AISDocumentsUploadAddInfoGridControlBag instance;

		public ControlReference AddInfosGrid { get; }

		public ControlReference AddInfosIM483Grid { get; }

		protected override Control CreateTemplate() => new AISDocumentsUploadAddInfoGridUserControl();
	}
}
