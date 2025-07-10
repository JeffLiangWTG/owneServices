using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public sealed class CompanyCredentialsControlBag : ControlBag
	{
		public CompanyCredentialsControlBag()
		{
			CompanyCredentialsDetailsUserControl = RegisterControl(nameof(CompanyCredentialsUserControl.CompanyCredentialsDetailsUserControl));
		}

		public static CompanyCredentialsControlBag Instance => instance ?? (instance = new CompanyCredentialsControlBag());

		[ThreadStatic]
		static CompanyCredentialsControlBag instance;

		protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

		public ControlReference CompanyCredentialsDetailsUserControl { get; }
	}
}
