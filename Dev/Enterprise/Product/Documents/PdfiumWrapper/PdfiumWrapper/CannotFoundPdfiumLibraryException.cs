using System;

namespace CargoWise.PdfiumWrapper
{
	[Serializable]
	public class CannotFoundPdfiumLibraryException : Exception
	{
		public CannotFoundPdfiumLibraryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected CannotFoundPdfiumLibraryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
