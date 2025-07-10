using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public class NctsDepartureMovementMessagingMenuProvider : EU.NCTS.GUI.NctsDepartureMovementMessagingMenuProvider
{
	public NctsDepartureMovementMessagingMenuProvider(NctsHeader header) : base(header)
	{
	}

	public NctsDepartureMovementMessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper) : base(header, ntcsHeaderUniversalMessagingHelper)
	{
	}

	NctsHeader ITNctsHeader => (NctsHeader)Header;

	public override IEnumerable<ZMenuItem> CreateMenuItems()
	{
		return base.CreateMenuItems().Concat(new[] { new ZMenuItem(ResString.GetMultilingualString("B6A5C152-16A4-4A2B-AA99-FAD3A29270FB", "Send Customs Messages"), SendCustomsMessagesMenuItemClick) });
	}

	protected override bool IsSendDepartureDeclarationMenuAllowed() => false;

	protected virtual NctsHeaderDepartureMessageSendingObjectParent GetMessageSendingObjectParent(NctsHeader nctsHeader) => new NctsHeaderDepartureMessageSendingObjectParent(nctsHeader);

	#region Implementation

	void SendCustomsMessagesMenuItemClick(object sender, EventArgs e)
	{
		if (PreSaveDeclaration())
		{
			ShowMessageSendingFormAndSendSelectedItem();
		}
	}

	void ShowMessageSendingFormAndSendSelectedItem()
	{
		try
		{
			SendSelectedObjectAndSaveMessage();
		}
		catch (ZSaveException e)
		{
			ZExceptionReporting.HandleSaveException(e);
		}
		catch (AidaXmlSignerException signerException)
		{
			Globals.Message.Show(signerException.Message);
		}
	}

	void SendSelectedObjectAndSaveMessage()
	{
		var sendingMessageFactory = new BusinessObjectFactory();
		var nctsInNewFactory = sendingMessageFactory.Load<NctsHeader>(ITNctsHeader.PK);
		var messageSendingObjectParent = GetMessageSendingObjectParent(nctsInNewFactory);
		using (var messageSendingForm = new Phase4MessageSendingForm(messageSendingObjectParent))
		{
			if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK && messageSendingObjectParent.SelectedSendingObjects.Any())
			{
				var selectedSendingObject = (NctsHeaderDepartureMessageSendingObject)messageSendingObjectParent.SelectedSendingObjects.Single();
				var messageSender = GetMessageSender(sendingMessageFactory, selectedSendingObject, nctsInNewFactory);
				var sendingResult = messageSender.Send();

				sendingMessageFactory.Save();
				Globals.Message.Show(EDIMenu.MessageSentSuccessfully);

				SaveMessageToFileIfNeeded(selectedSendingObject, sendingResult);
			}
		}
	}

	ITMessageSender GetMessageSender(BusinessObjectFactory sendingMessageFactory, NctsHeaderDepartureMessageSendingObject sendingObject, NctsHeader nctsHeader)
	{
		var messageCreationStrategy = GetStrategy(sendingMessageFactory, sendingObject);
		return new ITMessageSender(sendingMessageFactory, messageCreationStrategy, new NctsHeaderSendableCustomsEntry(nctsHeader));
	}

	protected virtual IOutgoingCustomsMessageCreationStrategy GetStrategy(BusinessObjectFactory factory, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		return SadOutgoingCustomsMessageCreationStrategy.GetStrategy(sendingObject.CustomsMessageSendingMode, factory, sendingObject);
	}

	void SaveMessageToFileIfNeeded(NctsHeaderDepartureMessageSendingObject sendingObject, ITEDIMessage message)
	{
		var messageExporter = CustomsMessageExporterFactory.GetMessageExporter(sendingObject.CustomsMessageSendingMode, message);
		messageExporter.SaveToFile(message);
	}

	#endregion
}
