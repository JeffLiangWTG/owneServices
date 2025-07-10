using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	class TextBoxDecimalPlacesHandlerTest : TestCase
	{
		public void TestDecimalPlaces_WhenTextSet()
		{
			Form.Controls.Add(TextBox);
			Form.Show();

			DecimalPlacesHandler.DecimalPlaces = 2;
			TextBox.Text = "";
			AssertEquals("", TextBox.Text);
			TextBox.Text = "-0.1";
			AssertEquals("-0.10", TextBox.Text);
			TextBox.Text = "1.1234";
			AssertEquals("1.12", TextBox.Text);
			TextBox.Text = "-1.1234";
			AssertEquals("-1.12", TextBox.Text);
			TextBox.Text = "1.1234.1234";
			AssertEquals("1.12", TextBox.Text);
			TextBox.Text = "12";
			AssertEquals("12.00", TextBox.Text);
			TextBox.Text = " 12.1 ";
			AssertEquals("12.10", TextBox.Text);

			DecimalPlacesHandler.DecimalPlaces = 0;
			TextBox.Text = "1.12";
			AssertEquals("1", TextBox.Text);
			TextBox.Text = "1.1.1";
			AssertEquals("1", TextBox.Text);
		}

		public void TestDecimalPlaces_WhenTextEntered()
		{
			Form.Controls.Add(TextBox);
			Form.Show();

			TextBox.EnteringText = true;
			DecimalPlacesHandler.DecimalPlaces = 2;

			DecimalPlacesHandler.SelectionStart = 5;
			TextBox.Text = "1.123";
			AssertEquals("1.12", TextBox.Text);

			DecimalPlacesHandler.SelectionStart = 5;
			TextBox.Text = "1.12.";
			AssertEquals("1.12", TextBox.Text);

			DecimalPlacesHandler.SelectionStart = 5;
			TextBox.Text = "1.12 ";
			AssertEquals("1.12", TextBox.Text);
			AssertEquals(4, DecimalPlacesHandler.SelectionStart);

			DecimalPlacesHandler.SelectionStart = 3;
			TextBox.Text = "1.1";
			AssertEquals("1.1", TextBox.Text);

			DecimalPlacesHandler.SelectionStart = 2;
			TextBox.Text = "12";
			AssertEquals("12", TextBox.Text);
			AssertEquals(2, DecimalPlacesHandler.SelectionStart);
		}

		[ExpectNoExceptions()]
		public void TestDecimalPlaces_WhenTextEntered_HandleInvalidValueFromPaste()
		{
			Form.Controls.Add(TextBox);
			Form.Show();

			TextBox.EnteringText = true;
			DecimalPlacesHandler.DecimalPlaces = 0;

			// although it is not possible to enter directly, but it is possible to paste
			DecimalPlacesHandler.SelectionStart = 8;
			TextBox.Text = "12.34.56";
			AssertEquals("Text", "12", TextBox.Text);
			AssertEquals("SelectionStart", 2, DecimalPlacesHandler.SelectionStart);

			DecimalPlacesHandler.DecimalPlaces = 1;
			DecimalPlacesHandler.SelectionStart = 8;
			TextBox.Text = "12.34.56";
			AssertEquals("Text", "12.3", TextBox.Text);
			AssertEquals("SelectionStart", 4, DecimalPlacesHandler.SelectionStart);

			DecimalPlacesHandler.DecimalPlaces = 2;
			DecimalPlacesHandler.SelectionStart = 8;
			TextBox.Text = "12.34.56";
			AssertEquals("Text", "12.34", TextBox.Text);
			AssertEquals("SelectionStart", 5, DecimalPlacesHandler.SelectionStart);

			DecimalPlacesHandler.DecimalPlaces = 3;
			DecimalPlacesHandler.SelectionStart = 8;
			TextBox.Text = "12.34.56";
			AssertEquals("Text", "12.34", TextBox.Text);
			AssertEquals("SelectionStart", 5, DecimalPlacesHandler.SelectionStart);

			DecimalPlacesHandler.SelectionStart = 18;
			TextBox.Text = "1234.5678.90123";
			AssertEquals("Text", "1234.567", TextBox.Text);
			AssertEquals("SelectionStart", 8, DecimalPlacesHandler.SelectionStart);
		}

		public void TestDecimalPlaces_DecimalPlacesInBoundObjectNotChangedIfUserDidntMakeChange()
		{
			CheckBox siblingControl = new CheckBox();
			siblingControl.Left = 200;
			Form.Controls.Add(TextBox);
			Form.Controls.Add(siblingControl);

			DataSource.DecimalValue_Places = 2;
			Form.Show();

			siblingControl.Focus();
			DataSource.DecimalValue = 1.234m;

			KBindingSource bindingSource = new KBindingSource();
			bindingSource.ContainerControl = Form;
			bindingSource.DataSourceType = typeof(TestDataSource);
			bindingSource.SetBindingMember(TextBox, "DecimalValue");
			bindingSource.DataSource = DataSource;

			TextBox.Focus();
			siblingControl.Focus();
			AssertEquals(1.234m, DataSource.DecimalValue);

			TextBox.Focus();
			TextBox.Text = "2.34";
			siblingControl.Focus();
			AssertEquals(2.34m, DataSource.DecimalValue);
		}

		#region Test Classes

		class TestTextBox : TextBox
		{
			[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
			[BindingMetaDataProperty(MetaDataTypes.DecimalPlaces, "DecimalPlaces")]
			public override string Text
			{
				get { return base.Text; }
				set
				{
					using (EnteringText ? new DisposableAction(() => { }) : DecimalPlacesHandler.NotifyInTextSetter())
					{
						base.Text = value;
					}
				}
			}

			public int DecimalPlaces
			{
				get { return DecimalPlacesHandler.DecimalPlaces; }
				set { DecimalPlacesHandler.DecimalPlaces = value; }
			}

			public bool EnteringText { get; set; }

			public TestTextBoxDecimalPlacesHandler DecimalPlacesHandler
			{ get { return decimalPlacesHandler ?? (decimalPlacesHandler = new TestTextBoxDecimalPlacesHandler(this)); } }
			TestTextBoxDecimalPlacesHandler decimalPlacesHandler;
		}

		class TestTextBoxDecimalPlacesHandler : TextBoxDecimalPlacesHandler
		{
			public TestTextBoxDecimalPlacesHandler(TextBoxBase textBox)
				: base(textBox)
			{
			}

			protected internal override int SelectionStart { get; set; }
		}

		protected class TestDataSource : ComponentModel.Testing.KComponentWithPropertyChange
		{
			[DecimalPlaces("DecimalValue_Places")]
			public decimal DecimalValue
			{
				get { return decimalValue; }
				set
				{
					if (decimalValue != value)
					{
						decimalValue = value;
						FirePropertyChanged(nameof(DecimalValue));
					}
				}
			}
			decimal decimalValue;

			public int DecimalValue_Places
			{
				get { return decimalValue_Places; }
				set
				{
					if (decimalValue_Places != value)
					{
						decimalValue_Places = value;
						FirePropertyChanged(nameof(DecimalValue_Places));
					}
				}
			}
			int decimalValue_Places;
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		KForm Form
		{ get { return form ?? (form = new KForm()); } }
		KForm form;

		TestTextBox TextBox
		{ get { return textBox ?? (textBox = new TestTextBox()); } }
		TestTextBox textBox;

		TestTextBoxDecimalPlacesHandler DecimalPlacesHandler
		{ get { return TextBox.DecimalPlacesHandler; } }

		TestDataSource DataSource
		{ get { return dataSource ?? (dataSource = new TestDataSource()); } }
		TestDataSource dataSource;

		#endregion
	}
}
