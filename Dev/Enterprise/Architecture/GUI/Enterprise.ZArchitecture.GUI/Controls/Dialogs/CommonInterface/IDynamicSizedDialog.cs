using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	interface IDynamicSizedDialog
	{
		KTextBox DialogTextField { get; }
		int MaxWidth { get; }
		int MaxHeight { get; }
		int VerticalPadding { get; }
	}

	static class IDynamicSizedDialogExtensions
	{
		internal static void EnsureDialogTextVisible<T>(this T dialog)
			where T : Control, IDynamicSizedDialog
		{
			var textBox = dialog.DialogTextField;
			var textSize = MeasureTextSize(dialog);
			int scrollBarsWidthForTest;

			var newTextBoxWidth = Math.Min(dialog.MaxWidth, (int)Utilities.Round(new decimal(textSize.Width), 0));
			var newTextBoxHeight = Math.Min(dialog.MaxHeight, (int)Math.Ceiling(textSize.Height) + ControlDpiScalingHelper.ScaleToCurrentDpiY(15));
			var increasedHeight = newTextBoxHeight - dialog.DialogTextField.Height;
			ControlDpiScalingHelper.SetHeight(dialog, dialog.Height + increasedHeight, false);
			ControlDpiScalingHelper.SetWidth(dialog, textBox.Left + newTextBoxWidth + (dialog.Width - dialog.ClientRectangle.Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(6), false);

			ControlDpiScalingHelper.SetWidth(textBox, newTextBoxWidth, false);
			ControlDpiScalingHelper.SetHeight(textBox, newTextBoxHeight, false);

			if (Utilities.Round(new decimal(textSize.Height), 0) > textBox.Height)
			{
				textBox.ScrollBars = ScrollBars.Vertical;
#if DEBUG
				ScrollBarsWidthForTest =
#endif
				scrollBarsWidthForTest = ControlDpiScalingHelper.ScaleToCurrentDpiX(SystemInformation.VerticalScrollBarWidth);
				ControlDpiScalingHelper.SetWidth(textBox, newTextBoxWidth + scrollBarsWidthForTest, false);
				ControlDpiScalingHelper.SetWidth(dialog, textBox.Left + newTextBoxWidth + (dialog.Width - dialog.ClientRectangle.Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(6) + scrollBarsWidthForTest, false);
			}
			else
			{
				textBox.ScrollBars = ScrollBars.None;
			}
		}

#if DEBUG
		[ThreadSafe]
		internal static int ScrollBarsWidthForTest;
#endif
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "No action needs to be taken if the cursor happens to be shown.")]
		internal static void EnsureCaretIsNotShown(this TextBox textBox, Control parent)
		{
			textBox.GotFocus += (s, e) => NativeMethods.HideCaret(textBox.Handle);
			textBox.LostFocus += (s, e) => NativeMethods.ShowCaret(parent.Handle);
		}

		///<remarks>returned Width and Height are scaled</remarks>
		internal static SizeF MeasureTextSize<T>(this T dialog)
			where T : Control, IDynamicSizedDialog
		{
			using (var graphics = dialog.DialogTextField.CreateGraphics())
			{
				graphics.PageUnit = GraphicsUnit.Pixel;
				// We replace tabs with a conservative number of spaces because TextRenderer.MeasureText underestimates how big a tab is.
				// Any fewer spaces and we can be tricked into making a message box that has no vertical scroll bar but has to be scrolled to show all its text (since it didn't fit).
				return TextRenderer.MeasureText(graphics, dialog.DialogTextField.Text.Replace("\t", "               "), dialog.DialogTextField.Font, ControlDpiScalingHelper.NewScaledSize(dialog.MaxWidth, 0, false), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
			}
		}

		internal static int DefaultDialogMaxWidth => Math.Min(ControlDpiScalingHelper.ScaleToCurrentDpiX(600), CurrentScreenInfo.Width * 2 / 3);

		internal static int DefaultDialogMaxHeight => CurrentScreenInfo.Height * 2 / 3;

		internal static Rectangle CurrentScreenInfo => CachedScreenInfo.Instance.getCurrentScreen(Cursor.Position);

		internal static class NativeMethods
		{
			[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
			internal static extern int HideCaret(IntPtr hWnd);

			[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
			internal static extern int ShowCaret(IntPtr hWnd);
		}
	}
}
