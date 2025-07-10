using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public sealed class CompanyConfigurationControlBag : ControlBag
	{
		public CompanyConfigurationControlBag()
		{
			CustomsConfigurationGroupBox = RegisterControl(nameof(CompanyConfigurationUserControl.CustomsConfigurationGroupBox));
		}

		public static CompanyConfigurationControlBag Instance => instance ?? (instance = new CompanyConfigurationControlBag());

		[ThreadStatic]
		static CompanyConfigurationControlBag instance;

		protected override Control CreateTemplate() => new CompanyConfigurationUserControl();

		public ControlReference CustomsConfigurationGroupBox { get; }
	}
}
