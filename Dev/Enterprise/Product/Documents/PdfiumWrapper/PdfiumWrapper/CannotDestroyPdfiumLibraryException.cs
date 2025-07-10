using System;

namespace CargoWise.PdfiumWrapper
{
	[Serializable]
	public class CannotDestroyPdfiumLibraryException : Exception
	{
		public CannotDestroyPdfiumLibraryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected CannotDestroyPdfiumLibraryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
