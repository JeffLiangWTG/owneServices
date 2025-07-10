using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	partial class MultiActionButtonDialogForm : ZChildForm, IDynamicSizedDialog
	{
		internal MultiActionButtonDialogForm(string caption, string message, Control control)
		{
			InitializeComponent();

			Text = caption;
			MessageTextBox.Text = message;
			control.Dock = DockStyle.Fill;
			ControlDpiScalingHelper.SetWidth(this, control.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(20), false);
			ActionsGroupBox.Controls.Add(control);

			this.EnsureDialogTextVisible();
			MessageTextBox.EnsureCaretIsNotShown(this);
		}

#if DEBUG
		internal MultiActionButtonDialogForm()
		{
			InitializeComponent();
		}
#endif

		#region IDynamicSizedDialog Members

		KTextBox IDynamicSizedDialog.DialogTextField => MessageTextBox;

		int IDynamicSizedDialog.MaxWidth => IDynamicSizedDialogExtensions.DefaultDialogMaxWidth;

		int IDynamicSizedDialog.MaxHeight => IDynamicSizedDialogExtensions.DefaultDialogMaxHeight;

		int IDynamicSizedDialog.VerticalPadding => 28;

		#endregion
	}
}
