using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DocumentEngine.FileFormatUtilities;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	internal class Memory : DeliveryMethod
	{
		public Memory(List<(string fileName, string docType, byte[] imageBytes)> output)
		{
			output.Clear();
			outputList = output;
			OutputFormatOverride = OutputFormatType.Unspecified;
		}

		readonly List<(string fileName, string docType, byte[] imageBytes)> outputList;
		public OutputFormatType OutputFormatOverride { get; set; }

		protected override void DeliverCore(INotifications notifications = null)
		{
			foreach (var info in DeliveryInfos)
			{
				info.FileContents.Position = 0;
				var buffer = new byte[info.FileContents.Length];
				info.FileContents.Read(buffer, 0, (int)info.FileContents.Length);
				var fileExtension = info.FileFormat;
				var fileName = info.AttachedFilename;

				if (info.ShouldConvertFromExcel(OutputFormatOverride))
				{
					buffer = DocumentConverter.ConvertFromExcel(buffer, string.Empty, OutputFormatOverride, info.Watermark, Enterprise.ZArchitecture.Environment.ColourDepth.BlackAndWhite, false, 1);
					fileExtension = OutputFormatOverride.ToString().ToLowerInvariant();
				}

				fileName = fileName + "." + fileExtension;
				outputList.Add((fileName, docType: info.DocumentType, imageBytes: buffer));
			}
		}
	}
}
