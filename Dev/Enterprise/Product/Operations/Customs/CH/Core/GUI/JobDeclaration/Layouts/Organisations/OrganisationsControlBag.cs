using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class OrganisationsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new OrganisationsUserControl();

	public static OrganisationsControlBag Instance => instance ??= new OrganisationsControlBag();

	[ThreadStatic]
	static OrganisationsControlBag instance;

	OrganisationsControlBag()
	{
		ConsignorDocAddressControl = RegisterControl(nameof(OrganisationsUserControl.ConsignorDocAddressControl));
	}

	public ControlReference ConsignorDocAddressControl { get; }
}
