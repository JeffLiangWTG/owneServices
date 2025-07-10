using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZTextBoxPopupForm))]
	sealed class ZTextBoxPopupFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var textBox = new ZTextBox();
			var form = new ZTextBoxPopupForm(textBox);
			form.Disposed += (sender, args) => { textBox.Dispose(); };
			return form;
		}
	}
}
