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
	public class SendPrelodgeAmendmentOperationalActionRunner
	{
		public SendPrelodgeAmendmentOperationalActionRunner(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			this.log = log;
			this.targets = targets;
		}

		public void SendPrelodgeAmendment(ZBool sendMessagesEvenWithMessageErrors)
		{
			var entries_DeltaG = targets.OfType<JobDeclaration>()
										.Where(x => x.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.DeltaG)
										.SelectMany(x => x.ActiveEntryHeaders)
										.Cast<CusEntryHeader>()
										.Where(x => x.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES050)
										.ToArray();

			var entries_DeltaIE = targets.OfType<JobDeclaration>()
										.Where(x => x.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.DeltaIE)
										.SelectMany(x => x.ActiveEntryHeaders)
										.Cast<CusEntryHeader>()
										.Where(x => x.CH_EntryStatus == DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered)
										.ToArray();

			var entriesThatCantBeAmended = targets.OfType<JobDeclaration>()
										.SelectMany(x => x.ActiveEntryHeaders)
										.Cast<CusEntryHeader>()
										.Where(x => (x.Declaration.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.DeltaG || x.CH_EntryStatus != EntryStatusDescriptionCodeList.Codes.ES050)
										&& (x.Declaration.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.DeltaIE || x.CH_EntryStatus != DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered))
										.ToArray();

			if (entries_DeltaG.Any())
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"{0} DeltaG entries with status {1} found", entries_DeltaG.Length, EntryStatusDescriptionCodeList.Descriptions.ES050);
				foreach (var entry in entries_DeltaG)
				{
					var errorCollector = new EU.Business.ErrorCollector();
					var objectToSend = new DeltaGJobDeclarationMessageSendingObject(entry);
					objectToSend.MessageType = entry.Declaration.JE_DeltaMode == OrgCusAccountDeltaGTypeList.Codes.G1 ? EntryActionCodeList.Codes.MAP : EntryActionCodeList.Codes.MDA;

					var declaration = entry.Declaration;
					declaration.RunPreSaveValidation();

					if (declaration.HasMessageErrors && !sendMessagesEvenWithMessageErrors)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Error, (NoResString)"Error creating {0} message for entry {1}\r\n{2}", objectToSend.MessageType, entry.CH_BGMReference, declaration.GetMessageErrors().ToUniqueMessageListString());
					}
					else
					{
						var messageSender = new DeltaGMessageSender(objectToSend, errorCollector);
						messageSender.Send();
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"Successfully created {0} message for entry {1}", objectToSend.MessageType, entry.CH_BGMReference);
					}
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"No Delta G entry with status {0} was found.", EntryStatusDescriptionCodeList.Descriptions.ES050);
			}

			if (entries_DeltaIE.Any())
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"{0} DeltaIE entries with status {1} found", entries_DeltaIE.Length, DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered);
				foreach (var entry in entries_DeltaIE)
				{
					var errorCollector = new EU.Business.ErrorCollector();
					var objectToSend = new DeltaIEJobDeclarationMessageSendingObject(entry);
					objectToSend.ChangeAcknowledgementIndicator = objectToSend.Lookups.MotivationForRectificationList.GetAllCodes().FirstOrDefault();
					objectToSend.MessageType = DeltaIESendMessageSubTypeList.Codes.AmendmentRequest;
					objectToSend.ShouldSend = true;
					var declaration = entry.Declaration;
					declaration.RunPreSaveValidation();

					if (declaration.HasMessageErrors && !sendMessagesEvenWithMessageErrors)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Error, (NoResString)"Error creating {0} message for entry {1}\r\n{2}", objectToSend.MessageType, entry.CH_BGMReference, declaration.GetMessageErrors().ToUniqueMessageListString());
					}
					else
					{
						var messageSender = new DeltaIEMessageSender(objectToSend, errorCollector);
						messageSender.Send();
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"Successfully created {0} message for entry {1}", objectToSend.MessageType, entry.CH_BGMReference);
					}
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"No Delta IE entry with status {0} was found.", DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered);
			}

			if ((entries_DeltaG.Any() || entries_DeltaIE.Any()) && entriesThatCantBeAmended.Any())
			{
				foreach (var entry in entriesThatCantBeAmended)
				{
					if (entry.Declaration.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.DeltaG && entry.Declaration.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.DeltaIE)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"Entry {0} is not available for amendment because its declaration application code is neither Delta G nor Delta IE", entry.CH_BGMReference);
					}
					var entryStatus = entry.CH_EntryStatus;
					if (entryStatus != EntryStatusDescriptionCodeList.Codes.ES050 && entryStatus != DeltaIEImportCusEntryStatusList.Codes.DeclarationRegistered)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, (NoResString)"Entry {0} is not available for amendment because its status is {1}.", entry.CH_BGMReference, entryStatus);
					}
				}
			}
		}

		readonly IOperationalActionSectionLog log;
		readonly BusinessObject[] targets;
	}
}
