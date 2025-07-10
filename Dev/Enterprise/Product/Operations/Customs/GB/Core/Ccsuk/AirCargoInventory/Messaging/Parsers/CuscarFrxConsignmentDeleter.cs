using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	class CuscarFrxConsignmentDeleter
	{
		public CuscarFrxConsignmentDeleter(EDIMessage inboundEdiMessageForAuditing, ILogger serviceLogger, CuscarWithFlagsToShowWhatsSet flagableCuscar)
		{
			this.inboundMsg = inboundEdiMessageForAuditing;
			this.serviceLogger = serviceLogger;
			this.flagableCuscar = flagableCuscar;
		}

		internal bool HandleDeleteOfConsignment()
		{
			inboundMsg.EM_MessageSubType = CcsukTransmissionMessageFunction.CUSCAR.FRX.Subcode;
			var awb = CuscarParserAndProcessor.GetExistingAwbFromInboundCuscarReferenceNumbers(flagableCuscar, inboundMsg.Factory);
			if (awb != null)
			{
				inboundMsg.EM_Status = EDIMessage.Status.Received;
				serviceLogger.Log(LogType.Information, delegate
				{ return string.Format("Inbound FRX #{1}, found consignment {0}, about to delete it", awb.ReferenceNumber, inboundMsg.EM_MessageNum); });
				MarkAsDeletedAfterFRX(awb);
			}
			else
			{
				if (inboundMsg.Interchange != null && inboundMsg.Interchange.EI_RetryCount < 10)
				{
					serviceLogger.Log(LogType.Warning, delegate
					{ return string.Format("Inbound FRX message #{0}, could not find consignment, unable to delete. Requeued. Message text starts: {1}", inboundMsg.EM_MessageNum, inboundMsg.EM_MessageText.SubstringSafe(0, 100)); });
					inboundMsg.Interchange.EI_RetryCount += 1;
					inboundMsg.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(1);
				}
				else
				{
					inboundMsg.EM_Status = EDIMessage.Status.Failed;
					serviceLogger.Log(LogType.Warning, delegate
					{ return string.Format("Inbound FRX message #{0}, could not find consignment, unable to delete. FAILS. Message text starts: {1}", inboundMsg.EM_MessageNum, inboundMsg.EM_MessageText.SubstringSafe(0, 100)); });
					new CcsukEmailSender(inboundMsg.Factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, null).SendEmail(
									"Could not find CCSUK job using inbound FRX data - " + inboundMsg.EM_MessageNum, string.Format("No master or house could be found using data in an inbound FRX message.  No delete has been performed. Examine and potentially reset-to-queued status the message in Maintain>System>EDIMessage using message number {0}.  Message begins {1}", inboundMsg.EM_MessageNum, inboundMsg.EM_MessageText.SubstringSafe(0, 100)),
									GBCustomsDataRegistry.Instance.NotificationCcsukErrors, "", Guid.Empty, Guid.Empty, Guid.Empty);
				}
			}
			return true;  // we understood the message even if it talked of unknown consignments
		}

		void MarkAsDeletedAfterFRX(ICcsukCusAwb awb, bool linkMessageToAwb = true)
		{
			if (awb is CusHAWB hawb)
			{
				var remainingUndeletedHouseBillsOnMawb = from CusHAWB h in hawb.MAWB.ChildBills where hawb.IsLodgedAtCcsuk select h;
				if (remainingUndeletedHouseBillsOnMawb.Take(2).Count() == 1)
				{
					// last house on master is being deleted....
					MarkAsDeletedAfterFRX(hawb.MAWB, false);
				}
			}

			SetInterpretationForDelete(awb);
			if (linkMessageToAwb)
			{
				awb.Messages.Add(inboundMsg);
				inboundMsg.EM_GB = awb.Branch.PK;
			}
			awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDbDeleted;
			var bizObj = awb as BusinessObject;
			bizObj.GetLogs().AddNew(Events.SetToInactive, inboundMsg.EM_MessageSubType, ZDateTimeOffset.Now);
			new CcsukEmailSender(inboundMsg.Factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, bizObj, awb.UserInChargeOfJob).SendEmail(
				string.Format("{0} deleted using community data, message #{1}", awb.HumanReadableName, inboundMsg.EM_MessageNum), inboundMsg.EM_MessageInterpretation,
				GBCustomsDataRegistry.Instance.NotificationCcsukCuscarFrx, "", awb.Branch.Company.PK.ToGuid(), awb.Branch.PK.ToGuid(), Guid.Empty);
		}

		void SetInterpretationForDelete(ICcsukCusAwb awb)
		{
			inboundMsg.EM_MessageInterpretation = string.Format("{0} <h3>Record {1} deleted using community request</h3>", MessagePrettierCss.CSS, awb.ReferenceNumber);
		}

		readonly EDIMessage inboundMsg;
		readonly ILogger serviceLogger;
		readonly CuscarWithFlagsToShowWhatsSet flagableCuscar;
	}
}
