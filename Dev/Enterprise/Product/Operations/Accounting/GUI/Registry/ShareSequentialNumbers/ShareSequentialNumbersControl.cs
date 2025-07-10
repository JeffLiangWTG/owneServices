using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Registry.GUI;
using Res = Enterprise.Accounting.GUI.Res;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class ShareSequentialNumbersControl : RegistryZUserControl
	{
		public ShareSequentialNumbersControl()
			: this(null)
		{
			YesShareSequentialNumbers.CheckedChanged += new EventHandler(YesShareSequentialNumbers_CheckedChanged);
		}

		public ShareSequentialNumbersControl(string customCaption)
		{
			InitializeComponent();
			OptionGroupBox.Text = customCaption ?? Res.GetString("4b7cbab1-f401-4101-8a48-9f09fcb7a257", "Option");
			needsDynamicResize = MeasureText(MinimumSize).Height > OptionGroupBox.Font.Height;
			ResizeControls();
		}

		protected virtual void YesShareSequentialNumbers_CheckedChanged(object sender, EventArgs e)
		{
			NoShareSequentialNumbers.Checked = !YesShareSequentialNumbers.Checked;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OptionGroupBox.Enabled = !readOnly;
			NoShareSequentialNumbers.Checked = true;
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			if (needsDynamicResize)
			{
				ResizeControls();
			}
		}

		void ResizeControls()
		{
			Form form = FindForm();
			if (form == null || form.WindowState != FormWindowState.Minimized)
			{
				Size textMeasurement = MeasureText(Size);
				int fontHeight = OptionGroupBox.Font.Height;
				int textLines = (textMeasurement.Height / fontHeight);

				int shareSequentialNumberssTop = fontHeight * (textLines + 1);
				ControlDpiScalingHelper.SetTop(ref YesShareSequentialNumbers, shareSequentialNumberssTop, false);
				ControlDpiScalingHelper.SetTop(ref NoShareSequentialNumbers, shareSequentialNumberssTop, false);

				int controlHeight = fontHeight * (textLines + 4);
				ControlDpiScalingHelper.SetHeight(ref OptionGroupBox, controlHeight, false);
				ControlDpiScalingHelper.SetHeight(this, controlHeight, false);

				AdjustGroupBoxFinally();
			}
		}

		void AdjustGroupBoxFinally()
		{
			var adjustedWidth = NoShareSequentialNumbers.Location.X + NoShareSequentialNumbers.Width + YesShareSequentialNumbers.Location.X;
			ControlDpiScalingHelper.SetWidth(ref OptionGroupBox, Width < adjustedWidth ? adjustedWidth : Width, false);
		}

		Size MeasureText(Size proposedSize)
		{
			return TextRenderer.MeasureText(OptionGroupBox.Text, OptionGroupBox.Font,
				ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(proposedSize.Width) - GroupBoxPadding, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(proposedSize.Height)),
				FormatFlags);
		}

		readonly bool needsDynamicResize;
		const TextFormatFlags FormatFlags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform; // This comes from CargoWise.Windows.UI.KGroupBox.OnPaint.
		const int GroupBoxPadding = 14; // This comes from System.Windows.Forms.GroupBoxRenderer.DrawThemedGroupBoxWithText.
	}
}
