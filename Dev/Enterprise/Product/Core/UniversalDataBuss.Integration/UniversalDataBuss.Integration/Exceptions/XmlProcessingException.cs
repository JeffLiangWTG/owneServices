using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.Integration
{
	[Serializable]
	public class XmlProcessingException : InvalidOperationException
	{
		public XmlProcessingException() : base() { }

		public XmlProcessingException(string message) : base(message) { }

		public XmlProcessingException(string message, Exception innerException) : base(message, innerException) { }

#if NETFRAMEWORK
		protected XmlProcessingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public XmlProcessingException(Type typeWithError, string message) : base(GetMessage(typeWithError, message)) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message for Exception")]
		static string GetMessage(Type typeWithError, string message)
		{
			return "Error processing Type [" + typeWithError.Name + "] - " + message;
		}

		public XmlProcessingException(PropertyInfo propertyWithError, string message) : base(GetMessage(propertyWithError, message)) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message for Exception")]
		static string GetMessage(PropertyInfo propertyWithError, string message)
		{
			return "Error processing Property [" + propertyWithError.DeclaringType.Name + "." + propertyWithError.Name + "] - " + message;
		}
	}
}
