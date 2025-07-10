using System;
using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	public partial class ZTextBoxPopupForm : ZChildForm
	{
		public ZTextBoxPopupForm(ZTextBox parentTextBox)
		{
			InitializeComponent();

			this.parentTextBox = parentTextBox;
			textBox.CharacterCasing = parentTextBox.CharacterCasing;
			textBox.Text = parentTextBox.Text;
			textBox.SelectionStart = parentTextBox.SelectionStart;
			textBox.SelectionLength = parentTextBox.SelectionLength;
			textBox.MaxLength = parentTextBox.MaxLength;
			if (textBox.contextMenuManager != null)
			{
				textBox.contextMenuManager.Dispose();
			}
			textBox.contextMenuManager = new ZTextBoxBaseContextMenuManager(parentTextBox, textBox, new NoMacroBox());
		}

		readonly ZTextBox parentTextBox;

		void OkButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing && DialogResult == DialogResult.OK)
			{
				parentTextBox.Text = textBox.Text;
			}

			base.OnFormClosed(e);
		}
	}
}
