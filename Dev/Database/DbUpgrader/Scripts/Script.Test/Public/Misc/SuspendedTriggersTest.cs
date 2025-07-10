using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(SuspendedTriggers))]
	class SuspendedTriggersTest : DbCreateScriptTest
	{
		#region TestIsSuspended

		public void TestIsSuspended()
		{
			var suspendableTriggers = SuspendableTriggers;

			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				foreach (var trigger in suspendableTriggers)
				{
					AssertEquals("When not in a Transaction, Trigger cannot be suspended.", 0,
						connection1.ExecuteScalar($"SELECT IsSuspended FROM dbo.SuspendedTriggers WHERE TriggerName = '{trigger}'"));

					connection1.ExecuteNonQuery($"EXEC sp_getapplock @Resource = '{trigger}', @LockMode = 'Shared', @LockOwner = 'Transaction', @DbPrincipal = 'public'");
					AssertEquals("When not in a Transaction, Trigger cannot be suspended.", 0,
						connection1.ExecuteScalar($"SELECT IsSuspended FROM dbo.SuspendedTriggers WHERE TriggerName = '{trigger}'"));
				}

				foreach (var trigger in suspendableTriggers)
				{
					using (connection1.BeginTransactionWithManager())
					{
						AssertEquals("When in a Transaction, IsSuspended should return 0 (false) if the Trigger is not suspended.", 0,
							connection1.ExecuteScalar($"SELECT IsSuspended FROM dbo.SuspendedTriggers WHERE TriggerName = '{trigger}'"));

						connection1.ExecuteNonQuery($"EXEC sp_getapplock @Resource = '{trigger}', @LockMode = 'Shared', @LockOwner = 'Transaction', @DbPrincipal = 'public'");
						AssertEquals("When in a Transaction, IsSuspended should return 1 (true) if the Trigger is suspended.", 1,
							connection1.ExecuteScalar($"SELECT IsSuspended FROM dbo.SuspendedTriggers WHERE TriggerName = '{trigger}'"));

						foreach (var otherTrigger in suspendableTriggers)
						{
							if (otherTrigger != trigger)
							{
								AssertEquals("When in a Transaction, IsSuspended should return 0 (false) if the Trigger is not suspended.", 0,
									connection1.ExecuteScalar($"SELECT IsSuspended FROM dbo.SuspendedTriggers WHERE TriggerName = '{otherTrigger}'"));
							}
						}

						using (var connection2 = Db.NewExtraConnectionToMainDb())
						{
							connection2.BeginTransaction();
							AssertEquals("When in a Transaction, IsSuspended should return 0 (false) if the Trigger is suspended in another transaction.", 0,
								connection2.ExecuteScalar($"SELECT IsSuspended FROM dbo.SuspendedTriggers WHERE TriggerName = '{trigger}'"));
						}

						connection1.ExecuteNonQuery($"EXEC sp_releaseapplock @Resource = '{trigger}', @LockOwner = 'Transaction', @DbPrincipal = 'public'"); // clean up
						connection1.CommitTransaction();
					}
				}
			}
		}

		#endregion

		#region SuspendableTriggers

		public static IEnumerable<string> SuspendableTriggers
		{
			get
			{
				return new[]
				{
					"TG_PreventOverCommitOfStockViaPickLine",
					"TG_PreventOverReduceOfStockViaInventoryLine",
					"TG_PreventOverfillLocationWithUnitsCapacity",
					"TG_WhsDocket_PreventClientChangeWhenHasLines",
					"TG_WhsDocket_PreventClientChangeWhenStartedReceiving",
					"TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised",
					"TG_WhsDocketLine_StockOnHandIsBalanced",
					"TG_WhsDocketLine_StockOnHandIsBalanced_Insert",
					"TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent",
					"TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect",
					"TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert",
					"TG_WhsPickLine_StockOnHandIsBalanced",
					"TG_WhsPickLine_TransactionAndPickedQtyIsCorrect",
					"TG_WhsOrder_EnsureDDLIsEnteredOnPick",
					"TG_PreventMismatchOnDocketStatusAndDateWithLines",
					"TG_PreventUnPickedPickLinesOnFinalisedDocketLines",
					"TG_PreventUnPickedPickLinesOnFinalisedJobs",
					"TG_PreventUnPickedPickLinesOnFinalisedPicks",
					"TG_OrgPartRelation_EnsureOwnerOfProductWithSameBarcodeIsUnique",
					"TG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique",
					"TG_WhsCycleCountLocationVariance_HasSameStatus",
					"TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVariance",
					"TG_CusStatementHeader_Del",
					"TG_WhsCycleCountLocationVariance_CannotBeModifiedOrDeleted",
					"TG_DtbConsignmentAddressesDoNotHaveDuplicateSequenceNumbers",
					"TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit",
					"TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit",
					"TG_WhsItemCycleCountLocation_PreventCreateIfLocationHasOpenVariance",
					"TG_WhsItemCycleCountLocationVariance_HasSameStatus",
					"TG_WhsItemCycleCountLocationVariance_PreventDelete",
					"TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN",
					"TG_WhsItemPackageState_ULDPackageCannotBeAttachedToLoadListIfItHasAPlannedULDContainer",
					"TG_WhsItemDispatchLoadListDTUPivot_ULDContainerCannotBePlannedForLoadListIfItHasAULDPackageAttached",
					"TG_PkgPackageHeaderUniqueIDPerJob",
					"TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder",
					"TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalized",
					"TG_WhsSerialNumber_PreventUpdateClientProductSerialNumber",
					"TG_WhsPick_PackingStationIsValid",
					"TG_OrgPartUnit_UpdateOrgPartRelationUnitsPerClientUQ",
					"TG_OrgSupplierPart_UpdateOrgPartRelationUnitsPerClientUQ",
					"TG_OrgPartRelation_UpdateOrgPartRelationUnitsPerClientUQ",
					"TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine",
					"TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine",
					"TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits",
					"TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory",
					"TG_JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJob",
					"TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick",
					"TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick",
					// add new triggers here (then the above test should fail), then add it to the view
				};
			}
		}
		#endregion
	}
}
