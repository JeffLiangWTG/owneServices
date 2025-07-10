using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Modules.AirCargo.Testing
{
	[TestedType(typeof(SoundexTesterForm))]
	public class SoundexTesterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new SoundexTesterForm();
		}
	}
}
