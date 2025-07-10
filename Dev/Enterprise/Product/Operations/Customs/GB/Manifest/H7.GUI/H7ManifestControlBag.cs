using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public sealed class H7ManifestControlBag : ControlBag
	{
		public static H7ManifestControlBag Instance => instance ?? (instance = new H7ManifestControlBag());

		[ThreadStatic]
		static H7ManifestControlBag instance;

		H7ManifestControlBag()
		{
			CSPDropEdit = RegisterControl(nameof(H7ManifestUserControl.CSPDropEdit));
			SupervisingOfficeAddressControl = RegisterControl(nameof(H7ManifestUserControl.SupervisingOfficeAddressControl));
		}

		protected override Control CreateTemplate() => new H7ManifestUserControl();

		public ControlReference CSPDropEdit { get; }

		public ControlReference SupervisingOfficeAddressControl { get; }
	}
}
