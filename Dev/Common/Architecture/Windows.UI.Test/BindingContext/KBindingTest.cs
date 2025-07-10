using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class KBindingTest : TestCase
	{
		#region Format/Parse Exception Handling

		public void TestOnFormat_ExceptionHandling()
		{
			TextBox.DataBindings.RemoveBinding(Binding.PropertyName);

			KBinding binding = new KBinding("Text", Entity, "StringProperty");
			TextBox.DataBindings.Add(binding);
			binding.Format += delegate
			{ throw new FormatException("Format failed!"); };

			AssertEquals("No reported exceptions", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			try
			{
				Entity.StringProperty = "TEST";
			}
			catch (FormatException)
			{
			}
			AssertEquals("Reported exception during Format", false, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			ErrorReporter.Clear();
		}

		public void TestOnParse_ExceptionHandling()
		{
			TextBox.DataBindings.RemoveBinding(Binding.PropertyName);

			KBinding binding = new KBinding("Text", Entity, "StringProperty");
			TextBox.DataBindings.Add(binding);
			binding.Parse += delegate
			{ throw new FormatException("Parse failed!"); };

			AssertEquals("No reported exceptions", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			try
			{
				TextBox.Text = "TEST";
				Form.ActiveControl = TextBox2;
			}
			catch (FormatException)
			{
			}
			AssertEquals("Reported exception during Parse", false, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			ErrorReporter.Clear();
		}

		public void TestOnFormat_FormatExceptionReporting()
		{
			using (var checkbox = new KCheckBox())
			{
				Form.Controls.Add(checkbox);
				Form.Show();

				checkbox.DataBindings.RemoveBinding("Checked");

				AssertExceptionThrown(typeof(FormatException), () => checkbox.DataBindings.Add(new KBinding("Checked", Entity, "StringProperty")));
				AssertEquals("Reported exception during Format", @"KBinding Formatting Error
DataSource    : CargoWise.Windows.UI.Testing.KBindingTest+TestEntity
BindingMember : StringProperty
PropertyName  : Checked
DesiredType   : System.Boolean
Value         : ", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Converting

		public void TestConvertStringToString()
		{
			AssertEquals("splaty", Convert("splaty", typeof(string)));
		}

		public void TestConvertNullToString()
		{
			AssertEquals(null, Convert(null, typeof(string)));
		}

		public void TestConvertDBNullToString()
		{
			AssertEquals(null, Convert(null, typeof(string)));
		}

		public void TestConvertMockTypeToString()
		{
			string mockType = "splaty";
			AssertEquals("splaty", Convert(mockType, typeof(string)));
		}

		public void TestConvertMockTypeToMockType()
		{
			string mockType = "splaty";
			AssertEquals("splaty", Convert(mockType, typeof(string)));
		}

		public void TestConvertStringToMockType()
		{
			AssertEquals("splaty", Convert("splaty", typeof(string)));
		}

		#endregion

		#region DecimalPlaces Meta-data

		public void TestConvertToNumericToString_WithDecimalPlaces()
		{
			AssertEquals("-1.23", Convert(-1.23456, typeof(string)));
		}

		public void TestConvertToStringToNumeric_WithDecimalPlaces()
		{
			AssertEquals(-1.23f, Convert("-1.23456", typeof(float)));
		}

		#endregion

		#region Nullable Types

		public void TestBindToNullableType()
		{
			KCheckBox checkbox = new KCheckBox();
			Form.Controls.Add(checkbox);
			Form.Show();

			Entity.NullableProperty = true;
			checkbox.DataBindings.Add(new KBinding("Checked", Entity, "NullableProperty")); // must add binding after value set for the test
			AssertEquals("Pulling nullable data from the data source", true, Entity.NullableProperty);
			AssertEquals("Pulling nullable data from the data source", true, checkbox.Checked);

			checkbox.Focus();
			checkbox.Checked = false;
			Label l = new Label();
			Form.Controls.Add(l);
			l.Focus();
			AssertEquals("Pushing nullable data from the data source", false, Entity.NullableProperty);
			AssertEquals("Pushing nullable data from the data source", false, checkbox.Checked);
		}

		#endregion

		#region Test Classes

		public class TestEntity : ComponentModel.Testing.KComponentWithPropertyChange
		{
			public string StringProperty
			{
				get { return stringProperty; }
				set
				{
					if (stringProperty != value)
					{
						stringProperty = value;
						FirePropertyChanged(nameof(StringProperty));
					}
				}
			}
			string stringProperty;

			[MetaDataValue(MetaDataTypes.DecimalPlaces, 2)]
			public float PropertyWithDecimalPlaces { get; set; }

			public bool? NullableProperty
			{
				get { return nullableProperty; }
				set
				{
					if (nullableProperty != value)
					{
						nullableProperty = value;
						FirePropertyChanged(nameof(NullableProperty));
					}
				}
			}
			bool? nullableProperty;
		}

		#endregion

		#region Implementation

		object Convert(object value, Type desiredType)
		{
			ConvertEventArgs e = new ConvertEventArgs(value, desiredType);
			Binding.Convert(e);
			return e.Value;
		}

		KBinding Binding
		{
			get
			{
				if (binding == null)
				{
					binding = new KBinding("Text", Entity, "PropertyWithDecimalPlaces");
				}
				return binding;
			}
		}
		KBinding binding;

		KForm Form
		{
			get
			{
				if (form == null)
				{
					form = new KForm();
				}
				return form;
			}
		}
		KForm form;

		TextBox TextBox
		{
			get
			{
				if (textBox == null)
				{
					textBox = new TextBox();
				}
				return textBox;
			}
		}
		TextBox textBox;

		TextBox TextBox2
		{
			get
			{
				if (textBox2 == null)
				{
					textBox2 = new TextBox();
				}
				return textBox2;
			}
		}
		TextBox textBox2;

		TestEntity Entity
		{
			get
			{
				if (entity == null)
				{
					entity = new TestEntity();
				}
				return entity;
			}
		}
		TestEntity entity;

		protected override void SetUp()
		{
			base.SetUp();
			TextBox.DataBindings.Add(Binding);
			Form.Controls.Add(TextBox);
			Form.Controls.Add(TextBox2);
			Form.Show();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (textBox != null)
			{
				textBox.Dispose();
			}
			if (textBox2 != null)
			{
				textBox2.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
