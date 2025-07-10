using System;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZTextBoxButtonTest : WebControlTest
	{
		protected ZTextBoxButton TextBoxButton
		{
			get { return (ZTextBoxButton)Control; }
		}

		public void TestCanBeEnabledByClient()
		{
			AssertEquals(false, TextBoxButton.CanBeEnabledByClient);

			TextBoxButton.CanBeEnabledByClient = true;
			AssertEquals(true, TextBoxButton.CanBeEnabledByClient);

			TextBoxButton.CanBeEnabledByClient = false;
			AssertEquals(false, TextBoxButton.CanBeEnabledByClient);
		}

		public void TestDataReadOnly()
		{
			var control = new ZTextBoxButtonForTest();
			AssertEquals(false, control.DataReadOnly);

			var dataSource = Factory.New<DummyBusinessObject>();
			control.BindTo = DummyBizoSchema.Z0_Description.Name;
			control.Bind(dataSource);
			
			dataSource.Z0_Description_ReadOnly = true;
			AssertEquals(true, dataSource.Z0_DescriptionInfo.ReadOnly);
			AssertEquals(true, control.DataReadOnly);

			dataSource.Z0_Description_ReadOnly = false;
			AssertEquals(false, dataSource.Z0_DescriptionInfo.ReadOnly);
			AssertEquals(false, control.DataReadOnly);
		}

		public void TestRenderControlBasedOnReadOnly()
		{
			var control = new ZTextBoxButtonForTest();
			var dataSource = Factory.New<DummyBusinessObject>();
			control.BindTo = DummyBizoSchema.Z0_Description.Name;
			control.Bind(dataSource);

			var output = new StringBuilder();
			var writer = new HtmlTextWriter(new StringWriter(output));

			control.CanBeEnabledByClient = false;
			dataSource.Z0_Description_ReadOnly = false;
			control.RenderControl(writer);
			AssertEquals(true, output.ToString().Contains("<input"));

			output.Clear();
			control.CanBeEnabledByClient = true;
			control.RenderControl(writer);
			AssertEquals(true, output.ToString().Contains("<input"));

			output.Clear();
			dataSource.Z0_Description_ReadOnly = true;
			control.RenderControl(writer);
			AssertEquals(true, output.ToString().Contains("<input"));

			output.Clear();
			control.CanBeEnabledByClient = false;
			control.RenderControl(writer);
			AssertEquals(false, output.ToString().Contains("<input"));
		}

		public void TestBindCreatesChildControls()
		{
			TextBoxButton.BindTo = DummyBizoSchema.Z0_VarCharMax.Name;
			AssertEquals("Precondition", 0, TextBoxButton.Controls.Count);
			TextBoxButton.Bind(Factory.New<DummyBusinessObject>());
			AssertChildControlsNameAndType();
		}

		protected virtual void AssertChildControlsNameAndType()
		{
			AssertEquals(2, TextBoxButton.Controls.Count);
			AssertEquals(typeof(ZTextBox), TextBoxButton.Controls[0].GetType());
			AssertEquals(typeof(HtmlInputButton), TextBoxButton.Controls[1].GetType());
			AssertEquals("TextBox", TextBoxButton.Controls[0].ID);
		}

		public void TestTextBoxAndButtonControls()
		{
			TextBoxButton.EnsureChildControlsInternal();
			AssertEquals(TextBoxButton.Controls[0], TextBoxButton.TextBoxControl);
			AssertEquals(TextBoxButton.Controls[1], TextBoxButton.ButtonControlInternal);
		}

		public void TestToolTipAssignment()
		{
			TextBoxButton.ToolTip = "This is the tooltip";
			TextBoxButton.EnsureChildControlsInternal();
			AssertEquals(typeof(ZTextBox), TextBoxButton.Controls[0].GetType());
			AssertEquals("This is the tooltip", ((ZTextBox)TextBoxButton.Controls[0]).ToolTip);
		}

		public virtual void TestControlDimension()
		{
			AssertEquals(TextBoxButton.ControlHeightInternal.Value, TextBoxButton.Height.Value);
			AssertEquals(TextBoxButton.MinWidthInternal.Value, TextBoxButton.Width.Value);
		}

		public void TestButtonText()
		{
			AssertEquals(TextBoxButton.ButtonText, TextBoxButton.ButtonControlInternal.Value);
			TextBoxButton.ButtonText = "New string";
			AssertEquals("New string", TextBoxButton.ButtonControlInternal.Value);
		}

		public void TestFont()
		{
			AssertEquals(TextBoxButton.TextBoxControl.Font, TextBoxButton.Font);
			TextBoxButton.Font.Italic = true;
			Assert(TextBoxButton.TextBoxControl.Font.Italic);
		}

		public void TestEnabled()
		{
			TextBoxButton.Enabled = true;
			AssertEquals(true, TextBoxButton.TextBoxControl.Enabled);
			AssertEquals(false, TextBoxButton.ButtonControlInternal.Disabled);

			TextBoxButton.TextBoxControl.Text = "ABC";
			TextBoxButton.Enabled = false;
			AssertEquals(false, TextBoxButton.TextBoxControl.Enabled);
			AssertEquals("ABC", TextBoxButton.TextBoxControl.Text);
			AssertEquals(true, TextBoxButton.ButtonControlInternal.Disabled);
		}

		public void TestReadOnly()
		{
			TextBoxButton.ReadOnly = true;
			AssertEquals(true, TextBoxButton.TextBoxControl.ReadOnly);
			AssertEquals(true, TextBoxButton.ButtonControlInternal.Disabled);

			TextBoxButton.TextBoxControl.Text = "ABC";
			TextBoxButton.ReadOnly = false;
			AssertEquals(false, TextBoxButton.TextBoxControl.ReadOnly);
			AssertEquals("ABC", TextBoxButton.TextBoxControl.Text);
			AssertEquals(false, TextBoxButton.ButtonControlInternal.Disabled);
		}

		public abstract void TestAssignSelectedValueToInvalidZType();
		public abstract void TestClickHandlerAssignment();

		public void TestOnPrerenderWithNoBackgroudStyle()
		{
			ZTextBoxButtonForTest button = new ZTextBoxButtonForTest();
			AssertEquals("ButtonText should be empty", string.Empty, button.ButtonText);
			button.OnPreRenderInternal(EventArgs.Empty);
			Assert("Should be no backgroud style", string.IsNullOrEmpty(button.ButtonControlInternal.Style["background"]));
			AssertEquals("ButtonText", "...", button.ButtonText);

			ZTextBoxButtonForTest button1 = new ZTextBoxButtonForTest();
			button1.ButtonText = "ABC";
			button1.OnPreRenderInternal(EventArgs.Empty);
			AssertEquals("ButtonText", "ABC", button1.ButtonText);

			AssertEquals("display style should be inline", "inline", button.Style[HtmlTextWriterStyle.Display]);
		}

		public void TestHideTextBox()
		{
			ZTextBoxButtonForTest button = new ZTextBoxButtonForTest();
			AssertEquals(false, button.HideTextBox);
			AssertNotEquals("Display", "none", button.TextBoxControl.Style[HtmlTextWriterStyle.Display]);
			AssertNull("Display", button.TextBoxControl.Style[HtmlTextWriterStyle.Display]);

			button.HideTextBox = true;
			AssertNotNull("Display", button.TextBoxControl.Style[HtmlTextWriterStyle.Display]);
			AssertEquals("Display", "none", button.TextBoxControl.Style[HtmlTextWriterStyle.Display]);
			AssertEquals("TextBoxControl.Width", Unit.Pixel(0).ToString(), button.TextBoxControl.Style[HtmlTextWriterStyle.Width]);
			AssertEquals("ButtonControl.Width", button.Width.ToString(), button.ButtonControlInternal.Style[HtmlTextWriterStyle.Width]);

			button.HideTextBox = false;
			AssertNull("Display", button.TextBoxControl.Style[HtmlTextWriterStyle.Display]);
			AssertEquals("TextBoxControl.Width", button.TextBoxWidth, button.TextBoxControl.Style[HtmlTextWriterStyle.Width]);
			AssertEquals("ButtonControl.Width", button.ButtonWidthInternal.ToString(), button.ButtonControlInternal.Style[HtmlTextWriterStyle.Width]);
		}

		public void TestRenderContentsOnly()
		{
			ZTextBoxButtonForTest button = new ZTextBoxButtonForTest();
			Assert("Default value of RenderContentsOnly", !button.RenderContentsOnly);

			StringBuilder sb = new StringBuilder();
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb));
			button.RenderInternal(writer);
			string outerHtml = sb.ToString();

			button.RenderContentsOnly = true;
			sb.Remove(0, sb.Length);
			button.RenderInternal(writer);
			string innerhtml = sb.ToString();

			Assert("All HTML output should be larger then child controls HTML output", outerHtml.Length > innerhtml.Length);
			Assert("All HTML output should contain child controls HTML output", outerHtml.Contains(innerhtml));
		}

		class ZTextBoxButtonForTest : ZTextBoxButton
		{
			protected override string ButtonBackgroundStyle => null;

			protected override string GetTextFromValue(IZType newValue) => null;

			protected override IZType GetSelectedValue() => null;

			protected override string ButtonClickHandler => string.Empty;
		}
	}
}
