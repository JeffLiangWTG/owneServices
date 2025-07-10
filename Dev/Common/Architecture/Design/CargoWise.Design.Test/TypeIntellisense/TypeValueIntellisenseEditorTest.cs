using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Design.Testing
{
	class TypeValueIntellisenseEditorTest : TestCase
	{
		#region TestCheckAppliedCorrectly
		[ExpectNoExceptions]
		public void TestCheckAppliedCorrectly()
		{
			TypeValueIntellisenseEditor.CheckAppliedCorrectly(TypeDescriptor.GetProperties(typeof(TestInstance1))["Type"]);
			TypeValueIntellisenseEditor.CheckAppliedCorrectly(TypeDescriptor.GetProperties(typeof(TestInstance2))["Type"]);
			TypeValueIntellisenseEditor.CheckAppliedCorrectly(TypeDescriptor.GetProperties(typeof(TestInstance3))["Type"]);
			try
			{
				TypeValueIntellisenseEditor.CheckAppliedCorrectly(null);
				Fail("Expected exception due to PropertyDescriptor being null");
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				TypeValueIntellisenseEditor.CheckAppliedCorrectly(TypeDescriptor.GetProperties(typeof(TestInstanceNoSerialiser))["Type"]);
				Fail("Expected exception due to " + nameof(TypeFixCodeDomSerializer) + " not being applied to instance");
			}
			catch (ArgumentException)
			{
			}

			try
			{
				TypeValueIntellisenseEditor.CheckAppliedCorrectly(TypeDescriptor.GetProperties(typeof(TestInstanceNoTypeConverter))["Type"]);
				Fail("Expected exception due to " + nameof(TypeTypeConverter) + " not being applied to property");
			}
			catch (ArgumentException)
			{
			}

			try
			{
				TypeValueIntellisenseEditor.CheckAppliedCorrectly(TypeDescriptor.GetProperties(typeof(TestInstanceWrongPropertyType))["Type"]);
				Fail("Expected exception due to PropertyType wrong");
			}
			catch (ArgumentException)
			{
			}
		}

		[DesignerSerializer(typeof(MyCodeDomSerialiser), typeof(CodeDomSerializer))]
		public class TestInstance1 : Component
		{
			internal class MyCodeDomSerialiser : TypeFixCodeDomSerializer
			{
				public MyCodeDomSerialiser() : base(typeof(TestInstance1))
				{
				}
			}

			[Editor(typeof(TypeValueIntellisenseEditor), typeof(UITypeEditor))]
			[TypeConverter(typeof(TypeTypeConverter))]
			public Type Type
			{
				get
				{
					return null;
				}
			}
		}

		[DesignerSerializer(typeof(MyCodeDomSerialiser), typeof(CodeDomSerializer))]
		public class TestInstance2 : Component
		{
			internal class MyCodeDomSerialiser : TypeFixCodeDomSerializer
			{
				public MyCodeDomSerialiser() : base(typeof(TestInstance2))
				{
				}
			}

			[Editor(typeof(TypeValueIntellisenseEditor), typeof(UITypeEditor))]
			[TypeConverter(typeof(TypeTypeConverter))]
			public Type Type
			{
				get
				{
					return null;
				}
			}
		}

		public class TestInstance3 : TestInstance2
		{
		}

		public class TestInstanceNoSerialiser : Component
		{
			[Editor(typeof(TypeValueIntellisenseEditor), typeof(UITypeEditor))]
			[TypeConverter(typeof(TypeTypeConverter))]
			public Type Type
			{
				get
				{
					return null;
				}
			}
		}

		[DesignerSerializer(typeof(MyCodeDomSerialiser), typeof(CodeDomSerializer))]
		public class TestInstanceNoTypeConverter : Component
		{
			internal class MyCodeDomSerialiser : TypeFixCodeDomSerializer
			{
				public MyCodeDomSerialiser() : base(typeof(TestInstanceNoTypeConverter))
				{
				}
			}

			[Editor(typeof(TypeValueIntellisenseEditor), typeof(UITypeEditor))]
			public Type Type
			{
				get
				{
					return null;
				}
			}
		}

		public class TestInstanceWrongPropertyType
		{
			[Editor(typeof(TypeValueIntellisenseEditor), typeof(UITypeEditor))]
			[TypeConverter(typeof(TypeTypeConverter))]
			public int Type
			{
				get
				{
					return 0;
				}
			}
		}
		#endregion
	}
}
