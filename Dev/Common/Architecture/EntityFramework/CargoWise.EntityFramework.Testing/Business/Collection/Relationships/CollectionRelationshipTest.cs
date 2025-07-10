using System;
using System.Collections;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CollectionRelationshipTest : TestCaseWithFactory
	{
		#region Equals / GetHashCode

		public void TestEquals()
		{
			AssertEquals(
	new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery()),
	new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery()));
			AssertEquals(
	new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "1")),
					new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "1")));
			AssertNotEquals(
	new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "1")),
	new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "2")));
		}

		#endregion

		#region IsMatchesRelationshipFilterOverridden

		public void TestIsMatchesRelationshipFilterOverridden()
		{
			AssertEquals(false, CollectionRelationship.IsMatchesRelationshipFilterOverridden(typeof(CollectionRelationship)));
			AssertEquals(false, CollectionRelationship.IsMatchesRelationshipFilterOverridden(typeof(DependentRelationship)));
			AssertEquals(true, CollectionRelationship.IsMatchesRelationshipFilterOverridden(typeof(ManyToManyRelationship)));
			AssertEquals(true, CollectionRelationship.IsMatchesRelationshipFilterOverridden(typeof(DirectlyImplementingICollectionRelationship)));
		}

		class DirectlyImplementingICollectionRelationship : ICollectionRelationship
		{
			#region ICollectionRelationship Members

			ZQuery ICollectionRelationship.RelationshipFilter
			{
				get { throw new NotImplementedException(); }
			}

			event EventHandler ICollectionRelationship.RelationshipFilterChanged
			{
				add { throw new NotImplementedException(); }
				remove { throw new NotImplementedException(); }
			}

			ICollectionRelationship ICollectionRelationship.AddFilter(ZQuery additionalFilter)
			{
				throw new NotImplementedException();
			}

			bool ICollectionRelationship.MatchesRelationshipFilter(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
			{
				throw new NotImplementedException();
			}

			BusinessObject[] ICollectionRelationship.LoadBusinessObjects(BusinessObjectFactory factory, ZQuery filter)
			{
				throw new NotImplementedException();
			}

			BusinessObject ICollectionRelationship.Master
			{
				get { throw new NotImplementedException(); }
			}

			bool ICollectionRelationship.HasChangesIncludingRelationship(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			void ICollectionRelationship.ClearHasChangesIncludingRelationship(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			bool ICollectionRelationship.SupportsAddToRelationship()
			{
				throw new NotImplementedException();
			}

			void ICollectionRelationship.AddToRelationship(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			void ICollectionRelationship.RemoveFromRelationship(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			void ICollectionRelationship.Clear()
			{
				throw new NotImplementedException();
			}

			public IEnumerator GetDataEnumerator() => null;

			#endregion
		}

		#endregion

		#region ICollectionRelationship

		public void TestLoadBusinessObjects()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, "Value");
			query.FetchOnlyFromLocalCache = true;
			var relationship = new CollectionRelationship(typeof(DummyBusinessObject), query);
			var factory = new BusinessObjectFactory();
			relationship.LoadBusinessObjects(factory, new ZQuery());
			var tableSelects = factory.TableSelects;
			AssertEquals(0, tableSelects.Length);
		}

		public void TestRelationshipFilter()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "Value");
			CollectionRelationship relationship = new CollectionRelationship(typeof(DummyBusinessObject), query);
			AssertEquals("RelationshipFilter", "Z0_Code = 'Value'", relationship.RelationshipFilter.LiteralTextADO);
			var mock = new Mock<CollectionRelationship>(new object[] { typeof(DummyBusinessObject), query });
			mock.Protected().Setup<ZQuery>("RelationshipFilterCore").Returns(new ZQuery(DummyBizoSchema.Z0_FK_Code, "AA"));
			relationship = mock.Object;
			AssertEquals("RelationshipFilter", "Z0_FK_Code = 'AA' and Z0_Code = 'Value'", relationship.RelationshipFilter.LiteralTextADO);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestAddToRelationship()
		{
			DummyBusinessObject dummy = Collection.AddNew();
			Collection.Add(dummy);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestRemoveFromRelationship()
		{
			DummyBusinessObject dummy = Collection.AddNew();
			Collection.RemoveFromRelationship(dummy);
		}

		public void TestAddFilter()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Description, "Value");
			filter.MaximumRows = 5;
			ICollectionRelationship relationship = new CollectionRelationship(typeof(DummyBusinessObject), filter);
			ZQuery accessedRelationshipFilter = relationship.RelationshipFilter;

			relationship = relationship.AddFilter(new ZQuery(DummyBizoSchema.Z0_Description, "Value2"));
			AssertEquals("Z0_Description = 'Value2' and Z0_Description = 'Value'", relationship.RelationshipFilter.LiteralTextADO);
			AssertEquals("MaximumRows preserved", 5, relationship.RelationshipFilter.MaximumRows);
		}

		#region TestResetFieldDuringRelationshipFilterCalculation

		[ExpectNoExceptions]
		public void TestResetFieldDuringRelationshipFilterCalculation()
		{
			ICollectionRelationship resetingRelationship = new ResetingRelationship(typeof(DummyBusinessObject));
			AssertNotNull(resetingRelationship.RelationshipFilter);
		}

		class ResetingRelationship : CollectionRelationship
		{
			public ResetingRelationship(Type elementType) : base(elementType) { }

			protected override ZQuery RelationshipFilterCore
			{
				get
				{
					OnRelationshipFilterChanged(EventArgs.Empty); // Clear relationshipFilter field
					return base.RelationshipFilterCore;
				}
			}
		}

		#endregion

		#endregion

		#region TestDeletedMaster

		public void TestDeletedMaster()
		{
			var master = Factory.New<DummyBusinessObject>();
			var relationship = new RelationshipWithMaster(master);

			Assert("Should have result", !relationship.RelationshipFilter.IsNoResultQuery);

			master.Delete();
			relationship.InvalidateRelationshipFilter();

			Assert("Should have no result for deleted master", relationship.RelationshipFilter.IsNoResultQuery);
		}

		class RelationshipWithMaster : CollectionRelationship
		{
			public RelationshipWithMaster(DummyBusinessObject master)
				: base(typeof(DummyDependantBusinessObject))
			{
				this.master = master;
			}

			readonly DummyBusinessObject master;

			public override BusinessObject Master
			{
				get { return master; }
			}

			protected override ZQuery RelationshipFilterCore
			{
				get { return new ZQuery(DummyDependentBizoSchema.ZD1_Z0, master.PK); }
			}

			public void InvalidateRelationshipFilter()
			{
				OnRelationshipFilterChanged(EventArgs.Empty);
			}
		}

		#endregion

		#region Implementation

		ActiveBusinessObjectCollection<DummyBusinessObject> Collection
		{
			get { return collection ?? (collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, Relationship)); }
		}
		ActiveBusinessObjectCollection<DummyBusinessObject> collection;

		CollectionRelationship Relationship
		{
			get { return relationship ?? (relationship = new CollectionRelationship(typeof(DummyBusinessObject), Filter)); }
		}
		CollectionRelationship relationship;

		ZQuery Filter
		{
			get { return filter ?? (filter = new ZQuery(DummyBizoSchema.Z0_Code, "Value")); }
		}
		ZQuery filter;

		#endregion
	}
}
