using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmScheduleTaskCopyRecipient))]
	sealed class StmScheduleTaskCopyRecipientTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSCR_AvailableEmailAddress_List()
		{
			// Arrange
			var scheduleTaskRecipient = Factory.NewWithValidTestData<StmScheduleTaskRecipient>();
			scheduleTaskRecipient.S6_OH = Organization.PK;
			var scheduleTaskCopyRecipient = Factory.NewWithValidTestData<StmScheduleTaskCopyRecipient>();
			scheduleTaskCopyRecipient.SCR_S6 = scheduleTaskRecipient.PK;
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "test1@test.com";
			contact1.OC_OH = Organization.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "test2@test.com";
			contact2.OC_OH = Organization.PK;
			// Act
			var availableEmailAddresses = scheduleTaskCopyRecipient.Lookups.SCR_AvailableEmailAddress_List.ToArray();
			// Assert
			AssertEquals(2, availableEmailAddresses.Length);
			AssertCollectionContains(availableEmailAddresses, cdp => cdp.Code == contact1.Email);
			AssertCollectionContains(availableEmailAddresses, cdp => cdp.Code == contact2.Email);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (StmScheduleTaskCopyRecipient)base.GetBusinessObjectForFetchForLoad();
			if (result != null)
			{
				var recipient = result.Recipient;
				if (recipient == null)
				{
					recipient = Factory.NewWithValidTestData<StmScheduleTaskRecipient>();
					result.SCR_S6 = recipient.PK;
				}
				if (recipient.S6_S5 == ZGuid.Empty)
				{
					var task = Factory.NewWithValidTestData<StmScheduleTask>();
					recipient.S6_S5 = task.PK;
				}
			}
			return result;
		}

		#region Implementations

		OrgHeader Organization
		{
			get { return organization ?? (organization = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader organization;

		#endregion
	}
}
