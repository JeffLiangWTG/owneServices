using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;

namespace Enterprise.RemotePrinting.Client
{
	public class CNSWMessageReceiverController : CustomsMessageController
	{
		public CNSWMessageReceiverController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken) { }

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
		{
			return new CNSWClientApplicationSettingManager(machineName, WebServiceClient);
		}

		const string responseFilePattern = "*.xml";
		const char fileNameSeperator = '_';

		DateTime GetResponseTime(string filePath)
		{
			int timeStampLength = CNSWConstants.TimeStampFormat.Length;
			var fileName = Path.GetFileNameWithoutExtension(filePath);
			if (!fileName.Contains(fileNameSeperator) || !DateTime.TryParseExact(fileName.Split(fileNameSeperator).Last().PadRight(timeStampLength, '0').Substring(0, timeStampLength),
				CNSWConstants.TimeStampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime responseTime))
			{
				responseTime = File.GetCreationTime(filePath);
			}
			return responseTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logs start and stop of process.")]
		protected override void Process()
		{
			OnShowInformation("CNSW Message Reciever Process Started.");
			base.Process();
			OnShowInformation("CNSW Message Reciever Process Stoped.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "suppresses warning in root")]
		protected override int ProcessCore(ICustomseHubClientSetting setting)
		{
			var result = 0;
			if (setting is ICNSWClientApplicationSetting cnswSetting)
			{
				var recipient = cnswSetting.EHubClientID;
				recipient = recipient.Length > 9 ? recipient.Substring(0, 9) : recipient;

				var foldersToSearch = new[] { cnswSetting.ErrorResponseFolder, cnswSetting.ReceiveFolder, cnswSetting.AcdaReceiveFolder, cnswSetting.AcdaErrorResponseFolder }
				.Where(path => !string.IsNullOrWhiteSpace(path));

				var responseXmlFiles = foldersToSearch.SelectMany(folder => Directory.EnumerateFiles(folder, responseFilePattern)).OrderBy(GetResponseTime);

				if (responseXmlFiles.Any())
				{
					using var eHubServiceClient = SettingManager.CreateEHubServiceClient(cnswSetting);
					foreach (var responseXmlFilePath in responseXmlFiles)
					{
						var shouldArchive = false;
						var processLog = string.Empty;

						Stream processedStream = null;
						XmlWriter writer = null;

						try
						{
							using (var originalStream = new FileStream(responseXmlFilePath, FileMode.Open))
							{
								var fileXmlDoc = new XmlDocument();
								fileXmlDoc.Load(originalStream);

								var fileName = Path.GetFileName(responseXmlFilePath);
								var docElement = fileXmlDoc.DocumentElement;
								var namespaceUrl = docElement.NamespaceURI;

								switch (docElement.Name)
								{
									case "Root":
										if (string.IsNullOrWhiteSpace(namespaceUrl))
										{
											namespaceUrl = CNSWConstants.XmlNamespaces.ChinaPortDec;
											docElement.SetAttribute("xmlns", namespaceUrl);
											PopulateStreamToSend(w => docElement.WriteTo(w));
										}
										break;
									case CNSWConstants.XmlElements.ImportAgrResponse:
										XNamespace ns0 = CNSWConstants.XmlNamespaces.GenericMessageDelivery;

										var gimInterchangeElement = new XElement(ns0 + CNSWConstants.XmlElements.GenericMessageInterchange,
											new XAttribute(XNamespace.Xmlns + "ns0", ns0),
											new XElement("Header",
												new XElement("SenderID", cnswSetting.EHubClientID),
												new XElement("RecipientID", recipient),
												new XElement("InterchangeType", "CSW"),
												new XElement("InterchangeNumber", GetImportAgrResponseInterchangeNumber())
											),
											new XElement("Body", XElement.Parse(fileXmlDoc.InnerXml))
										);

										string GetImportAgrResponseInterchangeNumber()
										{
											var words = fileName.Split('_', '.');
											return words[Math.Min(words.Length - 1, 1)];
										}

										PopulateStreamToSend(w => gimInterchangeElement.WriteTo(w));
										break;
								}

								void PopulateStreamToSend(Action<XmlWriter> populateAction)
								{
									processedStream = new VirtualStream();
									writer = new XmlTextWriter(processedStream, Encoding.UTF8);
									populateAction(writer);

									writer.Flush();
									processedStream.SeekBegin();
								}

								var trackingID = Guid.NewGuid();
								eHubServiceClient.SendStream(trackingID, recipient, fileName, namespaceUrl, processedStream ?? originalStream);
								processLog = $"TrackID: {trackingID}, {responseXmlFilePath} sent to eHub, RecipientID: {recipient}.";

								shouldArchive = true;
							}
						}
						catch (XmlException)
						{
							shouldArchive = true;
							processLog = $"{responseXmlFilePath} is an invalid xml file, skipped it.";
						}
						finally
						{
							writer?.Close();
							processedStream?.Dispose();
						}

						if (shouldArchive)
						{
							processLog += ArchiveOrBackupFile(responseXmlFilePath, cnswSetting);
						}

						OnShowInformation(processLog);
					}
				}
				else
				{
					OnShowInformation($"No xml file found in {string.Join(", ", foldersToSearch)}");
				}

				result = responseXmlFiles.Count();
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Handles file archive or backup.")]
		string ArchiveOrBackupFile(string responseXmlFilePath, ICNSWClientApplicationSetting setting)
		{
			string result;
			try
			{
				var acrhiveFolderPath = Path.GetDirectoryName(responseXmlFilePath) == setting.AcdaReceiveFolder ? setting.AcdaArchiveFolder : setting.ArchiveFolder;
				result = " Archived as: " + ArchiveFile(acrhiveFolderPath, responseXmlFilePath);
			}
			catch (Exception ex)
			{
				var responseXmlFileBackupPath = responseXmlFilePath + ".bak";
				result = $" Archive failed: {ex.Message}, backuped as: " + responseXmlFileBackupPath;
				File.Move(responseXmlFilePath, responseXmlFileBackupPath);
			}
			return result;
		}

		string ArchiveFile(string archiveFolder, string filePath)
		{
			var fileName = Path.GetFileName(filePath);
			var archivedFileName = GetNewTimeStampedFilePathIfNecessary(archiveFolder, fileName);

			File.Move(filePath, archivedFileName);

			return archivedFileName;
		}
	}
}
