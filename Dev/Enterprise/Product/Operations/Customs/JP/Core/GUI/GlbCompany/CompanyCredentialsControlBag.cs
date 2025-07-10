using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CompanyCredentialsControlBag : ControlBag
	{
		CompanyCredentialsControlBag()
		{
			NaccsMailboxGroupBox = RegisterControl(nameof(NaccsMailboxGroupBox));
		}

		protected override Control CreateTemplate() => new CompanyCredentialsUserControl();

		public static CompanyCredentialsControlBag Instance => instance ??= new CompanyCredentialsControlBag();

		[ThreadStatic]
		static CompanyCredentialsControlBag instance;

		public ControlReference NaccsMailboxGroupBox { get; }
	}
}
