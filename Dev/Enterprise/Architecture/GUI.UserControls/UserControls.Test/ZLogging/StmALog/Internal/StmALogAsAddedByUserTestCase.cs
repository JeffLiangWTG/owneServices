using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(StmALogAsAddedByUser))]
	sealed class StmALogAsAddedByUserTestCase : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("StmALog does not support delete", true);
		}

		public override void TestSettingDateTimeFieldsWithInvalidDateDoesntCauseTheInvalidDateToGetDefaultedToOtherFields()
		{
			Assert("This should not be run on StmALog as it tries to set fields it can't on this type of object.", true);
		}

		public void TestValidateSL_SE_NKEvent()
		{
			Assert("Precondition - Validation not yet run, expect no errors", !log.HasErrors);

			log.SL_SE_NKEvent = string.Empty;
			Assert("Event is invalid, SL_SE_NKEvent should have an error", log.SL_SE_NKEventInfo.HasErrors());

			log.SL_SE_NKEvent = Events.Booked.Code;
			Assert("Event is valid, SL_SE_NKEvent should not have errors", !log.SL_SE_NKEventInfo.HasErrors());

			log.SL_SE_NKEvent = Events.AddedARecordToTheSystem.Code;
			Assert("'ADD' Event is valid, SL_SE_NKEvent should not have an error. Should be same with Workflow Trigger business logic.", !log.SL_SE_NKEventInfo.HasErrors());

			var cleanFactory = new BusinessObjectFactory();

			var stmEvent = cleanFactory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, Events.Booked.Code);
			stmEvent.SE_IsActive = false;
			cleanFactory.Save();

			log.SL_SE_NKEvent = Events.Booked.Code;
			Assert("Event is inactive, SL_SE_NKEvent should have a system default error for inactive bizo", log.SL_SE_NKEventInfo.HasErrors());
		}

		#region ValidateSL_Reference

		public void TestValidateSL_Reference()
		{
			log.SL_Reference = "McLaren|LOC=UAIEV";
			AssertNoErrors("SL_Reference", log.SL_ReferenceInfo);
			AssertNoWarnings("SL_Reference", log.SL_ReferenceInfo);

			log.SL_Reference = "McLaren = Champion!";
			AssertNoErrors("SL_Reference", log.SL_ReferenceInfo);
			AssertNoWarnings("SL_Reference", log.SL_ReferenceInfo);

			log.SL_Reference = "|McLaren = Champion!";
			AssertNoErrors("SL_Reference", log.SL_ReferenceInfo);
			AssertNoWarnings("SL_Reference", log.SL_ReferenceInfo);

			log.SL_Reference = "McLaren = Champion!|LOC=UAIEV";
			AssertNoErrors("SL_Reference", log.SL_ReferenceInfo);
			AssertHasWarnings("SL_Reference", log.SL_ReferenceInfo);

			log.SL_Reference = "McLaren|LOC=UAIEV|LOC=AUSYD";
			AssertHasErrors("SL_Reference", log.SL_ReferenceInfo);
		}

		#endregion

		public void TestRunPreSaveValidation()
		{
			AssertEquals("Precondition - Validation not yet run, expect no errors", false, log.HasErrors);

			log.SL_SE_NKEvent = string.Empty;
			log.SL_EventTime = ZDateTime.Empty;
			log.RunPreSaveValidation();
			AssertEquals("Validation has run, there should be errors", true, log.HasErrors);
			Assert("Event is invalid, SL_SE_NKEvent should have an error", log.SL_SE_NKEventInfo.HasErrors());
			Assert("Event Time is invalid, SL_EventTime should have an error", log.SL_EventTimeInfo.HasErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();

			log = (StmALogAsAddedByUser)GetNewBusinessObject();
			log.Master = Factory.New<DummyEnterpriseBusinessObject>();
		}

		protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject bizObj, BusinessObjectFactory newFactory)
		{
			return newFactory.LoadTop1(bizObj.GetType(), new ZQuery(StmALogSchema.SL_Parent, ((BaseStmALog)bizObj).SL_Parent));
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		StmALogAsAddedByUser log;
	}
}
