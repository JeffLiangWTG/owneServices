using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;

namespace Enterprise.Customs.JP.Business
{
	sealed class FileProvider : IFile
	{
		public FileProvider(MSXMessageSendingObjectAttachment file)
		{
			Argument.NotNull(file, nameof(file));
			this.file = file;
		}

		readonly MSXMessageSendingObjectAttachment file;

		public string FileName => file.Document?.FileName ?? string.Empty;

		public decimal? FileSize
		{
			get
			{
				var result = file.FileSize.Round(2);
				return result > 10000 ? 9999 : result;
			}
		}

		public string DocumentType => file.Type;
	}
}
