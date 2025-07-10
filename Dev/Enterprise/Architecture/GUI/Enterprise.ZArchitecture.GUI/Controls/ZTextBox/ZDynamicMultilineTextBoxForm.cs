using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class ZDynamicMultilineTextBoxForm : KForm, INoActivateModalForm
	{
		public ZDynamicMultilineTextBoxForm(ZTextBox parentTextBox, Size initialSize)
		{
			InitializeComponent();

			this.parentTextBox = parentTextBox;
			this.Size = (initialSize != Size.Empty)
				? initialSize
				: ControlDpiScalingHelper.NewScaledSize(parentTextBox.Width, (parentTextBox.Multiline) ? parentTextBox.Height : ControlDpiScalingHelper.ScaleToCurrentDpiY(arbitraryMultiLineTextBoxHeight), false);

			this.multilineTextBox.CharacterCasing = parentTextBox.CharacterCasing;
			this.multilineTextBox.AcceptsTab = parentTextBox.AcceptsTab;
			this.multilineTextBox.Text = parentTextBox.Text;
			this.multilineTextBox.MaxLength = parentTextBox.MaxLength;
			this.multilineTextBox.ReadOnly = parentTextBox.ReadOnly;
			this.multilineTextBox.Select(parentTextBox.SelectionStart, parentTextBox.SelectionLength);
			contextMenuManager = new ZTextBoxBaseContextMenuManager(parentTextBox, multilineTextBox, new NoMacroBox());
		}

		const int arbitraryMultiLineTextBoxHeight = 200;
		readonly ZTextBox parentTextBox;
		readonly ZTextBoxBaseContextMenuManager contextMenuManager;

		void multilineTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Tab && !AcceptsTab)
			{
				Close();
#if !WINZOR
				SendKeys.Send("{TAB}");
#endif
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			parentTextBox.Text = multilineTextBox.Text;
			parentTextBox.lastMultilineTextBoxSize = Size;
			parentTextBox.Select(multilineTextBox.SelectionStart, multilineTextBox.SelectionLength);

			base.OnClosed(e);
		}

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);
			Close();
		}

		#region Resizing

		void resizeLabel_MouseDown(object sender, MouseEventArgs e)
		{
			isResizing = true;
			sizeOffset = ControlDpiScalingHelper.NewScaledPoint(this.Right - Cursor.Position.X, this.Bottom - Cursor.Position.Y, false);
		}

		void resizeLabel_MouseMove(object sender, MouseEventArgs e)
		{
			if (isResizing)
			{
				//Clip cursor to dissallow sizing of form below originalWidth * arbitraryMultiLineTextBoxHeight
				var clipRectangle = RectangleToScreen(ControlDpiScalingHelper.NewScaledRectangle(parentTextBox.Width, ControlDpiScalingHelper.ScaleToCurrentDpiY(arbitraryMultiLineTextBoxHeight), Width, Height, false));
				clipRectangle.Offset(sizeOffset);
				Cursor.Clip = clipRectangle;
				this.Size = ControlDpiScalingHelper.NewScaledSize(Cursor.Position.X + sizeOffset.X - Location.X, Cursor.Position.Y + sizeOffset.Y - Location.Y, false);
			}
		}

		void resizeLabel_MouseUp(object sender, MouseEventArgs e)
		{
			isResizing = false;
			Cursor.Clip = Rectangle.Empty;
		}

		bool isResizing;
		Point sizeOffset;

		#endregion

		internal bool AcceptsTab
		{
			get { return this.multilineTextBox.AcceptsTab; }
		}

		internal KTextBox MultilineTextBox => multilineTextBox;
	}
}
