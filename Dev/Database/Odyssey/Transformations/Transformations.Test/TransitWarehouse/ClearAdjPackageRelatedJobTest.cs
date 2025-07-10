using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(ClearAdjPackageRelatedJob))]
	public class ClearAdjPackageRelatedJobTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			var branch1 = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch1.PK) { WW_TransitSecurityProcessingRequired = false }.WithDockDoor(TestConnection);
			var row1 = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var row2 = new WhsRow(whs, "B").AppendInsertAndReturnObject(sql);
			var pickArea = new WhsArea(whs.PK, "pick").AppendInsertAndReturnObject(sql);
			var putArea = new WhsArea(whs.PK, "put").AppendInsertAndReturnObject(sql);
			var fromLocation = new WhsLocation(row1.PK, pickArea.PK, putArea.PK).AppendInsertAndReturnObject(sql);
			var toLocation = new WhsLocation(row2.PK, pickArea.PK, putArea.PK).AppendInsertAndReturnObject(sql);
			var rcn = new WhsItemReceiveConsignment(whs, "RCN1", "RCN1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rtu = new WhsItemReceiveTransportationUnit(whs, "RTU1", fromLocation, "VEH").AppendInsertAndReturnObject(sql);
			var dcn = new WhsItemDispatchConsignment(whs, "DCN1", "DCN1", "STD").AppendInsertAndReturnObject(sql);
			var dll = new WhsItemDispatchLoadList("DLL1", whs).AppendInsertAndReturnObject(sql);
			var transferHeader = new WhsItemTransferHeader("WTH1", whs)
			{
				WTH_TransferType = "TRF",
			}.AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn.PK, rcn.WRC_JobID, "WRC").AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
			var packageState = new WhsItemPackageState(package.PK, whs, rcn.PK, "ADJ", "SEC")
			{
				WPS_IsSecure = true,
				WPS_AdjustedOut = "LCC",
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WDL_LoadList = dll.PK,
			}.AppendInsertAndReturnObject(sql);
			var normalPackage = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
			var normalPackageState = new WhsItemPackageState(normalPackage.PK, whs, rcn.PK, "PUT", "SEC")
			{
				WPS_IsSecure = true,
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WDL_LoadList = dll.PK,
			}.AppendInsertAndReturnObject(sql);
			var transferLineForSinglePKG = new WhsItemTransferLine(packageState, fromLocation, toLocation, transferHeader)
			{
				WTF_PickTime = DateTime.Now,
				WTF_GS_NKPickUser = "A",
			}.AppendInsertAndReturnObject(sql);
			var openTransferLineFor = new WhsItemTransferLine(normalPackageState, fromLocation, toLocation, transferHeader)
			{
				WTF_PickTime = DateTime.Now,
				WTF_GS_NKPickUser = "A",
			}.AppendInsertAndReturnObject(sql);

			var handlingUnitForAdj = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageStateForAdj = new WhsItemPackageState(handlingUnitForAdj.PK, whs, rcn.PK, "ADJ", "SEC")
			{
				WPS_IsSecure = true,
				WPS_AdjustedOut = "LCC",
				WPS_IsHandlingUnit = true,
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WDL_LoadList = dll.PK,
			}.AppendInsertAndReturnObject(sql);
			var innerPackage = new PkgPackage(packageJob, handlingUnitForAdj, "BOX", 1).AppendInsertAndReturnObject(sql);
			var innerPackageState = new WhsItemPackageState(innerPackage.PK, whs, rcn.PK, "ADJ", "SEC")
			{
				WPS_IsSecure = true,
				WPS_AdjustedOut = "LCC",
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_WDL_LoadList = dll.PK,
			}.AppendInsertAndReturnObject(sql);
			var handlingUnitDivot1 = new PkgPackageHandlingUnitDivot(handlingUnitForAdj.PK, innerPackage.PK).AppendInsertAndReturnObject(sql);

			var packageNoNeedUpdate = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
			var packageStateNoNeedUpdate = new WhsItemPackageState(packageNoNeedUpdate.PK, whs, rcn.PK, "ADJ", "SEC")
			{
				WPS_IsSecure = true,
				WPS_AdjustedOut = "LCC",
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_SystemLastEditTimeUtc = new DateTime(2024, 4, 2),
				WPS_SystemLastEditUser = "LDN",
				WPS_SystemCreateTimeUtc = new DateTime(2024, 4, 2),
				WPS_SystemCreateUser = "LDN"
			}.AppendInsertAndReturnObject(sql);

			var packageForDeletedTransferLine = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
			var packageStateForDeletedTransferLine = new WhsItemPackageState(packageForDeletedTransferLine.PK, whs, rcn.PK, "ADJ", "SEC")
			{
				WPS_IsSecure = true,
				WPS_AdjustedOut = "LCC",
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WDL_LoadList = dll.PK,
			}.AppendInsertAndReturnObject(sql);
			var deletedTransferLine = new WhsItemTransferLine(packageStateForDeletedTransferLine, fromLocation, toLocation, transferHeader).AppendInsertAndReturnObject(sql);

			var packageForFinaliseTransferHeader = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
			var packageStateForFinaliseTransferHeader = new WhsItemPackageState(packageForFinaliseTransferHeader.PK, whs, rcn.PK, "ADJ", "SEC")
			{
				WPS_IsSecure = true,
				WPS_AdjustedOut = "LCC",
				WPS_WRH_TransitReceiveHeader = rtu.PK,
			}.AppendInsertAndReturnObject(sql);
			var finaliseTransferHeader = new WhsItemTransferHeader("WTH2", whs)
			{
				WTH_TransferType = "TRF",
			}.AppendInsertAndReturnObject(sql);
			var transferLineOnTransferHeader = new WhsItemTransferLine(packageStateForFinaliseTransferHeader, fromLocation, toLocation, finaliseTransferHeader).AppendInsertAndReturnObject(sql);

			packageStatePK = packageState.PK;
			handlingUnitPackageStateForAdjPK = handlingUnitPackageStateForAdj.PK;
			innerPackageStatePK = innerPackageState.PK;
			transferLinePK = transferLineForSinglePKG.PK;
			packageStateNoNeedUpdatePK = packageStateNoNeedUpdate.PK;
			fromLocationPK = transferLineForSinglePKG.WTF_WL_From;
			deletedTransferLinePK = deletedTransferLine.PK;
			finaliseTransferHeaderPK = finaliseTransferHeader.PK;
			notFinaliseTransferHeaderPK = transferHeader.PK;

			using (TestWhsDataSetupHelper.SuspendTrigger("TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit",
				   PkgPackageSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		protected override void AssertTransformationResults()
		{
			WhsItemPackageState.AssertFromDB(TestConnection, packageStatePK)
				.ExpectEquals("adj single package DCN should be cleared", p => p.WPS_WDC_TransitDispatchConsignment, null)
				.ExpectEquals("adj single package DLL should be cleared", p => p.WPS_WDL_LoadList, null)
				.VerifyAll();

			WhsItemPackageState.AssertFromDB(TestConnection, handlingUnitPackageStateForAdjPK)
				.ExpectEquals("adj handling unit DCN should be cleared", p => p.WPS_WDC_TransitDispatchConsignment, null)
				.ExpectEquals("adj handling unit DLL should be cleared", p => p.WPS_WDL_LoadList, null)
				.VerifyAll();

			WhsItemPackageState.AssertFromDB(TestConnection, innerPackageStatePK)
				.ExpectEquals("inner package in adj handling unit DCN should be cleared", p => p.WPS_WDC_TransitDispatchConsignment, null)
				.ExpectEquals("inner package in adj handling unit DLL should be cleared", p => p.WPS_WDL_LoadList, null)
				.VerifyAll();

			WhsItemPackageState.AssertFromDB(TestConnection, packageStateNoNeedUpdatePK)
				.ExpectEquals("package without DCN or DLL won't be update", p => p.WPS_SystemLastEditUser, "LDN")
				.ExpectEquals("package without DCN or DLL won't be update", p => p.WPS_SystemLastEditTimeUtc, new DateTime(2024, 4, 2))
				.VerifyAll();

			WhsItemTransferLine.AssertFromDB(TestConnection, transferLinePK)
				.ExpectEquals("transfer line should be put back", t => t.WTF_WL_To, fromLocationPK)
				.ExpectNotEquals("transfer line should be put", t => t.WTF_PutTime, null)
				.ExpectNotEquals("transfer line should be put", t => t.WTF_GS_NKPutUser, null)
				.VerifyAll();

			AssertEquals(WhsItemTransferLine.CountInDB(TestConnection, t => t.PK == deletedTransferLinePK), 0);

			WhsItemTransferHeader.AssertFromDB(TestConnection, finaliseTransferHeaderPK)
				.ExpectEquals("transfer header should be finalised", t => t.WTH_IsFinalised, true)
				.VerifyAll();

			WhsItemTransferHeader.AssertFromDB(TestConnection, notFinaliseTransferHeaderPK)
				.ExpectEquals("transfer header should not be finalised", t => t.WTH_IsFinalised, false)
				.VerifyAll();
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ClearAdjPackageRelatedJob();

		Guid packageStatePK;
		Guid handlingUnitPackageStateForAdjPK;
		Guid innerPackageStatePK;
		Guid transferLinePK;
		Guid packageStateNoNeedUpdatePK;
		Guid deletedTransferLinePK;
		Guid finaliseTransferHeaderPK;
		Guid notFinaliseTransferHeaderPK;
		ForeignKey fromLocationPK;
	}
}
