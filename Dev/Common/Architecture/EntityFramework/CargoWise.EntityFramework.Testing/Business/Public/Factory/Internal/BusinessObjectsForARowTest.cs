using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectsForARowTest : TestCaseWithDummy
	{
		public void TestLaxTypeCheck()
		{
			var dummySubclass1 = new DummyLaxSubclass1BusinessObject(Factory, Dummy.Row);
			var bO4Row = new BusinessObjectsForARow(dummySubclass1);
			AssertEquals(dummySubclass1, bO4Row[typeof(DummyLaxSubclass1BusinessObject)]);
			AssertEquals(dummySubclass1, bO4Row[typeof(DummyLaxBusinessObject)]);
			AssertEquals(null, bO4Row[typeof(DummyLaxSubclass2BusinessObject)]);
			AssertEquals(null, bO4Row[typeof(DummyBaseBusinessObject)]);
			AssertEquals(null, bO4Row[typeof(DummyBusinessObject)]);

			var dummySubclass2 = new DummyLaxSubclass2BusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummySubclass2);
			AssertEquals(dummySubclass1, bO4Row[typeof(DummyLaxSubclass1BusinessObject)]);
			AssertEquals(dummySubclass1, bO4Row[typeof(DummyLaxBusinessObject)]);
			AssertEquals(dummySubclass2, bO4Row[typeof(DummyLaxSubclass2BusinessObject)]);
			AssertEquals(null, bO4Row[typeof(DummyBaseBusinessObject)]);
			AssertEquals(null, bO4Row[typeof(DummyBusinessObject)]);

			var dummyBaseClass = new DummyLaxBusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummyBaseClass);
			AssertEquals(dummySubclass1, bO4Row[typeof(DummyLaxSubclass1BusinessObject)]);
			AssertEquals(dummyBaseClass, bO4Row[typeof(DummyLaxBusinessObject)]);
			AssertEquals(dummySubclass2, bO4Row[typeof(DummyLaxSubclass2BusinessObject)]);
			AssertEquals(null, bO4Row[typeof(DummyBaseBusinessObject)]);
			AssertEquals(null, bO4Row[typeof(DummyBusinessObject)]);
		}

		[IDontMindLoadingASubclassInstead]
		internal class DummyLaxBusinessObject : DummyBaseBusinessObject
		{
			public DummyLaxBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		internal class DummyLaxSubclass1BusinessObject : DummyLaxBusinessObject
		{
			public DummyLaxSubclass1BusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		internal class DummyLaxSubclass2BusinessObject : DummyLaxBusinessObject
		{
			public DummyLaxSubclass2BusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		public void TestHandleDeleted()
		{
			DummyBusinessObjectCollection collection1 = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObjectCollection collection2 = new DummyBusinessObjectCollection(Factory);

			BusinessObjectsForARow bO4Row = new BusinessObjectsForARow(Dummy);
			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummyBase);

			collection1.Add(Dummy);
			collection2.Add(dummyBase);

			bO4Row.HandleDeleted(Dummy);

			AssertEquals("Dummy should be removed from Collection1 when deleted", 0, collection1.Count);
			AssertEquals("DummyBase should be removed from Collection2 when Dummy was deleted", 0, collection2.Count);
		}

		public void TestSetHasChanges()
		{
			BusinessObjectsForARow bO4Row = new BusinessObjectsForARow(Dummy);
			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummyBase);

			bO4Row.SetHasChanges(true);
			Assert("HasChanges", Dummy.HasChanges);
			Assert("HasChanges", dummyBase.HasChanges);

			bO4Row.SetHasChanges(false);
			Assert("HasChanges", !Dummy.HasChanges);
			Assert("HasChanges", !dummyBase.HasChanges);

			Dummy.HasChanges = true;
			Assert("HasChanges", Dummy.HasChanges);
			Assert("HasChanges", dummyBase.HasChanges);

			dummyBase.HasChanges = false;
			Assert("HasChanges", !Dummy.HasChanges);
			Assert("HasChanges", !dummyBase.HasChanges);

			bO4Row.SetHasChanges(false);
			Assert("HasChanges", !Dummy.HasChanges);
			Assert("HasChanges", !dummyBase.HasChanges);
		}

		public void TestAddAndIndexer()
		{
			BusinessObjectsForARow bO4Row = new BusinessObjectsForARow(Dummy);
			AssertEquals("Dummy", bO4Row[typeof(DummyBusinessObject)], Dummy);

			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummyBase);

			AssertEquals("Dummy", bO4Row[typeof(DummyBusinessObject)], Dummy);
			AssertEquals("DummyBase", bO4Row[typeof(DummyBaseBusinessObject)], dummyBase);
		}

		public void TestIndexerReturnsNullForUnknownType()
		{
			BusinessObjectsForARow bO4Row = new BusinessObjectsForARow(Dummy);
			AssertEquals("Dummy", bO4Row[typeof(DummyBusinessObject)], Dummy);

			AssertNull("No entry for this type", bO4Row[typeof(DummyBaseBusinessObject)]);
		}

		public void TestIsSavedByFactory()
		{
			BusinessObjectsForARow bO4Row = new BusinessObjectsForARow(Dummy);

			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummyBase);

			Dummy.OverrideIsSavedByFactory = true;
			dummyBase.OverrideIsSavedByFactory = true;

			Dummy.IsSavedByFactoryOverride = true;
			dummyBase.IsSavedByFactoryOverride = true;
			AssertEquals("Should be saved", true, bO4Row.IsSavedByFactory);

			Dummy.IsSavedByFactoryOverride = false;
			dummyBase.IsSavedByFactoryOverride = false;
			AssertEquals("Should be saved", false, bO4Row.IsSavedByFactory);
		}

		public void TestHasChangesInAuditDetails()
		{
			BusinessObjectsForARow bizos = new BusinessObjectsForARow(Dummy);

			DummyBaseBusinessObject otherDummy = new DummyBaseBusinessObject(Factory, Dummy.Row);
			bizos.Add(otherDummy);

			Dummy.OverrideHasChangesInAuditDetails = true;
			otherDummy.OverrideHasChangesInAuditDetails = true;

			Dummy.HasChangesInAuditDetailsOverride = true;
			otherDummy.HasChangesInAuditDetailsOverride = true;
			AssertEquals("Should be saved by factory", true, bizos.HasChangesInAuditDetailsForTests);

			Dummy.HasChangesInAuditDetailsOverride = false;
			otherDummy.HasChangesInAuditDetailsOverride = false;
			AssertEquals("Should be saved by factory", false, bizos.HasChangesInAuditDetailsForTests);

			Dummy.HasChangesInAuditDetailsOverride = true;
			otherDummy.HasChangesInAuditDetailsOverride = false;
			AssertEquals("Should be saved by factory", true, bizos.HasChangesInAuditDetailsForTests);

			Dummy.HasChangesInAuditDetailsOverride = false;
			otherDummy.HasChangesInAuditDetailsOverride = true;
			AssertEquals("Should be saved by factory", true, bizos.HasChangesInAuditDetailsForTests);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestIsSavedByFactoryThrowsExceptionWithConflictingIsSavedByFactories()
		{
			BusinessObjectsForARow bO4Row = new BusinessObjectsForARow(Dummy);

			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummyBase);

			Dummy.OverrideIsSavedByFactory = true;
			dummyBase.OverrideIsSavedByFactory = true;
			Dummy.IsSavedByFactoryOverride = true;
			dummyBase.IsSavedByFactoryOverride = false;

			bool someVariable = bO4Row.IsSavedByFactory;

			throw new Exception("SHOULD NOT GET HERE!");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestAddDuplicateThrowsException()
		{
			BusinessObjectsForARow bO4Row = new BusinessObjectsForARow(Dummy);
			DummyBusinessObject dummy2 = new DummyBusinessObject(Factory, Dummy.Row);
			bO4Row.Add(dummy2);
		}
	}
}
