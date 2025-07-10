using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentImageProvider : IDocumentImage
	{
		public DocumentImageProvider(IeDoc eDoc)
		{
			ieDoc = Argument.NotNull(eDoc, nameof(eDoc));
			id = ZGuid.NewZGuid();
		}

		readonly IeDoc ieDoc;
		readonly ZGuid id;

		public string Id => id.ToString();

		public string Filename => ieDoc.FileName;

		public string FileType => ieDoc.DocType;

		public string Size => GetSizeText(ieDoc.FileSizeInMB);

		protected virtual string GetSizeText(ZDecimal fileSizeInMB) => string.Format(CultureInfo.CurrentCulture, "{0:0}", fileSizeInMB * 1024 * 1024);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying messaging infrastructure requires byte[]")]
		public byte[] Document => ieDoc.UniqueKey.ToGuid().ToByteArray();
	}
}
