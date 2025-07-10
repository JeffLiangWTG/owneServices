using System;
using System.ComponentModel;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KCheckBoxTest : TestCase
	{
		public void TestCommittedToDataSourceOnChange()
		{
			KCheckBoxForTest box = new KCheckBoxForTest();
			Form.BindingSource.SetBindingMember(box, "Boolean");
			Form.BindingSource.DataSourceType = typeof(TestEntity);

			Form.Controls.Add(box);
			Form.Show();
			TestEntity entity = new TestEntity();
			Form.SetDataBinding(entity, "");

			box.Focus();
			box.OnClick(EventArgs.Empty);
			AssertEquals("Checked should commit to data source immediately", true, entity.Boolean);

			box.OnClick(EventArgs.Empty);
			AssertEquals("Checked should commit to data source immediately", false, entity.Boolean);
		}

		#region IVariableLengthCaptionRenderer

		public void TestText_ComesFromCaptionRenderer()
		{
			Form.Controls.Add(CheckBox);
			Form.Show();

			((IVariableLengthCaptionRenderer)CheckBox).Captions = new string[] { "Caption" };
			AssertEquals("Text from caption renderer", "Caption", CheckBox.Text);

			CheckBox.Text = "Text";
			AssertEquals("Text when setting Text explicitly", "Text", CheckBox.Text);

			((IVariableLengthCaptionRenderer)CheckBox).Captions = new string[] { "NewCaption" };
			AssertEquals("Text when setting Text explicitly (renderer doesn't override)", "Text", CheckBox.Text);
		}

		public void TestText_ShouldSerialize()
		{
			CheckBox.AutoSize = true;
			using (ComponentExtensions.SwitchToDesignMode())
			{
				Form.Controls.Add(CheckBox);
				Form.Show();

				((IVariableLengthCaptionRenderer)CheckBox).Captions = new string[] { "Caption" };
				AssertEquals("Caption", CheckBox.Text);
				AssertEquals("Don't serialize auto-generated text", false, TypeDescriptor.GetProperties(CheckBox.GetType())["Text"].ShouldSerializeValue(CheckBox));

				CheckBox.Text = "Text";
				AssertEquals("Serialize text that has been explicitly set on the control", true, TypeDescriptor.GetProperties(CheckBox.GetType())["Text"].ShouldSerializeValue(CheckBox));

				CheckBox.Text = "";
				AssertEquals("Don't serialize empty text", false, TypeDescriptor.GetProperties(CheckBox.GetType())["Text"].ShouldSerializeValue(CheckBox));
			}
		}

		#endregion

		#region Implementation

		MyForm Form
		{
			get { return form ?? (form = new MyForm()); }
		}
		MyForm form;

		KCheckBox CheckBox
		{
			get { return checkBox ?? (checkBox = new KCheckBox()); }
		}
		KCheckBox checkBox;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion

		#region Test Classes

		public class TestEntity : ComponentModel.Testing.KComponentWithPropertyChange
		{
			public bool Boolean { get; set; }
		}

		public class MyForm : KForm
		{
			public new KBindingSource BindingSource
			{ get { return base.BindingSource; } }
		}

		class KCheckBoxForTest : KCheckBox
		{
			internal new void OnClick(EventArgs e) => base.OnClick(e);
		}

		#endregion
	}
}
