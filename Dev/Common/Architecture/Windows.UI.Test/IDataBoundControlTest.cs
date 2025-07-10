using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Testing;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class IDataBoundControlTest : TestCase
	{
		public void TestDataBinding_DataSourceDoesNotHaveDataMember()
		{
			var textBox = new KTextBox();
			Form.Controls.Add(textBox);

			AssertExceptionThrown(
				"GIVEN data-source does not have data-member WHEN binding should throw error stating data-source type, data-member and control type",
				typeof(KDataBindingException),
				"XXX_DataMember: Could not find data-member in data-source type 'TestObject' with binding-path '' and binding-field 'XXX_DataMember' for control-type 'KTextBox'.",
				() => DataBoundControl.GetDefaultImplementation(textBox).SetDataBinding(new TestObject(), "XXX_DataMember"));
		}

		public void TestReadOnlyMetaDataTrueWhenControlNotYetVisible()
		{
			KTextBox textBox = new KTextBox();
			Form.Controls.Add(textBox);

			DataBoundControl.GetDefaultImplementation(textBox).SetDataBinding(new TestObject(), "Value");
			AssertEquals(
				"ReadOnly true initially when the control has not been made visible. This is important when there is a master/detail situation otherwise ReadOnly never gets updated to true when there is no master record.",
				true, textBox.ReadOnly);

			Form.Show();
			AssertEquals(
				"ReadOnly set to false once the form is shown.",
				false, textBox.ReadOnly);
		}

		public void TestDataSourceUpdateModeNeverForMetaData()
		{
			KTextBox textBox = new KTextBox();
			Form.Controls.Add(textBox);
			DataBoundControl.Get(textBox).SetDataBinding(new TestObject(), "Value");
			AssertEquals("ReadOnly meta-data should never update the data source.", DataSourceUpdateMode.Never, textBox.DataBindings["ReadOnlyForBinding"].DataSourceUpdateMode);
		}

		public void TestSetDataBindingForMetadataPropertiesBindsMetadata()
		{
			var textBox = new TestTextBox();
			var testTypeDescriptorProvider = new TestDescriptionProvider();
			TypeDescriptor.AddProvider(testTypeDescriptorProvider, typeof(TestTextBox));
			Form.Controls.Add(textBox);

			DataBoundControl.SetDataBindingForMetadataProperties(textBox, new TestObject(), "Value");

			AssertNotNull(textBox.DataBindings["ReadOnly"]);
		}

		public void TestBreakingBadTypeDescriptorWillThrowAdditionalExceptionInfo()
		{
			var textBox = new TestTextBox();
			var badTypeDescriptorProvider = new TestDescriptionProvider();
			TypeDescriptor.AddProvider(badTypeDescriptorProvider, typeof(TestTextBox));
			Form.Controls.Add(textBox);
			DataBoundControl.SetDataBindingForMetadataProperties(textBox, new TestObject(), "Value");
			badTypeDescriptorProvider.ReturnNullPropertyDescriptor = true;

			var exception = AssertExceptionThrown<KDataBindingException>(
				() => DataBoundControl.SetDataBindingForMetadataProperties(textBox, new TestObject(), "Value"));

			var controlTypeDescriptorProviderType = exception.InnerException.Data["ControlTypeDescriptorProviderType"]?.ToString() ?? string.Empty;
			Assert(controlTypeDescriptorProviderType.Contains(nameof(TestDescriptionProvider)));
			var controlPropertyDescriptorCollection = exception.InnerException.Data["ControlPropertyDescriptorCollection"]?.ToString() ?? string.Empty;
			AssertEquals(
				"{Text Type=String Descriptor=ReflectPropertyDescriptor, null, ReadOnly Type=Boolean Descriptor=ReflectPropertyDescriptor, MaxLength Type=Int32 Descriptor=ReflectPropertyDescriptor}",
				controlPropertyDescriptorCollection);
		}

		public void TestBreakingBadNullReturnTypeDescriptorWillThrowAdditionalExceptionInfo()
		{
			var textBox = new TestTextBox();
			var badTypeDescriptorProvider = new TestDescriptionProvider();
			TypeDescriptor.AddProvider(badTypeDescriptorProvider, typeof(TestTextBox));
			Form.Controls.Add(textBox);
			DataBoundControl.SetDataBindingForMetadataProperties(textBox, new TestObject(), "Value");
			badTypeDescriptorProvider.ReturnNullTypeDescriptor = true;

			var exception = AssertExceptionThrown<KDataBindingException>(
				() => DataBoundControl.SetDataBindingForMetadataProperties(textBox, new TestObject(), "Value"));

			var controlTypeDescriptorProviderType = exception.InnerException.Data["ControlTypeDescriptorProviderType"]?.ToString() ?? string.Empty;
			Assert(controlTypeDescriptorProviderType.Contains(nameof(TestDescriptionProvider)));
			Assert(controlTypeDescriptorProviderType.Contains("ReflectTypeDescriptionProvider"));
			Assert(!controlTypeDescriptorProviderType.Contains("TypeDescriptionNode"));
			Assert(!controlTypeDescriptorProviderType.Contains("DelegatingTypeDescriptionProvider"));
			var controlPropertyDescriptorCollection = exception.InnerException.Data["ControlPropertyDescriptorCollection"]?.ToString() ?? string.Empty;
			Assert(controlPropertyDescriptorCollection.Contains("Could not retrieve property descriptors"));

			// This will stop recent versions of dotnet from throwing an exception during cleanup
			badTypeDescriptorProvider.ReturnNullTypeDescriptor = false;
		}

		#region Test Classes

		class TestObject : KComponent
		{
			[ReadOnlyMember(nameof(Value_ReadOnly))]
			public string Value { get; set; }

			public bool Value_ReadOnly { get; set; }
		}

		[DefaultBindingProperty("Text")]
		class TestTextBox : TextBox
		{
			[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
			[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnly")]
			public override string Text { get; set; }
		}

		class TestDescriptionProvider : TypeDescriptionProvider
		{
			public bool ReturnNullPropertyDescriptor { get; set; }
			public bool ReturnNullTypeDescriptor { get; set; }

			public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
				=> ReturnNullTypeDescriptor ? null : new TestTypeDescriptor(objectType, this);
		}

		class TestTypeDescriptor : CustomTypeDescriptor
		{
			readonly Type objectType;
			readonly TestDescriptionProvider parentProvider;

			public TestTypeDescriptor(Type objectType, TestDescriptionProvider parentProvider)
				: base(TypeDescriptor.GetProvider(typeof(Control)).GetTypeDescriptor(objectType))
			{
				this.objectType = objectType;
				this.parentProvider = parentProvider;
			}

			public override PropertyDescriptorCollection GetProperties()
			{
				var props = new List<PropertyDescriptor>
					{
						NewPropertyDescriptor<string>("Text"),
						NewPropertyDescriptor<bool>("ReadOnly"),
						NewPropertyDescriptor<int>("MaxLength")
					};

				if (parentProvider.ReturnNullPropertyDescriptor)
				{
					props.Insert(1, null);
				}

				return new PropertyDescriptorCollection(props.ToArray());
			}

			PropertyDescriptor NewPropertyDescriptor<T>(string name)
				=> TypeDescriptor.CreateProperty(objectType, name, typeof(T), null);
		}

		#endregion

		#region Implementation

		KForm Form
		{
			get { return form ?? (form = new KForm()); }
		}
		KForm form;

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
