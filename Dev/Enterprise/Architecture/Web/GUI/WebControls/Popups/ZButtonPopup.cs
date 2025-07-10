using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI
{
	#region SuppressResourceStringsCheckRegion

	public abstract class ZButtonPopup : ZTextIFramePopup
	{
		public ZButtonPopup()
		{
			TextBoxControl.AutoPostBack = true;
			HideTextBox = true;
			ButtonText = ButtonTextCore;
		}

		#region GetValue

		protected override string GetTextFromValue(IZType newValue)
		{
			string result = null;

			if (newValue is ZString)
			{
				result = (ZString)newValue;
			}

			return result;
		}

		protected override IZType GetSelectedValue()
		{
			return new ZString(TextBoxControl.Text);
		}

		#endregion

		#region Text

		protected abstract string ButtonToolTip
		{
			get;
		}

		protected abstract string ButtonTextCore
		{
			get;
		}

		#endregion

		#region Button Control

		protected sealed override HtmlInputButton ButtonControl
		{
			get
			{
				if (fButtonControl == null)
				{
					fButtonControl = GetPopupButton();
					fButtonControl.Style.Add("width", ButtonWidth.ToString());
					fButtonControl.Style.Add("height", ControlHeight.ToString());
					fButtonControl.Attributes[nameof(HtmlTextWriterAttribute.Title)] = ButtonToolTip;
					fButtonControl.Attributes[nameof(HtmlTextWriterAttribute.Class)] = "Button";
					fButtonControl.Disabled = !Enabled;
				}
				return fButtonControl;
			}
		}

		protected virtual HtmlInputButton GetPopupButton()
		{
			return new HtmlInputButton();
		}

		protected override Unit ControlHeight
		{
			get { return Unit.Pixel(21); }
		}

		protected override string ButtonBackgroundStyle
		{
			get { return ""; }
		}

		protected override string DisplayStyle
		{
			get { return fDisplayStyle ?? (fDisplayStyle = base.DisplayStyle + string.Format(";HEIGHT:{0};WIDTH:{1}", PopupHeight, PopupWidth)); }
		}

		HtmlInputButton fButtonControl;
		string fDisplayStyle;

		#endregion

		#region Button Dimensions

		public override sealed string TextBoxWidth
		{
			get { return Unit.Pixel(0).ToString(); }
		}

		protected override sealed Unit ButtonWidth
		{
			get { return Unit.Pixel(Convert.ToInt32(Width.Value)); }
		}

		protected override sealed Unit MinWidth
		{
			get { return Unit.Pixel(ButtonControlWidth); } // Changes the button width
		}

		protected abstract int ButtonControlWidth { get; }

		#endregion
	}

	#endregion
}
