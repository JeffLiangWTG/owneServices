using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public class EDIMenu : Customs.GUI.EDIMenu
{
	public new JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.Declaration; }
		set { base.Declaration = value; }
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		var declaration = Declaration;
		sendCustomsMessagesMenuItem.Visible = declaration != null && !declaration.ShowSubmitMenuItem;
	}

	protected virtual JobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent(JobDeclaration declaration)
	{
		return declaration.IsUCC6
			? new Ucc6JobDeclarationMessageSendingObjectParent(declaration)
			: new JobDeclarationMessageSendingObjectParent(declaration);
	}

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();
		sendCustomsMessagesMenuItem = new ZMenuItem(SendCustomsMessagesLabel, SendCustomsMessagesMenuItem_Click);
		MenuItems.Add(sendCustomsMessagesMenuItem);
	}

	protected Form GetNewMessageSendingForm(JobDeclarationMessageSendingObjectParent sendingObjectParent)
	{
		return Declaration.IsUCC6
			? new MessageSendingFormUcc6(sendingObjectParent)
			: new MessageSendingForm(sendingObjectParent);
	}

	protected override bool DisplayGenerateEntriesMenuOption => true;

	protected virtual IOutgoingCustomsMessageCreationStrategy GetMessageCreationStrategy(BusinessObjectFactory factory, ZString customsMessageSendingMode, JobDeclarationMessageSendingObject sendingObject)
	{
		return Declaration.IsUCC6
			? new AidaXmlOutgoingCustomsMessageCreationStrategy(factory, (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject)
			: SadOutgoingCustomsMessageCreationStrategy.GetStrategy(customsMessageSendingMode, factory, (ISadOutgoingCustomsMessageGeneratorValuesProvider)sendingObject);
	}

	void SendCustomsMessagesMenuItem_Click(object sender, EventArgs e)
	{
		SendCustomsMessages();
	}

	void SendCustomsMessages()
	{
		if (PerformMergeIfNeeded() && PreSaveDeclaration(Declaration))
		{
			try
			{
				SendAndSaveCustomMessage();
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
			catch (AidaXmlSignerException signerEx)
			{
				Globals.Message.Show(signerEx.Message);
			}
		}
	}

	void SendAndSaveCustomMessage()
	{
		var sendingMessageFactory = new BusinessObjectFactory();
		var declarationInNewFactory = LoadDeclarationInNewFactory(sendingMessageFactory);
		var decWrapper = GetJobDeclarationMessageSendingObjectParent(declarationInNewFactory);
		using (var form = GetNewMessageSendingForm(decWrapper))
		{
			if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK
				&& Customs.Business.MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(declarationInNewFactory)
				&& declarationInNewFactory.CheckCredit()
				&& decWrapper.SelectedSendingObjects.Any())
			{
				var sendingObject = decWrapper.SelectedSendingObjects.Cast<JobDeclarationMessageSendingObject>().Single();
				var sentMessage = SendMessage(sendingMessageFactory, sendingObject);

				if (sentMessage != null)
				{
					sendingMessageFactory.Save();
					Globals.Message.Show(MessageSentSuccessfully);

					SaveMessageToFileIfNeeded(sendingObject, sentMessage);
				}
			}
		}
	}

	ITEDIMessage SendMessage(BusinessObjectFactory sendingMessageFactory, JobDeclarationMessageSendingObject sendingObject)
	{
		ITEDIMessage message = null;
		bool sendingMessageFunction()
		{
			var messageSender = GetMessageSender(sendingObject, sendingMessageFactory);
			message = messageSender.Send();
			return message != null;
		}

		var entryHeader = sendingObject.Header;
		var declaration = entryHeader.Declaration;

		if (entryHeader.IsIntoWarehouseWarehousing || entryHeader.IsOutOfWarehouseWarehousing)
		{
			var messageAction = sendingObject.MessageType.ToString() switch
			{
				EDIMessageTypeList.Codes.NewDeclaration => Customs.Business.WarehouseExtensions.MessageAction.Original,
				EDIMessageTypeList.Codes.Cancellation => Customs.Business.WarehouseExtensions.MessageAction.Withdrawal,
				EDIMessageTypeList.Codes.Amendment => Customs.Business.WarehouseExtensions.MessageAction.Amendment,
				_ => throw new NotSupportedException()
			};

			declaration.SendMessageWithBondedWarehouseAutomation(entryHeader, entryHeader.GetInventoryAutomationAction(), () => sendingMessageFunction(), messageAction);
		}
		else
		{
			sendingMessageFunction();
		}
		return message;
	}

	void SaveMessageToFileIfNeeded(JobDeclarationMessageSendingObject sendingObject, ITEDIMessage message)
	{
		var messageExporter = CustomsMessageExporterFactory.GetMessageExporter(sendingObject.CustomsMessageSendingMode, message);
		messageExporter.SaveToFile(message);
	}

	JobDeclaration LoadDeclarationInNewFactory(BusinessObjectFactory sendingMessageFactory)
	{
		var declarationInNewFactory = sendingMessageFactory.Load<JobDeclaration>(Declaration.PK);
		declarationInNewFactory.MessageInitiator = Declaration.MessageInitiator;
		return declarationInNewFactory;
	}

	ITMessageSender GetMessageSender(JobDeclarationMessageSendingObject sendingObject, BusinessObjectFactory sendingMessageFactory)
	{
		var messageCreationStrategy = GetMessageCreationStrategy(sendingMessageFactory, sendingObject.CustomsMessageSendingMode, sendingObject);

		var entryHeaderInSendingMessageFactory = sendingMessageFactory.Load<CusEntryHeader>(sendingObject.Header.PK);
		return new ITMessageSender(sendingMessageFactory, messageCreationStrategy, entryHeaderInSendingMessageFactory);
	}

	bool PerformMergeIfNeeded()
	{
		var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
		return !needMerge || PerformMerge();
	}

	ZMenuItem sendCustomsMessagesMenuItem;

	static MultilingualString SendCustomsMessagesLabel => ResString.GetMultilingualString("9D3B1B7D-E18B-485F-BE24-430BAD0B1DEC", "Send Customs Messages");

	public static MultilingualString MessageSentSuccessfully => ResString.GetMultilingualString("300B6B6E-BDF0-4BAB-8488-81CC223FF0CF", "Message sent successfully");
}
