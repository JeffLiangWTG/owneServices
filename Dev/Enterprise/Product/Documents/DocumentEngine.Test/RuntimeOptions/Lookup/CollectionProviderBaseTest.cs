using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestsSubclassesOf(typeof(CollectionProvider), typeof(TestExcludeCollectionProviderAllHaveTestCase))]
	public abstract class CollectionProviderBaseTest : TestCaseWithFactory
	{
		public void TestCollection_ActiveBusinessObjectCollection_MustUseAdhocCollectionRelationship()
		{
			var collection = Provider.Collection as IActiveBusinessObjectCollection;
			if (collection != null && !(collection.Relationship is AdhocCollectionRelationship))
			{
				Fail($"CollectionProvider.CreateCollection must pass an AdhocCollectionRelationship to the constructor of {collection.GetType().FullName}.");
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCollectionForFindbox_ActiveBusinessObjectCollection_MustNotUseAdhocCollectionRelationship()
		{
			var collection = Provider.CollectionForFindbox as IActiveBusinessObjectCollection;
			if (collection != null && collection.Relationship is AdhocCollectionRelationship)
			{
				Fail($"AdhocCollectionRelationship cannot be used for find boxes. Override GetCollectionForFindbox() and return a {collection.GetType().FullName} that does not have an AdhocCollectionRelationship.");
			}
			else
			{
				Assert(true);
			}
		}

		public void TestCreateCollection()
		{
			AssertEquals(ExpectedCollectionType, Provider.Collection?.GetType());
		}

		public void TestGetCollectionForFindbox()
		{
			AssertEquals(ExpectedCollectionForFindBoxType, Provider.CollectionForFindbox?.GetType());
		}

		public void TestModuleID()
		{
			AssertEquals(ExpectedModuleID, Provider.ModuleID);
		}

		protected CollectionProvider Provider => (provider ?? (provider = (CollectionProvider)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()), Factory)));

		CollectionProvider provider;

		protected abstract Type ExpectedCollectionType { get; }

		protected virtual Type ExpectedCollectionForFindBoxType => ExpectedCollectionType;

		protected abstract ModuleIdentifier ExpectedModuleID { get; }
	}
}
