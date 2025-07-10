using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AsycudaManifestHeader = Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader;
using ILDocMessageBuilder = Enterprise.Customs.IL.Manifest.Business.ILDOC271MessageBuilder;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public sealed class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("EE2D6A01-0DCF-462C-B500-B74FD7C10B7E", "IL Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var sendMessageMenuItem = GetSendMessageMenuItem();
			var sendSupportingDocumentsMenuItem = GetSendSupportingDocumentsMenuItem();
			var sendManifestQueryMenuItem = GetSendManifestQueryMenuItem();
			sendManifestQueryMenuItem.Visible = Header.IsSea;
			return new ZMenuItem[] { sendMessageMenuItem, sendSupportingDocumentsMenuItem, sendManifestQueryMenuItem };
		}

		ZMenuItem GetSendMessageMenuItem()
		{
			ResourceString caption = ResString.GetMultilingualString("F5CA7A07-E4D7-44E0-9505-9B2390E10F05", "Send to Customs");
			var zMenuItem = new ZMenuItem(caption);
			zMenuItem.Click += delegate
			{
				if (SaveAndContinue())
				{
					var header = (AsycudaManifestHeader)Header;
					var messageNumInCurrentFactory = header.Messages.Count;
					header.Messages.Reload(true);
					var messages = header.Messages;
					if (messages.Count != messageNumInCurrentFactory)
					{
						Globals.Message.ShowError(Res.GetString("2284F5E4-9A51-40A6-87BE-07610B7BC2B5", "A new message has been attached to this manifest header, please reopen the form before sending a message."));
					}
					else
					{
						var messageSendingObjectParent = header.MessageSendingConfiguration.GetNewMessageSendingObjectParent(header);
						using (var messageSendingForm = GetNewMessageSendingForm(messageSendingObjectParent))
						{
							var dialogRes = ZFormModaliser.ShowDialogAndDispose(messageSendingForm);
							if (dialogRes == DialogResult.OK)
							{
								var objectsToSend = (AsycudaManifestMessageSendingObject)messageSendingObjectParent.SelectedSendingObjects.Single();
								var messageBuilder = new ILMAN170MessageBuilder(header, objectsToSend);
								var messageManager = new ManifestMessageManager(header, messageBuilder);
								messageManager.SendMessage(new SendsMessagesToCustomsGUI());
							}
						}
					}
				}
			};
			return zMenuItem;
		}

		ZMenuItem GetSendSupportingDocumentsMenuItem()
		{
			var caption = ResString.GetMultilingualString("8A992DEC-0597-4E3C-B54C-19FB3231E070", "Send Supporting Documents");
			var zMenuItem = new ZMenuItem(caption);
			zMenuItem.Click += delegate
			{
				if (!SaveAndContinue())
				{
					return;
				}

				var headerWrapper = new AsycudaManifestHeaderWrapper((AsycudaManifestHeader)Header);
				if (headerWrapper.SupportingDocuments.Count > 0)
				{
					using var dialog = new SupportingDocumentsSelectionDialog(headerWrapper);
					ZFormModaliser.ShowDialogWithoutDispose(dialog, mainForm);
					if (dialog.DialogResult == DialogResult.OK)
					{
						var selectedSupportingDocuments = headerWrapper.SupportingDocuments.Where(s => s.IsSelected);
						foreach (var wrapper in selectedSupportingDocuments)
						{
							var builder = new ILDocMessageBuilder(Header, wrapper.SupportingDocument);
							var messageManager = new BasicMessageManager(Header, builder, ILEDIMessageSubTypeList.Codes.SupportingDocumentsRequest);
							messageManager.SendMessage(new SendsMessagesToCustomsGUI());
						}
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("F46389EE-BB92-4CA7-8DEB-2C9FC6C7248E", "There are no any supporting documents ready to be sent to customs (Status ‘Requested’ or Empty)."));
				}
			};

			return zMenuItem;
		}

		ZMenuItem GetSendManifestQueryMenuItem()
		{
			var caption = ResString.GetMultilingualString("BDCE6D7D-6BBB-494D-95AF-785800D87A9C", "Send Manifest Query");
			var zMenuItem = new ZMenuItem(caption);
			zMenuItem.Click += delegate
			{
				if (!SaveAndContinue())
				{
					return;
				}
				var header = (AsycudaManifestHeader)Header;
				var messageSendingObjectParent = header.MessageSendingConfiguration.GetNewQueryMessageSendingObjectParent(header);
				using var dialog = new ManifestQuerySelectionDialog(messageSendingObjectParent);
				ZFormModaliser.ShowDialogWithoutDispose(dialog, mainForm);
				if (dialog.DialogResult == DialogResult.OK)
				{
					var manifestQueryMessageToSend = messageSendingObjectParent.SendingObjectsCollection.Cast<AsycudaManifestQueryMessageSendingObject>().FirstOrDefault(r => r.ShouldSend);
					if (manifestQueryMessageToSend == null)
					{
						return;
					}
					var builder = new ILMAN820MessageBuilder(header, manifestQueryMessageToSend);
					var messageManager = new BasicMessageManager(Header, builder, ILEDIMessageSubTypeList.Codes.ManifestQueryRequest);
					messageManager.SendMessage(new SendsMessagesToCustomsGUI());
				}
			};
			return zMenuItem;
		}

		bool SaveAndContinue()
		{
			return CustomsPlugIn.FormPreSaved(Header, mainForm);
		}

		MessageSendingFormWithValidationDetails GetNewMessageSendingForm(BaseMessageSendingObjectParent messageSendingObjectParent) => new MessageSendingFormWithValidationDetails(messageSendingObjectParent);
	}
}
