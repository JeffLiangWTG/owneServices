using System;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class WarningType : NotificationSubscriberType
	{
		protected WarningType(string name, string message) : base(name, message)
		{
		}

		protected WarningType(string message) : this(message, message)
		{
		}

		#region Notification Types

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public static WarningType Warning { get { return new WarningType("Warning", Res.GetString("c3fe115f-4b74-48d5-bcda-c06a6fba3b44", "Validation Warning")); } }
		public static WarningType MaxLengthExceeded { get { return new WarningType("MaxLengthExceeded", Res.GetString("80fa096d-85c3-405c-a048-f9be47fca03b", "Maximum length of this field has been exceeded")); } }
		public static WarningType RecordAlreadyExists { get { return new WarningType("RecordAlreadyExists", Res.GetString("c8360e85-bc8d-4db4-869e-36a4619d319b}", "Record already exists")); } }

		public override string GetDisplayMessage(string additionalInfo)
		{
			bool showErrorType = this != Warning;
			return Res.GetString("bf64f3f0-ecb3-4bce-b3c7-1bd4b549b910", "Warning:") + " " + base.GetDisplayMessage(additionalInfo, showErrorType);
		}

		#endregion
	}
}
