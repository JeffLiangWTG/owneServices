using System;
using CargoWise.Common;

namespace CargoWise.Types
{
	[Serializable]
	public sealed class ZTypeValueException : Exception
	{
		public ZTypeValueException()
		{
		}

		public ZTypeValueException(string message)
			: base(message)
		{
		}

		public ZTypeValueException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public ZTypeValueException(Type expectedZType, object value, Exception innerException = null)
			: base(UnsupportedTypeMessage(expectedZType, value), innerException)
		{
			Argument.NotNull(expectedZType, nameof(expectedZType));
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		ZTypeValueException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message string")]
		static string UnsupportedTypeMessage(Type expectedZType, object value)
		{
			Argument.NotNull(expectedZType, nameof(expectedZType));
			string descriptionOfValue = value == null ? "<null>" : "<" + value + "> (" + value.GetType() + ")";
			return "Cannot initialise a " + expectedZType + " with " + descriptionOfValue + ".";
		}
	}
}
