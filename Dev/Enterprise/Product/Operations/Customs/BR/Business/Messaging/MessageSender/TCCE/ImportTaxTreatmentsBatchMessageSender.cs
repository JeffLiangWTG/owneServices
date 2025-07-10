using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.MessageBuilders.TTCE.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class ImportTaxTreatmentsBatchMessageSender
	{
		public ImportTaxTreatmentsBatchMessageSender(IEnumerable<JobComInvoiceLine> lines)
		{
			Argument.NotNull(lines, nameof(lines));
			Argument.GreaterThanZero(lines.Count(), nameof(lines));

			this.lines = lines;
		}

		protected readonly IEnumerable<JobComInvoiceLine> lines;

		BusinessObjectFactory Factory => lines.First().Factory;

		public int SendMessagesAndSave()
		{
			var messages = CreateMessages(lines);

			if (messages.Length > 0)
			{
				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					messages.DeleteAll();
					ZExceptionReporting.HandleSaveException(ex);
					return 0;
				}
			}

			return messages.Length;
		}

		EDIMessage[] CreateMessages(IEnumerable<JobComInvoiceLine> lines)
		{
			var messages = new List<EDIMessage>();

			lines = lines.Where(x => !x.JI_Tariff.IsEmpty && !x.JI_CountryOfOrigin.IsEmpty)
						.GroupBy(x => new { x.JI_Tariff, x.JI_CountryOfOrigin })
						.Select(x => x.First());

			foreach (var line in lines)
			{
				var message = Factory.New<BREDIMessage>();
				message.EM_MessageType = MessageTypeList.Codes.RTT;
				message.EM_MessageSubType = EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment;
				message.EM_LinkedObject = line.Declaration;
				message.EM_MessageText = new ImportTaxTreatmentsMessageBuilder(new ImportTaxTreatmentsProvider(line)).GetMessageText();
				message.EM_GP = line.Declaration.BrokerCertificate?.PK ?? ZGuid.Empty;

				messages.Add(message);
			}

			return messages.ToArray() ?? [];
		}
	}
}
