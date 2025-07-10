using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	sealed class Message1035Processor : BaseMessageProcessorForSingleNumber<GpNg1035Msg2GatepassFeedbackMessage>
	{
		internal Message1035Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A04A4C3D-E538-4354-86A3-C71E39EBF366", "IL Gate Pass Movement Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { ILMessageTypeList.Codes.GPM };

		protected override ZString CouldNotLocateMessage => Res.GetString("0523E6CB-BEF1-45A8-9420-26FD36F15E5C", "Could not locate Shipment by Gate Pass Number #");

		protected override ZString MoreThanOneMessage => Res.GetString("213F9358-0617-493C-8606-693A693A4A96", "Found more than one shipment with the same Gate Pass Number – please check");

		protected override ZString EntryType => CusEntryNumberTypes.Israel.GatepassMovementNumber;

		protected override ZString GetMessageReferenceNumber(GpNg1035Msg2GatepassFeedbackMessage responseMessage)
		{
			var gatepassFeedbackMessage = responseMessage.GatepassFeedbackMessage?.FirstOrDefault();
			var gatepassNumber = gatepassFeedbackMessage?.GatepassNumber.ToString();
			return gatepassNumber;
		}

		protected override ZDateTimeOffset GetResponseDateTime(GpNg1035Msg2GatepassFeedbackMessage responseMessage)
			=> responseMessage.ResponseContentHeader.TransmitionDateTime;

		protected override bool ShouldSendNotificationEmail() => ILCustomsDataRegistry.Instance.ILGPMGroupNotification.Value.SendMode != Core.Constants.EmailTo.NoEmails;

		protected override ZBool GetShouldSendErrorEmailsOnly(IEDIMessageCollectionOwner owner, ILEDIMessage message)
		{
			IGlbBranch messageBranch = GlbBranch.CurrentBranch;

			var supportMessageSuppressRegistry = ILCustomsDataRegistry.Instance.ILGPMGroupNotification as ISupportMessageSuppressRegistry;
			var shouldSendErrorEmailsOnly = supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(messageBranch.GB_GC, messageBranch.PK, ZGuid.Empty) ?? false;
			return shouldSendErrorEmailsOnly;
		}

		protected override string GetJobNumber(IEDIMessageCollectionOwner owner) => ((ForwardingShipment)owner).JS_UniqueConsignRef;

		protected override ControllerID ControllerIDForEmail => ControllerIDs.JobShipment;

		protected override string MessageTypeInSubject => Res.GetString("F0469ADE-435B-4731-8091-2352DA993F28", "Shipment Message");

		protected override bool SupportsConsol => true;

		protected override ElectronicFormEventLogManager<GpNg1035Msg2GatepassFeedbackMessage> GetElectronicFormEventLogManager()
			=> new GatePassMovementEventLogManager();

		protected override void ClearMessageReference(EnterpriseBusinessObject enterpriseBusinessObject)
		{
			if (enterpriseBusinessObject is IGatePassMovementProviderFactory gatePassMovementProviderFactory)
			{
				gatePassMovementProviderFactory.GatePassMovementProvider.MessageReferenceNumber = ZString.Empty;
			}
		}

		protected override ZString GetApplicationId(GpNg1035Msg2GatepassFeedbackMessage responseMessage)
		{
			return responseMessage.ResponseContentHeader?.ApplicationId.ToString();
		}

		protected override ZString GetStatusName(BusinessObjectFactory factory, GpNg1035Msg2GatepassFeedbackMessage responseMessage)
		{
			if (responseMessage.GatepassFeedbackMessage != null
				&& responseMessage.GatepassFeedbackMessage.Count >= 1
				&& responseMessage.GatepassFeedbackMessage[0] is GpNg1035Msg2GatepassFeedbackMessageGatepassFeedbackMessage gatepassFeedbackMessage)
			{
				return factory.GetDescriptionFromRefCusCodeListCombinedCode(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILGatePassReturnedCode, gatepassFeedbackMessage.GatepassStatus.ToString());
			}
			return ZString.Empty;
		}
	}
}
