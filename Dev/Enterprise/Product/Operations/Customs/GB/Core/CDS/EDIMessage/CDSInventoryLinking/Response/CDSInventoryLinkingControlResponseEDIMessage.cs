using System;
using System.Data;
using CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingControlResponseEDIMessage : CDSEDIMessage<inventoryLinkingControlResponse>
	{
		public CDSInventoryLinkingControlResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingControlResponse;
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

		public CDSInventoryLinkingControlResponseEDIMessagePrettier Prettier => prettier ?? (prettier = new CDSInventoryLinkingControlResponseEDIMessagePrettier(this));
		CDSInventoryLinkingControlResponseEDIMessagePrettier prettier;

		public new InventoryLinkingControlResponse MessageDataObject => messageDataObject ?? (messageDataObject = new InventoryLinkingControlResponse(EM_MessageText));
		InventoryLinkingControlResponse messageDataObject;
	}
}
