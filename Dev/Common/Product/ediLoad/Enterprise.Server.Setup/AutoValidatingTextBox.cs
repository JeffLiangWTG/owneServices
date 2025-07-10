using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace Enterprise.Server.Setup
{
	sealed class AutoValidatingTextBox : TextBox
	{
		public AutoValidatingTextBox()
		{
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			OnValidating(new CancelEventArgs());
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			e.Handled = (!char.IsLetterOrDigit(e.KeyChar) || !IsASCIISymbol(e.KeyChar)) && !char.IsControl(e.KeyChar);
			base.OnKeyPress(e);
		}

		bool IsASCIISymbol(char c)
		{
			return Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(new char[] { c })).IndexOf('?') < 0;
		}
	}
}

