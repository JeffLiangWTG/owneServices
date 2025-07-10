using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;

namespace Enterprise.Customs.GB.Pentant
{
	class PentantFtpUploader : IGbCspUploaderInterface
	{
		public PentantFtpUploader(ILogger logger)
		{
			this.logger = logger;
		}
		public CredentialsSetting CredentialsSetting { get; set; }
		public string Url { get; set; }
		public string UserAgent { get; set; }

		public string processEDIMessage(string ediMessageToUpload, string companyCode, bool operationalFlag)
		{
			var urlWithUploadSubfolder = Url.EndsWith("/", System.StringComparison.OrdinalIgnoreCase) ? Url + CredentialsSetting.Printer + PentantConstants.OutputFolder
																: Url + "/" + CredentialsSetting.Printer + PentantConstants.OutputFolder;
			var options = new PentantFtpOptionsProvider(CredentialsSetting, urlWithUploadSubfolder);
			var ftpEngine = new PentantFtpEngineWithTrigger(options, logger);
			WriteUploadFilesToDisk(ediMessageToUpload, companyCode, options.TempFolder);
			ftpEngine.Upload();
			return "";
		}

		void WriteUploadFilesToDisk(ZString ediMessageToUpload, string companyCode, TempDirectory tempFolder)
		{
			var fileExtension = GetDataFileExtension(ediMessageToUpload);
			var likelyInterchangeNumber = "";
			if (fileExtension == PentantConstants.CargoMessageFileExtension)
			{
				likelyInterchangeNumber = Business.GbExtensionHelpers.MakeUniqueInterchangeNumber("");
			}
			else
			{
				// EDIFACT
				likelyInterchangeNumber = ediMessageToUpload.SubstringSafe(ediMessageToUpload.LastIndexOf("UNZ", System.StringComparison.OrdinalIgnoreCase) + 3);
				var badChars = Path.GetInvalidFileNameChars().Union(new[] { '+', '\'', });
				likelyInterchangeNumber = new string(likelyInterchangeNumber.Select(ch => badChars.Contains(ch) ? '_' : ch).ToArray());
			}
			var filename = ZDateTime.UtcNow.ToString("yyyyMMdd.HHmmss", CultureInfo.InvariantCulture) + companyCode + likelyInterchangeNumber;
			var filesetName = Path.Combine(tempFolder.DirectoryName, filename);
			File.WriteAllText(filesetName + fileExtension, ediMessageToUpload);
			File.WriteAllText(filesetName + PentantConstants.TriggerFileExtension, "");
		}

		string GetDataFileExtension(ZString ediMessageToUpload)
		{
			return ediMessageToUpload.StartsWith(Pentant.CargoReportMessage.BeginMessage, StringComparison.OrdinalIgnoreCase) ? PentantConstants.CargoMessageFileExtension : PentantConstants.DeclarationMessageFileExtension;
		}

		readonly ILogger logger;
	}
}
