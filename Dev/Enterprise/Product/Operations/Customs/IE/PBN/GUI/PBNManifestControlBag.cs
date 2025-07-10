using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.PBN.GUI
{
	public sealed class PBNManifestControlBag : ControlBag
	{
		public static PBNManifestControlBag Instance => instance ?? (instance = new PBNManifestControlBag());

		[ThreadStatic]
		static PBNManifestControlBag instance;

		PBNManifestControlBag()
		{
			IsEmptyVehicleCheckBox = RegisterControl(nameof(PBNUserControl.IsEmptyVehicleCheckBox));
			CarrierCodeDropEdit = RegisterControl(nameof(PBNUserControl.CarrierCodeDropEdit));
			CustomsReferencesGroupBox = RegisterControl(nameof(PBNUserControl.CustomsReferencesGroupBox));
			TransitReferencesGroupBox = RegisterControl(nameof(PBNUserControl.TransitReferencesGroupBox));
			PersonsGroupBox = RegisterControl(nameof(PBNUserControl.PersonsGroupBox));
		}

		protected override Control CreateTemplate() => new PBNUserControl();

		public ControlReference IsEmptyVehicleCheckBox { get; }
		public ControlReference CarrierCodeDropEdit { get; }
		public ControlReference CustomsReferencesGroupBox { get; }
		public ControlReference TransitReferencesGroupBox { get; }
		public ControlReference PersonsGroupBox { get; }
	}
}
