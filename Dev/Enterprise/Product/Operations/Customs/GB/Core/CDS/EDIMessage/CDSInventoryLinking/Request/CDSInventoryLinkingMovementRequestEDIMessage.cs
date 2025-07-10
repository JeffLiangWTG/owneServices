using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingMovementRequestEDIMessage : CDSEDIMessage<CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingMovementRequest>
	{
		public CDSInventoryLinkingMovementRequestEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.InventoryLinkingMovementRequest;
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

		public CDSInventoryLinkingMovementRequestEDIMessagePrettier Prettier => prettier ?? (prettier = new CDSInventoryLinkingMovementRequestEDIMessagePrettier(this));
		CDSInventoryLinkingMovementRequestEDIMessagePrettier prettier;
	}
}
