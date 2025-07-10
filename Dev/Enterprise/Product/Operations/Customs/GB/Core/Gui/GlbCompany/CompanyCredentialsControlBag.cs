using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public sealed class CompanyCredentialsControlBag : ControlBag
	{
		public CompanyCredentialsControlBag()
		{
			CredentialsDetailsUserControl = RegisterControl(nameof(CompanyCredentialsUserControl.CredentialsDetailsUserControl));
		}

		public static CompanyCredentialsControlBag Instance => instance ?? (instance = new CompanyCredentialsControlBag());

		[ThreadStatic]
		static CompanyCredentialsControlBag instance;

		protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

		public ControlReference CredentialsDetailsUserControl { get; }
	}
}
