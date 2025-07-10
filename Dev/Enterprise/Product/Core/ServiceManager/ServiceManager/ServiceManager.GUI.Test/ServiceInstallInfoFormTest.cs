using System.Windows.Forms;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(ServiceInstallInfoForm))]
	public class ServiceInstallInfoFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ServiceInstallInfoForm(new ServiceInstallInfo(Factory));
		}
	}
}
