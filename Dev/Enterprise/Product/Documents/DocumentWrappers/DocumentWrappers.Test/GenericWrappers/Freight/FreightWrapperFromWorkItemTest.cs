using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWorkItem))]
	sealed class FreightWrapperFromWorkItemTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var workItem = Factory.New<WorkItem>();
			var wrapper = new FreightWrapperFromWorkItem(workItem, Factory);
			AssertEquals("TrackingBusinessObjectPK", workItem.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestJobNumberShouldMatchWorkItemNumber()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI123456";
			var wrapper = new FreightWrapperFromWorkItem(workItem, Factory);
			AssertEquals($"Expected wrappers job number to be WI123456; but was {wrapper.JobNumber}. SAD!", "WI123456", wrapper.JobNumber);
		}

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWorkItem(workitem, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<WorkItem>();
		}

		protected override bool IsCarrierUsed => false;

		public override void TestWrapperNotes()
		{
			AssertEquals("Notes", 0, Wrapper.Notes.Count);
		}

		public override void TestWrapperNotesIncludingRelated()
		{
			AssertEquals("Notes", 0, Wrapper.NotesIncludingRelated.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			workitem = GetNewBusinessObjectToWrap() as WorkItem;
		}

		WorkItem workitem;

		#endregion Implementation
	}
}
