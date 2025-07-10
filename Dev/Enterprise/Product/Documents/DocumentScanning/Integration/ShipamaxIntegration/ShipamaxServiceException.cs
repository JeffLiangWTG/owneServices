using System;

namespace Enterprise.DocumentScanning.Integration
{
	[Serializable]
	public class ShipamaxServiceException : Exception
	{
		public ShipamaxServiceException(ShipamaxServiceErrorType errorType, string error)
			: this(errorType, error, null)
		{
		}

		public ShipamaxServiceException(ShipamaxServiceErrorType errorType, string error, Exception innerException)
			: base(error, innerException)
		{
			Error = error;
			ErrorType = errorType;
		}

#if NETFRAMEWORK
		protected ShipamaxServiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public string Error { get; }

		public ShipamaxServiceErrorType ErrorType { get; }

		public override string Message => FormattableString.Invariant($"{ErrorType} happens. Error message: {Error}");
	}
}
