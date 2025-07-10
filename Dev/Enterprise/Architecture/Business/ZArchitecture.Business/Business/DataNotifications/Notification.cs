using System;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public abstract class NotificationSubscriberNotification : Notification, INotificationSubscriberNotification
	{
		public NotificationSubscriberNotification(NotificationSubscriberType type, string additionalInfo)
			: base(type)
		{
			this.fAdditionalInfo = additionalInfo;
		}

		public string AdditionalInfo
		{
			get { return fAdditionalInfo; }
		}

		public new NotificationSubscriberType Type
		{
			get { return (NotificationSubscriberType)base.Type; }
		}

		protected string CachedDisplayMessage; // Used in BusinessObjectAfterSaveNotification
		public override string Message
		{
			get
			{
				if (CachedDisplayMessage != null)
				{
					return CachedDisplayMessage;
				}
				else
				{
					return DisplayMessageCore;
				}
			}
		}

		public bool ShouldBeDisplayedOnBatchProcessor
		{
			get { return ShouldBeDisplayedOnBatchProcessorCore; }
		}

		protected virtual bool ShouldBeDisplayedOnBatchProcessorCore
		{
			get { return false; }
		}

		protected virtual string DisplayMessageCore
		{
			get { return Type.GetDisplayMessage(AdditionalInfo); }
		}

		protected string CachedMultiLineDisplayMessage;
		public string MultiLineDisplayMessage
		{
			get { return CachedMultiLineDisplayMessage ?? MultiLineDisplayMessageCore; }
		}

		protected virtual bool AllowBlankDisplayMessage
		{
			get { return ((INotificationSubscriberNotification)this).AllowBlankDisplayMessage; }
		}

		bool INotificationSubscriberNotification.AllowBlankDisplayMessage
		{
			get { return false; }
		}

		protected virtual string MultiLineDisplayMessageCore
		{
			get { return Message; }
		}

		readonly string fAdditionalInfo;
	}
}
