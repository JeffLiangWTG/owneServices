using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	public enum ReportServiceErrorType
	{
		ValidationError,
		RunningError,
		DeliveryError,
		ConfigurationError,
		Unauthorized,
		LookupError,
		ScheduleError,
	}

	[Serializable]
	public class ReportServiceException : Exception
	{
#if NETFRAMEWORK
		protected ReportServiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public ReportServiceException(string[] errors, ReportServiceErrorType errorType)
		{
			Errors = errors.ToList();
			ErrorType = errorType;
		}

		public List<string> Errors { get; } = new List<string>();
		public ReportServiceErrorType ErrorType { get; }

		public override string Message => FormattableString.Invariant($"{ErrorType} happens, error message: \r\n{string.Join("\r\n", Errors)}");

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue((NoResString)"Errors", Errors);
			info.AddValue("ErrorType", ErrorType);
			base.GetObjectData(info, context);
		}
	}
}
