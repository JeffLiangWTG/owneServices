using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Enterprise.xTMessaging.Shared;

namespace Enterprise.RemotePrinting.Client
{
	public class CLSMSMessageReceiverController : CustomsMessageController
	{
		public CLSMSMessageReceiverController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken) { }

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName) => new CLSMSClientApplicationSettingManager(machineName, WebServiceClient);

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Back file name avoid repetition.")]
		protected override void Process()
		{
			OnShowInformation("CLSMS Message Reciever Process Started.");
			base.Process();
			OnShowInformation("CLSMS Message Reciever Process Stoped.");
		}

		protected override int ProcessCore(ICustomseHubClientSetting setting)
		{
			int numberOfMessages = 0;
			if (setting is ICLSMSClientApplicationSetting clsmsSetting)
			{
				var foldersToSearch = new[] { clsmsSetting.AcceptedFolder, clsmsSetting.InvalidFolder, clsmsSetting.RejectedFolder }
				.Where(path => !string.IsNullOrWhiteSpace(path));

				var responseXmlFiles = foldersToSearch.SelectMany(folder => Directory.EnumerateFiles(folder, responseFilePattern));
				if (responseXmlFiles.Any())
				{
					var msgClientProvider = CLSMSHelper.GetMsgClientProvider(clsmsSetting);
					using (var connector = new DirectxTConnector(msgClientProvider, new CLDirectxTMessagingConfig(), new XTLogger(OnShowInformation), new BasicSubmitMsgAttributeModifier(clsmsSetting.CW1LicenseKey)))
					{
						InitializeDXTConnector(connector);

						foreach (var responseXmlFilePath in responseXmlFiles)
						{
							try
							{
								var fileBytes = File.ReadAllBytes(responseXmlFilePath);
								using (var memoryStream = new MemoryStream(fileBytes))
								using (var bodyReader = new BinaryReader(memoryStream))
								{
									var messageType = "CHC";//Currently in DXT side, we only route by applicationCode, but the interface needs message type, so use 'CHC' at here.
									var applicationCode = "CLC";
									var fileName = Path.GetFileName(responseXmlFilePath);
									var sourceParty = clsmsSetting.ApplicationNodeName;
									var destinationParty = GetDestinationParty(clsmsSetting);
									var messageTrackingID = Guid.NewGuid().ToString();

									var xtMessageInfo = new BasicXtMessageInfo(
										applicationCode,
										messageType,
										sourceParty,
										destinationParty,
										Guid.NewGuid().ToString(),
										bodyReader);

									xtMessageInfo.XTMessageAttributes.Add("custom.FileName", fileName);
									(var isSuccess, var msgId, var error) = SendMessage(connector, xtMessageInfo);

									if (isSuccess)
									{
										numberOfMessages++;

										var sourceFile = responseXmlFilePath;
										var destinationFolder = clsmsSetting.UnknownFolder;
										if (Directory.Exists(destinationFolder))
										{
											var index = Guid.NewGuid().ToString();
											var newFileName = Path.Combine(destinationFolder, Path.GetFileNameWithoutExtension(responseXmlFilePath) + "_Processed_" + index + Path.GetExtension(responseXmlFilePath));
											try
											{
												File.Move(sourceFile, newFileName);
												OnShowInformation($"Message {responseXmlFilePath} processed successfully, and move to: {newFileName}");
											}
											catch (Exception ex)
											{
												OnShowInformation($"File move to backup folder fail: {sourceFile} processing failure: {ex.Message}");
											}
										}
									}
									else
									{
										OnShowInformation(error);
									}
								}
							}
							catch (Exception ex)
							{
								OnShowInformation($"Message {responseXmlFilePath} processing failure: {ex.Message}");
							}
						}
					}
				}
				else
				{
					OnShowInformation($"No xml file found in " + string.Join(", ", foldersToSearch));
				}
			}
			return numberOfMessages;
		}

		const string responseFilePattern = "*.xml";

		protected virtual void InitializeDXTConnector(DirectxTConnector connector)
		{
			connector.InitializeIfNeeded();
		}

		protected virtual (bool, long, string) SendMessage(DirectxTConnector connector, BasicXtMessageInfo xtMessageInfo)
		{
			return connector.SendInterchange(xtMessageInfo).GetAwaiter().GetResult();
		}

		protected virtual string GetDestinationParty(ICLSMSClientApplicationSetting clsmsSetting)
		{
			return clsmsSetting.CW1LicenseKey;
		}
	}
}
