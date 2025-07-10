using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	[TestsSubclassesOf(typeof(BusinessObjectCollection), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute), new Type[] { typeof(INonPersistentBusinessObjectCollection), typeof(BusinessObjectCollectionView<>), typeof(ISubsetBusinessObjectCollection) })]
	public abstract class BusinessObjectCollectionTestCase : BusinessObjectCollectionBaseTestCase<BusinessObjectCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			if (Collection.IsNonPersistent)
			{
				throw new NotSupportedException("Please use the NonPersistentBusinessObjectCollection test case for the class: " + GetType().FullName);
			}

			return Factory.New(Collection.TypeOfElements);
		}

		public void TestAdditionalFilterIsMarkedNotModifiable()
		{
			Assert(!Collection.AdditionalFilter.ModificationsEnabled);
		}

		public void TestRelationshipFilterIsMarkedNotModifiable()
		{
			Assert(!Collection.RelationshipFilter.ModificationsEnabled);
		}

		[ExpectNoExceptions()]
		public virtual void TestLoad()
		{
			try
			{
				ZQuery top1Filter = new ZQuery();
				top1Filter.AddToFilter(Collection.AdditionalFilter);
				top1Filter.MaximumRows = 1;
				Collection.Load(top1Filter);
			}
			catch (NotSupportedException) // Load not supported for this collection
			{
				Assert(true);
			}
		}

		public void TestSuspendListChanged()
		{
			using (var suspender = Collection.SuspendListChanged())
			{
				AssertNotNull("Suspender is not null", suspender);
				Assert("ListChanged is suspended", ((IBusinessObjectCollectionInternals)Collection).IsListChangedSuspended);
			}
		}

		public void TestHasChangesChangedEventArgs()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var childCollection = new DummyBusinessObjectCollection(Factory);
			dummy.RegisterEditableChildObject(childCollection);
			IBusiness objectToChange = childCollection;
			dummy.HasChangesChanged += HasChangesChangedHandler;
			var child = Factory.New<DummyBusinessObject>();
			childCollection.Add(child);
			objectToChange = child;
			dummy.HasChangesChanged += HasChangesChangedHandler;
			child.HasChanges = true;
			objectToChange = childCollection;
			dummy.HasChangesChanged += HasChangesChangedHandler;
			childCollection[0].Delete();

			void HasChangesChangedHandler(object sender, HasChangesChangedEventArgs e)
			{
				AssertEquals(objectToChange, e.ObjectThatWasChanged);
				dummy.HasChangesChanged -= HasChangesChangedHandler;
			}
		}

		public virtual void TestSuspendCountChanged()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			BusinessObject bizO3 = GetNewElementToAddToTheCollection();

			Collection.Add(bizO1);
			Collection.Add(bizO2);

			Action<IEnumerable<CollectionCountChangedEventArgs>> suspendedEventsProcessor = (suspendedEventArgs) =>
				{
					AssertEquals("All event args have been passed to processor", 2, suspendedEventArgs.Count());
					AssertEquals(true, suspendedEventArgs.First().ItemRemoved);
					AssertEquals(bizO2, suspendedEventArgs.First().BizObject);

					AssertEquals(true, suspendedEventArgs.Skip(1).First().ItemAdded);
					AssertEquals(bizO3, suspendedEventArgs.Skip(1).First().BizObject);
				};

			int countChangedEventHandlerCalled = 0;
			Collection.CountChanged += (s, e) => { countChangedEventHandlerCalled++; };

			using (Collection.SuspendCountChanged(suspendedEventsProcessor))
			{
				Collection.Remove(bizO2);
				Collection.Add(bizO3);
			}

			AssertEquals("CountChanged event was suspended", 0, countChangedEventHandlerCalled);

			AssertNoExceptionThrown("Null argument is allowed", () =>
				{
					using (Collection.SuspendCountChanged(null))
					{
						Collection.Add(GetNewElementToAddToTheCollection());
						Collection.Add(GetNewElementToAddToTheCollection());
					}
				});

			AssertEquals("CountChanged event was suspended", 0, countChangedEventHandlerCalled);

			Collection.Remove(bizO1);
			Collection.Add(GetNewElementToAddToTheCollection());
			AssertEquals("CountChanged event was not suspended", 2, countChangedEventHandlerCalled);
		}
	}
}
