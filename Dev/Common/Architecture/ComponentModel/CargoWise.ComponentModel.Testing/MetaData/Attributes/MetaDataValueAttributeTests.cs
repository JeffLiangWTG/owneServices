#if DEBUG
using System;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class MetaDataValueAttributeTests : TestCase
	{
		public void TestStaticFieldConstant()
		{
			MetaDataValueAttribute attr = (MetaDataValueAttribute)TypeDescriptor.GetAttributes(typeof(TestStaticField))[typeof(MetaDataValueAttribute)];
			AssertEquals(1, attr.Value);
		}

		public void TestStaticPropertyConstant()
		{
			MetaDataValueAttribute attr = (MetaDataValueAttribute)TypeDescriptor.GetAttributes(typeof(TestStaticProperty))[typeof(MetaDataValueAttribute)];
			AssertEquals(2, attr.Value);
		}

		public void TestNonExistantStaticFieldConstant()
		{
			MetaDataValueAttribute attr = (MetaDataValueAttribute)TypeDescriptor.GetAttributes(typeof(TestNonExistantStaticField))[typeof(MetaDataValueAttribute)];
			try
			{
				object x = attr.Value;
				Fail("Should have thrown an exception");
			}
			catch (InvalidOperationException)
			{
				Assert(true);
			}
		}

		[MetaDataValue("", typeof(TestStaticField), "Value")]
		internal class TestStaticField
		{
			public static readonly int Value = 1;
		}

		[MetaDataValue("", typeof(TestStaticProperty), "Value")]
		internal class TestStaticProperty
		{
			public static int Value
			{
				get { return 2; }
			}
		}

		[MetaDataValue("", typeof(TestNonExistantStaticField), "NonExistantMember")]
		internal class TestNonExistantStaticField
		{
		}
	}
}
#endif
