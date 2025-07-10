using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public class AdditionalInformationDetailsControlBag : ControlBag
{
	protected AdditionalInformationDetailsControlBag()
	{
		NKCountryCodeFindBox = RegisterControl(nameof(AdditionalInformationDetailsUserControl.NKCountryCodeFindBox));
	}

	public static AdditionalInformationDetailsControlBag Instance => instance ?? (instance = new AdditionalInformationDetailsControlBag());

	[ThreadStatic]
	static AdditionalInformationDetailsControlBag instance;

	protected override Control CreateTemplate() => new AdditionalInformationDetailsUserControl();

	public ControlReference NKCountryCodeFindBox { get; }
}
