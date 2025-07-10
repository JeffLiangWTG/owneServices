using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class Dashboard : NonPersistentBusinessObject, IObsoleteValidation
	{
		public enum TimeFrame { Past, Today, Future }

		public Dashboard(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new void Refresh()
		{
			TodaysDate = ZDateTime.Now;

			fAdminNoStatusPastArrivalShipments = CargoReportCount(TimeFrame.Past, CargoReportAdminNoStatusQueues);
			fAdminNoStatusPastArrivalPieces = CargoReportPieceSum(TimeFrame.Past, CargoReportAdminNoStatusQueues);
			fAdminHeldPastArrivalShipments = Count(TimeFrame.Past, CargoReportAdminHeldQueues, DecHoldQueue, DecAdminHeldReasons) + DecCount(TimeFrame.Past, DecEIRQueue, Array.Empty<string>());
			fAdminHeldPastArrivalPieces = PieceSum(TimeFrame.Past, CargoReportAdminHeldQueues, DecHoldQueue, DecAdminHeldReasons) + DecPieceSum(TimeFrame.Past, DecEIRQueue, Array.Empty<string>());
			fAdminHousePastArrivalShipments = Count(TimeFrame.Past, CargoReportAdminHouseQueues, DecHoldQueue, DecAdminHouseReasons) + DecCount(TimeFrame.Past, DecAdminHouseQueues, Array.Empty<string>());
			fAdminHousePastArrivalPieces = PieceSum(TimeFrame.Past, CargoReportAdminHouseQueues, DecHoldQueue, DecAdminHouseReasons) + DecPieceSum(TimeFrame.Past, DecAdminHouseQueues, Array.Empty<string>());
			fOpsClassifiersPastArrivalShipments = DecCount(TimeFrame.Past, DecHoldQueue, DecOpsClassifiersReasons) + DecCount(TimeFrame.Past, DecOpsClassifiersQueues, Array.Empty<string>());
			fOpsClassifiersPastArrivalPieces = DecPieceSum(TimeFrame.Past, DecHoldQueue, DecOpsClassifiersReasons) + DecPieceSum(TimeFrame.Past, DecOpsClassifiersQueues, Array.Empty<string>());
			fOpsBrokersPastArrivalShipments = DecCount(TimeFrame.Past, DecOpsBrkQueues, Array.Empty<string>());
			fOpsBrokersPastArrivalPieces = DecPieceSum(TimeFrame.Past, DecOpsBrkQueues, Array.Empty<string>());

			fAdminNoStatusTodayArrivalShipments = CargoReportCount(TimeFrame.Today, CargoReportAdminNoStatusQueues);
			fAdminNoStatusTodayArrivalPieces = CargoReportPieceSum(TimeFrame.Today, CargoReportAdminNoStatusQueues);
			fAdminHeldTodayArrivalShipments = Count(TimeFrame.Today, CargoReportAdminHeldQueues, DecHoldQueue, DecAdminHeldReasons) + DecCount(TimeFrame.Today, DecEIRQueue, Array.Empty<string>());
			fAdminHeldTodayArrivalPieces = PieceSum(TimeFrame.Today, CargoReportAdminHeldQueues, DecHoldQueue, DecAdminHeldReasons) + DecPieceSum(TimeFrame.Today, DecEIRQueue, Array.Empty<string>());
			fAdminHouseTodayArrivalShipments = Count(TimeFrame.Today, CargoReportAdminHouseQueues, DecHoldQueue, DecAdminHouseReasons) + DecCount(TimeFrame.Today, DecAdminHouseQueues, Array.Empty<string>());
			fAdminHouseTodayArrivalPieces = PieceSum(TimeFrame.Today, CargoReportAdminHouseQueues, DecHoldQueue, DecAdminHouseReasons) + DecPieceSum(TimeFrame.Today, DecAdminHouseQueues, Array.Empty<string>());
			fOpsClassifiersTodayArrivalShipments = DecCount(TimeFrame.Today, DecHoldQueue, DecOpsClassifiersReasons) + DecCount(TimeFrame.Today, DecOpsClassifiersQueues, Array.Empty<string>());
			fOpsClassifiersTodayArrivalPieces = DecPieceSum(TimeFrame.Today, DecHoldQueue, DecOpsClassifiersReasons) + DecPieceSum(TimeFrame.Today, DecOpsClassifiersQueues, Array.Empty<string>());
			fOpsBrokersTodayArrivalShipments = DecCount(TimeFrame.Today, DecOpsBrkQueues, Array.Empty<string>());
			fOpsBrokersTodayArrivalPieces = DecPieceSum(TimeFrame.Today, DecOpsBrkQueues, Array.Empty<string>());

			fAdminNoStatusFutureArrivalShipments = CargoReportCount(TimeFrame.Future, CargoReportAdminNoStatusQueues);
			fAdminNoStatusFutureArrivalPieces = CargoReportPieceSum(TimeFrame.Future, CargoReportAdminNoStatusQueues);
			fAdminHeldFutureArrivalShipments = Count(TimeFrame.Future, CargoReportAdminHeldQueues, DecHoldQueue, DecAdminHeldReasons) + DecCount(TimeFrame.Future, DecEIRQueue, Array.Empty<string>());
			fAdminHeldFutureArrivalPieces = PieceSum(TimeFrame.Future, CargoReportAdminHeldQueues, DecHoldQueue, DecAdminHeldReasons) + DecPieceSum(TimeFrame.Future, DecEIRQueue, Array.Empty<string>());
			fAdminHouseFutureArrivalShipments = Count(TimeFrame.Future, CargoReportAdminHouseQueues, DecHoldQueue, DecAdminHouseReasons) + DecCount(TimeFrame.Future, DecAdminHouseQueues, Array.Empty<string>());
			fAdminHouseFutureArrivalPieces = PieceSum(TimeFrame.Future, CargoReportAdminHouseQueues, DecHoldQueue, DecAdminHouseReasons) + DecPieceSum(TimeFrame.Future, DecAdminHouseQueues, Array.Empty<string>());
			fOpsClassifiersFutureArrivalShipments = DecCount(TimeFrame.Future, DecHoldQueue, DecOpsClassifiersReasons) + DecCount(TimeFrame.Future, DecOpsClassifiersQueues, Array.Empty<string>());
			fOpsClassifiersFutureArrivalPieces = DecPieceSum(TimeFrame.Future, DecHoldQueue, DecOpsClassifiersReasons) + DecPieceSum(TimeFrame.Future, DecOpsClassifiersQueues, Array.Empty<string>());
			fOpsBrokersFutureArrivalShipments = DecCount(TimeFrame.Future, DecOpsBrkQueues, Array.Empty<string>());
			fOpsBrokersFutureArrivalPieces = DecPieceSum(TimeFrame.Future, DecOpsBrkQueues, Array.Empty<string>());

			RefreshBinding();
		}
		ZDateTime TodaysDate;

		#region Properties for binding

		#region Past Arrivals

		#region AdminNoStatusPastArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminNoStatusPastArrivalShipments
		{
			get { return fAdminNoStatusPastArrivalShipments.ToString(); }
		}
		ZInt fAdminNoStatusPastArrivalShipments;

		public ZPropertyInfo AdminNoStatusPastArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminNoStatusPastArrivalShipments)); }
		}

		#endregion

		#region AdminNoStatusPastArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminNoStatusPastArrivalPieces
		{
			get { return fAdminNoStatusPastArrivalPieces.ToString(); }
		}
		ZInt fAdminNoStatusPastArrivalPieces;

		public ZPropertyInfo AdminNoStatusPastArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminNoStatusPastArrivalPieces)); }
		}

		#endregion

		#region AdminHeldPastArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHeldPastArrivalShipments
		{
			get { return fAdminHeldPastArrivalShipments.ToString(); }
		}
		ZInt fAdminHeldPastArrivalShipments;

		public ZPropertyInfo AdminHeldPastArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHeldPastArrivalShipments)); }
		}

		#endregion

		#region AdminHeldPastArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHeldPastArrivalPieces
		{
			get { return fAdminHeldPastArrivalPieces.ToString(); }
		}
		ZInt fAdminHeldPastArrivalPieces;

		public ZPropertyInfo AdminHeldPastArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHeldPastArrivalPieces)); }
		}

		#endregion

		#region AdminHousePastArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHousePastArrivalShipments
		{
			get { return fAdminHousePastArrivalShipments.ToString(); }
		}
		ZInt fAdminHousePastArrivalShipments;

		public ZPropertyInfo AdminHousePastArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHousePastArrivalShipments)); }
		}

		#endregion

		#region AdminHousePastArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHousePastArrivalPieces
		{
			get { return fAdminHousePastArrivalPieces.ToString(); }
		}
		ZInt fAdminHousePastArrivalPieces;

		public ZPropertyInfo AdminHousePastArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHousePastArrivalPieces)); }
		}

		#endregion

		#region OpsClassifiersPastArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsClassifiersPastArrivalShipments
		{
			get { return fOpsClassifiersPastArrivalShipments.ToString(); }
		}
		ZInt fOpsClassifiersPastArrivalShipments;

		public ZPropertyInfo OpsClassifiersPastArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(OpsClassifiersPastArrivalShipments)); }
		}

		#endregion

		#region OpsClassifiersPastArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsClassifiersPastArrivalPieces
		{
			get { return fOpsClassifiersPastArrivalPieces.ToString(); }
		}
		ZInt fOpsClassifiersPastArrivalPieces;

		public ZPropertyInfo OpsClassifiersPastArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(OpsClassifiersPastArrivalPieces)); }
		}

		#endregion

		#region OpsBrokersPastArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsBrokersPastArrivalShipments
		{
			get { return fOpsBrokersPastArrivalShipments.ToString(); }
		}
		ZInt fOpsBrokersPastArrivalShipments;

		public ZPropertyInfo OpsBrokersPastArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(OpsBrokersPastArrivalShipments)); }
		}

		#endregion

		#region OpsBrokersPastArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsBrokersPastArrivalPieces
		{
			get { return fOpsBrokersPastArrivalPieces.ToString(); }
		}
		ZInt fOpsBrokersPastArrivalPieces;

		public ZPropertyInfo OpsBrokersPastArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(OpsBrokersPastArrivalPieces)); }
		}

		#endregion

		#endregion

		#region Today Arrivals

		#region AdminNoStatusTodayArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminNoStatusTodayArrivalShipments
		{
			get { return fAdminNoStatusTodayArrivalShipments.ToString(); }
		}
		ZInt fAdminNoStatusTodayArrivalShipments;

		public ZPropertyInfo AdminNoStatusTodayArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminNoStatusTodayArrivalShipments)); }
		}

		#endregion

		#region AdminNoStatusTodayArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminNoStatusTodayArrivalPieces
		{
			get { return fAdminNoStatusTodayArrivalPieces.ToString(); }
		}
		ZInt fAdminNoStatusTodayArrivalPieces;

		public ZPropertyInfo AdminNoStatusTodayArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminNoStatusTodayArrivalPieces)); }
		}

		#endregion

		#region AdminHeldTodayArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHeldTodayArrivalShipments
		{
			get { return fAdminHeldTodayArrivalShipments.ToString(); }
		}
		ZInt fAdminHeldTodayArrivalShipments;

		public ZPropertyInfo AdminHeldTodayArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHeldTodayArrivalShipments)); }
		}

		#endregion

		#region AdminHeldTodayArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHeldTodayArrivalPieces
		{
			get { return fAdminHeldTodayArrivalPieces.ToString(); }
		}
		ZInt fAdminHeldTodayArrivalPieces;

		public ZPropertyInfo AdminHeldTodayArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHeldTodayArrivalPieces)); }
		}

		#endregion

		#region AdminHouseTodayArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHouseTodayArrivalShipments
		{
			get { return fAdminHouseTodayArrivalShipments.ToString(); }
		}
		ZInt fAdminHouseTodayArrivalShipments;

		public ZPropertyInfo AdminHouseTodayArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHouseTodayArrivalShipments)); }
		}

		#endregion

		#region AdminHouseTodayArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHouseTodayArrivalPieces
		{
			get { return fAdminHouseTodayArrivalPieces.ToString(); }
		}
		ZInt fAdminHouseTodayArrivalPieces;

		public ZPropertyInfo AdminHouseTodayArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHouseTodayArrivalPieces)); }
		}

		#endregion

		#region OpsClassifiersTodayArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsClassifiersTodayArrivalShipments
		{
			get { return fOpsClassifiersTodayArrivalShipments.ToString(); }
		}
		ZInt fOpsClassifiersTodayArrivalShipments;

		public ZPropertyInfo OpsClassifiersTodayArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(OpsClassifiersTodayArrivalShipments)); }
		}

		#endregion

		#region OpsClassifiersTodayArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsClassifiersTodayArrivalPieces
		{
			get { return fOpsClassifiersTodayArrivalPieces.ToString(); }
		}
		ZInt fOpsClassifiersTodayArrivalPieces;

		public ZPropertyInfo OpsClassifiersTodayArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(OpsClassifiersTodayArrivalPieces)); }
		}

		#endregion

		#region OpsBrokersTodayArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsBrokersTodayArrivalShipments
		{
			get { return fOpsBrokersTodayArrivalShipments.ToString(); }
		}
		ZInt fOpsBrokersTodayArrivalShipments;

		public ZPropertyInfo OpsBrokersTodayArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(OpsBrokersTodayArrivalShipments)); }
		}

		#endregion

		#region OpsBrokersTodayArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsBrokersTodayArrivalPieces
		{
			get { return fOpsBrokersTodayArrivalPieces.ToString(); }
		}
		ZInt fOpsBrokersTodayArrivalPieces;

		public ZPropertyInfo OpsBrokersTodayArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(OpsBrokersTodayArrivalPieces)); }
		}

		#endregion

		#endregion

		#region FutureArrivals

		#region AdminNoStatusFutureArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminNoStatusFutureArrivalShipments
		{
			get { return fAdminNoStatusFutureArrivalShipments.ToString(); }
		}
		ZInt fAdminNoStatusFutureArrivalShipments;

		public ZPropertyInfo AdminNoStatusFutureArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminNoStatusFutureArrivalShipments)); }
		}

		#endregion

		#region AdminNoStatusFutureArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminNoStatusFutureArrivalPieces
		{
			get { return fAdminNoStatusFutureArrivalPieces.ToString(); }
		}
		ZInt fAdminNoStatusFutureArrivalPieces;

		public ZPropertyInfo AdminNoStatusFutureArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminNoStatusFutureArrivalPieces)); }
		}

		#endregion

		#region AdminHeldFutureArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHeldFutureArrivalShipments
		{
			get { return fAdminHeldFutureArrivalShipments.ToString(); }
		}
		ZInt fAdminHeldFutureArrivalShipments;

		public ZPropertyInfo AdminHeldFutureArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHeldFutureArrivalShipments)); }
		}

		#endregion

		#region AdminHeldFutureArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHeldFutureArrivalPieces
		{
			get { return fAdminHeldFutureArrivalPieces.ToString(); }
		}
		ZInt fAdminHeldFutureArrivalPieces;

		public ZPropertyInfo AdminHeldFutureArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHeldFutureArrivalPieces)); }
		}

		#endregion

		#region AdminHouseFutureArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHouseFutureArrivalShipments
		{
			get { return fAdminHouseFutureArrivalShipments.ToString(); }
		}
		ZInt fAdminHouseFutureArrivalShipments;

		public ZPropertyInfo AdminHouseFutureArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHouseFutureArrivalShipments)); }
		}

		#endregion

		#region AdminHouseFutureArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString AdminHouseFutureArrivalPieces
		{
			get { return fAdminHouseFutureArrivalPieces.ToString(); }
		}
		ZInt fAdminHouseFutureArrivalPieces;

		public ZPropertyInfo AdminHouseFutureArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(AdminHouseFutureArrivalPieces)); }
		}

		#endregion

		#region OpsClassifiersFutureArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsClassifiersFutureArrivalShipments
		{
			get { return fOpsClassifiersFutureArrivalShipments.ToString(); }
		}
		ZInt fOpsClassifiersFutureArrivalShipments;

		public ZPropertyInfo OpsClassifiersFutureArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(OpsClassifiersFutureArrivalShipments)); }
		}

		#endregion

		#region OpsClassifiersFutureArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsClassifiersFutureArrivalPieces
		{
			get { return fOpsClassifiersFutureArrivalPieces.ToString(); }
		}
		ZInt fOpsClassifiersFutureArrivalPieces;

		public ZPropertyInfo OpsClassifiersFutureArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(OpsClassifiersFutureArrivalPieces)); }
		}

		#endregion

		#region OpsBrokersFutureArrivalShipments

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsBrokersFutureArrivalShipments
		{
			get { return fOpsBrokersFutureArrivalShipments.ToString(); }
		}
		ZInt fOpsBrokersFutureArrivalShipments;

		public ZPropertyInfo OpsBrokersFutureArrivalShipmentsInfo
		{
			get { return GetZPropertyInfo(nameof(OpsBrokersFutureArrivalShipments)); }
		}

		#endregion

		#region OpsBrokersFutureArrivalPieces

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString OpsBrokersFutureArrivalPieces
		{
			get { return fOpsBrokersFutureArrivalPieces.ToString(); }
		}
		ZInt fOpsBrokersFutureArrivalPieces;

		public ZPropertyInfo OpsBrokersFutureArrivalPiecesInfo
		{
			get { return GetZPropertyInfo(nameof(OpsBrokersFutureArrivalPieces)); }
		}

		#endregion

		#endregion

		#endregion

		#region Gather Data

		#region Queues and Reasons

		readonly string[] CargoReportAdminNoStatusQueues = new string[] { CargoReportQueueCodeDescriptionPairList.Codes.Unknown };

		readonly string[] CargoReportAdminHeldQueues = new string[]  {   CargoReportQueueCodeDescriptionPairList.Codes.EIR,
																																CargoReportQueueCodeDescriptionPairList.Codes.Hold,
																																CargoReportQueueCodeDescriptionPairList.Codes.Intervention
																												};

		readonly string[] DecAdminHeldReasons = new string[] {       ReasonCodeDescriptionPairList.Codes._34_Missort,
																																ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration,
																																ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments
																								};

		readonly string[] CargoReportAdminHouseQueues = new string[] {   CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation,
																																CargoReportQueueCodeDescriptionPairList.Codes.Quarantine
																												};

		readonly string[] DecAdminHouseQueues = new string[] {       DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding
																								};

		readonly string[] DecAdminHouseReasons = new string[]  {     ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold,
																																ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection,
																																ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold,
																																ReasonCodeDescriptionPairList.Codes.SS_CustomsHold
																								};

		readonly string[] DecOpsClassifiersQueues = new string[] {     DeclarationQueueCodeDescriptionPairList.Codes.Pending,
																																DeclarationQueueCodeDescriptionPairList.Codes.Unknown,
																																DeclarationQueueCodeDescriptionPairList.Codes.Classification,
																																DeclarationQueueCodeDescriptionPairList.Codes.Compiling
													};

		readonly string[] DecOpsClassifiersReasons = new string[]  {   ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment,
																																ReasonCodeDescriptionPairList.Codes._R0_Rebill,
																																ReasonCodeDescriptionPairList.Codes._R1_Abandon,
																																ReasonCodeDescriptionPairList.Codes._R2_Transhipment,
																																ReasonCodeDescriptionPairList.Codes._R3_RTS,
																																ReasonCodeDescriptionPairList.Codes._R4_FreeDomicile,
																																ReasonCodeDescriptionPairList.Codes.AM_RefusedCancelledOrder,
																																ReasonCodeDescriptionPairList.Codes.AN_RefusedDuplicateOrder,
																																ReasonCodeDescriptionPairList.Codes.AS_RefusedShippedTooLate,
																																ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription,
																																ReasonCodeDescriptionPairList.Codes.BK_CertificateOfOriginRequired,
																																ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing,
																																ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment,
																																ReasonCodeDescriptionPairList.Codes.DN_SplitShipment,
																																ReasonCodeDescriptionPairList.Codes.FE_DutyTaxRefused,
																																ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired,
																																ReasonCodeDescriptionPairList.Codes.NR_FreeDomicile,
																																ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice,
																																ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments,
																																ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient,
																																ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease,
																																ReasonCodeDescriptionPairList.Codes.UD_AlternateDeliveryAddress,
																																ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration,
																																ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient,
																																ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid
																										};

		readonly string[] DecOpsBrkQueues = new string[] {         DeclarationQueueCodeDescriptionPairList.Codes.Lodgement,
																																DeclarationQueueCodeDescriptionPairList.Codes.Submitted
												};

		readonly string[] DecHoldQueue = new string[] { DeclarationQueueCodeDescriptionPairList.Codes.Hold };
		readonly string[] DecEIRQueue = new string[] { DeclarationQueueCodeDescriptionPairList.Codes.EIR };

		#endregion

		#region Sum Of Pieces

		ZInt PieceSum(TimeFrame timeFrame, string[] cargoReportQueues, string[] decQueues, string[] decReasons)
		{
			return CargoReportPieceSum(timeFrame, cargoReportQueues) + DecPieceSum(timeFrame, decQueues, decReasons);
		}

		#region Sum of Pieces Cargo Report

		ZInt CargoReportPieceSum(TimeFrame timeFrame, string[] queues)
		{
			return CusHAWBPieceSum(ProcessQueueSchema.P4_CustomsQueue, timeFrame, queues);
		}

		ZInt CusHAWBPieceSum(SchemaColumn queueNameColumn, TimeFrame timeFrame, string[] queues)
		{
			const string TotalPieces = "TotalPieces";
			ZString sqlText =
				"SELECT sum(" + CusHAWBSchema.Constants.CS_PiecesLanded + ") AS " + TotalPieces +
				" FROM " + CusHAWBSchema.Constants.SqlSchemaName + "." + CusHAWBSchema.Constants.TableName + " " +
				" INNER JOIN " + CusMAWBSchema.Constants.SqlSchemaName + "." + CusMAWBSchema.Constants.TableName + " on " + CusHAWBSchema.Constants.CS_CM + " = " + CusMAWBSchema.Constants.PK +
				" INNER JOIN " + ProcessQueueSchema.Constants.SqlSchemaName + "." + ProcessQueueSchema.Constants.TableName + " on " + ProcessQueueSchema.Constants.P4_ParentID + " = " + CusHAWBSchema.Constants.PK +
				" WHERE";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			AddDateSql(timeFrame, CusMAWBSchema.CM_ArrivalDate, ref sqlText, @params);
			AddOrSql(timeFrame, queueNameColumn, queues, true, ref sqlText, @params);

			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			var mawbQuery = new ZDBOnlyQuery(typeof(UPECusMAWB));
			mawbQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

			sqlText += " AND " + mawbQuery.LiteralTextSqlFormatted;

			DynamicBusinessObjectCollection totalPiecesList = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			totalPiecesList.Load(sqlText, @params);
			return (ZInt)totalPiecesList[0][TotalPieces];
		}

		#endregion

		#region Sum of Pieces Declaration

		ZInt DecPieceSum(TimeFrame timeFrame, string[] queues, string[] reasons)
		{
			return DecPieceSum(timeFrame, queues, reasons, true);
		}

		ZInt DecPieceSum(TimeFrame timeFrame, string[] queues, string[] reasons, bool includeReasons)
		{
			const string TotalPieces = "TotalPieces";
			ZString sqlText =
				"SELECT sum(" + JobDeclarationSchema.Constants.JE_TotalNoOfPieces + ") AS " + TotalPieces +
				" FROM " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName + " " +
				" INNER JOIN " + ProcessQueueSchema.Constants.SqlSchemaName + "." + ProcessQueueSchema.Constants.TableName + " on " + ProcessQueueSchema.Constants.P4_ParentID + " = " + JobDeclarationSchema.Constants.PK +
				" WHERE";

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			AddDateSql(timeFrame, JobDeclarationSchema.JE_DateOfArrival, ref sqlText, @params);
			AddOrSql(timeFrame, ProcessQueueSchema.P4_CustomsQueue, queues, true, ref sqlText, @params);
			AddOrSql(timeFrame, ProcessQueueSchema.P4_CustomsStatus, reasons, includeReasons, ref sqlText, @params);
			sqlText += " AND " + JobDeclarationSchema.Constants.JE_IsCancelled + " = @False";
			@params.Add("@False", Core.Constants.BooleanFalseString, JobDeclarationSchema.JE_IsCancelled);

			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			var jobDecQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			jobDecQuery.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);

			sqlText += " AND " + jobDecQuery.LiteralTextSqlFormatted;

			DynamicBusinessObjectCollection totalPiecesList = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			totalPiecesList.Load(sqlText, @params);
			return (ZInt)totalPiecesList[0][TotalPieces];
		}

		#endregion

		void AddDateSql(TimeFrame timeFrame, SchemaColumn column, ref ZString sqlText, ZSqlParameterCollection @params)
		{
			const string DateParam = "@DateParam";
			if (timeFrame == TimeFrame.Past)
			{
				sqlText += " " + column.Name + " < " + DateParam;
				@params.Add(DateParam, TodaysDate.AddDays(GetNumberOfDaysInThePast()).Date, column);
			}
			else if (timeFrame == TimeFrame.Today)
			{
				sqlText += " " + column.Name + " >= " + DateParam;
				@params.Add(DateParam, TodaysDate.AddDays(GetNumberOfDaysInThePast()).Date, column);

				sqlText += " AND " + column.Name + " < " + DateParam + "2";
				@params.Add(DateParam + "2", TodaysDate.Date.AddDays(1), column);
			}
			else if (timeFrame == TimeFrame.Future)
			{
				sqlText += " " + column.Name + " >= " + DateParam;
				@params.Add(DateParam, TodaysDate.Date.AddDays(1), column);
			}
		}

		void AddOrSql(TimeFrame timeFrame, SchemaColumn column, string[] queues, bool include, ref ZString sqlText, ZSqlParameterCollection @params)
		{
			if (queues.Length > 0)
			{
				sqlText += " AND (";

				string comparisonSign = include ? "=" : "<>";
				string joinOperator = include ? "OR" : "AND";

				foreach (string s in queues)
				{
					sqlText += "  " + column.Name + " " + comparisonSign + " @_" + s + " " + joinOperator;
					@params.Add("@_" + s, s, column);
				}

				sqlText = sqlText.Left(sqlText.Length - joinOperator.Length) + ")";
			}
		}

		#endregion

		#region Counters

		ZInt CargoReportCount(TimeFrame timeFrame, string[] queues)
		{
			return Factory.GetDatabaseCount(typeof(UPECusHAWB), GetCargoReportFilter(timeFrame, queues));
		}

		ZInt DecCount(TimeFrame timeFrame, string[] queues, string[] reasons)
		{
			return Factory.GetDatabaseCount(typeof(UPEJobDeclaration), GetDecFilter(timeFrame, queues, reasons));
		}

		ZInt Count(TimeFrame timeFrame, string[] cargoReportQueues, string[] decQueues, string[] decReasons)
		{
			return CargoReportCount(timeFrame, cargoReportQueues) + DecCount(timeFrame, decQueues, decReasons);
		}

		#region CusHAWB Filter

		ZQuery GetCargoReportFilter(TimeFrame timeFrame, string[] queues)
		{
			return GetCusHAWBFilter(ProcessQueueSchema.P4_CustomsQueue, timeFrame, queues);
		}

		ZQuery GetCusHAWBFilter(SchemaColumn queueNameColumn, TimeFrame timeFrame, string[] queues)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(UPECusHAWB));

			ZDBOnlySubQuery relatedUPECusMAWBs = new ZDBOnlySubQuery(typeof(UPECusMAWB), CusHAWBSchema.CS_CM);
			relatedUPECusMAWBs.AddToFilter(GetDateFilter(CusMAWBSchema.CM_ArrivalDate, timeFrame));
			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			relatedUPECusMAWBs.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

			ZDBOnlySubQuery relatedProcessQueue = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);

			if (queues.Length > 0)
			{
				relatedProcessQueue.AddToFilter(queueNameColumn, queues);
			}

			query.AddSubQuery(relatedUPECusMAWBs, JoinCondition.And);
			query.AddSubQuery(relatedProcessQueue, JoinCondition.And);

			return query;
		}

		#endregion

		#region DecFilter

		ZQuery GetDecFilter(TimeFrame timeFrame, string[] queues, string[] reasons)
		{
			return GetDecFilter(timeFrame, queues, reasons, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetDecFilter(TimeFrame timeFrame, string[] queues, string[] reasons, bool includeReasons)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			query.AddToFilter(GetDateFilter(JobDeclarationSchema.JE_DateOfArrival, timeFrame));

			ZDBOnlySubQuery relatedProcessQueue = new ZDBOnlySubQuery(typeof(ProcessQueue), ProcessQueueSchema.P4_ParentID);

			if (queues.Length > 0)
			{
				ZQuery queueFilter = new ZQuery(ProcessQueueSchema.P4_CustomsQueue, queues);
				relatedProcessQueue.AddToFilter(queueFilter, JoinCondition.And);
			}

			if (reasons.Length > 0)
			{
				ZQuery reasonFilter = new ZQuery();
				foreach (string reason in reasons)
				{
					if (includeReasons)
					{
						reasonFilter.AddToFilter(JoinCondition.Or, ProcessQueueSchema.P4_CustomsStatus, SQLComparisonOperator.Equal, reason);
					}
					else
					{
						reasonFilter.AddToFilter(JoinCondition.And, ProcessQueueSchema.P4_CustomsStatus, SQLComparisonOperator.NotEqual, reason);
					}
				}
				relatedProcessQueue.AddToFilter(reasonFilter, JoinCondition.And);
			}

			query.AddSubQuery(relatedProcessQueue, JoinCondition.And);

			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			query.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);

			return query;
		}

		#endregion

		#region Date Filter

		ZQuery GetDateFilter(SchemaColumn column, TimeFrame timeFrame)
		{
			ZQuery result = new ZQuery();
			if (timeFrame == TimeFrame.Today)
			{
				result.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, TodaysDate);
				result.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, TodaysDate.AddDays(GetNumberOfDaysInThePast()));
			}
			else if (timeFrame == TimeFrame.Future)
			{
				result.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.GreaterThanOrEqualTo, TodaysDate.Date.AddDays(1));
			}
			else if (timeFrame == TimeFrame.Past)
			{
				result.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.LessThan, TodaysDate.AddDays(GetNumberOfDaysInThePast()).Date);
			}

			return result;
		}

		int GetNumberOfDaysInThePast()
		{
			int numberOfDaysInThePast = 0;
			if (TodaysDate.DayOfWeek == DayOfWeek.Monday)
			{
				var workTimeArithmetic = WorkingDays.GetInstance(Factory, GlbDepartment.CurrentDepartment.PK, GlbBranch.CurrentBranch.PK);
				numberOfDaysInThePast = workTimeArithmetic.IsDateTimeAHoliday(TodaysDate.AddDays(-3).ToDateTime()) ? -3 : -2;
			}
			return numberOfDaysInThePast;
		}

		#endregion

		#endregion

		#endregion
	}
}
