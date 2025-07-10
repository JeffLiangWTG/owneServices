using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class FieldInvalidTextMemoryTest : TestCase
	{
		public void TestGetSetInvalidText()
		{
			var obj = new object();
			AssertEquals("Default", "", FieldInvalidTextMemory.GetInvalidText(obj, "Property1"));
			AssertEquals("Default", "", FieldInvalidTextMemory.GetInvalidText(obj, "Property2"));

			FieldInvalidTextMemory.SetInvalidText(obj, "Property1", "Text");
			FieldInvalidTextMemory.SetInvalidText(obj, "Property2", "Text2");
			AssertEquals("Text", FieldInvalidTextMemory.GetInvalidText(obj, "Property1"));
			AssertEquals("Text2", FieldInvalidTextMemory.GetInvalidText(obj, "Property2"));

			FieldInvalidTextMemory.SetInvalidText(obj, "Property1", null);
			AssertEquals("", FieldInvalidTextMemory.GetInvalidText(obj, "Property1"));
			AssertEquals("Text2", FieldInvalidTextMemory.GetInvalidText(obj, "Property2"));

			FieldInvalidTextMemory.SetInvalidText(obj, "Property2", null);
			AssertEquals("", FieldInvalidTextMemory.GetInvalidText(obj, "Property1"));
			AssertEquals("", FieldInvalidTextMemory.GetInvalidText(obj, "Property2"));

			// expect no exception
			FieldInvalidTextMemory.SetInvalidText(obj, "Property2", null);
		}

		public void TestObjectsWeakReferenced()
		{
			var objRef = DummyHelper();
			GC.Collect();
			AssertEquals("Object collected", false, objRef.IsAlive);
		}

		public WeakReference DummyHelper()
		{
			var obj = new object();
			var objRef = new WeakReference(obj);
			FieldInvalidTextMemory.SetInvalidText(obj, "Property", "Text");
			return objRef;
		}
	}
}
