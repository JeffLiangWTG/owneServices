using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClassifierAllocationCollection))]
	public class ClassifierAllocationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ClassifierAllocationCollection>
	{
		public void TestGetIncludedForAllocation()
		{
			ClassifierAllocation classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = true;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = true;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = false;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = true;
			AssertEquals(3, ClassifierAllocationCollection.GetIncludedForAllocation().Count);
		}

		public void TestGetExcludedForAllocation()
		{
			ClassifierAllocation classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = false;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = false;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = true;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.IncludeForAllocation = false;
			AssertEquals(3, ClassifierAllocationCollection.GetExcludedForAllocation().Count);
		}

		public void TestClassifierWithLowestAllocation()
		{
			ClassifierAllocation classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 3;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 4;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 3;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 2;
			AssertEquals(2, ClassifierAllocationCollection.ClassifierWithLowestAllocation.NumberAllocated);
		}

		public void TestClassifierWithHighestAllocation()
		{
			ClassifierAllocation classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 3;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 4;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 3;
			classifierAllocation = ClassifierAllocationCollection.AddNew();
			classifierAllocation.NumberAllocated = 2;
			AssertEquals(4, ClassifierAllocationCollection.ClassifierWithHighestAllocation.NumberAllocated);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, ClassifierAllocationCollection.AllowNew);
		}

		protected override ClassifierAllocationCollection GetCollectionToTest()
		{
			return ClassifierAllocationCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClassifierAllocation(Factory);
		}

		ClassifierAllocationCollection ClassifierAllocationCollection
		{
			get
			{
				if (fClassifierAllocationCollection == null)
				{
					fClassifierAllocationCollection = new ClassifierAllocationCollection(Factory);
				}

				return fClassifierAllocationCollection;
			}
		}

		ClassifierAllocationCollection fClassifierAllocationCollection;
	}
}
