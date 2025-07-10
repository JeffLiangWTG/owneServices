using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using EMCSReceiptReasonCodeList = Enterprise.Customs.EU.EMCS.Business.EMCSReceiptReasonCodeList;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED818MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED818>, IED818>
	{
		public ED818MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("32A07A09-5550-478C-B1F2-1496300F4B42", "EMCS ED818 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED818> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var exciseMovement = provider.ExciseMovement;
				result = GetDeclarationFromEADNumber(message, exciseMovement.AdministrativeReferenceCode, provider.MessageGroup, exciseMovement.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED818> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.COM;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			if (IsConsigneeDeclaration(messageGroup))
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			}

			if (IsConsignorDeclaration(messageGroup) && provider.ReportOfReceipts.Any())
			{
				foreach (var reportOfReceipt in provider.ReportOfReceipts)
				{
					var invoiceLine = emcsDeclaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().SingleOrDefault(x => x.JI_LineNo == ZShort.ParseSafe(reportOfReceipt.LineNumber, ZShort.Zero));
					if (invoiceLine != null)
					{
						if (reportOfReceipt.IndicatorOfShortageOrExcess == "E")
						{
							invoiceLine.JI_CustomsQuantity = invoiceLine.JI_CustomsQuantity + reportOfReceipt.ObservedQuantity;
						}
						else if (reportOfReceipt.IndicatorOfShortageOrExcess == "S")
						{
							invoiceLine.JI_CustomsQuantity = invoiceLine.JI_CustomsQuantity - reportOfReceipt.ObservedQuantity;
						}
						invoiceLine.Outturn.C5_RejectedQuantity = reportOfReceipt.RefusedQuantity;

						foreach (var reason in reportOfReceipt.UnsatisfactoryReasons)
						{
							var addReason = invoiceLine.Outturn.ReportOfReceiptReasons.AddNew();
							addReason.CY_Code = reason.ReasonCode;
							addReason.CY_Data = reason.ComplementaryInformation;
						}
					}
				}
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("232EA544-7B70-4A0B-AB4F-56082BB57A69", "EMCS Report of Receipt")
				, GetEmailBody(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ExciseMovement.AdministrativeReferenceCode);
		}

		ZString GetEmailBody(EMCSJobDeclaration emcsDeclaration, IED818 dataProvider)
		{
			var receiptConclusion = dataProvider.GlobalConclusionOfReceipt;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("2856345A-9778-4EAD-991D-2A9FAA77642C", "Your EMCS Declaration for Job {0} received a report of receipt. For details please follow the link to the Job.", emcsDeclaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("B30FB8A3-F6C3-4D45-A3B3-23AD9FB59429", "ARC"), dataProvider.ExciseMovement.AdministrativeReferenceCode);
			tableCreator.WriteRow(Res.GetString("16EA15F2-7F6C-4148-83E4-127D19B142B4", "Global Conclusion of Receipt"), receiptConclusion + " - " + emcsDeclaration.Factory.GetCachedValue<EmcsGlobalConclusionOfReceiptList>().GetDescriptionFromCode(receiptConclusion));
			htmlBody.Append(tableCreator.ToHtml());

			if (dataProvider.ReportOfReceipts.Any())
			{
				htmlBody.Append("<br />");
				var reasonHtmlTableCreator = new HtmlTableCreator(new[]
				{
					Res.GetString("F23048D1-BE3C-407F-AAC7-B5AE22BED51A", "Line No."),
					Res.GetString("EE881D59-2CF8-44EB-A55B-E94E72038AF3", "Unsatisfactory Reason"),
					Res.GetString("0C6D9814-C2F4-47FE-9963-A73D5ADADDAE", "Information")
				});

				foreach (var reportOfReceipt in dataProvider.ReportOfReceipts)
				{
					foreach (var reason in reportOfReceipt.UnsatisfactoryReasons)
					{
						reasonHtmlTableCreator.WriteRow(reportOfReceipt.LineNumber
						, reason.ReasonCode + " - " + emcsDeclaration.Factory.GetCachedValue<EMCSReceiptReasonCodeList>().GetDescriptionFromCode(reason.ReasonCode)
						, reason.ComplementaryInformation);
					}
				}
				htmlBody.Append(reasonHtmlTableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}
