using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.DocumentWrappers.Mapping.Testing
{
	sealed class DocumentWrapperMapperTest : TestCaseWithFactory
	{
		public void TestGenerateMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness()
		{
			AssertMultilineASCIIEquals(">>> Use Araxis Merge to see changes in the map. <<<"
					, TestMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness.Trim()
					, new DocumentWrapperMapper(typeof(MainWrapper), true, false).GetMapAsText());
		}
		#region TestMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness
		const string TestMapIncludingChildrenAndRelatedObjectsWithoutIFirstLevelIBusiness = @"
Adult
======================================================================
Name                                    Type
----------------------------------------------------------------------
Relation                                Relation
Code                                    String
Description                             String

ChildrenBought                          Child Collection
ChildrenSold                            Child Collection

Child Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
Dumb                     Dumb It Down
Dumber                   Dumber Then Dumb

Child
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChildFieldA                             String
ChildFieldB                             Decimal

Relation                                (Default Field: RelatedFieldA)
======================================================================
Name                                    Type
----------------------------------------------------------------------
RelatedFieldA                           String
RelatedFieldB                           Int
";
		#endregion

		public void TestGenerateMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness()
		{
			AssertMultilineASCIIEquals(">>> Use Araxis Merge to see changes in the map. <<<"
				, TestMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness.Trim()
				, new DocumentWrapperMapper(typeof(MainWrapper), true, true).GetMapAsText());
		}
		#region TestMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness
		const string TestMapIncludingChildrenAndRelatedObjectsWithIFirstLevelIBusiness = @"
Adult
======================================================================
Name                                    Type
----------------------------------------------------------------------
Cousin                                  LongDistanceRelation
Relation                                Relation
Code                                    String
Description                             String

ChildrenBought                          Child Collection
ChildrenSold                            Child Collection

Child Collection - Available Types
======================================================================
Code                     Description
----------------------------------------------------------------------
Dumb                     Dumb It Down
Dumber                   Dumber Then Dumb

Child
======================================================================
Name                                    Type
----------------------------------------------------------------------
ChildFieldA                             String
ChildFieldB                             Decimal

Relation                                (Default Field: RelatedFieldA)
======================================================================
Name                                    Type
----------------------------------------------------------------------
RelatedFieldA                           String
RelatedFieldB                           Int

LongDistanceRelation
======================================================================
Name                                    Type
----------------------------------------------------------------------
Age                                     Int
FullName                                String
";
		#endregion

		#region Test DocBaseWrapper Subclasses
		[WrapperTypeName("Adult")]
		class MainWrapper : DocBaseWrapper
		{
			public MainWrapper(BusinessObjectFactory factory)
				: base(factory.New<DummyBusinessObject>(), factory)
			{
			}

			protected DummyBusinessObject WrappedBO
			{
				get { return (DummyBusinessObject)base.WrappedObject; }
			}

			public ZString Code
			{
				get { return WrappedBO.Z0_Code; }
			}

			public ChildWrapperCollection ChildrenBought
			{
				get { return new ChildWrapperCollection(Factory); }
			}

			public ChildWrapperCollection ChildrenSold
			{
				get { return new ChildWrapperCollection(Factory); }
			}

			public RelationWrapper Relation
			{
				get { return null; }
			}

			public ZString Description
			{
				get { return WrappedBO.Z0_Description; }
			}

			public LongDistanceRelation Cousin
			{
				get { return new LongDistanceRelation(Factory); }
			}
		}

		[DefaultField("RelatedFieldA")]
		class RelationWrapper : DocBaseWrapper
		{
			public RelationWrapper(BusinessObjectFactory factory)
				: base(factory.New<DummyBusinessObject>(), factory)
			{
			}

			protected DummyBusinessObject WrappedBO
			{
				get { return (DummyBusinessObject)base.WrappedObject; }
			}

			public ZString RelatedFieldA
			{
				get { return WrappedBO.Z0_Code; }
			}

			public ZInt RelatedFieldB
			{
				get { return WrappedBO.Z0_Number; }
			}
		}

		class ChildWrapper : DocBaseWrapper
		{
			public ChildWrapper(BusinessObjectFactory factory)
				: base(factory.New<DummyChildBusinessObject>(), factory)
			{
			}

			protected DummyChildBusinessObject WrappedBO
			{
				get { return (DummyChildBusinessObject)base.WrappedObject; }
			}

			public ZString ChildFieldA
			{
				get { return WrappedBO.Z0_Code; }
			}

			public ZDecimal ChildFieldB
			{
				get { return WrappedBO.Z0_Decimal; }
			}

			public ZGuid ChildC
			{
				get { return WrappedBO.Z0_Guid; }
			}
		}

		[CustomIndexerList(typeof(DummyListForTesting))]
		class ChildWrapperCollection : DocBaseWrapperCollection<ChildWrapper>
		{
			public ChildWrapperCollection(BusinessObjectFactory factory)
				: base(new DummyChildBusinessObjectCollection(factory), factory)
			{
			}
		}

		class LongDistanceRelation : NonPersistentBusinessObject
		{
			public LongDistanceRelation(BusinessObjectFactory factory) : base(factory) { }

			public DummyBusinessObject NestedIBusinessObject
			{
				get { return Factory.New<DummyBusinessObject>(); }
			}

			public DummyBusinessObjectCollection NestedIBusinessObjectCollection
			{
				get { return new DummyBusinessObjectCollection(Factory); }
			}

			public ZInt Age
			{
				get { return new ZInt(); }
			}

			public ZString FullName
			{
				get { return new ZString(); }
			}
		}
		#endregion
	}
}
