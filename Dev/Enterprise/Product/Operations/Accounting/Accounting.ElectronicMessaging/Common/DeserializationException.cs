using System;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	[Serializable]
	public class DeserializationException : Exception
	{
#if NETFRAMEWORK
		protected DeserializationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public DeserializationException(string message, string headingForSerializedText, string serializedText, Exception innerException = null)
			: base(BuildErrorMessage(message, headingForSerializedText, serializedText), innerException)
		{
		}

		static string BuildErrorMessage(string message, string headingForSerializedText, string serializedText) => FormattableString.Invariant($@"{message}
{headingForSerializedText}:
{(!string.IsNullOrEmpty(serializedText) ? serializedText : GetEmptyserializedText())}"); // Nothing to translate.

		static string GetEmptyserializedText() => Res.GetString("c1e742c4-fbda-458a-9db1-e574371665af", "<Empty>");
	}
}
