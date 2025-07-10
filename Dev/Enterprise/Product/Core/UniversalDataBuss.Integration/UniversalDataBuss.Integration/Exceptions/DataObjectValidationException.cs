using System;
using System.Reflection;
using CargoWise.Common;

namespace Enterprise.UniversalDataBuss.Integration
{
	[Serializable]
	[ExceptionVisibility(ExceptionVisibility.User)]
	public class DataObjectValidationException : Exception
	{
		public DataObjectValidationException()
		{
		}

		public DataObjectValidationException(string message) : base(message)
		{
		}

		public DataObjectValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DataObjectValidationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public DataObjectValidationException(Type typeWithError, string message) : base(GetMessage(typeWithError, message))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message for Exception")]
		static string GetMessage(Type typeWithError, string message)
		{
			return "Error processing Type [" + typeWithError.Name + "] - " + message;
		}

		public DataObjectValidationException(PropertyInfo propertyWithError, string message) : base(GetMessage(propertyWithError, message))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message for Exception")]
		static string GetMessage(PropertyInfo propertyWithError, string message)
		{
			return "Error processing Property [" + propertyWithError.DeclaringType.Name + "." + propertyWithError.Name + "] - " + message;
		}
	}
}
