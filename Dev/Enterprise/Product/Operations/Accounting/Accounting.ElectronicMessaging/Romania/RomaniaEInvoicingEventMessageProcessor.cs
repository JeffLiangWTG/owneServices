using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	internal class RomaniaEInvoicingEventMessageProcessor : GlobalEInvoicingEventMessageProcessor
	{
		public RomaniaEInvoicingEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(eventMessageProcessorData, countryEInvoicingObjectFactory)
		{
			TransactionPivot = invoiceBatch.TransactionPivots[0];
			Transaction = invoiceBatch.Factory.Load<InvoicingBase>(TransactionPivot?.AIP_ParentID ?? ZGuid.Empty);
		}

		AccEInvoicingTransactionPivot TransactionPivot { get; }
		InvoicingBase Transaction { get; }
		ZString EInvoicingNumber => TransactionPivot?.ParentTransactionHeader?.EInvoicingAuthorisationNumber ?? ZString.Empty;

		public override void Process()
		{
			var messageSubType = universalEvent.EventParameters?.MessageSubType ?? ZString.Empty;
			var authorisationData = universalEvent.ContextCollection?.FirstOrDefault(c => c.Type == EventContextTypeCode.AHF_AuthorisationData);
			var encodedZipFile = authorisationData?.Value ?? ZString.Empty;

			if (messageSubType == RomaniaEInvoiceAPICommandList.Codes.QueryInvoiceRequest && !encodedZipFile.IsEmpty)
			{
				try
				{
					HandleZipFile(encodedZipFile);
				}
				catch (XmlException xmlEx)
				{
					logger.LogBoth(LogType.Warning, Res.GetString("RomaniaEInvoicingEventMessageProcessor|XmlParsingException", "Cannot parse invoice XML in returned ZIP. {0} : {1}", xmlEx.GetType().Name, xmlEx.Message));
				}
			}

			base.Process();
		}

		protected override void ProcessIAKEventMessage()
		{
			if (string.IsNullOrWhiteSpace(EventAndDatabaseTransaction.EventData.Counter))
			{
				EventAndDatabaseTransaction.EventData.Counter = RomaniaConstants.DefaultAuthorizationCounter;
			}

			base.ProcessIAKEventMessage();

			if (EventAndDatabaseTransaction.EventData.PivotStatus == EInvoicingPivotState.Delivered && invoiceBatch.AIB_Status == EInvoicingBatchState.Sent)
			{
				invoiceBatch.AIB_Status = EInvoicingBatchState.Ready;
			}
		}

		void HandleZipFile(ZString encodedZipFile)
		{
			if (EInvoicingNumber.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("RomaniaEInvoicingEventMessageProcessor|EmptyEInvNumber", "E-Invoicing Number is empty. The decompression process terminated."));
				return;
			}

			var extractor = new ZipExtractor();
			using (var zipFileStream = new MemoryStream(Convert.FromBase64String(encodedZipFile)))
			using (var invoiceFileStream = new MemoryStream())
			using (var semnaturaFileStream = new MemoryStream())
			{
				var invoiceFileName = $"{EInvoicingNumber}.xml";
				var semnaturaFileName = $"semnatura_{EInvoicingNumber}.xml";

				extractor.ExtractZipStream(zipFileStream, invoiceFileStream, invoiceFileName);
				extractor.ExtractZipStream(zipFileStream, semnaturaFileStream, semnaturaFileName);

				if (invoiceFileStream.Length != 0)
				{
					var invoiceContent = invoiceFileStream.WriteToString();
					ExtractErrorMessageIfExists(invoiceContent);
				}
				else
				{
					logger.LogBoth(LogType.Warning, Res.GetString("RomaniaEInvoicingEventMessageProcessor|NoInvoiceFile", "Invoice File {0} does not exist in the returned zip file, or the filename does not match", invoiceFileName));
				}

				if (semnaturaFileStream.Length != 0)
				{
					SaveSemnaturaToEDocs(semnaturaFileStream, semnaturaFileName);
				}
				else
				{
					logger.LogBoth(LogType.Warning, Res.GetString("RomaniaEInvoicingEventMessageProcessor|NoSemnaturaFile", "Signature File {0} does not exist in the returned zip file, or the filename does not match", semnaturaFileName));
				}
			}
		}

		void ExtractErrorMessageIfExists(ZString invoiceContent)
		{
			if  (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
			{
				return;
			}

			var document = XDocument.Parse(invoiceContent);
			var ns = XNamespace.Get("mfp:anaf:dgti:efactura:mesajEroriFactuta:v1");
			var errorMessages = document
				.Descendants(ns + (NoResString)"Error")
				.Select(e => e.Attribute("errorMessage")?.Value ?? ZString.Empty);

			if (universalEvent.EventParameters != null)
			{
				universalEvent.EventParameters.Reason = string.Join(" ", errorMessages).Replace("\n", "").Replace("\t", "").Replace("\r", "");
			}
		}

		void SaveSemnaturaToEDocs(Stream semnaturaStream, ZString semnaturaFileName)
		{
			var isAcknowledged = universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode;
			var docType = isAcknowledged ? "ACC" : "ERL";
			var description = isAcknowledged ? (NoResString)"Signed Electronic Invoice" : (NoResString)"XML Failure Reasons";

			var edoc = Transaction.DocManagerInfo.AddFileOrDocument(semnaturaStream.ToByteArray(), semnaturaFileName, docType);
			edoc.Description = description;

			Transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
		}
	}
}
