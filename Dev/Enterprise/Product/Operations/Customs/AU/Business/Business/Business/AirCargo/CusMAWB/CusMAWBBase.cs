using System;
using System.Collections;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[SystemDefinedValues]
	public abstract class CusMAWBBase : Customs.Business.CusMAWB, ICusUnderbondParent, IConsignmentKeyChangeInhibitor, Integration.Customs.AU.ICusMAWBBase
	{
		public new class Schema : Customs.Business.CusMAWB.Schema
		{
			public const string CM_fUseAltPartShipModel = "CM_fUseAltPartShipModel";
		}

		public CusMAWBBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public static new readonly TypeDecider TypeDecider = new CusMAWBBaseTypeDecider();

		public override void OnLoaded()
		{
			if (CM_ApplicationCode != Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages)
			{
				ErrorReporter.ReportOnce("Attempting to load a non-AU CusMAWB as a AU CusMAWB",
					"Attempting to load a non-AU CusMAWB as a AU CusMAWB, CM_ApplicationCode: " + CM_ApplicationCode); // logging error message
			}

			base.OnLoaded();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CusMAWBBaseFetchStrategy(this);
		}

		public new class Loader : Customs.Business.CusMAWB.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public Loader(BusinessObjectFactory factory, CusMAWB parent)
				: base(factory, parent)
			{
			}

			public static ZString[] CMRApplicationCodes
			{
				get
				{
					return new ZString[] { Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages };
				}
			}

			protected override ZString[] GetApplicationCodes()
			{
				return CMRApplicationCodes;
			}
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			base.CM_HouseMessageIsSent = false;

			//Overriden setter should not be triggered. CusMAWB.HouseBills will be contructed and we dont want to do that
			//until this method is finished and let system have chance to set CM_JK.
			base.CM_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			base.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CM_fUseAltPartShipModel = IsAltPartShipModelActive;
		}

		#endregion

		#region Properties

		#region alternate part shipment

		public virtual ZBool IsAltPartShipModelActive
		{
			get { return false; }
		}

		public virtual ZBool CM_fUseAltPartShipModel
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.CM_fUseAltPartShipModel); }
			set
			{
				this.SetSystemDefinedValue(Schema.CM_fUseAltPartShipModel, value);
				CM_fUseAltPartShipModelInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCM_fUseAltPartShipModel();
				}
			}
		}

		public ZPropertyInfo CM_fUseAltPartShipModelInfo
		{
			get { return GetZPropertyInfo(Schema.CM_fUseAltPartShipModel); }
		}

		#endregion

		#region Stand Alone

		/// <summary>
		/// FreightForwarding AirCargo needs to know this as RegisterEditibleChild issue.
		/// User control for PlugIn to shipment has CusHAWB as Top BO and needs to register MAWB to be an editible child
		/// while Stand-alone MAWB form has CusMAWB as Top BO and needs to have all house bills as children. 
		/// </summary>
		public bool IsStandAlone
		{
			get { return IsStandAloneCore; }
		}

		protected virtual bool IsStandAloneCore
		{
			get { return false; }
		}

		#endregion

		#region CM_MAWB

		[BusinessObjectTestExclude]
		public override ZString CM_MAWB
		{
			get { return base.CM_MAWB; }
			set
			{
				base.CM_MAWB = value.Replace(" ", "").Replace("-", "").Left(CM_MAWBInfo.MaxLength);
			}
		}

		#endregion

		#region CM_ApplicationCode
		protected bool CM_ApplicationCode_ReadOnly
		{
			get { return true; }
		}
		#endregion

		#region New Properties

		public virtual ZString ShortDescription
		{
			get { return CM_MAWB.IsEmpty ? ZString.Empty : new ZString("MAWB: " + CM_MAWB); }
		}

		#endregion

		#region UnderbondStatus

		public ZString UnderbondStatus
		{
			get
			{
				ZString result = ZString.Empty;
				if (AllUnderbonds.Count > 1)
				{
					result = "More than one underbond";
				}
				else if (AllUnderbonds.Count == 1)
				{
					result = AllUnderbonds[0].UnderbondStatus.Description;
				}
				return result;
			}
		}

		public ZPropertyInfo UnderbondStatusInfo
		{
			get { return GetZPropertyInfo(nameof(UnderbondStatus)); }
		}

		#endregion

		public override ZString CM_ResponsiblePartyID
		{
			get { return base.CM_ResponsiblePartyID; }
			set
			{
				ZString originalValue = base.CM_ResponsiblePartyID;
				base.CM_ResponsiblePartyID = value;

				if (originalValue != value && ChildBillsIsLoaded)
				{
					ChildBills.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool CM_IsCTOMAWB
		{
			get
			{
				return base.CM_IsCTOMAWB;
			}
			set
			{
				bool isDiff = base.CM_IsCTOMAWB != value;
				base.CM_IsCTOMAWB = value;

				if (isDiff && ChildBillsIsLoaded)
				{
					ChildBills.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList bizORL = new ArrayList(base.BusinessObjectsWithRelatedEventsCore);
				bizORL.AddRange(PartShips.ToArray(PartShips.TypeOfElements));
				bizORL.AddRange(AllUnderbonds.ToArray(AllUnderbonds.TypeOfElements));
				return (BusinessObject[])bizORL.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Related Business Objects

		#region Messages

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		EDIMessageCollection fMessages;

		#endregion

		#region Part Shipments

		[ChildEditable(true)]
		public MAWBCusPartShipCollection PartShips
		{
			get
			{
				if (fPartShips == null)
				{
					fPartShips = new MAWBCusPartShipCollection(this);
					fPartShips.Load();
					RegisterEditableChildObject(fPartShips);
				}
				return fPartShips;
			}
		}

		MAWBCusPartShipCollection fPartShips;

		#endregion

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (allUnderbonds == null)
				{
					allUnderbonds = new CusUnderbondUnionCollection(this);
					allUnderbonds.Load();
				}
				return allUnderbonds;
			}
		}
		CusUnderbondUnionCollection allUnderbonds;

		ICusUnderbondDependentCollectionParent[] ICusUnderbondUnionCollectionParent.GetAllPossibleCollectionProviders()
		{
			return GetAllPossibleCollectionProvidersCore();
		}

		protected abstract ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProvidersCore();

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return true; }
		}

		#endregion

		#region IConsignmentKeyChangeInhibitor Members

		bool IConsignmentKeyChangeInhibitor.ShouldStopKeyFieldsChange
		{
			get { return ShouldStopKeyFieldsChange; }
		}

		bool ShouldStopKeyFieldsChange
		{
			get
			{
				if (shouldStopKeyFieldsChange == null)
				{
					shouldStopKeyFieldsChange = false;

					foreach (CusHAWBBase housebill in ChildBills)
					{
						if (!CMRStatusHelper.IsAcceptableStatusesForKeyValueChange(Factory, housebill.CS_MsgStatus))
						{
							shouldStopKeyFieldsChange = true;
							break;
						}
					}
				}
				return shouldStopKeyFieldsChange.Value;
			}
		}
		bool? shouldStopKeyFieldsChange;

		internal void RefreshShouldStopKeyFieldsChangeCalculation()
		{
			shouldStopKeyFieldsChange = null;
		}

		#endregion

		#region Scheduled messages and HVLV support

		public string ScheduledMessagesConfirmationText => CargoHelper.ScheduledMessagesConfirmationText(DeferredScheduledMessagesDateTimeString, "Original cargo reports");
		public ZString DeferredScheduledMessagesDateTimeString => CargoHelper.DeferredScheduledMessagesDateTimeString(DeferredScheduledMessagesDateTime);

		internal const string CanberraUNLOCO = "AUCBR";
		public const string AirCargoReportLogReference = "AIRCR";
		public const string AirCargoOutturnLogReference = "AIROUT";

		public ZDateTime DeferredScheduledMessagesDateTime => CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Air, CM_RL_NKDischargePort, CM_ArrivalDate);

		LogsForNominatedEvent AllDeferredScheduledMessageLogs
		{
			get { return allDeferredScheduledMessages ?? (allDeferredScheduledMessages = new LogsForNominatedEvent(this.GetLogs(), Events.DeferredScheduledMessage)); }
		}
		LogsForNominatedEvent allDeferredScheduledMessages;

		public bool HasDeferredScheduledMessageLog(string reference = "") => string.IsNullOrEmpty(reference) ? AllDeferredScheduledMessageLogs.Count > 0 : AllDeferredScheduledMessageLogs.Cast<StmALog>().Any(x => x.SL_Reference == reference);

		public ZString DeferredScheduledDateForDisplayInCanberraTime
		{
			get
			{
				return HasDeferredScheduledMessageLog(AirCargoReportLogReference) ? string.Format("Messaging deferred until {0}",
					EnvProxy.Instance.Time.GetUnlocoTimeFromUtc(CanberraUNLOCO, AllDeferredScheduledMessageLogs.Cast<StmALog>().First(x => x.SL_Reference == AirCargoReportLogReference).SL_EventTime.ToDateTime()).ToString("dd-MMM-yyyy HH:mm"))
					: string.Empty;
			}
		}

		public ZPropertyInfo DeferredScheduledDateForDisplayInCanberraTimeInfo
		{
			get { return GetZPropertyInfo(nameof(DeferredScheduledDateForDisplayInCanberraTime)); }
		}

		public void CancelDeferredScheduledMessageLogs(string reference = "")
		{
			if (string.IsNullOrEmpty(reference))
			{
				AllDeferredScheduledMessageLogs.CancelAll();
			}
			else
			{
				AllDeferredScheduledMessageLogs.Cast<StmALog>().Where(x => x.SL_Reference == reference).ToArray().ForEach(x => x.Cancel());
			}

			if (DeferredScheduledMessagesEventChanged != null)
			{
				DeferredScheduledMessagesEventChanged();
			}
		}

		public void AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(ZString triggerAction)
		{
			AddDeferredScheduledMessagesEvent(triggerAction);
			if (DeferredScheduledMessagesEventChanged != null)
			{
				DeferredScheduledMessagesEventChanged();
			}
		}

		protected override void AddDeferredScheduledMessagesEvent(ZString triggerAction)
		{
			ZString reference;
			switch (triggerAction)
			{
				case WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage:
					reference = AirCargoOutturnLogReference;
					break;
				case WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage:
					reference = AirCargoReportLogReference;
					break;
				default:
					return;
			}
			CancelDeferredScheduledMessageLogs(reference);
			var deferredScheduledMessagesDateTime = DeferredScheduledMessagesDateTime;
			var eventTime = deferredScheduledMessagesDateTime.IsEmpty ? ZDateTimeOffset.UtcNow : EnvProxy.Instance.Time.GetUtcFromUnlocoTime(CanberraUNLOCO, deferredScheduledMessagesDateTime.ToDateTime());
			var newLog = AllDeferredScheduledMessageLogs.AddNew(reference, eventTime);
			using (((IUpdateFieldsLock)newLog).LockForUpdatingKeyFields())
			{
				newLog.SL_IsEstimate = true;
			}
		}

		public event Action DeferredScheduledMessagesEventChanged;

		public override void OnSaving()
		{
			base.OnSaving();

			CheckMawbIsSingular();

			var deferredScheduledMessageTimeMayHaveChanged =
				(ZDateTime)CM_ArrivalDateInfo.OriginalValue != CM_ArrivalDate ||
				(ZString)CM_RL_NKDischargePortInfo.OriginalValue != CM_RL_NKDischargePort;
			if (deferredScheduledMessageTimeMayHaveChanged)
			{
				if (HasDeferredScheduledMessageLog(AirCargoReportLogReference))
				{
					AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
				}
				if (HasDeferredScheduledMessageLog(AirCargoOutturnLogReference))
				{
					AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage);
				}
			}
		}

		void CheckMawbIsSingular()
		{
			if (!IsInDatabase && CM_JK.IsValid && (Consol?.IsInDatabase ?? false))
			{
				var consoleQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
				consoleQuery.AddToFilter(JobConsolSchema.PK, Consol.PK);
				consoleQuery.TableHints |= TableHints.UPDLOCK;
				_ = Factory.Load<ForwardingConsol>(consoleQuery);

				// Check for an existing MAWB on this Consol
				var mawbQuery = new ZDBOnlyQuery(typeof(CusMAWBBase));
				mawbQuery.AddToFilter(CusMAWBSchema.CM_JK, Consol.PK);
				mawbQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
				mawbQuery.AddToFilter(CusMAWBSchema.PK, SQLComparisonOperator.NotEqual, PK);

				if (Factory.ExistsInDatabase(CusMAWB.Schema.TableName, mawbQuery))
				{
					var otherMawb = Factory.Load<CusMAWBBase>(mawbQuery).FirstOrDefault();
					if (otherMawb != null)
					{
						var message = ZString.Format("A Master Bill for this Consol has already been created (On saving).\r\nThis Mawb {0}\r\nOther Mawb {1}", GetDetailsForMasterBill(this), GetDetailsForMasterBill(otherMawb));  // Error Reporting
						throw new OdysseyException(message);
					}
				}
			}
		}

		string GetDetailsForMasterBill(CusMAWBBase mawb)
		{
			var messageBuilder = new ZStringBuilder();
			messageBuilder.Append($"PK: {mawb.PK}");
			messageBuilder.Append($"MAWB: '{mawb.CM_MAWB}'");
			messageBuilder.Append($"Consol: '{mawb.Consol?.JK_UniqueConsignRef}' ({mawb.CM_JK})");
			messageBuilder.Append($"Created: '{mawb.CM_SystemCreateTimeUtc.ToString("u")}' by '{mawb.CM_SystemCreateUser}'");

			return messageBuilder.ToStringWithNewLineBetweenAppends();
		}

		public ZGlobalMutex SendAIRCRMutex
		{
			get { return sendAIRCRMutex ?? (sendAIRCRMutex = new ZGlobalMutex(CusMAWBSendAIRCRMutex.Instance, PK.ToString())); }
		}
		ZGlobalMutex sendAIRCRMutex;

		public ZGlobalMutex SendAIROUTMutex
		{
			get { return sendAIROUTMutex ?? (sendAIROUTMutex = new ZGlobalMutex(MutexIDs.CusMAWBSendAIROUTMutex, PK.ToString())); }
		}
		ZGlobalMutex sendAIROUTMutex;

		#endregion

		public new CusMAWBValidation Validation
		{
			get { return (CusMAWBValidation)base.Validation; }
		}

		protected override Customs.Business.CusMAWBValidation GetNewValidation()
		{
			throw new NotImplementedException("GetNewValidation() must be overridden on all subclasses of Enterprise.Customs.AU.AirCargo.Business.CusMAWBBase");
		}

		public new CusMAWBLookups Lookups
		{
			get { return (CusMAWBLookups)base.Lookups; }
		}

		protected override Customs.Business.CusMAWBLookups GetNewLookups()
		{
			return new CusMAWBLookups(this);
		}
	}
}
