using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	internal abstract class ActiveBusinessObjectCollectionIndexTest : TestCaseWithFactory
	{
		#region Tests

		public void TestRefreshInNonDependentActiveBusinessObjectCollection()
		{
			for (var i = 0; i < 10; i++)
			{
				var bizObj = Factory.NewWithValidTestData<DummyBusinessObject>();
				if (i > 4)
				{
					bizObj.Z0_Number = 999;
				}
				else
				{
					bizObj.Z0_Number = 888;
				}
			}

			var newFactory = new BusinessObjectFactory();
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Number, 999);

			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(newFactory, new AdhocCollectionRelationship(typeof(DummyBusinessObject)));
			collection.AdditionalFilter = dbOnlyQuery;
			AddBizOToCollectionWithAdhocRelationship(collection);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
			{
				Factory.Save();
			}

			AssertEquals(5, collection.Count);
		}

		public void TestRefreshInDependentActiveBusinessObjectCollection()
		{
			var master = Factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();
			for (int i = 0; i < 10; i++)
			{
				var dummy = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
				dummy.ZD1_Z0 = master.PK;
				dummy.ZD1_Code = "DD";
			}

			var newFactory = new BusinessObjectFactory();
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			dbOnlyQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Code, "DD");
			var dependentCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(newFactory, master, dbOnlyQuery, DummyDependentBizoSchema.ZD1_Z0);
			var bizos = Factory.Load<DummyDependantBusinessObject>(filter);
			if (bizos != null)
			{
				bizos.ForEach(x => dependentCollection.Add(x));
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DummyDependentBizoSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
			{
				Factory.Save();
			}

			AssertEquals(10, dependentCollection.Count);
		}

		public void TestRefreshInManyToManyActiveBusinessObjectCollection()
		{
			var master = Factory.New<DummyWithDependentsBusinessObject>();
			for (int i = 0; i < 10; i++)
			{
				var dummy = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
				dummy.ZD1_Code = "FF";
			}

			var newFactory = new BusinessObjectFactory();
			var dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			dbOnlyQuery.AddToFilter(DummyDependentBizoSchema.ZD1_Code, "FF");
			var manyToManyCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(newFactory, new ManyToManyRelationship(master, typeof(DummyDependantBusinessObject), typeof(DummyPivot)));
			manyToManyCollection.AdditionalFilter = dbOnlyQuery;

			var bizos = Factory.Load<DummyDependantBusinessObject>(filter);
			if (bizos != null)
			{
				bizos.ForEach(x => manyToManyCollection.Add(x));
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DummyDependentBizoSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
			{
				Factory.Save();
			}

			AssertEquals(10, manyToManyCollection.Count);
		}

		public void TestRefreshDeletedInCollection()
		{
			var collectionInFactory1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(new BusinessObjectFactory(), new ZQuery(DummyBizoSchema.Z0_Number, 777));
			var dummyInFactory1 = collectionInFactory1.AddNew();
			dummyInFactory1.Z0_Description = "ABC";
			dummyInFactory1.Z0_Number = 777;
			dummyInFactory1.Factory.Save();

			AssertEquals(1, collectionInFactory1.Count);

			var dummyInFactory2 = new BusinessObjectFactory().Load<DummyBusinessObject>(dummyInFactory1.PK);

			dummyInFactory1.Delete();
			dummyInFactory2.Z0_Description = "XYZ";

			Assert(dummyInFactory1.IsDeleted);
			AssertEquals("ABC", dummyInFactory1.Z0_DescriptionInfo.OriginalValue);
			AssertEquals(0, collectionInFactory1.Count);
			Assert(dummyInFactory1.IsDeleted);

			dummyInFactory2.Factory.Save();

			CombineAssertions("WHEN bizObj @ other factory was modified and saved THEN", () =>
			{
				Assert("Original dummy should be deleted", dummyInFactory1.IsDeleted);
				AssertEquals("XYZ", dummyInFactory1.Z0_DescriptionInfo.OriginalValue);
				AssertEquals("Collection should be empty", 0, collectionInFactory1.Count);

				var dummyInCollectionInFactory1 = collectionInFactory1.FirstOrDefault();
				AssertNull("No elements in collection", dummyInCollectionInFactory1);
			});
		}

		public void TestRefreshDeletedAndSavedInCollection()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "TFactory1" };
			var factory2 = new BusinessObjectFactory { NameForDebugging = "TFactory2" };

			var collectionInFactory1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory1, new ZQuery(DummyBizoSchema.Z0_Number, 777));
			var dummyInFactory1 = collectionInFactory1.AddNew();
			dummyInFactory1.Z0_Description = "ABC";
			dummyInFactory1.Z0_Number = 777;
			dummyInFactory1.Factory.Save();

			AssertEquals(1, collectionInFactory1.Count);

			var dummyInFactory2 = factory2.Load<DummyBusinessObject>(dummyInFactory1.PK);

			dummyInFactory1.Delete();
			dummyInFactory2.Z0_Description = "XYZ";

			Assert(dummyInFactory1.IsDeleted);
			AssertEquals("ABC", dummyInFactory1.Z0_DescriptionInfo.OriginalValue);
			AssertEquals(0, collectionInFactory1.Count);
			Assert(dummyInFactory1.IsDeleted);

			factory2.Save();
			factory1.Save();

			CombineAssertions("WHEN bizObj @ other factory was modified and saved THEN", () =>
			{
				Assert("Original dummy should be deleted", dummyInFactory1.IsDeleted);
				AssertEquals("Collection should be empty", 0, collectionInFactory1.Count);

				var dummyInCollectionInFactory1 = collectionInFactory1.FirstOrDefault();
				AssertNull("No elements in collection", dummyInCollectionInFactory1);
			});
		}

		public void TestCompleteAdoReductionFilterShouldContainRelationshipFilter()
		{
			var dummyNol1 = Factory.New<DummyBusinessObjectWithCodeRestricted>();
			dummyNol1.Z0_Description = "Dummy1";
			var dummyNol2 = Factory.New<DummyBusinessObjectWithCodeRestricted>();
			dummyNol2.Z0_Description = "Dummy2";
			var otherDummy = Factory.New<DummyBusinessObject>();
			otherDummy.Z0_Code = "ANT";
			Factory.Save();

			var dummyCollection =
				new ActiveBusinessObjectCollection<DummyBusinessObjectWithCodeRestricted>(Factory,
					new CollectionRelationship(typeof(DummyBusinessObjectWithCodeRestricted),
						new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "NOL")))
				{
					AdditionalFilter = new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, "Dummy") { IsDBOnlyQuery = true }
				};

			AssertEquals(2, dummyCollection.Count);
			AssertNoExceptionThrown(() => dummyCollection.IndexExposed.ResetListWithCache());
		}

		public void TestDeactivateDoesNotThrow()
		{
			var index = GetCollection<DummyDependantBusinessObject>(Factory).IndexExposed;

			AssertNoExceptionThrown(delegate
			{ index.DeactivateAllOwners(); });
		}

		public void TestDeactivateIsCalledOnOwner()
		{
			var index = GetCollection<DummyDependantBusinessObject>(Factory).IndexExposed;

			var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory.New(typeof(DummyWithDependentsBusinessObject)));
			collection.AddNew();
			((IBusiness)collection).IncrementReadOnlyIncludingChildren();
			index.AddOwner(collection);

			index.DeactivateAllOwners();
			AssertEquals(true, collection.IsDeactivated);
		}

		public void TestReorder()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "001";
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "003";
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Code = "005";
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy4.Z0_Code = "007";
			var dummy5 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy5.Z0_Code = "009";

			var collection = GetCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "00"));

			collection.ApplySort(DummyBizoSchema.Constants.Z0_Code, ListSortDirection.Ascending);

			AssertEquals(5, collection.Count);
			AssertEquals("001", collection[0].Z0_Code);
			AssertEquals("003", collection[1].Z0_Code);
			AssertEquals("005", collection[2].Z0_Code);
			AssertEquals("007", collection[3].Z0_Code);
			AssertEquals("009", collection[4].Z0_Code);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				dummy2.Z0_Code = "008";

				AssertEquals(5, collection.Count);
				AssertEquals("001", collection[0].Z0_Code);
				AssertEquals("005", collection[1].Z0_Code);
				AssertEquals("007", collection[2].Z0_Code);
				AssertEquals("008", collection[3].Z0_Code);
				AssertEquals("009", collection[4].Z0_Code);
			}

			AssertEquals(5, collection.Count);
			AssertEquals("001", collection[0].Z0_Code);
			AssertEquals("005", collection[1].Z0_Code);
			AssertEquals("007", collection[2].Z0_Code);
			AssertEquals("008", collection[3].Z0_Code);
			AssertEquals("009", collection[4].Z0_Code);
		}

		public void TestReorder_WithDateTimeOffset()
		{
			//sorting should be done by respective UTCs

			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "001";
			dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 02, 02, 02, 40, 50, TimeSpan.Zero);
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "003";
			dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 02, 02, 03, 40, 50, TimeSpan.Zero).KeepUtcChangeOffset(TimeSpan.FromHours(10));
			var dummy3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Code = "005";
			dummy3.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 02, 02, 04, 40, 50, TimeSpan.Zero).KeepUtcChangeOffset(TimeSpan.FromHours(-10));
			var dummy4 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy4.Z0_Code = "007";
			dummy4.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 02, 02, 05, 40, 50, TimeSpan.Zero).KeepUtcChangeOffset(TimeSpan.FromHours(1));
			var dummy5 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy5.Z0_Code = "009";
			dummy5.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 02, 02, 06, 40, 50, TimeSpan.Zero).KeepUtcChangeOffset(TimeSpan.FromHours(-1));

			var collection = GetCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "00"));
			collection.ApplySort(DummyBizoSchema.Constants.Z0_DateTimeOffset, ListSortDirection.Ascending);

			//relying on equality also being based on UTC time

			AssertEquals(5, collection.Count);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 02, 40, 50, TimeSpan.Zero), collection[0].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 03, 40, 50, TimeSpan.Zero), collection[1].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 04, 40, 50, TimeSpan.Zero), collection[2].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 05, 40, 50, TimeSpan.Zero), collection[3].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 06, 40, 50, TimeSpan.Zero), collection[4].Z0_DateTimeOffset);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 02, 02, 05, 50, 50, TimeSpan.Zero).KeepUtcChangeOffset(TimeSpan.FromHours(8));

				AssertEquals(5, collection.Count);
				AssertEquals(new ZDateTimeOffset(2000, 02, 02, 02, 40, 50, TimeSpan.Zero), collection[0].Z0_DateTimeOffset);
				AssertEquals(new ZDateTimeOffset(2000, 02, 02, 04, 40, 50, TimeSpan.Zero), collection[1].Z0_DateTimeOffset);
				AssertEquals(new ZDateTimeOffset(2000, 02, 02, 05, 40, 50, TimeSpan.Zero), collection[2].Z0_DateTimeOffset);
				AssertEquals(new ZDateTimeOffset(2000, 02, 02, 05, 50, 50, TimeSpan.Zero), collection[3].Z0_DateTimeOffset);
				AssertEquals(new ZDateTimeOffset(2000, 02, 02, 06, 40, 50, TimeSpan.Zero), collection[4].Z0_DateTimeOffset);
			}

			AssertEquals(5, collection.Count);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 02, 40, 50, TimeSpan.Zero), collection[0].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 04, 40, 50, TimeSpan.Zero), collection[1].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 05, 40, 50, TimeSpan.Zero), collection[2].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 05, 50, 50, TimeSpan.Zero), collection[3].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 02, 02, 06, 40, 50, TimeSpan.Zero), collection[4].Z0_DateTimeOffset);
		}

		public virtual void TestHitDbOnceWhenUsingDbOnlyAdditionalFilterWithRelationshipFilter()
		{
			for (var i = 0; i < 5; i++)
			{
				var bizObjA1 = Factory.NewWithValidTestData<DummyBusinessObject>();
				bizObjA1.Z0_Description = "AAA";
				bizObjA1.Z0_Number = 1;

				var bizObjA2 = Factory.NewWithValidTestData<DummyBusinessObject>();
				bizObjA2.Z0_Description = "AAA";
				bizObjA2.Z0_Number = 2;

				var bizObjB1 = Factory.NewWithValidTestData<DummyBusinessObject>();
				bizObjB1.Z0_Description = "BBB";
				bizObjB1.Z0_Number = 1;
			}
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var collection = GetCollection<DummyBusinessObject>(anotherFactory, new ZQuery(DummyBizoSchema.Z0_Description, "AAA"));

			AssertEquals(10, collection.Count);

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(DummyBizoSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, anotherFactory);

			anotherFactory.ResetDatabaseLoadCount();

			var dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Number, 1);
			collection.AdditionalFilter = dbOnlyQuery;

			AssertEquals(5, collection.Count);

			expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(DummyBizoSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		public virtual void TestCollectionCountChange()
		{
			DependentCollection.AddNew();
			DependentCollection.AddNew();
			var d3 = DependentCollection.AddNew();
			DependentCollection.AddNew();

			var d5 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();

			CollectionCountChangedEventArgs args = null;
			var countChanged = false;

			DependentCollection.CollectionCountChange += (sender, e) =>
			{
				args = e;
			};

			DependentCollection.CountChanged += (sender1, e1) =>
			{
				countChanged = true;
			};

			Assert(DependentCollection.Contains(d3));

			d3.ZD1_Z0 = ZGuid.Empty;
			Assert(!DependentCollection.Contains(d3));
			Assert(countChanged);
			AssertEquals(d3, args.BizObject);

			countChanged = false;

			d5.ZD1_Z0 = Master.PK;
			Assert(DependentCollection.Contains(d5));
			Assert(countChanged);
			AssertEquals(d5, args.BizObject);
		}

		[ExpectNoExceptions]
		public void TestUnhookBusinessObject_CausingListChangedDuringEnumeration_DoesNotCauseCollectionModifiedException()
		{
			var collection = GetCollection<DummyWithDependentsBusinessObject>(Factory);
			var dummy1 = collection.AddNew();
			dummy1.Z0_Decimal = 1m;
			collection.CountChanged += delegate
			{ };
			collection.SetReadOnlyIncludingChildren(true);
			collection.ApplySort(DummyWithDependentsBusinessObject.Schema.Z0_Decimal, ListSortDirection.Descending);

			var dummy2 = collection.AddNew();
			((IBindingList)dummy2.Dependents).ListChanged += delegate
			{
				// causing the list to be re-ordered due to the applied sort
				dummy2.Z0_Decimal = 43.41m;
			};

			collection.IndexExposed.Dispose();
		}

		protected class DummyDependantBusinessObjectMatchesFilter : DummyDependantBusinessObject
		{
			public DummyDependantBusinessObjectMatchesFilter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool MatchesFilterCore(ZQuery filter, DataRow row, DataTable table, string tableName, string identifier)
			{
				return true;
			}
		}

		public virtual void TestHandleBusinessObjectElementChanged()
		{
			try
			{
				var item = Factory.New<DummyDependantBusinessObjectMatchesFilter>();
				var listChangedType = ListChangedType.Reset;
				int oldIndex = -1;
				int newIndex = -1;
				bool dontBubbleEvent = false;
				var method = Index.GetType().GetMethod("HandleBusinessObjectElementChanged", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(Index, new object[] { item, listChangedType, oldIndex, newIndex, dontBubbleEvent });

				var expectedMessage = string.Format(
					@"A change event Reset in DummyDependantBusinessObjectMatchesFilter with PK '{0}' (IsInDatabase=False) has invoked method ABOCI<DummyDependantBusinessObject>.HandleBusinessObjectElementChanged() while the business object does not exists in the inner list of the index.
Inner list count = 0, list was null = True, SortComparer = null, IsDisposed = False.
Active owner = CargoWise.EntityFramework.ActiveBusinessObjectCollection`1[[CargoWise.EntityFramework.Testing.DummyDependantBusinessObject, CargoWise.EntityFramework",
					item.PK);

				var actualMessage = ErrorReporter.LastMessageReported;
				var versionIndex = actualMessage.IndexOf(", Version=");
				if (versionIndex > 0)
				{
					actualMessage = actualMessage.Substring(0, versionIndex);
				}

				AssertEquals(expectedMessage, actualMessage);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public virtual void TestAddNew()
		{
			DummyDependantBusinessObject item = DependentCollection.AddNew();
			AssertEquals("Foreign key set", Master.PK, item.ZD1_Z0);
		}

		public void TestAddNew_BizoCanBeLoaded()
		{
			DummyBusinessObject[] bizos = null;
			var code = "SNG";
			var dummyCollection = new DummyBizoCollectionWithCodeFilter(Factory, code);
			var dummyCollection2 = new DummyBizoCollectionWithCodeFilter(Factory, code);

			dummyCollection2.CountChanged += (s, e) =>
				{
					if (bizos == null)
					{
						bizos = Factory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, code)).ToArray();
					}
				};

			var dummy1 = dummyCollection.AddNew();
			AssertEquals("Initially, only one bizo", 1, bizos.Length);

			bizos = null;
			dummyCollection2.AddAnExtraToTriggerCountChangedEvent = true;
			var dummy2 = dummyCollection.AddNew();

			AssertEquals("An arbitrary action has caused an extra bizo to be added to the collection during AddNew", 3, bizos.Length);
			AssertCollectionContains(dummy1, bizos);
			AssertCollectionContains("Dummy 2 is in the row factory before the second Factory.Load is triggered.", dummy2, bizos);
		}

		#region DummyDependantCollectionWithCodeFilter

		class DummyBizoCollectionWithCodeFilter : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public DummyBizoCollectionWithCodeFilter(BusinessObjectFactory factory, string code)
				: base(factory, GetFilter(code))
			{
				this.code = code;
			}

			readonly string code;

			public bool AddAnExtraToTriggerCountChangedEvent { get; set; }

			protected override void SetDefaultsForNewElementCore(DummyBusinessObject newElement)
			{
				base.SetDefaultsForNewElementCore(newElement);
				newElement.Z0_Code = code;

				if (AddAnExtraToTriggerCountChangedEvent)
				{
					AddAnExtraToTriggerCountChangedEvent = false;
					AddNew();
				}
			}

			static ZQuery GetFilter(string code)
			{
				return new ZQuery(DummyBizoSchema.Z0_Code, code);
			}
		}

		#endregion

		public virtual void TestAddNewAndCancelNoException()
		{
			((IBindingList)Collection).ListChanged +=
				(o, e) =>
				{
					if (e.ListChangedType == ListChangedType.ItemAdded && Collection.IsNonCommittedElement(Collection[e.NewIndex]))
					{
						((ICancelAddNew)Collection).CancelNew(e.NewIndex);

						// Set Collection.Index.hasChanges = true
						((IBusinessObjectCollectionInternals)Collection).HasChangesFromDelete = false;
						((IBusinessObjectCollectionInternals)Collection).HasChangesFromDelete = true;
					}
				};

			AssertNoExceptionThrown(() => ((IBindingList)Collection).AddNew());
		}

		[ExpectNoExceptions]
		public virtual void TestIndexNotCorruptedWithListChangedLogic()
		{
			var collection1 = DependentCollection.GetSubCollectionForTestPurposesOnly(new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "1"));
			var collection2 = DependentCollection.GetSubCollectionForTestPurposesOnly(new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "2"));
			collection1.ApplySort(DummyDependentBizoSchema.ZD1_Code.Name, ListSortDirection.Ascending);
			collection2.ApplySort(DummyDependentBizoSchema.ZD1_Code.Name, ListSortDirection.Ascending);
			int i = 0;
			((IBindingList)DependentCollection).ListChanged += delegate
			{
				if (DependentCollection.Count < 100)
				{
					collection1.AddNew().ZD1_Code = (i++).ToString();
					collection2.AddNew().ZD1_Code = (i / 2).ToString();
					if (collection1.Count == 40)
					{
						((IList)collection1).RemoveAt(39);
					}
				}
			};
			DummyDependantBusinessObject item = DependentCollection.AddNew();
		}

		public virtual void TestAddingUncommittedItem_WhenCommittingWithChanges()
		{
			DummyDependantBusinessObject item = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertEquals("IsNonCommittedElement after AddNewUncommitted()", true, DependentCollection.IsNonCommittedElement(item));

			item.ZD1_Code = "x";
			((ICancelAddNew)DependentCollection).EndNew(DependentCollection.Count - 1);
			AssertEquals("IsNonCommittedElement after NotifyUncommittedEndEdit()", false, DependentCollection.IsNonCommittedElement(item));
			AssertEquals("Item not deleted because no changes were made", false, item.IsDeleted);
		}

		public virtual void TestAddingUncommittedItem_WhenCancelling()
		{
			DummyDependantBusinessObject item = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertEquals("IsNonCommittedElement after AddNewUncommitted()", true, DependentCollection.IsNonCommittedElement(item));

			((ICancelAddNew)DependentCollection).CancelNew(DependentCollection.Count - 1);
			AssertEquals("Item deleted", true, item.IsDeleted);
		}

		public virtual void TestAddingUncommittedItem_WhenInvalidatingCacheAndCommitting()
		{
			DummyDependantBusinessObject item = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			Index.InvalidateCache();

			//Uses reflection to access List.Contains method as ActiveBusinessObjectIndex.List is private and can't be exposed
			PropertyInfo info = Index.GetType().GetProperty("List", BindingFlags.NonPublic | BindingFlags.Instance);
			var businessObjectList = info.GetValue(Index, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
			AssertNotNull(businessObjectList);

			object isItemInTheList = businessObjectList.GetType().InvokeMember("Contains", BindingFlags.Default | BindingFlags.InvokeMethod, null, businessObjectList, new object[] { item });
			AssertEquals("Item should still be in the list of BusinessObject after InvalidateCache has been called", true, (bool)isItemInTheList);

			((ICancelAddNew)DependentCollection).EndNew(DependentCollection.Count - 1);
			object isItemInTheListAfterCommit = businessObjectList.GetType().InvokeMember("Contains", BindingFlags.Default | BindingFlags.InvokeMethod, null, businessObjectList, new object[] { item });
			AssertEquals("Item not deleted because no changes were made", false, item.IsDeleted);
			AssertEquals("Item should still be in the list of BusinessObject after Commit", true, (bool)isItemInTheListAfterCommit);
		}

		public virtual void TestAddingUncommittedItem_WhenInvalidatingCacheAndCancelling()
		{
			DummyDependantBusinessObject item = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			Index.InvalidateCache();

			//Uses reflection to access List.Contains method as ActiveBusinessObjectIndex.List is private and can't be exposed
			PropertyInfo info = Index.GetType().GetProperty("List", BindingFlags.NonPublic | BindingFlags.Instance);
			var businessObjectList = info.GetValue(Index, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
			AssertNotNull(businessObjectList);

			object isItemInTheList = businessObjectList.GetType().InvokeMember("Contains", BindingFlags.Default | BindingFlags.InvokeMethod, null, businessObjectList, new object[] { item });
			AssertEquals("Item should still be in the list of BusinessObject after InvalidateCache has been called", true, (bool)isItemInTheList);

			((ICancelAddNew)DependentCollection).CancelNew(DependentCollection.Count - 1);
			object isItemInTheListAfterCancel = businessObjectList.GetType().InvokeMember("Contains", BindingFlags.Default | BindingFlags.InvokeMethod, null, businessObjectList, new object[] { item });
			AssertEquals("Item deleted", true, item.IsDeleted);
			AssertEquals("Item should be removed from the list of BusinessObject after Cancel", false, (bool)isItemInTheListAfterCancel);
		}

		public void TestAddRange_OnManyToManyCollection()
		{
			int lastKnownCount = 0;
			((IBindingList)ManyToManyCollection).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				listChangedEvents.Add(e);
				lastKnownCount = ManyToManyCollection.Count;
			};

			List<DummyDependantBusinessObject> list = new List<DummyDependantBusinessObject>();
			list.Add(Factory.New<DummyDependantBusinessObject>());
			list.Add(Factory.New<DummyDependantBusinessObject>());
			list.Add(Factory.New<DummyDependantBusinessObject>());
			list.Add(Factory.New<DummyDependantBusinessObject>());

			ManyToManyCollection.AddRange(list);
			AssertEquals("The last event was raised when all items were added to the collection", 4, lastKnownCount);
			AssertEquals("2 events were added, the first Reset event, and the last raised Reset event", 2, listChangedEvents.Count);
			AssertEquals("The last event must be a Reset event", ListChangedType.Reset, listChangedEvents[listChangedEvents.Count - 1].ListChangedType);
		}

		public void TestBusinessObjectPropertyMovesPositionBecauseOfSort()
		{
			Collection.ApplySort(DummyBizoSchema.Z0_Code.Name, ListSortDirection.Ascending);
			DummyBusinessObject element1 = Collection.AddNew();
			DummyBusinessObject element2 = Collection.AddNew();

			element1.Z0_Code = "1";
			element2.Z0_Code = "2";

			AssertEquals("Collection elements initally", element1, Collection[0]);
			AssertEquals("Collection elements initally", element2, Collection[1]);

			element1.Z0_Code = "3";
			AssertEquals("Collection elements swapped", element1, Collection[1]);
			AssertEquals("Collection elements swapped", element2, Collection[0]);

			element1.Z0_Code = "0";
			AssertEquals("Collection elements swapped again", element1, Collection[0]);
			AssertEquals("Collection elements swapped again", element2, Collection[1]);
		}

		public virtual void TestNewBusinessObjectInSortedListChangingFilterToBeOutsideOfList()
		{
			PredicateFilter = (DummyBusinessObject element) => { return element.Z0_Code != "1"; };
			Collection.ApplySort(DummyBizoSchema.Z0_Code.Name, ListSortDirection.Ascending);

			DummyBusinessObject element1 = Collection.AddNew();
			DummyBusinessObject element2 = Collection.AddNew();
			element1.Z0_Code = "3";
			element2.Z0_Code = "4";
			DummyBusinessObject element3 = Collection.AddNew();
			element3.Z0_Code = "5";
			int loaded = Collection.Count;
			element3.Z0_Code = "1"; // this line reproduces the problem

			AssertEquals(2, Collection.Count);
			AssertEquals("3", Collection[0].Z0_Code);
			AssertEquals("4", Collection[1].Z0_Code);
		}

		public virtual void TestNewBusinessObjectInSortedListChangingFilterToBeOutsideOfList_DateTimeOffsetVersion()
		{
			PredicateFilter = (DummyBusinessObject element) => { return element.Z0_DateTimeOffset != new ZDateTimeOffset(2000, 1, 1, 2, 3, 4, TimeSpan.Zero); };
			Collection.ApplySort(DummyBizoSchema.Z0_Code.Name, ListSortDirection.Ascending);

			DummyBusinessObject element1 = Collection.AddNew();
			DummyBusinessObject element2 = Collection.AddNew();
			element1.Z0_Code = "3";
			element1.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 2, 3, 4, TimeSpan.FromHours(1));
			element2.Z0_Code = "4";
			element2.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 2, 3, 4, TimeSpan.FromHours(-1));
			DummyBusinessObject element3 = Collection.AddNew();
			element3.Z0_Code = "5";
			element3.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 2, 3, 4, TimeSpan.FromHours(2));
			int loaded = Collection.Count;
			element3.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 2, 3, 4, TimeSpan.Zero);

			AssertEquals(2, Collection.Count);
			AssertEquals(new ZDateTimeOffset(2000, 1, 1, 2, 3, 4, TimeSpan.FromHours(1)), Collection[0].Z0_DateTimeOffset);
			AssertEquals(new ZDateTimeOffset(2000, 1, 1, 2, 3, 4, TimeSpan.FromHours(-1)), Collection[1].Z0_DateTimeOffset);
		}

		public virtual void TestIsNonCommittedElement()
		{
			DummyDependantBusinessObject committedItem = DependentCollection.AddNew();
			DummyDependantBusinessObject uncommittedItem = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertEquals(false, DependentCollection.IsNonCommittedElement(committedItem));
			AssertEquals(true, DependentCollection.IsNonCommittedElement(uncommittedItem));
		}

		public virtual void TestToArray()
		{
			DummyDependantBusinessObject item1 = DependentCollection.AddNew();
			DummyDependantBusinessObject item2 = DependentCollection.AddNew();
			AssertEquals("Length", 2, ((IBusinessObjectCollection)DependentCollection).ToArray().Length);
			AssertCollectionContains("Item1", item1, ((IBusinessObjectCollection)DependentCollection).ToArray());
			AssertCollectionContains("Item2", item2, ((IBusinessObjectCollection)DependentCollection).ToArray());
		}

		public virtual void TestFindByPK()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			DummyDependantBusinessObject detachedElement = DependentCollection.AddNew();
			DependentCollection.RemoveFromRelationship(detachedElement);

			AssertEquals(element1, ((IBusinessObjectCollection)DependentCollection).FindByPK(element1.PK));
			AssertEquals(element2, ((IBusinessObjectCollection)DependentCollection).FindByPK(element2.PK));
			AssertEquals(null, ((IBusinessObjectCollection)DependentCollection).FindByPK(detachedElement.PK));
		}

		public virtual void TestBusinessObjectActiveFilter()
		{
			ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter> collection = GetCollection<DummyBusinessObjectWithActiveFilter>(Factory);
			DummyBusinessObjectWithActiveFilter element1 = collection.AddNew();
			DummyBusinessObjectWithActiveFilter element2 = collection.AddNew();
			element1.Z0_Bool = true;
			element2.Z0_Bool = false;
			AssertEquals("Active filter produces 1 element", 1, collection.Count);
			AssertEquals("Active filter produces 1 element", element1, collection[0]);
		}

		public virtual void TestIgnoreActiveFilter()
		{
			ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter> collection = GetCollection<DummyBusinessObjectWithActiveFilter>(Factory);
			ZQuery query = new ZQuery();
			query.IgnoreActiveFilter = true;
			collection.AdditionalFilter = query;

			DummyBusinessObjectWithActiveFilter element1 = collection.AddNew();
			DummyBusinessObjectWithActiveFilter element2 = collection.AddNew();
			element1.Z0_Bool = true;
			element2.Z0_Bool = false;
			AssertEquals("Active filter ignored", 2, collection.Count);
		}

		public void TestNotificationsChangedNotCalledDuringDeleteWhenSuspended()
		{
			bool notificationsChangedFired = false;
			ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter> collection = GetCollection<DummyBusinessObjectWithActiveFilter>(Factory);
			collection.AddNew();
			((IBusinessObjectCollection)collection).NotificationsChanged += delegate
			{ notificationsChangedFired = true; };
			((IBusinessObjectCollection)collection).SuspendValidation();
			collection.DeleteAll();
			AssertEquals("NotificationsChanged not fired when validation suspended", false, notificationsChangedFired);
		}

		[ExpectNoExceptions]
		public void TestInvalidationOfCollectionDoesntCauseEnumerationError()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = GetCollection<DummyBusinessObject>(Factory);
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			foreach (DummyBusinessObject item in collection)
			{
				((IActiveBusinessObjectCollection)collection).Refresh();
				int loaded = collection.Count;
			}
		}

		public void TestWithFreezeSortOnElementModifyComparerSort()
		{
			var item1 = Collection.AddNew();
			var item2 = Collection.AddNew();
			var item3 = Collection.AddNew();
			item1.Z0_VarCharMax = "3";
			item2.Z0_VarCharMax = "2";
			item3.Z0_VarCharMax = "1";

			PropertyDescriptor property = TypeDescriptor.GetProperties(item1)[DummyBizoSchema.Z0_VarCharMax.Name];
			Collection.ApplySort(FreezeSortOnElementModifyComparer.Wrap(new PropertyComparer(property, ListSortDirection.Ascending)));
			AssertEquals("Sort order initially", "1", Collection[0].Z0_VarCharMax);
			AssertEquals("Sort order initially", "2", Collection[1].Z0_VarCharMax);
			AssertEquals("Sort order initially", "3", Collection[2].Z0_VarCharMax);

			Collection[1].Z0_VarCharMax = "4";
			AssertEquals("Sort order is now frozen", "1", Collection[0].Z0_VarCharMax);
			AssertEquals("Sort order is now frozen", "4", Collection[1].Z0_VarCharMax);
			AssertEquals("Sort order is now frozen", "3", Collection[2].Z0_VarCharMax);

			Collection.ApplySort(FreezeSortOnElementModifyComparer.Wrap(new PropertyComparer(property, ListSortDirection.Descending)));
			AssertEquals("Sort order should re-establish from sort", "4", Collection[0].Z0_VarCharMax);
			AssertEquals("Sort order should re-establish from sort", "3", Collection[1].Z0_VarCharMax);
			AssertEquals("Sort order should re-establish from sort", "1", Collection[2].Z0_VarCharMax);

			Collection.ApplySort(FreezeSortOnElementModifyComparer.Wrap(new PropertyComparer(property, ListSortDirection.Ascending)));
			AssertEquals("Sort order should re-establish from sort", "1", Collection[0].Z0_VarCharMax);
			AssertEquals("Sort order should re-establish from sort", "3", Collection[1].Z0_VarCharMax);
			AssertEquals("Sort order should re-establish from sort", "4", Collection[2].Z0_VarCharMax);
		}

		public void TestWithFreezeSortOnElementModifyComparerSort_DateTimeOffsetVersion()
		{
			var item1 = Collection.AddNew();
			var item2 = Collection.AddNew();
			var item3 = Collection.AddNew();
			item1.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-1));
			item2.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(0));
			item3.Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(1));

			PropertyDescriptor property = TypeDescriptor.GetProperties(item1)[DummyBizoSchema.Z0_DateTimeOffset.Name];
			Collection.ApplySort(FreezeSortOnElementModifyComparer.Wrap(new PropertyComparer(property, ListSortDirection.Ascending)));
			AssertEquals("Sort order initially", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(1)), Collection[0].Z0_DateTimeOffset);
			AssertEquals("Sort order initially", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(0)), Collection[1].Z0_DateTimeOffset);
			AssertEquals("Sort order initially", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-1)), Collection[2].Z0_DateTimeOffset);

			Collection[1].Z0_DateTimeOffset = new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-2));
			AssertEquals("Sort order is now frozen", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(1)), Collection[0].Z0_DateTimeOffset);
			AssertEquals("Sort order is now frozen", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-2)), Collection[1].Z0_DateTimeOffset);
			AssertEquals("Sort order is now frozen", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-1)), Collection[2].Z0_DateTimeOffset);

			Collection.ApplySort(FreezeSortOnElementModifyComparer.Wrap(new PropertyComparer(property, ListSortDirection.Descending)));
			AssertEquals("Sort order should re-establish from sort", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-2)), Collection[0].Z0_DateTimeOffset);
			AssertEquals("Sort order should re-establish from sort", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-1)), Collection[1].Z0_DateTimeOffset);
			AssertEquals("Sort order should re-establish from sort", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(1)), Collection[2].Z0_DateTimeOffset);

			Collection.ApplySort(FreezeSortOnElementModifyComparer.Wrap(new PropertyComparer(property, ListSortDirection.Ascending)));
			AssertEquals("Sort order should re-establish from sort", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(1)), Collection[0].Z0_DateTimeOffset);
			AssertEquals("Sort order should re-establish from sort", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-1)), Collection[1].Z0_DateTimeOffset);
			AssertEquals("Sort order should re-establish from sort", new ZDateTimeOffset(2000, 1, 1, 1, 2, 3, TimeSpan.FromHours(-2)), Collection[2].Z0_DateTimeOffset);
		}

		[ExpectNoExceptions]
		public void TestIncrementReadOnlyIncludingChildren_DuringWhichIndexDisposeHappens()
		{
			Collection.AddNew();
			Collection.AddNew();
			((IBindingList)Collection[0]).ListChanged += delegate
			{ ((IDisposable)((IActiveBusinessObjectCollection)Collection).Index).Dispose(); };
			((IActiveBusinessObjectCollection)Collection).IncrementReadOnlyIncludingChildren();
		}

		public virtual void TestDbOnlyQueryRefreshesProperly()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			DummyWithDependentsBusinessObject dummy = factory1.New<DummyWithDependentsBusinessObject>();
			factory1.Save();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			query.AddSubQuery(subQuery, JoinCondition.Or);

			var dummyCollection1 = GetCollection<DummyBusinessObject>(factory1, query);
			int count = dummyCollection1.Count; // loaded
			var dummyCollection2 = GetCollection<DummyBusinessObject>(factory2, query);
			int count2 = dummyCollection2.Count; // loaded

			DummyDependantBusinessObject dependent = dummy.Dependents.AddNew();
			factory1.Save();
			ActiveBusinessObjectCollection.RefreshAll(factory2);
			AssertEquals(1, dummyCollection2.Count);
			AssertEquals(dummy.PK, dummyCollection2[0].PK);
		}

		public void TestIsEnumeratingShoudBeTrueWhileDoingEnumeration()
		{
			DependentCollection.AddNew();
			DependentCollection.AddNew();
			foreach (var item1 in DependentCollection)
			{
				Assert("IsEnumerating should be true while doing enumeration", DependentCollection.IndexExposed.List.IsEnumerating);
				foreach (var item2 in DependentCollection)
				{
					Assert("IsEnumerating should be true while doing enumeration", DependentCollection.IndexExposed.List.IsEnumerating);
				}
				Assert("IsEnumerating should be true while doing enumeration", DependentCollection.IndexExposed.List.IsEnumerating);
			}
			Assert("IsEnumerating should be false while finished enumeration", !DependentCollection.IndexExposed.List.IsEnumerating);
		}

		#endregion

		#region HasChanges

		public virtual void TestHasChangesDoesNotHitDb()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "ZZ1";
			dummy1.Z0_Number = 1;

			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "ZZ2";
			dummy2.Z0_Number = 2;

			Factory.Save();

			var initialHitCount = Factory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value;

			var filter = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "Z");
			var collection = GetCollection<DummyBusinessObject>(Factory, filter);
			Assert("Precondition - collection is not yet loaded", !collection.IndexExposed.IsLoaded);
			Assert("Precondition - hasChanges is not yet calculated", !collection.IndexExposed.HasChangesFieldExposedForTest.HasValue);

			Assert("HasChanges should be calculated properly", !((IBusiness)collection).HasChanges);
			Assert("hasChanges field should be reset back to null", !collection.IndexExposed.HasChangesFieldExposedForTest.HasValue);

			var newHitCount = Factory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value - initialHitCount;
			AssertEquals(string.Format("There should be no db hits to {0} to check HasChanges in non-populated collection.", DummyBizoSchema.Constants.TableName), 0, newHitCount);

			dummy1.Z0_Number = 3; // There may be db hits here

			initialHitCount = Factory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value;

			// Use new collection with new index
			filter = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "ZZ");
			collection = GetCollection<DummyBusinessObject>(Factory, filter);
			Assert("Precondition - collection is not yet loaded", !collection.IndexExposed.IsLoaded);

			Assert("HasChanges should be calculated properly", ((IBusiness)collection).HasChanges);

			newHitCount = Factory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value - initialHitCount;
			AssertEquals(string.Format("There should be no db hits to {0} to check HasChanges in non-populated collection.", DummyBizoSchema.Constants.TableName), 0, newHitCount);
		}

		public virtual void TestLoadAfterHasChangesCheck()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "ZZ1";
			dummy1.Z0_Number = 1;

			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "ZZ2";
			dummy2.Z0_Number = 2;

			Factory.Save();

			var dummy3 = new BusinessObjectFactory { RefreshEnabled = false }.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Code = "ZZ3";
			dummy3.Z0_Number = 3;
			dummy3.Factory.Save();

			var filter = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "Z");
			var collection = GetCollection<DummyBusinessObject>(Factory, filter);
			Assert("Precondition - collection is not yet loaded", !collection.IndexExposed.IsLoaded);
			Assert("Precondition - hasChanges is not yet calculated", !collection.IndexExposed.HasChangesFieldExposedForTest.HasValue);

			Assert("HasChanges should be calculated properly", !((IBusiness)collection).HasChanges); // Call HasChanges
			Assert("hasChanges field should be reset back to null", !collection.IndexExposed.HasChangesFieldExposedForTest.HasValue);

			var checkFilter = new ZQuery(filter) { FetchOnlyFromLocalCache = true };
			AssertEquals("Should only have local data", 2, Factory.Load<DummyBusinessObject>(checkFilter).Length);

			AssertEquals("Should load all data from db", 3, collection.Count);
			AssertEquals("Should load all data from db", 3, Factory.Load<DummyBusinessObject>(checkFilter).Length);
		}

		#endregion

		#region TestNonStableIndexCacheKeyIndex

		[ExpectNoExceptions]
		public void TestNonStableIndexCacheKeyIndex()
		{
			DummyBusinessObject bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_Code = "XYZ";
			Factory.Save();

			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new NonStableTestRelationship(typeof(DummyBusinessObject)));

			collection.SetReadOnlyIncludingChildren(true);
			AssertEquals(1, collection.Count);

			collection.ApplySort("Z0_Code", ListSortDirection.Descending);
			AssertEquals(1, collection.Count);
		}

		class NonStableTestRelationship : CollectionRelationship
		{
			public NonStableTestRelationship(Type elementType) : base(elementType) { }

			public override int GetHashCode()
			{
				return Environment.TickCount;
			}
		}

		#endregion

		#region OnLoadedIntoCollection

		public virtual void TestOnLoadedIntoCollection()
		{
			CollectionForOnLoadedTesting collection = GetCollectionForOnLoaded(Factory);
			AssertEquals("collection.OnLoadedIntoParentIndexCoreCalled", 0, collection.OnLoadedIntoCollectionCoreCalled);
			AssertEquals("collection.LastLoadedObject", null, collection.LastLoadedObject);
			DummyBusinessObject bo1 = collection.AddNew();
			bo1.FillWithValidTestData();
			AssertEquals("collection.OnLoadedIntoParentIndexCoreCalled", 1, collection.OnLoadedIntoCollectionCoreCalled);
			AssertEquals("collection.LastLoadedObject", bo1, collection.LastLoadedObject);
			collection.OnLoadedIntoCollectionCoreCalled = 0;
			collection.LastLoadedObject = null;
			Factory.Save();
			AssertEquals("collection.OnLoadedIntoParentIndexCoreCalled", 0, collection.OnLoadedIntoCollectionCoreCalled);
			AssertEquals("collection.LastLoadedObject", null, collection.LastLoadedObject);

			CollectionForOnLoadedTesting collection1 = GetCollectionForOnLoaded(Factory);
			AssertEquals("collection1.Count - touching should trigger load and pick up object from other collection.", 1, collection1.Count);
			AssertEquals("collection1.OnLoadedIntoParentIndexCoreCalled", 0, collection1.OnLoadedIntoCollectionCoreCalled);
			AssertEquals("collection1.LastLoadedObject", null, collection1.LastLoadedObject);

			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CollectionForOnLoadedTesting collection2 = GetCollectionForOnLoaded(factory2);
			AssertEquals("collection2.Count - touching should trigger load and pick up object from other collection.", 1, collection2.Count);
			AssertEquals("collection2.OnLoadedIntoParentIndexCoreCalled", 1, collection2.OnLoadedIntoCollectionCoreCalled);
			AssertEquals("collection2.LastLoadedObject", bo1.PK, collection2.LastLoadedObject.PK);

			DummyBusinessObject bo2 = factory2.New<DummyBusinessObject>();
			AssertEquals("collection2.Count - matching element created in same factory should automatically get added to the collection.", 2, collection2.Count);
			AssertEquals("collection2.OnLoadedIntoParentIndexCoreCalled", 2, collection2.OnLoadedIntoCollectionCoreCalled);
			AssertEquals("collection2.LastLoadedObject", bo2.PK, collection2.LastLoadedObject.PK);
		}

		public virtual void TestOnLoadedIntoCollection_ForAddNew()
		{
			var collection = GetCollectionForOnLoaded(Factory);
			DummyBusinessObject dummy = collection.AddNew();
			AssertEquals("Not loaded for the test", false, ((IActiveBusinessObjectCollection)collection).IsLoaded);
			AssertEquals("Called once for AddNew() while collection isn't loaded", 1, collection.OnLoadedIntoCollectionCoreCalled);
			collection.OnLoadedIntoCollectionCoreCalled = 0;

			int loaded = collection.Count;
			collection.OnLoadedIntoCollectionCoreCalled = 0;
			dummy = collection.AddNew();
			AssertEquals("Called once for AddNew() while collection is loaded", 1, collection.OnLoadedIntoCollectionCoreCalled);
		}

		#endregion

		#region ListChanged event unsorted

		public virtual void TestListChanged_ItemAddedToEnd()
		{
			DummyDependantBusinessObject dummy1 = DependentCollection.AddNew();
			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);

			DummyDependantBusinessObject dummy2 = DependentCollection.AddNew();
			AssertListChanged(ListChangedType.ItemAdded, -1, 1, 1);
		}

		public virtual void TestListChanged_ItemAddedToMiddle()
		{
			DummyDependantBusinessObject dummy1 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy3 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals(0, DependentCollection.Count);
			DependentCollection.Add(dummy1);
			DependentCollection.Add(dummy3);

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			DependentCollection.Add(dummy2);
			AssertListChanged(ListChangedType.ItemAdded, -1, 2, 1);
		}

		public virtual void TestListChanged_ItemDeleted()
		{
			DummyDependantBusinessObject dummy1 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy3 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals(0, DependentCollection.Count);
			DependentCollection.Add(dummy1);
			DependentCollection.Add(dummy2);
			DependentCollection.Add(dummy3);

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			dummy2.Delete();
			AssertListChanged(ListChangedType.ItemDeleted, -1, 1, 1);
		}

		public virtual void TestListChanged_ItemDeleted_WhenObjectGoesOutOfScopeOfFilter()
		{
			Filter = new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "EXL");
			DummyDependantBusinessObject dummy1 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy3 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals(0, DependentCollection.Count);
			DependentCollection.Add(dummy1);
			DependentCollection.Add(dummy2);
			DependentCollection.Add(dummy3);

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			dummy2.ZD1_Code = "EXL";
			AssertListChanged(ListChangedType.ItemDeleted, -1, 1, 1);
		}

		public virtual void TestListChanged_ItemChanged()
		{
			DummyDependantBusinessObject dummy1 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy2 = Factory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject dummy3 = Factory.New<DummyDependantBusinessObject>();
			AssertEquals(0, DependentCollection.Count);
			DependentCollection.Add(dummy1);
			DependentCollection.Add(dummy2);
			DependentCollection.Add(dummy3);

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			dummy2.Delete();
			AssertListChanged(ListChangedType.ItemDeleted, -1, 1, 1);
		}

		#endregion

		#region ListChanged event with sort

		public virtual void TestListChanged_SortedItemAddedToMiddle()
		{
			IComparer comparer = new PropertyComparer(TypeDescriptor.GetProperties(typeof(DummyDependantBusinessObject))[DummyDependentBizoSchema.ZD1_Code.Name], ListSortDirection.Ascending);
			DependentCollection.ApplySort(comparer);

			DummyDependantBusinessObject dummy2 = DependentCollection.AddNew();
			dummy2.ZD1_Code = "2";

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			DummyDependantBusinessObject dummy = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			dummy.ZD1_Code = "1";
			AssertListChanged(ListChangedType.ItemAdded, -1, 1, 2);

			((ICancelAddNew)DependentCollection).EndNew(DependentCollection.Count - 1);
			AssertListChanged(ListChangedType.Reset);
		}

		public virtual void TestListChanged_SortedItemsAddedToMiddle()
		{
			var collection = GetCollection<DummyBusinessObject>(Factory, new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan, 0));
			collection.ApplySort(DummyBizoSchema.Constants.Z0_Code, ListSortDirection.Ascending);

			var collectionChangedCounter = 0;
			var codes = new List<string>();
			CollectionCountChangedEventHandler countChangeHandler =
				(s, e) =>
				{
					if (e != null && e.ItemAdded)
					{
						collectionChangedCounter++;
						var dummy = (DummyBusinessObject)e.BizObject;
						codes.Add(dummy.PK.ToString());
					}
				};
			collection.CollectionCountChange += countChangeHandler;

			AssertEquals(0, collection.Count);

			var bizo1 = Factory.New<DummyBusinessObject>();
			bizo1.Z0_Code = "A01";
			bizo1.Z0_Number = 1;

			var bizo2 = Factory.New<DummyBusinessObject>();
			bizo2.Z0_Code = "A03";
			bizo2.Z0_Number = 1;

			var bizo3 = Factory.New<DummyBusinessObject>();
			bizo3.Z0_Code = "A02";
			bizo3.Z0_Number = 1;

			AssertEquals(3, collection.Count);
			AssertEquals("A02", collection[1].Z0_Code);
			AssertEquals(3, collectionChangedCounter);
			AssertEquals(codes[0], collection[0].PK.ToString());
			AssertEquals(codes[1], collection[2].PK.ToString());
			AssertEquals(codes[2], collection[1].PK.ToString());
		}

		public virtual void TestListChanged_SortedItemAddedToEnd()
		{
			IComparer comparer = new PropertyComparer(TypeDescriptor.GetProperties(typeof(DummyDependantBusinessObject))[DummyDependentBizoSchema.ZD1_Code.Name], ListSortDirection.Ascending);
			DependentCollection.ApplySort(comparer);

			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			dummy.ZD1_Code = "1";

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			DummyDependantBusinessObject dummy2 = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			dummy2.ZD1_Code = "2";
			AssertListChanged(ListChangedType.ItemAdded, -1, 1, 2);

			((ICancelAddNew)DependentCollection).EndNew(DependentCollection.Count - 1);
			AssertListChanged(ListChangedType.ItemAdded, ListChangedType.ItemChanged);
		}

		public virtual void TestListChanged_SortedItemChangedOrder()
		{
			IComparer comparer = new PropertyComparer(TypeDescriptor.GetProperties(typeof(DummyDependantBusinessObject))[DummyDependentBizoSchema.ZD1_Code.Name], ListSortDirection.Ascending);
			DependentCollection.ApplySort(comparer);

			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			DummyDependantBusinessObject dummy2 = DependentCollection.AddNew();
			DummyDependantBusinessObject dummy3 = DependentCollection.AddNew();
			dummy.ZD1_Code = "10";
			dummy2.ZD1_Code = "20";
			dummy3.ZD1_Code = "30";

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			dummy.ZD1_Code = "25";
			AssertListChanged(ListChangedType.Reset);
		}

		public virtual void TestListChanged_SortedItemChangedOrder_FromDataRefreshBus()
		{
			IComparer comparer = new PropertyComparer(TypeDescriptor.GetProperties(typeof(DummyDependantBusinessObject))[DummyDependentBizoSchema.ZD1_Code.Name], ListSortDirection.Ascending);
			DependentCollection.ApplySort(comparer);

			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			DummyDependantBusinessObject dummy2 = DependentCollection.AddNew();
			DummyDependantBusinessObject dummy3 = DependentCollection.AddNew();
			dummy.ZD1_Code = "10";
			dummy2.ZD1_Code = "20";
			dummy3.ZD1_Code = "30";
			Factory.Save();

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			dummy = new BusinessObjectFactory().Load<DummyDependantBusinessObject>(dummy.PK);
			dummy.ZD1_Code = "25";
			dummy.Factory.Save();
			AssertListChanged(ListChangedType.Reset);
		}

		public virtual void TestListChanged_SortedItemChanged()
		{
			IComparer comparer = new PropertyComparer(TypeDescriptor.GetProperties(typeof(DummyDependantBusinessObject))[DummyDependentBizoSchema.ZD1_Code.Name], ListSortDirection.Ascending);
			DependentCollection.ApplySort(comparer);

			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			DummyDependantBusinessObject dummy2 = DependentCollection.AddNew();
			DummyDependantBusinessObject dummy3 = DependentCollection.AddNew();
			dummy.ZD1_Code = "10";
			dummy2.ZD1_Code = "20";
			dummy3.ZD1_Code = "30";

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			dummy.ZD1_Code = "15";
			AssertListChanged(ListChangedType.Reset, -1, -1, 1);
		}

		#endregion

		#region IList

		public virtual void TestAddingImplicitly()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("Collection contains added item", 1, Collection.Count);
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			AssertEquals("Collection contains added item", 2, Collection.Count);
		}

		public virtual void TestAddingToRelationship()
		{
			DummyDependantBusinessObject dummy = Factory.New<DummyDependantBusinessObject>();
			AssertEquals("Not part of the relationship initially", ZGuid.Empty, dummy.ZD1_Z0);

			AssertEquals("No items in the collection initially", 0, DependentCollection.Count);
			DependentCollection.Add(dummy);
			AssertEquals("Added to the collection", 1, DependentCollection.Count);
			AssertEquals("Added to the collection", true, DependentCollection.Contains(dummy));
			AssertEquals("Added to the relationship by setting FK", Master.PK, dummy.ZD1_Z0);
		}

		public virtual void TestRemovingFromRelationship_ByRemove()
		{
			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			AssertEquals("Added to the collection", true, DependentCollection.Contains(dummy));

			DependentCollection.RemoveFromRelationship(dummy);
			AssertEquals("Removed from the relationship by setting the FK to empty", ZGuid.Empty, dummy.ZD1_Z0);
		}

		public virtual void TestRemoveAt_DeletesBusinessObject()
		{
			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			AssertEquals("Added to the collection", 0, DependentCollection.IndexOf(dummy));

			((IList)DependentCollection).RemoveAt(0);
			AssertEquals("RemoveAt() deletes the business object", true, dummy.IsDeleted);
		}

		public virtual void TestContains()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("Collection contains added item", true, Collection.Contains(dummy));
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			AssertEquals("Collection contains added item", true, Collection.Contains(dummy2));

			dummy.Delete();
			AssertEquals("Collection doesn't contain deleted item", false, Collection.Contains(dummy));
		}

		public void TestContains_WithMatchesFilterOverride()
		{
			PredicateFilter = (DummyBusinessObject element) => { return element.Z0_Description != "excluded"; };
			DummyBusinessObject dummy = Collection.AddNew();
			DummyBusinessObject excludedDummy = Collection.AddNew();
			excludedDummy.Z0_Description = "excluded";

			AssertEquals("Collection contain item", true, Collection.Contains(dummy));
			AssertEquals("Collection doesn't contain excluded item", false, Collection.Contains(excludedDummy));
		}

		public virtual void TestIndexOf()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			AssertEquals("Collection contains added item", 0, Collection.IndexOf(dummy));
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			AssertEquals("Collection contains added item", 0, Collection.IndexOf(dummy));
			AssertEquals("Collection contains added item", 1, Collection.IndexOf(dummy2));

			dummy.Delete();
			AssertEquals("Collection doesn't contain deleted item", 0, Collection.IndexOf(dummy2));
		}

		public virtual void TestCopyTo()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			BusinessObject[] array = new BusinessObject[3];
			((ICollection)Collection).CopyTo(array, 1);
			AssertEquals("Copied 1st item", dummy.PK, array[1].PK);
			AssertEquals("Copied 2nd item", dummy2.PK, array[2].PK);
		}

		#endregion

		#region IBusiness

		public void TestIsValidationSuspendedOnCollection()
		{
			var collectionIndex = GetCollection<DummyBusinessObject>(Factory).IndexExposed;
			AssertEquals("Precondition", false, collectionIndex.IsValidationSuspendedOnCollection);

			collectionIndex.ResumeValidation(); // suspendValidationIndex = 0, doesn't go below 0
			AssertEquals("When validation is not suspended ResumeValidation should do nothing.", false, collectionIndex.IsValidationSuspendedOnCollection);

			collectionIndex.SuspendValidation(); // suspendValidationIndex = 1
			AssertEquals("SuspendValidation should suspend the validation.", true, collectionIndex.IsValidationSuspendedOnCollection);

			collectionIndex.SuspendValidation(); // suspendValidationIndex = 2
			AssertEquals("SuspendValidation second time should increase suspension index.", true, collectionIndex.IsValidationSuspendedOnCollection);

			collectionIndex.ResumeValidation(); // suspendValidationIndex = 1
			AssertEquals("ResumeValidation should decrease suspension index.", true, collectionIndex.IsValidationSuspendedOnCollection);

			collectionIndex.ResumeValidation(); // suspendValidationIndex = 0
			AssertEquals("ResumeValidation should decrease suspension index and un-suspend validation.", false, collectionIndex.IsValidationSuspendedOnCollection);
		}

		#endregion

		#region TestCompleteFilterWithNullRelationshipFilter

		[ExpectNoExceptions]
		public void TestCompleteFilterWithNullRelationshipFilter()
		{
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new RelationshipWithNullRelationshipFilter());
			AssertNotNull(collection.CompleteFilter);
		}

		public class RelationshipWithNullRelationshipFilter : ICollectionRelationship
		{
			public ZQuery RelationshipFilter
			{
				get { return null; }
			}

			public event EventHandler RelationshipFilterChanged
			{
				add { }
				remove { }
			}

			public ICollectionRelationship AddFilter(ZQuery additionalFilter)
			{
				return this;
			}

			public bool MatchesRelationshipFilter(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
			{
				return false;
			}

			public BusinessObject[] LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter)
			{
				return Array.Empty<BusinessObject>();
			}

			public BusinessObject Master
			{
				get { return null; }
			}

			public bool HasChangesIncludingRelationship(BusinessObject businessObject)
			{
				return false;
			}

			public void ClearHasChangesIncludingRelationship(BusinessObject businessObject)
			{
			}

			public bool SupportsAddToRelationship()
			{
				return false;
			}

			public void AddToRelationship(BusinessObject businessObject)
			{
			}

			public void RemoveFromRelationship(BusinessObject businessObject)
			{
			}

			public void Clear()
			{
			}

			public IEnumerator GetDataEnumerator() => null;
		}

		#endregion

		#region Factory checking
		//[ExpectException(typeof(ArgumentException))]
		//public void TestAddWithWrongFactory()
		//{
		//  var secondFactory = new BusinessObjectFactory();
		//  var bizO = secondFactory.New<DummyBusinessObject>();
		//  Collection.Add(bizO);
		//}

		//[ExpectException(typeof(ArgumentException))]
		//public void TestContainsWithWrongFactory()
		//{
		//  var secondFactory = new BusinessObjectFactory();
		//  var bizO = secondFactory.New<DummyBusinessObject>();
		//  Collection.Contains(bizO);
		//}

		//[ExpectException(typeof(ArgumentException))]
		//public void TestIndexOfWithWrongFactory()
		//{
		//  var secondFactory = new BusinessObjectFactory();
		//  var bizO = secondFactory.New<DummyBusinessObject>();
		//  Collection.IndexOf(bizO);
		//}

		//[ExpectException(typeof(ArgumentException))]
		//public void TestDeleteWithWrongFactory()
		//{
		//  var secondFactory = new BusinessObjectFactory();
		//  var bizO = secondFactory.New<DummyBusinessObject>();
		//  Collection.Delete(bizO);
		//}

		#endregion

		#region OnListChanged / AssertListChanged

		readonly List<ListChangedEventArgs> listChangedEvents = new List<ListChangedEventArgs>();

		void OnListChanged(object sender, ListChangedEventArgs e)
		{
			listChangedEvents.Add(e);
		}

		void AssertListChanged(ListChangedType expectedEventType, int expectedOldIndex, int expectedNewIndex, int eventsCounts)
		{
			AssertEquals("Expected number of events", eventsCounts, listChangedEvents.Count);
			AssertEquals(expectedEventType, listChangedEvents[0].ListChangedType);
			AssertEquals(expectedOldIndex, listChangedEvents[0].OldIndex);
			AssertEquals(expectedNewIndex, listChangedEvents[0].NewIndex);
			listChangedEvents.Clear();
		}

		void AssertListChanged(params ListChangedType[] expectedEventTypes)
		{
			AssertEquals("Expected number of events", expectedEventTypes.Length, listChangedEvents.Count);
			for (int i = 0; i < expectedEventTypes.Length; i++)
			{
				AssertEquals("Change event " + i, expectedEventTypes[i], listChangedEvents[i].ListChangedType);
			}
			listChangedEvents.Clear();
		}

		#endregion

		#region TestAddNewUncommittedWithRelationshipDoesNotFiresResetEvent

		public void TestAddNewUncommittedWithRelationshipDoesNotFiresResetEvent()
		{
			var dummyCollection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new AdhocCollectionRelationship(typeof(DummyBusinessObject)));

			bool resetWasFired = false;
			((IBindingList)dummyCollection).ListChanged += (sender, e) => resetWasFired |= e.ListChangedType == ListChangedType.Reset;

			((IBindingList)dummyCollection).AddNew();
			((IBindingList)dummyCollection).AddNew();
			((IBindingList)dummyCollection).AddNew();

			Assert("Reset event should not be called during AddNewUncommitted() method", !resetWasFired);
		}

		#endregion

		#region TestRelationshipFilterChangeFiresResetEventForNonAdhocReltionship

		public void TestRelationshipFilterChangeFiresResetEventForNonAdhocReltionship()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();

			ActiveBusinessObjectCollection<DummyDependantBusinessObject> dummyCollection =
				new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory, new ManyToManyRelationship(dummy, typeof(DummyDependantBusinessObject), typeof(DummyPivot)));

			bool resetWasFired = false;
			((IBindingList)dummyCollection).ListChanged += (sender, e) => resetWasFired |= e.ListChangedType == ListChangedType.Reset;

			Assert("Reset event was not fired yet", !resetWasFired);

			((IBindingList)dummyCollection).AddNew();
			((IBindingList)dummyCollection).AddNew();
			((IBindingList)dummyCollection).AddNew();

			Assert("Reset event should be called if Relationship is no AdHoc (many to many in our case)", resetWasFired);
		}

		#endregion

		#region TestAddNewUpdatesInternalList

		public void TestAddNewUpdatesInternalListWhenEventsAreEnabled()
		{
			var collection = GetCollection<DummyBusinessObject>(Factory);
			AssertEquals(0, collection.Count);

			collection.AddNew();
			AssertEquals(1, collection.Count);
		}

		public void TestAddNewUpdatesInternalListWhenEventsAreDelayed()
		{
			var collection = GetCollection<DummyBusinessObject>(Factory);
			AssertEquals(0, collection.Count);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				collection.AddNew();
				AssertEquals(1, collection.Count);
			}
		}

		#endregion

		#region TestDeleteAllCallsFetchForDelete

		public virtual void TestDeleteAllCallsFetchForDelete()
		{
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var factory = new BusinessObjectFactory();
				for (int i = 0; i < 10; i++)
				{
					factory.New<DummyBizoForDeleteAllTest>();
				}
				factory.Save();

				ActiveBusinessObjectCollection<DummyBizoForDeleteAllTest> collection = GetCollection<DummyBizoForDeleteAllTest>(factory);
				AssertEquals(10, collection.Count);

				var initialHits = factory.GetTableHitCount(DummyDependentBizoSchema.Constants.TableName);
				int initialFetchHints = factory.GetLoadedFetchHintCountForTable(DummyDependentBizoSchema.Constants.TableName);
				int expectedFetchHints = initialFetchHints + collection.Count;

				collection.DeleteAll();

				AssertEquals("There should be only 1 new hit to DummyDependentBizo table", initialHits + 1, factory.GetTableHitCount(DummyDependentBizoSchema.Constants.TableName));
				AssertEquals(expectedFetchHints, factory.GetLoadedFetchHintCountForTable(DummyDependentBizoSchema.Constants.TableName));
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		class DummyBizoForDeleteAllTest : DummyBusinessObject
		{
			public DummyBizoForDeleteAllTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override void Delete()
			{
				Factory.Load<DummyDependantBusinessObject>(new ZQuery(DummyDependentBizoSchema.ZD1_Z0, PK));
			}

			protected override IBusinessObjectFetchStrategy GetFetchStrategy() => new BusinessObjectFetchStrategyForTest(this);
		}

		class BusinessObjectFetchStrategyForTest : BusinessObjectFetchStrategy
		{
			public BusinessObjectFetchStrategyForTest(BusinessObject businessObject) : base(businessObject) { }

			protected override void FetchForDeleteCore()
			{
				BusinessObject.Factory.AddFetchHint(DummyDependentBizoSchema.ZD1_Z0, BusinessObject.PK);
			}
		}

		#endregion

		#region TestDeletedElements

		[ExpectNoExceptions]
		public void TestDeletedElementIsNotReturnedInForeach()
		{
			var collection = GetCollection<DummyBusinessObject>(Factory);
			AssertEquals(0, collection.Count);

			var bizO2Delete = collection.AddNew();
			collection.AddNew();
			AssertEquals(2, collection.Count);
			collection[0].Z0_Description = "To be deleted";
			collection[1].Z0_Description = "Still here";

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				collection[0].Row.Delete();

				foreach (var bizO in collection)
				{
					AssertEquals("Still here", bizO.Z0_Description);
				}
			}
		}

		public void TestDeletedElementsAreNotReturnedInToArray()
		{
			var collection = GetCollection<DummyBusinessObject>(Factory);
			collection.AddNew().Z0_Code = "XX1";
			collection.AddNew().Z0_Code = "XX2";
			collection.AddNew().Z0_Code = "XX3";
			collection.AddNew().Z0_Code = "XX4";

			AssertEquals("Precondition", 4, collection.Count);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				AssertEquals(4, collection.Count);

				collection[1].Delete();
				AssertEquals(3, collection.Count);

				collection[2].Delete();
				AssertEquals(2, collection.Count);

				var array = collection.ToArray();

				AssertEquals("Only non-deleted items should be returned by ToArray()", 2, array.Length);
				AssertEquals("XX1", array[0].Z0_Code);
				AssertEquals("XX3", array[1].Z0_Code);
			}
		}

		#endregion

		#region TestDeleteWhileListChangedSuspended

		public void TestDeleteWhileListChangedSuspended()
		{
			var collection = GetCollection<DummyBusinessObject>(Factory);
			collection.AddNew().Z0_Code = "D01";
			collection.AddNew().Z0_Code = "D02";
			collection.AddNew().Z0_Code = "D03";

			AssertEquals(3, collection.Count);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				collection.Delete(collection[1]);

				AssertEquals(2, collection.Count);
				AssertEquals("D01", collection[0].Z0_Code);
				AssertEquals("D03", collection[1].Z0_Code);
			}

			AssertEquals("No extra elements removed", 2, collection.Count);
		}

		public void TestDeleteBizoWhileListChangedSuspended()
		{
			var collection = GetCollection<DummyBusinessObject>(Factory);
			collection.AddNew().Z0_Code = "D01";
			collection.AddNew().Z0_Code = "D02";
			collection.AddNew().Z0_Code = "D03";

			AssertEquals(3, collection.Count);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				collection[1].Delete();

				AssertEquals(2, collection.Count);
				AssertEquals("D01", collection[0].Z0_Code);
				AssertEquals("D03", collection[1].Z0_Code);
			}

			AssertEquals("No extra elements removed", 2, collection.Count);
		}

		public void TestDeleteWhileListChangedSuspendedNoNullReferenceOnListReset()
		{
			var collection = GetCollection<DummyWithListReset>(Factory);
			collection.AddNew().ParentList = collection;
			collection.AddNew().ParentList = collection;
			collection.AddNew().ParentList = collection;

			AssertEquals(3, collection.Count);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				AssertNoExceptionThrown(() => collection.Delete(collection[1]));
				AssertEquals(2, collection.Count);
			}

			AssertEquals("No extra elements removed", 2, collection.Count);
		}

		class DummyWithListReset : DummyBusinessObject
		{
			public DummyWithListReset(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ActiveBusinessObjectCollection<DummyWithListReset> ParentList { get; set; }

			public override void Delete()
			{
				base.Delete();
				((IActiveBusinessObjectCollection)ParentList).Refresh();
			}
		}

		public void TestDeleteWhileListChangedSuspendedAndAlreadyDeleted()
		{
			var collection = GetCollection<DummyWithReadOnDelete>(Factory);
			collection.AddNew().Z0_Code = "D01";
			collection.AddNew().Z0_Code = "D02";
			collection.AddNew().Z0_Code = "D03";
			collection.AddNew().Z0_Code = "D04";

			AssertEquals(4, collection.Count);

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				collection[1].Delete();

				AssertEquals(3, collection.Count);

				collection.Delete(collection[1]);

				AssertEquals(2, collection.Count);
				AssertEquals("D01", collection[0].Z0_Code);
				AssertEquals("D04", collection[1].Z0_Code);
			}

			AssertEquals("No extra elements removed", 2, collection.Count);
		}

		class DummyWithReadOnDelete : DummyBusinessObject
		{
			public DummyWithReadOnDelete(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override void Delete()
			{
				if (Z0_Code == "Just access to a persistent property")
				{
					throw new ApplicationException("This will never be thrown");
				}

				base.Delete();
			}
		}

		#endregion

		#region TestUseFetchHintsOnLoadingCollectionWithDbOnlyFilter

		public virtual void TestUseFetchHintsOnLoadingCollectionWithDbOnlyFilter()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			for (int i = 0; i < 100; i++)
			{
				DummyBusinessObject dummy = factory1.New<DummyBusinessObject>();
				dummy.Z0_Code = "X" + i.ToString();
				dummy.Z0_Number = i;
			}
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 0);

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "X");

			ActiveBusinessObjectCollection<DummyBusinessObject> collection = GetCollection<DummyBusinessObject>(factory2, query);
			collection.AdditionalFilter = dbOnlyQuery; // Collection should have both primary and additional filter to cause excess db hits

			AssertEquals("Precondition - no db hits to DummyBizo table", 0, factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertEquals(100, collection.Count);
			AssertEquals(1, factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public virtual void TestUseFetchHintsOnLoadingCollectionWithDbOnlyCompositeFilter()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			for (int i = 0; i < 100; i++)
			{
				DummyBusinessObject dummy = factory1.New<DummyBusinessObject>();
				dummy.Z0_Code = "X" + i.ToString();
				dummy.Z0_Number = i;
				dummy.Z0_Bool = true;
			}
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 0);

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "X");
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Bool, true);

			ActiveBusinessObjectCollection<DummyBusinessObject> collection = GetCollection<DummyBusinessObject>(factory2, query);
			collection.AdditionalFilter = dbOnlyQuery; // Collection should have both primary and additional filter to cause excess db hits

			AssertEquals("Precondition - no db hits to DummyBizo table", 0, factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertEquals(100, collection.Count);
			AssertEquals(1, factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public virtual void TestUseFetchHintsOnLoadingCollectionWithDbOnlyFilterAndMaximumRows()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			for (int i = 0; i < 100; i++)
			{
				DummyBusinessObject dummy = factory1.New<DummyBusinessObject>();
				dummy.Z0_Code = "X" + i.ToString();
				dummy.Z0_Number = i;
			}
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "X");
			dbOnlyQuery.MaximumRows = 1000;

			ActiveBusinessObjectCollection<DummyBusinessObject> collection = GetCollection<DummyBusinessObject>(factory2);
			collection.AdditionalFilter = dbOnlyQuery;

			AssertEquals("Precondition - no db hits to DummyBizo table", 0, factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName));
			AssertEquals(100, collection.Count);
			AssertEquals(1, factory2.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		#endregion

		#region TestListResetShouldFireOnHasChangesChanged

		public void TestListResetShouldFireOnHasChangesChanged()
		{
			bool hasChangesChangedFired = false;
			EventHandler<HasChangesChangedEventArgs> hasChangesChangedHandler = (object sender, HasChangesChangedEventArgs e) =>
			{
				hasChangesChangedFired = true;
				AssertEquals(Collection.IndexExposed, e.ObjectThatWasChanged);
			};
			((IBusinessObjectState)Collection).HasChangesChanged += hasChangesChangedHandler;
			try
			{
				Assert("Precondition", !hasChangesChangedFired);

				Collection.IndexExposed.Refresh();

				Assert("OnHasChangesChanged should fire on collection", hasChangesChangedFired);
			}
			finally
			{
				((IBusinessObjectState)Collection).HasChangesChanged -= hasChangesChangedHandler;
			}
		}

		#endregion

		#region TestListChanged_ShouldNotReSortWhileEnumerating

		[ExpectNoExceptions]
		public virtual void TestListChanged_ShouldNotReSortWhileEnumerating()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			DependentCollection.ApplySort(DummyDependentBizoSchema.ZD1_Code.Name, ListSortDirection.Ascending);
			element1.ZD1_Code = "1";
			element2.ZD1_Code = "2";
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			foreach (var item in DependentCollection)
			{
				DummyDependantBusinessObject loadedElement = newFactory.Load<DummyDependantBusinessObject>(item.PK);
				loadedElement.ZD1_Code = "3";
				newFactory.Save();
			}
		}

		#endregion

		#region TestActiveOwnerAndGC

		public void TestActiveOwnerAndGC()
		{
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "XX1";
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "XX2";
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "XX3";

			var index = GetIndexFromUnrefrencedCollection<DummyBusinessObject>();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertNull("Collection will be collected in such situation", index.ActiveOwnerExposed);
			AssertEquals("Count still should be correct", 3, index.Count);

			AssertEquals("ABOCI<DummyBusinessObject>.PopulateCache() was called with ActiveOwner == null. IsDisposed = False. hasDisconncted = False. hasDeactivated = False. hasLostActiveOwner = True.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		ActiveBusinessObjectCollectionIndex<T> GetIndexFromUnrefrencedCollection<T>() where T : BusinessObject
		{
			return GetCollection<T>(Factory).IndexExposed;
		}

		/// <summary>
		/// Collection object should be alive while its iterator is alive.
		/// </summary>
		/// <remarks>
		/// In runtime build there are code optimizations, and it should be easier to achieve same effect.
		/// In debug build I had to do operations in specific way and order to allow collection variable to be cleared and collection object to be collected by GC.
		/// </remarks>
		public void TestEnumerationOnCollectionWithNoReferencesToItsVariable()
		{
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "XX1";

			var index = GetIndexFromUnrefrencedCollectionAfterDisposingEnumerator<DummyBusinessObject>();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertNull("Collection should be collected after enumerator was disposed", index.ActiveOwnerExposed);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		ActiveBusinessObjectCollectionIndex<T> GetIndexFromUnrefrencedCollectionAfterDisposingEnumerator<T>() where T : BusinessObject
		{
			(var index, var enumerator) = GetIndexAndEnumeratorFromUnrefrencedCollection<T>();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			AssertNotNull("Collection should not be collected while enumerator is alive", index.ActiveOwnerExposed);

			enumerator.Dispose();

			return index;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		(ActiveBusinessObjectCollectionIndex<T>, IEnumerator<T>) GetIndexAndEnumeratorFromUnrefrencedCollection<T>() where T : BusinessObject
		{
			var collection = GetCollection<T>(Factory);
			return (collection.IndexExposed, collection.GetEnumerator());
		}

		#endregion

		#region Test Classes

		class DummyBusinessObjectWithCodeRestricted : DummyBusinessObject
		{
			public DummyBusinessObjectWithCodeRestricted(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				if (row[DummyBizoSchema.Z0_Code.Name].ToString() != "NOL")
				{
					throw new ArgumentException("Wrong Type!!!");
				}
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				Z0_Code = "NOL";
			}
		}

		class TestActiveBusinessObjectCollection : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public TestActiveBusinessObjectCollection(BusinessObjectFactory factory, Predicate<DummyBusinessObject> predicateFilter) : base(factory) => Initialise(predicateFilter);

			public TestActiveBusinessObjectCollection(BusinessObjectFactory factory, Predicate<DummyBusinessObject> predicateFilter, ICollectionRelationship relationship) : base(factory, relationship) => Initialise(predicateFilter);

			public Predicate<DummyBusinessObject> PredicateFilter { get; private set; }

			void Initialise(Predicate<DummyBusinessObject> predicateFilter)
			{
				this.PredicateFilter = predicateFilter;
			}

			protected override bool MatchesFilterCore(DummyBusinessObject element, bool fetchOnlyFromLocalCache)
			{
				return PredicateFilter(element);
			}

			protected override object[] GetCollectionState()
			{
				return new object[] { PredicateFilter };
			}
		}

		class CollectionForOnLoadedTesting : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public CollectionForOnLoadedTesting(BusinessObjectFactory factory) : base(factory) { }
			public CollectionForOnLoadedTesting(BusinessObjectFactory factory, ICollectionRelationship relationship) : base(factory, relationship) { }

			protected override void OnLoadedIntoCollectionCore(DummyBusinessObject loadedObject)
			{
				base.OnLoadedIntoCollectionCore(loadedObject);
				OnLoadedIntoCollectionCoreCalled++;
				LastLoadedObject = loadedObject;
			}
			public int OnLoadedIntoCollectionCoreCalled;
			public BusinessObject LastLoadedObject;
		}

		#endregion

		#region Implementation

		IDisposable invariant;

		protected override void SetUp()
		{
			base.SetUp();
			invariant = ActiveBusinessObjectCollection.EnableInvariant();
		}

		protected override void TearDown()
		{
			base.TearDown();
			invariant.Dispose();
		}

		TestActiveBusinessObjectCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = GetCollection(Factory, PredicateFilter);
					((IActiveBusinessObjectCollection)collection).AdditionalFilter = Filter;
				}
				return collection;
			}
		}
		TestActiveBusinessObjectCollection collection;

		Predicate<DummyBusinessObject> PredicateFilter = delegate { return true; };

		ActiveBusinessObjectCollection<DummyDependantBusinessObject> DependentCollection
		{
			get
			{
				if (dependentCollection == null)
				{
					dependentCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory, new DependentRelationship(Master, typeof(DummyDependantBusinessObject)));
					((IActiveBusinessObjectCollection)dependentCollection).AdditionalFilter = Filter;
				}
				return dependentCollection;
			}
		}
		ActiveBusinessObjectCollection<DummyDependantBusinessObject> dependentCollection;

		ActiveBusinessObjectCollectionIndex<DummyDependantBusinessObject> Index
		{
			get
			{
				if (index == null)
				{
					index = DependentCollection.IndexExposed;
				}
				return index;
			}
		}
		ActiveBusinessObjectCollectionIndex<DummyDependantBusinessObject> index;

		ActiveBusinessObjectCollection<DummyDependantBusinessObject> ManyToManyCollection
		{
			get
			{
				if (manyToManyCollection == null)
				{
					manyToManyCollection = GetCollection<DummyDependantBusinessObject>(Master, typeof(DummyPivot));
					manyToManyCollection.AdditionalFilter = Filter;
				}
				return manyToManyCollection;
			}
		}
		ActiveBusinessObjectCollection<DummyDependantBusinessObject> manyToManyCollection;

		ZQuery Filter
		{
			get { return filter; }
			set
			{
				collection = null;
				dependentCollection = null;
				filter = value;
			}
		}
		ZQuery filter = new ZQuery();

		DummyWithDependentsBusinessObject Master
		{
			get
			{
				if (master == null)
				{
					master = Factory.New<DummyWithDependentsBusinessObject>();
					master.Z0_Code = "1";
				}
				return master;
			}
		}
		DummyWithDependentsBusinessObject master;

		ActiveBusinessObjectCollection<T> GetCollection<T>(BusinessObjectFactory factory, ZQuery query = null) where T : BusinessObject
		{
			var verifiedFilter = query ?? filter;
			var collection = new ActiveBusinessObjectCollection<T>(factory, verifiedFilter);

			if (GetType().Name.Contains("Adhoc"))
			{
				collection = new ActiveBusinessObjectCollection<T>(factory, new AdhocCollectionRelationship(typeof(T)));
				collection.AdditionalFilter = verifiedFilter;

				AddBizOToCollectionWithAdhocRelationship(collection);
			}

			return collection;
		}

		ActiveBusinessObjectCollection<T> GetCollection<T>(BusinessObject master, Type pivotObjectType) where T : BusinessObject
		{
			var collection = new ActiveBusinessObjectCollection<T>(master, pivotObjectType);
			return collection;
		}

		TestActiveBusinessObjectCollection GetCollection(BusinessObjectFactory factory, Predicate<DummyBusinessObject> predicateFilter)
		{
			var collection = new TestActiveBusinessObjectCollection(factory, predicateFilter);

			if (GetType().Name.Contains("Adhoc"))
			{
				collection = new TestActiveBusinessObjectCollection(factory, predicateFilter, new AdhocCollectionRelationship(typeof(DummyBusinessObject)));
				AddBizOToCollectionWithAdhocRelationship(collection);
			}

			return collection;
		}

		CollectionForOnLoadedTesting GetCollectionForOnLoaded(BusinessObjectFactory factory)
		{
			var collection = new CollectionForOnLoadedTesting(factory);
			if (GetType().Name.Contains("Adhoc"))
			{
				collection = new CollectionForOnLoadedTesting(factory, new AdhocCollectionRelationship(typeof(DummyBusinessObject)));
				AddBizOToCollectionWithAdhocRelationship(collection);
			}

			return collection;
		}

		void AddBizOToCollectionWithAdhocRelationship<T>(ActiveBusinessObjectCollection<T> collection) where T : BusinessObject
		{
			BusinessObject[] bizos = null;
			bizos = Factory.Load<T>(filter).ToArray();
			if (bizos != null)
			{ bizos.ForEach(bizo => collection.Add((T)bizo)); }
		}

		#endregion
	}
}
