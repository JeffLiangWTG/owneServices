using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Manifest.GUI
{
	public sealed class BRManifestControlBag : ControlBag
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "There is no way to change static backing field of this property.")]
		public static BRManifestControlBag Instance { get; } = new BRManifestControlBag();

		BRManifestControlBag()
		{
			CustomsOwnNumberTextBox = RegisterControl(nameof(BRManifestCountrySpecificUserControl.CustomsOwnNumberTextBox));
		}

		protected override Control CreateTemplate() => new BRManifestCountrySpecificUserControl();

		public ControlReference CustomsOwnNumberTextBox { get; }
	}
}
