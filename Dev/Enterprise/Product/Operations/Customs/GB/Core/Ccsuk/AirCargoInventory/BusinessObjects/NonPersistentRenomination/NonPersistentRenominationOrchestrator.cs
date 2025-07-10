using System;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentRenominationOrchestrator
	{
		public NonPersistentRenominationOrchestrator(ICcsukCusAwb awb)
		{
			Renomination = new NonPersistentRenomination(awb.Factory);
			this.awb = awb;
		}

		public NonPersistentRenomination Renomination { get; set; }

		public bool RenominateToAgentAndMaybeSendGenral(ISendsMessagesToCustoms initiator)
		{
			var frnManager = new CcsukInventoryMessageManager(awb, new CcsukTransmissionMessageFunction.CIM.FRN(Renomination.NewAgent), initiator);
			var result = frnManager.SendToCommunity();  // popup here
			try
			{
				if (result && Renomination.SendNewAgentGenral)
				{
					var newAgentPima = "CUKFFW98000" + Renomination.NewAgent;
					var textMessage = string.Format(renominationAdviceMask,
						awb.AgentBadge,
						awb.CargoTerminalOperatorAirport,
						awb.CargoTerminalOperator,
						awb.ReferenceNumber,
						Renomination.NewAgent,
						ZDateTime.Now,
						awb.Branch.Company.GC_Name,
						awb.Branch.GB_BranchName, BrandingFactory.Instance.ProductName,
						BrandingFactory.Instance.CompanyName);
					var genral = GenralEdiMessage.MakeNewOutboundFromPayload(textMessage, newAgentPima, awb.Factory, awb.Profile, true);
					awb.Messages.Add(genral);
					genral.EM_GB = awb.Branch.PK;
					awb.Factory.Save();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				initiator.NotifyUserOfAnInvalidOperation("Could not send GENRAL message.  The FRN is unaffected.\r\nError=" + ex.Message);
				result = false;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		readonly string renominationAdviceMask = @"** Advice of renomination request **
{3} at {1}{2}
{0} --> {4}
Please be advised that agent {0} has requested shed {1}{2}
renominate record {3} to you, {4}. 
The date of the request was {5}.
If the shed honours this request then you will receive further 
messages from them. 
Sender: 
{6} ({7}).

Sent using {8} from {9}.";

		readonly ICcsukCusAwb awb;
	}
}
