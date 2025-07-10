using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMReleaseSequence))]
	class BMReleaseSequenceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNonStandardDefaultValues()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3500);
			var releaseSequence = Factory.New<BMReleaseSequence>();
			AssertEquals(3500, releaseSequence.BMR_SequenceNudge);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();
			AssertNotNull(Factory.Load<BMReleaseSequence>(releaseSequence.PK));

			var pk = releaseSequence.PK;
			releaseSequence.Delete();
			Factory.Save();
			AssertNull(Factory.Load<BMReleaseSequence>(pk));
		}

		#region Sequence Logging

		public void TestLoggingSequenceCreated()
		{
			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();

			var log = releaseSequence.Logs.MostRecentLogByEventTime(AutoEvents.AddedARecordToTheSystem);
			AssertEquals(BMReleaseSequenceSchema.Constants.TableName, log.SL_Table);
			AssertEquals("ADD", log.SL_SE_NKEvent);
		}

		public void TestLoggingSequencedPublished()
		{
			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();

			releaseSequence.BMR_IsActive = false;
			Factory.Save();

			releaseSequence.BMR_IsActive = true;
			Factory.Save();

			var log = releaseSequence.Logs.MostRecentLogByEventTime(AutoEvents.SetToActive);
			AssertEquals(BMReleaseSequenceSchema.Constants.TableName, log.SL_Table);
			AssertEquals("ACT", log.SL_SE_NKEvent);
		}

		public void TestLoggingSequenceDeactivated()
		{
			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();

			releaseSequence.BMR_IsActive = false;
			Factory.Save();

			var log = releaseSequence.Logs.MostRecentLogByEventTime(AutoEvents.SetToInactive);
			AssertEquals(BMReleaseSequenceSchema.Constants.TableName, log.SL_Table);
			AssertEquals("INA", log.SL_SE_NKEvent);
		}

		public void TestLoggingSequenceItemAdded()
		{
			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);

			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();

			var releaseSequenceItem = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			releaseSequenceItem.BMI_FH_ProcessHeader = workflow.PK;
			releaseSequenceItem.BMI_Position = 3;
			releaseSequence.Items.Add(releaseSequenceItem);
			Factory.Save();

			var log = releaseSequence.Logs.MostRecentLogByEventTime(AutoEvents.ItemAdded);
			AssertEquals(BMReleaseSequenceSchema.Constants.TableName, log.SL_Table);
			AssertEquals("IMA", log.SL_SE_NKEvent);
			AssertEquals("Item added at position 3", log.SL_Reference);
		}

		public void TestLoggingSequenceItemNotAdded_WhenNotSaved()
		{
			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);

			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();

			var releaseSequenceItem = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			releaseSequence.Items.Add(releaseSequenceItem);
			try
			{
				// The exception is thrown here because we tried to add a BMReleaseSequenceItem without the valid BMI_FH_ProcessHeader (that's a constraint violation).
				// It is intended in this test because we are testing the fact that we internally roll out ItemAdded event in case of an unsuccessful Save.
				Factory.Save();
			}
			catch (ZSaveException) { }

			var log = releaseSequence.Logs.MostRecentLogByEventTime(AutoEvents.ItemAdded);
			AssertNull(log);
		}

		public void TestLoggingSequenceItemRemoved()
		{
			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "lalala", buffer);

			var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();

			var releaseSequenceItem = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
			releaseSequenceItem.BMI_FH_ProcessHeader = workflow.PK;
			releaseSequenceItem.BMI_Position = 3;
			releaseSequence.Items.Add(releaseSequenceItem);
			Factory.Save();

			releaseSequence.Items.Delete(releaseSequenceItem);
			Factory.Save();

			var log = releaseSequence.Logs.MostRecentLogByEventTime(AutoEvents.ItemRemoved);
			AssertEquals(BMReleaseSequenceSchema.Constants.TableName, log.SL_Table);
			AssertEquals("IMR", log.SL_SE_NKEvent);
			AssertEquals("Item removed at position 3", log.SL_Reference);
		}

		#endregion
	}
}
