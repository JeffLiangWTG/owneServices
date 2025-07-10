#if NETFRAMEWORK
using System.Web;
#else
using MimeKit;
#endif
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class BinaryFileProvider : IBinaryFile
	{
		public BinaryFileProvider(CusStorageDocPivot attachment)
		{
			binaryFile = Argument.NotNull(attachment, nameof(attachment));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string ContentIDPrefix = "cid:";

		readonly CusStorageDocPivot binaryFile;

		public string Identification => ContentIDPrefix + binaryFile.CSD_StorageDocReference;

		public string Filename => binaryFile.FileName;

		public string MIME
		{
			get
			{
#if NETFRAMEWORK
				return MimeMapping.GetMimeMapping(binaryFile.FileName);
#else
				return MimeTypes.GetMimeType(binaryFile.FileName);
#endif
			}
		}

		public string Description => binaryFile.CSD_Description;
	}
}
