using System.Windows.Forms;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(ServiceTaskHostConfigurationForm))]
	internal sealed class ServiceTaskProxyConfigurationFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ServiceTaskHostConfigurationForm(Factory.New<StmServiceHost>());
		}

		#endregion
	}
}
