using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DefaultFieldAttributeTest : TestCaseWithFactory
	{
		public void TestGetDefaultFieldName()
		{
			AssertEquals(null, DefaultFieldAttribute.GetDefaultFieldName(typeof(GenericWithNoAttribute)));
			AssertEquals("ValidValue", DefaultFieldAttribute.GetDefaultFieldName(typeof(GenericWithValidAttribute)));
			AssertEquals("ValidValue", DefaultFieldAttribute.GetDefaultFieldName(typeof(GenericWithValidAttributeOnParent)));
			AssertEquals("AnotherValidValue", DefaultFieldAttribute.GetDefaultFieldName(typeof(GenericWithOverriddenAttribute)));
			AssertEquals("InValidValue", DefaultFieldAttribute.GetDefaultFieldName(typeof(GenericWithInValidAttribute)));
		}

		public void TestGetDefaultValue()
		{
			GenericWithNoAttribute genericWithNoAttribute = new GenericWithNoAttribute();
			AssertEquals(string.Format(DefaultFieldAttribute.DefaultFieldNotImplementedMessage, genericWithNoAttribute.HumanReadableName), DefaultFieldAttribute.GetDefaultValue(genericWithNoAttribute));
			AssertEquals("ValidValue", DefaultFieldAttribute.GetDefaultValue(new GenericWithValidAttribute()));
			AssertEquals("ValidValueOverride", DefaultFieldAttribute.GetDefaultValue(new GenericWithValidAttributeOnParent()));
			AssertEquals("AnotherValidValue", DefaultFieldAttribute.GetDefaultValue(new GenericWithOverriddenAttribute()));
			AssertExceptionThrown(typeof(MissingFieldException), delegate
			{ DefaultFieldAttribute.GetDefaultValue(new GenericWithInValidAttribute()); });
		}

		#region Test Class Definitions
		class GenericWithNoAttribute : DocumentWrapper
		{
			public GenericWithNoAttribute() : base(null, null) { }
		}

		[DefaultField("ValidValue")]
		class GenericWithValidAttribute : GenericWithNoAttribute
		{
			public virtual ZString ValidValue
			{
				get { return "ValidValue"; }
			}
		}

		class GenericWithValidAttributeOnParent : GenericWithValidAttribute
		{
			public override ZString ValidValue
			{
				get { return "ValidValueOverride"; }
			}
		}

		[DefaultField("AnotherValidValue")]
		class GenericWithOverriddenAttribute : GenericWithValidAttribute
		{
			public ZString AnotherValidValue
			{
				get { return "AnotherValidValue"; }
			}
		}

		[DefaultField("InValidValue")]
		class GenericWithInValidAttribute : GenericWithValidAttribute
		{
		}
		#endregion
	}
}
