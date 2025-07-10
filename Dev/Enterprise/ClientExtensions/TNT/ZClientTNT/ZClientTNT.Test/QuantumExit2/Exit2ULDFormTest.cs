using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(Exit2ULDForm))]
	public class Exit2ULDFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new Exit2ULDForm("", "", "");
		}
	}
}
