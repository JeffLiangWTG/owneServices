using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh.Testing
{
	[TestedType(typeof(AutoRefreshForm))]
	sealed class AutoRefreshFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new AutoRefreshForm(5);
		}
	}
}
