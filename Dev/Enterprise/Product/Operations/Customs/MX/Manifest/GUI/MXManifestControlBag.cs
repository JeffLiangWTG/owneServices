using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public sealed class MXManifestControlBag : ControlBag
	{
		public static MXManifestControlBag Instance => manifestControlBag.Value;

		MXManifestControlBag()
		{
			LastForeignPortCodeFindBox = RegisterControl(nameof(MXManifestCountrySpecificUserControl.LastForeignPortCodeFindBox));
		}

		protected override Control CreateTemplate() => new MXManifestCountrySpecificUserControl();

		public ControlReference LastForeignPortCodeFindBox { get; }

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<MXManifestControlBag> manifestControlBag = new Lazy<MXManifestControlBag>(() => new MXManifestControlBag());
	}
}
