using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using BaseBusiness = Enterprise.Customs.Business;
using BaseCusHAWB = Enterprise.Customs.Business.CusHAWB;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// AU Customs AirCargo Automation Messaging Job
	/// </summary>
	[DependentBusinessObject(typeof(CusMAWB), "ChildBills")]
	public class CusHAWB :
		CusHAWBBase,
		Integration.Customs.Shared.ICusHAWB,
		ICMRMessageRespondee,
		ICMRControlMessageRespondee,
		ICusUnderbondDependentCollectionParent,
		ICusMAWBProvider,
		IUnderbondMovementRequestHeaderProvider,
		IAirOutturnReportHeaderInformationProvider,
		IProcessQueueParent,
		IUnderbondDefaultValueProvider,
		IDetailsTabPageHeadingProvider,
		IDocManagerSupport,
		BaseBusiness.IMessageManageableBizObj,
		Integration.Customs.AU.ICusHAWB,
		IDataExportCSVFileNameProvider,
		IDocManagerSupportIncudingRelatedObjects,
		IWorkflowTriggerEventSource,
		IWorkflowProvider,
		IValidateForCustomsMessagingSupporter,
		IScanHouseBillProvider,
		IManifestInfo,
		IControllerIDProvider,
		ICargoDepotEventParent
	{
		public CusHAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Static Load/Create

		public static CusHAWB CreateNew(CusMAWB mAWB, ForwardingShipment shipment)
		{
			CusHAWB result = mAWB.ChildBills.AddNew();
			result.DefaultFromFreight(shipment);
			return result;
		}

		public static CusHAWB[] Load(BusinessObjectFactory factory, ZQuery query)
		{
			return factory.Load<BaseCusHAWB>(query).OfType<CusHAWB>().ToArray();
		}

		public static CusHAWB[] Load(BusinessObjectFactory factory, IList shipmentPKs, bool reloadExistingRows = false)
		{
			return Load(factory, new ZQuery(CusHAWBSchema.CS_JS, shipmentPKs) { ReLoadExistingRows = reloadExistingRows });
		}

		public static CusHAWB Load(CommonShipment shipment, bool reloadExistingRows = false)
		{
			return Load(shipment.Factory, new[] { shipment.PK }, reloadExistingRows).FirstOrDefault();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public static CusHAWB Load(BusinessObjectFactory factory, ICusHAWBInformationProvider hAWBInfo)
		{
			if (hAWBInfo.ArrivalDate.IsEmpty || !hAWBInfo.ArrivalDate.IsValid)
			{
				return null;
			}

			bool mAWBMatch = false;
			ZQuery hAWBMAWBFilter = new ZQuery();
			foreach (CusMAWB mAWB in new CusMAWBBase.Loader(factory).FindMatchingForwarderMAWBs(hAWBInfo.MAWB, hAWBInfo.FlightNumber, hAWBInfo.ArrivalDate))
			{
				hAWBMAWBFilter.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_CM, SQLComparisonOperator.Equal, mAWB.PK);
				mAWBMatch = true;
			}
			if (!mAWBMatch)
			{
				return null;
			}
			ZQuery hAWBFilter = new ZQuery();
			hAWBFilter.AddToFilter(hAWBMAWBFilter);
			hAWBFilter.AddToFilter(CusHAWBSchema.CS_HAWB, hAWBInfo.HAWB);

			return (CusHAWB)CusHAWBBase.LoadFromQuery(hAWBFilter, factory);
		}

		#endregion

		public bool IsDamaged;
		public bool IsPillaged;

		#region Constants
		public abstract new class Schema : CusHAWBBase.Schema
		{
			public const string CS_MessageStatus = "CS_MessageStatus";
			public const string CS_MessageMainStatus = "CS_MessageMainStatus";
			public const string Destination = "Destination";

			public const string CS_MasterBillNum = "CS_MasterBillNum";
			public const string CS_LoadPort = "CS_LoadPort";
			public const string CS_DischargePort = "CS_DischargePort";
			public const string InBondStore = "InBondStore";
		}

		public static class MessageStatus
		{
			public const string NotSent = "Not Sent";
		}

		public const string ZeroLandingMessage = "Zero-landing Message has been sent. It is suggested that you allow some time before sending messages on this house bill again.";

		#endregion

		#region Overrides

		public override void DefaultFromShipment()
		{
			SynchroniseData();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			SetMessageReference();
			AttachAnyUnattachedHouseCARSTS();
			LogIfCS_CustomsStatusChanged();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!IsInDatabase && Shipment == null)
			{
				CS_MessageReference = ZString.Empty;
			}

			base.OnSaved(saveSucceeded);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			try
			{
				using (GetValidationSuspender())
				{
					base.CS_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
					base.CS_RX_NKGoodsCurrency = JobDeclaration.LocalCurrencyConstantCode;
					base.CS_CustomsStatus = AirCargoMessage.NewStatus.NotSent;
					base.CS_CustomsMainStatus = AirCargoMessage.NewStatus.NotSent;
					base.CS_FreightPrepaidCollect = new CMRUtilities().ConvertOldAirCargoPaymentType(Env.Registry.ConsolPaymentTerm);
				}
			}
			finally
			{
				HasChanges = false;
			}
		}

		public override void OnLoaded()
		{
			using (SuspendSettingHasChanges())
			{
				base.OnLoaded();
				LandingToSendMessageOn = this;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusHAWBFetchStrategy(this);
		}

		[RelatedBusinessObject(nameof(MAWB))]
		public override ZGuid CS_CM
		{
			get { return base.CS_CM; }
			set { base.CS_CM = value; }
		}

		public override bool CanDelete
		{
			get { return CanDeleteWhenShipmentIsDetached && (MAWB == null || MAWB.Consol == null); }
		}

		public bool CanDeleteWhenShipmentIsDetached
		{
			get { return base.CanDelete && !HasActiveDCLOutturnLine; }
		}

		internal bool HasActiveDCLOutturnLine
		{
			get
			{
				if (MAWB != null && MAWB.Underbonds.Count > 0)
				{
					var query = new ZQuery(CusOutturnSchema.C5_ParentID, PK);
					query.AddToFilter(CusOutturnSchema.C5_C4_Underbond, MAWB.Underbonds.GetPKs());
					return Factory.LoadTop1<CusOutturn>(query) != null;
				}
				return false;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return HasActiveDCLOutturnLine ? HAWBCannotBeDeletedDueToOutturn : (MAWB != null && MAWB.Consol != null ? CannotDeleteHAWBsOnConsol : base.ReasonForNotAbleToDelete); }
		}

		internal static MultilingualString HAWBCannotBeDeletedDueToOutturn
		{
			get
			{
				return ResString.GetMultilingualString("42703FA3-A745-4B9E-8095-736175F0646E", "This HAWB cannot be deleted because there are outturn lines associated with it.");
			}
		}

		internal static MultilingualString CannotDeleteHAWBsOnConsol
		{
			get
			{
				return ResString.GetMultilingualString("BEB4350E-38FE-4E18-B848-D7ABC7FE47F3", "House Bills on a Consol cannot be deleted. Please delete/detach a related shipment instead.");
			}
		}

		protected override void DeleteCore()
		{
			if (!IsDeleted)
			{
				PartShips.RemoveAndDeleteAll();
				OrgMatchApproval.DeleteRelatedMatchApprovalsAndAddresses(this);
				ProcessQueueParentHelper.DeleteProcessQueue();
			}
			base.DeleteCore();
		}

		public override ZShort CS_PiecesManifested
		{
			get { return base.CS_PiecesManifested; }
			set
			{
				base.CS_PiecesManifested = value;
				base.CS_PiecesLanded = value;
			}
		}

		public override ZString CS_CustomsMainStatus
		{
			get
			{
				if (CS_IsResponsePending && !IsLastMessagePartshipment)
				{
					return AirCargoMessage.NewStatus.Waiting;
				}
				return base.CS_CustomsMainStatus;
			}
		}

		[RelatedBusinessObject(nameof(Shipment))]
		public override ZGuid CS_JS
		{
			get { return base.CS_JS; }
			set
			{
				if (CS_JS != value)
				{
					base.CS_JS = value;
					BusinessObject shipmentDeclaration = null;
					ZDateTime currentDeclarationTime = ZDateTime.Empty;
					if (Shipment != null)
					{
						foreach (BusinessObject dec in Shipment.Declarations)
						{
							StmALog mostRecentEvent = dec.GetLogs().MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
							if (mostRecentEvent == null || currentDeclarationTime.IsEmpty || mostRecentEvent.SL_EventTime > currentDeclarationTime)
							{
								shipmentDeclaration = dec;
								if (mostRecentEvent != null)
								{
									currentDeclarationTime = mostRecentEvent.SL_EventTime;
								}
							}
						}
					}

					if (IsCopying)
					{
						CS_JE_CustomsFormalEntry = (shipmentDeclaration == null) ? CS_JE_CustomsFormalEntry : shipmentDeclaration.PK;
					}
					else
					{
						((IBusinessObjectInternals)this).IsCopying = true;
						try
						{
							CS_JE_CustomsFormalEntry = (shipmentDeclaration == null) ? ZGuid.Empty : shipmentDeclaration.PK;
						}
						finally
						{
							((IBusinessObjectInternals)this).IsCopying = false;
						}
					}
				}
			}
		}

		public override ZGuid CS_JE_CustomsFormalEntry
		{
			get { return base.CS_JE_CustomsFormalEntry; }
			set
			{
				if (CS_JE_CustomsFormalEntry != value)
				{
					base.CS_JE_CustomsFormalEntry = value;
					if (!IsCopying)
					{
						BusinessObject chkDeclaration = Declaration;
						((IBusinessObjectInternals)this).IsCopying = true;
						try
						{
							CS_JS = (chkDeclaration == null) ? ZGuid.Empty : (ZGuid)chkDeclaration[JobDeclarationSchema.JE_JS.Name];
						}
						finally
						{
							((IBusinessObjectInternals)this).IsCopying = false;
						}
					}
				}
			}
		}

		public override ZString CS_ResponsiblePartyID
		{
			get
			{
				return base.CS_ResponsiblePartyID;
			}
			set
			{
				base.CS_ResponsiblePartyID = value.Replace(" ", "");
			}
		}

		public override ZString CS_MsgStatus
		{
			get
			{
				return base.CS_MsgStatus;
			}
			set
			{
				bool hasChanges = CS_MsgStatus != value;
				base.CS_MsgStatus = value;
				if (hasChanges && MAWB != null)
				{
					MAWB.MarkAsNeedingValidation();
					AddReportToCustomsPreFlightEventIfNecessary();
					AddShipmentMessagingEventIfRequired();
				}
			}
		}

		void AddReportToCustomsPreFlightEventIfNecessary()
		{
			if (MAWB != null)
			{
				switch (CS_MsgStatus)
				{
					case CMRBaseStatuses.Codes.OriginalAccepted:
						MAWB.ReportToCustomsPreFlight();
						break;
					case CMRBaseStatuses.Codes.AwaitingResponseToAmendment:
					case CMRBaseStatuses.Codes.AwaitingResponseToOriginal:
					case CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal:
						if (MAWB.IsReportedToCustomsPreFlight)
						{
							Logs.AddNew(Events.ReportedToCustomsPreFlight);
						}
						break;
				}
			}
		}

		[DecimalPlaces(3)]
		public override ZDecimal CS_Weight
		{
			get { return base.CS_Weight; }
			set { base.CS_Weight = value; }
		}

		[DecimalPlaces(2)]
		public override ZDecimal CS_GoodsValue
		{
			get { return base.CS_GoodsValue; }
			set { base.CS_GoodsValue = value; }
		}

		public override ZGuid CS_OA_ConsigneeAddress
		{
			get => base.CS_OA_ConsigneeAddress;
			set
			{
				var oldValue = CS_OA_ConsigneeAddress;
				base.CS_OA_ConsigneeAddress = value;
				if (!IsCopying && oldValue != CS_OA_ConsigneeAddress)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		public override ZGuid CS_OH_Consignee
		{
			get { return base.CS_OH_Consignee; }
			set
			{
				var oldValue = CS_OH_Consignee;
				base.CS_OH_Consignee = value;
				if (!IsCopying && oldValue != CS_OH_Consignee)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		public override ZString CS_RL_NKOrigin
		{
			get => base.CS_RL_NKOrigin;
			set
			{
				var oldValue = CS_RL_NKOrigin;
				base.CS_RL_NKOrigin = value;
				if (!IsCopying && oldValue != CS_RL_NKOrigin)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		public override ZString CS_RL_NKDestination
		{
			get => base.CS_RL_NKDestination;
			set
			{
				var oldValue = CS_RL_NKDestination;
				base.CS_RL_NKDestination = value;
				if (!IsCopying && oldValue != CS_RL_NKDestination)
				{
					ResetCustomBusinessObject();
				}
			}
		}

		#endregion

		#region Flags

		/// <summary>
		/// true == Database version of HAWB is different to the current value in memory
		/// </summary>
		public bool HouseBillNumberChanged
		{
			get { return (ZString)CS_HAWBInfo.OriginalValue != CS_HAWB; }
		}

		public bool Zero_Landed
		{
			get { return CS_CustomsStatus == AirCargoMessage.NewStatus.Zero_landed; }
		}

		public bool IsCargoStatusClear
		{
			get { return CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased == CS_CustomsStatus || CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement == CS_CustomsStatus; }
		}

		public bool IsMainStatusClear
		{
			get
			{
				return CS_CustomsMainStatus == "C000"
					|| CS_CustomsMainStatus == "C150"
					|| CS_CustomsMainStatus == "A100"
					|| CS_CustomsMainStatus == "A120"
					|| CS_CustomsMainStatus == "A140"
					|| CS_CustomsMainStatus == "P200"
					|| CS_CustomsMainStatus == "P300";
			}
		}

		#endregion

		#region Related BizObjects

		public new CusMAWB MAWB
		{
			get { return (CusMAWB)base.MAWB; }
		}

		public CusHAWB MasterHouseBill => Factory.GetValue(ref masterHouseBillCached, CalculateMasterHouseBill);

		CachedProperty<CusHAWB> masterHouseBillCached;

		CusHAWB CalculateMasterHouseBill()
		{
			CusHAWB result = null;
			if (!CS_MasterHouseBill.IsEmpty && CS_CM.IsValid)
			{
				ZQuery filter = new ZQuery(CusHAWBSchema.CS_HAWB, CS_MasterHouseBill);
				filter.AddToFilter(JoinCondition.And, CusHAWBSchema.CS_CM, SQLComparisonOperator.Equal, CS_CM);
				filter.AddToFilter(JoinCondition.And, CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, PK);
				result = Factory.LoadTop1<CusHAWB>(filter);
			}
			return result;
		}

		public override object ParentForCargoReportingEvents
		{
			get { return Shipment; }
		}

		public BusinessObject LandingToSendMessageOn
		{
			get { return fLandingToSendMessageOn; }
			set { fLandingToSendMessageOn = value; }
		}
		BusinessObject fLandingToSendMessageOn;

		public JobDeclaration Declaration
		{
			get { return Factory.Load<JobDeclaration>(CS_JE_CustomsFormalEntry); }
		}

		#endregion

		#region New Properties/Methods

		public bool InBondStore
		{
			get { return CS_IsHeldAtOutturn && CargoReceivedAtDepotLogs.Count > 0; }
		}

		protected bool CS_IsRemailReporter_ReadOnly
		{
			get { return IsHVLVOrRemailReadOnly; }
		}

		protected bool CS_IsSpecialReporter_ReadOnly
		{
			get { return IsHVLVOrRemailReadOnly; }
		}

		ZBool IsHVLVOrRemailReadOnly
		{
			get
			{
				return !this.CS_MsgStatus.IsEmpty && this.CS_MsgStatus != CMRBaseStatuses.Codes.NotSent
					&& this.CS_MsgStatus != CMRBaseStatuses.Codes.OriginalRejected && this.CS_MsgStatus != CMRBaseStatuses.Codes.WithdrawalAccepted;
			}
		}

		public ZString CS_MasterBillNum
		{
			get { return MAWB != null ? MAWB.CM_MAWB : ZString.Empty; }
		}

		public ZPropertyInfo CS_MasterBillNumInfo
		{
			get { return GetZPropertyInfo(CusHAWB.Schema.CS_MasterBillNum); }
		}

		public ZString CS_LoadPort
		{
			get { return MAWB != null ? MAWB.CM_RL_NKLoadPort : ZString.Empty; }
		}
		public ZPropertyInfo CS_LoadPortInfo
		{
			get { return GetZPropertyInfo(CusHAWB.Schema.CS_LoadPort); }
		}

		public ZString CS_DischargePort
		{
			get { return MAWB != null ? MAWB.CM_RL_NKDischargePort : ZString.Empty; }
		}
		public ZPropertyInfo CS_DischargePortInfo
		{
			get { return GetZPropertyInfo(CusHAWB.Schema.CS_DischargePort); }
		}

		public string MessageErrorsString
		{
			get
			{
				IEnumerable<INotification> messageErrors = new ZNotificationCollector(this, false, false).GetMessageErrors();
				return messageErrors.ToUniqueMessageListString();
			}
		}

		#region New Methods

		public void SetMessageReference()
		{
			if (!IsInDatabase && CS_MessageReference.IsEmpty)
			{
				MAWB?.BulkAllocateReferenceNumbersForChildBills();
				if (CS_MessageReference.IsEmpty)
				{
					SetMessageReferenceCore();
				}
			}
		}

		protected void SetMessageReferenceCore()
		{
			if (Shipment is ForwardingShipment shipment)
			{
				shipment.PopulateBillAndShipmentNumberIfNeeded();
				CS_MessageReference = shipment.JS_UniqueConsignRef.Left(CS_MessageReferenceInfo.MaxLength);
			}
			else
			{
				CS_MessageReference = Env.NumberFountains.AUAirCargoJobNumber.GetNextFormatted(Factory);
			}
		}

		void AttachAnyUnattachedHouseCARSTS()
		{
			if (ShouldAttachOrphanedCARSTs)
			{
				var orphanedMessages = Factory.Load<CMRCARSTMessage>(GetUnattachedHouseCARSTSQuery());
				if (orphanedMessages.Length > 0)
				{
					foreach (CMRCARSTMessage unattachedHouseCARST in orphanedMessages)
					{
						var currentParent = unattachedHouseCARST.EM_LinkedObject as CusMAWB;
						if (currentParent != null && unattachedHouseCARST.HAWB == CS_HAWB)
						{
							currentParent.Messages.Remove(unattachedHouseCARST);
							Messages.Add(unattachedHouseCARST);
						}
					}
					Calculator.DeriveStatusNow();
					CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
				}
			}
		}

		internal bool ShouldAttachOrphanedCARSTs
		{
			get { return !CS_IsHVLV && !IsInDatabase && !CS_MasterBillNum.IsEmpty && !CS_HAWB.IsEmpty && AUCustomsDataRegistry.Instance.AttachOrphanedCARSTsWhenCusHAWBCreated.Value; }
		}

		internal ZQuery GetUnattachedHouseCARSTSQuery()
		{
			var result = new CMRCARSTMessage.Loader(Factory).GetApplicationAndOwnerReferenceQuery(CS_MasterBillNum, CS_HAWB);
			result.AddToFilter(EDIMessageSchema.EM_LinkTable, CusMAWBSchema.Constants.TableName);
			result.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value));
#if DEBUG
			result.FetchOnlyFromLocalCache = IsGettingUnattachedHouseCARSTSQueryFromLocalCacheForTest;
#endif
			return result;
		}

#if DEBUG
		internal bool IsGettingUnattachedHouseCARSTSQueryFromLocalCacheForTest;
#endif

		#endregion

		public ZString Details
		{
			get
			{
				StringBuilder builder = new StringBuilder();

				if (MAWB != null)
				{
					builder.Append(MAWB.Details);
					builder.Append("\r\n");
				}

				builder.Append("HOUSE BILL DETAILS:\r\n");

				if (!CS_MessageReference.IsEmpty)
				{
					builder.Append("Message Reference: " + CS_MessageReference + "\r\n");
				}

				if (!CS_HAWB.IsEmpty)
				{
					builder.Append("HAWB: " + CS_HAWB + "\r\n");
				}

				if (Consignor != null)
				{
					builder.Append("Consignor: " + Consignor.OH_FullNameTruncated + "\r\n");
				}

				if (Consignee != null)
				{
					builder.Append("Consignee: " + Consignee.OH_FullNameTruncated + "\r\n");
				}

				if (Origin != null)
				{
					builder.Append("Origin: " + Origin.Code + "\r\n");
				}

				if (Destination != null)
				{
					builder.Append("Destination: " + Destination.Code + "\r\n");
				}

				return builder.ToString();
			}
		}

		#region HVLV properties

		public int RemainingDCLOutturnQuantity
		{
			get
			{
				if (!remainingDCLOutturnQuantity.HasValue)
				{
					int result = CS_PiecesManifested > 0 ? CS_PiecesManifested : 1;
					if (this.IsHVLVShipment())
					{
						result = 0;
					}
					else
					{
						if (MAWB != null && MAWB.UnderbondsForDCL.Length > 0)
						{
							int reportedPiecesNumber = 0;
							var outturns = Factory.Load<CusOutturn>(new ZQuery(CusOutturnSchema.C5_ParentID, PK));
							foreach (var outturn in outturns)
							{
								if (!outturn.C5_LastMessageDate.IsEmpty && MAWB.UnderbondsForDCL.Contains(outturn.Underbond))
								{
									if (outturn.C5_OutturnResultType == CMROutturnResultType.Codes.ShortLanded)
									{
										reportedPiecesNumber += outturn.C5_PackagesOutturned;
									}
									else
									{
										result = 0;
										break;
									}
								}
							}
							if (result > 0)
							{
								result -= reportedPiecesNumber;
								if (result < 1)
								{
									result = 1;
								}
							}
						}
					}
					remainingDCLOutturnQuantity = result;
				}
				return remainingDCLOutturnQuantity.Value;
			}
		}
		int? remainingDCLOutturnQuantity;

#if DEBUG
		public void ResetRemainingDCLOutturnQuantityCacheForTesting()
		{
			remainingDCLOutturnQuantity = null;
			MAWB.ResetDCLUnderbondCacheForTest();
		}
#endif

		#endregion

		#endregion

		#region Underbond Movement Logs

		protected LogsForNominatedEvent fRequestLogs;
		public LogsForNominatedEvent RequestLogs
		{
			get
			{
				if (fRequestLogs == null)
				{
					fRequestLogs = new LogsForNominatedEvent(Logs, Events.UnderbondRequest);
				}
				return fRequestLogs;
			}
		}

		protected LogsForNominatedEvent fAcquittalLogs;
		public LogsForNominatedEvent AcquittalLogs
		{
			get
			{
				if (fAcquittalLogs == null)
				{
					fAcquittalLogs = new LogsForNominatedEvent(Logs, Events.UnderbondAcquitReceivalReported);
				}
				return fAcquittalLogs;
			}
		}

		protected LogsForNominatedEvent fUnderbondCancelLogs;
		public LogsForNominatedEvent UnderbondCancelLogs
		{
			get
			{
				if (fUnderbondCancelLogs == null)
				{
					fUnderbondCancelLogs = new LogsForNominatedEvent(Logs, Events.UnderbondCancel);
				}
				return fUnderbondCancelLogs;
			}
		}

		protected LogsForNominatedEvent fUnderbondCustomsResponses;
		public LogsForNominatedEvent UnderbondCustomsResponses
		{
			get
			{
				if (fUnderbondCustomsResponses == null)
				{
					fUnderbondCustomsResponses = new LogsForNominatedEvent(Logs, Events.UnderbondCustomsApproval);
				}
				return fUnderbondCustomsResponses;
			}
		}

		#endregion

		#region Check-in Discrepancy

		public IMessageBuilder GetCargoReportBuilder()
		{
			AIRCRMessageBuilder builder = new AIRCRMessageBuilder(this);
			builder.MessageSubType = Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = Messages;
			return builder;
		}

		public StmALog LastCSINFOAmendmentLog
		{
			get
			{
				StmALog result = LastDiscrepancy;
				StmALog underbondLog = LastUnderbondLog;

				if (result == null || (underbondLog != null && result.SL_EventTime < underbondLog.SL_EventTime))
				{
					result = underbondLog;
				}
				return result;
			}
		}

		public ZString CurrentBondLocation
		{
			get
			{
				string result = "";
				StmALog underbondLogCached = LastUnderbondLog;
				if (underbondLogCached != null)
				{
					ZString[] description = underbondLogCached.SL_Reference.Split(':');
					if (description.Length == 3)
					{
						result = description[1];
					}
				}
				return result;
			}
		}

		public ZString DestinationBondLocation
		{
			get
			{
				string result = "";
				StmALog underbondLogCached = LastUnderbondLog;
				if (underbondLogCached != null)
				{
					ZString[] description = underbondLogCached.SL_Reference.Split(':');
					if (description.Length == 3)
					{
						result = description[2];
					}
				}
				return result;
			}
		}

		public StmALog LastUnderbondLog
		{
			get
			{
				StmALog result = RequestLogs.Count > 0 ? RequestLogs[0] : null;

				if (AcquittalLogs.Count > 0)
				{
					if (result == null || result.SL_EventTime < AcquittalLogs[0].SL_EventTime)
					{
						result = AcquittalLogs[0];
					}
				}

				if (UnderbondCancelLogs.Count > 0)
				{
					if (result == null || result.SL_EventTime < UnderbondCancelLogs[0].SL_EventTime)
					{
						result = UnderbondCancelLogs[0];
					}
				}
				return result;
			}
		}

		protected internal StmALog LastDiscrepancy
		{
			get
			{
				return Discrepancies.Count == 0 ? null : Discrepancies[0];
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		public bool UnderbondRequestBranchDiffersFromThisBranch
		{
			get
			{
				ZQuery filter = new ZQuery(EDIMessageSchema.EM_MessageSubType, MessageSubTypes.UnderbondRequest.Code);
				filter.AddToFilter(EDIMessageSchema.EM_GB, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
				EDIMessageCollection messages = new EDIMessageCollection(this, Factory);
				messages.Load(filter);
				return messages.Count > 0;
			}
		}

		protected LogsForNominatedEvent fDiscrepancies;
		protected internal LogsForNominatedEvent Discrepancies
		{
			get
			{
				if (fDiscrepancies == null)
				{
					fDiscrepancies = new LogsForNominatedEvent(Logs, Events.CargoCheckinDiscrepancy);
				}
				return fDiscrepancies;
			}
		}

		#endregion

		#region Synchronisation

		public void DefaultFromFreight(ForwardingShipment shipment)
		{
			bool shipmentChanges = shipment.HasChanges;
			CS_JS = shipment.PK;

			MAWB.SynchroniseData();
			SynchroniseData();

			HasChanges = false;
			MAWB.HasChanges = false;
			MAWB.HasMAWBChangesOnly = false;
			shipment.HasChanges = shipmentChanges;
		}

		public void SynchroniseData()
		{
			if (Shipment != null && !CS_IsResponsePending && (new HAWBToShipmentBridge(this).ShouldWeSynchronise))
			{
				CS_HAWB = FilterInvalidHouseBillCharacters(Shipment.JS_HouseBill);
				CS_FreightPrepaidCollect = new CMRUtilities().ConvertOldAirCargoPaymentType(Shipment.JS_PaymentTerm);
				CS_IsMasterHouse = Shipment.IsCoLoadMaster;

				var coLoadMasterShipment = Shipment.CoLoadMasterShipment;
				if (coLoadMasterShipment != null)
				{
					var masterHouse = CusHAWB.Load(coLoadMasterShipment);
					CS_CS_MasterHouseBill = masterHouse == null ? ZGuid.Empty : masterHouse.PK;
					CS_MasterHouseBill = FilterInvalidHouseBillCharacters(coLoadMasterShipment.JS_HouseBill);
				}
				CS_GoodsDescription = Shipment.FullGoodsDescription;
				CS_GoodsValue = Shipment.JS_GoodsValue;
				CS_RX_NKGoodsCurrency = Shipment.JS_RX_NKGoodsValueCurr;
				if (!CS_IsPrealerted)
				{
					CS_PiecesManifested = (short)Shipment.JS_OuterPacks;
				}

				var shipmentUnitOfWeight = Shipment.JS_UnitOfWeight;
				if (shipmentUnitOfWeight != Enterprise.Core.Constants.Weight.Kilograms && shipmentUnitOfWeight != Enterprise.Core.Constants.Weight.Pounds)
				{
					CS_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
					CS_Weight = Enterprise.Core.Constants.Weight.Convert(Shipment.JS_ActualWeight, shipmentUnitOfWeight, Enterprise.Core.Constants.Weight.Kilograms);
				}
				else
				{
					CS_WeightUQ = shipmentUnitOfWeight;
					CS_Weight = Shipment.JS_ActualWeight;
				}

				CopyConsigneeDetails();
				CopyConsignorDetails();

				CS_RL_NKDestination = Shipment.JS_RL_NKDestination;
				CS_RL_NKOrigin = Shipment.JS_RL_NKOrigin;
				CS_MessageReference = Shipment.JS_UniqueConsignRef.Left(CS_MessageReferenceInfo.MaxLength);
			}
		}

		ZString FilterInvalidHouseBillCharacters(ZString houseBillNumber)
		{
			return houseBillNumber.KeepChars(ZString.AlphanumericCharacters + "-/");
		}

		public BaseBusiness.IAddress ConsigneeAddressOnShipment
		{
			get
			{
				BaseBusiness.IAddress result = null;
				if (Shipment != null)
				{
					var defaultAddress = CargoHelper.GetDefaultConsigneeAddress(Shipment.ConsigneeDocumentaryAddress, Shipment.ConsigneeDeliveryAddress, true);
					if (defaultAddress != null)
					{
						result = BaseBusiness.JobDocAddressWrapper.New(defaultAddress);
					}
					else
					{
						result = null;
					}
				}
				return result;
			}
		}

		public BaseBusiness.IAddress ConsignorAddressOnShipment
		{
			get
			{
				BaseBusiness.IAddress result = null;
				if (Shipment != null)
				{
					var docAddress = Shipment.ConsignorDocumentaryAddress;
					var consignor = docAddress.E2_AddressOverride ? null : docAddress.Organisation;
					if (consignor != null)
					{
						result = new BaseBusiness.PortBasedOrgAddressDecider(consignor, BaseBusiness.CargoAddressType.Pickup, () => CS_RL_NKOrigin);
					}
					else
					{
						result = BaseBusiness.JobDocAddressWrapper.New(docAddress);
					}
				}
				return result;
			}
		}

		public void CopyConsigneeDetails()
		{
			if (Shipment != null)
			{
				SetConsigneeDetails(ConsigneeAddressOnShipment);
				JobDocAddress docAddress = null;
				var deliveryAddress = Shipment.ConsigneeDeliveryAddress;
				var consigneeAddress = Shipment.ConsigneeDocumentaryAddress;
				docAddress = CargoHelper.GetDefaultConsigneeAddress(consigneeAddress, deliveryAddress, true);

				if (docAddress != null)
				{
					CS_ConsigneeContactName = docAddress.E2_Contact.Left(CS_ConsigneeContactNameInfo.MaxLength);
				}
			}
		}

		public void CopyConsignorDetails()
		{
			if (Shipment != null)
			{
				SetConsignorDetails(ConsignorAddressOnShipment);
				CS_ConsignorContactName = Shipment.ConsignorDocumentaryAddress.E2_Contact.Left(CS_ConsignorContactNameInfo.MaxLength);
			}
		}

		#endregion

		#region Boolean Flag HasMessageChanges

		/// <summary>
		/// This flag is meaningful when there is an original message sent and Change Master /Sub-Master(80) amendment is to be sent.
		/// </summary>
		public bool IsMasterFlagOrMasterHouseBillDifferent
		{
			get { return IsMasterFlagDifferent || IsMasterHouseBillDifferent; }
		}

		internal bool IsMasterFlagDifferent
		{
			get
			{
				ZBool result = false;
				try
				{
					result = (ZBool)CS_IsMasterHouseInfo.OriginalValue != CS_IsMasterHouse;
				}
				catch (VersionNotFoundException) { }
				return result;
			}
		}

		internal bool IsMasterHouseBillDifferent
		{
			get
			{
				bool result = false;
				if (IsInDatabase && MAWB != null)
				{
					ZString oldValue = CS_MasterHouseBillInfo.OriginalValue.IsEmpty ? (ZString)MAWB.CM_MasterHouseBillInfo.OriginalValue : (ZString)CS_MasterHouseBillInfo.OriginalValue;
					ZString newValue = AggregatedCoLoadMaster;
					result = oldValue.ToUpper() != newValue.ToUpper();
				}
				return result;
			}
		}

		/// <summary>
		/// This flag exclues MAWB changes and is meaningful when there is an original message sent and Typo(50) amendment is to be sent
		/// Typo amendment messages always have Consignee & Consignor details as a fix to problems system cannot work out if Org details have changed
		/// </summary>
		public bool HasMessageChanges
		{
			get
			{
				return CS_IsPrealerted && HasChanges &&
					(IsWeightDifferent || IsPaymentMethodDifferent || IsMoneyValueDifferent || IsDescriptionDifferent
					|| IsPortDifferent(AirCargoMessage.LOCQualifier.Origin) || IsPortDifferent(AirCargoMessage.LOCQualifier.Destination)
					|| IsManifestedPiecesDifferent || IsLandedPiecesDifferent
					);
			}
		}

		public bool IsManifestedPiecesDifferent
		{
			get { return (ZShort)CS_PiecesManifestedInfo.OriginalValue != CS_PiecesManifested; }
		}

		public bool IsLandedPiecesDifferent
		{
			get { return (ZShort)CS_PiecesLandedInfo.OriginalValue != CS_PiecesLanded; }
		}

		public bool IsWeightDifferent
		{
			get
			{
				return ((ZDecimal)CS_WeightInfo.OriginalValue != CS_Weight
				|| ((ZString)CS_WeightUQInfo.OriginalValue).ToUpper() != CS_WeightUQ.ToUpper());
			}
		}

		public bool IsPaymentMethodDifferent
		{
			get { return (ZString)CS_FreightPrepaidCollectInfo.OriginalValue != CS_FreightPrepaidCollect; }
		}

		public bool IsMoneyValueDifferent
		{
			get
			{
				return (ZDecimal)CS_GoodsValueInfo.OriginalValue != CS_GoodsValue
					|| (ZString)CS_RX_NKGoodsCurrencyInfo.OriginalValue != CS_RX_NKGoodsCurrency;
			}
		}

		public bool IsDescriptionDifferent
		{
			get { return ((ZString)CS_GoodsDescriptionInfo.OriginalValue).ToUpper() != CS_GoodsDescription.ToUpper(); }
		}

		public bool IsPortDifferent(string qualifier)
		{
			bool result = false;
			if (qualifier == AirCargoMessage.LOCQualifier.Origin)
			{
				result = (ZString)CS_RL_NKOriginInfo.OriginalValue != CS_RL_NKOrigin;
			}
			else if (qualifier == AirCargoMessage.LOCQualifier.Destination)
			{
				result = (ZString)CS_RL_NKDestinationInfo.OriginalValue != CS_RL_NKDestination;
			}
			return result;
		}

		#endregion

		#region Messaging

		void LogIfCS_CustomsStatusChanged()
		{
			if ((ZString)CS_CustomsStatusInfo.OriginalValue != CS_CustomsStatus)
			{
				PerformActionInCorrectBranch(() =>
				{
					var logEntry = Logs.AddNew(Events.CustomsEntryStatus, CS_CustomsStatus);
					if (CS_CustomsStatus == CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased)
					{
						CS_ClearanceDate = logEntry.SL_EventTime;
					}
				});
			}
		}

		void PerformActionInCorrectBranch(Action action)
		{
			var branchPk = MAWB?.CM_GB ?? ZGuid.Empty;
			using (branchPk.IsValid && branchPk != GlbBranch.CurrentBranch.PK ? DisposableEnvironment.ForBranch(branchPk.ToGuid()) : null)
			{
				action.Invoke();
			}
		}

		protected override ZString GetReason()
		{
			return CargoHelper.GetACSAQISStatusReason(CMRCargoStatus.UserFriendlyStatuses);
		}

		protected override ZString GetHVLVStatusMapping(ZString status)
		{
			switch (status)
			{
				case CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased:
				case CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement:
					return CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Cleared;
				case CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl:
				case CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms:
				case CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn:
					return CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Held;
				case CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation:
				case CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine:
					return CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.GovernmentAgencyRequirements;
				case CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus:
				case CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement:
					return CargoWise.EventReference.Constants.EventReferenceTypeTypes.Codes.Transshipment;
				default:
					return ZString.Empty;
			}
		}

		public virtual ZString CS_MessageStatus
		{
			get
			{
				string result = CS_CustomsStatus;
				if (CS_IsResponsePending)
				{
					result = AirCargoMessage.WaitingDescription;
				}
				else if (result.ToUpper() == AirCargoMessage.NewStatus.NotSent)
				{
					result = MessageStatus.NotSent;
				}
				else
				{
					result = result + "\r\n" + result;
				}
				return result;
			}
		}
		public ZPropertyInfo CS_MessageStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CS_MessageStatus); }
		}

		public virtual ZString CS_MessageMainStatus
		{
			get
			{
				string result = CS_CustomsMainStatus;
				if (!CS_IsPrealerted && CS_IsResponsePending)//waiting for main manifest
				{
					result = AirCargoMessage.WaitingDescription;
				}
				else if (result.ToUpper() == AirCargoMessage.NewStatus.NotSent)
				{
					result = MessageStatus.NotSent;
				}
				else
				{
					result = result + "\r\n" + result;
				}
				return result;
			}
		}
		public ZPropertyInfo CS_MessageMainStatusInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.CS_MessageMainStatus); }
		}

		public bool NeedToSaveFirst
		{
			get
			{
				bool result = HasChanges;
				if (MAWB == null)
				{
					ErrorReporter.ReportOnce("MAWB is null in CusHAWB", "MAWB is null in CusHAWB and CS_CM = " + CS_CM);
				}
				else
				{
					result |= MAWB.HasMAWBChangesOnly;
				}
				return result;
			}
		}

		public bool NeedToClearMessageErrors
		{
			get
			{
				bool result = HasMessageErrors;
				if (MAWB == null)
				{
					ErrorReporter.ReportOnce("MAWB is null in CusHAWB", "MAWB is null in CusHAWB and CS_CM = " + CS_CM);
				}
				else
				{
					result |= MAWB.Notifications.HasMessageErrors();
				}
				return result;
			}
		}

		ZString ICMRControlMessageRespondee.UpdateStatusWhenControlMessageSyntaxError(EDIMessage incomingMessage, EDIMessage outgoingMessage)
		{
			ZString logText = ZString.Empty;
			ZString newStatus = ZString.Empty;
			if (outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Original)
			{
				newStatus = CMRBaseStatuses.Codes.OriginalRejected;
			}
			else if (outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Change ||
				outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment ||
				outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.ReplaceHeader)
			{
				newStatus = CMRBaseStatuses.Codes.AmendmentRejected;
			}
			else if (outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Withdraw)
			{
				newStatus = CMRBaseStatuses.Codes.WithdrawalRejected;
			}
			if (newStatus != ZString.Empty)
			{
				CS_MsgStatus = newStatus;
				logText = "HAWB to: " + newStatus;
			}
			return logText;
		}

		#endregion

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = "Air Cargo House";

				var parameters = new ZStringBuilder();
				parameters.AppendIfNotEmpty("HAWB: ", CS_HAWB);
				parameters.AppendIfNotEmpty("MHB: ", CS_MasterHouseBill);
				if (!parameters.IsEmpty)
				{
					result += " (" + parameters.ToStringWithDelimiterBetweenAppends(" ") + ")";
				}

				return result;
			}
		}

		protected bool IsLastMessagePartshipment
		{
			get
			{
				EDIMessage lastOutgoing = Messages.LastOutgoingMessage;
				return CS_IsPrealerted && lastOutgoing != null && lastOutgoing.EM_MessageSubType == AirCargoMessage.MessageSubType.PartShipment;
			}
		}

		public void CheckPreAlertConstraint()
		{
			if (CS_IsPrealerted)
			{
				throw new AirCargoException("This housebill has already been pre-alerted.");
			}
			if (CS_IsPrealertHeldByUser)
			{
				throw new AirCargoException("This house bill is pre-alert held. Please untick 'Is Pre-alert held?' and try again.");
			}
		}

		protected override BaseBusiness.CusHAWBValidation GetNewValidation()
		{
			return new CMRCusHAWBValidation(this);
		}

		#endregion

		#region IUnderbondMovementRequestHeaderProvider Members

		IUnderbondMovementRequestHeader IUnderbondMovementRequestHeaderProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusHAWBUnderbondMovementRequestHeader(underbond, this);
		}

		public bool IsBureau
		{
			get
			{
				bool result = false;
				if (MAWB != null)
				{
					result = MAWB.CM_IsBureau;
				}
				return result;
			}
		}

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		#region GetAllPossibleCollectionProviders

		protected override ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProvidersCore()
		{
			ArrayList result = new ArrayList();
			result.Add(this);
			result.AddRange(PartShips);
			return (ICusUnderbondDependentCollectionParent[])result.ToArray(typeof(ICusUnderbondDependentCollectionParent));
		}

		#endregion

		#endregion

		#region ICusUnderbondDependentCollectionParent

		#region UnderbondHumanReadableNameCore

		protected override ZString UnderbondHumanReadableNameCore
		{
			get { return "HouseBill" + (CS_HAWB.IsEmpty ? "" : (" " + CS_HAWB)); }
		}

		#endregion

		protected override ZString DetailsCore
		{
			get { return Details; }
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get
			{
				return (IOutturnableLine[])MAWB.ChildBills.ToArray(typeof(IOutturnableLine));
			}
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return true; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return CS_RL_NKDestination; }
		}

		#endregion

		#region ICusMAWBProvider

		CusMAWB ICusMAWBProvider.MAWB
		{
			get { return MAWB; }
		}

		#endregion

		#region IAirOutturnReportHeaderInformationProvider Members

		IAirOutturnReportHeaderInformation IAirOutturnReportHeaderInformationProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusHAWBOutturnReportHeaderInformation(this, underbond);
		}

		#endregion

		#region IProcessQueueParent Members

		public ActiveProcessQueueCollection ActiveProcessQueueForBinding
		{
			get { return ProcessQueueParentHelper.ActiveProcessQueueForBinding; }
		}

		public virtual ProcessQueue CurrentQueue
		{
			get { return ProcessQueueParentHelper.CurrentQueue; }
		}

		protected virtual Type TypeOfProcessQueue
		{
			get { return typeof(ProcessQueue); }
		}

		ProcessQueueParentHelper ProcessQueueParentHelper
		{
			get
			{
				if (fProcessQueueParentHelper == null)
				{
					fProcessQueueParentHelper = new ProcessQueueParentHelper(this, TypeOfProcessQueue);
				}
				return fProcessQueueParentHelper;
			}
		}

		ProcessQueueParentHelper fProcessQueueParentHelper;

		#endregion

		#region IUnderbondDefaultValueProvider Members

		void IUnderbondDefaultValueProvider.SetUnderbondDefaultValues(CusUnderbond underbond)
		{
			underbond.C4_PiecesManifested = CS_PiecesManifested;
			underbond.C4_FlightNo = MAWB.CM_FlightNo;
			underbond.C4_IsBureau = MAWB.CM_IsBureau;
		}

		#endregion

		#region IDetailsTabPageHeadingProvider Members

		string IDetailsTabPageHeadingProvider.Heading
		{
			get { return UnderbondHumanReadableName; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return DocManagerInfo; }
		}

		protected DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerIncludingRelatedObjectsInfo(this, Core.Constants.DocManagerCodes.AirCargoHouse);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IMessageManageableBizObj Members

		BaseBusiness.IMessageManager BaseBusiness.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusHAWBMessageManager(delegate
			{ return this; });
		}

		bool BaseBusiness.IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		BaseBusiness.ContinueWithDetection BaseBusiness.IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		#region IDataExportCSVFileNameProvider Members

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get { return CS_HAWB; }
		}

		#endregion

		#region IDocManagerSupportIncudingRelatedObjects Members

		BusinessObject IDocManagerSupportIncudingRelatedObjects.SelfReference
		{
			get { return this; }
		}

		IEnumerable<BusinessObject> IDocManagerSupportIncudingRelatedObjects.GetRelatedBusinessObjects()
		{
			foreach (var underbond in AllUnderbonds)
			{
				yield return underbond;
			}
		}

		#endregion

		#region HVLV support

		void AddShipmentMessagingEventIfRequired()
		{
			if (Shipment?.JS_ShipmentType == (ZString?)Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy)
			{
				switch (CS_MsgStatus)
				{
					case CMRBaseStatuses.Codes.AwaitingResponseToOriginal:
					case CMRBaseStatuses.Codes.AwaitingResponseToAmendment:
						new LogsForNominatedEvent(Shipment.GetLogs(), Events.HVLVReady).AddNew(ZString.Format("{0}|RES=Cargo Reporting", Shipment.JS_HouseBill));
						break;
				}
			}
		}

		LogsForNominatedEvent cargoReceivedAtDepotLogs;
		public LogsForNominatedEvent CargoReceivedAtDepotLogs
		{
			get { return cargoReceivedAtDepotLogs ?? (cargoReceivedAtDepotLogs = new LogsForNominatedEvent(Logs, Events.CargoReceivedAtDepot)); }
		}

		public bool ReleaseFromBondStore()
		{
			var result = CS_IsHeldAtOutturn && IsCargoStatusClear;
			if (result)
			{
				this.LogReadyForLocalDeliveryIfIsCargoStatusClear();
			}
			return result;
		}

		public LogsForNominatedEvent ReadyForLocalDeliveryLogs
		{
			get { return readyForLocalDeliveryLogs ?? (readyForLocalDeliveryLogs = new LogsForNominatedEvent(Logs, Events.ReadyForLocalDelivery)); }
		}
		LogsForNominatedEvent readyForLocalDeliveryLogs;

		public LogsForNominatedEvent CargoAvailableAtDepotLogs
		{
			get { return cargoAvailableAtDepotLogs ?? (cargoAvailableAtDepotLogs = new LogsForNominatedEvent(Logs, Events.CargoAvailableAtDeConsolidator)); }
		}
		LogsForNominatedEvent cargoAvailableAtDepotLogs;

		#endregion

		#region IWorkflowProvider Members

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		protected override ProcessTaskCollection GetNewCusHAWBProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>(this);
		}

		#endregion

		#region IWorkflowProviderCore Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, CS_OH_Consignee, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, CS_RL_NKOrigin, CS_RL_NKOrigin.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, CS_RL_NKDestination, CS_RL_NKDestination.Substring(0, 2), ZString.Empty);
			return result;
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get
			{
				IGlbCompany result = null;
				var mawb = MAWB;
				if (mawb != null)
				{
					var branch = mawb.Branch;
					result = branch == null ? null : branch.Company;
				}
				else
				{
					var shipment = Shipment;
					if (shipment == null)
					{
						var job = shipment.Job;
						result = job == null ? null : job.Company;
					}
				}
				return result ?? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			}
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var shipment = Shipment;
				if (shipment != null)
				{
					list.Add(shipment);
				}
				return list;
			}
		}

		#endregion

		#region IScanHouseBillProvider

		ZString IScanHouseBillProvider.ShipmentType
		{
			get { return Shipment != null ? Shipment.JS_ShipmentType : ZString.Empty; }
		}

		ZString IScanHouseBillProvider.HouseBill
		{
			get { return CS_HAWB; }
		}

		ZString IScanHouseBillProvider.ConsigneeName
		{
			get { return CS_ConsigneeName; }
		}

		ZString IScanHouseBillProvider.ConsignorName
		{
			get { return CS_ConsignorName; }
		}

		ZString IScanHouseBillProvider.GoodsDescription
		{
			get { return CS_GoodsDescription; }
		}

		IManifestInfo IScanHouseBillProvider.GetManifestInformation(CusUnderbond underbond)
		{
			return this;
		}

		internal ZBool IsSurplus { get; set; }

		ZBool IScanHouseBillProvider.ShouldScan(CusUnderbond underbond)
		{
			return RemainingDCLOutturnQuantity > 0;
		}

		void IScanHouseBillProvider.LogReadyForLocalDeliveryIfIsCargoStatusClear()
		{
			this.LogReadyForLocalDeliveryIfIsCargoStatusClear();
		}

		void IScanHouseBillProvider.ResetCargoReceivedAtDepotLogs(string reference)
		{
			this.ResetCargoReceivedAtDepotLogs(reference);
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return this;
		}

		#endregion

		#region IManifestInfo

		ZInt IManifestInfo.Quantity
		{
			get { return IsSurplus ? 0 : RemainingDCLOutturnQuantity; }
		}

		ZString IManifestInfo.UQ
		{
			get { return ZString.Empty; }
		}

		ZString IManifestInfo.GoodsDescription
		{
			get { return CS_GoodsDescription; }
		}

		ZString IManifestInfo.MarksAndNumbers
		{
			get { return ZString.Empty; }
		}

		ZString IManifestInfo.CustomsStatus
		{
			get { return CS_CustomsStatus; }
		}

		#endregion

		#region IControllerIDProvider

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.AU.HouseAirCargo; }
		}

		#endregion

		#region ICargoDepotEventParent

		bool ICargoDepotEventParent.IsHeldAtOutturn
		{
			get { return CS_IsHeldAtOutturn; }
			set { CS_IsHeldAtOutturn = value; }
		}

		#endregion

		#region Enterprise.Integration.Customs.AU.ICusHAWB

		ICodeDescriptionPairList Integration.Customs.AU.ICusHAWB.ConsolidatedCargoStatusesList => Factory.GetCachedValue<CMRConsolidatedCargoStatuses>();

		#endregion
	}
}
