using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMComponentResourceLink))]
	class BMComponentResourceLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreatingDuplicateRows_ShouldViolateUniqueConstraint()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			Factory.Save();

			var link1 = BMSTestHelper.CreateComponentResourceLink(config.Buffer, resource1);
			var link2 = BMSTestHelper.CreateComponentResourceLink(config.Buffer, resource2);

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			AssertNoErrors(link1.FD_GS_NKResourceInfo);
			AssertNoErrors(link2.FD_GS_NKResourceInfo);
			AssertNoExceptionThrown(Factory.Save);

			link2.FD_GS_NKResource = resource1.GS_Code;

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			AssertHasError(link1.FD_GS_NKResourceInfo, "This combination of component and resource has been duplicated.");
			AssertHasError(link2.FD_GS_NKResourceInfo, "This combination of component and resource has been duplicated.");
			AssertExceptionThrown<ZSaveException>(Factory.Save);
		}

		public void TestCapacityReservationDetails()
		{
			var component = BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory));
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var link = component.ResourceLinks.AddNew();
			link.FD_GS_NKResource = resource.GS_Code;
			link.CapacityReservationDetails = "Blah";

			Factory.Save();

			var loadedLink = new BusinessObjectFactory().Load<BMComponentResourceLink>(link.PK);
			AssertEquals("Blah", loadedLink.CapacityReservationDetails);

			var note = loadedLink.Factory.LoadTop1<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, loadedLink.PK).AddToFilter(StmNoteSchema.ST_Table, loadedLink.TableName));
			AssertNotNull(note);

			loadedLink.CapacityReservationDetails = ZString.Empty;
			AssertEquals(true, note.IsDeleted);
		}

		public void TestSavingCapacityReservationDetails_ShouldNotSaveStmALog()
		{
			// Arrange
			var link = Factory.NewWithValidTestData<BMComponentResourceLink>();
			link.CapacityReservationDetails = "Testing Content";

			// Act
			Factory.Save();

			// Assert
			var stmNoteQuery = new ZQuery();
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_ParentID, link.PK);
			var note = Factory.LoadTop1<StmNote>(stmNoteQuery);
			AssertNotNull("We should still save a StmNote, even though we are not saving StmALog ", note);
			AssertEquals("Expected Capacity Reservation Details to be: Testing Content", "Testing Content", link.CapacityReservationDetails);

			var logQuery = new ZQuery();
			logQuery.AddToFilter(StmALogSchema.SL_Parent, note.PK);
			var log = Factory.LoadTop1<StmALog>(logQuery);
			AssertNull("We Shouldn't save a StmLog when save the StmNote as part of CapacityReservationDetails", log);
		}

		public void TestTimeConsideredCCR()
		{
			var link = Factory.New<BMComponentResourceLink>();
			AssertEquals(string.Empty, link.TimeConsideredCCR);

			link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow.AddDays(-32);
			AssertEquals("over 1 month ago", link.TimeConsideredCCR);
		}

		public void TestIsCCR_ReadOnly()
		{
			var link = Factory.New<BMComponentResourceLink>();
			AssertEquals("Should be readonly when no CCR detection time specified", true, link.FD_IsCapacityConstrainedInfo.ReadOnly);

			link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow;
			AssertEquals("Should NOT be readonly when  CCR detection time is specified", false, link.FD_IsCapacityConstrainedInfo.ReadOnly);
		}

		public void TestClearCCRDetectionTime_ShouldRemoveIsPersistentlyOverloadedFlagButNotCCRFlag()
		{
			var link = Factory.New<BMComponentResourceLink>();
			link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow;
			link.FD_IsCapacityConstrained = true;
			link.FD_IsPersistentlyOverloaded = true;

			link.FD_CapacityConstraintDetectedUtc = ZDateTime.Empty;
			AssertEquals(true, link.FD_IsCapacityConstrained);
			AssertEquals(false, link.FD_IsPersistentlyOverloaded);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<BMSystem>().Components.AddNew().ResourceLinks.AddNew();
		}
	}
}
