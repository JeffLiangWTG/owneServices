using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI;

public sealed class ManifestControlBag : ControlBag
{
	protected override Control CreateTemplate() => new ManifestLayoutsUserControl();

	public static ManifestControlBag Instance => instance ?? (instance = new ManifestControlBag());

	[ThreadStatic]
	static ManifestControlBag instance;

	ManifestControlBag()
	{
		CarrierMPCITextBox = RegisterControl(nameof(ManifestLayoutsUserControl.CarrierMPCITextBox));
		ShippingAgentMPCITextBox = RegisterControl(nameof(ManifestLayoutsUserControl.ShippingAgentMPCITextBox));
	}

	public ControlReference CarrierMPCITextBox;
	public ControlReference ShippingAgentMPCITextBox;
}
