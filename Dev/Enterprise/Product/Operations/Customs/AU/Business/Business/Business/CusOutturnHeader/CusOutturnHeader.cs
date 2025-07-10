using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CusOutturnHeaderSchema.Constants.C6_SendersMessageReference), DescriptionProperty(CusOutturnHeaderSchema.Constants.C6_SendersMessageReference)]
	public class CusOutturnHeader : Customs.Business.CusOutturnHeader,
		Integration.Customs.AU.ICusOutturnHeader,
		IEDIMessageCollectionProvider,
		ICMRMessageRespondee,
		IMessageManageableBizObj,
		IBackDoorSavingSupportableBizObj,
		ISeaOutturnReportHeaderInformation,
		ITriggerActionMessagingSupporter,
		ITriggerActionMessagingSupporterProvider,
		IValidateForCustomsMessagingSupporter,
		ITransitWarehouseSyncDataParent
	{
		public CusOutturnHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Calculator = new CusOutturnHeaderStatusCalculator(this);
			if (HasSplitMessageProcessingIssue)
			{
				Calculator.DeriveStatusNow();
			}
		}

		public readonly CusOutturnHeaderStatusCalculator Calculator;

		#region RescindMessageReceived

		public event EventHandler<RescindMessageEventArgs> RescindMessageReceived;

		protected virtual void OnRescindMessageReceived(RescindMessageEventArgs e)
		{
			EventHandler<RescindMessageEventArgs> eventMessageReceived = RescindMessageReceived;

			if (eventMessageReceived != null)
			{
				eventMessageReceived(this, e);
			}
		}

		public void ShowPopupRescindMessageReceived(string messageText)
		{
			RescindMessageEventArgs e = new RescindMessageEventArgs(messageText);

			OnRescindMessageReceived(e);
		}

		#endregion

		#region Static

		public static CusOutturnHeader New(BusinessObjectFactory factory)
		{
			return factory.New<CusOutturnHeader>();
		}

		#endregion

		#region Calculated Fields

		public bool HasSplitMessageOriginalRejectedLog
		{
			get { return CusUnderbondOutturnLogManager.HasSplitMessageOriginalRejectedLog; }
		}

		public bool HasSplitMessageFailedLog
		{
			get { return CusUnderbondOutturnLogManager.HasSplitMessageFailedLog; }
		}

		public bool HasNonExistantLineAtCustomsLog
		{
			get { return CusUnderbondOutturnLogManager.HasNonExistantLineAtCustoms; }
		}

		public bool HasSplitMessageProcessingIssue => HasSplitMessageOriginalRejectedLog || HasSplitMessageFailedLog || HasNonExistantLineAtCustomsLog;

		public CusUnderbondOutturnLogManager CusUnderbondOutturnLogManager
		{
			get
			{
				if (cusUnderbondOutturnLogManager == null)
				{
					cusUnderbondOutturnLogManager = new CusUnderbondOutturnLogManager(this);
				}
				return cusUnderbondOutturnLogManager;
			}
		}
		CusUnderbondOutturnLogManager cusUnderbondOutturnLogManager;

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("44D55404-60CE-49D6-B7C5-3440890A2435", "Sea Cargo Outturn {0}", C6_SendersMessageReference);

		#region Lookups

		public new CusOutturnHeaderLookups Lookups
		{
			get { return (CusOutturnHeaderLookups)base.Lookups; }
		}

		protected override Customs.Business.CusOutturnHeaderLookups GetNewLookups()
		{
			return new CusOutturnHeaderLookups(this);
		}

		#endregion

		#region Validation

		public new CusOutturnHeaderValidation Validation
		{
			get { return (CusOutturnHeaderValidation)base.Validation; }
		}

		protected override Customs.Business.CusOutturnHeaderValidation GetNewValidation()
		{
			return new CusOutturnHeaderValidation(this);
		}

		#endregion

		#region Outturns

		[ChildEditable(true)]
		public new CusOutturnHeaderDepotCusOutturnCollection Outturns
		{
			get { return (CusOutturnHeaderDepotCusOutturnCollection)base.Outturns; }
		}

		protected override CusOutturnHeaderCusOutturnCollection GetNewOutturns()
		{
			return new CusOutturnHeaderDepotCusOutturnCollection(this);
		}

		#endregion

		#region WorkflowItems
		protected override ProcessTaskCollection GetCusOutturnHeaderProcessTaskCollection()
		{
			return new CusOutturnHeaderProcessTaskCollection(this);
		}
		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (!string.IsNullOrEmpty(GlbCompany.CurrentCompany.OrgProxy?.PrimaryRegistrationNumber.Number))
			{
				C6_ResponsiblePartyID = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Left(C6_ResponsiblePartyIDInfo.MaxLength);
			}
		}

		public override void Delete()
		{
			Underbonds.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Properties

		#region C6_VesselName

		public override ZString C6_VesselName
		{
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.C6_VesselName))
				{
					base.C6_VesselName = value;
					var lloydsIMO = VesselName?.RV_LloydsNumber;

					if (lloydsIMO.HasValue && C6_LloydsIMO != lloydsIMO.Value)
					{
						C6_LloydsIMO = lloydsIMO.Value;
					}
				}
			}
			get { return base.C6_VesselName; }
		}

		#endregion

		#region C6_LloydsIMO

		public override ZString C6_LloydsIMO
		{
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.C6_LloydsIMO))
				{
					base.C6_LloydsIMO = value;

					if (!C6_LloydsIMO.IsEmpty)
					{
						var vessels = RefVessel.LookupVesselsByLloyds(value, Factory);
						if (vessels != null && vessels.Length == 1 && C6_VesselName != vessels[0].RV_Code)
						{
							C6_VesselName = vessels[0].RV_Code;
						}
					}
				}
			}
			get { return base.C6_LloydsIMO; }
		}

		#endregion

		#region C6_OA_OutturningPremise

		public override ZGuid C6_OA_OutturningPremise
		{
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.C6_OA_OutturningPremise))
				{
					base.C6_OA_OutturningPremise = value;
					var premiseID = OutturningPremise?.LocalControlledPremisesID;

					// HACK: Checking nullness before calling LocalControlledPremisesID shouldn't be needed. This should be fixed there, rather than here. This will cause the ModuleCanShowAndSearch test to fail for modules using this business object.
					if (premiseID.HasValue && !premiseID.Value.IsEmpty && C6_OutturningPremiseID != premiseID.Value)
					{
						C6_OutturningPremiseID = premiseID.Value;
					}
				}
			}
			get { return base.C6_OA_OutturningPremise; }
		}

		#endregion

		#region C6_OutturningPremiseID

		public override ZString C6_OutturningPremiseID
		{
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.C6_OutturningPremiseID))
				{
					base.C6_OutturningPremiseID = value;

					ZQuery filter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID);
					filter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					filter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, C6_OutturningPremiseID);
					filter.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, SQLComparisonOperator.NotEqual, null);
					var cusCode = Factory.LoadTop1<OrgCusCode>(filter);

					if (cusCode != null && cusCode.PremisesAddress != null)
					{
						C6_OA_OutturningPremise = cusCode.OK_OA_PremisesAddress;
					}
				}
			}
			get { return base.C6_OutturningPremiseID; }
		}

		protected bool C6_OutturningPremiseID_ReadOnly
		{
			get { return OutturningPremise != null && OutturningPremise.Header != null; }
		}

		#endregion

		#region C6_ResponsiblePartyID

		public override ZString C6_ResponsiblePartyID
		{
			get
			{
				return base.C6_ResponsiblePartyID;
			}
			set
			{
				base.C6_ResponsiblePartyID = value.Replace(" ", "");
			}
		}

		#endregion

		#endregion

		#region Collections

		public DepotCusUnderbondCusOutturnHeaderCollection Underbonds
		{
			get
			{
				if (underbonds == null)
				{
					underbonds = new DepotCusUnderbondCusOutturnHeaderCollection(this);
					underbonds.Load();
				}
				return underbonds;
			}
		}
		protected DepotCusUnderbondCusOutturnHeaderCollection underbonds;

		#region SEIMessageLines

		public Dictionary<ZString, CMRSEIMessageLine> SEIMessageLines
		{
			get
			{
				if (_sEIMessageLines == null)
				{
					_sEIMessageLines = new Dictionary<ZString, CMRSEIMessageLine>();
					foreach (ICMRDepotMessage message in Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CMR, new ZString[] { CMRMessage.CMRMessageTypes.SEI }, EDIMessage.Direction.Receive, true, ListSortDirection.Descending))
					{
						foreach (CMRSEIMessageLine line in message.Lines)
						{
							ZString key = line.ContainerNumber + "/" + line.OceanBillNumber + "/" + line.HouseBillNumber;
							if (!_sEIMessageLines.ContainsKey(key))
							{
								_sEIMessageLines.Add(key, line);
							}
						}
					}
				}
				return _sEIMessageLines;
			}
		}
		Dictionary<ZString, CMRSEIMessageLine> _sEIMessageLines;

		#endregion

#if DEBUG

		public void ResetCachedValuesForTesting()
		{
			_sEIMessageLines = null;
			fCustomsLines = null;
		}

#endif

		#endregion

		#region ICMRMessageRespondee Members

		public ZString Details
		{
			get
			{
				ZString result = ZString.Empty;
				RefVessel vessel = VesselName;
				result += "Vessel: " + (vessel != null ? vessel.RV_Code : ZString.Empty) + "\r\n";
				result += "Voyage: " + C6_VoyageNum + "\r\n";
				return result;
			}
		}

		public ZString ShortDescription
		{
			get { return "Voyage: " + C6_VoyageNum; }
		}

		#endregion

		#region OutturnStatus

		public MessageCusStatus OutturnStatus
		{
			get
			{
				if (fOutturnStatus == null)
				{
					fOutturnStatus = new MessageCusStatus(C6_MessageStatusInfo, Calculator);
				}
				return fOutturnStatus;
			}
		}
		MessageCusStatus fOutturnStatus;

		#endregion

		#region Nil Outturn

		public void NilOutturn()
		{
			foreach (DepotCusOutturn outturn in Outturns)
			{
				outturn.C5_PackagesOutturned = outturn.C5_OuterPacks;
				outturn.C5_PackagesUnits = outturn.C5_OuterPackUnits;
				outturn.C5_SealIntactIndicator = (outturn.IsFCL || outturn.IsFCX);
				outturn.C5_PillageIndicator = false;
				outturn.C5_DamageIndicator = false;
			}
		}

		#endregion

		#region IMessageManageableBizObj Members

		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusOutturnHeaderMessageManager(this);
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Enterprise.Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		internal SEAOUTMessageLineSentToCustoms[] CustomsLines
		{
			get { return fCustomsLines ?? (fCustomsLines = new SEAOUTMessageLineSentToCustomsBuilder(this).LineCollection); }
		}
		SEAOUTMessageLineSentToCustoms[] fCustomsLines;

		#region IBackDoorSavingSupportableBizObj

		AmendmentWithdrawalReason IBackDoorSavingSupportableBizObj.GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReason();
		}

		bool IBackDoorSavingSupportableBizObj.SupportBackDoorForSavingWhenAmendmentDetected
		{
			get { return true; }
		}

		#region ISeaOutturnReportHeaderInformation Members

		ZString ISeaOutturnReportHeaderInformation.VoyageNumber
		{
			get { return C6_VoyageNum; }
		}

		ZString ISeaOutturnReportHeaderInformation.VesselID
		{
			get { return C6_LloydsIMO; }
		}

		IEnumerable<ISeaOutturnReportLineInformation> ISeaOutturnReportHeaderInformation.Lines
		{
			get { return GetLines(this); }
		}

		IEnumerable<ISeaOutturnReportLineInformation> ISeaOutturnReportHeaderInformation.MessageLines
		{
			get
			{
				CusOutturnHeader outturnHeaderInNewFactory = new BusinessObjectFactory().Load<CusOutturnHeader>(PK);

				ArrayList result = new ArrayList();
				if (outturnHeaderInNewFactory != null)
				{
					foreach (CusOutturn outturn in outturnHeaderInNewFactory.Outturns)
					{
						if (!outturn.C5_LastMessageDate.IsEmpty && !outturn.C5_CargoReceiptDate.IsEmpty)
						{
							result.Add(new DepotCusOutturnOutturnReportLineInformation((DepotCusOutturn)outturn));
						}
					}
				}

				return (IEnumerable<ISeaOutturnReportLineInformation>)result.ToArray(typeof(ISeaOutturnReportLineInformation));
			}
		}

		IEDIMessageCollectionProvider ISeaOutturnReportHeaderInformation.MessagesProvider
		{
			get { return this; }
		}

		ZString IOutturnReportHeaderInformation.ResponsiblePartyID
		{
			get { return C6_ResponsiblePartyID; }
		}

		ZString IOutturnReportHeaderInformation.EstablishmentID
		{
			get { return C6_OutturningPremiseID; }
		}

		IEnumerable<ISeaOutturnReportLineInformation> GetLines(CusOutturnHeader outturnHeader)
		{
			ArrayList result = new ArrayList();
			if (outturnHeader != null)
			{
				foreach (CusOutturn outturn in outturnHeader.Outturns)
				{
					if (!outturn.C5_CargoReceiptDate.IsEmpty)
					{
						result.Add(new DepotCusOutturnOutturnReportLineInformation((DepotCusOutturn)outturn));
					}
				}
			}

			return (IEnumerable<ISeaOutturnReportLineInformation>)result.ToArray(typeof(ISeaOutturnReportLineInformation));
		}

		#endregion

		#endregion

		#region ITriggerActionMessagingSupporter
		void ITriggerActionMessagingSupporter.SendMessage(INotifications notifications, ZString queuedUserNK, ZString triggerAction)
		{
			AddDeferredScheduledMessagesEvent();
		}
		#endregion

		#region ITriggerActionMessagingSupporterProvider

		ITriggerActionMessagingSupporter ITriggerActionMessagingSupporterProvider.GetSupporter(string triggerAction)
		{
			return this;
		}

		#endregion

		#region ScheduledMessagesEvent

		LogsForNominatedEvent AllDeferredScheduledMessageLogs
		{
			get { return allDeferredScheduledMessages ?? (allDeferredScheduledMessages = new LogsForNominatedEvent(this.GetLogs(), Events.DeferredScheduledMessage)); }
		}
		LogsForNominatedEvent allDeferredScheduledMessages;

		public void AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent()
		{
			AddDeferredScheduledMessagesEvent();
		}

		protected void AddDeferredScheduledMessagesEvent()
		{
			CancelDeferredScheduledMessageLogs();
			var newLog = AllDeferredScheduledMessageLogs.AddNew("SEAOUT", ZDateTimeOffset.UtcNow);
			using (((IUpdateFieldsLock)newLog).LockForUpdatingKeyFields())
			{
				newLog.SL_IsEstimate = true;
			}
		}

		public void CancelDeferredScheduledMessageLogs()
		{
			AllDeferredScheduledMessageLogs.CancelAll();
		}
		#endregion

		#region IValidateForCustomsMessagingSupporter

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction) => this;

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging => true;

		#endregion

		public bool HasOutturnHeaderMessageErrors
		{
			get
			{
				IEnumerable<INotification> messageErrors = new ZNotificationCollector(this, false, false).GetMessageErrors();
				return messageErrors.HasNotifications();
			}
		}

		public string MessageErrorsString
		{
			get { return Notifications.GetMessageErrors().ToUniqueMessageListString(); }
		}

		public ZGlobalMutex SendSEAOUTMutex
		{
			get { return sendSEAOUTMutex ?? (sendSEAOUTMutex = new ZGlobalMutex(CusOutturnHeaderSendSEAOUTMutex.Instance, PK.ToString())); }
		}
		ZGlobalMutex sendSEAOUTMutex;

		public void ResetRejectedOutturnMessageStatus()
		{
			foreach (CusOutturn outturn in Outturns)
			{
				if (outturn.C5_MessageStatus == CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected)
				{
					outturn.C5_MessageStatus = ZString.Empty; // reset any previously received line rejections.
				}
			}
		}

		public IEDIMessageCollectionProvider MessagesProvider
		{
			get { return this; }
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public EDIMessageCollection SeaCargoOutturnMessages
		{
			get
			{
				if (seaCargoOutturnMessages == null)
				{
					seaCargoOutturnMessages = new EDIMessageCollection(this, new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CMR));
					seaCargoOutturnMessages.Load();
					seaCargoOutturnMessages.IsManagedForDataRefresh = true;
				}
				return seaCargoOutturnMessages;
			}
		}
		EDIMessageCollection seaCargoOutturnMessages;
	}
}
