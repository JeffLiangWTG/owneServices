using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public abstract class BaseGoodsCatalogBatchMessageSender
	{
		public BaseGoodsCatalogBatchMessageSender(IEnumerable<CusGoodsCatalog> goodsCatalogs, IOperationalActionSectionLog log)
		{
			this.goodsCatalogs = Argument.NotNull(goodsCatalogs, nameof(goodsCatalogs));
			this.log = log;
		}

		readonly IEnumerable<CusGoodsCatalog> goodsCatalogs;
		readonly IOperationalActionSectionLog log;

		protected abstract bool IsOKToSendMessage(CusGoodsCatalog catalog);

		protected abstract string SearchingLogMessage { get; }
		protected abstract string SendAction { get; }

		public void SendMessagesAndInterchange()
		{
			if (goodsCatalogs.Any())
			{
				log?.SetSectionProgressMax(goodsCatalogs.Count() + 1);
				log?.NotifyFormat(OperationalActionLogErrorLevel.Informational, SearchingLogMessage);

				var messages = new NonDependentEDIMessageCollection(goodsCatalogs.First().Factory);

				foreach (var goodsCatalog in goodsCatalogs)
				{
					messages.AddRange(CreateMessages(goodsCatalog));
					log?.BumpSectionProgress();
				}

				var interchanges = messages.Count > 0 ? new BRBatchInterchangeProvider(messages).Interchanges : null;

				log?.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					Res.GetString("1A04F679-8DBF-4FBF-8EFC-C5AB6490D9B8", "{0} Message(s) have been generated and packed into {1} Interchange(s).",
					messages.Count, interchanges?.Length ?? 0));

				log?.BumpSectionProgress();
			}
		}

		EDIMessage[] CreateMessages(CusGoodsCatalog goodsCatalog)
		{
			EDIMessage[] messages = null;

			if (IsOKToSendMessage(goodsCatalog))
			{
				var sendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);
				sendingObject.Action = SendAction;
				sendingObject.BrokerCode = goodsCatalog.Owner is OrgHeader owner ? BROrgImpAddInfo.Get(owner).ZO_BrokerCode : ZString.Empty;

				if (sendingObject.BrokerCertificate?.IsValidCertificate ?? false)
				{
					messages = new GoodsCatalogMessageManager(sendingObject).GenerateMessages();
					Notify(OperationalActionLogErrorLevel.Informational, goodsCatalog, Res.GetString("FB3CBF97-D7E2-4C03-9DB6-BCAF955A82E2", "has been sent."));
				}
				else
				{
					Notify(OperationalActionLogErrorLevel.Warning, goodsCatalog, Res.GetString("C04FA91D-11AD-46D9-8B0B-89FCDDF787B8", "has not been sent because the digital certificate for the Catalog Staff mentioned in the Consignee is missing, expired, or invalid."));
				}
			}
			else
			{
				Notify(OperationalActionLogErrorLevel.Warning, goodsCatalog, Res.GetString("EEB8CD52-292E-4995-8C2D-93742475086E", "has not been sent because does not attend the searching criteria."));
			}

			return messages ?? Array.Empty<EDIMessage>();
		}

		void Notify(OperationalActionLogErrorLevel errorLevel, CusGoodsCatalog goodsCatalog, string message)
		{
			log?.NotifyFormat(errorLevel, "{0} {1}", GetLinkForGoodsCatalog(goodsCatalog), message);
		}

		LogControllerLink GetLinkForGoodsCatalog(CusGoodsCatalog goodsCatalog) => goodsCatalog == null ? null : new LogControllerLink(goodsCatalog.CGC_CatalogCode, ControllerIDs.Customs.GoodsCatalog, goodsCatalog.PK);
	}
}
