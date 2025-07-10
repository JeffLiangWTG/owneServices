using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(ControlProperty))]
	sealed class ControlPropertyTest : NonPersistentBusinessObjectTestCase
	{
		readonly DisposableList disposablesCreatedForTest = new DisposableList(1);

		protected override BusinessObject GetNewBusinessObject()
		{
			var control = new KTextBox();
			var properties = control.GetType().GetProperties().First();
			disposablesCreatedForTest.Add(control);

			return new ControlProperty(properties, control);
		}

		public void TestErrorReadingValue()
		{
			using (var control = new TestControlWithSetOnlyProperty())
			{
				var controlInformationDigger = new ControlInformationDigger(control);
				var propertyInfo = control.GetType().GetProperties().Single(p => p.Name == "Foo");
				var property = new ControlProperty(propertyInfo, control);

				AssertEquals("If an exception occurs when getting the property value, we should tell the user but not propogate it", "PROPERTY UNREADABLE", property.Value);
			}
		}

		public void TestGetProperties()
		{
			using (var control = new KTextBox())
			{
				control.Text = "TestSango";
				control.MaxLength = 50;
				var properties = control.GetType().GetProperties().First(p => p.Name == "MaxLength");
				var controlProperty = new ControlProperty(properties, control);
				AssertEquals("MaxLength", controlProperty.Name);
				AssertEquals("50", controlProperty.Value);
				AssertEquals("System.Int32", controlProperty.PropertyType);
			}
		}

		public void TestValueIsReadOnlyWhenPropertyOnlyHasGet()
		{
			using (var control = new TestBasicControl())
			{
				var readOnlyProperty = control.GetType().GetProperty("ReadOnlyStringProperty");
				var readOnlyControlProperty = new ControlProperty(readOnlyProperty, control);
				Assert("Properties without setters are not editable", readOnlyControlProperty.ValueInfo.ReadOnly);
			}
		}

		public void TestComplexTypesAreReadOnly()
		{
			using (var control = new TestBasicControl())
			{
				var complexProperty = control.GetType().GetProperty("ControlProperty");
				var complexControlProperty = new ControlProperty(complexProperty, control);
				Assert("Properties of complex types cannot currently be handled so they must be set as read only", complexControlProperty.ValueInfo.ReadOnly);
			}
		}

		public void TestNullPropertyReturnsNULLValue()
		{
			using (var control = new TestBasicControl())
			{
				var nullProperty = control.GetType().GetProperty("NullProperty");
				var nullControlProperty = new ControlProperty(nullProperty, control);
				Assert("Null properties are displayed as NULL", nullControlProperty.Value == "NULL");
			}
		}

		public void TestSettingPropertyWithNull()
		{
			using (var control = new TestBasicControl())
			{
				AssertTypeIsEditable(control, "IntegerProperty", "NULL", 0);
				AssertTypeIsEditable(control, "StringProperty", "NULL", null);
				AssertTypeIsEditable(control, "BooleanProperty", "NULL", false);
			}
		}

		public void TestSimpleTypesAreEditable()
		{
			using (var control = new TestBasicControl())
			{
				AssertTypeIsEditable(control, "IntegerProperty", 10);
				AssertTypeIsEditable(control, "StringProperty", "tiana major9");
				AssertTypeIsEditable(control, "BooleanProperty", false);
			}
		}

		void AssertTypeIsEditable(Control c, string propertyName, object value) => AssertTypeIsEditable(c, propertyName, value.ToString(), value);

		void AssertTypeIsEditable(Control c, string propertyName, string inputValue, object expectedValue)
		{
			var property = c.GetType().GetProperty(propertyName);
			var controlProperty = new ControlProperty(property, c);
			Assert("PRE: Properties with setters are editable", !controlProperty.ValueInfo.ReadOnly);

			controlProperty.Value = inputValue;

			var updatedPropertyValue = property.GetValue(c, null);
			AssertEquals("Control value is updated via edit made through controlProperty", updatedPropertyValue, expectedValue);
		}

		protected override void TearDown()
		{
			disposablesCreatedForTest.Dispose();

			base.TearDown();
		}

		class TestBasicControl : Control
		{
			public int IntegerProperty { get; set; } = 21;
			public string StringProperty { get; set; } = "2NE1";
			public bool BooleanProperty { get; set; } = true;
			public string ReadOnlyStringProperty => "dembow";
			public int? NullProperty { get; set; }
			public Control ControlProperty { get; set; } = new Control();
		}
	}
}
