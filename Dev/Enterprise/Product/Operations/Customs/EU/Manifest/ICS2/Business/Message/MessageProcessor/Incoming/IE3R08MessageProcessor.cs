using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R08;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Manifest.ICS2.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3R08MessageProcessor : MessageProcessorWithEmailNotification<Ie3R08Type>
	{
		public IE3R08MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("48a88a6f-f80b-44c5-bb03-4f539471de96", "Consultation Results Response");

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3R08Type messageObject) => Res.GetString("fe663881-5c6f-4603-977e-14f08d2b4901", "ICS2 – ENS Consultation Results Response");

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3R08Type messageObject)
		{
			var stateList = RefCusCodeListTypes.GetCachedList(manifestHeader.Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2SC, ZDateTime.Today);
			var htmlBuilder = new ZStringBuilder(Res.GetString("e2673f1d-e670-45ff-8c49-8b8fd1701f63", "ICS2 – ENS Consultation Results Response"));
			htmlBuilder.AppendLine();

			var messageInfoTableBuilder = new HtmlTableCreator(new string[] { Res.GetString("fb86a9e5-d919-4604-9b02-bcaa53a2974c", "Type"), Res.GetString("05996967-c5f7-4f35-ba96-f3a494cd08da", "Identification"), Res.GetString("50ed9c10-de11-4192-b41d-2e9bb1d193e2", "State") });

			var euics2EnsEntityType = new EUICS2ENSEntityType();
			foreach (var item in messageObject.EnsEntity)
			{
				messageInfoTableBuilder.WriteRow($"{item.Type} - {euics2EnsEntityType.GetDescriptionFromCode(item.Type)}", item.Identification, $"{item.State} - {stateList.GetDescriptionFromCode(item.State)}");
			}

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override void SetMessageProcessedStatus(EDIMessage message)
		{
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
		}

		protected override Func<Ie3R08Type, EDIMessage, AsycudaManifestHeader> FindManifestHeaderFromSpecificContext => (messageObject, message) => {
			var functionalReference = messageObject.FunctionalReference;
			EDIMessage transmitMessage = null;

			if (!string.IsNullOrEmpty(functionalReference))
			{
				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_ExternalReferenceNumber, functionalReference);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.IC2);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypes.Codes.Q05);
				query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.ProcessedOK);

				transmitMessage = message.Factory.LoadTop1<EDIMessage>(query);
			}

			AsycudaManifestHeader header = null;
			if (transmitMessage != null)
			{
				header = message.Factory.Load<AsycudaManifestHeader>(transmitMessage.EM_LinkUniqueID);
			}

			return header;
		};
	}
}
