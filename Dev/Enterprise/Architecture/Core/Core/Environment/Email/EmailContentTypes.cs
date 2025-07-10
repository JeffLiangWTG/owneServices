using System;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class EmailContentTypes
	{
		public abstract string ContentTypeCode { get; }
		public abstract string ContentTypeDescription { get; }

		public static HTMLContentType HTML
		{
			get { return fHTML ?? (fHTML = new HTMLContentType()); }
		}

		[ThreadStatic]
		static HTMLContentType fHTML;

		public static PlainTextContentType PlainText
		{
			get { return fPlainText ?? (fPlainText = new PlainTextContentType()); }
		}

		[ThreadStatic]
		static PlainTextContentType fPlainText;

		public static CalendarContentType Calendar
		{
			get { return fCalendar ?? (fCalendar = new CalendarContentType()); }
		}

		[ThreadStatic]
		static CalendarContentType fCalendar;
	}

	#region HTML

	public class HTMLContentType : EmailContentTypes
	{
		public override string ContentTypeCode
		{
			get { return "HTM"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer constant")]
		public override string ContentTypeDescription
		{
			get { return "HTML Document"; }
		}
	}

	#endregion

	#region Plain Text

	public class PlainTextContentType : EmailContentTypes
	{
		public override string ContentTypeCode
		{
			get { return "PLN"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "developer constant")]
		public override string ContentTypeDescription
		{
			get { return "Plain Text"; }
		}
	}

	#endregion

	#region Calendar

	public class CalendarContentType : EmailContentTypes
	{
		public override string ContentTypeCode
		{
			get { return "CAL"; }
		}

		public override string ContentTypeDescription
		{
			get { return "vCalendar"; }
		}
	}

	#endregion
}
