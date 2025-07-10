using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EntityFramework.Testing.DependentRelationshipTest;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DependentBusinessObjectCollection_IFindBoxListProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructionWithNullMaster()
		{
			DummyDependentBusinessObjectCollection dependentCollection1 = new DummyDependentBusinessObjectCollection(null, Factory);
		}

		public void TestDescriptionFromCode()
		{
			string result = "";
			SetupDummiesForCodeMatching();

			result = DependantListProvider1.DescriptionFromCode(DuplicateInList1And2Code);
			AssertEquals("Description on BusinessObject that meets dependant filter criteria should be returned first", Child1of1_Desc, result);

			result = DependantListProvider2.DescriptionFromCode(DuplicateInList1And2Code);
			AssertEquals("Description on BusinessObject that meets dependant filter criteria should be returned first", Child1of2_Desc, result);

			result = DependantListProvider1.DescriptionFromCode(UniqueInList1Code);
			AssertEquals("Description on BusinessObject that doesn't meet additional filter criteria should be returned only if BusinessObject that meets additional filter criteria cannot be found", Child2of1_Desc, result);

			result = DependantListProvider2.DescriptionFromCode(UniqueInList2Code);
			AssertEquals("Description on BusinessObject that doesn't meet additional filter criteria should be returned only if BusinessObject that meets additional filter criteria cannot be found", Child2of2_Desc, result);

			result = DependantListProvider1.DescriptionFromCode(UniqueInList2Code);
			AssertNull("No Description should be returned if BusinessObject that meets dependant filter criteria cannot be found", result);

			result = DependantListProvider2.DescriptionFromCode(UniqueInList1Code);
			AssertNull("No Description should be returned if BusinessObject that meets dependant filter criteria cannot be found", result);
		}

		public void TestPrimaryKeyFromCode()
		{
			ZGuid result = ZGuid.Empty;
			SetupDummiesForCodeMatching();

			result = DependantListProvider1.PrimaryKeyFromCode(DuplicateInList1And2Code);
			AssertEquals("PK on BusinessObject that meets dependant filter criteria should be returned first", Child1of1.PK, result);

			result = DependantListProvider2.PrimaryKeyFromCode(DuplicateInList1And2Code);
			AssertEquals("PK on BusinessObject that meets dependant filter criteria should be returned first", Child1of2.PK, result);

			result = DependantListProvider1.PrimaryKeyFromCode(UniqueInList1Code);
			AssertEquals("ZGuid.Missing should be returned if BusinessObject that meets additional filter criteria cannot be found", ZGuid.Missing, result);

			result = DependantListProvider2.PrimaryKeyFromCode(UniqueInList2Code);
			AssertEquals("ZGuid.Missing should be returned if BusinessObject that meets additional filter criteria cannot be found", ZGuid.Missing, result);

			result = DependantListProvider1.PrimaryKeyFromCode(UniqueInList2Code);
			AssertEquals("ZGuid.Invalid should be returned if BusinessObject that meets dependant filter criteria cannot be found", ZGuid.Invalid, result);

			result = DependantListProvider2.PrimaryKeyFromCode(UniqueInList1Code);
			AssertEquals("ZGuid.Invalid should be returned if BusinessObject that meets dependant filter criteria cannot be found", ZGuid.Invalid, result);
		}

		#region Implementation

		IFindBoxListProvider DependantListProvider1;
		IFindBoxListProvider DependantListProvider2;
		DummyDependantBusinessObject Child1of1;
		DummyDependantBusinessObject Child2of1;
		DummyDependantBusinessObject Child1of2;
		DummyDependantBusinessObject Child2of2;

		const string Child1of1_Desc = "11";
		const string Child2of1_Desc = "21";
		const string Child1of2_Desc = "12";
		const string Child2of2_Desc = "22";

		const string DuplicateInList1And2Code = "AA";
		const string UniqueInList1Code = "AB";
		const string UniqueInList2Code = "AC";

		void SetupDummiesForCodeMatching()
		{
			DummyWithDependentsBusinessObject dummy1 = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));
			DummyDependentBusinessObjectCollection dependentCollection1 = new DummyDependentBusinessObjectCollection(dummy1, new ZQuery(DummyDependentBizoSchema.ZD1_Number, 1));
			DependantListProvider1 = dependentCollection1;
			AssertNotNull("DependantBusinessObjectCollection should inherit BusinessObjectCollection's implementation of IFindBoxListProvider", DependantListProvider1);

			DummyWithDependentsBusinessObject dummy2 = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));
			DummyDependentBusinessObjectCollection dependentCollection2 = new DummyDependentBusinessObjectCollection(dummy2, new ZQuery(DummyDependentBizoSchema.ZD1_Number, 1));
			DependantListProvider2 = dependentCollection2;
			AssertNotNull("DependantBusinessObjectCollection should inherit BusinessObjectCollection's implementation of IFindBoxListProvider", DependantListProvider2);

			Child1of1 = dependentCollection1.AddNew();
			Child1of1.ZD1_Code = DuplicateInList1And2Code;
			Child1of1.ZD1_NumberUnitCode = Child1of1_Desc;
			Child1of1.ZD1_Number = 1;

			Child2of1 = dependentCollection1.AddNew();
			Child2of1.ZD1_Code = UniqueInList1Code;
			Child2of1.ZD1_NumberUnitCode = Child2of1_Desc;

			Child1of2 = dependentCollection2.AddNew();
			Child1of2.ZD1_Code = DuplicateInList1And2Code;
			Child1of2.ZD1_NumberUnitCode = Child1of2_Desc;
			Child1of2.ZD1_Number = 1;

			Child2of2 = dependentCollection2.AddNew();
			Child2of2.ZD1_Code = UniqueInList2Code;
			Child2of2.ZD1_NumberUnitCode = Child2of2_Desc;
		}

		#endregion
	}

	sealed class DependentBusinessObjectCollectionTest : TestCaseWithDummy
	{
		public void TestAddNonCommittedAndCancelDoesNotCauseHasChangesToBecomeTrue()
		{
			AssertEquals("Precondition", 0, Dummy.Collection.Count);
			Dummy.Collection.HasChanges = false;

			((IBindingList)Dummy.Collection).AddNew();
			Assert("Has changes is false", !Dummy.Collection.HasChanges);

			((IBindingList)Dummy.Collection).RemoveAt(0);
			Assert("Has changes is false", !Dummy.Collection.HasChanges);
		}

		public void TestAddNewDoesNotCauseHasChangesToBeTrue()
		{
			Dummy.Collection.HasChanges = false;
			Dummy.Collection.AddNew();
			Assert("Has changes is false", !Dummy.Collection.HasChanges);
		}

		public void TestMastersAreInDatabase()
		{
			DummyWithDependents.SetIsInDatabase(false);
			AssertEquals("master in DB", false, ((IBusinessObjectCollectionInternals)DummyWithDependents.Dependents).MastersAreInDatabase);
			DummyWithDependents.SetIsInDatabase(true);
			AssertEquals("master not in DB", true, ((IBusinessObjectCollectionInternals)DummyWithDependents.Dependents).MastersAreInDatabase);
		}

		public void TestSetCollectionRelationships_ParentID()
		{
			var collection = new DummyDependantBusinessObjectWithParentIDCollection(Dummy);
			var child = collection.AddNew();
			AssertEquals(Dummy.TablePrefix, child.ZD1_ParentTableCode);
			AssertEquals(Dummy.TableName, child.ZD1_ParentTable);
		}

		public void TestRemoveCollectionRelationships_ParentID()
		{
			var collection = new DummyDependantBusinessObjectWithParentIDCollection(Dummy);
			var child = collection.AddNew();
			AssertEquals(Dummy.TablePrefix, child.ZD1_ParentTableCode);
			AssertEquals(Dummy.TableName, child.ZD1_ParentTable);
			collection.Remove(child);
			AssertEquals(ZString.Empty, child.ZD1_ParentTableCode);
			AssertEquals(ZString.Empty, child.ZD1_ParentTable);
		}

		public class DummyDependantBusinessObjectWithParentIDCollection : DependentBusinessObjectCollection<DummyDependantBusinessObjectWithParentID, DummyBusinessObject>
		{
			public DummyDependantBusinessObjectWithParentIDCollection(DummyBusinessObject parent) : base(parent)
			{
			}

			protected internal override SchemaGuidColumn FKSchemaColumnInDependent => new SchemaGuidColumn(DummyDependentBizoSchema.Instance, "ZD1_ParentID", 1, DBNull.Value, isNullable: true);
		}

		public void TestRemoveCollectionRelationships()
		{
			AssertEquals("Initial collection count", 0, DependentCollection.Count);

			DummyDependantBusinessObject dummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Z0 = ParentPK;
			DependentCollection.Load();
			AssertEquals("Found element with relationship", 1, DependentCollection.Count);

			DependentCollection.Remove(dummyChild);
			DependentCollection.Load();
			AssertEquals("Should not have found anything as relationship has been removed", 0, DependentCollection.Count);
		}

		public void TestHasChangesDoesNotChangeWhenChildWithRelationshipIsAdded()
		{
			AssertEquals("DependentCollection HasChanges", false, DependentCollection.HasChanges);

			DummyDependantBusinessObject dummyWithRelationship = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyWithRelationship.ZD1_Z0 = ParentPK;
			dummyWithRelationship.HasChanges = false;
			DependentCollection.Add(dummyWithRelationship);
			AssertEquals("DummyWithRelationship HasChanges", false, dummyWithRelationship.HasChanges);
			AssertEquals("DependentCollection HasChanges", false, DependentCollection.HasChanges);
		}

		public void TestHasChangesSetWhenChildWithNoRelationshipIsAdded()
		{
			AssertEquals("DependentCollection HasChanges", false, DependentCollection.HasChanges);

			DummyDependantBusinessObject dummy = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));

			DependentCollection.Add(dummy);
			AssertEquals("DummyWithRelationship HasChanges", true, dummy.HasChanges);
			AssertEquals("DependentCollection HasChanges", true, DependentCollection.HasChanges);
		}

		public void TestSetupNewElementButDoNotAddItDoesNotSetHasChanges()
		{
			AssertEquals("DependentCollection HasChanges", false, DependentCollection.HasChanges);

			DummyDependantBusinessObject dummy = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));

			DependentCollection.SetupNewElementButDoNotAddIt(dummy, true);
			AssertEquals("DummyWithRelationship HasChanges", false, dummy.HasChanges);
			AssertEquals("DependentCollection HasChanges", false, DependentCollection.HasChanges);
		}

		public void TestRelationshipFiltering()
		{
			AssertEquals(0, DependentCollection.Count);

			DummyDependantBusinessObject dummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Z0 = ParentPK;

			DummyDependantBusinessObject notDummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));

			DependentCollection.Load();

			AssertEquals(1, DependentCollection.Count);
		}

		public void TestRelationshipFilterNotInDatabase()
		{
			Assert("Pre:Condition Master Not In Database", !DependentCollection.Master.IsInDatabase);
			Assert("Should only Fetch from local cache since not in database", DependentCollection.RelationshipFilter.FetchOnlyFromLocalCache);
		}

		public void TestLoadWithFilter()
		{
			DummyWithDependentsBusinessObject dummy = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));

			DummyDependentBusinessObjectCollection dependentCollection = new DummyDependentBusinessObjectCollection(dummy, Factory);
			AssertEquals(0, dependentCollection.Count);

			DummyDependantBusinessObject dummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Z0 = dummy.PK;
			dummyChild.ZD1_Number = 10;

			DummyDependantBusinessObject dummyChild2 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Number = 10;

			dependentCollection.Load(new ZQuery(DummyDependentBizoSchema.ZD1_Number, 10));
			AssertEquals(1, dependentCollection.Count);

			dummyChild.ZD1_Number = 5;
			dependentCollection.Load(new ZQuery(DummyDependentBizoSchema.ZD1_Number, 10));
			AssertEquals(0, dependentCollection.Count);
		}

		public void TestLoadWithFilterAndParams()
		{
			DummyWithDependentsBusinessObject dummy = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));

			DummyDependentBusinessObjectCollection dependentCollection = new DummyDependentBusinessObjectCollection(dummy, Factory);
			AssertEquals(0, dependentCollection.Count);

			DummyDependantBusinessObject dummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Z0 = dummy.PK;
			dummyChild.ZD1_Number = 10;

			DummyDependantBusinessObject dummyChild2 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Number = 10;

			ZQuery query = new ZQuery(DummyDependentBizoSchema.ZD1_Number, 10);
			dependentCollection.Load(query);
			AssertEquals(1, dependentCollection.Count);

			dummyChild.ZD1_Number = 5;
			dependentCollection.Load(query);
			AssertEquals(0, dependentCollection.Count);
		}

		public void TestConstuctorProvidedFiltering()
		{
			DummyWithDependentsBusinessObject dummy = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));

			DummyDependentBusinessObjectCollection dependentCollection = new DummyDependentBusinessObjectCollection(dummy, new ZQuery(DummyDependentBizoSchema.ZD1_Number, new ZInt(10)));
			AssertEquals(0, dependentCollection.Count);

			DummyDependantBusinessObject dummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Z0 = dummy.PK;
			dummyChild.ZD1_Number = 10;

			dependentCollection.Load();
			AssertEquals(1, dependentCollection.Count);

			dummyChild.ZD1_Number = 5;
			dependentCollection.Load();
			AssertEquals(0, dependentCollection.Count);
		}

		public void TestIsMatchingCollectionFilter()
		{
			var matchingCollection = DependentCollection as IBusinessObjectFilterFactory;
			DependentCollection.Load();
			DummyDependantBusinessObject bizO = DependentCollection.AddNew();
			Assert(matchingCollection.NewBusinessObjectFilter().IsMatching(bizO));

			bizO.ZD1_Z0 = ZGuid.NewZGuid();
			Assert(!matchingCollection.NewBusinessObjectFilter().IsMatching(bizO));

			bizO.ZD1_Z0 = ParentPK;
			Assert(matchingCollection.NewBusinessObjectFilter().IsMatching(bizO));

			DependentCollection.Load(new ZQuery(DummyDependentBizoSchema.ZD1_Code, "123"));
			Assert(!matchingCollection.NewBusinessObjectFilter().IsMatching(bizO));

			bizO.ZD1_Code = "123";
			Assert(matchingCollection.NewBusinessObjectFilter().IsMatching(bizO));
		}

		#region TestIsMatchinCollectionFilterCreatesOneFilterForBatchCompare

		public void TestIsMatchinCollectionFilterCreatesOneFilterForBatchCompare()
		{
			var collection = new DummyDependentBusinessObjectCollectionWithFilterCreation(DummyWithDependents, DummyWithDependents.Factory);
			collection.Load();
			var child1 = collection.AddNew();
			var child2 = collection.AddNew();

			AssertCreateRelationshipFilterCounter("Creates new filter each time by default", 2, collection, false, child1, child2);
			AssertCreateRelationshipFilterCounter("Should only create one filter", 1, collection, true, child1, child2);
			AssertCreateRelationshipFilterCounter("Creates new filter each time again after releasing HoldFilterForBatchMatching", 2, collection, false, child1, child2);
		}

		void AssertCreateRelationshipFilterCounter(string message, int expectedCount, DummyDependentBusinessObjectCollectionWithFilterCreation collection, bool holdFilter, params DummyDependantBusinessObject[] children)
		{
			var matchingCollection = collection as IBusinessObjectFilterFactory;

			collection.CreateRelationshipFilterCounter = 0;

			var businessObjectFilter = holdFilter ? matchingCollection.NewBusinessObjectFilter() : null;
			foreach (var child in children)
			{
				businessObjectFilter = holdFilter ? businessObjectFilter : matchingCollection.NewBusinessObjectFilter();
				businessObjectFilter.IsMatching(child);
			}

			AssertEquals(message, expectedCount, collection.CreateRelationshipFilterCounter);
		}

		class DummyDependentBusinessObjectCollectionWithFilterCreation : DummyDependentBusinessObjectCollection
		{
			public DummyDependentBusinessObjectCollectionWithFilterCreation(DummyBaseBusinessObject parent, BusinessObjectFactory factory) : base(parent, factory) { }

			protected override ZQuery CreateRelationshipFilter()
			{
				CreateRelationshipFilterCounter++;
				return base.CreateRelationshipFilter();
			}

			public int CreateRelationshipFilterCounter { get; set; }
		}

		#endregion

		public void TestAddNew()
		{
			AssertEquals(0, DependentCollection.Count);

			DummyDependantBusinessObject newBO = DependentCollection.AddNew();

			AssertEquals(1, DependentCollection.Count);
			AssertNotNull("Check FK is set", newBO.ZD1_Z0);
		}

		[ExpectException(typeof(Exception))]
		public void TestAddBeforeRemoveFromOtherCollectionThrowsException()
		{
			var dummy2 = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));
			var dependentCollection2 = dummy2.Dependents;

			var col1Obj = DependentCollection.AddNew();
			dependentCollection2.Add(col1Obj);
		}

		[ExpectNoExceptions()]
		public void TestAddAfterRemoveFromOtherCollectionCausesNoException()
		{
			var dummy2 = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));
			var dependentCollection2 = dummy2.Dependents;

			var col1Obj = DependentCollection.AddNew();
			DependentCollection.Remove(col1Obj);
			dependentCollection2.Add(col1Obj);
		}

		public void TestAddShouldNotRaiseAccessingPropertyOnDeletedBusinessObjectError()
		{
			ErrorReporter.Clear();
			var dummy = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));
			var dependentCollection = dummy.Dependents;
			var bizo = dependentCollection.AddNew();
			bizo.Delete();
			dependentCollection.Add(bizo);
			AssertEquals(0, dependentCollection.Count);
			AssertNotContains("DummyDependentBizoGetPropertyValueByName(String propertyName)", ErrorReporter.LastKeyReported);
			AssertNotContains("Developer Error: Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
		}

		public void TestDataRowUnchangedAfterLoad()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBaseBusinessObject parent = (DummyBaseBusinessObject)factory.New(typeof(DummyBaseBusinessObject));
			DummyDependentBusinessObjectCollection collection = new DummyDependentBusinessObjectCollection(parent, factory);

			collection.AddNew();
			factory.Save();
			factory = new BusinessObjectFactory();

			DummyBaseBusinessObject retrievedParent = (DummyBaseBusinessObject)factory.Load(typeof(DummyBaseBusinessObject), parent.PK);
			collection = new DummyDependentBusinessObjectCollection(parent, parent.Factory);
			collection.Load();
			AssertEquals("DataRowState.Unchanged on retrieve", DataRowState.Unchanged, ((INeedRow)collection[0]).Row.RowState);
		}

		public void TestDependentCollectionWithNullMaster()
		{
			DummyDependentBusinessObjectCollection collection = new DummyDependentBusinessObjectCollection(Factory);
			AssertEquals("DependentBusinessObjectCollection with null master read-only", true, collection.ReadOnly);
			AssertEquals("DependentBusinessObjectCollection with null master read-only", 0, collection.Count);
		}

		public void TestDependentCollectionWithNullMaster_AddNewThrowsSilentException()
		{
			UnitTestUserNotification.Instance.ClearShownErrorKeys();
			DummyDependentBusinessObjectCollection collection = new DummyDependentBusinessObjectCollection(Factory);
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));

			collection.AddNew();
			AssertNotNull("Calling AddNew on a dependent collection without a master results in silent exception", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestRelationshipFilterWithNullMaster()
		{
			DummyDependentBusinessObjectCollection collection = new DummyDependentBusinessObjectCollection(Factory);
			ZQuery filter = collection.RelationshipFilter;
		}

		[ExpectNoExceptions]
		public void TestCompleteFilterWithNullMaster()
		{
			DummyDependentBusinessObjectCollection collection = new DummyDependentBusinessObjectCollection(Factory);
			ZQuery filter = collection.CompleteFilter;
		}

		class DummyDependentBusinessObjectCollectionThatUsesMaster : DummyDependentBusinessObjectCollection
		{
			public DummyDependentBusinessObjectCollectionThatUsesMaster(DummyBaseBusinessObject parent, BusinessObjectFactory factory)
				: base(parent, factory)
			{
			}

			public new DummyDependantBusinessObject AddNew()
			{
				return (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			}

			protected override ZQuery CreateRelationshipFilter()
			{
				if (Master.Z0_AnotherNumber != 0)
				{
					//dont care as long as Master is touched here.
				}
				return base.CreateRelationshipFilter();
			}
		}

		[ExpectNoExceptions()]
		public void TestAddFromDatabaseIfMatchLastLoadedFilterWhenMasterIsDeleted()
		{
			DummyDependentBusinessObjectCollectionThatUsesMaster collection = new DummyDependentBusinessObjectCollectionThatUsesMaster(Dummy, Factory);
			collection.Load();

			AssertEquals("IsInDatabase", false, Dummy.IsInDatabase);
			Dummy.Delete();

			DummyDependantBusinessObject dummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Code = "TTT";

			collection.AddFromDatabaseIfMatchLastLoadedFilter(dummyChild.PK);
		}

		public void TestChildType()
		{
			AssertEquals(typeof(DummyDependantBusinessObject), ((IDependentBusinessObjectCollection)DependentCollection).ChildType);
		}

		public void TestMaster()
		{
			AssertEquals(DependentCollection.Master, ((IDependentBusinessObjectCollection)DependentCollection).Master);
		}

		public void TestCloneElementsTo()
		{
			var object1 = DependentCollection.AddNew();
			object1.ZD1_Code = "Obj1";
			var object2 = DependentCollection.AddNew();
			object2.ZD1_Code = "Obj2";
			object1.Delete();

			var newCollection = new DummyDependentBusinessObjectCollection(Factory);
			DependentCollection.CloneElementsTo(newCollection);
			AssertEquals(1, newCollection.Count);
			AssertEquals("Obj2", newCollection[0].ZD1_Code);
		}

		public void TestGetTypeDeciderContext()
		{
			var dummyWithDependentsAndTypeDeciderContext = Factory.New<DummyWithDependentsAndTypeDeciderContextBusinessObject>();
			var newCollection = dummyWithDependentsAndTypeDeciderContext.Dependents;

			var typeDeciderContext = newCollection.GetTypeDeciderContext();
			AssertNotNull(typeDeciderContext);
			AssertSame(dummyWithDependentsAndTypeDeciderContext, typeDeciderContext);
		}

		#region Implementation

		DummyDependentBusinessObjectCollection DependentCollection;
		ZGuid ParentPK;
		DummyWithDependentsBusinessObject DummyWithDependents;

		protected override void SetUp()
		{
			base.SetUp();
			DummyWithDependents = (DummyWithDependentsBusinessObject)Factory.New(typeof(DummyWithDependentsBusinessObject));
			DependentCollection = DummyWithDependents.Dependents;
			ParentPK = DummyWithDependents.PK;
		}

		#endregion
	}
}
