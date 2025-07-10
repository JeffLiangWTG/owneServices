using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test.NetworkDiagram
{
	[TestedType(typeof(ModifyAffinitiesForm))]
	class ModifyAffinitiesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ModifyAffinitiesForm();
		}
	}
}
