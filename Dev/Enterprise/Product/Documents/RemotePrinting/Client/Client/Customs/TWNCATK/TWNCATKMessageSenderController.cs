using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.eHub.Common.Extensions;

namespace Enterprise.RemotePrinting.Client
{
	public class TWNCATKMessageSenderController : CustomsMessageController
	{
		public TWNCATKMessageSenderController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
		{
			fileFormatChecker = new MessageFileFormatChecker(this);
		}

		readonly MessageFileFormatChecker fileFormatChecker;

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
		{
			return new TWNCATKClientApplicationSettingManager(machineName, WebServiceClient);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305")]
		protected override int ProcessCore(ICustomseHubClientSetting setting)
		{
			var result = 0;

			if (setting is ITWNCATKClientApplicationSetting twSetting)
			{
				using (var eHubServiceClient = SettingManager.CreateEHubServiceClient(twSetting))
				{
					var response = eHubServiceClient?.ReceiveStream();
					if (response != null && response.Messages.Length > 0)
					{
						result = response.Messages.Length;
						var trackingID = response.TrackingID;

						foreach (var message in response.Messages)
						{
							using (var stream = message.MessageStream.DecodeAndDecompress())
							{
								if (fileFormatChecker.CheckIsXMLDocument(stream))
								{
									WriteToLocalAsUTF8Xml(stream, twSetting.SendToFolder, message.FileName);
									OnShowInformation($"Message with tracking ID: {message.MessageTrackingID} finalised.");
								}
							}
						}

						eHubServiceClient.Finalise(trackingID);
					}
					else
					{
						OnShowInformation($"No message received from eHub.");
					}
				}
			}

			return result;
		}
	}
}
