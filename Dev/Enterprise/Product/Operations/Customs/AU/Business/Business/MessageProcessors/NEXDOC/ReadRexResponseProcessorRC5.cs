using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.NEXDOC.RC5.ReadRex;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ReadRexResponseProcessorRC5 : NEXDOCMessageProcessor<ReadRexResponse>
	{
		public ReadRexResponseProcessorRC5(LoggingInformation logger) : base(logger)
		{
		}

		protected override void ProcessMessageInternal(EDIMessage message, ReadRexResponse readRexResponse)
		{
			var rexNumber = GetRexNumberFromInterchangeHeader(message);
			if (!rexNumber.IsEmpty)
			{
				var factory = message.Factory;

				var invoice = FindInvoiceByRexNumber(factory, rexNumber);
				if (invoice == null)
				{
					var reader = new ReadRexResponseReaderRC5(factory, Logger);

					invoice = reader.GenerateInvoice(readRexResponse, rexNumber);
					invoice.JZ_GB = GlbBranch.CurrentBranch.PK;
				}

				UpdateValuesOnInvoice(invoice, readRexResponse);

				message.EM_LinkedObject = invoice.QuarantineExDocHeader;

				var detailsBuilder = new ZStringBuilder();
				detailsBuilder.Append("REX Number: " + rexNumber);
				detailsBuilder.AppendLine("Exporter Reference: " + invoice.JZ_ExporterReference);

				var details = detailsBuilder.ToStringWithDelimiterBetweenAppends("<br />");

				var email = CreateEmail("NEXDOC Notification Advice", "A Transferred REX " + rexNumber + " Has Been Received", details);
				SendAcknowledgementReport(null, email);
			}
			else
			{
				throw new InvalidFormatException("Invalid or Missing Rex Number. Cannot Process.");
			}
		}

		void UpdateValuesOnInvoice(JobComInvoiceHeader invoiceHeader, ReadRexResponse response)
		{
			var details = response.rexResponseDetails;
			if (details != null)
			{
				var docHeader = invoiceHeader.QuarantineExDocHeader;

				var permitNumber = details.permitNumber;
				if (!string.IsNullOrWhiteSpace(permitNumber))
				{
					docHeader.QH_ExportPermitNumber = ((ZString)permitNumber).SubstringSafe(0, CusEntryNumber.Schema.CE_EntryNumMaxLength);
				}

				if (invoiceHeader.IsInDatabase)
				{
					if (details.lastAmendDateTimeSpecified)
					{
						docHeader.QH_LastAmendDateTime = details.lastAmendDateTime.ToUniversalTime();
					}

					if (details.complianceStatusSpecified)
					{
						docHeader.RequestForPermitStatus = ((ZString)details.complianceStatus.ToString()).SubstringSafe(0, CusEntryNumber.Schema.CE_EntryStatusMaxLength);
					}
				}
			}

			var ednNumber = response.exportDetails?.sew?.edn;

			if (!string.IsNullOrWhiteSpace(ednNumber))
			{
				ednNumber = ((ZString)ednNumber).SubstringSafe(0, CusEntryNumber.Schema.CE_EntryNumMaxLength);

				var declaration = invoiceHeader.JobDeclaration;

				if (declaration != null && declaration.IsPersistent && declaration.EntryType == CANType.CustomsAuthorityNumber.Code)
				{
					declaration.DeclarationNumber = ednNumber;
				}

				using (new JobComInvoiceHeader.ResetLineValuesFromAddInfoSuspender(invoiceHeader))
				{
					invoiceHeader.AddInfo.ZA_EDN_Hidden = ednNumber;
				}
			}
		}
	}
}
