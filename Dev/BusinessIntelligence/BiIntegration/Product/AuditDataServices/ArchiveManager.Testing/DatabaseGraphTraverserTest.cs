using System.Linq;
using CargoWise.Data;
using Enterprise.AuditDataServices.ArchiveManager.Helpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.ArchiveManager.Testing
{
	class DatabaseGraphTraverserTest : TestCase
	{
		public void TestTraverseDatabaseToFindAllRelevantRelationships()
		{
			var dbGraphTraverser = new DatabaseGraphTraverser();
			var archiveRelationships = dbGraphTraverser.TraverseDatabaseToFindAllRelevantRelationships(["ProcessTasks"]);

			(string ParentPKName, string ChildFKName)[] relations =
			[
				(ProcessTasksSchema.Constants.PK, ProcessTaskIterationLinkSchema.Constants.P9I_P9_ContainmentBarrierTask),
				(ProcessTasksSchema.Constants.PK, ProcessTaskIterationLinkSchema.Constants.P9I_P9_IterationTask),
				(ProcessTasksSchema.Constants.PK, ProcessTaskIterationLinkPivotSchema.Constants.P9P_P9_Task),
				(ProcessTasksSchema.Constants.PK, ProcessWorkflowExceptionSchema.Constants.WEX_P9_ProcessTask),
				(ProcessTasksSchema.Constants.PK, ProcessTasksSecureSchema.Constants.P9H_P9_Parent),
				(ProcessTasksSchema.Constants.PK, WorkItemSchema.Constants.WKI_P9_DefectCausedByTask),
				(ProcessTasksSchema.Constants.PK, WorkItemSchema.Constants.WKI_P9_DefectFirstMissedInTask),
				(ProcessTasksSchema.Constants.PK, WhsCycleCountLocationSchema.Constants.WCL_P9_Task),
				(ProcessTasksSchema.Constants.PK, WhsDocketLineSchema.Constants.WE_P9_Task),
				(ProcessTasksSchema.Constants.PK, WhsDocketSchema.Constants.WD_P9_PackingTask),
				(ProcessTasksSchema.Constants.PK, WhsPickLineSchema.Constants.WZ_P9_Task),
				(ProcessTasksSchema.Constants.PK, WhsPickByLabelJobSchema.Constants.WTK_P9_Task),
				(WorkItemSchema.Constants.PK, WorkItemRequestLinkSchema.Constants.WKL_WKI_WorkItem),
				(ProcessTaskIterationLinkSchema.Constants.PK, ProcessTaskIterationLinkSchema.Constants.P9I_P9I_ParentIteration), // Self referential relations, should only be included once
				(ProcessTaskIterationLinkSchema.Constants.PK, ProcessTaskIterationLinkPivotSchema.Constants.P9P_P9I_Iteration),
				// Until support for ON DELETE SET NULL is added to the Odyssey schema, the whole warehouse relationship graph will be included here from our added Task FKs
				(WhsCycleCountLocationSchema.Constants.PK, WhsCycleCountLocationSchema.Constants.WCL_WCL_RejectedCycleCount),
				(WhsCycleCountLocationSchema.Constants.PK, WhsCycleCountLocationVarianceSchema.Constants.WCC_WCL_CycleCountLocation),
				(WhsDocketSchema.Constants.PK, WhsVASOrderSchema.Constants.WVO_WD_TransferIntoServiceArea),
				(WhsDocketSchema.Constants.PK, WhsVASOrderSchema.Constants.WVO_WD_TransferOutOfServiceArea),
				(WhsDocketSchema.Constants.PK, WhsCycleCountLocationVarianceSchema.Constants.WCC_WD_RelatedAdjustment),
				(WhsDocketSchema.Constants.PK, WhsDocketSchema.Constants.WD_WD_ParentDocket),
				(WhsDocketSchema.Constants.PK, WhsDocketSchema.Constants.WD_WD_Split),
				(WhsDocketSchema.Constants.PK, WhsDocketContainerSchema.Constants.WC_WD),
				(WhsDocketSchema.Constants.PK, WhsDocketJobPivotSchema.Constants.WV_WD_Docket),
				(WhsDocketSchema.Constants.PK, WhsDocketLineSchema.Constants.WE_WD),
				(WhsDocketSchema.Constants.PK, WhsDocketPalletSchema.Constants.W2_WD),
				(WhsDocketSchema.Constants.PK, WhsDocketReferenceSchema.Constants.WX_WD),
				(WhsDocketSchema.Constants.PK, WhsPackageAuditSchema.Constants.WPA_WD_Order),
				(WhsDocketLineSchema.Constants.PK, WhsPickShortLineSchema.Constants.WZS_WE_TransactionLine),
				(WhsDocketLineSchema.Constants.PK, WhsPickShortLineSchema.Constants.WZS_WE_InventoryLine),
				(WhsDocketLineSchema.Constants.PK, WhsDocketLineSchema.Constants.WE_WE_MatchingLine),
				(WhsDocketLineSchema.Constants.PK, WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating),
				(WhsDocketLineSchema.Constants.PK, WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine),
				(WhsDocketLineSchema.Constants.PK, WhsInventoryHoldChangeLogSchema.Constants.WHL_WE_ParentDocketLine),
				(WhsDocketLineSchema.Constants.PK, WhsBOMInventoryPivotSchema.Constants.WIP_WE_ComponentLine),
				(WhsDocketLineSchema.Constants.PK, WhsBOMInventoryPivotSchema.Constants.WIP_WE_InventoryLine),
				(WhsDocketLineSchema.Constants.PK, WhsPickLineSchema.Constants.WZ_WE_InventoryLine),
				(WhsDocketLineSchema.Constants.PK, WhsPickLineSchema.Constants.WZ_WE_TransactionLine),
				(WhsDocketLineSchema.Constants.PK, WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine),
				(WhsDocketLineSchema.Constants.PK, WhsPickLineSchema.Constants.WZ_WE_OriginalPickedInventoryLine),
				(WhsPickByLabelJobSchema.Constants.PK, WhsPickByLabelLabelSchema.Constants.WTL_WTK_PickByLabelJob),
				(WhsPickLineSchema.Constants.PK, WhsSerialNumberPivotSchema.Constants.WSV_WZ_PickingLine),
				(WhsVASOrderSchema.Constants.PK, WhsVASOrderLineSchema.Constants.WVL_WVO_VASOrder),
				(WhsPackageAuditSchema.Constants.PK, WhsPackageAuditLineFailureSchema.Constants.WPF_WPA_WhsPackageAudit),
			];

			AssertContainsExactElementsInAnyOrder("Contains the correct relations and does not include cascade relations.", relations, archiveRelationships.Select(r => (r.ParentPKColumn.Name, r.ChildFKColumn.Name)));
		}

		public void TestTraverseDatabaseCachesSqlResults()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				var dbGraphTraverser = new DatabaseGraphTraverser();
				AssertEquals("No SQL should have been executed yet", 0, Db.Connection.ExecutedCommands.Count());

				var initial = dbGraphTraverser.TraverseDatabaseToFindAllRelevantRelationships([JobVoyageSchema.Constants.TableName, BMBufferTimespanSchema.Constants.TableName]).ToArray();
				AssertEquals("One SQL command should have been executed to get all FKs", 1, Db.Connection.ExecutedCommands.Count());

				var secondary = dbGraphTraverser.TraverseDatabaseToFindAllRelevantRelationships([JobVoyageSchema.Constants.TableName, BMBufferTimespanSchema.Constants.TableName]).ToArray();
				AssertEquals("The executed command count should remain the same as the previous results should've been cached", 1, Db.Connection.ExecutedCommands.Count());
				AssertContainsExactElementsInAnyOrder("The result set should be the same", initial, secondary);

				_ = dbGraphTraverser.TraverseDatabaseToFindAllRelevantRelationships([GlbCompanySchema.Constants.TableName, GlbStaffSchema.Constants.TableName]).ToArray();
				AssertEquals("The previous results should've been cached even though it's a different set of tables", 1, Db.Connection.ExecutedCommands.Count());
			}
		}

		public void TestResultsShouldBeUnique()
		{
			var dbGraphTraverser = new DatabaseGraphTraverser();
			var jobVoyage = dbGraphTraverser.TraverseDatabaseToFindAllRelevantRelationships([JobVoyageSchema.Constants.TableName]);
			var glbStaff = dbGraphTraverser.TraverseDatabaseToFindAllRelevantRelationships([GlbStaffSchema.Constants.TableName]);
			var orgHeader = dbGraphTraverser.TraverseDatabaseToFindAllRelevantRelationships([OrgHeaderSchema.Constants.TableName]);

			AssertContainsExactElementsInAnyOrder(jobVoyage.Distinct(), jobVoyage);
			AssertContainsExactElementsInAnyOrder(glbStaff.Distinct(), glbStaff);
			AssertContainsExactElementsInAnyOrder(orgHeader.Distinct(), orgHeader);
		}
	}
}
