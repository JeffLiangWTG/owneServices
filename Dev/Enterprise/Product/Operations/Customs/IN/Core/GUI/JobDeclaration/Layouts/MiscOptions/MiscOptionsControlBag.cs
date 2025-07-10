using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class MiscOptionsControlBag : ControlBag
{
	public static MiscOptionsControlBag Instance => instance ??= new MiscOptionsControlBag();

	[ThreadStatic]
	static MiscOptionsControlBag instance;

	MiscOptionsControlBag()
	{
		NonStandardExchangeRateGroupBox = RegisterControl(nameof(MiscOptionsLayoutUserControl.NonStandardExchangeRateGroupBox));
	}

	public ControlReference NonStandardExchangeRateGroupBox { get; }

	protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
}
