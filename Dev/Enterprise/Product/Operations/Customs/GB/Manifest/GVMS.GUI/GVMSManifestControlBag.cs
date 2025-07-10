using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GVMS.GUI
{
	public sealed class GVMSManifestControlBag : ControlBag
	{
		public static GVMSManifestControlBag Instance => instance ?? (instance = new GVMSManifestControlBag());

		[ThreadStatic]
		static GVMSManifestControlBag instance;

		GVMSManifestControlBag()
		{
			CarrierCodeDropEdit = RegisterControl(nameof(GVMSUserControl.CarrierCodeDropEdit));
			RouteIdDropEdit = RegisterControl(nameof(GVMSUserControl.RouteIdDropEdit));
			IsUnaccompaniedCheckBox = RegisterControl(nameof(GVMSUserControl.IsUnaccompaniedCheckBox));
			EmptyVehicleDropEdit = RegisterControl(nameof(GVMSUserControl.EmptyVehicleDropEdit));
			InspectionRequiredCheckBox = RegisterControl(nameof(GVMSUserControl.InspectionRequiredCheckBox));
			InspectionLocationsGroupBox = RegisterControl(nameof(GVMSUserControl.InspectionLocationsGroupBox));
			CustomsReferencesGroupBox = RegisterControl(nameof(GVMSUserControl.CustomsReferencesGroupBox));
			TransitReferencesGroupBox = RegisterControl(nameof(GVMSUserControl.TransitReferencesGroupBox));
			OtherReferencesGroupBox = RegisterControl(nameof(GVMSUserControl.OtherReferencesGroupBox));
			HaulierTypeDropEdit = RegisterControl(nameof(GVMSUserControl.HaulierTypeDropEdit));
		}

		protected override Control CreateTemplate() => new GVMSUserControl();

		public ControlReference CarrierCodeDropEdit { get; }
		public ControlReference RouteIdDropEdit { get; }
		public ControlReference IsUnaccompaniedCheckBox { get; }
		public ControlReference EmptyVehicleDropEdit { get; }
		public ControlReference InspectionRequiredCheckBox { get; }
		public ControlReference InspectionLocationsGroupBox { get; }
		public ControlReference CustomsReferencesGroupBox { get; }
		public ControlReference TransitReferencesGroupBox { get; }
		public ControlReference OtherReferencesGroupBox { get; }
		public ControlReference HaulierTypeDropEdit { get; }
	}
}
