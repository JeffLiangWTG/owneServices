using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class ExportDeclarationResponseMessageProcessor : ExportGenericResponseMessageProcessor
	{
		public ExportDeclarationResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}
		protected override string MessageFriendlyNameCore => (NoResString)"Export Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.Export, Messaging.DeclarationMessageTypeList.Codes.ExportAmendment };

		protected override void ProcessDocuments(CusEntryHeader entryHeader)
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessExportEntryLineSupportingDocuments();
				entryLine.ProcessExportEntryLinePreviousDocuments();
			}
		}
	}
}
