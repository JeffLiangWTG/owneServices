using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	public static class ArchiveRelationships
	{
		public static void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
		{
			SetupRelationsMinusRatingHeader(systemSetup, config);
			SetupRatingHeaderRelationships(systemSetup);
			SetupEDIMessageForAllArchiveableRelationships(systemSetup);
		}

		public static void SetupArchiveRelationshipsForPDORatingStage(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
		{
			SetupRelationsMinusRatingHeader(systemSetup, config);
			systemSetup.AddRelationship(RatingHeaderSchema.PK, JobShipmentSchema.JS_TH_OneTimeQuote);
			SetupEDIMessageForAllArchiveableRelationships(systemSetup);
		}

		public static void SetupRelationsMinusRatingHeader(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
		{
			SetupJobHeaderRelationships(systemSetup);
			SetupJobShipmentRelationships(systemSetup);
			SetupJobConsolRelationships(systemSetup);
			SetupJobCartageRelationships(systemSetup);

			systemSetup.AddRelationship(JobDeclarationSchema.PK, JobHeaderSchema.JH_ParentID);

			if (config.ShouldIncludeDeclarations)
			{
				CustomsArchiveRelationships.SetupCustomsArchiveRelationships(systemSetup);
			}

			SetupJobChargeRelationships(systemSetup);
			SetupJobComInvoiceLineRelationships(systemSetup);
			SetupJobContainerRelationships(systemSetup);
			SetupJobOrderRelationships(systemSetup);
			SetupHVLVArchiveRelationships(systemSetup);
		}

		public static void SetupJobHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobHeaderSchema.PK, JobHeaderSchema.JH_JH_ParentJob);
			systemSetup.AddRelationship(JobHeaderSchema.JH_ParentID, JobShipmentSchema.PK, isReversed: true);
			systemSetup.AddRelationship(JobHeaderSchema.JH_ParentID, JobConsolSchema.PK, isReversed: true);
			systemSetup.AddRelationship(JobHeaderSchema.JH_ParentID, RatingHeaderSchema.PK, isReversed: true);
			systemSetup.AddRelationship(JobHeaderSchema.JH_ParentID, JobDeclarationSchema.PK, isReversed: true);

			systemSetup.AddRelationship(JobHeaderSchema.PK, ConsolidationProfitShareSchema.CPS_JH_ConsolJob);
			systemSetup.AddRelationship(JobHeaderSchema.PK, JobChargeSchema.JR_JH);
			systemSetup.AddRelationship(JobHeaderSchema.PK, JobChargeSchema.JR_JH_InternalJob);
			systemSetup.AddRelationship(JobHeaderSchema.PK, JobChargeRevRecognitionSchema.D3_JH);
			systemSetup.AddRelationship(JobHeaderSchema.PK, JobExRateSchema.JF_JH);
			systemSetup.AddRelationship(JobHeaderSchema.PK, ShipmentProfitSharesSchema.PSS_JH_ShipmentJob);
		}

		public static void SetupJobShipmentRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobHeaderSchema.JH_ParentID);

			systemSetup.AddRelationship(JobShipmentSchema.PK, ELoadListSchema.DO_JS_MasterHouseShipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, HVLVItemSchema.HVI_JS_LoadedOnShipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobCartageSchema.JJ_ParentID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobConShipLinkSchema.JN_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobContainerSchema.JC_JS_FCLBookingOnlyLink);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobContainerPenaltySchema.CPY_JS_Shipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobConsolTransportSchema.JW_ParentGUID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobDeclarationSchema.JE_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobDocAddressSchema.E2_ParentID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobDocsAndCartageSchema.JP_ParentID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobMawbSchema.JM_ParentID);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobOrderHeaderSchema.JD_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobPackLinesSchema.JL_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobPickupDeliveryConfirmSchema.EU_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobShipmentPreplanningSchema.EF_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobShipmentGatewaySchema.JSG_JS_Shipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobShipmentPortMessagingSchema.JSM_JS_Shipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobShipmentSchema.JS_JS_ColoadMasterShipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobShipmentSchema.JS_JS_SplitSwitchShipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, JobSupplierBookingLineGroupSchema.JLG_JS_Shipment);
			systemSetup.AddRelationship(JobShipmentSchema.PK, OrderShipmentPlanningSchema.OPS_JS_Shipment);
			systemSetup.AddRelationship(JobShipmentSchema.JS_TH_OneTimeQuote, RatingHeaderSchema.PK);
			systemSetup.AddRelationship(JobShipmentSchema.PK, ShipmentProfitSharesSchema.PSS_JS);
			systemSetup.AddRelationship(JobShipmentSchema.PK, SupplierBookingLineSchema.DL_JS_ApprovedShipment);

			systemSetup.AddRelationship(ELoadListSchema.PK, SupplierBookingLineSchema.DL_DO_LoadList);
			systemSetup.AddRelationship(JobConShipLinkSchema.JN_JS, JobShipmentSchema.PK);
			systemSetup.AddRelationship(JobContainerLegsSchema.PK, LocalCartageVehicleActivitySchema.EN_JU);
			systemSetup.AddRelationship(JobDocsAndCartageSchema.PK, JobOrderItemSchema.JT_JP);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, ContainerLoadListLineSchema.CLL_JL_PackLine);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, HVLVOuterPackageSchema.HVO_JL_PackLine);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, JobContainerPackPivotSchema.J6_JL);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, JobPackLineHarmonisedCodeSchema.JLH_JL);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, JobPackLinePackageSchema.JPP_JL_PackLine);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, JobPackLinePortMessagingSchema.JLM_JL_PackLine);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, JobPackLocSchema.JQ_JL);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, JobPackProductSchema.D2_JL);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, JobTransportLegPackLineDivotSchema.J8_JL);
			systemSetup.AddRelationship(JobPackLinesSchema.PK, OrderShipmentPlanningLineSchema.OPL_JL_PackLine);
			systemSetup.AddRelationship(HVLVOuterPackageSchema.PK, HVLVItemSchema.HVI_HVO_OuterPackage);
			systemSetup.AddRelationship(JobPickupDeliveryConfirmSchema.PK, JobDocAddressSchema.E2_ParentID);
			systemSetup.AddRelationship(JobPickupDeliveryConfirmSchema.PK, JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm);
		}

		public static void SetupJobConsolRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobConsolSchema.PK, JobHeaderSchema.JH_ParentID);

			systemSetup.AddRelationship(JobConsolSchema.PK, ConsolidationProfitShareSchema.CPS_JK);
			systemSetup.AddRelationship(JobConsolSchema.PK, CusMAWBSchema.CM_JK);
			systemSetup.AddRelationship(JobConsolSchema.PK, HVLVOuterPackageSchema.HVO_JK_LoadedOnConsol);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobCartageSchema.JJ_ParentID);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobConShipLinkSchema.JN_JK);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobConsolAWBSpecialHandlingSchema.JKH_JK_Consol);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobConsolSchema.JK_JK_MasterConsol);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobConsolCostSchema.E6_ParentID);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobConsolDGRestrictionsSchema.JKD_JK);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobConsolTransportSchema.JW_ParentGUID);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobContainerSchema.JC_JK);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobMawbSchema.JM_ParentID);
			systemSetup.AddRelationship(JobConsolSchema.PK, JobSupplierBookingLineGroupSchema.JLG_JK_Consol);

			systemSetup.AddRelationship(ConsolidationProfitShareSchema.PK, ShipmentProfitSharesSchema.PSS_CPS);
			systemSetup.AddRelationship(ConsolidationProfitShareSchema.CPS_PSR, ProfitShareRedistributionSchema.PK, isReversed: true);
			systemSetup.AddRelationship(ProfitShareRedistributionSchema.PK, ConsolidationProfitShareSchema.CPS_PSR);
			systemSetup.AddRelationship(JobConsolCostSchema.PK, JobChargeSchema.JR_E6);
			systemSetup.AddRelationship(JobConsolCostSchema.PK, JobChargeSchema.JR_E6_GatewaySellHeader);
			systemSetup.AddRelationship(JobConsolCostSchema.PK, JobPaymentBasisSchema.PBS_E6);
			systemSetup.AddRelationship(JobConShipLinkSchema.JN_JK, JobConsolSchema.PK, isReversed: true);
			systemSetup.AddRelationship(CusHAWBSchema.PK, CusHAWBItemsSchema.CHI_CS);
			systemSetup.AddRelationship(CusHAWBSchema.PK, CusHAWBSchema.CS_CS_MasterHouseBill);
			systemSetup.AddRelationship(CusHAWBSchema.PK, CusPartShipSchema.CG_CS);
			systemSetup.AddRelationship(CusMAWBSchema.PK, CusHAWBSchema.CS_CM);
			systemSetup.AddRelationship(CusMAWBSchema.PK, CusPartShipSchema.CG_CM_LinkToPartMaster);
		}

		public static void SetupRatingHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(RatingHeaderSchema.PK, JobShipmentSchema.JS_TH_OneTimeQuote);

			systemSetup.AddRelationship(RatingHeaderSchema.PK, RateOneOffShipmentSchema.TT_TH);
			systemSetup.AddRelationship(RateOneOffShipmentSchema.PK, RateOneOffContainersSchema.TC_TT);
			systemSetup.AddRelationship(RateOneOffShipmentSchema.PK, RateOneOffCarrierSchema.TTC_TT);
			systemSetup.AddRelationship(RateOneOffShipmentSchema.PK, RateOneOffPackLineSchema.TPL_TT_RateOneOffShipment);
		}

		public static void SetupJobCartageRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobCartageSchema.PK, JobBookedCtgMoveSchema.EW_JJ);
			systemSetup.AddRelationship(JobCartageSchema.PK, JobDocAddressSchema.E2_ParentID);

			systemSetup.AddRelationship(JobBookedCtgMoveSchema.PK, JobContainerLegsSchema.JU_EW);
			systemSetup.AddRelationship(JobBookedCtgMoveSchema.PK, JobBookedCtgMovPkgDivotSchema.KE_EW_CartageMove);
		}

		public static void SetupJobChargeRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobChargeSchema.PK, JobChargeAttribSchema.EC_JR);
			systemSetup.AddRelationship(JobChargeSchema.PK, JobChargePostingQueueSchema.JPQ_JR);
			systemSetup.AddRelationship(JobChargeSchema.PK, JobChargeTargetSchema.JRT_JR);
			systemSetup.AddRelationship(JobChargeSchema.PK, JobPaymentBasisSchema.PBS_JR);
		}

		public static void SetupJobComInvoiceLineRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, CusContainerInvoiceLinePivotSchema.C2_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, CusHouseContPackInvoiceLinePivotSchema.CHC_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, CusRulingConfigSchema.ZZY_JI_InvoiceLine);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, CusUnderbondDecSchema.BU_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, JobComInvLineComponentInventorySchema.JIV_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, JobComInvLineRefsSchema.JG_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, JobComInvoiceLineTaxSchema.JLT_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, JobTWComInvoiceLineSchema.TWL_JI);
			systemSetup.AddRelationship(JobComInvoiceLineSchema.PK, QuarantineExDocLineSchema.QL_JI);
			systemSetup.AddRelationship(QuarantineExDocLineSchema.PK, QuarantineExDocEstablishmentAndTimeSchema.EE_QL);
		}

		public static void SetupJobContainerRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobContainerSchema.PK, ContainerLoadListLineSchema.CLL_JC_Container);
			systemSetup.AddRelationship(JobContainerSchema.PK, CusContainerSchema.CO_JC);
			systemSetup.AddRelationship(JobContainerSchema.PK, CusSCADepotContainerSchema.CJ_JC);
			systemSetup.AddRelationship(JobContainerSchema.PK, JobBookedCtgMoveSchema.EW_JC_Container);
			systemSetup.AddRelationship(JobContainerSchema.PK, JobContainerPackPivotSchema.J6_JC);
			systemSetup.AddRelationship(JobContainerSchema.PK, JobContainerPenaltySchema.CPY_JC_Container);
			systemSetup.AddRelationship(JobContainerSchema.PK, JobOrderLineDeliverContainerSchema.J5_JC);
			systemSetup.AddRelationship(JobContainerSchema.PK, JobPickupDeliveryConfirmSchema.EU_JC);

			systemSetup.AddRelationship(CusContainerSchema.PK, CusContainerEntryHeaderPivotSchema.CCE_CO_Container);
			systemSetup.AddRelationship(CusContainerSchema.PK, CusContainerEntryInstructionPivotSchema.CEP_CO_Container);
			systemSetup.AddRelationship(CusContainerSchema.PK, CusContainerInvoiceLinePivotSchema.C2_CO);
			systemSetup.AddRelationship(CusContainerSchema.PK, CusDecHouseContainerPivotSchema.CR_CO_Container);
			systemSetup.AddRelationship(CusContainerSchema.PK, JobComInvoiceLineSchema.JI_CO);
			systemSetup.AddRelationship(CusDecHouseContainerPivotSchema.PK, CusDecHouseContainerPackSchema.CW_CR_HouseContainer);
			systemSetup.AddRelationship(CusSCADepotContainerSchema.PK, CusSCADepotHouseSchema.CX_CJ);
		}

		public static void SetupJobOrderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobOrderHeaderSchema.PK, JobDocAddressSchema.E2_ParentID);
			systemSetup.AddRelationship(JobOrderHeaderSchema.PK, JobOrderContainerSchema.J1_ParentID);
			systemSetup.AddRelationship(JobOrderHeaderSchema.PK, JobOrderLineSchema.JO_JD);
			systemSetup.AddRelationship(JobOrderLineDeliverContainerSchema.J5_J4, JobOrderLineDeliverySchema.PK);
			systemSetup.AddRelationship(JobOrderLineDeliverySchema.PK, JobOrderLineDeliverContainerSchema.J5_J4);
			systemSetup.AddRelationship(JobOrderLineSchema.PK, JobOrderLineDeliverySchema.J4_JO);
			systemSetup.AddRelationship(JobOrderLineSchema.PK, JobPackProductSchema.D2_JO);
			systemSetup.AddRelationship(JobOrderLineSchema.PK, JobSupplierBookingLineSchema.JSL_JO_OrderLine);
			systemSetup.AddRelationship(JobSupplierBookingLineSchema.PK, ContainerLoadListLineSchema.CLL_JSL_BookingLine);
			systemSetup.AddRelationship(JobSupplierBookingLineSchema.PK, OrderShipmentPlanningLineSchema.OPL_JSL_BookingLine);
			systemSetup.AddRelationship(JobSupplierBookingLineSchema.PK, JobPackLinesSchema.JL_JSL_BookingLine);
			systemSetup.AddRelationship(JobSupplierBookingLineGroupSchema.PK, JobSupplierBookingLineSchema.JSL_JLG_Group);
			systemSetup.AddRelationship(JobSupplierBookingLineGroupSchema.PK, JobSupplierBookingLineGroupSchema.JLG_JLG_Parent);
			systemSetup.AddRelationship(OrderShipmentPlanningSchema.PK, OrderShipmentPlanningLineSchema.OPL_OPS_Planning);
		}

		public static void SetupEDIMessageForAllArchiveableRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationshipToAllPKs(EDIMessageSchema.EM_LinkUniqueID);
			systemSetup.AddRelationship(EDIMessageSchema.PK, EDIMessageAttachSchema.EG_EM);
			systemSetup.AddRelationship(EDIMessageSchema.PK, EDIMessageQueueStateSchema.EQS_EM);
			systemSetup.AddRelationship(EDIMessageSchema.PK, EDIMessageSchema.EM_EM_RequestMessage);
		}

		public static void SetupHVLVArchiveRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(JobShipmentSchema.PK, HVLVConsignmentHeaderSchema.HCH_JS_Shipment);
			systemSetup.AddRelationship(HVLVConsignmentHeaderSchema.PK, HVLVConsignmentSchema.HVC_HCH_Header);
			systemSetup.AddRelationship(HVLVItemSchema.PK, HVLVItemLineSchema.HVS_HVI_HVLVItem);
			SetUpHVLVConsignmentRelationships(systemSetup);
		}

		public static void SetUpHVLVConsignmentRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(HVLVConsignmentSchema.PK, HVLVItemSchema.HVI_HVC_Consignment);
			systemSetup.AddRelationship(HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.HVP_HVC_Former);
			systemSetup.AddRelationship(HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.HVP_HVC_Return);
			systemSetup.AddRelationship(HVLVConsignmentSchema.PK, CusEntryNumSchema.CE_ParentID);
		}

		public static void SetUpHVLVOriginLoadListRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(HVLVOriginLoadListSchema.PK, HVLVOuterPackageSchema.HVO_HVL_LoadList);
		}

		public static void SetUpHVLVBookingHeaderRelationships(IArchiveSystemSetup systemSetup)
		{
			systemSetup.AddRelationship(HVLVBookingHeaderSchema.PK, HVLVConsignmentSchema.HVC_HVH_BookingHeader);
			SetUpHVLVConsignmentRelationships(systemSetup);
		}
	}
}
