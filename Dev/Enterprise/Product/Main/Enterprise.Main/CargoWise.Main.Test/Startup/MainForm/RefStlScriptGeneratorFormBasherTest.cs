using System.Windows.Forms;
using CargoWise.Main.Startup.MainForm;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(RefStlScriptGeneratorForm))]
	sealed class RefStlScriptGeneratorFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new RefStlScriptGeneratorForm();
		}

		#endregion
	}
}
