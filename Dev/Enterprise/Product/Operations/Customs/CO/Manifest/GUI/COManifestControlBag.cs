using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CO.Manifest.GUI
{
	public class COManifestControlBag : ControlBag
	{
		public static COManifestControlBag Instance => manifestControlBag.Value;

		COManifestControlBag()
		{
			CargoDispositionDropEdit = RegisterControl(nameof(COManifestCountrySpecificUserControl.CargoDispositionDropEdit));
			TravelDocumentTypeDropEdit = RegisterControl(nameof(COManifestCountrySpecificUserControl.TravelDocumentTypeDropEdit));
			MultimodalCheckBox = RegisterControl(nameof(COManifestCountrySpecificUserControl.MultimodalCheckBox));
			PrecursorsCheckBox = RegisterControl(nameof(COManifestCountrySpecificUserControl.PrecursorsCheckBox));
			CarriersLiabilityCheckBox = RegisterControl(nameof(COManifestCountrySpecificUserControl.CarriersLiabilityCheckBox));
			DeliveryModeDropEdit = RegisterControl(nameof(COManifestCountrySpecificUserControl.DeliveryModeDropEdit));
		}

		public ControlReference CargoDispositionDropEdit { get; }
		public ControlReference TravelDocumentTypeDropEdit { get; }
		public ControlReference MultimodalCheckBox { get; }
		public ControlReference PrecursorsCheckBox { get; }
		public ControlReference CarriersLiabilityCheckBox { get; }
		public ControlReference DeliveryModeDropEdit { get; }

		protected override Control CreateTemplate() => new COManifestCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<COManifestControlBag> manifestControlBag = new Lazy<COManifestControlBag>(() => new COManifestControlBag());
	}
}
