using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public class CLManifestControlBag : ControlBag
	{
		public static CLManifestControlBag Instance => manifestControlBag.Value;

		CLManifestControlBag()
		{
			IsTrampCheckBox = RegisterControl(nameof(CLManifestCountrySpecificUserControl.IsTrampCheckBox));
			TranshipmentTypeDropEdit = RegisterControl(nameof(CLManifestCountrySpecificUserControl.TranshipmentTypeDropEdit));
		}

		public ControlReference IsTrampCheckBox { get; }
		public ControlReference TranshipmentTypeDropEdit { get; }

		protected override Control CreateTemplate() => new CLManifestCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CLManifestControlBag> manifestControlBag = new Lazy<CLManifestControlBag>(() => new CLManifestControlBag());
	}
}
