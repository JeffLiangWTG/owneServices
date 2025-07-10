using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(LoginForUpgradeForm))]
	sealed class LoginForUpgradeFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new LoginForUpgradeForm("", true, "");
	}
}
