using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test
{
	class CustomsArchiveRelationshipsTest : TestCaseWithFactory
	{
		public void TestSetupCustomsArchiveRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCustomsArchiveRelationships(stage);

			AssertEquals(58, stage.ArchiveableRelationships.Count);
			AssertEquals(178, stage.ArchiveableRelationships.Sum(a => a.Value.Count));
		}

		public void TestSetupJobShipmentRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupJobShipmentRelationships(stage);

			AssertEquals(10, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.ABL_JS_Shipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.BW_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.CU_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusExitControlHeaderSchema.Constants.TableName, CusExitControlHeaderSchema.CEH_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusHAWBSchema.Constants.TableName, CusHAWBSchema.CS_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.BH_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusSCAHouseSchema.Constants.TableName, CusSCAHouseSchema.CA_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusSCADepotHouseSchema.Constants.TableName, CusSCADepotHouseSchema.CX_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusISFHeaderSchema.Constants.TableName, CusISFHeaderSchema.BF_JS_Shipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, CusOutturnSchema.Constants.TableName, CusOutturnSchema.C5_ParentID, isReversed: false));
		}

		public void TestSetupJobConsolRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupJobConsolRelationships(stage);

			AssertEquals(4, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, AsycudaManifestHeaderSchema.Constants.TableName, AsycudaManifestHeaderSchema.AMA_ParentId, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, CusCAeMHMasterSchema.Constants.TableName, CusCAeMHMasterSchema.BP_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.BH_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, CusSCAOceanBillSchema.Constants.TableName, CusSCAOceanBillSchema.CB_ParentId, isReversed: false));
		}

		public void TestSetupJobDeclarationRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupJobDeclarationRelationships(stage);

			AssertEquals(33, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusContainerSchema.Constants.TableName, CusContainerSchema.CO_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.CU_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusDV1DetailSchema.Constants.TableName, CusDV1DetailSchema.DV1_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusEntryCPDecSchema.Constants.TableName, CusEntryCPDecSchema.ON_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.CH_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.CEI_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusEquipmentSchema.Constants.TableName, CusEquipmentSchema.CEQ_JE_Declaration, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusExitControlHeaderSchema.Constants.TableName, CusExitControlHeaderSchema.CEH_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.CXH_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusHAWBSchema.Constants.TableName, CusHAWBSchema.CS_JE_CustomsFormalEntry, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, CusHouseContPackInvoiceLinePivotSchema.CHC_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.BH_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusLiquidationSchema.Constants.TableName, CusLiquidationSchema.B8_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusPackingListSchema.Constants.TableName, CusPackingListSchema.CUL_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, CusReconDeclarationSchema.Constants.TableName, CusReconDeclarationSchema.CRD_JE_LeadDeclaration, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobCADeclarationSchema.Constants.TableName, JobCADeclarationSchema.CAD_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.JZ_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobDecRefsSchema.Constants.TableName, JobDecRefsSchema.J3_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.E2_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobDocsAndCartageSchema.Constants.TableName, JobDocsAndCartageSchema.JP_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobEUDeclarationSchema.Constants.TableName, JobEUDeclarationSchema.EUD_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.JD_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobSGDeclarationSchema.Constants.TableName, JobSGDeclarationSchema.SGE_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobShipmentPreplanningSchema.Constants.TableName, JobShipmentPreplanningSchema.EF_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, JobUSDeclarationSchema.Constants.TableName, JobUSDeclarationSchema.USD_JE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, ProcessTasksSchema.Constants.TableName, ProcessTasksSchema.P9_ParentID, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDV1DetailSchema.Constants.TableName, CusDV1DetailSchema.PK, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.CEI_DV1_Detail, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEquipmentSchema.Constants.TableName, CusEquipmentSchema.PK, CusDecHouseContainerPivotSchema.Constants.TableName, CusDecHouseContainerPivotSchema.CR_CEQ_Equipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentPreplanningSchema.Constants.TableName, JobShipmentPreplanningSchema.PK, JobOrderContainerSchema.Constants.TableName, JobOrderContainerSchema.J1_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentPreplanningSchema.Constants.TableName, JobShipmentPreplanningSchema.PK, JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning, isReversed: false));
		}

		public void TestSetupAsycudaRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupAsycudaRelationships(stage);

			AssertEquals(19, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaManifestHeaderSchema.Constants.TableName, AsycudaManifestHeaderSchema.PK, AsycudaArrivalHeaderSchema.Constants.TableName, AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaManifestHeaderSchema.Constants.TableName, AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.ABL_AMA, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaManifestHeaderSchema.Constants.TableName, AsycudaManifestHeaderSchema.PK, AsycudaContainerSchema.Constants.TableName, AsycudaContainerSchema.ACN_AMA_Manifest, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaArrivalHeaderSchema.Constants.TableName, AsycudaArrivalHeaderSchema.PK, AsycudaTransferHeaderSchema.Constants.TableName, AsycudaTransferHeaderSchema.ATF_ATH_ArrivalHeader, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaArrivalHeaderSchema.Constants.TableName, AsycudaArrivalHeaderSchema.PK, AsycudaArrivalLineSchema.Constants.TableName, AsycudaArrivalLineSchema.ATL_ATH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaArrivalLineSchema.Constants.TableName, AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaBillScreeningSchema.Constants.TableName, AsycudaBillScreeningSchema.ASR_ABL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaPackedItemSchema.Constants.TableName, AsycudaPackedItemSchema.API_ABL_Bill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.APA_ABL_Bill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaTaxSchema.Constants.TableName, AsycudaTaxSchema.AET_ABL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaBillSchema.Constants.TableName, AsycudaBillSchema.PK, AsycudaTransferBillSchema.Constants.TableName, AsycudaTransferBillSchema.ATB_ABL_Bill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaContainerSchema.Constants.TableName, AsycudaContainerSchema.PK, AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaPackedItemSchema.Constants.TableName, AsycudaPackedItemSchema.PK, AsycudaPackPackedItemPivotSchema.Constants.TableName, AsycudaPackPackedItemPivotSchema.APP_API_Item, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaPackedItemSchema.Constants.TableName, AsycudaPackedItemSchema.PK, AsycudaTaxSchema.Constants.TableName, AsycudaTaxSchema.AET_API_AsycudaPackedItem, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.PK, AsycudaArrivalLineSchema.Constants.TableName, AsycudaArrivalLineSchema.ATL_APA_AsycudaPack, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.PK, AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaPackSchema.Constants.TableName, AsycudaPackSchema.PK, AsycudaPackPackedItemPivotSchema.Constants.TableName, AsycudaPackPackedItemPivotSchema.APP_APA_Pack, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, AsycudaTransferHeaderSchema.Constants.TableName, AsycudaTransferHeaderSchema.PK, AsycudaTransferBillSchema.Constants.TableName, AsycudaTransferBillSchema.ATB_ATF_TransferHeader, isReversed: false));
		}

		public void TestSetupCusCAeManifestHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusCAeManifestHeaderRelationships(stage);

			AssertEquals(5, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusCAeMHContainerSchema.Constants.TableName, CusCAeMHContainerSchema.PK, CusCAeMHHouseContainerPivotSchema.Constants.TableName, CusCAeMHHouseContainerPivotSchema.BPA_BQ_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHHouseContainerPivotSchema.Constants.TableName, CusCAeMHHouseContainerPivotSchema.BPA_BW_House, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHItemSchema.Constants.TableName, CusCAeMHItemSchema.BX_BW_House, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusCAeMHMasterSchema.Constants.TableName, CusCAeMHMasterSchema.PK, CusCAeMHContainerSchema.Constants.TableName, CusCAeMHContainerSchema.BQ_BP_Master, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusCAeMHMasterSchema.Constants.TableName, CusCAeMHMasterSchema.PK, CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.BW_BP_Master, isReversed: false));
		}

		public void TestSetupCusDecHouseRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusDecHouseRelationships(stage);

			AssertEquals(7, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.PK, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.CU_CU_ParentBill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.PK, CusDecHouseContainerPivotSchema.Constants.TableName, CusDecHouseContainerPivotSchema.CR_CU_HouseBill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.PK, CusUSDecHouseBillSchema.Constants.TableName, CusUSDecHouseBillSchema.USB_CU, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseBillSchema.Constants.TableName, CusDecHouseBillSchema.PK, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseContainerPackSchema.Constants.TableName, CusDecHouseContainerPackSchema.PK, CusDecHouseContainerPackSchema.Constants.TableName, CusDecHouseContainerPackSchema.CW_CW_Parent, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseContainerPackSchema.Constants.TableName, CusDecHouseContainerPackSchema.PK, CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, CusHouseContPackInvoiceLinePivotSchema.CHC_CW, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseContainerPackSchema.Constants.TableName, CusDecHouseContainerPackSchema.PK, CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_CW, isReversed: false));
		}

		public void TestSetupCusContainerHouseRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusContainerRelationships(stage);

			AssertEquals(1, stage.ArchiveableRelationships.Sum(a => a.Value.Count));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusContainerSchema.Constants.TableName, CusContainerSchema.PK, QuarantineColsDirectionSchema.Constants.TableName, QuarantineColsDirectionSchema.QCD_CO_Container, isReversed: false));
		}

		public void TestSetupCusEntryRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusEntryRelationships(stage);

			AssertEquals(26, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.PK, CusCNEntryInstructionSchema.Constants.TableName, CusCNEntryInstructionSchema.CNE_CEI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.PK, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.JI_CEI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.PK, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.CH_CEI_Instruction, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.PK, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.BH_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.PK, CusTWControllingMessageHeaderSchema.Constants.TableName, CusTWControllingMessageHeaderSchema.TW1_CEI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.PK, CusContainerEntryInstructionPivotSchema.Constants.TableName, CusContainerEntryInstructionPivotSchema.CEP_CEI_EntryInstruction, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusContainerEntryHeaderPivotSchema.Constants.TableName, CusContainerEntryHeaderPivotSchema.CCE_CH_EntryHeader, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusEntryCPDecSchema.Constants.TableName, CusEntryCPDecSchema.ON_CH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.CH_CH_PrimeEntry, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusEntryHeaderChargesSchema.Constants.TableName, CusEntryHeaderChargesSchema.C1_CH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusEntryLineSchema.Constants.TableName, CusEntryLineSchema.CL_CH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusReconEntrySchema.Constants.TableName, CusReconEntrySchema.CRE_CH_OriginalEntry, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryLineSchema.Constants.TableName, CusEntryLineSchema.PK, CusEntryLineFeeSchema.Constants.TableName, CusEntryLineFeeSchema.CF_CL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryLineSchema.Constants.TableName, CusEntryLineSchema.PK, CusUnderbondDecSchema.Constants.TableName, CusUnderbondDecSchema.BU_CL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryLineSchema.Constants.TableName, CusEntryLineSchema.PK, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.JI_CL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryLineSchema.Constants.TableName, CusEntryLineSchema.PK, QuarantineColsDirectionSchema.Constants.TableName, QuarantineColsDirectionSchema.QCD_CL_CusEntryLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusEntryPayInfoSchema.Constants.TableName, CusEntryPayInfoSchema.C9_CH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusEntrySnapshotSchema.Constants.TableName, CusEntrySnapshotSchema.CES_CH_EntryHeader, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, CusEUEntryHeaderSchema.Constants.TableName, CusEUEntryHeaderSchema.EUH_CH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusEntryHeaderSchema.Constants.TableName, CusEntryHeaderSchema.PK, QuarantineColsHeaderSchema.Constants.TableName, QuarantineColsHeaderSchema.QCH_CH_CusEntryHeader, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusReconDeclarationSchema.Constants.TableName, CusReconDeclarationSchema.PK, CusReconEntrySchema.Constants.TableName, CusReconEntrySchema.CRE_CRD, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusReconEntrySchema.Constants.TableName, CusReconEntrySchema.PK, CusReconSnapshotSchema.Constants.TableName, CusReconSnapshotSchema.CRS_CRE_Entry, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusReconEntrySchema.Constants.TableName, CusReconEntrySchema.PK, CusReconEntryLineSchema.Constants.TableName, CusReconEntryLineSchema.CRL_CRE, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusReconEntryLineSchema.Constants.TableName, CusReconEntryLineSchema.PK, CusReconSnapshotSchema.Constants.TableName, CusReconSnapshotSchema.CRS_CRL_Line, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusReconEntryLineSchema.Constants.TableName, CusReconEntryLineSchema.PK, CusReconCustomsChargeSchema.Constants.TableName, CusReconCustomsChargeSchema.CRC_CRL_Line, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTWControllingMessageHeaderSchema.Constants.TableName, CusTWControllingMessageHeaderSchema.PK, CusTWProductLabelRangeSchema.Constants.TableName, CusTWProductLabelRangeSchema.TW0_TW1, isReversed: false));
		}

		public void TestSetupCusExitControlHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusExitControlHeaderRelationships(stage);

			AssertEquals(4, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitControlHeaderSchema.Constants.TableName, CusExitControlHeaderSchema.PK, CusExitDetailSchema.Constants.TableName, CusExitDetailSchema.CED_CEH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitDetailSchema.Constants.TableName, CusExitDetailSchema.PK, CusExitItemSchema.Constants.TableName, CusExitItemSchema.CXI_CED, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitItemSchema.Constants.TableName, CusExitItemSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInvPackSchema.Constants.TableName, CusInvPackSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_B5_ParentPackage, isReversed: false));
		}

		public void TestSetupCusExitHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusExitHeaderRelationships(stage);

			AssertEquals(13, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitConsignmentPackageSchema.Constants.TableName, CusExitConsignmentPackageSchema.CXP_CXH_Header, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitConsignmentSchema.Constants.TableName, CusExitConsignmentSchema.CXC_CXH_Header, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitContainerSchema.Constants.TableName, CusExitContainerSchema.CXN_CXH_Header, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitHeaderSchema.Constants.TableName, CusExitHeaderSchema.PK, CusExitReportSchema.Constants.TableName, CusExitReportSchema.CER_CXH_Header, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitConsignmentItemSchema.Constants.TableName, CusExitConsignmentItemSchema.PK, CusExitConsignmentPivotSchema.Constants.TableName, CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitConsignmentItemSchema.Constants.TableName, CusExitConsignmentItemSchema.PK, CusExitReportItemSchema.Constants.TableName, CusExitReportItemSchema.ERI_CCI_ConsignmentItem, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitConsignmentPackageSchema.Constants.TableName, CusExitConsignmentPackageSchema.PK, CusExitConsignmentPivotSchema.Constants.TableName, CusExitConsignmentPivotSchema.CNP_CXP_Package, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitConsignmentPackageSchema.Constants.TableName, CusExitConsignmentPackageSchema.PK, CusExitReportItemSchema.Constants.TableName, CusExitReportItemSchema.ERI_CXP_Package, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitConsignmentSchema.Constants.TableName, CusExitConsignmentSchema.PK, CusExitConsignmentItemSchema.Constants.TableName, CusExitConsignmentItemSchema.CCI_CXC_Consignment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitConsignmentSchema.Constants.TableName, CusExitConsignmentSchema.PK, CusExitReportSchema.Constants.TableName, CusExitReportSchema.CER_CXC_Consignment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitContainerSchema.Constants.TableName, CusExitContainerSchema.PK, CusExitConsignmentPivotSchema.Constants.TableName, CusExitConsignmentPivotSchema.CNP_CXN_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitReportSchema.Constants.TableName, CusExitReportSchema.PK, CusExitReportItemSchema.Constants.TableName, CusExitReportItemSchema.ERI_CER_Report, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusExitReportSchema.Constants.TableName, CusExitReportSchema.PK, CusExitReportSchema.Constants.TableName, CusExitReportSchema.CER_CER_ExitReport, isReversed: false));
		}

		public void TestSetupCusInBondHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusInBondHeaderRelationships(stage);

			AssertEquals(29, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusESNctsHeaderSchema.Constants.TableName, CusESNctsHeaderSchema.CEN_BH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusFRNctsHeaderSchema.Constants.TableName, CusFRNctsHeaderSchema.CFN_BH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.B0_BH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondEquipmentSchema.Constants.TableName, CusInBondEquipmentSchema.BJ_BH_Header, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondEventSchema.Constants.TableName, CusInBondEventSchema.BN_BH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondPersonSchema.Constants.TableName, CusInBondPersonSchema.CP_BH_Header, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.BM_BH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.PK, CusInbondBillAddRefSchema.Constants.TableName, CusInbondBillAddRefSchema.BR_B0, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.PK, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.B9_B0, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_BY_Commodity, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondFeeSchema.Constants.TableName, CusInBondFeeSchema.BFE_BY, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.PK, CusInBondVehicleCtrlSchema.Constants.TableName, CusInBondVehicleCtrlSchema.BV_BC, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondEquipmentSchema.Constants.TableName, CusInBondEquipmentSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_BJ_Equipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondEventSchema.Constants.TableName, CusInBondEventSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondMoveLineItemSchema.Constants.TableName, CusInBondMoveLineItemSchema.BI_B9, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.B9_BM, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.BM_BM_DepartureMovement, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondPayInfoSchema.Constants.TableName, CusInBondPayInfoSchema.BPI_BM, isReversed: false));
		}

		public void TestSetupJobComInvoiceHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupJobComInvoiceHeaderRelationships(stage);

			AssertEquals(10, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, JobCAComInvoiceHeaderSchema.Constants.TableName, JobCAComInvoiceHeaderSchema.CAZ_JZ, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, JobComInvoiceHeaderRefsSchema.Constants.TableName, JobComInvoiceHeaderRefsSchema.J2_JZ, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.JI_JZ, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JZ, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, CusPackingListSchema.Constants.TableName, CusPackingListSchema.CUL_JZ, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.PK, QuarantineExDocHeaderSchema.Constants.TableName, QuarantineExDocHeaderSchema.QH_JZ, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusPackingListSchema.Constants.TableName, CusPackingListSchema.PK, CusPackableItemSchema.Constants.TableName, CusPackableItemSchema.CUI_CUL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, QuarantineExDocHeaderSchema.Constants.TableName, QuarantineExDocHeaderSchema.PK, QuarantineExDocShipsCompartmentSchema.Constants.TableName, QuarantineExDocShipsCompartmentSchema.QC_QH, isReversed: false));
		}

		public void TestSetupJobComInvoiceLineRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupJobComInvoiceLineRelationships(stage);

			AssertEquals(3, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, CusPackableItemSchema.Constants.TableName, CusPackableItemSchema.CUI_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, JobUSComInvoiceLineSchema.Constants.TableName, JobUSComInvoiceLineSchema.USI_JI, isReversed: false));
		}

		public void TestSetupCusCAeMHHouseRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusCAeMHHouseRelationships(stage);

			AssertEquals(2, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHHouseContainerPivotSchema.Constants.TableName, CusCAeMHHouseContainerPivotSchema.BPA_BW_House, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHItemSchema.Constants.TableName, CusCAeMHItemSchema.BX_BW_House, isReversed: false));
		}

		public void TestSetupCusUnderbondRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusUnderbondRelationships(stage);

			AssertEquals(1, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusUnderbondSchema.Constants.TableName, CusUnderbondSchema.PK, CusOutturnSchema.Constants.TableName, CusOutturnSchema.C5_C4_Underbond, isReversed: false));
		}

		public void TestSetupCusOutturnRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusOutturnRelationships(stage);

			AssertEquals(3, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusOutturnSchema.Constants.TableName, CusOutturnSchema.C5_C6, CusOutturnHeaderSchema.Constants.TableName, CusOutturnHeaderSchema.PK, isReversed: true));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusOutturnHeaderSchema.Constants.TableName, CusOutturnHeaderSchema.PK, CusOutturnSchema.Constants.TableName, CusOutturnSchema.C5_C6, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusOutturnHeaderSchema.Constants.TableName, CusOutturnHeaderSchema.PK, CusUnderbondSchema.Constants.TableName, CusUnderbondSchema.C4_C6, isReversed: false));
		}

		public void TestSetupSeaCargoRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupSeaCargoRelationships(stage);

			AssertEquals(5, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusSCAOceanBillSchema.Constants.TableName, CusSCAOceanBillSchema.PK, CusSCAContainerSchema.Constants.TableName, CusSCAContainerSchema.CN_CB, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusSCAOceanBillSchema.Constants.TableName, CusSCAOceanBillSchema.PK, CusSCAHouseSchema.Constants.TableName, CusSCAHouseSchema.CA_CB, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusSCAOceanBillSchema.Constants.TableName, CusSCAOceanBillSchema.PK, CusSCAPivotSchema.Constants.TableName, CusSCAPivotSchema.CV_CB, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusSCAContainerSchema.Constants.TableName, CusSCAContainerSchema.PK, CusSCAPivotSchema.Constants.TableName, CusSCAPivotSchema.CV_CN, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusSCAHouseSchema.Constants.TableName, CusSCAHouseSchema.PK, CusSCAPivotSchema.Constants.TableName, CusSCAPivotSchema.CV_CA, isReversed: false));
		}

		public void TestSetupEcommerceRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupEcommerceRelationships(stage);

			AssertEquals(4, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, CusUSLVConsignmentSchema.Constants.TableName, CusUSLVConsignmentSchema.ULB_HVC_Consignment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusUSLVConsignmentSchema.Constants.TableName, CusUSLVConsignmentSchema.PK, CusUSLVItemSchema.Constants.TableName, CusUSLVItemSchema.ULI_ULB, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusUSLVConsignmentSchema.Constants.TableName, CusUSLVConsignmentSchema.ULB_ULH, CusUSLVClearanceSchema.Constants.TableName, CusUSLVClearanceSchema.PK, isReversed: true));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusUSLVItemSchema.Constants.TableName, CusUSLVItemSchema.PK, CusUSLVItemPGASchema.Constants.TableName, CusUSLVItemPGASchema.ULP_ULI, isReversed: false));
		}

		#region StandaloneRecordsArchiveSystem (STA)

		public void TestSetupCusTempStorageRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusTempStorageRelationships(stage);

			AssertEquals(5, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageJobHeaderSchema.Constants.TableName, CusTempStorageJobHeaderSchema.PK, CusTempStorageDecSchema.Constants.TableName, CusTempStorageDecSchema.STH_SJH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageDecSchema.Constants.TableName, CusTempStorageDecSchema.PK, CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.TSL_STH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.PK, CusTempStorageLineItemSchema.Constants.TableName, CusTempStorageLineItemSchema.TSI_TSL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.PK, CusTempStorageLinePivotSchema.Constants.TableName, CusTempStorageLinePivotSchema.SLR_TSL_FromLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageLineSchema.Constants.TableName, CusTempStorageLineSchema.PK, CusTempStorageLinePivotSchema.Constants.TableName, CusTempStorageLinePivotSchema.SLR_TSL_ToLine, isReversed: false));
		}

		public void TestSetupCusTempStorageRegHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCusTempStorageRegHeaderRelationships(stage);

			AssertEquals(5, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageRegHeaderSchema.Constants.TableName, CusTempStorageRegHeaderSchema.PK, CusTempStorageRegLineSchema.Constants.TableName, CusTempStorageRegLineSchema.SRL_SRH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageRegLineSchema.Constants.TableName, CusTempStorageRegLineSchema.PK, CusTempStorageRegLineItemPivotSchema.Constants.TableName, CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageRegLineItemSchema.Constants.TableName, CusTempStorageRegLineItemSchema.PK, CusTempStorageRegLineItemPivotSchema.Constants.TableName, CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageRegLineSchema.Constants.TableName, CusTempStorageRegLineSchema.PK, CusTempStorageRegLineTransactionSchema.Constants.TableName, CusTempStorageRegLineTransactionSchema.SRT_SRL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusTempStorageRegPremisesSchema.Constants.TableName, CusTempStorageRegPremisesSchema.PK, CusTempStorageRegHeaderSchema.Constants.TableName, CusTempStorageRegHeaderSchema.SRH_SRP_Premises, isReversed: false));
		}

		#endregion
	}
}
