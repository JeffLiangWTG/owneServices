using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.ImportLicense.Incoming;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using XmlObjectSerializer = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class BRCImportLicenseAcceptMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCImportLicenseAcceptMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("d39aca28-c2cb-42a3-8adf-3a0fb5542985", "Import License Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.LIC };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			var loteli = XmlObjectSerializer.Deserialize<loteli>(message.EM_MessageText);
			ZString importLicenseIdentifier = loteli.listaLIVORetorno[0].idSolicitacao;

			var entryHeader = new CusEntryHeader.Loader(message.Factory).GetEntryHeaderByImportLicenseIdentifier(importLicenseIdentifier);
			if (entryHeader == null)
			{
				Logger.LogError($"Unable to find an Entry with Import License Identifier '{importLicenseIdentifier}' for {message.EM_MessageType} message #{message.EM_MessageNum}");
			}

			return entryHeader;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var loteli = XmlObjectSerializer.Deserialize<loteli>(message.EM_MessageText);
				var li = loteli.listaLIVORetorno[0];
				ZString batchIdentifier = loteli.idLote;
				ZString importLicenseIdentifier = li.idSolicitacao;
				ZString entryNumber = li.numeroLI;
				var reasons = li.mensagemDiagnostico.mensagemDiagnostico.IsNullOrEmpty() ? new[] { string.Empty } : li.mensagemDiagnostico.mensagemDiagnostico;

				if (!ZDateTime.TryParseExact((ZString)li.dtRegistro, out var issueDate, Constants.DataFormat))
				{
					issueDate = ZDateTime.Today;
				}

				if (entryNumber.IsEmpty)
				{
					entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
					foreach (var reason in reasons)
					{
						entryHeader.Logs.AddNew(Events.MessageRejected, new ZDateTimeOffset(issueDate), new[]
						{
							new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber, batchIdentifier + ";" + importLicenseIdentifier),
							new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason),
						});
					}
				}
				else
				{
					entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
					entryHeader.MovementReferenceNumberSetter(entryNumber, issueDate);
					UpdateLinkedISWInvoiceLines(entryHeader);
				}
			}
		}

		void UpdateLinkedISWInvoiceLines(CusEntryHeader cusEntryHeader)
		{
			var entryNumber = cusEntryHeader.EntryNumber.Left(10);
			var linesPKs = cusEntryHeader.InvoiceLines.Select(line => line.PK).ToArray();
			var query = JobComInvoiceLine.GetChildInvoiceLinesQuery(BRJobMessageTypeList.Codes.ImportSiscomex, linesPKs);
			var lines = cusEntryHeader.Factory.Load<JobComInvoiceLine>(query);
			lines.ForEach(line => line.ImportLicenseNumber = entryNumber);
		}
	}
}
