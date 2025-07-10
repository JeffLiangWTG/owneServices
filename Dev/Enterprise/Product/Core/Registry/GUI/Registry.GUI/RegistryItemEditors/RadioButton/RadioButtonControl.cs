using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RadioButtonControl : ZUserControl
	{
		protected RadioButtonControl()
			: this(null)
		{
		}

		public RadioButtonControl(string customCaption)
		{
			InitializeComponent();
			optionGroupBox.Text = customCaption ?? (NoResString)"Option";
			needsDynamicResize = MeasureText(MinimumSize).Height > optionGroupBox.Font.Height;
			SetDataBinding(new RadioButtonWrapper(), "");
			ResizeControls();
		}

		public bool Value
		{
			get { return yesRadioButton.Checked; }
			set
			{
				yesRadioButton.Checked = value;
				noRadioButton.Checked = !value;
			}
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
				using (var graphics = CreateGraphics())
				{
					Size textMeasurement = MeasureText(Size);
					int fontHeight = optionGroupBox.Font.Height;
					int textLines = (textMeasurement.Height / fontHeight);

					int radioButtonsTop = fontHeight * (textLines + ControlDpiScalingHelper.ScaleToCurrentDpiX(1));
					ControlDpiScalingHelper.SetTop(ref yesRadioButton, radioButtonsTop, false);
					ControlDpiScalingHelper.SetTop(ref noRadioButton, radioButtonsTop, false);

					int controlHeight = fontHeight * (textLines + ControlDpiScalingHelper.ScaleToCurrentDpiY(4));
					ControlDpiScalingHelper.SetHeight(ref optionGroupBox, controlHeight, false);
					ControlDpiScalingHelper.SetHeight(this, controlHeight, false);

					int textWidth = textMeasurement.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(groupBoxPadding);
					if (textWidth > MinimumSize.Width)
					{
						ControlDpiScalingHelper.SetWidth(ref optionGroupBox, (textWidth > Width) ? Width : textWidth, false);
					}
					else
					{
						ControlDpiScalingHelper.SetWidth(ref optionGroupBox, MinimumSize.Width, false);
					}
				}
			}
		}

		Size MeasureText(Size proposedSize)
		{
			return TextRenderer.MeasureText(optionGroupBox.Text, optionGroupBox.Font, ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(proposedSize.Width) - groupBoxPadding, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(proposedSize.Height)), formatFlags);
		}

		readonly bool needsDynamicResize;
		const TextFormatFlags formatFlags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform; // This comes from CargoWise.Windows.UI.KGroupBox.OnPaint.
		const int groupBoxPadding = 14; // This comes from System.Windows.Forms.GroupBoxRenderer.DrawThemedGroupBoxWithText.
	}

	#region Wrapper

	public class RadioButtonWrapper : NonPersistentBusinessObject
	{
		ZBool _value;
		public ZBool Value
		{
			get { return _value; }
			set { SetNonPersistentPropertyValue(ValueInfo, ref _value, value); }
		}

		public ZPropertyInfo ValueInfo => GetZPropertyInfo(nameof(Value));
	}

	#endregion
}
