using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ManyToManyComplexRelationship_SimpleRelationship_Test : TestCaseWithFactory
	{
		public void TestInitPivotsErrorReport()
		{
			var master = Factory.New<DummyBusinessObjectWithManyToMany>();
			var el1 = master.ManyToManyCollection.AddNew();
			var pivot = Factory.New<DummyPivot>();
			pivot.ZDP_Z0 = master.PK;
			pivot.ZDP_ZD1 = el1.PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var masterFromFactory2 = factory2.Load<DummyBusinessObjectWithManyToMany>(master.PK);
			master.ManyToManyCollection.Add(el1);
			masterFromFactory2.ManyToManyCollection.Add(el1);
			AssertContains($"Master business object of type 'CargoWise.EntityFramework.Testing.ManyToManyComplexRelationship_SimpleRelationship_Test+DummyBusinessObjectWithManyToMany', with PK '{master.PK}'", ErrorReporter.LastMessageReported);
			AssertContains($"Collection element type 'CargoWise.EntityFramework.Testing.DummyDependantBusinessObject', with PivotTableFKToElements.Name 'ZDP_ZD1', Value '{el1.PK}'", ErrorReporter.LastMessageReported);
			AssertContains("query.FetchOnlyFromLocalCache: False", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetPivotObject()
		{
			var element = Collection.AddNew();
			var element2 = Collection.AddNew();
			var pivot = Factory.LoadTop1<DummyPivot>(new ZQuery(DummyPivotSchema.ZDP_ZD1, element2.PK));
			AssertEquals("GetPivotObject", pivot, Relationship.GetPivotObject(element2));
		}

		public void TestSavingMasterDoesNotCreateExtraDBHits()
		{
			new BusinessObjectFactory().New<DummyPivot>(); // initialize the wrapped property cache first

			var newFactory = new BusinessObjectFactory();
			var master = Factory.New<DummyBusinessObjectWithManyToMany>();
			master.Z0_Code = "ZZ1";
			AssertEquals(0, master.ManyToManyCollection.Count);
			AssertEquals(0, master.DependentCollection.Count);
			AssertNull(newFactory.Load<DummyBusinessObjectWithManyToMany>(master.PK));
			Factory.Save();

			var reloadMaster = newFactory.Load<DummyBusinessObjectWithManyToMany>(master.PK);
			AssertEquals(0, reloadMaster.ManyToManyCollection.Count);
			AssertEquals(0, reloadMaster.DependentCollection.Count);

			var element = master.ManyToManyCollection.AddNew();
			element.ZD1_Code = "ZZ2";
			element.ZD1_Z0 = master.PK;
			Factory.Save();
			AssertHits(Factory, 0, newFactory, 4);
			AssertEquals(1, master.ManyToManyCollection.Count);
			AssertEquals(1, master.DependentCollection.Count);
			AssertEquals(1, reloadMaster.ManyToManyCollection.Count);
			AssertEquals(1, reloadMaster.DependentCollection.Count);
			AssertHits(Factory, 0, newFactory, 4);

			var element2 = Factory.New<DummyDependantBusinessObject>();
			element2.ZD1_Code = "ZZ3";
			element2.ZD1_Z0 = master.PK;
			Factory.Save();
			AssertHits(Factory, 0, newFactory, 4);
			AssertEquals(1, master.ManyToManyCollection.Count);
			AssertEquals(2, master.DependentCollection.Count);
			AssertEquals(1, reloadMaster.ManyToManyCollection.Count);
			AssertEquals(2, reloadMaster.DependentCollection.Count);
			AssertHits(Factory, 0, newFactory, 4);

			master.ManyToManyCollection.Add(element2);
			Factory.Save();
			AssertEquals(2, master.ManyToManyCollection.Count);
			AssertEquals(2, master.DependentCollection.Count);
			AssertEquals(2, reloadMaster.ManyToManyCollection.Count);
			AssertEquals(2, reloadMaster.DependentCollection.Count);
			AssertHits(Factory, 0, newFactory, 4);

			var master2 = newFactory.New<DummyBusinessObjectWithManyToMany>();
			master2.Z0_Code = "ZZ2";
			newFactory.Save();

			var reloadMaster2 = Factory.Load<DummyBusinessObjectWithManyToMany>(master2.PK);
			AssertEquals(0, master2.ManyToManyCollection.Count);
			AssertEquals(0, master2.DependentCollection.Count);
			AssertEquals(0, reloadMaster2.ManyToManyCollection.Count);
			AssertEquals(0, reloadMaster2.DependentCollection.Count);
			AssertHits(Factory, 3, newFactory, 6);

			var pivot3 = Factory.New<DummyPivot>();
			pivot3.ZDP_Z0 = master2.PK;
			pivot3.ZDP_ZD1 = element.PK;
			var pivot4 = Factory.New<DummyPivot>();
			pivot4.ZDP_Z0 = master2.PK;
			pivot4.ZDP_ZD1 = element2.PK;
			Factory.Save();
			AssertHits(Factory, 3, newFactory, 6);
			AssertEquals(2, master2.ManyToManyCollection.Count);
			AssertEquals(0, master2.DependentCollection.Count);
			AssertEquals(2, reloadMaster2.ManyToManyCollection.Count);
			AssertEquals(0, reloadMaster2.DependentCollection.Count);
			AssertHits(Factory, 3, newFactory, 6);

			for (var i = 1; i < 101; i++)
			{
				var elementI = master.ManyToManyCollection.AddNew();
				elementI.ZD1_Code = i.ToString().PadLeft(3, '0');
			}
			Factory.Save();

			AssertHits(Factory, 3, newFactory, 106);
		}

		void AssertHits(BusinessObjectFactory factory1, int maximumAllowableHitsForFactory1, BusinessObjectFactory factory2, int maximumAllowableHitsForFactory2)
		{
			var actualHitsForFactory1 = factory1.DatabaseLoadCount;
			var hitsResult = new ZStringBuilder();
			hitsResult.Append("Factory 1 - Maximum hits : " + maximumAllowableHitsForFactory1 + ", Actual hits : " + actualHitsForFactory1 + System.Environment.NewLine);
			foreach (var hitCount in factory1.TableSelects)
			{
				hitsResult.Append("   " + hitCount.TableName + ": " + hitCount.Value.ToString() + " selects" + System.Environment.NewLine);
			}
			hitsResult.Append(System.Environment.NewLine);

			var actualHitsForFactory2 = factory2.DatabaseLoadCount;
			hitsResult.Append("Factory 2 - Maximum hits : " + maximumAllowableHitsForFactory2 + ", Actual hits : " + actualHitsForFactory2 + System.Environment.NewLine);
			foreach (var hitCount in factory2.TableSelects)
			{
				hitsResult.Append("   " + hitCount.TableName + ": " + hitCount.Value.ToString() + " selects" + System.Environment.NewLine);
			}
			AssertEquals(hitsResult.ToString(), false, maximumAllowableHitsForFactory1 < actualHitsForFactory1 || maximumAllowableHitsForFactory2 < actualHitsForFactory2);
		}

		public void TestHasChangesIncludingRelationship()
		{
			var child = Collection.AddNew();
			var pivot = Factory.LoadTop1<DummyPivot>(new ZQuery(DummyPivotSchema.ZDP_ZD1, child.PK));
			AssertEquals("Foreign keys set on pivot on Add()", Master.PK, pivot.ZDP_Z0);
			AssertEquals("Foreign keys set on pivot on Add()", child.PK, pivot.ZDP_ZD1);
			Factory.Save();
			AssertEquals("No changes initially", false, Collection.Relationship.HasChangesIncludingRelationship(child));

			pivot.HasChanges = true;
			AssertEquals("Has changes after foreign key modified", true, Collection.Relationship.HasChangesIncludingRelationship(child));
		}

		public void TestClearHasChangesIncludingRelationship()
		{
			var child = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			Factory.Save();

			Collection.Add(child);
			AssertEquals("Precondition", true, Collection.Relationship.HasChangesIncludingRelationship(child));

			Collection.Relationship.ClearHasChangesIncludingRelationship(child);

			AssertEquals(false, Collection.Relationship.HasChangesIncludingRelationship(child));
		}

		public void TestAddToRelationship_OnAddNew()
		{
			var child = Collection.AddNew();
			var pivot = Factory.LoadTop1<DummyPivot>(new ZQuery(DummyPivotSchema.ZDP_ZD1, child.PK));
			AssertEquals("Foreign keys set on pivot on AddNew()", Master.PK, pivot.ZDP_Z0);
			AssertEquals("Foreign keys set on pivot on AddNew()", child.PK, pivot.ZDP_ZD1);
		}

		public void TestAddToRelationship_OnAdd()
		{
			var child = Factory.New<DummyDependantBusinessObject>();
			Collection.Add(child);

			var pivot = Factory.LoadTop1<DummyPivot>(new ZQuery(DummyPivotSchema.ZDP_ZD1, child.PK));
			AssertEquals("Foreign keys set on pivot on Add()", Master.PK, pivot.ZDP_Z0);
			AssertEquals("Foreign keys set on pivot on Add()", child.PK, pivot.ZDP_ZD1);
		}

		public void TestRemoveFromRelationship()
		{
			var child = Collection.AddNew();

			var pivot = Factory.LoadTop1<DummyPivot>(new ZQuery(DummyPivotSchema.ZDP_ZD1, child.PK));
			AssertEquals("Foreign keys on pivot set", Master.PK, pivot.ZDP_Z0);
			AssertEquals("Foreign keys on pivot set", child.PK, pivot.ZDP_ZD1);

			Collection.RemoveFromRelationship(child);
			AssertEquals("Pivot deleted", true, pivot.IsDeleted);
		}

		public void TestAddedToCollectionWhenPivotCreatedManually()
		{
			((IBindingList)Collection).ListChanged += new ListChangedEventHandler(OnListChanged);

			var child = Factory.New<DummyDependantBusinessObject>();
			var pivot = Factory.New<DummyPivot>();
			pivot.ZDP_Z0 = Master.PK;
			pivot.ZDP_ZD1 = child.PK;

			AssertEquals("Reset event raised", ListChangedType.Reset, listChangedEvents[0].ListChangedType);
			var list = new List<DummyDependantBusinessObject>(Collection);
			AssertEquals("Creating the pivot implicitly should cause it to be in the collection", true, list.Contains(child));
		}

		public void TestAddedToCollectionWhenPivotCreatedManually_SettingFKsInReverse()
		{
			((IBindingList)Collection).ListChanged += new ListChangedEventHandler(OnListChanged);

			var child = Factory.New<DummyDependantBusinessObject>();
			var pivot = Factory.New<DummyPivot>();
			pivot.ZDP_ZD1 = child.PK;
			pivot.ZDP_Z0 = Master.PK;

			AssertEquals("Reset event raised", ListChangedType.Reset, listChangedEvents[0].ListChangedType);
			var list = new List<DummyDependantBusinessObject>(Collection);
			AssertEquals("Creating the pivot implicitly should cause it to be in the collection", true, list.Contains(child));
		}

		public void TestContainsInAnotherFactory()
		{
			var child = Collection.AddNew();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedMaster = anotherFactory.Load<DummyBusinessObject>(Master.PK);
			var loadedChild = anotherFactory.Load<DummyDependantBusinessObject>(child.PK);
			var loadedCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(loadedMaster, typeof(DummyPivot));
			Assert(loadedCollection.Contains(loadedChild));
		}

		public void TestCountInAnotherFactory()
		{
			var child = Collection.AddNew();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedMaster = anotherFactory.Load<DummyBusinessObject>(Master.PK);
			var loadedCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(loadedMaster, typeof(DummyPivot));
			AssertEquals(1, loadedCollection.Count);
		}

		public void TestDataViewIsRefreshedAfterAddNew()
		{
			AssertEquals(0, Collection.Count);
			var child = Collection.AddNew();
			var child2 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals(1, Collection.Count);
		}

		[ExpectNoExceptions]
		public void TestNotRelatedPivotsAreNotLoaded()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var child = Factory.New<DummyDependantBusinessObject>();

			var pivot1 = Factory.New<DummyPivot>();
			pivot1.ZDP_Z0 = dummy.PK;
			pivot1.ZDP_ZD1 = child.PK;

			var pivot2 = Factory.New<DummyPivot>();
			pivot2.ZDP_Z0 = dummy.PK;

			var pivot3 = Factory.New<DummyPivot>();
			pivot3.ZDP_Z0 = dummy.PK;

			var manyToManyComplexRelationship = new DefaultDummyBizoManyToManyComplexRelationship(dummy, new ZQuery());
			manyToManyComplexRelationship.LoadBusinessObjects(Factory, new ZQuery());
			AssertEquals(1, manyToManyComplexRelationship.pivots.Count);

			var reverseRelationship = new ReverseDummyBizoManyToManyComplexRelationship(child, new ZQuery());
			reverseRelationship.LoadBusinessObjects(Factory, new ZQuery());
			AssertEquals(1, reverseRelationship.pivots.Count);
			AssertEquals(pivot1, reverseRelationship.pivots.First().Value);
		}

		public void TestManyToManyComplexRelationship_WhenElementPointsToPivotTable()
		{
			var master = Factory.New<DummyBusinessObject>();
			var relationship = new ElementFKtoPivotPKManyToManyComplexRelationship(master, new ZQuery());
			var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(master.Factory, relationship);

			var pivot1 = Factory.New<DummyPivot>();
			pivot1.ZDP_Z0 = master.PK;

			AssertEquals("Precondition: Collection is empty.", 0, collection.Count);

			var element1 = Factory.New<DummyDependantBusinessObject>();
			element1.ZD1_NumberUnit = pivot1.PK;

			var element2 = Factory.New<DummyDependantBusinessObject>();
			element2.ZD1_NumberUnit = pivot1.PK;

			AssertContainsExactElementsInAnyOrder(new[] { element1, element2 }, collection);
		}

		public void TestManyToManyComplexRelationship_DeletedPivotsAreNotAdded_FromListChangedEvent()
		{
			((IBindingList)Collection).ListChanged += new ListChangedEventHandler(OnListChanged);

			var child = Factory.New<DummyDependantBusinessObject>();
			var pivot = Factory.New<DummyPivot>();
			pivot.ZDP_Z0 = Master.PK;
			pivot.ZDP_ZD1 = child.PK;
			pivot.Delete();

			AssertEquals("Reset event raised", ListChangedType.Reset, listChangedEvents[0].ListChangedType);
			var list = new List<DummyDependantBusinessObject>(Collection);
			AssertEquals("A deleted pivot should not cause be loaded implicitly in the collection", false, list.Contains(child));
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			AssertEquals(
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master, new ZQuery()),
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master, new ZQuery()));
			AssertNotEquals(
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master, new ZQuery()),
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master2, new ZQuery()));
		}

		public void TestHashCode()
		{
			AssertEquals(
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master, new ZQuery()).GetHashCode(),
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master, new ZQuery()).GetHashCode());
			AssertNotEquals(
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master, new ZQuery()).GetHashCode(),
				new ElementFKtoPivotPKManyToManyComplexRelationship(Master2, new ZQuery()).GetHashCode());
		}

		#endregion

		#region Implementation

		readonly List<ListChangedEventArgs> listChangedEvents = new List<ListChangedEventArgs>();

		void OnListChanged(object sender, ListChangedEventArgs e)
		{
			listChangedEvents.Add(e);
		}

		ActiveBusinessObjectCollection<DummyDependantBusinessObject> Collection
		{
			get { return collection ?? (collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Master.Factory, new DefaultDummyBizoManyToManyComplexRelationship(Master, new ZQuery()))); }
		}
		ActiveBusinessObjectCollection<DummyDependantBusinessObject> collection;

		ManyToManyComplexRelationship Relationship
		{
			get { return (ManyToManyComplexRelationship)Collection.Relationship; }
		}

		DummyBusinessObject Master
		{
			get { return master ?? (master = Factory.New<DummyBusinessObject>()); }
		}
		DummyBusinessObject master;

		DummyBusinessObject Master2
		{
			get { return master2 ?? (master2 = Factory.New<DummyBusinessObject>()); }
		}
		DummyBusinessObject master2;

		class DummyBusinessObjectWithManyToMany : DummyBusinessObject
		{
			public DummyBusinessObjectWithManyToMany(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ActiveBusinessObjectCollection<DummyDependantBusinessObject> ManyToManyCollection
			{
				get
				{
					if (manyToManyCollection == null)
					{
						manyToManyCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(this, typeof(DummyPivot));
						RegisterEditableChildObject(manyToManyCollection);
					}
					return manyToManyCollection;
				}
			}
			ActiveBusinessObjectCollection<DummyDependantBusinessObject> manyToManyCollection;

			public ActiveBusinessObjectCollection<DummyDependantBusinessObject> DependentCollection
			{
				get
				{
					if (dependentCollection == null)
					{
						dependentCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(this);
						RegisterEditableChildObject(dependentCollection);
					}
					return dependentCollection;
				}
			}
			ActiveBusinessObjectCollection<DummyDependantBusinessObject> dependentCollection;
		}

		class DefaultDummyBizoManyToManyComplexRelationship : ManyToManyComplexRelationship
		{
			public DefaultDummyBizoManyToManyComplexRelationship(DummyBusinessObject master, ZQuery filter)
			 : base(master, typeof(DummyDependantBusinessObject), typeof(DummyPivot), filter
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyPivot.Schema.ZDP_Z0, DummyPivot.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyBusinessObject.Schema.PK, DummyBusinessObject.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyPivot.Schema.ZDP_ZD1, DummyPivot.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyDependantBusinessObject.Schema.PK, DummyDependantBusinessObject.Schema.TableName) })
			{
			}
		}

		class ReverseDummyBizoManyToManyComplexRelationship : ManyToManyComplexRelationship
		{
			public ReverseDummyBizoManyToManyComplexRelationship(DummyDependantBusinessObject master, ZQuery filter)
			 : base(master, typeof(DummyBusinessObject), typeof(DummyPivot), filter
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyPivot.Schema.ZDP_ZD1, DummyPivot.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyDependantBusinessObject.Schema.PK, DummyDependantBusinessObject.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyPivot.Schema.ZDP_Z0, DummyPivot.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyBusinessObject.Schema.PK, DummyBusinessObject.Schema.TableName) })
			{
			}
		}

		class ElementFKtoPivotPKManyToManyComplexRelationship : ManyToManyComplexRelationship
		{
			public ElementFKtoPivotPKManyToManyComplexRelationship(DummyBusinessObject master, ZQuery filter)
			 : base(master, typeof(DummyDependantBusinessObject), typeof(DummyPivot), filter
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyPivot.Schema.ZDP_Z0, DummyPivot.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyBusinessObject.Schema.PK, DummyBusinessObject.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyPivot.Schema.PK, DummyPivot.Schema.TableName) }
				, new List<SchemaColumn>() { (SchemaGuidColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyDependantBusinessObject.Schema.ZD1_NumberUnit, DummyDependantBusinessObject.Schema.TableName) } )
			{
			}
		}

		#endregion
	}
}
