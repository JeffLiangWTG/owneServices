using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WorkflowIterationReasonCollection))]
	sealed class WorkflowIterationReasonCollectionTest : RegistryBusinessObjectCollectionTestCase<WorkflowIterationReasonCollection>
	{
		public void TestWorkflowIterationReasonCollectionClone()
		{
			var clone = Collection.Clone(Collection.CurrentFallbackLevel, Collection.Factory);
			AssertNotNull(clone);
			AssertEquals(typeof(WorkflowIterationReasonCollection), clone.GetType());
		}

		public void TestAddNewAndIndexer()
		{
			var iterationReason = Collection.AddNew();
			AssertNotNull(iterationReason);
			AssertEquals(iterationReason, Collection[0]);
		}

		public void TestICodeDescriptionPairListMembers()
		{
			var collection = new WorkflowIterationReasonCollection();
			var iterationReason1 = collection.AddNew();
			iterationReason1.Code = "001";
			iterationReason1.Description = (NoResString)"One";

			var iterationReason2 = collection.AddNew();
			iterationReason2.Code = "002";
			iterationReason2.Description = (NoResString)"Two";

			var pairList = (ICodeDescriptionPairList)collection;
			Assert(pairList.ContainsCode("001"));
			AssertEquals("One", pairList.GetDescriptionFromCode("001"));

			Assert(pairList.ContainsCode("002"));
			AssertEquals("Two", pairList.GetDescriptionFromCode("002"));

			Assert(!pairList.ContainsCode("003"));
			AssertEquals("", pairList.GetDescriptionFromCode("003"));
		}

		#region Implementation

		protected override WorkflowIterationReasonCollection GetCollectionToTest()
		{
			return new WorkflowIterationReasonCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WorkflowIterationReason();
		}

		protected override bool RequiresFactory { get; }
		protected override bool RequiresFallbackLevel { get; }

		#endregion
	}
}
