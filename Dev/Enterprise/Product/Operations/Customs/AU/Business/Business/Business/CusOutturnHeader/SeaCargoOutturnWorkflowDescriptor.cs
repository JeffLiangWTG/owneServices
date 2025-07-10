using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	class SeaCargoOutturnWorkflowDescriptor : Customs.Business.CusSCAOceanBillWorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.SeaCargoOutturnWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("77A6FA17-3DC1-4A1C-ABAA-1276D748260F", "Sea Cargo Outturn");
		public override Type WorkflowProviderType => typeof(CusOutturnHeader);

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.ArrivalTransitWarehouse;
		}

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.SupportedTriggerPartyServicesCore(recipient);
			}
		}

		public override bool SupportsEventTracking => true;

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return false;
		}

		protected override IEnumerable<string> GetScheduleDeferredMessageSendTriggerActionsCore(IBaseTrigger trigger, IBusiness business)
		{
			yield return WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUSeaCargoMessage;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);
			var outturnHeader = (CusOutturnHeader)bizObj;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(outturnHeader.OutturningPremise));
					break;
			}
		}

		public override ControllerID ControllerID => ControllerIDs.Customs.AU.SeaCargoDepotStandAloneController;
	}
}
