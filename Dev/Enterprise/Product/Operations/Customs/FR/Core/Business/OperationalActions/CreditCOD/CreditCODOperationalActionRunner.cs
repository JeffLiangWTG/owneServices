using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Logging;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.MessagesWrappers.COD;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Customs.FR.Messaging.MessageBuilders.COD;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class CreditCODOperationalActionRunner
	{
		public CreditCODOperationalActionRunner(IOperationalActionSectionLog log)
		{
			logger = new OperationalActionSectionLogWrapper(log);
		}
		readonly ICommonLogger logger;

		public void CreditCOD(CreditCODDataObjectCollection items)
		{
			var validItems = items.Where(x => x.ReleasingEntryHeader != null).ToList();
			var invalidItems = items.Except(validItems).Cast<CreditCODDataObject>().ToList();

			var action3Items = validItems.Where(x => x.CreditMethod == CreditMethodList.Codes.CreditPreviousEntry).ToList();
			var action1Items = validItems.Except(action3Items).ToList();

			foreach (var group in action3Items.GroupBy(x => x.ReleasingEntryHeader))
			{
				var gens = group.Select(x => new GenWrapper(x)).Cast<IGen>().ToList();
				var message = CreateCODMessage(group.Key, new CODWrapper(group.Key, "3", new List<IArticle>(), gens));
				logger.LogFormat(LogType.Information, (EZC.NoResString)"Create COD message {0} in entry header {1} successfully", message.EM_MessageNum, group.Key.CH_BGMReference);
			}

			foreach (var group in action1Items.GroupBy(x => x.ReleasingEntryHeader))
			{
				var articles = group.Select(x => new ArticleWrapper(x, null)).Cast<IArticle>().ToList();
				var message = CreateCODMessage(group.Key, new CODWrapper(group.Key, "1", articles, new List<IGen>()));
				logger.LogFormat(LogType.Information, (EZC.NoResString)"Create COD message {0} in entry header {1} successfully", message.EM_MessageNum, group.Key.CH_BGMReference);
			}

			foreach (var item in invalidItems)
			{
				logger.LogFormat(LogType.Information, (EZC.NoResString)"There were no entry header found for the entry number {0}.", item.ReleasingEntryReference);
			}
		}

		EDIMessage CreateCODMessage(CusEntryHeader entryHeader, ICOD codWrapper)
		{
			var messageText = new CODSendMessageBuilder(codWrapper, new EU.Business.ErrorCollector(), TransactionTypes.Original).GetMessage();

			var factory = entryHeader.Factory;
			var ediMessage = factory.New<CODSendMessage>();
			ediMessage.EM_MessageText = messageText;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.MessageNumberStrategy = new FRMessageNumberStrategy(factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			ediMessage.EM_LinkedObject = entryHeader;

			entryHeader.Logs.AddNew(Events.CustomsGuaranteeUpdated, string.Format("COD | {0} Released", entryHeader.Declaration.CustomsGuarantee?.CPH_Number ?? ZString.Empty), ZDateTimeOffset.Now);

			factory.Save();

			return ediMessage;
		}
	}
}
