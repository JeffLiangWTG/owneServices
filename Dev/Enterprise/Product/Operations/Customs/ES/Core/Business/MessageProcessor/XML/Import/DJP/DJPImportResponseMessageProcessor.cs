using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DocumentosSimplifiV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.ES.Business.Declaration.ReadOnlySupportingDocumentCollection;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public class DJPImportResponseMessageProcessor : XMLResponseMessageProcessor<DocumentosSimplifiV1Sal, IMessagePrettyFormatter>
	{
		public DJPImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => PendingSupportingDocumentsFriendlyName;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string PendingSupportingDocumentsFriendlyName = "Pending Supporting Documents Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameDocumentosSimplifiV1Sal;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string XsdSchemaNameDocumentosSimplifiV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.DocumentosSimplifiV1Sal.xsd";

		protected override ZString AcceptedResponseCode => ResponseMessageCodeList.AcceptedCode;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.PendingSupportingDocuments };

		protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			var businessObject = MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<DocumentosSimplifiV1Sal>(message, XsdSchemaEmbeddedResourceName, sentBusinessObjects);
			return businessObject ?? base.FindRelevantBusinessObjectCore(message, sentBusinessObjects, isDirectxTMessage);
		}

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(DocumentosSimplifiV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
			=> new DJPImportMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(DocumentosSimplifiV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			UpdateEntryInstructionSubStyleFromBToX(entryHeader);
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			entryHeader.ZG_DJPMRN = response.NumeroDeReferencia;
			UpdateDocuments(entryHeader);

			return ZString.Empty;
		}

		void UpdateDocuments(CusEntryHeader entryHeader)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				var documentsInCL = entryLine.GetPreviouslySentSupportingDocuments();

				entryLine.ResetReadOnlySupportingDocuments();
				var readOnlyDocuments = entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>();
				var newDeclaredDocuments = readOnlyDocuments.Except(documentsInCL.Cast<EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentEqualityKey>(), new AcceptedDocumentModificationComparer());

				var newDocumentsToAdd = newDeclaredDocuments.Except(documentsInCL.Cast<EU.Business.Declaration.MultiLineAddInfos.ISupportingDocumentEqualityKey>(), new AcceptedDocumentModificationAllButProcedureComparer());
				var documentsToUpdate = newDeclaredDocuments.Except(newDocumentsToAdd);

				foreach (ReadOnlySupportingDocument readOnlySupDoc in newDocumentsToAdd)
				{
					var document = SupportingDocument.CopyFrom(readOnlySupDoc);
					document.CSI_ParentID = entryLine.PK;
					document.CSI_ParentTableCode = entryLine.TablePrefix;

					SetStatus(document);
				}

				foreach (var clSupDoc in documentsInCL)
				{
					var clUpdateDocs = documentsToUpdate.Cast<ReadOnlySupportingDocument>().Where(x => new AcceptedDocumentModificationAllButProcedureComparer().Equals(x, clSupDoc)).ToArray();
					if (clUpdateDocs != null && clUpdateDocs.Length == 1)
					{
						clSupDoc.CSI_Procedure = clUpdateDocs[0].CSI_Procedure;
					}

					SetStatus(clSupDoc);
				}
			}
		}

		void SetStatus(SupportingDocument document)
		{
			var procedure = document.CSI_Procedure;
			if (procedure == SupportingDocumentProcedure.Accord || procedure == SupportingDocumentProcedure.Regularize)
			{
				document.CSI_Status = DocumentStatus.Accepted;
			}
			else if (procedure == SupportingDocumentProcedure.NotProvided)
			{
				document.CSI_Status = DocumentStatus.Cancelled;
			}
		}
	}
}
