using System;
using System.Linq;
using System.Text;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE818MessageProcessor : EMCSMessageProcessor<IIE818>
	{
		public IE818MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("21B8D10C-26DA-45EE-9DF0-3856028EC89E", "EMCS IE818 Message Processor");

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
			SendEmailNotification(message, Res.GetString("D6DE03CA-D30C-41D4-93AE-DDA0470C6A39", "EMCS Report of Receipt"), false, provider, null, GetEmailBody);
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent exciseMovementEad)
		{
			var provider = (IIE818)dataProvider;
			var receiptConclusion = provider.GlobalConclusionOfReceipt;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("3412465F-630B-417D-B776-BC850D780912", "Your EMCS Declaration for Job {0} received a report of receipt. For details please follow the link to the Job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("97F2501E-D91A-4C8F-8AEA-C41A6FCFEF2C", "ARC"), provider.ExciseMovementEad.AdministrativeReferenceCode);
			tableCreator.WriteRow(Res.GetString("38EEE364-0C8C-430D-A59B-F51B2CD04233", "Global Conclusion of Receipt"), receiptConclusion + " - " + declaration.Factory.GetCachedValue<EMCSGBGlobalConclusionOfReceiptList>().GetDescriptionFromCode(receiptConclusion));
			htmlBody.Append(tableCreator.ToHtml());

			if (provider.ReportOfReceipts.Any())
			{
				htmlBody.Append("<br />");
				var reasonHtmlTableCreator = new HtmlTableCreator(new[]
				{
					Res.GetString("A4D72CAF-D71B-458C-8271-B037054FC8AA", "Line No."),
					Res.GetString("65492470-9412-4537-8666-256E24D409D8", "Unsatisfactory Reason"),
					Res.GetString("669D1666-92AF-4403-9A14-FD1E94663E55", "Information")
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
