#define CODE_ANALYSIS

using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public abstract class TransactionApprovalRequest<DetailsType> : GenApprovalRequest
		where DetailsType : ApprovalRequestDetails
	{
		public TransactionApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void PrepareFoSaving()
		{
			PrepareFoSavingCore();
		}

		public void UpdateApprovalUserAndStatus(ZString approvingUser, ZString status)
		{
			UpdateApprovalUserAndStatusCore(approvingUser, status);
		}

		public void FinalizeCancelling()
		{
			FinalizeCancellingCore();
		}

		protected void Initialize(ZGuid parentID, string parentTableCode)
		{
			XP_ParentID = parentID;
			XP_ParentTableCode = parentTableCode;
		}

		public virtual bool IsAllowedToChangeStatus(ZString status)
		{
			return true;
		}

		public virtual bool IsAllowedToApproveRequest => true;
		public virtual bool IsAllowedToCancelRequest => true;

		public bool IsPostingActionTheSame(TransactionApprovalRequest<DetailsType> request)
		{
			return IsPostingActionTheSameCore(request);
		}

		protected virtual bool IsPostingActionTheSameCore(TransactionApprovalRequest<DetailsType> request)
		{
			return PostingDetails.IsPostingActionTheSame(request.PostingDetails);
		}

		public bool ArePostingDetailsTheSame(TransactionApprovalRequest<DetailsType> request)
		{
			return ArePostingDetailsTheSameCore(request);
		}

		protected virtual bool ArePostingDetailsTheSameCore(TransactionApprovalRequest<DetailsType> request)
		{
			return PostingDetails == request.PostingDetails;
		}

		public virtual bool TransactionIsAlreadyPosted => false;

		#region ApprovingTransactionType

		public Type ApprovingTransactionType
		{
			get { return ApprovingTransactionTypeCore; }
		}

		protected abstract Type ApprovingTransactionTypeCore { get; }

		#endregion

		#region Properties

		#region ReferenceID

		public ZString ReferenceID
		{
			get { return ReferenceIDCore; }
		}

		protected abstract ZString ReferenceIDCore { get; }

		#endregion

		[ResourceStringData("ReferenceType", Caption = "Reference Type", ShortCaption = "Ref. Type")]
		public ZString ReferenceType => ReferenceTypeCore;

		protected virtual ZString ReferenceTypeCore
		{
			get
			{
				var type = "";
				switch (XP_ParentTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
						type = Res.GetString("4bc43c1e-fd3c-4c33-9efa-c6869ca428ef", "Job");
						break;
					case AccTransactionHeaderSchema.Constants.Prefix:
						type = Res.GetString("8ba911bf-cc63-4f59-bfc3-17be3b471a17", "Transaction");
						break;
					case GenApprovalRequestSchema.Constants.Prefix:
						type = Res.GetString("ed3cdfe4-7cc4-4828-aa6c-4942b209f6cd", "Approval Request");
						break;
					default:
						type = Res.GetString("2cfd7587-d057-4f4b-b98b-79e5bd29f7d8", "Consol");
						break;
				}

				return type;
			}
		}

		#region PostingDetails

		public DetailsType PostingDetails
		{
			get
			{
				if (postingDetails == null)
				{
					postingDetails = CreatePostingApprovalDetails();
					ReadPostingDetails();
					RegisterEditableChildObject(postingDetails);
				}

				return postingDetails;
			}
		}
		DetailsType postingDetails;

#if DEBUG
		public void ResetPostingDetails_ForTestOnly()
		{
			if (postingDetails != null)
			{
				UnRegisterEditableChildObject(postingDetails);
			}

			postingDetails = null;
		}
#endif

		protected IDisposable CreateTemporaryPostingDetails()
		{
			if (IsPostingDetailsCreated)
			{
				throw new InvalidOperationException("CreateJournalCopyInPostingDetails can't be called for loaded PostingDetails");
			}
			return new DisposableAction(
				() => postingDetails = CreatePostingApprovalDetails(),
				() => postingDetails = null);
		}

		bool IsPostingDetailsCreated { get { return postingDetails != null; } }

		protected abstract DetailsType CreatePostingApprovalDetails();

		#endregion

		[ReadOnly(true)]
		public override ZGuid XP_GB_JobBranch
		{
			get => base.XP_GB_JobBranch;
			set => base.XP_GB_JobBranch = value;
		}

		[ReadOnly(true)]
		public override ZGuid XP_GE_JobDepartment
		{
			get => base.XP_GE_JobDepartment;
			set => base.XP_GE_JobDepartment = value;
		}

		[ReadOnly(true)]
		public override ZString XP_RequestID
		{
			get { return base.XP_RequestID; }
			set { base.XP_RequestID = value; }
		}

		[ReadOnly(true)]
		public override ZGuid XP_GB_RequestingBranch
		{
			get { return base.XP_GB_RequestingBranch; }
			set { base.XP_GB_RequestingBranch = value; }
		}

		[ReadOnly(true)]
		public override ZGuid XP_ParentID
		{
			get { return base.XP_ParentID; }
			set { base.XP_ParentID = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_ParentTableCode
		{
			get { return base.XP_ParentTableCode; }
			set { base.XP_ParentTableCode = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_ApprovalType
		{
			get { return base.XP_ApprovalType; }
			set { base.XP_ApprovalType = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_SubSystem
		{
			get { return base.XP_SubSystem; }
			set { base.XP_SubSystem = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime XP_ApprovalDate
		{
			get { return base.XP_ApprovalDate; }
			set { base.XP_ApprovalDate = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_ApprovalStatus
		{
			get { return base.XP_ApprovalStatus; }
			set { base.XP_ApprovalStatus = value; }
		}

		public ZString XP_ReasonCodeDescription
		{
			get
			{
				return GenApprovalRequestLookups.ReasonCodeDescriptionList(this).GetDescriptionFromCode(XP_ReasonCode);
			}
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser1
		{
			get { return base.XP_GS_NKApprovingUser1; }
			set { base.XP_GS_NKApprovingUser1 = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser2
		{
			get { return base.XP_GS_NKApprovingUser2; }
			set { base.XP_GS_NKApprovingUser2 = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser3
		{
			get { return base.XP_GS_NKApprovingUser3; }
			set { base.XP_GS_NKApprovingUser3 = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser4
		{
			get { return base.XP_GS_NKApprovingUser4; }
			set { base.XP_GS_NKApprovingUser4 = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser5
		{
			get { return base.XP_GS_NKApprovingUser5; }
			set { base.XP_GS_NKApprovingUser5 = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_GS_NKApprovingUser6
		{
			get { return base.XP_GS_NKApprovingUser6; }
			set { base.XP_GS_NKApprovingUser6 = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime XP_SystemCreateTimeUtc
		{
			get { return base.XP_SystemCreateTimeUtc; }
			set { base.XP_SystemCreateTimeUtc = value; }
		}

		[ReadOnly(true)]
		public override ZString XP_SystemCreateUser
		{
			get { return base.XP_SystemCreateUser; }
			set { base.XP_SystemCreateUser = value; }
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			XP_GB_RequestingBranch = GlbBranch.CurrentBranch.PK;
			XP_SubSystem = Constants.GenApprovalRequestSubSystem.Accounting;
			XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			WritePostingDetails();
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			ReadPostingDetails();
		}

		protected override GenApprovalRequestValidation GetNewValidation()
		{
			return new TransactionApprovalRequestValidation(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			shouldEmailBeSent = ShouldEmailBeSent;

			if (!Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateXP_RequestID))
			{
				if (!IsInDatabase)
				{
					if (SetXP_RequestIDFromNumberFoutain())
					{
						FactoryCountUsedToGenerateXP_RequestID = Factory.SaveCount;
					}
				}
			}
		}

		protected virtual ZBool SetXP_RequestIDFromNumberFoutain()
		{
			ZBool result = ZBool.False;
			if (NumberFountainForRequestID != null)
			{
				XP_RequestID = NumberFountainForRequestID.GetNextFormatted(Factory);
				result = ZBool.True;
			}
			return result;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && shouldEmailBeSent)
			{
				shouldEmailBeSent = false;
				CreateEmail().Send();
			}

			this.RemoveContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction);
			this.RemoveContext(BusinessContext.CancelApprovalRequestByUser);
			this.RemoveContext(BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted);
		}

		protected abstract TransactionApprovalRequestEmail CreateEmail();

		#endregion

		#region Implementation

		protected virtual void PrepareFoSavingCore() { }

		protected virtual void FinalizeCancellingCore() { }

		protected virtual void UpdateApprovalUserAndStatusCore(ZString approvingUser, ZString status)
		{
			XP_GS_NKApprovingUser1 = approvingUser;
			XP_ApprovalStatus = status;
			XP_ApprovalDate = ZDateTime.Now;
		}

		int FactoryCountUsedToGenerateXP_RequestID
		{
			get
			{
				return AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(this, FactoryCountUsedToGenerateXP_RequestIDName);
			}
			set
			{
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(this, FactoryCountUsedToGenerateXP_RequestIDName, value);
			}
		}
		const string FactoryCountUsedToGenerateXP_RequestIDName = "FactoryCountUsedToGenerateXP_RequestIDName";

		protected virtual INumberFountainProxy NumberFountainForRequestID
		{
			get { return null; }
		}

		protected virtual bool ShouldEmailBeSent
		{
			get
			{
				return (ZString)XP_ApprovalStatusInfo.OriginalValue == Constants.GenApprovalRequestApprovalStatus.Requested &&
								(XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved ||
								XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Rejected);
			}
		}

		protected void ReadPostingDetails(bool doAlways = false)
		{
			if (IsPostingDetailsCreated && (doAlways || !PostingDetails.HasChanges))
			{
				using (PostingDetails.SuspendSettingHasChangesIncludingChildren())
				{
					if (XP_ApprovalRequestData != ZBlob.Empty)
					{
						ReadXMLFromBlobAndDeserialize(XP_ApprovalRequestData, reader =>
							{
								var serialiser = ZXmlSerializer.New(typeof(DetailsType));
								PostingDetails.CopyFrom((ApprovalRequestDetails)serialiser.Deserialize(reader));
							});
					}
					else
					{
						PostingDetails.CopyFrom(CreatePostingApprovalDetails());
					}
					PostingDetails.SetReadOnlyIncludingChildren(true);
					OnAfterReadPostingDetails();
				}
			}
		}

		protected void ReadXMLFromBlobAndDeserialize(ZBlob vaule, Action<XmlTextReader> deserializeFromReader)
		{
			if (vaule != ZBlob.Empty)
			{
				using (MemoryStream stream = new MemoryStream(vaule))
				using (XmlTextReader reader = new XmlTextReader(stream))
				{
					deserializeFromReader(reader);
#if DEBUG
					if (Globals.IsTest)
					{
						ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly++;
					}
#endif
				}
			}
		}

#if DEBUG

		[ThreadStatic]
		[SuppressMessage("Microsoft.Design", "CA1000: Do not declare static members on generic types", Justification = "For test only to count calls despite of instances and when an instance out of reach.")]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only to count calls despite of instances and when an instance out of reach.")]
		public static int ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly;

#endif

		protected virtual void OnAfterReadPostingDetails() { }

		protected void WritePostingDetails(bool doAlways = false)
		{
			if (doAlways || (IsPostingDetailsCreated && PostingDetails.HasChanges))
			{
				using (MemoryStream stream = new MemoryStream())
				using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Unicode))
				{
					var serialiser = ZXmlSerializer.New(typeof(DetailsType));
					serialiser.Serialize(writer, PostingDetails);
					writer.Flush();
					XP_ApprovalRequestData = stream.ToArray();
				}
			}
		}

		public void SerializePostingDetails()
		{
			WritePostingDetails(true);
		}

		bool shouldEmailBeSent;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			XP_ParentID = ZGuid.NewZGuid();
		}
#endif
		#endregion
	}
}
