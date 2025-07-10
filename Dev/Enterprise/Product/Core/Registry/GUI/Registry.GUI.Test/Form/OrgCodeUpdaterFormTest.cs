using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgCodeUpdaterForm))]
	sealed class OrgCodeUpdaterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OrgCodeUpdaterForm();
		}
	}
}
