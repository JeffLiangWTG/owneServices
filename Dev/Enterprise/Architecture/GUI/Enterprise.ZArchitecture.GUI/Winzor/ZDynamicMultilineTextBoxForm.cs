using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZDynamicMultilineTextBoxForm : Control
	{
		public override bool RenderInPortal => true;
		protected override bool ShouldRender => true;
		const int arbitraryMultiLineTextBoxHeight = 200;

		readonly ZTextBox parentTextBox;
		public ZTextBox ParentTextBox => parentTextBox;
		internal bool AcceptsTab => multilineTextBox.AcceptsTab;

		readonly KTextBox multilineTextBox;
		public KTextBox MultilineTextBox => multilineTextBox;

		public ZDynamicMultilineTextBoxForm(ZTextBox parentTextBox, Size initialSize)
		{
			SetVisibleState(false);
			this.parentTextBox = parentTextBox;
			Parent = parentTextBox;

			multilineTextBox = new KTextBox();
			var initialHeight = parentTextBox.Multiline ? Math.Max(arbitraryMultiLineTextBoxHeight, parentTextBox.Height) : arbitraryMultiLineTextBoxHeight;

			Size = (initialSize != Size.Empty)
				? initialSize : ControlDpiScalingHelper.NewScaledSize(parentTextBox.Width, initialHeight , isInStandardDpi: false);

			InitializeMultilineTextBox();

			var contextMenuManager = new ZTextBoxBaseContextMenuManager(parentTextBox, multilineTextBox, new NoMacroBox());

			Controls.Add(multilineTextBox);
		}

		public override bool UseParentDivForLayout => false;

		bool isDynamicMultilineTextBoxFormTextChanged;

		public void ShowDynamicTextBox()
		{
			Visible = true;
			parentTextBox.Controls.Remove(this);
			parentTextBox.Parent.Controls.Add(this);
			multilineTextBox.LostFocus += MultilineTextBox_LostFocus;
			multilineTextBox.TextChanged += MultilineTextBox_TextChanged;
		}

		void MultilineTextBox_TextChanged(object sender, EventArgs e)
		{
			isDynamicMultilineTextBoxFormTextChanged = true;
		}

		void MultilineTextBox_LostFocus(object sender, EventArgs e)
		{
			HideDynamicTextBox();
		}

		public void HideDynamicTextBox()
		{
			parentTextBox.Text = multilineTextBox.Text;
			parentTextBox.lastMultilineTextBoxSize = Size;
			parentTextBox.Select(multilineTextBox.SelectionStart, multilineTextBox.SelectionLength);

			SetVisibleState(false);
			parentTextBox.Parent.Controls.Remove(this);
			parentTextBox.Controls.Add(this);
			multilineTextBox.LostFocus -= MultilineTextBox_LostFocus;
			multilineTextBox.TextChanged -= MultilineTextBox_TextChanged;
			if (isDynamicMultilineTextBoxFormTextChanged)
			{
				FindForm().Validate();
			}
			parentTextBox.Enabled = true;
			isDynamicMultilineTextBoxFormTextChanged = false;
		}

		public void InitializeMultilineTextBox()
		{
			multilineTextBox.CharacterCasing = parentTextBox.CharacterCasing;
			multilineTextBox.AcceptsTab = parentTextBox.AcceptsTab;
			multilineTextBox.Text = parentTextBox.Text;
			multilineTextBox.MaxLength = parentTextBox.MaxLength;
			multilineTextBox.ReadOnly = parentTextBox.ReadOnly;
			multilineTextBox.Select(parentTextBox.SelectionStart, parentTextBox.SelectionLength);

			multilineTextBox.Multiline = true;
			multilineTextBox.Size = Size;
			multilineTextBox.ExtraStyleString += $"min-width: {parentTextBox.Width}px; min-height: {arbitraryMultiLineTextBoxHeight}px;";
			multilineTextBox.ScrollBars = ScrollBars.Both;
			multilineTextBox.KeyDown += multilineTextBox_KeyDown;
		}

		void multilineTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Tab && !AcceptsTab)
			{
				HideDynamicTextBox();
			}
		}
	}
}
