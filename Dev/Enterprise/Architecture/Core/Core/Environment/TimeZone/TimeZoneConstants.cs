using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class TimeZoneConstants
	{
		// DST Transition Types
		public const string DstTransitionTypeStart = "STA";
		public const string DstTransitionTypeEnd = "END";

		// DST Week-day / Day-of-Month Based
		public const string DstRuleDayOfMonth = "DAT";
		public const string DstRuleWeekday = "MON";

		// DST Transition TimeBase
		public const string DstTimeBaseStandard = "STD";
		public const string DstTimeBaseLocal = "LOC";
		public const string DstTimeBaseUtc = "UTC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal date formatting")]
		public static readonly string[] AbbreviatedMonths = new string[13] { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

		/// <summary>
		/// Gets the month number
		/// </summary>
		public static int GetMonthAsInt(string monthCode)
		{
			int result = -1;
			string upperCaseMonthCode = monthCode.ToUpper();

			for (int i = 1; i < AbbreviatedMonths.Length; i++)
			{
				if (upperCaseMonthCode == AbbreviatedMonths[i].ToUpper())
				{
					result = i;
					break;
				}
			}

			return result;
		}
	}

	public enum DstTransitionType
	{
		Start,
		End,
	}

	public enum DstTransitionTimeBase
	{
		Standard,
		Local,
		Utc,
	}

	[Serializable]
	public class TimeZoneException : Exception, IExceptionReporterExtender
	{
		public TimeZoneException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected TimeZoneException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region IExceptionReporterExtender Members

		/// <summary>
		/// Handle specific exceptions raised by Time Zone classes
		///   - Shows a message
		///   - Shuts down the application
		/// </summary>
		bool IExceptionReporterExtender.HandleException()
		{
			ObjectFactory.Get<IProgramRestarter>().ShutdownEnterpriseWithMessage(Message);
			return true;
		}

		#endregion
	}
}
