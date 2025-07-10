using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = MailManager.Res;

namespace Enterprise.MailManager.Business
{
	public class MailRecipientReadonlyCollection : ICollection
	{
		public MailRecipientReadonlyCollection(MailRecipientCollection masterCollection)
		{
			inner = masterCollection;
		}

		readonly MailRecipientCollection inner;

		#region Properties

		public IMailRecipient this[int index]
		{
			get { return inner[index]; }
		}

		public bool ContainsRecipientWithEmail(ZString email)
		{
			foreach (MailRecipient recipient in this)
			{
				if (recipient.MR_RecipientMailAddress.ExcludeChars("<>").EqualsIgnoringCase(email))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasRecipientsWithSendAttemptsRemaining
		{
			get { return inner.HasRecipientsWithSendAttemptsRemaining; }
		}

		public bool ContainsFailedRecipients
		{
			get { return inner.ContainsFailedRecipients; }
		}

		public bool AllRecipientsSuccessfullyDelivered
		{
			get { return inner.AllRecipientsSuccessfullyDelivered; }
		}

		public bool ContainsWaitingForACKRecipients
		{
			get { return inner.ContainsWaitingForACKRecipients; }
		}

		#endregion

		#region wrapper properties

		public void RemoveAndDeleteAll()
		{
			inner.RemoveAndDeleteAll();
		}

		public bool Contains(ZGuid pk)
		{
			return inner.Contains(pk);
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			((ICollection)inner).CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return inner.Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return ((ICollection)inner).IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return ((ICollection)inner).SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return ((ICollection)inner).GetEnumerator();
		}

		#endregion
	}

	[Serializable]
	public class InvalidMailFormatException : Exception
	{
		public InvalidMailFormatException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected InvalidMailFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
		{ }
#endif
	}

	public class MailRecipientCollection : DependentBusinessObjectCollection<MailRecipient, MailItem>
	{
		public MailRecipientCollection(MailItem master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public bool HasRecipientsWithSendAttemptsRemaining
		{
			get
			{
				foreach (MailRecipient recipient in this)
				{
					if (recipient.MR_AckAttempt > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public MailRecipient AddNew(string emailAddress, MailRecipient.RecipientTypes recipientType)
		{
			if (AllowNewCore)
			{
				var result = AddNew();

				if (emailAddress.Length > result.MR_RecipientMailAddressInfo.MaxLength)
				{
					throw new InvalidMailFormatException(Res.GetString("4ff352c7-b478-4a2f-a164-3b3ec4901d3f", "The email address [{0}] exceeds the maximum allowable of {1}.", emailAddress, result.MR_RecipientMailAddressInfo.MaxLength));
				}

				result.MR_RecipientMailAddress = emailAddress;
				result.MR_RecipientType = recipientType.ToString();

				return result;
			}
			return null;
		}

		public bool ContainsRecipientWithEmail(ZString email)
		{
			foreach (MailRecipient recipient in this)
			{
				if (recipient.MR_RecipientMailAddress.EqualsIgnoringCase(email))
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsFailedRecipients
		{
			get
			{
				foreach (MailRecipient recipient in this)
				{
					if (recipient.IsFailed)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AllRecipientsSuccessfullyDelivered
		{
			get
			{
				foreach (MailRecipient recipient in this)
				{
					if (!recipient.IsDelivered)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool ContainsWaitingForACKRecipients
		{
			get
			{
				foreach (MailRecipient recipient in this)
				{
					if (recipient.IsWaitingForAcknowledgement)
					{
						return true;
					}
				}
				return false;
			}
		}
	}
}
