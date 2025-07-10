using System;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules
{
	sealed class ZModuleResultsTest : TestCase
	{
		public void TestInstance()
		{
			AssertNotNull("Instance created", ZModuleResults.Instance);
			AssertEquals("Instance constant", ZModuleResults.Instance, ZModuleResults.Instance);
		}

		public void TestGetPKCollectionForModule()
		{
			var weakRef = GetWeakReferenceToPKList();
			GC.Collect();
			Assert("Should be GCed", !weakRef.IsAlive);
		}

		WeakReference GetWeakReferenceToPKList()
		{
			var pKList = ZModuleResults.Instance.GetPKCollectionForModule(DummyModuleIDs.Dummy);
			AssertNotNull("Never null", pKList);

			var weakRef = new WeakReference(pKList);
			pKList = null;

			return weakRef;
		}
	}
}
