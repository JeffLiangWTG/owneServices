using System;
using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using Enterprise.DocumentEngine.FileFormatUtilities;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	internal class Disk : DeliveryMethod
	{
		public Disk()
		{
			OutputFormatOverride = OutputFormatType.Unspecified;
		}

		public string OutputDirectory
		{
			get { return fOutputDirectory; }
			set { fOutputDirectory = value; }
		}

		public OutputFormatType OutputFormatOverride
		{
			get;
			set;
		}

		const int MaxLengthOfFilePath = 260; //The maximum length for a path(file name and its directory route) — also known as MAX_PATH — has been defined by 260 characters.

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Extensions should be lowercase")]
		protected override void DeliverCore(INotifications notifications = null)
		{
			foreach (DeliveryInfo info in DeliveryInfos)
			{
				info.FileContents.Position = 0;
				byte[] buffer = new byte[info.FileContents.Length];
				int bytesRead = info.FileContents.Read(buffer, 0, (int)info.FileContents.Length);
				string fileExtension = info.FileFormat;
				string fileName = info.AttachedFilename;
				if (string.IsNullOrEmpty(fileName))
				{
					fileName = Guid.NewGuid().ToString();
				}
				else
				{
					fileName = SanitizedFileName(fileName);
				}

				if (info.ShouldConvertFromExcel(OutputFormatOverride))
				{
					buffer = DocumentConverter.ConvertFromExcel(buffer, string.Empty, OutputFormatOverride, info.Watermark, Enterprise.ZArchitecture.Environment.ColourDepth.BlackAndWhite, false, 1);
					bytesRead = buffer.Length;
					fileExtension = OutputFormatOverride.ToString().ToLowerInvariant();
				}

				var maxLengthOfFileName = MaxLengthOfFilePath - OutputDirectory.Length - fileExtension.Length - 5; //5 is reserved place for file name duplication suffix : [1],[112]
				if (fileName.Length > maxLengthOfFileName)
				{
					fileName = fileName.Substring(0, maxLengthOfFileName).TrimEnd();
				}

				string filePath = OutputDirectory + "\\" + fileName + "." + fileExtension;

				int duplicateCount = 0;
				while (File.Exists(filePath))
				{
					duplicateCount++;
					filePath = OutputDirectory + "\\" + fileName + duplicateCount.ToString(CultureInfo.InvariantCulture) + "." + fileExtension;
				}

				if (duplicateCount > 0)
				{
					filePath = OutputDirectory + "\\" + fileName + duplicateCount.ToString(CultureInfo.InvariantCulture) + "." + fileExtension;
				}

				using (FileStream fileToSave = File.Create(filePath))
				{
					fileToSave.Write(buffer, 0, bytesRead);
				}

				info.FilePath = filePath;
			}
		}

		protected string fOutputDirectory;

		string SanitizedFileName(string fileName)
		{
			char[] invalidChars = Path.GetInvalidFileNameChars();

			int index = fileName.LastIndexOfAny(invalidChars);
			while (index >= 0)
			{
				fileName = fileName.Remove(index, 1);
				index = fileName.LastIndexOfAny(invalidChars);
			}
			return fileName;
		}
	}
}
