using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	[TestedType(typeof(EditingExcelForm))]
	sealed class EditingExcelFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new EditingExcelForm();
		}
	}
}
