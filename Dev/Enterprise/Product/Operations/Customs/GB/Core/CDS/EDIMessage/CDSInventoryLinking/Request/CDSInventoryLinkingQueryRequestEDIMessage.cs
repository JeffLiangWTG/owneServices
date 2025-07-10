using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingQueryRequestEDIMessage : CDSEDIMessage<CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingQueryRequest>
	{
		public CDSInventoryLinkingQueryRequestEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingQueryRequest;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => !EM_MessageText.IsEmpty ? Prettier.MakeHumanReadable() : base.EM_MessageInterpretation;
			set
			{
				base.EM_MessageInterpretation = value;
			}
		}

		public CDSInventoryLinkingQueryRequestEDIMessagePrettier Prettier => prettier ?? (prettier = new CDSInventoryLinkingQueryRequestEDIMessagePrettier(this));
		CDSInventoryLinkingQueryRequestEDIMessagePrettier prettier;
	}
}
