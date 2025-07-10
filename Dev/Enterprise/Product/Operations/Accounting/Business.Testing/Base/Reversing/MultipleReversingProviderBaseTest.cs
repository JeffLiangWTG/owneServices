using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Reversing;
using NUnit.Framework.TestHelper;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class MultipleReversingProviderBaseTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIEnumerator()
		{
			var bizObjects = GetBusinessObjectsForIEnumeratorTesting();
			TestObject.BizObjectsForReversing.AddRange(bizObjects);
			AssertEquals(bizObjects[0], GetCurrentBusinessObjectForIEnumeratorTesting());

			int i = 0;
			foreach (var bizo in TestObject)
			{
				AssertEquals("IEnumerator must sequentially return elements from TransactionsForReversing collection.", bizObjects[i], bizo);
				i++;
			}
		}

		protected abstract BusinessObject[] GetBusinessObjectsForIEnumeratorTesting();

		protected abstract BusinessObject GetCurrentBusinessObjectForIEnumeratorTesting();

		protected MultipleReversingProviderBase TestObject
		{
			get { return testObject ?? (testObject = GetNewTestObject()); }
		}

		MultipleReversingProviderBase testObject;

		protected Type GetExpectedBusinessObjectTypeCore() => TestedTypeHelper.GetTestedType(GetType());

		protected abstract MultipleReversingProviderBase GetNewTestObject();
	}
}
