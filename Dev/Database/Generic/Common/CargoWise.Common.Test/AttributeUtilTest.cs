using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Common
{
	internal class AttributeUtilTest : TestCase
	{
		public void TestFindAttributesOnProperty()
		{
			PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(ChildObj))["SomeProp"];
			IEnumerable<Attribute> attrList = AttributeUtil.FindAttributes(prop.ComponentType, prop.Name, prop.PropertyType, typeof(TestAttribute), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			TestAttribute[] attrs = (TestAttribute[])new ArrayList(new List<Attribute>(attrList)).ToArray(typeof(TestAttribute));
			AssertEquals("Should be 3 TestAttribute attributes on property", 3, attrs.Length);
			AssertEquals("Attribute on base should be specified last", "attr_on_base_prop", attrs[2].Value);
			SortAttributeArray(attrs);
			AssertEquals("attr_on_base_prop", attrs[0].Value);
			AssertEquals("attr_on_child_prop1", attrs[1].Value);
			AssertEquals("attr_on_child_prop2", attrs[2].Value);
			IEnumerable<Attribute> no_attrs = AttributeUtil.FindAttributes(typeof(string), "Length", typeof(int), typeof(DescriptionAttribute), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals("Property with no attributes", false, no_attrs.GetEnumerator().MoveNext());
		}

		public void TestFindAttributesOnClass()
		{
			List<Attribute> list = new List<Attribute>(AttributeUtil.FindAttributes(typeof(ChildObj), typeof(TestAttribute)));
			TestAttribute[] attrs = (TestAttribute[])new ArrayList(list).ToArray(typeof(TestAttribute));
			AssertEquals("Attribute on base should be specified last", "attr_on_base", attrs[2].Value);
			SortAttributeArray(attrs);
			AssertEquals("Should be 3 TestAttribute attributes", 3, attrs.Length);
			AssertEquals("attr_on_base", attrs[0].Value);
			AssertEquals("attr_on_child1", attrs[1].Value);
			AssertEquals("attr_on_child2", attrs[2].Value);
			Attribute[] no_attrs = new List<Attribute>(AttributeUtil.FindAttributes(typeof(string), typeof(DescriptionAttribute))).ToArray();
			AssertEquals("Type with no attributes", 0, no_attrs.Length);
		}

		void SortAttributeArray(TestAttribute[] attrs)
		{
			string[] values = new string[attrs.Length];
			for (int i = 0; i < attrs.Length; i++)
			{
				values[i] = attrs[i].Value;
			}

			Array.Sort(values, attrs);
		}

		[Description("base_desc")]
		[Test("attr_on_base")]
		public class ParentObj
		{
			[Description("desc_on_base_prop")]
			[Test("attr_on_base_prop")]
			public virtual int SomeProp
			{
				get
				{
					return 0;
				}
			}
		}

		[Description("child_desc")]
		[Test("attr_on_child1")]
		[Test("attr_on_child2")]
		public class ChildObj : ParentObj
		{
			[Description("desc_on_child_prop")]
			[Test("attr_on_child_prop1")]
			[Test("attr_on_child_prop2")]
			public override int SomeProp
			{
				get
				{
					return 0;
				}
			}
		}

		[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
		public sealed class TestAttribute : Attribute
		{
			public TestAttribute(string value)
			{
				this.Value = value;
			}

			public readonly string Value;
		}
	}
}