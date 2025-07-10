using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for ESMRMessageProcessor.
	/// </summary>
	public class ESMRMessageProcessor : ManifestMessageResponseProcessor
	{
		public ESMRMessageProcessor(LoggingInformation logger)
			: base(logger, "ESM", "Export Sub Manifest Response(ESMR)")
		{
		}

		#region Implementation

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			ZString result = base.DoProcessingReturningStatus(message);
			if (message.EM_LinkedObject != null && message.EM_LinkTable == CommonConsol.Schema.TableName)
			{
				ForwardingConsol consol = (ForwardingConsol)message.EM_LinkedObject;
				ESMManifestStatus status = new ESMManifestStatus(consol);
				var consolWrapper = new FreightConsolWrapper(consol, message);
				if (status.E2_MessageStatus == Enterprise.Customs.Business.ManifestStatus.Cleared.AsString)
				{
					consol.Logs.AddNew(Events.CustomsEntryStatus, "CLO");
				}

				#region Update manifest sequencing

				if (statusType.Contains(RejectedString))
				{
					if (!consolWrapper.IsWithdrawalResponse)
					{
						consolWrapper.RemovePreliminaryLineNumbers();
					}
				}
				else if (statusType.Contains(WithdrawnString))
				{
					consolWrapper.RemoveAllESMLineNumbers();
				}
				else if (status.E2_MessageStatus == Enterprise.Customs.Business.ManifestStatus.Cleared.AsString
						|| statusType.Contains("CLEAR")
						|| statusType.Contains("EXPIRED")
						|| statusType.Contains("ERROR - VALIDATION"))
				{
					consolWrapper.UpdateDeletedManifestLines();
					consolWrapper.UpdatePreliminaryLinesToManifestedLines();
				}

				#endregion

				var exportCustomsClearanceStatus = consolWrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM);

				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					if (shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValue)
					{
						foreach (IHVLVConsignment consignment in shipment.HVLVConsignments)
						{
							consignment.HVC_ExportCustomsClearanceStatus = exportCustomsClearanceStatus;
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}
