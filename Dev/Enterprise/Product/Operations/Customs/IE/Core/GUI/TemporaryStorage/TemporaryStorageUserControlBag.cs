using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class TemporaryStorageUserControlBag : ControlBag
	{
		public static TemporaryStorageUserControlBag Instance => temporaryStorageControlBag.Value;

		TemporaryStorageUserControlBag()
		{
			ManifestTypeDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.ManifestTypeDropEdit));
			CustomsOfficeofLodgementCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.CustomsOfficeofLodgementCodeFindBox));
			CustomsOfficeOfFirstEntryCodeFindBox = RegisterControl(nameof(TemporaryStorageUserControl.CustomsOfficeOfFirstEntryCodeFindBox));
			RepresentativeStatusDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.RepresentativeStatusDropEdit));
			BorderTransportTypeDropEdit = RegisterControl(nameof(TemporaryStorageUserControl.BorderTransportTypeDropEdit));
			BorderTransportIDTextBox = RegisterControl(nameof(TemporaryStorageUserControl.BorderTransportIDTextBox));
		}

		public ControlReference ManifestTypeDropEdit { get; }
		public ControlReference CustomsOfficeofLodgementCodeFindBox { get; }
		public ControlReference CustomsOfficeOfFirstEntryCodeFindBox { get; }
		public ControlReference RepresentativeStatusDropEdit { get; }
		public ControlReference BorderTransportTypeDropEdit { get; }
		public ControlReference BorderTransportIDTextBox { get; }

		protected override Control CreateTemplate() => new TemporaryStorageUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TemporaryStorageUserControlBag> temporaryStorageControlBag = new Lazy<TemporaryStorageUserControlBag>(() => new TemporaryStorageUserControlBag());
	}
}
