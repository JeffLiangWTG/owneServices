using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class DocumentCaptureResponseMessageProcessor : ESCommonResponseMessageProcessor<BusinessObject, AttachedDocument>
	{
		public DocumentCaptureResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Document Capture Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.EsDocumentRequest };

		const string XsdSchemaNameDocumentCaptureResponse = "CargoWise.Customs.ES.MessageDefinitions.DocumentCaptureRequest.Incoming.DocumentCaptureResponse.xsd";

		protected override void ProcessMessageCore(EDIMessage message, BusinessObject linkedBusinessObject, AttachedDocument provider)
		{
			var prettyFormatter = new DocumentCaptureMessagePrettyFormatter(provider);

			try
			{
				var docManagerInfo = ((IDocManagerSupport)linkedBusinessObject).DocManagerInfo;

				var eDoc = docManagerInfo.AddFileOrDocument(provider.ImageData, provider.FileName, provider.Type.Code);
				if (eDoc != null)
				{
					eDoc.Description = provider.Type.Description;
					if (EDocsSaver == null)
					{
						throw new InvalidOperationException("You must set an EDocsSaver before trying to save to eDocs");
					}
					EDocsSaver.QueueForSaving(docManagerInfo);

					AddDocumentAllocatedEvent(linkedBusinessObject, eDoc);
					SetMessageInterpretation();
					SetMessageStatusAsReceived(message);
				}
			}
			catch (Exception ex) when (ex is ImageFormatException || !ex.IsCriticalException())
			{
				throw new InvalidOperationException(prettyFormatter.CreateMessageDetailsRejected());
			}

			void SetMessageInterpretation()
			{
				message.EM_MessageInterpretation = prettyFormatter.CreateMessageDetailsAccepted();
			}
		}

		protected override AttachedDocument GetMessageProviderCore(EDIMessage message)
		{
			using (var textReader = message.GetEM_MessageTextReader())
			{
				return ESXmlObjectSerializer.DeserializeWithValidation<AttachedDocument>(XsdSchemaNameDocumentCaptureResponse, textReader);
			}
		}

		void AddDocumentAllocatedEvent(BusinessObject linkedBusinessObject, IeDoc document)
		{
			if (linkedBusinessObject is IStmALogProvider logsProvider)
			{
				var reference = linkedBusinessObject is IESMessageInfoProvider infoProvider
					? $"{document.DocType}|{document.FileNameOnly} for {infoProvider.DocumentJobReference}|{document.UniqueKey}"
					: document.CreateReference();

				logsProvider.Logs.AddNew(Events.DocumentAllocated, reference);
			}
		}
	}
}
