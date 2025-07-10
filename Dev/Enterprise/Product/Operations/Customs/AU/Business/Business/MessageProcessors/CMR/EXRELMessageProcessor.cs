using Enterprise.BatchProcessor;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXRELMessageProcessor : CMRMessageResponseProcessor
	{
		public EXRELMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.EXREL, "Export Consignment Release Advice (EXREL)")
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (result)
			{
				var houseBill = incomingMessage.EM_LinkedObject as ForwardingShipment;
				if (houseBill != null)
				{
					incomingMessage.LinkOrCloneMessage(houseBill);
				}
				else
				{
					var manifestHeader = incomingMessage.EM_LinkedObject as ExportCustomsManifestHeader;
					if (manifestHeader != null)
					{
						incomingMessage.LinkOrCloneMessage(manifestHeader);
					}
					else
					{
						var consol = incomingMessage.EM_LinkedObject as ForwardingConsol;
						if (consol != null)
						{
							incomingMessage.LinkOrCloneMessage(consol);
						}

						else
						{
							result = false;
						}
					}
				}
			}

			return result;
		}

		protected override bool FindShipment()
		{
			bool result = false;

			var message = incomingMessage as CMREXRELMessage;
			if (message != null)
			{
				var shipment = message.FindShipment();
				if (shipment != null)
				{
					incomingMessage.LinkOrCloneMessage(shipment);
					result = true;
				}
			}

			return result;
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return true; }
		}
	}
}
