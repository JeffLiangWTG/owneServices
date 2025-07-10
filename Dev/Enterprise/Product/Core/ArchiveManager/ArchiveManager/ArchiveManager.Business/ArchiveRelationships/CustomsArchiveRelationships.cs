using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	public static class CustomsArchiveRelationships
	{
		public static void SetupCustomsArchiveRelationships(IArchiveSystemSetup systemSetup)
		{
			SetupJobShipmentRelationships(systemSetup);
			SetupJobConsolRelationships(systemSetup);
			SetupJobDeclarationRelationships(systemSetup);

			SetupAsycudaRelationships(systemSetup);
			SetupCusCAeManifestHeaderRelationships(systemSetup);
			SetupCusContainerRelationships(systemSetup);
			SetupCusDecHouseRelationships(systemSetup);
			SetupCusEntryRelationships(systemSetup);
			SetupCusExitControlHeaderRelationships(systemSetup);
			SetupCusExitHeaderRelationships(systemSetup);
			SetupCusInBondHeaderRelationships(systemSetup);
			SetupJobComInvoiceHeaderRelationships(systemSetup);
			SetupJobComInvoiceLineRelationships(systemSetup);
			SetupSeaCargoRelationships(systemSetup);
			SetupCusOutturnRelationships(systemSetup);
			SetupCusUnderbondRelationships(systemSetup);
			SetupEcommerceRelationships(systemSetup);
			systemSetup.AddRelationship(JobOrderLineSchema.PK, JobComInvoiceLineSchema.JI_JO);

			ClientHookLoader.Instance.ClientHook?.SetupClientSpecificRelationships(systemSetup);
		}

		public static void SetupJobShipmentRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobShipmentSchema.PK, AsycudaBillSchema.ABL_JS_Shipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusCAeMHHouseSchema.BW_ParentID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusDecHouseBillSchema.CU_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusExitControlHeaderSchema.CEH_ParentID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusHAWBSchema.CS_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusInBondHeaderSchema.BH_ParentID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusISFHeaderSchema.BF_JS_Shipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusSCAHouseSchema.CA_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusSCADepotHouseSchema.CX_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, CusOutturnSchema.C5_ParentID);
		}

		public static void SetupJobConsolRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobConsolSchema.PK, AsycudaManifestHeaderSchema.AMA_ParentId);
			systemSetup.AddRelationship(JobConsolSchema.PK, CusCAeMHMasterSchema.BP_ParentID);
			systemSetup.AddRelationship(JobConsolSchema.PK, CusInBondHeaderSchema.BH_ParentID);
			systemSetup.AddRelationship(JobConsolSchema.PK, CusSCAOceanBillSchema.CB_ParentId);
		}

		public static void SetupJobDeclarationRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusContainerSchema.CO_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusDecHouseBillSchema.CU_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusDV1DetailSchema.DV1_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusEntryCPDecSchema.ON_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusEntryHeaderSchema.CH_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusEntryInstructionSchema.CEI_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusEquipmentSchema.CEQ_JE_Declaration);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusExitControlHeaderSchema.CEH_ParentID);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusExitHeaderSchema.CXH_ParentID);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusHAWBSchema.CS_JE_CustomsFormalEntry);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusHouseContPackInvoiceLinePivotSchema.CHC_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusInBondContainerSchema.BC_ParentID);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusInBondHeaderSchema.BH_ParentID);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusInvPackSchema.B5_ParentID);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusLiquidationSchema.B8_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusPackingListSchema.CUL_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, CusReconDeclarationSchema.CRD_JE_LeadDeclaration);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobCADeclarationSchema.CAD_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobComInvoiceHeaderSchema.JZ_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobDecRefsSchema.J3_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobDocAddressSchema.E2_ParentID);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobDocsAndCartageSchema.JP_ParentID);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobEUDeclarationSchema.EUD_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobOrderHeaderSchema.JD_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobSGDeclarationSchema.SGE_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobShipmentPreplanningSchema.EF_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobUSDeclarationSchema.USD_JE);
			systemSetup.AddRelationship(JobDeclarationSchema.PK, ProcessTasksSchema.P9_ParentID);

			systemSetup.AddRelationship(CusDV1DetailSchema.PK, CusEntryInstructionSchema.CEI_DV1_Detail);
			systemSetup.AddRelationship(CusEquipmentSchema.PK, CusDecHouseContainerPivotSchema.CR_CEQ_Equipment);
			systemSetup.AddRelationship(JobShipmentPreplanningSchema.PK, JobOrderContainerSchema.J1_ParentID);
			systemSetup.AddRelationship(JobShipmentPreplanningSchema.PK, JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning);
		}

		public static void SetupAsycudaRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(AsycudaManifestHeaderSchema.PK, AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader);
			systemSetup.AddRelationship(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA);
			systemSetup.AddRelationship(AsycudaManifestHeaderSchema.PK, AsycudaContainerSchema.ACN_AMA_Manifest);
			systemSetup.AddRelationship(AsycudaArrivalHeaderSchema.PK, AsycudaTransferHeaderSchema.ATF_ATH_ArrivalHeader);
			systemSetup.AddRelationship(AsycudaArrivalHeaderSchema.PK, AsycudaArrivalLineSchema.ATL_ATH);
			systemSetup.AddRelationship(AsycudaBillSchema.PK, AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill);
			systemSetup.AddRelationship(AsycudaBillSchema.PK, AsycudaBillScreeningSchema.ASR_ABL);
			systemSetup.AddRelationship(AsycudaBillSchema.PK, AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill);
			systemSetup.AddRelationship(AsycudaBillSchema.PK, AsycudaPackedItemSchema.API_ABL_Bill);
			systemSetup.AddRelationship(AsycudaBillSchema.PK, AsycudaPackSchema.APA_ABL_Bill);
			systemSetup.AddRelationship(AsycudaBillSchema.PK, AsycudaTaxSchema.AET_ABL);
			systemSetup.AddRelationship(AsycudaBillSchema.PK, AsycudaTransferBillSchema.ATB_ABL_Bill);
			systemSetup.AddRelationship(AsycudaContainerSchema.PK, AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container);
			systemSetup.AddRelationship(AsycudaPackedItemSchema.PK, AsycudaPackPackedItemPivotSchema.APP_API_Item);
			systemSetup.AddRelationship(AsycudaPackedItemSchema.PK, AsycudaTaxSchema.AET_API_AsycudaPackedItem);
			systemSetup.AddRelationship(AsycudaPackSchema.PK, AsycudaArrivalLineSchema.ATL_APA_AsycudaPack);
			systemSetup.AddRelationship(AsycudaPackSchema.PK, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack);
			systemSetup.AddRelationship(AsycudaPackSchema.PK, AsycudaPackPackedItemPivotSchema.APP_APA_Pack);
			systemSetup.AddRelationship(AsycudaTransferHeaderSchema.PK, AsycudaTransferBillSchema.ATB_ATF_TransferHeader);
		}

		public static void SetupCusCAeManifestHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusCAeMHContainerSchema.PK, CusCAeMHHouseContainerPivotSchema.BPA_BQ_Container);
			SetupCusCAeMHHouseRelationships(systemSetup);
			systemSetup.AddRelationship(CusCAeMHMasterSchema.PK, CusCAeMHContainerSchema.BQ_BP_Master);
			systemSetup.AddRelationship(CusCAeMHMasterSchema.PK, CusCAeMHHouseSchema.BW_BP_Master);
		}

		public static void SetupCusContainerRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusContainerSchema.PK, QuarantineColsDirectionSchema.QCD_CO_Container);
		}

		public static void SetupCusDecHouseRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusDecHouseBillSchema.PK, CusDecHouseBillSchema.CU_CU_ParentBill);
			systemSetup.AddRelationship(CusDecHouseBillSchema.PK, CusDecHouseContainerPivotSchema.CR_CU_HouseBill);
			systemSetup.AddRelationship(CusDecHouseBillSchema.PK, CusUSDecHouseBillSchema.USB_CU);
			systemSetup.AddRelationship(CusDecHouseBillSchema.PK, JobComInvoiceHeaderSchema.JZ_CU_RelatedHouseBill);

			systemSetup.AddRelationship(CusDecHouseContainerPackSchema.PK, CusDecHouseContainerPackSchema.CW_CW_Parent);
			systemSetup.AddRelationship(CusDecHouseContainerPackSchema.PK, CusHouseContPackInvoiceLinePivotSchema.CHC_CW);
			systemSetup.AddRelationship(CusDecHouseContainerPackSchema.PK, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_CW);
		}

		public static void SetupCusEntryRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusEntryInstructionSchema.PK, CusCNEntryInstructionSchema.CNE_CEI);
			systemSetup.AddRelationship(CusEntryInstructionSchema.PK, JobComInvoiceLineSchema.JI_CEI);
			systemSetup.AddRelationship(CusEntryInstructionSchema.PK, CusContainerEntryInstructionPivotSchema.CEP_CEI_EntryInstruction);
			systemSetup.AddRelationship(CusEntryInstructionSchema.PK, CusEntryHeaderSchema.CH_CEI_Instruction);
			systemSetup.AddRelationship(CusEntryInstructionSchema.PK, CusInBondHeaderSchema.BH_ParentID);
			systemSetup.AddRelationship(CusEntryInstructionSchema.PK, CusTWControllingMessageHeaderSchema.TW1_CEI);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusContainerEntryHeaderPivotSchema.CCE_CH_EntryHeader);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusEntryCPDecSchema.ON_CH);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusEntryHeaderSchema.CH_CH_PrimeEntry);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusEntryHeaderChargesSchema.C1_CH);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusEntryLineSchema.CL_CH);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusReconEntrySchema.CRE_CH_OriginalEntry);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, QuarantineColsHeaderSchema.QCH_CH_CusEntryHeader);
			systemSetup.AddRelationship(CusEntryLineSchema.PK, CusEntryLineFeeSchema.CF_CL);
			systemSetup.AddRelationship(CusEntryLineSchema.PK, CusUnderbondDecSchema.BU_CL);
			systemSetup.AddRelationship(CusEntryLineSchema.PK, JobComInvoiceLineSchema.JI_CL);
			systemSetup.AddRelationship(CusEntryLineSchema.PK, QuarantineColsDirectionSchema.QCD_CL_CusEntryLine);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusEntryPayInfoSchema.C9_CH);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusEntrySnapshotSchema.CES_CH_EntryHeader);
			systemSetup.AddRelationship(CusEntryHeaderSchema.PK, CusEUEntryHeaderSchema.EUH_CH);
			systemSetup.AddRelationship(CusReconDeclarationSchema.PK, CusReconEntrySchema.CRE_CRD);
			systemSetup.AddRelationship(CusReconEntrySchema.PK, CusReconSnapshotSchema.CRS_CRE_Entry);
			systemSetup.AddRelationship(CusReconEntrySchema.PK, CusReconEntryLineSchema.CRL_CRE);
			systemSetup.AddRelationship(CusReconEntryLineSchema.PK, CusReconCustomsChargeSchema.CRC_CRL_Line);
			systemSetup.AddRelationship(CusReconEntryLineSchema.PK, CusReconSnapshotSchema.CRS_CRL_Line);
			systemSetup.AddRelationship(CusTWControllingMessageHeaderSchema.PK, CusTWProductLabelRangeSchema.TW0_TW1);
		}

		public static void SetupCusExitControlHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusExitControlHeaderSchema.PK, CusExitDetailSchema.CED_CEH);
			systemSetup.AddRelationship(CusExitDetailSchema.PK, CusExitItemSchema.CXI_CED);
			systemSetup.AddRelationship(CusExitItemSchema.PK, CusInvPackSchema.B5_ParentID);
			systemSetup.AddRelationship(CusInvPackSchema.PK, CusInvPackSchema.B5_B5_ParentPackage);
		}

		public static void SetupCusExitHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusExitHeaderSchema.PK, CusExitConsignmentPackageSchema.CXP_CXH_Header);
			systemSetup.AddRelationship(CusExitHeaderSchema.PK, CusExitConsignmentSchema.CXC_CXH_Header);
			systemSetup.AddRelationship(CusExitHeaderSchema.PK, CusExitContainerSchema.CXN_CXH_Header);
			systemSetup.AddRelationship(CusExitHeaderSchema.PK, CusExitReportSchema.CER_CXH_Header);

			systemSetup.AddRelationship(CusExitConsignmentItemSchema.PK, CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem);
			systemSetup.AddRelationship(CusExitConsignmentItemSchema.PK, CusExitReportItemSchema.ERI_CCI_ConsignmentItem);
			systemSetup.AddRelationship(CusExitConsignmentPackageSchema.PK, CusExitConsignmentPivotSchema.CNP_CXP_Package);
			systemSetup.AddRelationship(CusExitConsignmentPackageSchema.PK, CusExitReportItemSchema.ERI_CXP_Package);
			systemSetup.AddRelationship(CusExitConsignmentSchema.PK, CusExitConsignmentItemSchema.CCI_CXC_Consignment);
			systemSetup.AddRelationship(CusExitConsignmentSchema.PK, CusExitReportSchema.CER_CXC_Consignment);
			systemSetup.AddRelationship(CusExitContainerSchema.PK, CusExitConsignmentPivotSchema.CNP_CXN_Container);
			systemSetup.AddRelationship(CusExitReportSchema.PK, CusExitReportItemSchema.ERI_CER_Report);
			systemSetup.AddRelationship(CusExitReportSchema.PK, CusExitReportSchema.CER_CER_ExitReport);
		}

		public static void SetupCusInBondHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusESNctsHeaderSchema.CEN_BH);
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusFRNctsHeaderSchema.CFN_BH);
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusInBondBillSchema.B0_BH);
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusInBondContainerSchema.BC_ParentID);
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusInBondEquipmentSchema.BJ_BH_Header);
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusInBondEventSchema.BN_BH);
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusInBondPersonSchema.CP_BH_Header);
			systemSetup.AddRelationship(CusInBondHeaderSchema.PK, CusInBondMoveHeaderSchema.BM_BH);
			systemSetup.AddRelationship(CusInBondBillSchema.PK, CusInbondBillAddRefSchema.BR_B0);
			systemSetup.AddRelationship(CusInBondBillSchema.PK, CusInBondCargoDescSchema.BY_ParentID);
			systemSetup.AddRelationship(CusInBondBillSchema.PK, CusInBondMoveDetailSchema.B9_B0);
			systemSetup.AddRelationship(CusInBondCargoDescSchema.PK, CusInBondCargoDescSchema.BY_BY_Commodity);
			systemSetup.AddRelationship(CusInBondCargoDescSchema.PK, CusInBondCargoDescSchema.BY_ParentID);
			systemSetup.AddRelationship(CusInBondCargoDescSchema.PK, CusInBondContainerSchema.BC_ParentID);
			systemSetup.AddRelationship(CusInBondCargoDescSchema.PK, CusInBondFeeSchema.BFE_BY);
			systemSetup.AddRelationship(CusInBondCargoDescSchema.PK, CusInvPackSchema.B5_ParentID);
			systemSetup.AddRelationship(CusInBondContainerSchema.PK, CusInBondCargoDescSchema.BY_ParentID);
			systemSetup.AddRelationship(CusInBondContainerSchema.PK, CusInBondVehicleCtrlSchema.BV_BC);
			systemSetup.AddRelationship(CusInBondEquipmentSchema.PK, CusInBondCargoDescSchema.BY_BJ_Equipment);
			systemSetup.AddRelationship(CusInBondEventSchema.PK, CusInBondContainerSchema.BC_ParentID);
			systemSetup.AddRelationship(CusInBondMoveDetailSchema.PK, CusInBondCargoDescSchema.BY_ParentID);
			systemSetup.AddRelationship(CusInBondMoveDetailSchema.PK, CusInBondContainerSchema.BC_ParentID);
			systemSetup.AddRelationship(CusInBondMoveDetailSchema.PK, CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail);
			systemSetup.AddRelationship(CusInBondMoveDetailSchema.PK, CusInBondMoveLineItemSchema.BI_B9);
			systemSetup.AddRelationship(CusInBondMoveDetailSchema.PK, CusInvPackSchema.B5_ParentID);
			systemSetup.AddRelationship(CusInBondMoveHeaderSchema.PK, CusInBondCargoDescSchema.BY_ParentID);
			systemSetup.AddRelationship(CusInBondMoveHeaderSchema.PK, CusInBondMoveDetailSchema.B9_BM);
			systemSetup.AddRelationship(CusInBondMoveHeaderSchema.PK, CusInBondMoveHeaderSchema.BM_BM_DepartureMovement);
			systemSetup.AddRelationship(CusInBondMoveHeaderSchema.PK, CusInBondPayInfoSchema.BPI_BM);
		}

		public static void SetupJobComInvoiceHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, JobCAComInvoiceHeaderSchema.CAZ_JZ);
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, JobComInvoiceHeaderRefsSchema.J2_JZ);
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, JobComInvoiceHeaderSchema.JZ_JZ_GroupInvoiceFK);
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, JobComInvoiceLineSchema.JI_JZ);
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JZ);
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, CusInvPackSchema.B5_ParentID);
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, CusPackingListSchema.CUL_JZ);
			systemSetup.AddRelationship(JobComInvoiceHeaderSchema.PK, QuarantineExDocHeaderSchema.QH_JZ);

			systemSetup.AddRelationship(CusPackingListSchema.PK, CusPackableItemSchema.CUI_CUL);
			systemSetup.AddRelationship(QuarantineExDocHeaderSchema.PK, QuarantineExDocShipsCompartmentSchema.QC_QH);
		}

		public static void SetupJobComInvoiceLineRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, CusInvPackSchema.B5_ParentID);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, CusPackableItemSchema.CUI_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, JobUSComInvoiceLineSchema.USI_JI);
		}

		public static void SetupCusCAeMHHouseRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusCAeMHHouseSchema.PK, CusCAeMHHouseContainerPivotSchema.BPA_BW_House);
			systemSetup.AddRelationship(CusCAeMHHouseSchema.PK, CusCAeMHItemSchema.BX_BW_House);
		}

		public static void SetupSeaCargoRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusSCAOceanBillSchema.PK, CusSCAContainerSchema.CN_CB);
			systemSetup.AddRelationship(CusSCAOceanBillSchema.PK, CusSCAHouseSchema.CA_CB);
			systemSetup.AddRelationship(CusSCAOceanBillSchema.PK, CusSCAPivotSchema.CV_CB);
			systemSetup.AddRelationship(CusSCAContainerSchema.PK, CusSCAPivotSchema.CV_CN);
			systemSetup.AddRelationship(CusSCAHouseSchema.PK, CusSCAPivotSchema.CV_CA);
		}

		public static void SetupCusOutturnRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusOutturnSchema.C5_C6, CusOutturnHeaderSchema.PK, isReversed: true);
			systemSetup.AddRelationship(CusOutturnHeaderSchema.PK, CusOutturnSchema.C5_C6);
			systemSetup.AddRelationship(CusOutturnHeaderSchema.PK, CusUnderbondSchema.C4_C6);
		}

		public static void SetupCusUnderbondRelationships(IArchiveSystemSetup systemSetup)
			=> systemSetup.AddRelationship(CusUnderbondSchema.PK, CusOutturnSchema.C5_C4_Underbond);

		public static void SetupEcommerceRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(HVLVConsignmentSchema.PK, CusUSLVConsignmentSchema.ULB_HVC_Consignment);
			systemSetup.AddRelationship(CusUSLVConsignmentSchema.PK, CusUSLVItemSchema.ULI_ULB);
			systemSetup.AddRelationship(CusUSLVConsignmentSchema.ULB_ULH, CusUSLVClearanceSchema.PK, isReversed: true);
			systemSetup.AddRelationship(CusUSLVItemSchema.PK, CusUSLVItemPGASchema.ULP_ULI);
		}

		#region StandaloneRecordsArchiveSystem (STA)

		public static void SetupCusTempStorageRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusTempStorageJobHeaderSchema.PK, CusTempStorageDecSchema.STH_SJH);
			systemSetup.AddRelationship(CusTempStorageDecSchema.PK, CusTempStorageLineSchema.TSL_STH);
			systemSetup.AddRelationship(CusTempStorageLineSchema.PK, CusTempStorageLineItemSchema.TSI_TSL);
			systemSetup.AddRelationship(CusTempStorageLineSchema.PK, CusTempStorageLinePivotSchema.SLR_TSL_FromLine);
			systemSetup.AddRelationship(CusTempStorageLineSchema.PK, CusTempStorageLinePivotSchema.SLR_TSL_ToLine);
		}

		public static void SetupCusTempStorageRegHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusTempStorageRegHeaderSchema.PK, CusTempStorageRegLineSchema.SRL_SRH);
			systemSetup.AddRelationship(CusTempStorageRegLineSchema.PK, CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line);
			systemSetup.AddRelationship(CusTempStorageRegLineItemSchema.PK, CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item);
			systemSetup.AddRelationship(CusTempStorageRegLineSchema.PK, CusTempStorageRegLineTransactionSchema.SRT_SRL);
			systemSetup.AddRelationship(CusTempStorageRegPremisesSchema.PK, CusTempStorageRegHeaderSchema.SRH_SRP_Premises);
		}

		public static void SetupCusIntrastatGroupRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusIntrastatGroupSchema.PK, CusIntrastatHeaderSchema.CIH_CIG_MergedGroup);
			systemSetup.AddRelationship(CusIntrastatGroupSchema.PK, CusIntrastatMergedLineSchema.CIM_CIG_Group);
			SetupCusIntrastatHeaderRelationships(systemSetup);
		}

		public static void SetupCusIntrastatHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(CusIntrastatHeaderSchema.PK, CusIntrastatMergedLineSchema.CIM_CIH_Header);
			systemSetup.AddRelationship(CusIntrastatHeaderSchema.PK, CusIntrastatLineSchema.CIL_CIH_Header);
			systemSetup.AddRelationship(CusIntrastatMergedLineSchema.PK, CusIntrastatLineSchema.CIL_CIM_MergedLine);
		}

		#endregion
	}
}
