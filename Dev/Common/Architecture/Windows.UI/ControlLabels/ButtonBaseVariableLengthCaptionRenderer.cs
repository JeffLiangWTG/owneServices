using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Renders a variable length caption on a CheckBox or RadioButton.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	public sealed class ButtonBaseVariableLengthCaptionRenderer : IVariableLengthCaptionRenderer, IDisposable, IControlExtension //  ControlExtension,
	{
		public IExtendedControl Owner
		{
			get
			{
				return Parent as IExtendedControl;
			}
		}
		public void Initialize(IExtendedControl owner)
		{
			if (Parent != null)
			{
				throw new ArgumentException("The extension has already been initialized");
			}
			Argument.NotNull(owner, "owner");
			Parent = owner as Control;
		}

		public ButtonBaseVariableLengthCaptionRenderer(ButtonBase button)
		{
			this.button = button;
			try
			{
				disableAutoSizeWarning = true;
				IsCaptionOverridden = !string.IsNullOrEmpty(button.Text);
			}
			finally
			{
				disableAutoSizeWarning = false;
			}
			this.button.TextChanged += ButtonBase_TextChanged;
			this.button.MouseHover += Button_MouseHover;

			if (!(button is Button || button is CheckBox || button is RadioButton))
			{
				throw new ArgumentException("Currently only Button, CheckBox, RadioButton button types are supported, " + button.GetType().Name + " was not expected.");
			}
		}

		void Button_MouseHover(object sender, EventArgs e)
		{
			MouseOverButton();
		}

		#region IVariableLengthCaptionRenderer Members

		public string[] Captions
		{
			get { return captions; }
			set
			{
				if (!ArrayUtil.ArrayEquals(captions, value))
				{
					captions = value;
					UpdateText();
				}
			}
		}
		string[] captions;

		public MultilingualString ToolTipCaption
		{
			get { return toolTipCaption; }
			set
			{
				toolTipCaption = value;
				HasAutoSetToolTip = true;
			}
		}
		MultilingualString toolTipCaption;

		public bool IsCaptionOverridden
		{
			get { return !Enabled; }
			set { Enabled = !value; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Enabled = false;
		}

		#endregion

		#region Implementation

		readonly ButtonBase button;
		bool isSettingCheckBoxText;

		bool Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled != value)
				{
					if (Enabled)
					{
						button.MouseHover -= Button_MouseHover;
						button.ParentChanged -= new EventHandler(ButtonBase_ParentChanged);
						button.SizeChanged -= UpdateText;
						button.LocationChanged -= UpdateText;
						button.VisibleChanged -= UpdateText;
						button.HandleCreated -= UpdateText;
						button.AutoSizeChanged -= new EventHandler(ButtonBase_AutoSizeChanged);
					}
					this.enabled = value;
					if (Enabled)
					{
						button.MouseHover += Button_MouseHover;
						button.ParentChanged += new EventHandler(ButtonBase_ParentChanged);
						button.SizeChanged += UpdateText;
						button.LocationChanged += UpdateText;
						button.VisibleChanged += UpdateText;
						button.HandleCreated += UpdateText;
						button.AutoSizeChanged += new EventHandler(ButtonBase_AutoSizeChanged);
						UpdateText();
					}
					Parent = value ? button.Parent : null;

					if (!disableAutoSizeWarning && button.IsDesignMode() && !button.AutoSize && value && (button is CheckBox || button is RadioButton))
					{
						WarnAboutAutoSize();
					}
				}
			}
		}
		bool enabled;
		readonly bool disableAutoSizeWarning;

		Control Parent
		{
			get
			{
				return parent;
			}
			set
			{
				if (parent != null)
				{
					parent.SizeChanged -= UpdateText;
				}
				parent = value;
				if (parent != null)
				{
					parent.SizeChanged += UpdateText;
				}
			}
		}
		Control parent;

		void UpdateText(object sender, EventArgs e)
		{
			UpdateText();
		}

		void UpdateText()
		{
			if (Enabled && !isSettingCheckBoxText)
			{
				isSettingCheckBoxText = true;
				try
				{
					if (!IsCheckRightAligned || !button.IsDesignMode())
					{
						UpdateTextCore();
					}
					else
					{
						button.Text = "";
					}
				}
				finally
				{
					isSettingCheckBoxText = false;
				}
			}
		}

		void UpdateTextCore()
		{
			if (Captions != null && button.Created && button.Visible)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("CheckBoxVariableLengthCaptionRenderer.UpdateCheckBoxTextCore", ""))
				{
					//If the button is currently on a parent that's suspended, its width can't change, so wait until the next time Application.DoEvents happens.
					if (button.Parent == null || (!button.Parent.IsLayoutSuspended()))
					{
						UpdateTextCoreCore();
					}
					else
					{
						button.BeginInvoke(new Action(UpdateTextCoreCore));
					}
				}
			}
		}

		void UpdateTextCoreCore()
		{
			if (!Enabled)
			{
				return;
			}

			var isSettingCheckBoxTextOld = isSettingCheckBoxText;
			try
			{
				isSettingCheckBoxText = true;

				var text = button.AutoEllipsis ? Captions.OrderLongToShort().Last() : StringRenderingHelper.MeasureBestFit(Captions, HasObstruction, out bool hasObstruction);

				var previousWidth = button.Width;
				button.Text = text;
				if (text != null && button.AutoSize && IsCheckRightAligned)
				{
					button.PerformLayout();
					var left = Math.Max(0, button.Left - (button.Width - previousWidth));
					ControlDpiScalingHelper.SetLeft(button, left, false);
				}
			}
			finally
			{
				isSettingCheckBoxText = isSettingCheckBoxTextOld;
			}
		}

		Size MeasureTextSize(string text)
		{
			if (!button.AutoSize)
			{
				var width = button.Width;
				var height = button.Height;
				if (button is Button)
				{
					var buttonTextPaddingSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(8);
					width -= buttonTextPaddingSize;
					height -= buttonTextPaddingSize;
				}
				else
				{
					width -= CheckBoxWidth;
				}

				var proposedSize = ControlDpiScalingHelper.NewScaledSize(width, height, false);
				return StringRenderingHelper.MeasureText(text, button.Font, proposedSize);
			}

			return MeasureControlWidthAndTextHeight(text);
		}

		Size MeasureControlWidthAndTextHeight(string proposedText)
		{
			var textSize = TextRenderer.MeasureText(proposedText, button.Font);
			var size = textSize;

			if (!(button is Button))
			{
				ControlDpiScalingHelper.SetWidth(ref size, size.Width + CheckBoxWidth, false);
			}
			size += button.Padding.Size;
			return size;
		}

		public static int CheckBoxWidth => ControlDpiScalingHelper.ScaleToCurrentDpiX(16);

		bool HasObstruction(string caption)
		{
			bool result;

			var measuredTextSize = MeasureTextSize(caption);

			if (button.AutoSize)
			{
				using (PerformanceStatisticsCollector.StartMonitoring("ButtonBaseVariableLengthCaptionRenderer.HasObstruction", ""))
				{
					var bounds = IsCheckRightAligned ?
						ControlDpiScalingHelper.NewScaledRectangle(button.Right - measuredTextSize.Width < 0 ? 0 : button.Right - measuredTextSize.Width, button.Top, measuredTextSize.Width, measuredTextSize.Height, false) :
						ControlDpiScalingHelper.NewScaledRectangle(button.Location.X, button.Location.Y, measuredTextSize.Width, measuredTextSize.Height, false);
					result = button.Parent != null && button.Parent.DisplayRectangle.Width < bounds.Left + bounds.Width;
					if (!result && button.Parent != null)
					{
						foreach (Control control in button.Parent.Controls)
						{
							if (control != button && control.Visible
								&& ((!IsCheckRightAligned && bounds.Left <= control.Bounds.Left) || (IsCheckRightAligned && button.Location.X >= control.Bounds.Left))
								&& bounds.IntersectsWith(control.Bounds))
							{
								result = true;
								break;
							}
						}
					}
				}
			}
			else
			{
				return StringRenderingHelper.HasObstruction(button, measuredTextSize);
			}

			return result;
		}

		bool IsCheckRightAligned => !(button is Button) && (CheckAlign == ContentAlignment.TopRight || CheckAlign == ContentAlignment.MiddleRight || CheckAlign == ContentAlignment.BottomRight);

		ContentAlignment CheckAlign
		{
			get
			{
				CheckBox checkBox = button as CheckBox;
				RadioButton radioButton = button as RadioButton;
				ContentAlignment result = ContentAlignment.MiddleCenter;
				if (checkBox != null)
				{
					result = checkBox.CheckAlign;
				}
				if (radioButton != null)
				{
					result = radioButton.CheckAlign;
				}
				return result;
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		void WarnAboutAutoSize()
		{
			DesignTimeUI.ShowMessageOnceUntilIdle(button?.Site, (NoResString)"It is recommended you set the value of AutoSize to true for control '" + button.Name + (NoResString)"'"); // This is shown at design time only
		}

		void ButtonBase_ParentChanged(object sender, EventArgs e)
		{
			Parent = button.Parent;
		}

		void ButtonBase_TextChanged(object sender, EventArgs e)
		{
			if (!isSettingCheckBoxText)
			{
				IsCaptionOverridden = !string.IsNullOrEmpty(button.Text);
			}
		}

		void ButtonBase_AutoSizeChanged(object sender, EventArgs e)
		{
			if (!button.AutoSize && button.IsDesignMode())
			{
				WarnAboutAutoSize();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a ZButton")]
#if DEBUG
		public
#endif
		void MouseOverButton()
		{
			if (!button.AutoEllipsis)
			{
				var caption = IsCaptionOverridden ? button.Text : ToolTipCaption ?? Captions?.OrderLongToShort().FirstOrDefault() ?? button.Text;

				if (!string.IsNullOrEmpty(caption) && caption.Any(char.IsLetterOrDigit))
				{
					var fixedMnemonic = Regex.Replace(caption, "&(?!&)", "");

					if (!ToolTipService.HasToolTip(button) || HasAutoSetToolTip)
					{
						HasAutoSetToolTip = false;
						ToolTipService.SetToolTip(button, fixedMnemonic, 500);
					}
				}
			}
		}

		public bool HasAutoSetToolTip
		{
			get
			{
				return hasAutoSetToolTip;
			}
			set
			{
				hasAutoSetToolTip = value;
			}
		}
		bool hasAutoSetToolTip;
		#endregion
	}
}
