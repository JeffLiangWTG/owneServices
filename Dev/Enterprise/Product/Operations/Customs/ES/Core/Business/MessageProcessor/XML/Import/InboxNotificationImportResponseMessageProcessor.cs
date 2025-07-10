using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PresentaMercanciasV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationImportResponseMessageProcessor : ImportGenericResponseMessageProcessor<PresentaMercanciasV1Sal>
	{
		public InboxNotificationImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Import inbox Notification Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNamePresentaMercanciasV1Sal;

		protected override ZString AcceptedResponseCode => ImportActivationResultCode.AcceptedDeclaration;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.InBoxNotificationForImport };

		protected override ZBool IsInboxDeclaration => true;

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(PresentaMercanciasV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new ImportCommonMessagePrettyFormatter<PresentaMercanciasV1Sal>(response, entryHeader);

		protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			return MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<PresentaMercanciasV1Sal>(message, XsdSchemaEmbeddedResourceName, sentBusinessObjects);
		}

		protected override ZString ProcessAcceptedDeclaration(PresentaMercanciasV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			SetCircuit(response, entryHeader);
			var entryStatus = GetEntryStatus(response, entryHeader, message, ImmutableHashSet.Create<ZString>());
			SetAcceptedDeclarationData(response, entryHeader, entryStatus, isSimplified: false);
			SetEntryStatusAndCSVClearance(response, entryHeader, message, entryStatus);

			ResetGuaranteesAmountAndAddTransactions(response, entryHeader);

			return ZString.Empty;
		}

		protected override Collection<GarantiaGrNutilizadaTd> GetResponseGuaranteesCan(PresentaMercanciasV1Sal response) => ((IImportCommon)response).GRNGuaranteesCan;

		protected override bool ShouldSetEntryStatusCLP(CusEntryHeader entryHeader) => entryHeader.EntryInstruction?.IsSubStyleBOrCOrZ ?? false;

		protected override void ProcessRejectedDeclaration(PresentaMercanciasV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

		const string XsdSchemaNamePresentaMercanciasV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.PresentaMercanciasV1Sal.xsd";
	}
}
