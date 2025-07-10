using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(RelatedJobCollection))]
	sealed class RelatedJobCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#endregion

		#region TestAddingNonIRelatedJobObjectThrowsException

		[ExpectExceptionMessage(typeof(ArgumentException),
			"The BusinessObject 'DummyBusinessObject' added to RelatedJobCollection must implement IRelatedJob.")]
		public void TestAddingNonIRelatedJobObjectThrowsException()
		{
			Collection.Add(Factory.New<DummyBusinessObject>());
		}

		#endregion

		#region TestIndexer

		public void TestIndexer()
		{
			Collection.Add(Dummy);
			AssertEquals(Dummy, Collection[0]);
		}

		#endregion

		#region TestLoad

		public override void TestLoad()
		{
			Assert("Related Jobs are heterogeneous BusinessObjects implementing a common interface and thus cannot be loaded from the factory.", true);
		}

		#endregion

		#region Implementation

		DummyBusinessObjectWithRelatedJobs Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBusinessObjectWithRelatedJobs>()); }
		}

		new RelatedJobCollection Collection
		{
			get { return (RelatedJobCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RelatedJobCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DummyBusinessObjectWithRelatedJobs>();
		}

		DummyBusinessObjectWithRelatedJobs dummy;

		#endregion
	}
}
