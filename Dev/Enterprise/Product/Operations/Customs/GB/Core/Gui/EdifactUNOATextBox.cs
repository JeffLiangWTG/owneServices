using System;
using System.Windows.Forms;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.GUI
{
	public class EdifactUNOATextBox : ZTextBox
	{
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			UkCharSet ukCharSet = new UkCharSet();
			var keyChar = Char.ToUpper(e.KeyChar);
			e.Handled = !(ukCharSet.ContainsValidUNOACharacters(keyChar) || keyChar == (char)Keys.Back);
		}
	}
}
