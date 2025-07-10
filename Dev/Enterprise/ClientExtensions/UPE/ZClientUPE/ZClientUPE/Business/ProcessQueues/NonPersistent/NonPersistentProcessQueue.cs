using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public abstract class NonPersistentProcessQueue : NonPersistentBusinessObject, IActiveProcessQueue, IObsoleteValidation
	{
		public NonPersistentProcessQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateQueueName();
			ValidateStatus();
			ValidateSubStatus();
			ValidateTaskAssignedTo();
		}

		void ValidateQueueName()
		{
			QueueNameInfo.ClearAllNotifications();
			ValidationHelper.ValidateQueueName();
		}

		void ValidateStatus()
		{
			StatusInfo.ClearAllNotifications();
			ValidationHelper.ValidateReasonCode();
		}

		void ValidateSubStatus()
		{
			SubStatusInfo.ClearAllNotifications();
			ValidationHelper.ValidateStatusCode();
		}

		void ValidateTaskAssignedTo()
		{
			AssignedToInfo.ClearAllNotifications();
			ValidationHelper.ValidateTaskAssignedTo();
		}

		void ValidateReason()
		{
			ReasonInfo.ClearAllNotifications();
			ValidationHelper.ValidateRemarks();
		}

		#endregion

		#region IActiveProcessQueue Members

		#region QueueName

		[List("Lookups.QueueList")]
		public ZString QueueName
		{
			get { return fQueueName; }
			set
			{
				CheckMaximumLength(QueueNameInfo, value);
				SetNonPersistentPropertyValue(QueueNameInfo, ref	fQueueName, value);
				if (!IsValidationSuspended)
				{
					ValidateQueueName();
				}
			}
		}

		public ZPropertyInfo QueueNameInfo
		{
			get { return GetZPropertyInfo(nameof(QueueName)); }
		}

		public int QueueName_MaxLength
		{
			get { return QueueNameSchemaColumn.MaxLength; }
		}

		public MultilingualString QueueNameCaption
		{
			get { return (NoResString)"Queue"; }
		}

		public ZPropertyInfo QueueNameCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(QueueNameCaption)); }
		}

		protected abstract SchemaStringColumn QueueNameSchemaColumn { get; }

		ZString fQueueName;

		#endregion

		#region Status

		[List("Lookups.StatusList")]
		public ZString Status
		{
			get { return fStatus; }
			set
			{
				CheckMaximumLength(StatusInfo, value);
				SetNonPersistentPropertyValue(StatusInfo, ref fStatus, value);
				if (!IsValidationSuspended)
				{
					ValidateStatus();
				}
			}
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(nameof(Status)); }
		}

		public int Status_MaxLength
		{
			get { return StatusSchemaColumn.MaxLength; }
		}

		public MultilingualString StatusCaption
		{
			get { return (NoResString)"Reason"; }
		}

		public ZPropertyInfo StatusCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(StatusCaption)); }
		}

		protected abstract SchemaStringColumn StatusSchemaColumn { get; }

		ZString fStatus;

		#endregion

		#region SubStatus

		[List("Lookups.SubStatusList")]
		public ZString SubStatus
		{
			get { return fSubStatus; }
			set
			{
				CheckMaximumLength(SubStatusInfo, value);
				SetNonPersistentPropertyValue(SubStatusInfo, ref fSubStatus, value);
				if (!IsValidationSuspended)
				{
					ValidateSubStatus();
				}
			}
		}

		public ZPropertyInfo SubStatusInfo
		{
			get { return GetZPropertyInfo(nameof(SubStatus)); }
		}

		public int SubStatus_MaxLength
		{
			get { return SubStatusSchemaColumn.MaxLength; }
		}

		public MultilingualString SubStatusCaption
		{
			get { return (NoResString)"Status"; }
		}

		public ZPropertyInfo SubStatusCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(SubStatusCaption)); }
		}

		public bool HasSubStatuses
		{
			get { return Lookups.SubStatusList.Count > 0; }
		}

		protected abstract SchemaStringColumn SubStatusSchemaColumn { get; }

		ZString fSubStatus;

		#endregion

		#region Reason

		public ZString Reason
		{
			get { return fReason; }
			set
			{
				CheckMaximumLength(ReasonInfo, value);
				SetNonPersistentPropertyValue(ReasonInfo, ref fReason, value);
				if (!IsValidationSuspended)
				{
					ValidateReason();
				}
			}
		}

		public ZPropertyInfo ReasonInfo
		{
			get { return GetZPropertyInfo(nameof(Reason)); }
		}

		public int Reason_MaxLength
		{
			get { return ReasonSchemaColumn.MaxLength; }
		}

		public MultilingualString ReasonCaption
		{
			get { return (NoResString)"Remarks"; }
		}

		public ZPropertyInfo ReasonCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonCaption)); }
		}

		protected abstract SchemaStringColumn ReasonSchemaColumn { get; }

		ZString fReason;

		#endregion

		#region AssignedTo
		[List("Lookups.TaskAssignedToList")]
		public ZString AssignedTo
		{
			get { return fAssignedTo; }
			set
			{
				CheckMaximumLength(AssignedToInfo, value);
				SetNonPersistentPropertyValue(AssignedToInfo, ref	fAssignedTo, value);
				if (!IsValidationSuspended)
				{
					ValidateTaskAssignedTo();
				}
			}
		}

		public ZPropertyInfo AssignedToInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedTo)); }
		}

		public int AssignedTo_MaxLength
		{
			get { return AssignedToSchemaColumn.MaxLength; }
		}

		public MultilingualString AssignedToCaption
		{
			get { return (NoResString)"Assigned To"; }
		}

		public ZPropertyInfo AssignedToCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedToCaption)); }
		}

		protected abstract SchemaStringColumn AssignedToSchemaColumn { get; }

		ZString fAssignedTo;

		#endregion

		#region P4_CustomAttrib8

		[MaxLength(ProcessQueue.Schema.P4_CustomAttrib8MaxLength)]
		public ZString P4_CustomAttrib8
		{
			get { return fP4_CustomAttrib8; }
			set
			{
				CheckMaximumLength(P4_CustomAttrib8Info, value);
				SetNonPersistentPropertyValue(P4_CustomAttrib8Info, ref fP4_CustomAttrib8, value);
			}
		}

		public ZPropertyInfo P4_CustomAttrib8Info
		{
			get { return GetZPropertyInfo(nameof(P4_CustomAttrib8)); }
		}

		ZString fP4_CustomAttrib8;

		#endregion

		#region P4_CustomDate4

		public ZDateTime P4_CustomDate4
		{
			get { return fP4_CustomDate4; }
			set
			{
				SetNonPersistentPropertyValue(P4_CustomDate4Info, ref fP4_CustomDate4, value);
			}
		}

		public ZPropertyInfo P4_CustomDate4Info
		{
			get { return GetZPropertyInfo(nameof(P4_CustomDate4)); }
		}

		ZDateTime fP4_CustomDate4;

		#endregion

		public IActiveProcessQueueLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewNonPersistentProcessQueueLookups();
				}
				return fLookups;
			}
		}

		internal UPEProcessQueueValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = GetNewUPEProcessQueueValidationHelper();
				}
				return fValidationHelper;
			}
		}

		protected abstract NonPersistentProcessQueueLookups GetNewNonPersistentProcessQueueLookups();
		protected abstract UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper();

		IActiveProcessQueueLookups fLookups;
		UPEProcessQueueValidationHelper fValidationHelper;

		#endregion
	}
}
