using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class JobInvoicingParentLinkDeleteCheckerTest : TestCaseWithFactory
	{
		public void TestDeleteDetails_ShouldDisallowDeletion_WhenJobHeaderExistsInMemoryAndParentInMemory()
		{
			var expectedExceptionMessage = "dummyJobNumber cannot be deleted. An Invoicing Job Header has been created for this.";

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			var jobHeader = CreateJobHeaderWithParent(jobHeaderParent);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => jobHeaderParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete JobHeaderParent", !jobHeaderParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenJobHeaderExistsInMemoryAndParentInDatabase()
		{
			var expectedExceptionMessage = "dummyJobNumber cannot be deleted. An Invoicing Job Header has been created for this.";

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			Factory.Save();

			var jobHeader = CreateJobHeaderWithParent(jobHeaderParent);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => jobHeaderParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete JobHeaderParent", !jobHeaderParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenJobHeaderExistsButNotLoaded()
		{
			var expectedExceptionMessage = "dummyJobNumber cannot be deleted. An Invoicing Job Header has been created for this.";

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();

			var secondFactory = new BusinessObjectFactory();
			var jobHeader = secondFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = jobHeaderParent.PK;
			secondFactory.Save();

			var exception = AssertExceptionThrown<CannotDeleteException>(() => jobHeaderParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete JobHeaderParent", !jobHeaderParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenJobHeaderExistsInDatabaseAndParentInDatabase()
		{
			var expectedExceptionMessage = "dummyJobNumber cannot be deleted. An Invoicing Job Header has been created for this.";

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			var jobHeader = CreateJobHeaderWithParent(jobHeaderParent);
			Factory.Save();

			var exception = AssertExceptionThrown<CannotDeleteException>(() => jobHeaderParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete JobHeaderParent", !jobHeaderParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldAllowDeletion_WhenJobHeaderDoesNotExist()
		{
			var expectedExceptionMessage = "dummyJobNumber cannot be deleted. An Invoicing Job Header has been created for this.";

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			var jobHeader = CreateJobHeaderWithParent(jobHeaderParent);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => jobHeaderParent.Delete());

			AssertContains("Precondition: Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Precondition: Delete JobHeaderParent", !jobHeaderParent.IsDeleted);

			jobHeader.Delete();

			AssertNoExceptionThrown(() => jobHeaderParent.Delete());
			Assert("Delete JobHeaderParent", jobHeaderParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldAllowDeletion_WhenBusinessObjectIsNotJobHeaderParent()
		{
			var jobHeaderParent = Factory.New<DummyBusinessObject>();

			AssertNoExceptionThrown(() => jobHeaderParent.Delete());
			Assert("Delete JobHeaderParent", jobHeaderParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenJobHeaderParentHasOverridedPK()
		{
			var expectedExceptionMessage = "dummyJobNumber cannot be deleted. An Invoicing Job Header has been created for this.";
			var jobHeaderParentWithOveridedPK = Factory.New<DummyJobHeaderParentWithOverridedPK>();

			var jobHeaderParentCorePK = (jobHeaderParentWithOveridedPK as IJobHeaderParentCore).PK;
			AssertEquals("Precondition: Overrided BusinessObject PK", ZGuid.Empty, jobHeaderParentCorePK);
			AssertNotEquals("Precondition: Base BusinessObject PK", jobHeaderParentCorePK, jobHeaderParentWithOveridedPK.PK);

			CreateJobHeaderWithParent(jobHeaderParentWithOveridedPK);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => jobHeaderParentWithOveridedPK.Delete());
			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete JobHeaderParent", !jobHeaderParentWithOveridedPK.IsDeleted);
		}

		JobHeader CreateJobHeaderWithParent(DummyJobHeaderParent parent)
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = parent.PK;
			return jobHeader;
		}

		class DummyJobHeaderParentWithOverridedPK : DummyJobHeaderParent, IJobHeaderParent
		{
			public DummyJobHeaderParentWithOverridedPK(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZGuid IJobHeaderParentCore.PK => ZGuid.Empty;
		}

		class DummyJobHeaderParent : DummyBusinessObject, IJobHeaderParent
		{
			public DummyJobHeaderParent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			BusinessObjectFactory IJobHeaderParentCore.Factory
			{
				get { return Factory; }
			}

			string IJobHeaderParentCore.TableName
			{
				get { return ""; }
			}

			bool IJobHeaderParentCore.IsInDatabase
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			string IJobNumber.JobNumber
			{
				get { return "dummyJobNumber"; }
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { throw new NotImplementedException(); }
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}
		}
	}
}
