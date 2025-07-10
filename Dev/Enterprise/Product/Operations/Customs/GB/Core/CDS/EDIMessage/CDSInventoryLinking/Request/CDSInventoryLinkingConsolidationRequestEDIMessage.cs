using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingConsolidationRequestEDIMessage : CDSEDIMessage<CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingConsolidationRequest>
	{
		public CDSInventoryLinkingConsolidationRequestEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingConsolidationRequest;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		public override ZString EM_MessageInterpretation
		{
			get => !EM_MessageText.IsEmpty ? Prettier.MakeHumanReadable() : base.EM_MessageInterpretation;
			set
			{
				base.EM_MessageInterpretation = value;
			}
		}

		public CDSInventoryLinkingConsolidationRequestEDIMessagePrettier Prettier => prettier ?? (prettier = new CDSInventoryLinkingConsolidationRequestEDIMessagePrettier(this));
		CDSInventoryLinkingConsolidationRequestEDIMessagePrettier prettier;
	}
}
