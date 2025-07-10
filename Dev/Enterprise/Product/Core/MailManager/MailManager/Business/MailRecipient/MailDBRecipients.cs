using System.Data;

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MailManager.Business
{
	public interface IMailRecipient
	{
		ZString EmailAddress { get; }
		ZByte MR_AckAttempt { get; set; }
		void Delete();
		void Reload();
		ZDateTime MR_LastAttempt { get; set; }
		ZDateTime MR_DeliveredTime { get; set; }
		ZString MR_RecipientType { get; set; }
		ZGuid MR_MI { get; set; }
		ZGuid PK { get; }
		bool IsSuspended { get; }
		bool IsDeleted { get; }
	}

	public class MailRecipient : AutoMailDBRecipients, IMailRecipient
	{
		public enum RecipientTypes
		{
			TO, CC, BCC
		}
		public MailRecipient(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public MailItem MailItem
		{
			get
			{
				if (fMailItem == null)
				{
					fMailItem = Factory.Load<MailItem>(MR_MI);
				}
				return fMailItem;
			}
		}

		public bool IsDelivered
		{
			get
			{
				return !(this.MR_DeliveredTime.IsEmpty);
			}
		}

		public bool IsWaitingForAcknowledgement
		{
			get
			{
				return ((MailItem.MI_Status == MailStatus.QueuedWithAck) && !MR_LastAttempt.IsEmpty &&
						(MR_LastAttempt > ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout)));
			}
		}

		public bool IsFailed
		{
			get
			{
				return (!IsDelivered && (MR_AckAttempt >= MailAcknowledgement.MaxAttempts) && !IsWaitingForAcknowledgement);
			}
		}

		public bool IsSuspended
		{
			get
			{
				return ((MailItem.MI_Status != MailStatus.Queued) && (IsDelivered || IsFailed || IsWaitingForAcknowledgement));
			}
		}

		public void CopyValuesFrom(MailRecipient sourceRecipient)
		{
			base.CopyValuesFrom(sourceRecipient);
		}

		#region Calculated Local Time Properties

		public ZDateTime CalcDeliveredTimeLocal
		{
			get
			{
				return (MR_DeliveredTime.IsValid) ?
					Enterprise.Environment.Env.Time.GetLocalTimeFromUtc(MR_DeliveredTime.ToDateTime()) :
					ZDateTime.Empty;
			}
		}

		public ZPropertyInfo CalcReceivedDateTimeLocalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CalcDeliveredTimeLocal)); }
		}

		#endregion

		#region Implementation
		MailItem fMailItem;

		#endregion

		#region IMailRecipient Members

		public ZString EmailAddress
		{
			get { return MR_RecipientMailAddress; }
		}

		#endregion
	}

	#region TestCase

#if DEBUG

	[TestExcludeBusinessObjectsAllHaveTestCases]
	class DummyMailItem : MailItem
	{
		public DummyMailItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public MailRecipientCollection MailRecipientsCore_Exposed
		{
			get
			{
				return this.MailRecipientsCore;
			}
		}
	}

#endif

	#endregion
}
