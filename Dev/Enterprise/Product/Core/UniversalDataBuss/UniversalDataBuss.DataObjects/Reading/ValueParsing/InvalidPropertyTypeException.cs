using System;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	[Serializable]
	public class InvalidPropertyTypeException : Exception
	{
		public InvalidPropertyTypeException() : base() { }

		public InvalidPropertyTypeException(string message) : base(message) { }

		public InvalidPropertyTypeException(string message, Exception innerException) : base(message, innerException) { }

#if NETFRAMEWORK
		protected InvalidPropertyTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
	}
}
