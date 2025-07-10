using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZButtonPopupTest : TestCase
	{
		#region TestButtonTextIsSetOnConstruction

		public void TestButtonTextIsSetOnConstruction()
		{
			AssertEquals("ButtonTextCore", Popup.ButtonText);
		}

		#endregion

		#region TestTextBoxIsHidden

		public void TestTextBoxIsHidden()
		{
			AssertEquals("none", Popup.TextBoxControl.Style[HtmlTextWriterStyle.Display]);
			AssertEquals(true, Popup.TextBoxControl.AutoPostBack);
		}

		#endregion

		#region TestGetTextFromValue

		public void TestGetTextFromValue()
		{
			AssertEquals(new ZString("cbr"), Popup.GetTextFromValue(new ZString("cbr")));
			AssertNull(Popup.GetTextFromValue(ZDateTime.Today));
		}

		#endregion

		#region TestGetSelectedValue

		public void TestGetSelectedValue()
		{
			Popup.TextBoxControl.Text = "cbr";
			AssertEquals(new ZString("cbr"), Popup.GetSelectedValue());
		}

		#endregion

		#region TestButtonControl

		public void TestButtonControl()
		{
			AssertEquals("10px", Popup.ButtonControl.Style["width"]);
			AssertEquals("ButtonToolTip", Popup.ButtonControl.Attributes[nameof(HtmlTextWriterAttribute.Title)]);
			AssertEquals("Button", Popup.ButtonControl.Attributes[nameof(HtmlTextWriterAttribute.Class)]);
			AssertEquals(false, Popup.ButtonControl.Disabled);

			DummyZButtonPopup disabledPopup = new DummyZButtonPopup();
			disabledPopup.Enabled = false;
			AssertEquals(true, disabledPopup.ButtonControl.Disabled);
		}

		#endregion

		#region TestControlHeight

		public void TestControlHeight()
		{
			AssertEquals(new Unit(21), Popup.Height);
		}

		#endregion

		#region TestButtonBackgroundStyle

		public void TestButtonBackgroundStyle()
		{
			AssertEquals("", Popup.ButtonBackgroundStyle);
		}

		#endregion

		#region TestDisplayStyle

		public void TestDisplayStyle()
		{
			AssertEquals(true, Popup.DisplayStyle.Contains(";HEIGHT:50px;WIDTH:100px"));

			DummyZButtonPopup newPopup = new DummyZButtonPopup();
			newPopup.PopupHeightForTest = 25;
			newPopup.PopupWidthForTest = 75;
			AssertEquals(true, newPopup.DisplayStyle.Contains(";HEIGHT:25px;WIDTH:75px"));
		}

		#endregion

		#region Implementation

		DummyZButtonPopup Popup
		{
			get { return fPopup ?? (fPopup = new DummyZButtonPopup()); }
		}

		DummyZButtonPopup fPopup;

		#region class DummyZButtonPopup

		class DummyZButtonPopup : ZButtonPopup
		{
			public new ZTextBox TextBoxControl
			{
				get { return base.TextBoxControl; }
			}

			public new string GetTextFromValue(IZType newValue)
			{
				return base.GetTextFromValue(newValue);
			}

			public new IZType GetSelectedValue()
			{
				return base.GetSelectedValue();
			}

			public new HtmlInputButton ButtonControl
			{
				get { return base.ButtonControl; }
			}

			public new string ButtonBackgroundStyle
			{
				get { return base.ButtonBackgroundStyle; }
			}

			public new string DisplayStyle
			{
				get { return base.DisplayStyle; }
			}

			#region abstract Implementation

			protected override string ButtonToolTip
			{
				get { return "ButtonToolTip"; }
			}

			protected override string ButtonTextCore
			{
				get { return "ButtonTextCore"; }
			}

			protected override int ButtonControlWidth
			{
				get { return 10; }
			}

			protected override string IFrameSourcePageName
			{
				get { return "IFrameSourcePageName"; }
			}

			protected override Unit PopupWidth
			{
				get { return new Unit(PopupWidthForTest); }
			}

			protected override Unit PopupHeight
			{
				get { return new Unit(PopupHeightForTest); }
			}

			public int PopupWidthForTest = 100;
			public int PopupHeightForTest = 50;

			#endregion
		}

		#endregion

		#endregion
	}
}
