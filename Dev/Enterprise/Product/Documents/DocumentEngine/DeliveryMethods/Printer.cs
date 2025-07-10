using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	class Printer : QueuedForBatchProcessor
	{
		public Printer(DocDeliveryPrintDetails printerDetails)
		{
			this.printerDetails = Argument.NotNull(printerDetails, "printerDetails");
		}

		protected readonly DocDeliveryPrintDetails printerDetails;

		protected override PrintType PrintType
		{
			get { return PrintType.PRN; }
		}

		protected override bool ConsolidateReports
		{
			get { return false; }
		}

		protected override void SetAdditionalProperties(StmPrintJob printJob, DeliveryInfo deliveryInfo)
		{
			if (deliveryInfo.PrintQueue != null)
			{
				printJob.SP_SQ = deliveryInfo.PrintQueue.PK;
				var printerLanguage = PrinterLanguages.Base.Create(deliveryInfo.PrintQueue.SQ_PrintLanguage);
				printJob.SP_EscapeSequence = printerLanguage.PaperFeedSequence(deliveryInfo.TrailingSpace);
			}
			else if (printerDetails.PrintQueue != null)
			{
				printJob.SP_SQ = printerDetails.PrintQueuePK;
				var printerLanguage = PrinterLanguages.Base.Create(printerDetails.PrintQueue.SQ_PrintLanguage);
				printJob.SP_EscapeSequence = printerLanguage.PaperFeedSequence(deliveryInfo.TrailingSpace);
			}
			else
			{
				ReportNoPrinterError(deliveryInfo);
			}

			printJob.SP_Copies = (ZShort)(printerDetails.NumberOfCopies * deliveryInfo.Copies);
		}

		#region SuppressResourceStringsCheckRegion

		void ReportNoPrinterError(DeliveryInfo deliveryInfo)
		{
			var builder = new ZStringBuilder();

			builder.Append("No Printer for [" + deliveryInfo.Name + "]. A printer must be specified when using a delivery method of 'PRN'");

			if (deliveryInfo.Instructions == null)
			{
				builder.Append("Delivery instructions on delivery info are not set");
			}
			else
			{
				builder.Append("Destination [" + deliveryInfo.Instructions.Destination + "]");

				foreach (DocDeliveryContact recipient in deliveryInfo.Instructions.Recipients)
				{
					builder.Append("Recipient: name [" + recipient.Name + "] company [" + recipient.CompanyName + "] delivery method [" + recipient.DeliveryMethod + "]");
				}

				if (deliveryInfo.Instructions.PrinterDelivery != printerDetails)
				{
					builder.Append("Printer details do not match instruction");
				}
			}

			if (printerDetails.PrintQueuePK.IsValid)
			{
				builder.Append("Printer was requested but could not be loaded");
			}
			else
			{
				builder.Append("Printer has not been set");
			}
			builder.Append("Please close the form and reopen it again");

			throw new InvalidPrinterException(builder.ToStringWithDelimiterBetweenAppends(". "));
		}

		#endregion
	}
}
