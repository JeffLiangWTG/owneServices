using System;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Application.Testing
{
	class ObjectHandleTest : TestCase
	{
		public void TestConstructorInternal()
		{
			ConstructorInfo constructor = typeof(ObjectHandle).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
			AssertEquals("Constructor taking objectName cannot be public, otherwise developers can get around interface security", false, constructor.IsFamily);
			AssertEquals("Constructor taking objectName cannot be public, otherwise developers can get around interface security", false, constructor.IsPublic);
		}
	}

	public class TestObjectHandle : ObjectHandle
	{
		public TestObjectHandle(object obj)
		{
			this.obj = obj;
		}

		public override object GetObject()
		{
			return obj;
		}

		public override Type GetObjectType()
		{
			return obj.GetType();
		}

		public override object GetObject(params object[] arguments)
		{
			return obj;
		}

		readonly object obj;
	}

	public interface ITestDynamicObjectHandleSupporter
	{
		object GetObject();
		Type GetObjectType();
		object GetObject(object[] arguments);
	}

	public class TestDynamicObjectHandle : ObjectHandle
	{
		public TestDynamicObjectHandle(ITestDynamicObjectHandleSupporter obj)
		{
			this.obj = obj;
		}

		public override object GetObject()
		{
			return supporter.GetObject();
		}

		public override Type GetObjectType()
		{
			return supporter.GetObjectType();
		}

		public override object GetObject(params object[] arguments)
		{
			return supporter.GetObject(arguments);
		}

		ITestDynamicObjectHandleSupporter supporter => (ITestDynamicObjectHandleSupporter)obj;
		readonly object obj; // Has to use object as CodeAnalysis complain about: Field:CargoWise.Application.TestDynamicObjectHandle.supporter - All fields of an [Immutable] type must be readonly immutable types
	}
}
