using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromCustomerServiceTicket))]
	sealed class FreightWrapperFromCustomerServiceTicketTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var workRequest = Factory.New<WorkRequest>();
			var wrapper = new FreightWrapperFromCustomerServiceTicket(workRequest, Factory);
			AssertEquals("TrackingBusinessObjectPK", workRequest.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestJobNumberShouldMatchWorkRequestNumber()
		{
			var workRequest = Factory.NewWithValidTestData<WorkRequest>();
			workRequest.WKR_RequestNumber = "CST0002";
			var wrapper = new FreightWrapperFromCustomerServiceTicket(workRequest, Factory);
			AssertEquals($"Expected wrappers job number to be CST0002 but was {wrapper.JobNumber}. SAD!", "CST0002", wrapper.JobNumber);
		}

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromCustomerServiceTicket(workRequest, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<WorkRequest>();
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

			workRequest = GetNewBusinessObjectToWrap() as WorkRequest;
		}

		WorkRequest workRequest;

		#endregion Implementation
	}
}
