using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	[SuppressFormDesignerAnalysis]
	sealed class TestRichTextBox : ZRichTextBox
	{
		public new RichTextBox RichEdit
		{
			get { return base.RichEdit; }
		}
	}
}
