#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class BindableComponentMetaDataPropertyLocatorTests : TestCase
	{
		public void TestGetDefaultMetaDataProperty()
		{
			AssertEquals("NullValue", BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(typeof(TestControlBase), MetaDataTypes.Null).Name);
		}

		public void TestDefaultBindingProperty()
		{
			AssertEquals("Text", MetaDataPropertyLocator.DefaultBindingProperty.Name);
		}

		public void TestGetMetaDataProperties()
		{
			IDictionary<string, PropertyDescriptor> metaDataProperties = MetaDataPropertyLocator.GetControlMetaDataProperties(MetaDataPropertyLocator.DefaultBindingProperty);
			AssertEquals(2, metaDataProperties.Count);
			AssertEquals("MaxLength", metaDataProperties[MetaDataTypes.MaxLength].Name);
			AssertEquals("DecimalPlaces", metaDataProperties[MetaDataTypes.DecimalPlaces].Name);
		}

		public void TestGetFormattingType()
		{
			AssertEquals(BindingOptionsAttribute.Default, MetaDataPropertyLocator.GetBindingOptions(TypeDescriptor.GetProperties(typeof(TestControl))["Text"]));
			AssertEquals(true, MetaDataPropertyLocator.GetBindingOptions(TypeDescriptor.GetProperties(typeof(TestControl))["PropertyWithFormattingEnabled"]).FormattingEnabled);
		}

		#region Test Classes

		[DefaultBindingProperty("Text")]
		class TestControlBase
		{
			[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
			[BindingMetaDataProperty(MetaDataTypes.DecimalPlaces, "DecimalPlaces")]
			[BindingMetaDataProperty(MetaDataTypes.Null, "NullValue")]
			public virtual string Text { get; set; }

			public int DecimalPlaces { get; set; }
			public int MaxLength { get; set; }
			public string NullValue { get; set; }
		}

		class TestControl : TestControlBase
		{
			[BindingMetaDataProperty(MetaDataTypes.Null, "NullValue", Enabled = false)]
			public override string Text { get; set; }

			[BindingOptions(FormattingEnabled = true)]
			public string PropertyWithFormattingEnabled { get; set; }
		}

		#endregion

		#region Implementation

		BindableComponentMetaDataPropertyLocator MetaDataPropertyLocator
		{
			get
			{
				if (metaDataPropertyLocator == null)
				{
					metaDataPropertyLocator = BindableComponentMetaDataPropertyLocator.GetInstance(typeof(TestControl));
				}
				return metaDataPropertyLocator;
			}
		}
		BindableComponentMetaDataPropertyLocator metaDataPropertyLocator;

		#endregion
	}
}
#endif
