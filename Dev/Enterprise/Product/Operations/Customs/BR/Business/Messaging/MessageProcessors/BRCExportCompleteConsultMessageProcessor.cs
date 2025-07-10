using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCExportCompleteConsultMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCExportCompleteConsultMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0C6CC702-0002-432D-8938-60217A69480C", "DU-E Complete Consult");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CDE };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.CompleteConsult };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var due = BRMessageHelper.DeserializeObject<Due>(message.EM_MessageText);
				if (due != null)
				{
					if (!due.canal.IsNullOrEmpty())
					{
						var riskChannel = RiskChannelList.GetRiskChannelValue(due.canal);
						if (riskChannel.IsNullOrEmpty())
						{
							ErrorReporter.ReportOnce($"Unable to find Risk Channel Code for canal '{due.canal}'.");
						}
						else
						{
							entryHeader.CH_RiskChannel = riskChannel;
						}
					}
					if (!due.situacaoDoTratamentoAdministrativo.IsNullOrEmpty())
					{
						var administrativeSituation = BRAdministrativeStatusList.MapToCWCode(due.situacaoDoTratamentoAdministrativo);
						if (administrativeSituation.IsNullOrEmpty())
						{
							ErrorReporter.ReportOnce($"Unable to find Export Administrative Status Code for situacaoDoTratamentoAdministrativo '{due.situacaoDoTratamentoAdministrativo}'.");
						}
						else
						{
							entryHeader.CH_AdministrativeStatus = administrativeSituation;
						}
					}
					var code = due.situacoesDaCarga?.FirstOrDefault()?.codigo.ToString() ?? string.Empty;
					if (!code.IsNullOrEmpty())
					{
						if (!entryHeader.Lookups.CargoStatusList.ContainsCode(code))
						{
							ErrorReporter.ReportOnce($"Unable to find Export Cargo Situation Code for situacoesDaCarga '{code}'.");
						}
						else
						{
							entryHeader.CH_CargoStatus = code;
						}
					}
					if (!due.chaveDeAcesso.IsNullOrEmpty() && due.dataDeRegistro != DateTime.MinValue)
					{
						entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
					}
				}
			}
		}
	}
}
