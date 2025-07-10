using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.PushNotification;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class BRCLPCOPushNotificationMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCLPCOPushNotificationMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("65FF5FFE-6F4A-4E60-9962-9797943CB45E", "LPCO Push Notification");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.PUS };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.LPCO };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			var lpcoEvent = BRMessageHelper.DeserializeObject<LpcoEvent>(message.EM_MessageText);
			var lpcoNumber = new ZString(lpcoEvent?.numeroLPCO);
			var lpco = new CusLPCOHeader.Loader(message.Factory).GetLPCOByNumber(lpcoNumber);

			if (lpco == null)
			{
				Logger.LogError($"Unable to find a LPCO with numeroLPCO '{lpcoNumber}' for {message.EM_MessageType} message #{message.EM_MessageNum}");
			}

			return lpco;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusLPCOHeader lpco)
			{
				var lpcoEvent = BRMessageHelper.DeserializeObject<LpcoEvent>(message.EM_MessageText);
				var lpcoNumber = new ZString(lpcoEvent?.numeroLPCO);

				if (!string.IsNullOrEmpty(lpcoEvent.dataEvento) && lpcoEvent.novaSituacao != null)
				{
					var date = ZDateTimeOffset.TryParse(lpcoEvent.dataEvento, out var result) ? result : ZDateTimeOffset.Now;
					lpco.AddCustomsUpdateLog(date, customsReferenceNumber: lpcoNumber, status: lpcoEvent.novaSituacao.descricao, reason: lpcoEvent.justificativa);

					var customsStatus = BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(message.Factory, lpcoEvent.novaSituacao.id, EntryStatusListHelper.LPCOStatusPrefix);
					if (!customsStatus.IsEmpty)
					{
						lpco.CPH_CustomsStatus = customsStatus;
					}
					else
					{
						ErrorReporter.ReportOnce($"Unable to find LPCO Customs Status Code (starts with '{EntryStatusListHelper.LPCOStatusPrefix}') for novaSituacao '{lpcoEvent.novaSituacao.id}'.");
					}
				}
				if (!string.IsNullOrEmpty(lpcoEvent.dataExigencia) && lpcoEvent.situacao != null)
				{
					var date = ZDateTimeOffset.TryParse(lpcoEvent.dataExigencia, out var result) ? result : ZDateTimeOffset.Now;
					lpco.AddCustomsUpdateLog(date, customsReferenceNumber: lpcoNumber,
						status: lpcoEvent.situacao.descricao, action: lpcoEvent.exigencia);
				}
				if (!string.IsNullOrEmpty(lpcoEvent.dataCancelamento))
				{
					var date = ZDateTimeOffset.TryParse(lpcoEvent.dataCancelamento, out var result) ? result : ZDateTimeOffset.Now;
					lpco.AddCustomsUpdateLog(date, customsReferenceNumber: lpcoNumber, action: lpcoEvent.exigencia, freeText: (NoResString)"Exigência Cancelada");
				}
			}
		}
	}
}
