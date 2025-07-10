using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(true)]
	public partial class ZIntEdit : ZTextBox
	{
		public ZIntEdit()
		{
			InitializeComponent();
		}

		protected override Type DataSourceType
		{
			get { return typeof(ZInt); }
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			const uint Backspace = 8;
			const uint CarriageReturn = 13;

			var isValidChar = ((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == Backspace || e.KeyChar == CarriageReturn);
			e.Handled = !isValidChar;

			base.OnKeyPress(e);
		}
	}
}
