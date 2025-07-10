using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class ZCannotSaveException : ZException
	{
		public ZCannotSaveException(string message, string heading, bool shouldReprocess, Exception inner, ExceptionType type = ExceptionType.Unhandled)
			: base(message, inner)
		{
			Heading = heading;
			ShouldReprocess = shouldReprocess;
			Type = type;
		}

		public ZCannotSaveException(string message, string heading, bool shouldReprocess, ExceptionType type = ExceptionType.Unhandled)
			: base(message)
		{
			Heading = heading;
			ShouldReprocess = shouldReprocess;
			Type = type;
		}

		public ZCannotSaveException(string message, string heading, Exception inner, ExceptionType type = ExceptionType.Unhandled)
			: this(message, heading, false, inner, type)
		{
		}

		public ZCannotSaveException(string message, string heading, ExceptionType type = ExceptionType.Unhandled)
			: this(message, heading, false, type)
		{
		}

		public bool IsBusinessFailure => Type == ExceptionType.BusinessFailure;

#if NETFRAMEWORK
		protected ZCannotSaveException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			Heading = info.GetString(nameof(Heading));
			ShouldReprocess = info.GetBoolean(nameof(ShouldReprocess));
			Type = (ExceptionType)Enum.Parse(typeof(ExceptionType), info.GetString(nameof(Type)));
		}
#endif

		public string Heading { get; }
		public bool ShouldReprocess { get; }
		public ExceptionType Type { get; }

#if NETFRAMEWORK
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			if (info == null)
			{ throw new ArgumentNullException(nameof(info)); }

			info.AddValue(nameof(Heading), Heading);
			info.AddValue(nameof(ShouldReprocess), ShouldReprocess);
			info.AddValue(nameof(Type), Type);

			base.GetObjectData(info, context);
		}
#endif
	}
}
