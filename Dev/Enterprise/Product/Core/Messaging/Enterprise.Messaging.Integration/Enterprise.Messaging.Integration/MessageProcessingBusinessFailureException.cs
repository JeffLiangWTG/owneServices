using System;

namespace Enterprise.Messaging.Integration
{
	[Serializable]
	public class MessageProcessingBusinessFailureException : Exception
	{
		public MessageProcessingBusinessFailureException(string message, string caption, bool shouldRetry, string logNote)
			: base(message)
		{
			Caption = caption;
			ShouldRetry = shouldRetry;
			LogNote = logNote;
		}

		public MessageProcessingBusinessFailureException(string message, bool shouldRetry, string logNote)
			: this(message, "", shouldRetry, logNote)
		{
		}

		public MessageProcessingBusinessFailureException(string message, string caption, bool shouldRetry)
			: this(message, caption, shouldRetry, "")
		{
		}

		public MessageProcessingBusinessFailureException(string message)
			: base(message)
		{
		}

		public MessageProcessingBusinessFailureException(string message, string caption, bool shouldRetry, string logNote, Exception innerException)
			: base(message, innerException)
		{
			Caption = caption;
			ShouldRetry = shouldRetry;
			LogNote = logNote;
		}

		public MessageProcessingBusinessFailureException(string message, bool shouldRetry, string logNote, Exception innerException)
			: this(message, "", shouldRetry, logNote, innerException)
		{
		}

		public MessageProcessingBusinessFailureException(string message, string caption, bool shouldRetry, Exception innerException)
			: this(message, caption, shouldRetry, "", innerException)
		{
		}

		public MessageProcessingBusinessFailureException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public MessageProcessingBusinessFailureException()
			: base()
		{
		}

#if NETFRAMEWORK
		protected MessageProcessingBusinessFailureException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			if (info != null)
			{
				Caption = info.GetString(captionFileName);
				ShouldRetry = info.GetBoolean(shouldRetryFileName);
				LogNote = info.GetString(logNoteFileName);
			}
		}
#endif

		public string Caption { get; private set; }
		public bool ShouldRetry { get; private set; }
		public string LogNote { get; private set; }

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);

			if (info != null)
			{
				info.AddValue(captionFileName, Caption);
				info.AddValue(shouldRetryFileName, ShouldRetry);
				info.AddValue(logNoteFileName, LogNote);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Serialization constant")]
		const string captionFileName = "Caption";
		const string shouldRetryFileName = "ShouldRetry";
		const string logNoteFileName = "LogNote";
	}
}
