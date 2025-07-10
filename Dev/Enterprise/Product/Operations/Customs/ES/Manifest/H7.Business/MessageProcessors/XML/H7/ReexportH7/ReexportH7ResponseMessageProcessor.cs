using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ReexportacionH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class ReexportH7ResponseMessageProcessor : H7CommonResponseMessageProcessor<ReexportacionH7V1Sal, ReexportH7MessagePrettyFormatter>
	{
		public ReexportH7ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Reexport H7 Response Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.H7ReExport };

		protected override ZString XsdSchemaEmbeddedResourceName => "CargoWise.Customs.ES.MessageDefinitions.Version1.H7.Incoming.ReexportacionH7V1Sal.xsd";

		protected override ReexportH7MessagePrettyFormatter GetNewMessagePrettyFormatter(ReexportacionH7V1Sal response, EDIMessage message, AsycudaBill bill) => new ReexportH7MessagePrettyFormatter(response);

		protected override void ProcessAcceptedResponse(ReexportacionH7V1Sal response, EDIMessage message, AsycudaBill bill)
		{
			base.ProcessAcceptedResponse(response, message, bill);

			SetBillStatus(response, bill);
		}

		void SetBillStatus(ReexportacionH7V1Sal response, AsycudaBill bill)
		{
			bill.ABL_BillStatus = AISEntryStatusList.Codes.Invalid;
		}
	}
}
