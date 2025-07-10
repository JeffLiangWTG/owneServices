using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture
{
	[Serializable]
	public class InfoNotification : NotificationSubscriberNotification
	{
		public InfoNotification(string message) : base(NotificationSubscriberType.Info, message)
		{
			if (message?.Length == 0)
			{
				ErrorReporter.ReportOnce(new System.Diagnostics.StackTrace().ToString(), "You cannot have an info notification without any info (blank Message string)");
			}
		}

		protected override string DisplayMessageCore
		{
			get { return string.IsNullOrEmpty(AdditionalInfo) ? string.Empty : base.DisplayMessageCore; }
		}
	}
}
