using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.PushNotification;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCImportPushNotificationMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCImportPushNotificationMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E962730C-F3B4-4BBF-A741-9C5F4168E68A", "Import Push Notification");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.PUS };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Import };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			var duimpEvent = BRMessageHelper.DeserializeObject<DuimpEvent>(message.EM_MessageText);
			var identificationNumber = new ZString(duimpEvent?.identificacao?.numero);

			var entryHeader = new CusEntryHeader.Loader(message.Factory).GetEntryHeaderByMRNQuery(identificationNumber, MessageTypeList.Codes.CDI);
			if (entryHeader == null)
			{
				Logger.LogError($"Unable to find an Entry with Entry Number '{identificationNumber}' for {message.EM_MessageType} message #{message.EM_MessageNum}");
			}

			return entryHeader;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var duimpEvent = BRMessageHelper.DeserializeObject<DuimpEvent>(message.EM_MessageText);
				var identificationNumber = new ZString(duimpEvent?.identificacao?.numero);

				var date = DateTimeOffset.TryParse(duimpEvent.dataEvento, out var result) ? result : ZDateTimeOffset.Now;
				if (duimpEvent.diagnostico?.resultado?.Count > 0)
				{
					foreach (var resultValue in duimpEvent.diagnostico.resultado)
					{
						entryHeader.AddCustomsUpdateLog(date, identificationNumber, customsStatus: $"{resultValue.mensagem?.codigo} - {resultValue.mensagem?.texto}");
					}
				}
				else
				{
					entryHeader.AddCustomsUpdateLog(date, identificationNumber, customsStatus: duimpEvent.message);
				}

				if (!duimpEvent.canal.IsNullOrEmpty())
				{
					var riskChannel = RiskChannelList.GetRiskChannelValue(duimpEvent.canal);
					if (riskChannel.IsNullOrEmpty())
					{
						ErrorReporter.ReportOnce($"Unable to find Risk Channel Code for canal '{duimpEvent.canal}'.");
					}
					else
					{
						entryHeader.CH_RiskChannel = riskChannel;
					}
				}

				if (!duimpEvent.situacaoDuimp.IsNullOrEmpty())
				{
					var customsStatus = BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(message.Factory, duimpEvent.situacaoDuimp, EntryStatusListHelper.ImportEntryStatusPrefix);
					if (!customsStatus.IsEmpty)
					{
						entryHeader.CH_EntryStatus = customsStatus;
						entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, customsStatus, date);

						var version = duimpEvent.identificacao?.versao;
						if (!version.IsNullOrEmpty())
						{
							entryHeader.CH_AuthorityVersion = version;
						}
					}
					else
					{
						ErrorReporter.ReportOnce($"Unable to find Import Customs Status Code (starts with '{EntryStatusListHelper.ImportEntryStatusPrefix}') for situacaoDuimp '{duimpEvent.situacaoDuimp}'.");
					}
				}
			}
		}
	}
}
