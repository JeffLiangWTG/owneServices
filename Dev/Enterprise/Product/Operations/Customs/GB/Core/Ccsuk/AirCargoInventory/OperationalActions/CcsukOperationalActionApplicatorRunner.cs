using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class CcsukOperationalActionApplicatorRunner
	{
		public CcsukOperationalActionApplicatorRunner(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			this.log = log;
			this.targets = targets;
		}

		public void Renominate(ZString newAgent)
		{
			this.newAgent = newAgent;
			InvokeMethodAfterCheckingForICcsukCusAwb(RenominateCore, true);
		}

		public void QueryFsrWithUpdate()
		{
			InvokeMethodAfterCheckingForICcsukCusAwb(QueryFsrWithUpdateCore);
		}

		void InvokeMethodAfterCheckingForICcsukCusAwb(Action<ICcsukCusAwb> methodToInvoke, bool requiresShedLicence = false)
		{
			if (requiresShedLicence && !LicenceAndPimaHelper.ShedEnabled)
			{
				log.Notify(OperationalActionLogErrorLevel.Error, "You have disabled Shed functions in the registry; this action cannot run.");
				return;
			}

			foreach (var bo in targets)
			{
				var awb = bo as ICcsukCusAwb;
				if (awb == null)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Skipping {0} because it is not an ICcsukCusAwb", bo.HumanReadableName);
				}
				else
				{
					methodToInvoke(awb);
				}
			}
		}

		public void DetachFromForwarding()
		{
			InvokeMethodAfterCheckingForICcsukCusAwb(DetachFromForwardingCore);
		}

		void DetachFromForwardingCore(ICcsukCusAwb awb)
		{
			var mawb = awb as CusMAWB;
			if (mawb != null)
			{
				if (!mawb.CM_JK.IsEmpty)
				{
					mawb.CM_JK = ZGuid.Empty;
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Detached MAWB from forwarding for {0}", awb.ReferenceNumber);
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Skipping {0} because MAWB is not linked to Forwarding", awb.ReferenceNumber);
				}
			}
			else
			{
				var hawb = awb as CusHAWB;
				if (hawb != null)
				{
					if (!hawb.CS_JS.IsEmpty)
					{
						hawb.CS_JS = ZGuid.Empty;
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Detached HAWB from forwarding for {0}", awb.ReferenceNumber);
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Skipping {0} because HAWB is not linked to Forwarding", awb.ReferenceNumber);
					}
				}
			}
		}

		void RenominateCore(ICcsukCusAwb awb)
		{
			if (awb.AgentBadgeReadOnly)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Skipping {0} because the agent field is locked", awb.ReferenceNumber);
				if (awb.HasSplits)
				{
					foreach (SplitConsignment split in awb.Splits)
					{
						RenominateCore(split);
					}
				}
			}
			else
			{
				if (awb.AgentBadge == newAgent)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Skipping {0} because the requested agent is already set", awb.ReferenceNumber);
				}
				else
				{
					if (awb.HasSplits)
					{
						foreach (SplitConsignment split in awb.Splits)
						{
							RenominateCore(split);
						}
						RenominateCore_ApplyAndSendFrc(awb);
					}
					else
					{
						RenominateCore_ApplyAndSendFrc(awb);
					}
				}
			}
		}

		void QueryFsrWithUpdateCore(ICcsukCusAwb awb)
		{
			var messagingResultErrorMessage = SendMessage(awb, new CcsukTransmissionMessageFunction.CUKFSR.FsaWithUpdate());
			if (messagingResultErrorMessage.IsEmpty)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Queued FSR query message for {0}", awb.ReferenceNumber);
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Could not queue FSR query message for {0}. {1}", awb.ReferenceNumber, messagingResultErrorMessage);
			}
		}

		void RenominateCore_ApplyAndSendFrc(ICcsukCusAwb awb)
		{
			var originalAgent = awb.AgentBadge;
			awb.AgentBadge = newAgent;
			var messagingResultErrorMessage = SendMessage(awb, new CcsukTransmissionMessageFunction.CUSCAR.FRC());
			if (messagingResultErrorMessage.IsEmpty)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "New agent set on {0} and FRC message queued", awb.ReferenceNumber);
			}
			else
			{
				awb.AgentBadge = originalAgent;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "FRC message for {0} could not be queued so agent has been reverted. {1}", awb.ReferenceNumber, messagingResultErrorMessage);
			}
		}

		ZString SendMessage(ICcsukCusAwb awb, CcsukTransmissionMessageFunction messageType)
		{
			var silentInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var sender = new CcsukInventoryMessageManager(awb, messageType, silentInitiator);
			return sender.SendToCommunity() ? "" : silentInitiator.LastErrorsAsString.Replace(System.Environment.NewLine, " ").TrimEnd();
		}

		ZString newAgent;
		readonly IOperationalActionSectionLog log;
		readonly BusinessObject[] targets;
	}
}
