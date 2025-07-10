using System;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class ExcelInterfaceException : ExcelInterfaceExceptionBase, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal ExcelInterfaceException(ExcelInterfaceExceptionType type, string message)
			: base(message)
		{
			this.Type = type;
		}

		internal ExcelInterfaceException(ExcelInterfaceExceptionType type, string message, Exception innerException)
			: base(message, innerException)
		{
			this.Type = type;
		}

		#region Public Static Constructor for Testing
#if DEBUG
		public static ExcelInterfaceException NewForTesting(ExcelInterfaceExceptionType type, string message)
		{
			return new ExcelInterfaceException(type, message);
		}
#endif
		#endregion

		public readonly ExcelInterfaceExceptionType Type;

#if NETFRAMEWORK
		protected ExcelInterfaceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			string type = info.GetString("ExcelInterfaceExceptionType"); // Binary Serialiization Key
			if (!string.IsNullOrEmpty(type) && Enum.IsDefined(typeof(ExcelInterfaceExceptionType), type))
			{
				this.Type = (ExcelInterfaceExceptionType)Enum.Parse(typeof(ExcelInterfaceExceptionType), type);
			}
		}
#endif

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("ExcelInterfaceExceptionType", Type.ToString());
		}

		#region Constructor For IJsonSerializable

		internal ExcelInterfaceException(ExcelInterfaceExceptionJsonData data)
			: this(data.Type, data.Message, data.InnerExceptionMessage != null ? new Exception(data.InnerExceptionMessage) : null)
		{
		}

		#endregion

		public object GetJsonData() => new ExcelInterfaceExceptionJsonData()
		{
			Type = Type,
			Message = base.Message,
			InnerExceptionMessage = base.InnerException?.Message,
		};

		public override string Message
		{
			get
			{
				string returnMessage;
				switch (this.Type)
				{
					case ExcelInterfaceExceptionType.ErrorInsertingImage:
						returnMessage = ExcelInterfaceExceptionBase.ErrorInsertingImageMessage;
						break;
					case ExcelInterfaceExceptionType.ErrorFontNotSupported:
						returnMessage = ExcelInterfaceExceptionBase.ErrorFontNotSupportedMessage;
						returnMessage += System.Environment.NewLine + (NoResString)"Details: [" + base.Message + (NoResString)"]";
						break;
					case ExcelInterfaceExceptionType.ErrorFontNotFound:
						returnMessage = ExcelInterfaceExceptionBase.ErrorFontNotFoundMessage;
						returnMessage += System.Environment.NewLine + (NoResString)"Details: [" + base.Message + (NoResString)"]";
						break;
					case ExcelInterfaceExceptionType.FileFormatNotSupported:
						returnMessage = ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported;
						break;
					case ExcelInterfaceExceptionType.CouldNotOpenFile:
					case ExcelInterfaceExceptionType.CouldNotOpenStream:
						returnMessage = ExcelInterfaceExceptionBase.FileCorruptedMessage;
						break;
					case ExcelInterfaceExceptionType.CopyOLEObjectInTemplateException:
						returnMessage = ExcelInterfaceExceptionBase.ErrorCopyOLEObjectInTemplateMessage;
						break;
					default:
						returnMessage = base.Message;
						break;
				}
				return returnMessage;
			}
		}
	}

	public enum ExcelInterfaceExceptionType
	{
		ErrorSettingCellValue,
		ErrorInsertingImage,
		CouldNotOpenFile,
		CouldNotOpenStream,
		NoVisibleSheetsToSelect,
		FlexCelScaleSheetError,
		ErrorFindingCellRange,
		ErrorRemovingObject,
		FileFormatNotSupported,
		ErrorFontNotFound,
		ErrorFontNotSupported,
		CopyOLEObjectInTemplateException,
		TooManyCellStyles,
		CouldNotOpenCustomizedDocumentElements,
		DigitalSignatureNotValid,
		ErrorGettingCertificateChain,
		ErrorInvalidColumn,
		IllegalCharactersInPath,
	}
}
