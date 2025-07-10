using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515X_v514.CC515XV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.Business
{
	public class ComplXAESResponseMessageProcessor : AESCommonResponseMessageProcessor<Cc515Xv1Sal>
	{
		public ComplXAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		public ComplXAESResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Export Type X Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC515XV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.TypeXExportUcc6 };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc515Xv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new ComplXAESMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cc515Xv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			UpdateEntryInstructionSubStyleFromBToX(entryHeader);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;

				ProcessDocumentsComplX(entryHeader);
			}

			return ZString.Empty;
		}

		void ProcessDocumentsComplX(CusEntryHeader entryHeader)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessComplXExportEntryLineSupportingDocuments();
				var hasAnyDocCL = entryLine.GetPreviouslySentPreviousDocuments().Any();
				if (!entryLine.HasPRECustomsOffice && !hasAnyDocCL)
				{
					entryLine.ProcessC651EntryLinePreviousDocuments();
				}
			}
		}

		const string XsdSchemaNameCC515XV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CC515XV1Sal.xsd";
	}
}
