using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public sealed class CompanyCredentialsControlBag : ControlBag
	{
		public CompanyCredentialsControlBag()
		{
			CredentialsGroupBox = RegisterControl(nameof(CompanyCredentialsUserControl.CredentialsGroupBox));
		}

		public static CompanyCredentialsControlBag Instance => instance ?? (instance = new CompanyCredentialsControlBag());

		[ThreadStatic]
		static CompanyCredentialsControlBag instance;

		protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

		public ControlReference CredentialsGroupBox { get; }
	}
}
