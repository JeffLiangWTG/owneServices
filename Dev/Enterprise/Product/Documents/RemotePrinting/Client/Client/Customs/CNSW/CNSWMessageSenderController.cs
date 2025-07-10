using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;

namespace Enterprise.RemotePrinting.Client
{
	public class CNSWMessageSenderController : CustomsMessageController
	{
		public CNSWMessageSenderController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
		{
			fileFormatChecker = new MessageFileFormatChecker(this);
		}

		readonly MessageFileFormatChecker fileFormatChecker;

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
		{
			return new CNSWClientApplicationSettingManager(machineName, WebServiceClient);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		protected override void Process()
		{
			OnShowInformation("CNSW Message Sender Process Started.");
			base.Process();
			OnShowInformation("CNSW Message Sender Process Stoped.");
		}

		protected override int ProcessCore(ICustomseHubClientSetting setting)
		{
			int numberOfMessages = 0;

			if (setting is ICNSWClientApplicationSetting cnswSetting)
			{
				using (var eHubServiceClient = SettingManager.CreateEHubServiceClient(setting))
				{
					var response = eHubServiceClient?.ReceiveStream();
					if (response != null && response.Messages.Length > 0)
					{
						numberOfMessages = response.Messages.Length;
						var responseTrackingID = response.TrackingID;

						foreach (var message in response.Messages)
						{
							ProcessResponseMessage(message.MessageStream, message.MessageTrackingID.ToString(), message.FileName, cnswSetting);
						}

						eHubServiceClient.Finalise(responseTrackingID);
						OnShowInformation($"Response message with tracking ID: {responseTrackingID} processed.");
					}
					else
					{
						OnShowInformation($"No message received from eHub.");
					}
				}
			}

			return numberOfMessages;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Processes and extracts zipped XML messages.")]
		protected void ProcessResponseMessage(Stream eHubStream, string trackingId, string messageFileName, ICNSWClientApplicationSetting setting)
		{
			string outputFullPath = null;
			if (eHubStream.Length > 0)
			{
				var eHubStreamDecompressed = eHubStream.DecodeAndDecompress();
				using (var eHubStreamDecompressedReader = new XmlTextReader(eHubStreamDecompressed))
				{
					XElement rootElement = null;
					if (!eHubStreamDecompressedReader.IsEmptyElement)
					{
						try
						{
							rootElement = XElement.Load(eHubStreamDecompressedReader);
						}
						catch (XmlException)
						{
							outputFullPath = WriteToLocalAsUTF8Text(eHubStreamDecompressed, setting.ArchiveFolder, messageFileName);
						}
					}

					if (rootElement != null && !rootElement.IsEmpty)
					{
						var rootLocalName = rootElement.Name.LocalName;
						if (rootLocalName == CNSWConstants.XmlElements.GenericMessageInterchange)
						{
							var nsmgr = new XmlNamespaceManager(eHubStreamDecompressedReader.NameTable);
							nsmgr.AddNamespace("gmi", CNSWConstants.XmlNamespaces.GenericMessageDelivery);

							_ = ProcessZippedMessage() || ProcessImportAgrRequest() || ProcessDecMessage();

							bool ProcessZippedMessage()
							{
								var result = false;

								var attachedDoc = rootElement.XPathSelectElement("./gmi:Body/gmi:ZippedMessage", nsmgr)?.Value;
								if (attachedDoc != null && attachedDoc.Length > 0)
								{
									var attachedDocBytes = Convert.FromBase64String(attachedDoc);

									if (fileFormatChecker.CheckIsZipCompressedData(attachedDocBytes))
									{
										outputFullPath = GetNewTimeStampedFilePathIfNecessary(setting.SendFolder, messageFileName + ".zip");
										File.WriteAllBytes(outputFullPath, attachedDocBytes);
										result = true;
									}
								}

								return result;
							}

							bool ProcessImportAgrRequest()
							{
								var result = false;

								var importAgrRequestText = ReadXElement($@"./*[local-name()='Body']/*[local-name()='{CNSWConstants.XmlElements.ImportAgrRequest}']");
								if (fileFormatChecker.CheckIsXMLDocument(importAgrRequestText))
								{
									var interchangeNumber = ReadXElement("./*[local-name()='Header']/*[local-name()='InterchangeNumber']", readText: true);
									if (!string.IsNullOrWhiteSpace(interchangeNumber))
									{
										outputFullPath = GetNewTimeStampedFilePathIfNecessary(setting.AcdaSendFolder, interchangeNumber + ".xml");
										File.WriteAllText(outputFullPath, importAgrRequestText, new UTF8Encoding(false));
										result = true;
									}
								}

								return result;
							}

							bool ProcessDecMessage()
							{
								var result = false;

								var decMessageText = ReadXElement("./gmi:Body/dec:DecMessage", () => nsmgr.AddNamespace("dec", CNSWConstants.XmlNamespaces.ChinaPortDec));
								if (fileFormatChecker.CheckIsXMLDocument(decMessageText))
								{
									outputFullPath = GetNewTimeStampedFilePathIfNecessary(setting.SendFolder, messageFileName + ".xml");
									File.WriteAllText(outputFullPath, decMessageText, new UTF8Encoding(false));
									result = true;
								}

								return result;
							}

							string ReadXElement(string xPath, Action addExtraNameSpace = null, bool readText = false)
							{
								var result = string.Empty;

								addExtraNameSpace?.Invoke();

								var targetElement = rootElement.XPathSelectElement(xPath, nsmgr);
								if (targetElement != null)
								{
									using (var targetReader = targetElement.CreateReader())
									{
										targetReader.MoveToContent();
										result = readText ? targetReader.ReadInnerXml() : targetReader.ReadOuterXml();
									}
								}

								return result;
							}
						}
						else if (rootLocalName == "DecMessage")
						{
							eHubStreamDecompressed.SeekBegin();
							outputFullPath = GetNewTimeStampedFilePathIfNecessary(setting.SendFolder, messageFileName + ".xml");

							using (var fileStream = new FileStream(outputFullPath, FileMode.CreateNew))
							{
								if (fileFormatChecker.CheckIsXMLDocument(eHubStreamDecompressed))
								{
									eHubStreamDecompressed.SeekBegin();
									eHubStreamDecompressed.WriteTo(fileStream);
								}
							}
						}
					}
				}
			}
			if (!string.IsNullOrWhiteSpace(outputFullPath))
			{
				OnShowInformation($"Message with tracking ID: {trackingId} from eHub saved to: {outputFullPath}");
			}
			else
			{
				OnShowInformation($"Could not find valid data in content of message with Tracking Id: {trackingId} from eHub.");
			}
		}
	}
}
