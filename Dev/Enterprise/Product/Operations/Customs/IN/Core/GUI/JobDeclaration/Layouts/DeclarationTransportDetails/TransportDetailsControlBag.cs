using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class TransportDetailsControlBag : ControlBag
{
	public static TransportDetailsControlBag Instance => instance ??= new TransportDetailsControlBag();

	[ThreadStatic]
	static TransportDetailsControlBag instance;

	TransportDetailsControlBag()
	{
		LoadingAndDestinationInformationSeparatorUserControl = RegisterControl(nameof(TransportDetailsUserControl.LoadingAndDestinationInformationSeparatorUserControl));
	}

	protected override Control CreateTemplate() => new TransportDetailsUserControl();

	public ControlReference LoadingAndDestinationInformationSeparatorUserControl { get; }
}
