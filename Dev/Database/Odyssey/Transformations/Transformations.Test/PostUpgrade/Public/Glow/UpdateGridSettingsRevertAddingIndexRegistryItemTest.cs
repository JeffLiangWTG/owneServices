using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Glow
{
	static class StmDataHelper
	{
		public static Guid Insert(Guid pk, string sD_Name, Guid sD_Owner)
		{
			var sql = "INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner) VALUES	(@SD_PK, @SD_Name, @SD_Owner);";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@SD_PK", pk, StmDataSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@SD_Name", sD_Name, StmDataSchema.SD_Name);
				cmd.AddParameterBasedOnDbColumn("@SD_Owner", (sD_Owner == Guid.Empty) ? DBNull.Value : sD_Owner, StmDataSchema.SD_Owner);
				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static string GetNameByPK(Guid pk)
		{
			var sqlText = "SELECT SD_Name FROM dbo.StmData WHERE SD_PK = @PK";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@PK", pk, StmDataSchema.PK);

				var data = cmd.ExecuteScalar();
				return Convert.IsDBNull(data) ? null : (string)data;
			}
		}

		public static bool Exists(Guid pk) => GetNameByPK(pk) != null;
	}

	static class StmModuleFilterHelper
	{
		public static Guid Insert(Guid pk, string s9_ModuleID, Guid s9_RelatedEntityID)
		{
			var sql = @"INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_RelatedEntityID, S9_SystemCreateUser, S9_SystemCreateTimeUtc, S9_SystemLastEditUser, S9_SystemLastEditTimeUtc)
						VALUES	(@S9_PK, @S9_ModuleID, @S9_RelatedEntityID, '~BP', GETUTCDATE(), '~BP', GETUTCDATE());";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@S9_PK", pk, StmModuleFilterSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@S9_ModuleID", s9_ModuleID, StmModuleFilterSchema.S9_ModuleID);
				cmd.AddParameterBasedOnDbColumn("@S9_RelatedEntityID", s9_RelatedEntityID, StmModuleFilterSchema.S9_RelatedEntityID);
				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static string GetNameByPK(Guid pk)
		{
			var sqlText = "SELECT S9_ModuleID FROM dbo.StmModuleFilter WHERE S9_PK = @PK";

			using (var cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@PK", pk, StmModuleFilterSchema.PK);

				var data = cmd.ExecuteScalar();
				return Convert.IsDBNull(data) ? null : (string)data;
			}
		}

		public static bool Exists(Guid pk) => GetNameByPK(pk) != null;
	}

	public abstract class UpdateGridSettingsRevertAddingIndexRegistryItemTestBase : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			owner1 = Guid.NewGuid();
			owner2 = Guid.NewGuid();
			return new UpdateGridSettingsRevertAddingIndexRegistryItem();
		}

		protected override void PrepareTestData()
		{
			using (var cmd = TestConnection.Command("DELETE dbo.StmData WHERE SD_Name like 'GridSettings_Search_%'"))
			{
				cmd.ExecuteNonQuery();
				PrepareTestDataCore();
				Reset();
			}
		}

		protected override void AssertTransformationResults()
		{
			Reset();
			AssertTransformationResultsCore();
		}

		protected Guid Next
		{
			get
			{
				if (gridsettings.Count < ++gridIndex)
				{
					gridsettings.Add(Guid.NewGuid());
				}
				return gridsettings[gridIndex - 1];
			}
		}

		protected void Reset() => gridIndex = 0;

		protected abstract void PrepareTestDataCore();
		protected abstract void AssertTransformationResultsCore();

		int gridIndex;
		protected Guid owner1;
		protected Guid owner2;
		protected List<Guid> gridsettings = new List<Guid>();
	}

	[TestedType(typeof(UpdateGridSettingsRevertAddingIndexRegistryItem))]
	public class UpdateGridSettingsRevertAddingIndexRegistryItem_DoNothingTest : UpdateGridSettingsRevertAddingIndexRegistryItemTestBase
	{
		protected override void PrepareTestDataCore()
		{
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType1_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType_filterName", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType1_filterName", owner2);

			// settings from hyetip
			StmDataHelper.Insert(Next, "GridSettings_0a89006f-5f39-463f-8c05-27c2d7952f3c", owner2);
			StmDataHelper.Insert(Next, "GridSettings_0d3ec2d563df4b27be93bb1809402bc1_BookingInstruction().Packages", owner2);
			StmDataHelper.Insert(Next, "GridSettings_66e8263758bc4f2bb361e0c930be689a_WhsItemPackageStates", owner2);
			StmDataHelper.Insert(Next, "GridSettings_IBPMConfigurationTmpl", owner2);
			StmDataHelper.Insert(Next, "GridSettings_RDT_0885f398-e95b-4580-82a3-bb07168ef452_Entity", owner2);
			StmDataHelper.Insert(Next, "GridSettings_RDT_Entity", owner2);
			StmDataHelper.Insert(Next, "GridSettings_RDT_Entity_IAccTransactionHeaderInfo_38ddea0fe9214afd8b8e7f9e8edf71a4_bd03eb9d-8588-4173-9b5a-2242cc0213cb", owner2);
			StmDataHelper.Insert(Next, "GridSettings_RDT_Entity_INote[[IJobContainer]]", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SDT_Entity_IAdditionalService[[IDtbConsignment]]_f9e3f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SDT_IJobSupplierBooking_9f2562b3f3254364b80788087a275277_aae1f1ac-afd1-4ebe-813e-9403fa4d64c2", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SDT_IWhsItemPackageState", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_0afdf49683bf422e920d0438dcf25297_5b5349cf-5680-4164-b4c3-e3282160d801_Entity_IHRMStaff", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Entity_IAdditionalService[[IDtbConsignment]]_Test_1", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Entity_IAdditionalService[[IDtbConsignment]]", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Entity_ICurrentDeviceLocation", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_ICYContainerLoadList", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_IJobOrderLine_Test1", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_IJobShipment_Last_Completed_Milestone", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SEP_IJobShipment_189a43c94e784fa28743ce5e22a8951e_490465d1-5497-4aa9-bba8-fbac663311f4", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SEP_Janet's", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SLS_IJobShipment_dbed0e27-e6ff-4d1f-9f5f-d24107d258f6_b8343981-7395-41eb-8bdd-9e5934e00eab", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SLS_Janet's", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Trackable_3c95f54513ea4829aebfe9ffb9b4c647_Search_eSHIPPER", owner2);
			StmDataHelper.Insert(Next, "GridSettings_WorkflowAuditLogs", owner2);

			StmModuleFilterHelper.Insert(Next, "SDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner1);
			StmModuleFilterHelper.Insert(Next, "RDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner1);
			StmModuleFilterHelper.Insert(Next, "SLS_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner1);
			StmModuleFilterHelper.Insert(Next, "SEP_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner1);

			StmModuleFilterHelper.Insert(Next, "SDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "RDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "SLS_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "SEP_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);

			StmModuleFilterHelper.Insert(Next, "SDTICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "RDTICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "SLSICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "SEPICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "SomeRandomness", owner2);

			StmModuleFilterHelper.Insert(Next, "SDT_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "RDT_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "SLS_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
			StmModuleFilterHelper.Insert(Next, "SEP_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", owner2);
		}

		protected override void AssertTransformationResultsCore()
		{
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType1_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType1_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_0a89006f-5f39-463f-8c05-27c2d7952f3c", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_0d3ec2d563df4b27be93bb1809402bc1_BookingInstruction().Packages", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_66e8263758bc4f2bb361e0c930be689a_WhsItemPackageStates", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_IBPMConfigurationTmpl", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_RDT_0885f398-e95b-4580-82a3-bb07168ef452_Entity", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_RDT_Entity", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_RDT_Entity_IAccTransactionHeaderInfo_38ddea0fe9214afd8b8e7f9e8edf71a4_bd03eb9d-8588-4173-9b5a-2242cc0213cb", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_RDT_Entity_INote[[IJobContainer]]", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SDT_Entity_IAdditionalService[[IDtbConsignment]]_f9e3f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SDT_IJobSupplierBooking_9f2562b3f3254364b80788087a275277_aae1f1ac-afd1-4ebe-813e-9403fa4d64c2", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SDT_IWhsItemPackageState", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_0afdf49683bf422e920d0438dcf25297_5b5349cf-5680-4164-b4c3-e3282160d801_Entity_IHRMStaff", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_Entity_IAdditionalService[[IDtbConsignment]]_Test_1", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_Entity_IAdditionalService[[IDtbConsignment]]", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_Entity_ICurrentDeviceLocation", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_ICYContainerLoadList", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_IJobOrderLine_Test1", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_IJobShipment_Last_Completed_Milestone", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SEP_IJobShipment_189a43c94e784fa28743ce5e22a8951e_490465d1-5497-4aa9-bba8-fbac663311f4", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SEP_Janet's", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SLS_IJobShipment_dbed0e27-e6ff-4d1f-9f5f-d24107d258f6_b8343981-7395-41eb-8bdd-9e5934e00eab", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SLS_Janet's", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Trackable_3c95f54513ea4829aebfe9ffb9b4c647_Search_eSHIPPER", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_WorkflowAuditLogs", StmDataHelper.GetNameByPK(Next));

			AssertEquals("SDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));

			AssertEquals("SDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));

			AssertEquals("SDTICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDTICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLSICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEPICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SomeRandomness", StmModuleFilterHelper.GetNameByPK(Next));

			AssertEquals("SDT_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_Entity_ICYDYardUnitState_133f8bf349094e03ac8761152ada063d_29edc091-b934-4ac0-800c-29683ba75ae7", StmModuleFilterHelper.GetNameByPK(Next));
		}
	}

	[TestedType(typeof(UpdateGridSettingsRevertAddingIndexRegistryItem))]
	public class UpdateGridSettingsRevertAddingIndexRegistryItem_ConvertAllSettingsTest : UpdateGridSettingsRevertAddingIndexRegistryItemTestBase
	{
		protected override void PrepareTestDataCore()
		{
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType1_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType_filterName", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType1_filterName", owner2);

			StmDataHelper.Insert(Next, "GridSettings_SDT_Index_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner2);
			StmDataHelper.Insert(Next, "GridSettings_RDT_Index_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_ICYDTransportationUnit_Expected_trucks", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_ICYDYardUnitState_Inventory", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SEP_Index_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SLS_Index_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", owner2);
			StmDataHelper.Insert(Next, "GridSettings_SLS_Index_IHVLVConsignment_99df5515-9da8-43df-9558-4db7e16e0869_d65f756a-3d9f-4523-843b-f96e32c75429", owner2);

			StmModuleFilterHelper.Insert(Next, "SDT_Index_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner2);
			StmModuleFilterHelper.Insert(Next, "RDT_Index_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", owner2);
			StmModuleFilterHelper.Insert(Next, "SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", owner2);
			StmModuleFilterHelper.Insert(Next, "SLS_Index_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", owner2);
			StmModuleFilterHelper.Insert(Next, "SDT_Index_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner1);
			StmModuleFilterHelper.Insert(Next, "RDT_Index_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", owner1);
			StmModuleFilterHelper.Insert(Next, "SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", owner1);
			StmModuleFilterHelper.Insert(Next, "SLS_Index_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", owner1);
		}

		protected override void AssertTransformationResultsCore()
		{
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType1_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType1_filterName", StmDataHelper.GetNameByPK(Next));

			AssertEquals("GridSettings_SDT_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_RDT_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_ICYDTransportationUnit_Expected_trucks", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_ICYDYardUnitState_Inventory", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SEP_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SLS_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_SLS_IHVLVConsignment_99df5515-9da8-43df-9558-4db7e16e0869_d65f756a-3d9f-4523-843b-f96e32c75429", StmDataHelper.GetNameByPK(Next));

			AssertEquals("SDT_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SDT_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", StmModuleFilterHelper.GetNameByPK(Next));
		}
	}

	[TestedType(typeof(UpdateGridSettingsRevertAddingIndexRegistryItem))]
	public class UpdateGridSettingsRevertAddingIndexRegistryItem_ConvertIndexOnesLeaveRestTest : UpdateGridSettingsRevertAddingIndexRegistryItemTestBase
	{
		protected override void PrepareTestDataCore()
		{
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType1_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType_filterName", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType1_filterName", owner2);

			StmModuleFilterHelper.Insert(Next, "SDT_Index_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner1);
			StmModuleFilterHelper.Insert(Next, "RDT_Index_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", owner1);
			StmModuleFilterHelper.Insert(Next, "SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", owner1);
			StmModuleFilterHelper.Insert(Next, "SLS_Index_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", owner1);
			StmModuleFilterHelper.Insert(Next, "SDT_Entity_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner1);
			StmModuleFilterHelper.Insert(Next, "RDT_Entity_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", owner1);
			StmModuleFilterHelper.Insert(Next, "SEP_Entity_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", owner1);
			StmModuleFilterHelper.Insert(Next, "SLS_Entity_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", owner1);
		}

		protected override void AssertTransformationResultsCore()
		{
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType1_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("GridSettings_Search_EntityType1_filterName", StmDataHelper.GetNameByPK(Next));

			AssertEquals("SDT_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SDT_Entity_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_Entity_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_Entity_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_Entity_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", StmModuleFilterHelper.GetNameByPK(Next));
		}
	}

	[TestedType(typeof(UpdateGridSettingsRevertAddingIndexRegistryItem))]
	public class UpdateGridSettingsRevertAddingIndexRegistryItem_AvoidCollisionTest : UpdateGridSettingsRevertAddingIndexRegistryItemTestBase
	{
		protected override void PrepareTestDataCore()
		{
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType_filterName", owner1);
			StmDataHelper.Insert(Next, "GridSettings_Search_EntityType_filterName", owner2);
			StmDataHelper.Insert(Next, "GridSettings_Search_Index_EntityType_filterName", owner2);
			StmModuleFilterHelper.Insert(Next, "SDT_Index_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner1);
			StmModuleFilterHelper.Insert(Next, "RDT_Index_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", owner1);
			StmModuleFilterHelper.Insert(Next, "SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", owner1);
			StmModuleFilterHelper.Insert(Next, "SLS_Index_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", owner1);
			StmModuleFilterHelper.Insert(Next, "SDT_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", owner1);
			StmModuleFilterHelper.Insert(Next, "RDT_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", owner1);
			StmModuleFilterHelper.Insert(Next, "SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", owner1);
			StmModuleFilterHelper.Insert(Next, "SLS_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", owner1);

			StmModuleFilterHelper.Insert(Next, "SDT_Index_IAdditionalService", owner1);
			StmModuleFilterHelper.Insert(Next, "RDT_Index_IIncidentRequest", owner1);
			StmModuleFilterHelper.Insert(Next, "SEP_Index_IHVLVConsignment", owner1);
			StmModuleFilterHelper.Insert(Next, "SLS_Index_5bb7cc3e", owner1);
			StmModuleFilterHelper.Insert(Next, "SDT_IAdditionalService", owner2);
			StmModuleFilterHelper.Insert(Next, "RDT_IIncidentRequest", owner2);
			StmModuleFilterHelper.Insert(Next, "SEP_IHVLVConsignment", owner2);
			StmModuleFilterHelper.Insert(Next, "SLS_5bb7cc3e", owner2);
		}

		protected override void AssertTransformationResultsCore()
		{
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals(false, StmDataHelper.Exists(Next));
			AssertEquals(false, StmDataHelper.Exists(Next));
			AssertEquals("GridSettings_Search_EntityType_filterName", StmDataHelper.GetNameByPK(Next));
			AssertEquals("SDT_IAdditionalService_0003f8778ab049d6acca601115a33417_5b706408-d4df-4268-be48-678f5017b875", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_IIncidentRequest_c007cf9b0f594aab8e41271f6d634e6d_4dfe51f9-2bd6-4ed6-8992-666960752ae6", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_5bb7cc3e-052d-49cc-b469-443e17802ce6_d623555f-4da6-4c68-b8be-7df144847fcb", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals(false, StmDataHelper.Exists(Next));
			AssertEquals(false, StmDataHelper.Exists(Next));
			AssertEquals(false, StmDataHelper.Exists(Next));
			AssertEquals(false, StmDataHelper.Exists(Next));

			AssertEquals("SDT_IAdditionalService", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_IIncidentRequest", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_IHVLVConsignment", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_5bb7cc3e", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SDT_IAdditionalService", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("RDT_IIncidentRequest", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SEP_IHVLVConsignment", StmModuleFilterHelper.GetNameByPK(Next));
			AssertEquals("SLS_5bb7cc3e", StmModuleFilterHelper.GetNameByPK(Next));
		}
	}
}
