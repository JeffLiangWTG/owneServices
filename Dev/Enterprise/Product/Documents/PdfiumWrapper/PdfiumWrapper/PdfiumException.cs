using System;

namespace CargoWise.PdfiumWrapper
{
	[Serializable]
	public class PdfiumException : Exception
	{
		public PdfiumError ErrorType { get; }

		public PdfiumException(PdfiumError error, Exception innerException = null)
			: base(MessageForError(error), innerException)
		{
			ErrorType = error;
		}

#if NETFRAMEWORK
		protected PdfiumException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			ErrorType = (PdfiumError)Enum.Parse(typeof(PdfiumError), info.GetString(nameof(ErrorType)));
		}
#endif

#if NET8_0_OR_GREATER
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(ErrorType), ErrorType.ToString());
		}

		#region SuppressResourceStringsCheckRegion

		static string MessageForError(PdfiumError error)
		{
			switch (error)
			{
				case PdfiumError.None: return "No Error";
				case PdfiumError.Unknown: return "Unknown error";
				case PdfiumError.FileNotFound: return "File not found or could not be opened";
				case PdfiumError.FileCorrupt: return "File not in PDF format or corrupted";
				case PdfiumError.IncorrectPassword: return "Password required or incorrect password";
				case PdfiumError.UnsupportedEncryption: return "Unsupported security scheme";
				case PdfiumError.PageNotFound: return "Page not found or content error";
				case PdfiumError.LicensingError: return "The requested operation cannot be completed due to a license restrictions";
				case PdfiumError.CannotImportPages: return "Cannot Import Pages";
				default: return "Error Code Not Defined";
			}
		}

		#endregion
	}

	public enum PdfiumError
	{
		None = 0,
		Unknown = 1,
		FileNotFound = 2,
		FileCorrupt = 3,
		IncorrectPassword = 4,
		UnsupportedEncryption = 5,
		PageNotFound = 6,
		CannotImportPages = 487,
		LicensingError = 1001
	}
}
