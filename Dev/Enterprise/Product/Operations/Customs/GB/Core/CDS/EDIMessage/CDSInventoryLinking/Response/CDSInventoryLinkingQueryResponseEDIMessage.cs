using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingQueryResponseEDIMessage : CDSEDIMessage<CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingQueryResponse>
	{
		public CDSInventoryLinkingQueryResponseEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryResponse;
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

		public CDSInventoryLinkingQueryResponseEDIMessagePrettier Prettier => prettier ?? (prettier = new CDSInventoryLinkingQueryResponseEDIMessagePrettier(this));
		CDSInventoryLinkingQueryResponseEDIMessagePrettier prettier;

		public new InventoryLinkingQueryResponse MessageDataObject => messageDataObject ?? (messageDataObject = new InventoryLinkingQueryResponse(EM_MessageText));
		InventoryLinkingQueryResponse messageDataObject;
	}
}
