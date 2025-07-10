using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public sealed class CompanyCredentialsControlBag : ControlBag
{
	public CompanyCredentialsControlBag()
	{
		UserIDTextBox = RegisterControl(nameof(CompanyCredentialsUserControl.UserIDTextBox));
		CurrentPasswordTextBox = RegisterControl(nameof(CompanyCredentialsUserControl.CurrentPasswordTextBox));
	}

	public static CompanyCredentialsControlBag Instance => instance ?? (instance = new CompanyCredentialsControlBag());

	[ThreadStatic]
	static CompanyCredentialsControlBag instance;

	protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

	public ControlReference UserIDTextBox { get; }
	public ControlReference CurrentPasswordTextBox { get; }
}
