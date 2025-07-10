#if DEBUG
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Common.Collections;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class KReflectPropertyDescriptorTests : TestCase
	{
		public void TestGetValue()
		{
			testObject.Property1 = "HI!";
			AssertEquals("GetValue 1", "HI!", propertyDesc1.GetValue(testObject));

			testChildObject.Property1 = "HI KID!";
			AssertEquals("GetValue 1a", "HI KID!", propertyDescChild1.GetValue(testChildObject));
		}

		public void TestGetValueWithExceptionDataAndStackTrace()
		{
			Exception exceptionExpected = null;
			try
			{
				var val = propertyDesc4.GetValue(testObject);
				Fail();
			}
			catch (Exception ex)
			{
				exceptionExpected = ex;
			}

			CombineAssertions(() =>
			{
				AssertType<NullReferenceException>(exceptionExpected);
				AssertEquals("Object reference not set to an instance of an object.", exceptionExpected.Message);
				AssertContains("someBadInnerMethod", exceptionExpected.StackTrace);
				AssertEquals("CargoWise.ComponentModel.Testing.KReflectPropertyDescriptorTests+TestObject", exceptionExpected.Data["KReflectPropertyDescriptor.Component"]);
				AssertEquals("Property4", exceptionExpected.Data["KReflectPropertyDescriptor.Name"]);
			});
		}

		public void TestSetValue()
		{
			propertyDesc1.SetValue(testObject, "HELLO");
			AssertEquals("SetValue 1", "HELLO", testObject.Property1);

			propertyDescChild1.SetValue(testChildObject, "HELLO KID");
			AssertEquals("SetValue 1a", "HELLO KID", testChildObject.Property1);
		}

		public void TestSetValueOnReadOnlyProperty()
		{
			propertyDesc2.SetValue(testObject, "MEH MEH");
			AssertEquals("Should not be setting property if read-only", testObject.GetType().ToString(), testObject.Property2);

			propertyDescChild3.SetValue(testChildObject, 23);
			//AssertEquals("Should not be setting property if read-only", 0, testChildObject.Property3);
			AssertEquals("Should be settable as it has setter", 23, testChildObject.Property3);
		}

		public void TestReadOnly()
		{
			Assert("!IsReadOnly", !propertyDesc1.IsReadOnly);
			Assert("IsReadOnly", propertyDesc2.IsReadOnly);
		}

		public void TestGetValueWithGenerics()
		{
			genericObject.Property1 = 12;
			AssertEquals(12, propertyGeneric1.GetValue(genericObject));

			genericChild.Property1 = "AU...";
			AssertEquals("AU...", propertyGenericChild1.GetValue(genericChild));
		}

		public void TestRespectAttributeOverrides()
		{
			ReadOnlyAttribute attribute;

			attribute = (ReadOnlyAttribute)propertyDesc3.Attributes[typeof(ReadOnlyAttribute)];
			AssertEquals(false, attribute.IsReadOnly);

			attribute = (ReadOnlyAttribute)propertyDescChild3.Attributes[typeof(ReadOnlyAttribute)];
			AssertEquals(true, attribute.IsReadOnly);
		}

		public void TestAttributesDontLeak()
		{
			var attrRef = CreateUnreferencedPropertyAttribute();
			Collect();
			AssertEquals("Attribute should be collected", null, attrRef.Target);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference CreateUnreferencedPropertyAttribute()
		{
			var property = new KReflectPropertyDescriptor(typeof(TestObject), typeof(TestObject).GetProperty("Property1"));
			var attr = (BindableAttribute)property.Attributes[typeof(BindableAttribute)];
			return new WeakReference(attr);
		}

		public void TestUnboxValueTypeComponents()
		{
			TestValueType valueComponent = new TestValueType();
			valueComponent.Length = 1023091;
			KReflectPropertyDescriptor property = new KReflectPropertyDescriptor(typeof(TestValueType), typeof(TestValueType).GetProperty("Length"));
			AssertEquals(1023091, property.GetValue(valueComponent));
		}

		#region Test Classes

		public struct TestValueType
		{
			public int Length
			{
				get;
				set;
			}
		}

		public class TestObject
		{
			[Bindable(BindableSupport.No)]
			public string Property1
			{
				get { return property1; }
				set { property1 = value; }
			}
			string property1;

			public string Property2
			{
				get { return GetType().ToString(); }
			}

			[ReadOnly(false)]
			public virtual int Property3
			{
				set { property3 = value; }
				get { return 0; }
			}
			protected int property3;

			public string Property4 => GenerateProperty4();

			static string GenerateProperty4()
			{
				string someBadInnerMethod()
				{
					return string.Join(",", new[] { "1", null, "3" }.Where(x => x.Length > 0));
				}

				return someBadInnerMethod();
			}
		}

		public class TestChildObject : TestObject
		{
			[ReadOnly(true)]
			public override int Property3
			{
				get { return property3; }
			}
		}

		public class TestGeneric<T>
		{
			public virtual T Property1
			{
				get { return property1; }
				set { property1 = value; }
			}

			T property1;
		}

		public class TestChildOfGeneric : TestGeneric<string>
		{
		}

		#endregion

		#region Implementation

		readonly TestObject testObject = new TestObject();
		PropertyDescriptor propertyDesc1;
		PropertyDescriptor propertyDesc2;
		PropertyDescriptor propertyDesc3;
		PropertyDescriptor propertyDesc4;

		readonly TestObject testChildObject = new TestChildObject();
		PropertyDescriptor propertyDescChild1;
		PropertyDescriptor propertyDescChild3;

		readonly TestGeneric<int> genericObject = new TestGeneric<int>();
		PropertyDescriptor propertyGeneric1;

		readonly TestChildOfGeneric genericChild = new TestChildOfGeneric();
		PropertyDescriptor propertyGenericChild1;

		protected override void SetUp()
		{
			base.SetUp();

			propertyDesc1 = new KReflectPropertyDescriptor(typeof(TestObject), typeof(TestObject).GetProperty("Property1"));
			propertyDesc2 = new KReflectPropertyDescriptor(typeof(TestObject), typeof(TestObject).GetProperty("Property2"));
			propertyDesc3 = new KReflectPropertyDescriptor(typeof(TestObject), typeof(TestObject).GetProperty("Property3"));
			propertyDesc4 = new KReflectPropertyDescriptor(typeof(TestObject), typeof(TestObject).GetProperty("Property4"));

			propertyDescChild1 = new KReflectPropertyDescriptor(typeof(TestChildObject), typeof(TestChildObject).GetProperty("Property1"));
			propertyDescChild3 = new KReflectPropertyDescriptor(typeof(TestChildObject), typeof(TestChildObject).GetProperty("Property3"));

			propertyGeneric1 = new KReflectPropertyDescriptor(genericObject.GetType(), genericObject.GetType().GetProperty("Property1"));
			propertyGenericChild1 = new KReflectPropertyDescriptor(genericChild.GetType(), genericChild.GetType().GetProperty("Property1"));
		}

		static void Collect()
		{
			foreach (ILRUCache cache in LRUCache.AllInstances)
			{
				cache.Clear();
			}
			GC.Collect();
		}

		#endregion
	}
}
#endif
