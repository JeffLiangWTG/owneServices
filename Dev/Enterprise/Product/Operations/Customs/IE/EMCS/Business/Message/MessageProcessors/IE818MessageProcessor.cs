using System;
using System.Linq;
using System.Text;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.tcl;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class IE818MessageProcessor : EMCSMessageProcessor<IIE818>
	{
		public IE818MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("575ECB03-03F8-495A-B912-E1A823BCA3FD", "EMCS IE818 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE818 provider)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EntryStatusList.Codes.COM;

			if (emcsDeclaration.IsConsignee)
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			}

			if (emcsDeclaration.IsConsignor && provider.ReportOfReceipts.Any())
			{
				foreach (var reportOfReceipt in provider.ReportOfReceipts)
				{
					var invoiceLine = emcsDeclaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().SingleOrDefault(x => x.JI_LineNo == ZShort.ParseSafe(reportOfReceipt.LineNumber, ZShort.Zero));
					if (invoiceLine != null)
					{
						if (reportOfReceipt.IndicatorOfShortageOrExcess == nameof(IndicatorOfShortageOrExcess.E))
						{
							invoiceLine.JI_CustomsQuantity += reportOfReceipt.ObservedQuantity;
						}
						else if (reportOfReceipt.IndicatorOfShortageOrExcess == nameof(IndicatorOfShortageOrExcess.S))
						{
							invoiceLine.JI_CustomsQuantity -= reportOfReceipt.ObservedQuantity;
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

			linkedEMCSDeclaration = emcsDeclaration;
			SendEmailNotification(message, Res.GetString("3F4F1E27-1E13-43F4-88CE-F57976B05691", "EMCS Report of Receipt"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent exciseMovementEad)
		{
			var provider = (IIE818)dataProvider;
			var receiptConclusion = provider.GlobalConclusionOfReceipt;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("7C3B353A-537D-4CED-8395-C541A53CC6FC", "Your EMCS Declaration for Job {0} received a report of receipt. For details please follow the link to the Job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("DE321873-1E9A-4EAE-BB3F-813B0E15FC3E", "ARC"), provider.ExciseMovementEad.AdministrativeReferenceCode);
			tableCreator.WriteRow(Res.GetString("4CBEB407-C8C2-4B6F-B31F-FE2F7A622A69", "Global Conclusion of Receipt"), receiptConclusion + " - " + declaration.Factory.GetCachedValue<EMCSGlobalConclusionOfReceiptList>().GetDescriptionFromCode(receiptConclusion));
			htmlBody.Append(tableCreator.ToHtml());

			if (provider.ReportOfReceipts.Any())
			{
				htmlBody.Append("<br />");
				var reasonHtmlTableCreator = new HtmlTableCreator(new[]
				{
					Res.GetString("E05DA336-D224-4ABB-89C4-AD8F3FB987DE", "Line No."),
					Res.GetString("D2D096A9-0D34-4F42-8927-A77A87530B1F", "Unsatisfactory Reason"),
					Res.GetString("CC9A9BEE-2B90-4212-9392-62295CE40043", "Information")
				});

				foreach (var reportOfReceipt in provider.ReportOfReceipts)
				{
					foreach (var reason in reportOfReceipt.UnsatisfactoryReasons)
					{
						reasonHtmlTableCreator.WriteRow(reportOfReceipt.LineNumber
						, reason.ReasonCode + " - " + declaration.Factory.GetCachedValue<EMCSReceiptReasonCodeList>().GetDescriptionFromCode(reason.ReasonCode)
						, reason.ComplementaryInformation);
					}
				}
				htmlBody.Append(reasonHtmlTableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}
