using System;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class NotificationSubscriberType : INotificationSubscriberType
	{
		protected NotificationSubscriberType(string name, string message)
		{
			this.name = name;
			this.message = message;
		}

		protected NotificationSubscriberType(string message)
			: this(message, message)
		{
		}

		public string Name
		{
			get { return name; }
		}
		readonly string name;

		public string Message
		{
			get { return message; }
		}
		readonly string message;

		#region Notification Types

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used in a comparison")]
		public static NotificationSubscriberType Info
		{
			get { return new NotificationSubscriberType("Info"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used in a comparison")]
		public static NotificationSubscriberType VerboseInfo
		{
			get { return new NotificationSubscriberType("Verbose Info"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used in a comparison")]
		public static NotificationSubscriberType Progress
		{
			get { return new NotificationSubscriberType("Progress"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used in a comparison")]
		public static NotificationSubscriberType BusinessObjectCreatedOrUpdated
		{
			get { return new NotificationSubscriberType("Business Object Created / Updated"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used in a comparison")]
		public static NotificationSubscriberType OrganisationMatched
		{
			get { return new NotificationSubscriberType("Organisation Matched"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used in a comparison")]
		public static NotificationSubscriberType OrganisationUnmatched
		{
			get { return new NotificationSubscriberType("Organisation Unmatched"); }
		}

		#endregion

		#region Equals / GetHashCode / Operators

		public static bool operator ==(NotificationSubscriberType lhs, NotificationSubscriberType rhs)
		{
			return object.Equals(lhs, rhs);
		}

		public static bool operator !=(NotificationSubscriberType lhs, NotificationSubscriberType rhs)
		{
			return !object.Equals(lhs, rhs);
		}

		public override bool Equals(object obj)
		{
			NotificationSubscriberType rhs = obj as NotificationSubscriberType;
			return rhs != null && Message.Equals(rhs.Message);
		}

		public override int GetHashCode()
		{
			return Message.GetHashCode();
		}

		#endregion

		#region GetDisplayMessage

		public virtual string GetDisplayMessage(string additionalInfo)
		{
			return GetDisplayMessage(additionalInfo, !(this == Info || this == VerboseInfo));
		}

		protected string GetDisplayMessage(string additionalInfo, bool showErrorType)
		{
			string result = Message;
			if (!string.IsNullOrEmpty(additionalInfo))
			{
				if (showErrorType)
				{
					result = Message + " (" + additionalInfo + ")";
				}
				else
				{
					result = additionalInfo;
				}
			}
			return result;
		}

		#endregion

		#region INotificationType Members

		bool INotificationType.IsFatal
		{
			get { return false; }
		}

		int INotificationType.Severity
		{
			get { return 0; }
		}

		#endregion

		#region INotificationType Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used in a comparison")]
		public string EnumValueName
		{
			get { return "Information"; }
		}

		#endregion
	}
}
