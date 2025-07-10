using System;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class UnsupportedDocumentException : Exception
	{
		/// <summary>
		/// Needs to be phrased like "Operation is not supported for this document because {0}"
		/// </summary>
		public string ReasonDocumentIsUnsupported { get; }

		public UnsupportedDocumentException(IMultilingualString reasonDocumentIsUnsupported, string message, Exception innerException)
			: base(message, innerException)
		{
			ReasonDocumentIsUnsupported = Argument.NotNull(reasonDocumentIsUnsupported, nameof(reasonDocumentIsUnsupported)).ToString();
		}

#if NETFRAMEWORK
		protected UnsupportedDocumentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			ReasonDocumentIsUnsupported = info.GetString(nameof(ReasonDocumentIsUnsupported));
		}
#endif

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue(nameof(ReasonDocumentIsUnsupported), ReasonDocumentIsUnsupported);
			base.GetObjectData(info, context);
		}
	}
}
