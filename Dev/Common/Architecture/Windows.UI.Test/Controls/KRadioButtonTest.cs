using System;
using System.ComponentModel;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class KRadioButtonTest : TestCase
	{
		public void TestCheckedPropertyDescriptor()
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(KRadioButton));
			PropertyDescriptor checkedProperty = properties["Checked"];
			AssertEquals("ComponentType on Checked property should be correct", typeof(KRadioButton), checkedProperty.ComponentType);
		}

		public void TestCommittedToDataSourceOnChange()
		{
			KRadioButtonForTest box = new KRadioButtonForTest();
			Form.BindingSource.SetBindingMember(box, "Boolean");
			Form.BindingSource.DataSourceType = typeof(TestEntity);

			Form.Controls.Add(box);
			Form.Show();
			TestEntity entity = new TestEntity();
			Form.SetDataBinding(entity, "");

			AssertEquals("Checked is false initially", false, entity.Boolean);
			box.OnClick(EventArgs.Empty);
			AssertEquals("Checked should commit to data source immediately", true, entity.Boolean);
		}

		#region IVariableLengthCaptionRenderer

		public void TestText_ComesFromCaptionRenderer()
		{
			Form.Controls.Add(RadioButton);
			Form.Show();

			((IVariableLengthCaptionRenderer)RadioButton).Captions = new string[] { "Caption" };
			AssertEquals("Text from caption renderer", "Caption", RadioButton.Text);

			RadioButton.Text = "Text";
			AssertEquals("Text when setting Text explicitly", "Text", RadioButton.Text);

			((IVariableLengthCaptionRenderer)RadioButton).Captions = new string[] { "NewCaption" };
			AssertEquals("Text when setting Text explicitly (renderer doesn't override)", "Text", RadioButton.Text);
		}

		public void TestText_ShouldSerialize()
		{
			RadioButton.AutoSize = true;
			using (ComponentExtensions.SwitchToDesignMode())
			{
				Form.Controls.Add(RadioButton);
				Form.Show();

				((IVariableLengthCaptionRenderer)RadioButton).Captions = new string[] { "Caption" };
				AssertEquals("Caption", RadioButton.Text);
				AssertEquals("Don't serialize auto-generated text", false, TypeDescriptor.GetProperties(RadioButton.GetType())["Text"].ShouldSerializeValue(RadioButton));

				RadioButton.Text = "Text";
				AssertEquals("Serialize text that has been explicitly set on the control", true, TypeDescriptor.GetProperties(RadioButton.GetType())["Text"].ShouldSerializeValue(RadioButton));

				RadioButton.Text = "";
				AssertEquals("Don't serialize empty text", false, TypeDescriptor.GetProperties(RadioButton.GetType())["Text"].ShouldSerializeValue(RadioButton));
			}
		}

		#endregion

		#region Test Classes

		public class TestEntity : ComponentModel.Testing.KComponent
		{
			public bool Boolean { get; set; }
		}

		public class TestForm : KForm
		{
			public new KBindingSource BindingSource
			{ get { return base.BindingSource; } }
		}

		class KRadioButtonForTest : KRadioButton
		{
			internal new void OnClick(EventArgs e) => base.OnClick(e);
		}

		#endregion

		#region Implementation

		TestForm Form
		{
			get { return form ?? (form = new TestForm()); }
		}
		TestForm form;

		KRadioButton RadioButton
		{
			get { return radioButton ?? (radioButton = new KRadioButton()); }
		}
		KRadioButton radioButton;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
