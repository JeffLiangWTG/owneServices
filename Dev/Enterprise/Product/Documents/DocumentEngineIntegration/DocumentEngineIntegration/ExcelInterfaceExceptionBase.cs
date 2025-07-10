using System;

namespace Enterprise.DocumentEngineIntegration
{
	[Serializable]
	public class ExcelInterfaceExceptionBase : Exception
	{
		#region Exception messages

		public static string ErrorInsertingImageMessage
		{
			get
			{
				return Res.GetString("7925af25-8ccd-4855-a3cf-fe23114fb8ce", "Error inserting image into cell or group of cells. It's likely the image resolution is too big");
			}
		}

		public static string ErrorFontNotSupportedMessage
		{
			get
			{
				return Res.GetString("5d32efed-a948-4063-ba5d-0227f6f0ac75", "Bad font");
			}
		}

		public static string ErrorFontNotFoundMessage
		{
			get
			{
				return Res.GetString("4c81b0ca-5084-4c6b-bcab-fe9189a73a8d", "Font not found");
			}
		}

		public static string ErrorMessageFileFormatNotSupported
		{
			get
			{
				return Res.GetString("7fce63da-11b2-40bd-8ec0-f9fbc09d8bb7", "You can only load templates saved as 'Excel 97-2003 Workbook' (.xls) or 'Excel 2007 Workbook' (.xlsx) format.");
			}
		}

		public static string FileCorruptedMessage
		{
			get
			{
				return Res.GetString("7665e045-df5c-42e2-9baa-31abf707036e", "The selected file could not be loaded. It may be damaged or an unsupported format. Please check the file and try again.");
			}
		}

		public static string ErrorCopyOLEObjectInTemplateMessage
		{
			get
			{
				return Res.GetString("78923A5B-C3B4-4A18-8F86-7963F170CFA9", "Embedded OLE objects are not supported in Document Engine. Please remove them from your document templates or strips.");
			}
		}

		#endregion // Exception messages

		protected ExcelInterfaceExceptionBase(string message)
			: base(message)
		{
		}

		protected ExcelInterfaceExceptionBase(string message, Exception inner)
			: base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected ExcelInterfaceExceptionBase(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
