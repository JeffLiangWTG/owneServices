using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingMovementTotalsResponseEDIMessage : CDSEDIMessage<CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingMovementTotalsResponse>
	{
		public CDSInventoryLinkingMovementTotalsResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementTotalsResponse;
			EM_MessageSubType = CDSEDIMessageTypeList.Codes.ConversationID;
			EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => !EM_MessageText.IsEmpty ? Prettier.MakeHumanReadable() : base.EM_MessageInterpretation;
			set
			{
				if (!EM_MessageText.IsEmpty)
				{
					throw new NotSupportedException("Setting EM_MessageInterpretation is not supported.");
				}
				base.EM_MessageInterpretation = value;
			}
		}

		public CDSInventoryLinkingMovementTotalsResponseEDIMessagePrettier Prettier => prettier ?? (prettier = new CDSInventoryLinkingMovementTotalsResponseEDIMessagePrettier(this));
		CDSInventoryLinkingMovementTotalsResponseEDIMessagePrettier prettier;

		public new InventoryLinkingMovementTotalsResponse MessageDataObject => messageDataObject ?? (messageDataObject = new InventoryLinkingMovementTotalsResponse(EM_MessageText));
		InventoryLinkingMovementTotalsResponse messageDataObject;
	}
}
