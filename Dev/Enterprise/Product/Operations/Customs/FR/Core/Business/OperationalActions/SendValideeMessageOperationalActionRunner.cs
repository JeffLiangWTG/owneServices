using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class SendValideeMessageOperationalActionRunner
	{
		public SendValideeMessageOperationalActionRunner(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			this.log = log;
			this.targets = targets;
		}

		public void SendValidee(ZBool sendMessagesEvenWithMessageErrors)
		{
			var entries = targets.OfType<JobDeclaration>()
										.SelectMany(x => x.ActiveEntryHeaders)
										.Cast<CusEntryHeader>()
										.Where(x => x.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES050 || x.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES055)
										.ToArray();

			var entriesThatCantBeValidate = targets.OfType<JobDeclaration>()
										.SelectMany(x => x.ActiveEntryHeaders)
										.Cast<CusEntryHeader>()
										.Where(x => x.CH_EntryStatus != EntryStatusDescriptionCodeList.Codes.ES050 && x.CH_EntryStatus != EntryStatusDescriptionCodeList.Codes.ES055)
										.ToArray();

			if (entries.Any())
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"{0} entries with status {1} or {2} found", entries.Length, EntryStatusDescriptionCodeList.Descriptions.ES050, EntryStatusDescriptionCodeList.Descriptions.ES055);

				foreach (var entry in entries)
				{
					var errorCollector = new EU.Business.ErrorCollector();
					var objectToSend = new DeltaGJobDeclarationMessageSendingObject(entry);
					objectToSend.MessageType = entry.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES050 ? EntryActionCodeList.Codes.VAA : EntryActionCodeList.Codes.EAV;

					var declaration = entry.Declaration;
					declaration.RunPreSaveValidation();

					if (declaration.HasMessageErrors && !sendMessagesEvenWithMessageErrors)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"Error creating {0} message for entry {1}\r\n{2}", objectToSend.MessageType, entry.CH_BGMReference, declaration.GetMessageErrors().ToUniqueMessageListString());
					}
					else
					{
						var messageSender = new DeltaGMessageSender(objectToSend, errorCollector);
						messageSender.Send();
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"Successfully created {0} message for entry {1}", objectToSend.MessageType, entry.CH_BGMReference);
					}
				}

				if (entriesThatCantBeValidate.Any())
				{
					foreach (var entry in entriesThatCantBeValidate)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"Entry {0} is not available for validation, the entry status is {1}.", entry.CH_BGMReference, entry.CH_EntryStatus);
					}
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"No entries with status {0} or {1} were found.", EntryStatusDescriptionCodeList.Descriptions.ES050, EntryStatusDescriptionCodeList.Descriptions.ES055);
			}
		}

		readonly IOperationalActionSectionLog log;
		readonly BusinessObject[] targets;
	}
}
