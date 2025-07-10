using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.Testing
{
	[TestedType(typeof(NetworkMonitorForm))]
	sealed class NetworkMonitorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mock = new Mock<IWiseCloudSecurityClient>();

			mock.Setup(m => m.GetClientIPAddress(It.IsAny<string>(), It.IsAny<string>()))
				.Returns("127.0.0.1");

			using (ObjectFactory.Substitute(mock.Object))
			{
				var form = new NetworkMonitorForm();

				mock.VerifyAll();
				return form;
			}
		}
	}
}
