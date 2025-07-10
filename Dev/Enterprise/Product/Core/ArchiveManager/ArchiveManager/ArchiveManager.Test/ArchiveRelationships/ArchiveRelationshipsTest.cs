using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test
{
	class ArchiveRelationshipsTest : TestCaseWithFactory
	{
		public void TestSetupJobHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobHeaderRelationships(stage);

			AssertEquals(11, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK, JobHeaderSchema.Constants.TableName, JobHeaderSchema.JH_JH_ParentJob, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.JH_ParentID, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, isReversed: true));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.JH_ParentID, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, isReversed: true));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.JH_ParentID, RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK, isReversed: true));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.JH_ParentID, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK, isReversed: true));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK, ConsolidationProfitShareSchema.Constants.TableName, ConsolidationProfitShareSchema.CPS_JH_ConsolJob, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK, JobChargeSchema.Constants.TableName, JobChargeSchema.JR_JH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK, JobChargeSchema.Constants.TableName, JobChargeSchema.JR_JH_InternalJob, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK, JobChargeRevRecognitionSchema.Constants.TableName, JobChargeRevRecognitionSchema.D3_JH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK, JobExRateSchema.Constants.TableName, JobExRateSchema.JF_JH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK, ShipmentProfitSharesSchema.Constants.TableName, ShipmentProfitSharesSchema.PSS_JH_ShipmentJob, isReversed: false));
		}

		public void TestSetupJobShipmentRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobShipmentRelationships(stage);

			AssertEquals(42, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobHeaderSchema.Constants.TableName, JobHeaderSchema.JH_ParentID, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, ELoadListSchema.Constants.TableName, ELoadListSchema.DO_JS_MasterHouseShipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_JS_LoadedOnShipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobCartageSchema.Constants.TableName, JobCartageSchema.JJ_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobConShipLinkSchema.Constants.TableName, JobConShipLinkSchema.JN_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobContainerSchema.Constants.TableName, JobContainerSchema.JC_JS_FCLBookingOnlyLink, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobConsolTransportSchema.Constants.TableName, JobConsolTransportSchema.JW_ParentGUID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.JE_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.E2_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobDocsAndCartageSchema.Constants.TableName, JobDocsAndCartageSchema.JP_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobMawbSchema.Constants.TableName, JobMawbSchema.JM_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.JD_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.JL_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobPickupDeliveryConfirmSchema.Constants.TableName, JobPickupDeliveryConfirmSchema.EU_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobShipmentPreplanningSchema.Constants.TableName, JobShipmentPreplanningSchema.EF_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobShipmentGatewaySchema.Constants.TableName, JobShipmentGatewaySchema.JSG_JS_Shipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobShipmentPortMessagingSchema.Constants.TableName, JobShipmentPortMessagingSchema.JSM_JS_Shipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobShipmentSchema.Constants.TableName, JobShipmentSchema.JS_JS_ColoadMasterShipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobShipmentSchema.Constants.TableName, JobShipmentSchema.JS_JS_SplitSwitchShipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, JobSupplierBookingLineGroupSchema.Constants.TableName, JobSupplierBookingLineGroupSchema.JLG_JS_Shipment, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.JS_TH_OneTimeQuote, RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, ShipmentProfitSharesSchema.Constants.TableName, ShipmentProfitSharesSchema.PSS_JS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, SupplierBookingLineSchema.Constants.TableName, SupplierBookingLineSchema.DL_JS_ApprovedShipment, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, ELoadListSchema.Constants.TableName, ELoadListSchema.PK, SupplierBookingLineSchema.Constants.TableName, SupplierBookingLineSchema.DL_DO_LoadList, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConShipLinkSchema.Constants.TableName, JobConShipLinkSchema.JN_JS, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerLegsSchema.Constants.TableName, JobContainerLegsSchema.PK, LocalCartageVehicleActivitySchema.Constants.TableName, LocalCartageVehicleActivitySchema.EN_JU, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobDocsAndCartageSchema.Constants.TableName, JobDocsAndCartageSchema.PK, JobOrderItemSchema.Constants.TableName, JobOrderItemSchema.JT_JP, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, ContainerLoadListLineSchema.Constants.TableName, ContainerLoadListLineSchema.CLL_JL_PackLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.HVO_JL_PackLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, JobContainerPackPivotSchema.Constants.TableName, JobContainerPackPivotSchema.J6_JL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, JobPackLineHarmonisedCodeSchema.Constants.TableName, JobPackLineHarmonisedCodeSchema.JLH_JL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, JobPackLinePackageSchema.Constants.TableName, JobPackLinePackageSchema.JPP_JL_PackLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, JobPackLinePortMessagingSchema.Constants.TableName, JobPackLinePortMessagingSchema.JLM_JL_PackLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, JobPackLocSchema.Constants.TableName, JobPackLocSchema.JQ_JL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, JobPackProductSchema.Constants.TableName, JobPackProductSchema.D2_JL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK, JobTransportLegPackLineDivotSchema.Constants.TableName, JobTransportLegPackLineDivotSchema.J8_JL, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.PK, HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_HVO_OuterPackage, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPickupDeliveryConfirmSchema.Constants.TableName, JobPickupDeliveryConfirmSchema.PK, JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.E2_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobPickupDeliveryConfirmSchema.Constants.TableName, JobPickupDeliveryConfirmSchema.PK, JobTransportLegPackLineDivotSchema.Constants.TableName, JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm, isReversed: false));
		}

		public void TestSetupJobConsolRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobConsolRelationships(stage);

			AssertEquals(26, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobHeaderSchema.Constants.TableName, JobHeaderSchema.JH_ParentID, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, ConsolidationProfitShareSchema.Constants.TableName, ConsolidationProfitShareSchema.CPS_JK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, CusMAWBSchema.Constants.TableName, CusMAWBSchema.CM_JK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.HVO_JK_LoadedOnConsol, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobCartageSchema.Constants.TableName, JobCartageSchema.JJ_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobConShipLinkSchema.Constants.TableName, JobConShipLinkSchema.JN_JK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobConsolSchema.Constants.TableName, JobConsolSchema.JK_JK_MasterConsol, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobConsolAWBSpecialHandlingSchema.Constants.TableName, JobConsolAWBSpecialHandlingSchema.JKH_JK_Consol, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobConsolCostSchema.Constants.TableName, JobConsolCostSchema.E6_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobConsolDGRestrictionsSchema.Constants.TableName, JobConsolDGRestrictionsSchema.JKD_JK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobConsolTransportSchema.Constants.TableName, JobConsolTransportSchema.JW_ParentGUID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobContainerSchema.Constants.TableName, JobContainerSchema.JC_JK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobMawbSchema.Constants.TableName, JobMawbSchema.JM_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, JobSupplierBookingLineGroupSchema.Constants.TableName, JobSupplierBookingLineGroupSchema.JLG_JK_Consol, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, ConsolidationProfitShareSchema.Constants.TableName, ConsolidationProfitShareSchema.PK, ShipmentProfitSharesSchema.Constants.TableName, ShipmentProfitSharesSchema.PSS_CPS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, ConsolidationProfitShareSchema.Constants.TableName, ConsolidationProfitShareSchema.CPS_PSR, ProfitShareRedistributionSchema.Constants.TableName, ProfitShareRedistributionSchema.PK, isReversed: true));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, ProfitShareRedistributionSchema.Constants.TableName, ProfitShareRedistributionSchema.PK, ConsolidationProfitShareSchema.Constants.TableName, ConsolidationProfitShareSchema.CPS_PSR, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolCostSchema.Constants.TableName, JobConsolCostSchema.PK, JobChargeSchema.Constants.TableName, JobChargeSchema.JR_E6, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolCostSchema.Constants.TableName, JobConsolCostSchema.PK, JobChargeSchema.Constants.TableName, JobChargeSchema.JR_E6_GatewaySellHeader, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConsolCostSchema.Constants.TableName, JobConsolCostSchema.PK, JobPaymentBasisSchema.Constants.TableName, JobPaymentBasisSchema.PBS_E6, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobConShipLinkSchema.Constants.TableName, JobConShipLinkSchema.JN_JK, JobConsolSchema.Constants.TableName, JobConsolSchema.PK, isReversed: true));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusHAWBSchema.Constants.TableName, CusHAWBSchema.PK, CusHAWBItemsSchema.Constants.TableName, CusHAWBItemsSchema.CHI_CS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusHAWBSchema.Constants.TableName, CusHAWBSchema.PK, CusHAWBSchema.Constants.TableName, CusHAWBSchema.CS_CS_MasterHouseBill, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusHAWBSchema.Constants.TableName, CusHAWBSchema.PK, CusPartShipSchema.Constants.TableName, CusPartShipSchema.CG_CS, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusMAWBSchema.Constants.TableName, CusMAWBSchema.PK, CusHAWBSchema.Constants.TableName, CusHAWBSchema.CS_CM, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusMAWBSchema.Constants.TableName, CusMAWBSchema.PK, CusPartShipSchema.Constants.TableName, CusPartShipSchema.CG_CM_LinkToPartMaster, isReversed: false));
		}

		public void TestSetupRatingHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupRatingHeaderRelationships(stage);

			AssertEquals(5, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK, JobShipmentSchema.Constants.TableName, JobShipmentSchema.JS_TH_OneTimeQuote, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK, RateOneOffShipmentSchema.Constants.TableName, RateOneOffShipmentSchema.TT_TH, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, RateOneOffShipmentSchema.Constants.TableName, RateOneOffShipmentSchema.PK, RateOneOffContainersSchema.Constants.TableName, RateOneOffContainersSchema.TC_TT, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, RateOneOffShipmentSchema.Constants.TableName, RateOneOffShipmentSchema.PK, RateOneOffCarrierSchema.Constants.TableName, RateOneOffCarrierSchema.TTC_TT, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, RateOneOffShipmentSchema.Constants.TableName, RateOneOffShipmentSchema.PK, RateOneOffPackLineSchema.Constants.TableName, RateOneOffPackLineSchema.TPL_TT_RateOneOffShipment, isReversed: false));
		}

		public void TestSetupJobCartageRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobCartageRelationships(stage);

			AssertEquals(4, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobCartageSchema.Constants.TableName, JobCartageSchema.PK, JobBookedCtgMoveSchema.Constants.TableName, JobBookedCtgMoveSchema.EW_JJ, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobCartageSchema.Constants.TableName, JobCartageSchema.PK, JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.E2_ParentID, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobBookedCtgMoveSchema.Constants.TableName, JobBookedCtgMoveSchema.PK, JobContainerLegsSchema.Constants.TableName, JobContainerLegsSchema.JU_EW, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobBookedCtgMoveSchema.Constants.TableName, JobBookedCtgMoveSchema.PK, JobBookedCtgMovPkgDivotSchema.Constants.TableName, JobBookedCtgMovPkgDivotSchema.KE_EW_CartageMove, isReversed: false));
		}

		public void TestSetupJobChargeRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobChargeRelationships(stage);

			AssertEquals(4, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobChargeSchema.Constants.TableName, JobChargeSchema.PK, JobChargeAttribSchema.Constants.TableName, JobChargeAttribSchema.EC_JR, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobChargeSchema.Constants.TableName, JobChargeSchema.PK, JobChargePostingQueueSchema.Constants.TableName, JobChargePostingQueueSchema.JPQ_JR, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobChargeSchema.Constants.TableName, JobChargeSchema.PK, JobChargeTargetSchema.Constants.TableName, JobChargeTargetSchema.JRT_JR, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobChargeSchema.Constants.TableName, JobChargeSchema.PK, JobPaymentBasisSchema.Constants.TableName, JobPaymentBasisSchema.PBS_JR, isReversed: false));
		}

		public void TestSetupJobComInvoiceLineRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobComInvoiceLineRelationships(stage);

			AssertEquals(10, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, CusContainerInvoiceLinePivotSchema.Constants.TableName, CusContainerInvoiceLinePivotSchema.C2_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, CusHouseContPackInvoiceLinePivotSchema.CHC_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, CusRulingConfigSchema.Constants.TableName, CusRulingConfigSchema.ZZY_JI_InvoiceLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, CusUnderbondDecSchema.Constants.TableName, CusUnderbondDecSchema.BU_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, JobComInvLineComponentInventorySchema.Constants.TableName, JobComInvLineComponentInventorySchema.JIV_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, JobComInvLineRefsSchema.Constants.TableName, JobComInvLineRefsSchema.JG_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, JobComInvoiceLineTaxSchema.Constants.TableName, JobComInvoiceLineTaxSchema.JLT_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, JobTWComInvoiceLineSchema.Constants.TableName, JobTWComInvoiceLineSchema.TWL_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK, QuarantineExDocLineSchema.Constants.TableName, QuarantineExDocLineSchema.QL_JI, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, QuarantineExDocLineSchema.Constants.TableName, QuarantineExDocLineSchema.PK, QuarantineExDocEstablishmentAndTimeSchema.Constants.TableName, QuarantineExDocEstablishmentAndTimeSchema.EE_QL, isReversed: false));
		}

		public void TestSetupJobContainerRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobContainerRelationships(stage);

			AssertEquals(15, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, ContainerLoadListLineSchema.Constants.TableName, ContainerLoadListLineSchema.CLL_JC_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, CusContainerSchema.Constants.TableName, CusContainerSchema.CO_JC, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, CusSCADepotContainerSchema.Constants.TableName, CusSCADepotContainerSchema.CJ_JC, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, JobBookedCtgMoveSchema.Constants.TableName, JobBookedCtgMoveSchema.EW_JC_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, JobContainerPackPivotSchema.Constants.TableName, JobContainerPackPivotSchema.J6_JC, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, JobContainerPenaltySchema.Constants.TableName, JobContainerPenaltySchema.CPY_JC_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, JobOrderLineDeliverContainerSchema.Constants.TableName, JobOrderLineDeliverContainerSchema.J5_JC, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobContainerSchema.Constants.TableName, JobContainerSchema.PK, JobPickupDeliveryConfirmSchema.Constants.TableName, JobPickupDeliveryConfirmSchema.EU_JC, isReversed: false));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusContainerSchema.Constants.TableName, CusContainerSchema.PK, CusContainerEntryHeaderPivotSchema.Constants.TableName, CusContainerEntryHeaderPivotSchema.CCE_CO_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusContainerSchema.Constants.TableName, CusContainerSchema.PK, CusContainerEntryInstructionPivotSchema.Constants.TableName, CusContainerEntryInstructionPivotSchema.CEP_CO_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusContainerSchema.Constants.TableName, CusContainerSchema.PK, CusContainerInvoiceLinePivotSchema.Constants.TableName, CusContainerInvoiceLinePivotSchema.C2_CO, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusContainerSchema.Constants.TableName, CusContainerSchema.PK, CusDecHouseContainerPivotSchema.Constants.TableName, CusDecHouseContainerPivotSchema.CR_CO_Container, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusContainerSchema.Constants.TableName, CusContainerSchema.PK, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.JI_CO, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusDecHouseContainerPivotSchema.Constants.TableName, CusDecHouseContainerPivotSchema.PK, CusDecHouseContainerPackSchema.Constants.TableName, CusDecHouseContainerPackSchema.CW_CR_HouseContainer, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, CusSCADepotContainerSchema.Constants.TableName, CusSCADepotContainerSchema.PK, CusSCADepotHouseSchema.Constants.TableName, CusSCADepotHouseSchema.CX_CJ, isReversed: false));
		}

		public void TestSetupJobOrderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupJobOrderRelationships(stage);

			AssertEquals(14, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.PK, JobDocAddressSchema.Constants.TableName, JobDocAddressSchema.E2_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.PK, JobOrderContainerSchema.Constants.TableName, JobOrderContainerSchema.J1_ParentID, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderHeaderSchema.Constants.TableName, JobOrderHeaderSchema.PK, JobOrderLineSchema.Constants.TableName, JobOrderLineSchema.JO_JD, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderLineDeliverContainerSchema.Constants.TableName, JobOrderLineDeliverContainerSchema.J5_J4, JobOrderLineDeliverySchema.Constants.TableName, JobOrderLineDeliverySchema.PK, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderLineDeliverySchema.Constants.TableName, JobOrderLineDeliverySchema.PK, JobOrderLineDeliverContainerSchema.Constants.TableName, JobOrderLineDeliverContainerSchema.J5_J4, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderLineSchema.Constants.TableName, JobOrderLineSchema.PK, JobOrderLineDeliverySchema.Constants.TableName, JobOrderLineDeliverySchema.J4_JO, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderLineSchema.Constants.TableName, JobOrderLineSchema.PK, JobPackProductSchema.Constants.TableName, JobPackProductSchema.D2_JO, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobOrderLineSchema.Constants.TableName, JobOrderLineSchema.PK, JobSupplierBookingLineSchema.Constants.TableName, JobSupplierBookingLineSchema.JSL_JO_OrderLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobSupplierBookingLineSchema.Constants.TableName, JobSupplierBookingLineSchema.PK, ContainerLoadListLineSchema.Constants.TableName, ContainerLoadListLineSchema.CLL_JSL_BookingLine, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobSupplierBookingLineGroupSchema.Constants.TableName, JobSupplierBookingLineGroupSchema.PK, JobSupplierBookingLineSchema.Constants.TableName, JobSupplierBookingLineSchema.JSL_JLG_Group, isReversed: false));
			Assert(TestHelpers.ArchiveStageHasRelationship(stage, JobSupplierBookingLineGroupSchema.Constants.TableName, JobSupplierBookingLineGroupSchema.PK, JobSupplierBookingLineGroupSchema.Constants.TableName, JobSupplierBookingLineGroupSchema.JLG_JLG_Parent, isReversed: false));
		}

		public void TestSetupHVLVArchiveRelationships()
		{
			var archiveStage = new ArchiveStage(new DummyArchiveStage(), null);

			ArchiveRelationships.SetupHVLVArchiveRelationships(archiveStage);

			AssertEquals(7, archiveStage.ArchiveableRelationships.Sum(a => a.Value.Count));

			CombineAssertions(() =>
			{
				Assert(TestHelpers.ArchiveStageHasRelationship(archiveStage, JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK, HVLVConsignmentHeaderSchema.Constants.TableName, HVLVConsignmentHeaderSchema.HCH_JS_Shipment, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(archiveStage, HVLVConsignmentHeaderSchema.Constants.TableName, HVLVConsignmentHeaderSchema.PK, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.HVC_HCH_Header, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(archiveStage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_HVC_Consignment, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(archiveStage, HVLVItemSchema.Constants.TableName, HVLVItemSchema.PK, HVLVItemLineSchema.Constants.TableName, HVLVItemLineSchema.HVS_HVI_HVLVItem, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(archiveStage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Former, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(archiveStage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Return, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(archiveStage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, CusEntryNumSchema.Constants.TableName, CusEntryNumSchema.CE_ParentID, isReversed: false));
			});
		}

		public void TestSetupEDIMessageForAllArchiveableRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetupEDIMessageForAllArchiveableRelationships(stage);

			AssertEquals(4, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			CombineAssertions(() =>
			{
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, DummyBizoSchema.Constants.TableName, DummyBizoSchema.PK, EDIMessageSchema.Constants.TableName, EDIMessageSchema.EM_LinkUniqueID, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, EDIMessageSchema.Constants.TableName, EDIMessageSchema.PK, EDIMessageAttachSchema.Constants.TableName, EDIMessageAttachSchema.EG_EM, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, EDIMessageSchema.Constants.TableName, EDIMessageSchema.PK, EDIMessageQueueStateSchema.Constants.TableName, EDIMessageQueueStateSchema.EQS_EM, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, EDIMessageSchema.Constants.TableName, EDIMessageSchema.PK, EDIMessageSchema.Constants.TableName, EDIMessageSchema.EM_EM_RequestMessage, isReversed: false));
			});
		}

		public void TestSetUpHVLVOriginLoadListRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetUpHVLVOriginLoadListRelationships(stage);

			AssertEquals(1, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVOriginLoadListSchema.Constants.TableName, HVLVOriginLoadListSchema.PK, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.HVO_HVL_LoadList, isReversed: false));
		}

		public void TestSetUpHVLVBookingHeaderRelationships()
		{
			var stage = new ArchiveStage(new DummyArchiveStage(), null);
			ArchiveRelationships.SetUpHVLVBookingHeaderRelationships(stage);

			AssertEquals(5, stage.ArchiveableRelationships.Sum(a => a.Value.Count));

			CombineAssertions(() =>
			{
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVBookingHeaderSchema.Constants.TableName, HVLVBookingHeaderSchema.PK, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.HVC_HVH_BookingHeader, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_HVC_Consignment, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Former, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Return, isReversed: false));
				Assert(TestHelpers.ArchiveStageHasRelationship(stage, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.PK, CusEntryNumSchema.Constants.TableName, CusEntryNumSchema.CE_ParentID, isReversed: false));
			});
		}

		#region SetupArchiveRelationships

		public void TestSetupArchiveRelationships_RelationsNotYetIncludedInArchiveManager()
		{
			var differenceMessage = $@"Relations have been added or updated and are not yet included in Archive Manager. Please consider if these relations should be added to ArchiveRelationships for archiving.
If they should not be archived, then add these relations to the IgnoreList in Enterprise.ArchiveManager.Test.ArchiveRelationshipsTest.cs. Assign a task to AMA capability for review.
See more information here: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/14061/Unit-Test-Reference?anchor=testsetuparchiverelationships_relationsexistinginarchivemanagerbutnotodyssey {System.Environment.NewLine}";
			var archiveStage = new ArchiveStage(new DummyArchiveStage(), null);

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			ArchiveRelationships.SetupArchiveRelationships(archiveStage, config);

			var stageDescriptors = new STAArchiveSystemDescriptor().GetArchiveStageDescriptors(config);

			var relationshipsBeingArchived = GetArchiveableRelationshipsList(archiveStage);
			foreach (var stageDescriptor in stageDescriptors)
			{
				var staArchiveStage = new STAArchiveStage(stageDescriptor, null);
				stageDescriptor.SetupArchiveRelationships(staArchiveStage, config);
				relationshipsBeingArchived.AddRange(GetArchiveableRelationshipsList(staArchiveStage));
			}
			stageDescriptors = new HARArchiveSystemDescriptor().GetArchiveStageDescriptors(config);

			foreach (var stageDescriptor in stageDescriptors)
			{
				var harArchiveStage = new HARArchiveStage(stageDescriptor, null);
				stageDescriptor.SetupArchiveRelationships(harArchiveStage, config);
				relationshipsBeingArchived.AddRange(GetArchiveableRelationshipsList(harArchiveStage));
			}
			var validRelationships = GetValidRelationshipsList();
			var difference = validRelationships.Except(relationshipsBeingArchived).ToList();

			Assert(differenceMessage + string.Join("\n", difference.Select(r => r.ToString())), !(difference.Count > 0));
		}

		public void TestSetupArchiveRelationships_RelationsExistingInArchiveManagerButNotOdyssey()
		{
			var subsetMessage = $@"Relations have been removed or updated and exist in Archive Manager but not Odyssey Schema. Please correct the relations, or add them to the IgnoreList in Enterprise.ArchiveManager.Test.ArchiveRelationshipsTest.cs.
Assign a task to AMA capability for review.
See more information here: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/14061/Unit-Test-Reference?anchor=testsetuparchiverelationships_relationsexistinginarchivemanagerbutnotodyssey {System.Environment.NewLine}";
			var archiveStage = new ArchiveStage(new DummyArchiveStage(), null);
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);

			ArchiveRelationships.SetupArchiveRelationships(archiveStage, config);

			var archiveableRelationships = GetArchiveableRelationshipsList(archiveStage);
			var validRelationships = GetValidRelationshipsList();
			var invalidArchiveRelationships = archiveableRelationships
				.Except(validRelationships)
				.Where(relationship => !ArchiveRelationshipUsesSoftReferencing(relationship) && !ArchiveRelationshipIsParentContainingFK(relationship));

			AssertEquals(subsetMessage + string.Join("\n", invalidArchiveRelationships.Select(r => r.ToString())), 0, invalidArchiveRelationships.Count());
		}

		bool ArchiveRelationshipUsesSoftReferencing(TestArchiveableRelationship relationship)
		{
			var resolver = new EnterpriseSchemaResolver();
			var parentKeyColumnReferencedByChild = resolver.GetSchemaColumn(relationship.ParentKeyColumnReferencedByChild, relationship.ParentTableName);
			var childFKColumn = resolver.GetSchemaColumn(relationship.ChildFKColumnName, relationship.ChildFKTableName);

			if (parentKeyColumnReferencedByChild.IsPKColumn)
			{
				return ColumnIsSoftReference(childFKColumn, parentKeyColumnReferencedByChild);
			}

			if (relationship.IsReversed && childFKColumn.IsPKColumn)
			{
				return ColumnIsSoftReference(parentKeyColumnReferencedByChild, childFKColumn);
			}

			return false;
		}

		bool ColumnIsSoftReference(SchemaColumn column, SchemaColumn otherColumn)
		{
			var columnNameLower = column.Name.ToLower();

			return columnNameLower.Contains("parent") && columnNameLower.Contains("id")
				|| columnNameLower.Contains("linkuniqueid")
				|| column.Name.Contains(otherColumn.ColumnPrefix);
		}

		bool ArchiveRelationshipIsParentContainingFK(TestArchiveableRelationship relationship)
		{
			var resolver = new EnterpriseSchemaResolver();
			var childFKColumn = resolver.GetSchemaColumn(relationship.ChildFKColumnName, relationship.ChildFKTableName);

			return !relationship.IsReversed && relationship.ParentKeyColumnReferencedByChild.Contains(childFKColumn.ColumnPrefix) && childFKColumn.IsPKColumn;
		}

		List<TestArchiveableRelationship> GetArchiveableRelationshipsList(ArchiveStage archiveStage)
		{
			var archiveableRelationships = new List<TestArchiveableRelationship>();

			foreach (var pkTableName in archiveStage.ArchiveableRelationships.Keys)
			{
				foreach (var relationship in archiveStage.ArchiveableRelationships[pkTableName])
				{
					var testArchiveableRelationship = new TestArchiveableRelationship(pkTableName, relationship.ParentKeyColumnReferencedByChild.Name, relationship.ChildName, relationship.ChildFKColumn.Name, relationship.IsReversed);
					archiveableRelationships.Add(testArchiveableRelationship);
				}
			}

			return archiveableRelationships;
		}

		List<TestArchiveableRelationship> GetValidRelationshipsList()
		{
			var validRelationships = new List<TestArchiveableRelationship>();
			Db.Connection.ExecuteReader(ArchiveableObjects + ValidArchiveableObjects, reader => ReadValidArchiveableRelationships(validRelationships, reader));

			_ = validRelationships.RemoveAll(r => IgnoreList.Contains(r));

			return validRelationships;
		}

		void ReadValidArchiveableRelationships(List<TestArchiveableRelationship> validRelationships, IDataRecord reader)
		{
			var relationship = new TestArchiveableRelationship(reader["PK_Table_Name"].ToString(), reader["PK_Column_Name"].ToString(), reader["FK_Table_Name"].ToString(), reader["FK_Column_Name"].ToString(), false);
			validRelationships.Add(relationship);
		}

		const string ArchiveableObjects = @"
SELECT id,
       NAME
INTO   #archiveableobjects
FROM   sysobjects
WHERE  ( NAME NOT LIKE 'Acc%'
         AND NAME NOT LIKE 'Alert%'
		 AND NAME NOT LIKE 'ArchiveMainItemQueue'
		 AND NAME NOT LIKE 'ArchiveRelatedItemQueue'
         AND NAME NOT LIKE 'Barcode%'
         AND NAME NOT LIKE 'BM%'
         AND NAME NOT LIKE 'BPM%'
		 AND NAME NOT LIKE 'Carrier%'
		 AND NAME <> 'CertificateOfOrigin'
		 AND NAME <> 'ComplianceRiskStatus'
		 AND NAME <> 'ContainerLoadListHeader'
         AND NAME NOT LIKE 'Crm%'
         AND NAME NOT LIKE 'CusClass%'
		 AND NAME <> 'CusGoodsCatalog'
		 AND NAME NOT LIKE 'CusIntrastat%'
         AND NAME NOT LIKE 'CusISF%'
         AND NAME NOT LIKE 'CusMisc%'
         AND NAME NOT LIKE 'CusOutturn%'
		 AND NAME NOT LIKE 'CusPermit%' --Multiple jobs of different types can consume a permit (create transactions from it). However, the permit doesn't link to the children. So transactions captured against the permit will remain valid even if the associated job is archived.
         AND NAME <> 'CusPerson'
         AND NAME NOT LIKE 'CusRef%'
         AND NAME NOT LIKE 'CusSea%'
		 AND NAME NOT LIKE 'CusStatement%'
         AND NAME <> 'CusSupportingInfo'
         AND NAME NOT LIKE 'CusUSLV%'
		 AND NAME NOT LIKE 'CusWHS%'
         AND NAME NOT LIKE 'CY%'
		 AND NAME NOT LIKE 'Dash%'
         AND NAME NOT LIKE 'Dmg%'
         AND NAME <> 'DsbJobCloseBatch'
         AND NAME NOT LIKE 'Dtb%'
         AND NAME <> 'DummyBizo'
		 AND NAME NOT LIKE 'EDICommunication%'
	     AND NAME <> 'EDIInterchange'
		 AND NAME <> 'EDIMessageContentFilter'
		 AND NAME NOT LIKE 'EDIMessageDelivery%'
		 AND NAME <> 'ELoadList'
         AND NAME <> 'ExamSetting'
         AND NAME <> 'ExportAWBHeader'
         AND NAME <> 'ExportCustomsManifestHeader'
         AND NAME NOT LIKE 'Fax%'
         AND NAME NOT LIKE 'Gate%'
         AND NAME <> 'GenCustomAddOnRule'
         AND NAME NOT LIKE 'Glb%'
		 AND NAME <> 'GlobalCommercialInvoiceHeader'
		 AND NAME NOT LIKE 'Gte%'
		 AND NAME NOT LIKE 'HR%'
		 AND NAME <> 'JobCartageRunSheet'
		 AND NAME <> 'JobConsolidatedTransportBooking'
         AND NAME <> 'JobContainerDetention'
         AND NAME NOT LIKE 'JobConversation%'
         AND NAME <> 'JobDocAddress'
         AND NAME <> 'JobDocumentDelivery'
         AND NAME <> 'JobRequiredDocument'
         AND NAME <> 'JobSailing'
		 AND NAME <> 'JobService'
         AND NAME <> 'JobSlotAllocation'
		 AND NAME <> 'JobSupplierBooking'
		 AND NAME <> 'JobToCloseQueue'
         AND NAME <> 'JobTradeLane'
         AND NAME <> 'JobVesselSchedule'
         AND NAME NOT LIKE 'JobVoy%'
		 AND NAME NOT LIKE 'JPAFR%'
         AND NAME NOT LIKE 'LandedCost%'
         AND NAME NOT LIKE 'LocalCartage%'
         AND NAME <> 'MailDBItems'
         AND NAME NOT LIKE 'MENT%'
         AND NAME NOT LIKE 'MNR%'
         AND NAME NOT LIKE 'Netting%'
         AND NAME NOT LIKE 'Org%'
		 AND NAME NOT LIKE 'PkgPackage%'
		 AND NAME <> 'PortHubSelection'
         AND NAME <> 'ProcessFieldChangeRule'
		 AND NAME NOT LIKE 'ProcessHeader%'
		 AND NAME <> 'ProcessTasks'
		 AND NAME <> 'ProcessTaskIterationLink'
         AND NAME <> 'ProcessTaskTemplate'
         AND NAME NOT LIKE 'ProcessTemplate%'
		 AND NAME NOT LIKE 'ProcessWorkflow%'
		 AND NAME NOT LIKE 'ProductionRule%'
		 AND NAME NOT LIKE 'ProfitShareRedistribution'
         AND NAME NOT LIKE 'P4%'
		 AND NAME <> 'QuarantineColsHeader'
         AND NAME <> 'RateAttachmentSet'
         AND NAME NOT LIKE 'RateTransport%'
		 AND NAME NOT LIKE 'RatingContract%'
		 AND NAME NOT LIKE 'Ref%'
		 AND NAME NOT LIKE 'Review%'
		 AND NAME NOT LIKE 'RouteSegment'
         AND NAME NOT LIKE 'Stm%'
		 AND NAME NOT LIKE 'Storage%'
		 AND NAME <> 'SupplierBookingHeader'
         AND NAME NOT LIKE 'Tag%'
         AND NAME NOT LIKE 'Tal%'
         AND NAME NOT LIKE 'Tel%'
         AND NAME NOT LIKE 'UNDG%'
		 AND NAME <> 'UniversalValidationRuleSet'
         AND NAME NOT LIKE 'Vote%'
         AND NAME NOT LIKE 'VoyageTemplate%'
         AND NAME NOT LIKE 'Whs%'
         AND NAME <> 'WorkItem'
         AND NAME <> 'WorkRequest'
         AND NAME <> 'YardUnit'
         AND NAME NOT LIKE 'ZZ%'
         AND NAME NOT LIKE 'AllocationRouteWizard%'
)";
		const string ValidArchiveableObjects = @"
SELECT a1.NAME AS PK_Table_Name,
       c2.NAME AS PK_Column_Name,
       o1.NAME AS FK_Table_Name,
       c1.NAME AS FK_Column_Name,
       s.NAME  AS Constraint_name
FROM   sysforeignkeys fk
       INNER JOIN sysobjects o1
               ON fk.fkeyid = o1.id
       INNER JOIN #archiveableobjects a1
               ON fk.rkeyid = a1.id
       INNER JOIN syscolumns c1
               ON c1.id = o1.id
                  AND c1.colid = fk.fkey
       INNER JOIN syscolumns c2
               ON c2.id = a1.id
                  AND c2.colid = fk.rkey
       INNER JOIN sysobjects s
               ON fk.constid = s.id
ORDER  BY a1.NAME
";

		List<TestArchiveableRelationship> IgnoreList
		{
			get
			{
				if (ignoreList == null)
				{
					ignoreList = new List<TestArchiveableRelationship>()
					{
						new(EDIMessageSchema.Constants.TableName, EDIMessageSchema.PK.Name, EDIMessageLogPivotSchema.Constants.TableName, EDIMessageLogPivotSchema.EML_EM.Name, false),

						new(HVLVBookingHeaderSchema.Constants.TableName,HVLVBookingHeaderSchema.PK.Name, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.HVC_HVH_BookingHeader.Name, false),
						new(HVLVConsignmentSchema.Constants.TableName,HVLVConsignmentSchema.PK.Name, CusUSLVConsignmentSchema.Constants.TableName, CusUSLVConsignmentSchema.ULB_HVC_Consignment.Name, false),
						new(HVLVConsignmentSchema.Constants.TableName,HVLVConsignmentSchema.PK.Name, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Former.Name, false),
						new(HVLVConsignmentSchema.Constants.TableName,HVLVConsignmentSchema.PK.Name, HVLVReturnPivotSchema.Constants.TableName, HVLVReturnPivotSchema.HVP_HVC_Return.Name, false),
						new(HVLVItemSchema.Constants.TableName, HVLVItemSchema.PK.Name, "HVLVUsage", "HXU_HVI_ParentItem", false ),
						new(HVLVOriginLoadListSchema.Constants.TableName, HVLVOriginLoadListSchema.PK.Name, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.HVO_HVL_LoadList.Name, false),
						new(HVLVOriginLoadListSchema.Constants.TableName, HVLVOriginLoadListSchema.PK.Name, HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_HVL_LoadList.Name, false),
						new(HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.PK.Name,HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_HVO_OuterPackage.Name, false),
						new("HVLVShipmentHistory", "HSH_PK", "HVLVUsageHistory", "HUS_HSH_ShipmentHistory", false),

						new(JobBookedCtgMoveSchema.Constants.TableName, JobBookedCtgMoveSchema.PK.Name, JobBookedCtgMovPkgDivotSchema.Constants.TableName, JobBookedCtgMovPkgDivotSchema.KE_EW_CartageMove.Name, false),

						new(JobChargeSchema.Constants.TableName, JobChargeSchema.PK.Name, JobChargeTargetSchema.Constants.TableName, JobChargeTargetSchema.JRT_JR.Name, false),
						new(JobChargeSchema.Constants.TableName, JobChargeSchema.PK.Name, JobChargeSchema.Constants.TableName, JobChargeSchema.JR_JR_RevenueLine.Name, false), // Ignored in WI00577008: Unused and should potentially be removed from schema_main.
						new(JobChargeSchema.Constants.TableName, JobChargeSchema.PK.Name, JobChargePostingQueueSchema.Constants.TableName, JobChargePostingQueueSchema.JPQ_JR.Name, false),

						new(JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK.Name, JobComInvLineComponentInventorySchema.Constants.TableName, JobComInvLineComponentInventorySchema.JIV_JI.Name, false),
						new(JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.PK.Name, JobTWComInvoiceLineSchema.Constants.TableName, JobTWComInvoiceLineSchema.TWL_JI.Name, false),

						new(JobConsolSchema.Constants.TableName, JobConsolSchema.PK.Name, ConsolidationProfitShareSchema.Constants.TableName, ConsolidationProfitShareSchema.CPS_JK.Name, false),
						new(JobConsolSchema.Constants.TableName, JobConsolSchema.PK.Name, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.HVO_JK_LoadedOnConsol.Name, false),

						new(JobConsolCostSchema.Constants.TableName, JobConsolCostSchema.PK.Name, JobConsolCostAttribSchema.Constants.TableName, JobConsolCostAttribSchema.E6A_E6_JobConsolCost.Name, false),
						new(JobConsolCostSchema.Constants.TableName, JobConsolCostSchema.PK.Name, JobChargeSchema.Constants.TableName, JobChargeSchema.JR_E6_GatewaySellHeader.Name, false),

						new(JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK.Name, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.HVC_JE_ImportDeclaration.Name, false),
						new(JobDeclarationSchema.Constants.TableName, JobDeclarationSchema.PK.Name, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.HVC_JE_ExportDeclaration.Name, false),

						new(JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK.Name, "JobToCloseQueue", "JHC_JH", false), // WI00628513: JobToCloseQueue is a queue and is exclusively accessed via raw SQL. Also the delete rule is CASCADE. See WI for more explanation.

						new(JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK.Name, ConsolidationProfitShareSchema.Constants.TableName, ConsolidationProfitShareSchema.CPS_JH_ConsolJob.Name, false),
						new(JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK.Name, AccTransactionLinesSchema.Constants.TableName, AccTransactionLinesSchema.AL_JH.Name, false), // Nullified
						new(JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK.Name, AccCashAdvanceRequestHeaderSchema.Constants.TableName, AccCashAdvanceRequestHeaderSchema.CAH_JH_Job.Name, false),
						new(JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK.Name, OrgCommissionCalculationQueueSchema.Constants.TableName, OrgCommissionCalculationQueueSchema.CAQ_JH.Name, false),
						new(JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK.Name, AccTransactionHeaderSchema.Constants.TableName, AccTransactionHeaderSchema.AH_JH.Name, false), // Nullified
						new(JobHeaderSchema.Constants.TableName, JobHeaderSchema.PK.Name, AccHotChequeSchema.Constants.TableName, AccHotChequeSchema.AQ_JH.Name, false), // Nullified

						new(JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK.Name, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.HVO_JL_PackLine.Name, false),
						new(JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK.Name, JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.JL_JL_OuterPackLine.Name, false),
						new(JobPackLinesSchema.Constants.TableName, JobPackLinesSchema.PK.Name, JobSupplierBookingLineSchema.Constants.TableName, JobSupplierBookingLineSchema.JSL_JL_LooseCargo.Name, false),

						new(JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK.Name, ELoadListSchema.Constants.TableName, ELoadListSchema.DO_JS_MasterHouseShipment.Name, false),
						new(JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK.Name, "HVLVScanningSummary", "HSR_JS_Shipment", false),
						new(JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK.Name, JobContainerPenaltySchema.Constants.TableName, JobContainerPenaltySchema.CPY_JS_Shipment.Name, false),
						new(JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK.Name, HVLVItemSchema.Constants.TableName, HVLVItemSchema.HVI_JS_LoadedOnShipment.Name, false),
						new(JobShipmentSchema.Constants.TableName, JobShipmentSchema.PK.Name, SupplierBookingLineSchema.Constants.TableName, SupplierBookingLineSchema.DL_JS_ApprovedShipment.Name, false),

						new(RateLinesSchema.Constants.TableName, RateLinesSchema.PK.Name, RateLineItemsSchema.Constants.TableName, RateLineItemsSchema.TM_TL.Name, false),

						new(RateEntrySchema.Constants.TableName, RateEntrySchema.PK.Name, RateLinesSchema.Constants.TableName, RateLinesSchema.TL_TI.Name, false),

						new(RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK.Name, RateAttachmentSchema.Constants.TableName, RateAttachmentSchema.TA_TH.Name, false),
						new(RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK.Name, RateEntrySchema.Constants.TableName, RateEntrySchema.TI_TH.Name, false),
						new(RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK.Name, RateTariffDiscountSchema.Constants.TableName, RateTariffDiscountSchema.TD_TH.Name, false),
						new(RatingHeaderSchema.Constants.TableName, RatingHeaderSchema.PK.Name, QuoteScopeSchema.Constants.TableName, QuoteScopeSchema.QS_TH_RatingHeader.Name, false),

						// TODO: WI00693357 (as suggested by Core Components team) - there is no need to cascade archive for this relashionship, fk can be set to null
						new(CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK.Name, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.BM_BM_DepartureMovement.Name, false),

						new(CusBRForeignOperatorSchema.Constants.TableName, CusBRForeignOperatorSchema.PK.Name, CusGoodsCatalogProductionInfoSchema.Constants.TableName, CusGoodsCatalogProductionInfoSchema.CGI_BFR_ForeignOperator.Name, false),

						new(JobEquipmentSchema.Constants.TableName, JobEquipmentSchema.PK.Name, JobEquipmentItemSchema.Constants.TableName, JobEquipmentItemSchema.JEI_JEQ_JobEquipment.Name, false),

						new (ExternalRequestTypeSchema.Constants.TableName, ExternalRequestTypeSchema.PK.Name, ExternalRequestSchema.Constants.TableName, ExternalRequestSchema.REQ_RQT_Type.Name, false),
						new (ExternalRequestTypeSchema.Constants.TableName, ExternalRequestTypeSchema.PK.Name, ProcessTemplateValidationSchema.Constants.TableName, ProcessTemplateValidationSchema.P0V_RQT_RequestTypeOnFailure.Name, false),

						new (ContainerPenaltyDayExclusionSchema.Constants.TableName, ContainerPenaltyDayExclusionSchema.PK.Name, RatingContractContainerDetentionSchema.Constants.TableName, RatingContractContainerDetentionSchema.RCD_CEX_DurationExclusion.Name, false),
						new (ContainerPenaltyDayExclusionSchema.Constants.TableName, ContainerPenaltyDayExclusionSchema.PK.Name, RatingContractContainerDetentionSchema.Constants.TableName, RatingContractContainerDetentionSchema.RCD_CEX_FreeDayExclusion.Name, false),
						new (ContainerPenaltyDayExclusionSchema.Constants.TableName, ContainerPenaltyDayExclusionSchema.PK.Name, OrgContainerDetentionSchema.Constants.TableName, OrgContainerDetentionSchema.PD_CEX_FreeDayExclusion.Name, false),
						new (ContainerPenaltyDayExclusionSchema.Constants.TableName, ContainerPenaltyDayExclusionSchema.PK.Name, OrgContainerDetentionSchema.Constants.TableName, OrgContainerDetentionSchema.PD_CEX_DurationExclusion.Name, isReversed: false),
						new (ContainerPenaltyDayExclusionSchema.Constants.TableName, ContainerPenaltyDayExclusionSchema.PK.Name, JobContainerPenaltySchema.Constants.TableName, JobContainerPenaltySchema.CPY_CEX_FreeDayExclusion.Name, false),
						new (ContainerPenaltyDayExclusionSchema.Constants.TableName, ContainerPenaltyDayExclusionSchema.PK.Name, JobContainerPenaltySchema.Constants.TableName, JobContainerPenaltySchema.CPY_CEX_DurationExclusion.Name, isReversed: false),

						new (ExternalRequestInfoTemplateSchema.Constants.TableName, ExternalRequestInfoTemplateSchema.PK.Name, ExternalRequestTypeSchema.Constants.TableName, ExternalRequestTypeSchema.RQT_RIT_Template.Name, false),

						new (CusEntryNumSchema.Constants.TableName, CusEntryNumSchema.PK.Name, CusTRPreviousDocumentSchema.Constants.TableName, CusTRPreviousDocumentSchema.TPD_CE_EntryNumber.Name, false),
						new (CusTRPreviousDocumentSchema.Constants.TableName, CusTRPreviousDocumentSchema.PK.Name, CusTRPreviousDocumentItemSchema.Constants.TableName, CusTRPreviousDocumentItemSchema.TPI_TPD.Name, false),

						new (JobStorageSchema.Constants.TableName, JobStorageSchema.PK.Name, CYDYardStorageLinesSchema.Constants.TableName, CYDYardStorageLinesSchema.YSL_ET_JobStorage.Name, false),
					};
				}

				return ignoreList;
			}
		}

		List<TestArchiveableRelationship> ignoreList;

		public void TestSetupArchiveRelationships_LinksEDIMessageToAllArchiveableRelationships()
		{
			var archiveStage = new ArchiveStage(new DummyArchiveStage(), null);
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);

			ArchiveRelationships.SetupArchiveRelationships(archiveStage, config);

			var tableNames = GetTableNamesMissingEDIMessageRelationship(archiveStage);

			Assert($"The following archiveable relations should be linked to EDIMessage:\r\n{string.Join("\r\n", tableNames.Select(r => r))}", !(tableNames.Count > 0));
		}

		List<string> GetTableNamesMissingEDIMessageRelationship(ArchiveStage archiveStage)
		{
			var tableNames = new List<string>();

			foreach (var archiveableRelationships in archiveStage.ArchiveableRelationships)
			{
				if (archiveableRelationships.Key == EDIMessageSchema.Constants.TableName)
				{
					continue;
				}

				var includesEDIMessageRelation = archiveableRelationships.Value.Any(listOfArchiveableRelationships
					=> listOfArchiveableRelationships.ChildName.Equals(EDIMessageSchema.Constants.TableName)
					&& listOfArchiveableRelationships.ChildFKColumn.Name.Equals(EDIMessageSchema.Constants.EM_LinkUniqueID));				

				if (!includesEDIMessageRelation)
				{
					tableNames.Add(archiveableRelationships.Key);
				}
			}

			return tableNames;
		}

		#endregion

		public void TestArchiveRelationshipsAndCustomsArchiveRelationshipsAreMutuallyExclusive()
		{
			var archiveStage = new ArchiveStage(new DummyArchiveStage(), null);
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			ArchiveRelationships.SetupArchiveRelationships(archiveStage, config);

			var archiveStageForCustoms = new ArchiveStage(new DummyArchiveStage(), null);
			CustomsArchiveRelationships.SetupCustomsArchiveRelationships(archiveStageForCustoms);

			var output = archiveStage.ArchiveableRelationships.SelectMany(r => r.Value)
				.Intersect(archiveStageForCustoms.ArchiveableRelationships.SelectMany(r => r.Value), new ArchiveableRelationshipComparer());

			AssertEquals($"There should be no ArchiveableRelationships in common.", 0, output.Count());
		}
	}
}
