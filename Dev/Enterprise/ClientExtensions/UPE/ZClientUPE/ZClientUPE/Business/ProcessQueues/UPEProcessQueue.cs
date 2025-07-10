using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEProcessQueue : ProcessQueue
	{
		public UPEProcessQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract UPECusHAWB FirstUPECusHAWB { get; }
		public abstract UPECusHAWB[] GetFinalisedCusHAWBs();
		public abstract string ReferenceCode { get; }
		public abstract GlbBranch ParentBranch { get; }

		#region Property overrides

		protected override void SetDefaultForCustomsStatusAndCustomsSubStatus()
		{
			if (P4_CustomsQueue == CargoReportQueueCodeDescriptionPairList.Codes.Quarantine)
			{
				P4_CustomsStatus = ReasonCodeDescriptionPairList.AQUA.Codes.DI_DocInspect;
				P4_CustomsSubStatus = ZString.Empty;
			}
			else
			{
				base.SetDefaultForCustomsStatusAndCustomsSubStatus();
			}
		}

		public override ZDecimal P4_CustomDecimal1
		{
			get { return base.P4_CustomDecimal1; }
			set
			{
				base.P4_CustomDecimal1 = value;
				if (ParentBusinessObject != null)
				{
					ParentBusinessObject.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool P4_CustomFlag4
		{
			get { return base.P4_CustomFlag4; }
			set
			{
				base.P4_CustomFlag4 = value;
				if (ParentBusinessObject != null)
				{
					ParentBusinessObject.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool P4_CustomFlag5
		{
			get { return base.P4_CustomFlag5; }
			set
			{
				base.P4_CustomFlag5 = value;
				if (ParentBusinessObject != null)
				{
					ParentBusinessObject.MarkAsNeedingValidation();
				}
			}
		}

		protected override ProcessQueueLogCollection GetNewQueueLogs(ProcessQueueType.Enum queueType)
		{
			return new UPEProcessQueueLogCollection(this, queueType);
		}

		public override void OnSaving()
		{
			if (P4_CustomsStatusInfo.HasChanges && IsInspectIndicator)
			{
				CustomsQueueLogs.AddNew(ZString.Empty, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, ZString.Empty, ZString.Empty, P4_GS_NKCustomsTaskAssignedTo);
			}

			base.OnSaving();

			UPETools.Instance.PerformActionInCorrectBranch(ParentBranch, () => { SetQueuedDateIfQueueNameOrStatusesChanged(); });

			SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Observe);

			if (FirstUPECusHAWB != null)
			{
				FirstUPECusHAWB.OnCusHAWBOrDeclarationProcessQueueSaving();
			}
		}
		#endregion

		public ZBool IsInspectIndicator
		{
			get
			{
				return (P4_CustomsQueue == CargoReportQueueCodeDescriptionPairList.Codes.Quarantine
							&& P4_CustomsStatus == ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold)
							||
							(P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.BCA
							&& P4_CustomsStatus == ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold);
			}
		}

		#region Queued Dates

		#region CustomsQueuedDate

		public ZDateTime CustomsQueuedDate
		{
			get { return P4_CustomDate6; }
			set { P4_CustomDate6 = value; }
		}

		public ZPropertyInfo CustomsQueuedDateInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(CustomsQueuedDate), x => P4_CustomDate6Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region CommercialReleasedDate

		public static SchemaDateTimeColumn CommercialReleasedDateColumn
		{
			get { return ProcessQueueSchema.P4_CustomDate4; }
		}

		public ZDateTime CommercialReleasedDate
		{
			get { return P4_CustomDate4; }
			set { P4_CustomDate4 = value; }
		}

		public ZPropertyInfo CommercialReleasedDateInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(CommercialReleasedDate), x => P4_CustomDate4Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region CommercialQueuedDate

		public ZDateTime CommercialQueuedDate
		{
			get { return P4_CustomDate7; }
			set { P4_CustomDate7 = value; }
		}

		public ZPropertyInfo CommercialQueuedDateInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(CommercialQueuedDate), x => P4_CustomDate7Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#region CommercialQueuedByUser

		public ZString CommercialQueuedByUser
		{
			get { return P4_CustomAttrib5; }
			set { P4_CustomAttrib5 = value; }
		}

		public ZPropertyInfo CommercialQueuedByUserInfo
		{
			get
			{
				ZPropertyInfo result = GetWrappedZPropertyInfo(nameof(CommercialQueuedByUser), x => P4_CustomAttrib5Info);
				((IZPropertyInfoObsolete)result).ReadOnly = true;
				return result;
			}
		}

		#endregion

		#endregion

		#region Maintaining Queued Dates

		void SetQueuedDateIfQueueNameOrStatusesChanged()
		{
			if (CustomsQueueNameOrStatusesHasChanges)
			{
				CustomsQueuedDate = ZDateTime.Now;
			}

			if (CommercialQueueNameOrStatusesHasChanges)
			{
				CommercialQueuedByUser = GlbStaff.CurrentUser.GS_Code;
				CommercialQueuedDate = ZDateTime.Now;

				SetCommercialReleasedDate();
			}
		}

		protected virtual void SetCommercialReleasedDate()
		{
			bool wasInACompletedState = ((IList)CommercialQueueCodeDescriptionPairList.CompletedQueueNames).Contains((string)(ZString)P4_QueueNameInfo.OriginalValue);
			bool isInACompletedState = ((IList)CommercialQueueCodeDescriptionPairList.CompletedQueueNames).Contains((string)P4_QueueName);

			if (!isInACompletedState)
			{
				CommercialReleasedDate = ZDateTime.Empty;
			}
			else if ((!IsInDatabase || !wasInACompletedState) && isInACompletedState)
			{
				CommercialReleasedDate = ZDateTime.Now;
			}
		}

		bool CustomsQueueNameOrStatusesHasChanges
		{
			get
			{
				return
					(IsInDatabase && (P4_CustomsQueueInfo.HasChanges || P4_CustomsStatusInfo.HasChanges || P4_CustomsSubStatusInfo.HasChanges)) ||
					(!IsInDatabase && (!P4_CustomsQueue.IsEmpty || !P4_CustomsStatus.IsEmpty || !P4_CustomsSubStatus.IsEmpty));
			}
		}

		bool CommercialQueueNameOrStatusesHasChanges
		{
			get
			{
				return
					(IsInDatabase && (P4_QueueNameInfo.HasChanges || P4_StatusInfo.HasChanges || P4_SubStatusInfo.HasChanges)) ||
					(!IsInDatabase && (!P4_QueueName.IsEmpty || !P4_Status.IsEmpty || !P4_SubStatus.IsEmpty));
			}
		}

		#endregion

		#region ParentBusinessObject

		public EnterpriseBusinessObject ParentBusinessObject
		{
			get
			{
				if (fParentBusinessObject == null)
				{
					fParentBusinessObject = (Parent != null) ? Parent as EnterpriseBusinessObject : GetParentBusinessObjectFromParentID();
				}
				return fParentBusinessObject;
			}
		}

		public override IProcessQueueParent Parent
		{
			get { return base.Parent; }
			set
			{
				base.Parent = value;
				ResetParentBusinessObject();
			}
		}

		protected abstract Type ParentBusinessObjectType { get; }

		EnterpriseBusinessObject GetParentBusinessObjectFromParentID()
		{
			return (EnterpriseBusinessObject)Factory.Load(ParentBusinessObjectType, P4_ParentID);
		}

		void ResetParentBusinessObject()
		{
			fParentBusinessObject = null;
		}

		EnterpriseBusinessObject fParentBusinessObject;

		#endregion

		#region Queue Summary

		#region Customs

		public ZString CustomsQueueHeldOrCompleted
		{
			get { return HasCustomsQueueBeenCompleted ? QueueCompletedText : QueueHeldText; }
		}

		public ZPropertyInfo CustomsQueueHeldOrCompletedInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsQueueHeldOrCompleted)); }
		}

		public ZString CustomsQueueSummary
		{
			get { return GetQueueSummary((ZString)P4_CustomsQueueInfo.OriginalValue, (ZString)P4_CustomsStatusInfo.OriginalValue); }
		}

		public ZPropertyInfo CustomsQueueSummaryInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsQueueSummary)); }
		}

		public bool HasCustomsQueueBeenCompleted
		{
			get { return IsInDatabase && CompletedCustomsQueueNames.Contains((string)(ZString)P4_CustomsQueueInfo.OriginalValue); }
		}

		public bool IsCustomsQueueCompleted
		{
			get { return CompletedCustomsQueueNames.Contains((string)P4_CustomsQueue); }
		}

		IList CompletedCustomsQueueNames
		{
			get { return CustomsQueueCodeDescriptionPairList.CompletedQueueNames.ToList(); }
		}

		#endregion

		#region Commercial

		public ZString CommercialQueueHeldOrCompleted
		{
			get { return HasCommercialQueueBeenCompleted ? QueueCompletedText : QueueHeldText; }
		}

		public ZPropertyInfo CommercialQueueHeldOrCompletedInfo
		{
			get { return GetZPropertyInfo(nameof(CommercialQueueHeldOrCompleted)); }
		}

		public ZString CommercialQueueSummary
		{
			get { return GetQueueSummary((ZString)P4_QueueNameInfo.OriginalValue, (ZString)P4_StatusInfo.OriginalValue); }
		}

		public ZPropertyInfo CommercialQueueSummaryInfo
		{
			get { return GetZPropertyInfo(nameof(CommercialQueueSummary)); }
		}

		public bool HasCommercialQueueBeenCompleted
		{
			get { return IsInDatabase && CompletedCommercialQueueNames.Contains((string)(ZString)P4_QueueNameInfo.OriginalValue); }
		}

		public bool IsCommercialQueueCompleted
		{
			get { return CheckIfCommercialQueueIsCompleted((string)P4_QueueName); }
		}

		protected bool CheckIfCommercialQueueIsCompleted(string queue)
		{
			return CompletedCommercialQueueNames.Contains(queue);
		}

		IList CompletedCommercialQueueNames
		{
			get { return CommercialQueueCodeDescriptionPairList.CompletedQueueNames.ToList(); }
		}

		#endregion

		ZString GetQueueSummary(ZString queueName, ZString status)
		{
			ZString result = ZString.Empty;
			if (IsInDatabase)
			{
				result = queueName;
				if (!status.IsEmpty)
				{
					result += " / " + ReasonCodeList.GetDescriptionFromCode(status);
				}
			}
			return result;
		}

		ReasonCodeDescriptionPairList ReasonCodeList
		{
			get
			{
				if (fReasonCodeList == null)
				{
					fReasonCodeList = new ReasonCodeDescriptionPairList();
				}
				return fReasonCodeList;
			}
		}
		ReasonCodeDescriptionPairList fReasonCodeList;

		public const string QueueCompletedText = "COMPLETED";
		public const string QueueHeldText = "HELD";

		#endregion

		#region EIR Raised

		public ZString EIRRaisedLog
		{
			get { return fEIRRaisedLog; }
		}
		ZString fEIRRaisedLog;

		public override void OnLoaded()
		{
			base.OnLoaded();
			OriginalQueue = P4_CustomsQueue;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			RaiseEIRProcessing();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			fEIRRaisedLog = string.Empty;
		}

		protected virtual void RaiseEIRProcessing()
		{
			if (SubscribedToEIRRaisedProcessing && Parent.HasChanges)
			{
				if (OriginalQueue == CustomsQueueCodeDescriptionPairList.Codes.EIR)
				{
					if (P4_CustomsQueue == CustomsQueueCodeDescriptionPairList.Codes.EIR)
					{
						if (P4_CustomDate4.IsEmpty)
						{
							CancelEventArgs eventArgs = new CancelEventArgs();
							AskHasEIRBeenRaised(this, eventArgs);
							if (!eventArgs.Cancel)
							{
								P4_CustomDate4 = ZDateTime.Now;
								P4_CustomDate4Info.RefreshBinding();
								fEIRRaisedLog = "EIR Raised";
							}
							else
							{
								fEIRRaisedLog = "EIR Not Raised";
							}
						}
					}
					else
					{
						P4_CustomDate4 = ZDateTime.Empty;
						P4_CustomDate4Info.RefreshBinding();
					}
				}
			}
		}
		ZString OriginalQueue;
		public event CancelEventHandler AskHasEIRBeenRaised;

		internal bool SubscribedToEIRRaisedProcessing
		{
			get { return AskHasEIRBeenRaised != null; }
		}

		#endregion

		public override string ToString()
		{
			StringBuilder result = new StringBuilder(10);
			result.Append(" UPEProcessQueueType=" + UPEProcessQueueType);
			result.Append(" P4_QueueName=" + P4_QueueName);
			result.Append(" P4_Status=" + P4_Status);
			result.Append(" P4_SubStatus=" + P4_SubStatus);
			result.Append(" P4_Reason=" + P4_Reason);
			result.Append(" CustomsQueueHeldOrCompleted=" + CustomsQueueHeldOrCompleted);
			result.Append(" CommercialQueueHeldOrCompleted=" + CommercialQueueHeldOrCompleted);
			result.Append(" BISI Upload Date=" + P4_CustomDate1);
			result.Append(" BISI Download Date OR Customs Entry Date=" + P4_CustomDate2);
			result.Append(" Customs Queued Date=" + P4_CustomDate6);
			result.Append(" Commercial Queued Date=" + P4_CustomDate7);
			return result.ToString();
		}

		protected abstract string UPEProcessQueueType { get; }
	}
}
