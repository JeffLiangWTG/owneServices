
using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFRN : CargoImpBase, ICimParser
	{
		public CIMFRN(ZString airportOfArrival, ZString cargoTerminalOperator, ZString airWaybillNumberFormatted, ZString houseAirWaybillNumber, ZString splitNumber, ZString agentCode, ErrorCollector ec)
			: base(ec)
		{
			this.agentCode = agentCode;
			this.airportOfArrival = airportOfArrival;
			this.airWaybillNumber = airWaybillNumberFormatted;
			this.cargoTerminalOperator = cargoTerminalOperator;
			this.houseAirWaybillNumber = houseAirWaybillNumber;
			this.splitNumber = splitNumber;
			DemandFieldsNotEmpty("Badge", agentCode,
								"Airport", this.airportOfArrival,
								"AWB number", airWaybillNumber,
								"Shed code", this.cargoTerminalOperator);
		}

		public CIMFRN(List<string> textLines, EDIMessage inboundEdiMessageForAuditing)
			: base(new ErrorCollector())
		{
			this.textLinesInbound = textLines;
			this.inboundEdiMessageForAuditing = inboundEdiMessageForAuditing;
		}

		public override ZString CargoImpCode
		{
			get { return Code; }
		}

		public const string Code = "FRN";

		protected override string[] CargoImpLinesWithoutType
		{
			get
			{
				var result = new List<string>();
				result.Add(airportOfArrival + cargoTerminalOperator);
				if (!houseAirWaybillNumber.IsEmpty)
				{
					result.Add(airWaybillNumber + "-" + houseAirWaybillNumber);
				}
				else
				{
					result.Add(airWaybillNumber);
				}
				if (!splitNumber.IsEmpty)
				{
					result.Add("SPT/" + splitNumber);
				}
				result.Add("AGT/" + agentCode);

				messageInterpretation = messageInterpretationForSentRequests;
				return result.ToArray();
			}
		}

		ZString messageInterpretation;
		public override ZString MessageInterpretation
		{
			get { return messageInterpretation; }
		}

		ZString messageInterpretationForSentRequests
		{
			get
			{
				return string.Format(
					   @"
<p><b>Request renomination to another agent, {0}</b></p>
<p>MAWB / HAWB / Split: {1} {2} {3}</p>
<p>Airport & Shed: {4}{5}</p>",
					   agentCode,
					   airWaybillNumber, houseAirWaybillNumber, splitNumber,
					   airportOfArrival, cargoTerminalOperator);
			}
		}

		public bool DoAllProcessingBeforePrinting()
		{
			var requestednewAgent = GetNewAgent();
			SetAwbFromConsignmentNumber();
			if (Awb != null)
			{
				return AutoRenominateOrSendEmail(requestednewAgent);
			}
			return false;
		}

		bool SendEmail(string newAgent, string previousAgent, string requestorFromPima, bool isAutoRenomination)
		{
			var autoRenomStatement = isAutoRenomination ? "The request has been automatically processed." : "The request requires an FRC if you approve the change (the requesting agent is not allow to make auto-renominations).";
			var recipientPima = inboundEdiMessageForAuditing.Interchange.EI_To.Replace("/", "").Right(6);
			var html = string.Format(@" {5}
										<h3>Renomination request for {0}</h3>
										<h4>{6}</h4>
										<p>Shed {1} has received an FRN request to renominate consignment {0} to <b>{2}</b>.</p>
										<p>This change is from agent {3} and the request was made by {4}.</p>
										", Awb.ReferenceNumber, recipientPima, newAgent, previousAgent, requestorFromPima, MessagePrettierCss.CSS, autoRenomStatement);
			messageInterpretation = html;
			if (!isAutoRenomination)
			{
				html += "<p>To action this request, click below to open the job, change the agent code to the above, and send an FRC message to update the community with the new data.</p>";
			}
			var sender = new CcsukEmailSender(inboundEdiMessageForAuditing.Factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, Awb as BusinessObject, Awb.UserInChargeOfJob);
			sender.SendEmail(string.Format("{0} - {1}renomination request {2}-->{3}", Awb.HumanReadableName, (isAutoRenomination ? "auto-" : ""), previousAgent, newAgent), html,
						GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactCimFrn, "", Awb.Branch.Company.PK.ToGuid(), Awb.Branch.PK.ToGuid(), Guid.Empty);
			return true;
		}

		bool AutoRenominateOrSendEmail(string requestedNewAgent)
		{
			var agentRequestingTheRenomination = inboundEdiMessageForAuditing.Interchange.EI_From.Right(6); // e.g. 000CAR			
			var currentAgent = Awb.AgentBadge;
			var autoRenomWasSuccessful = IsAutoRenominationAllowed(currentAgent, agentRequestingTheRenomination, Awb.Branch) && AutoRenominate(requestedNewAgent);
			return SendEmail(requestedNewAgent, currentAgent, agentRequestingTheRenomination, autoRenomWasSuccessful);
		}

		internal static bool IsAutoRenominationAllowed(ZString currentAgent, ZString agentRequestingTheRenomination, GlbBranch branchForRegistry)
		{
			if (!currentAgent.IsEmpty && agentRequestingTheRenomination.Right(3) == currentAgent)
			{
				var trustedAgents = GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateTrustedAgents.GetFallBackValueAtAllLevels(branchForRegistry.Company.PK.ToGuid(), branchForRegistry.PK.ToGuid(), Guid.Empty);
				var forbiddenAgents = GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.GetFallBackValueAtAllLevels(branchForRegistry.Company.PK.ToGuid(), branchForRegistry.PK.ToGuid(), Guid.Empty);

				if (forbiddenAgents == "*" || forbiddenAgents.Contains(currentAgent) || string.IsNullOrEmpty(trustedAgents)) // forbid everyone, or forbid this guy, or trust no-one
				{
					return false;
				}

				if (trustedAgents == "*" || trustedAgents.Contains(currentAgent))  // this guy has not been forbidden... so we either trust everyone or trust him
				{
					return true;
				}
			}
			return false;  // if the guy does not have either rule... default is forbid
		}

		bool AutoRenominate(string requestedNewAgent)
		{
			Awb.AgentBadge = requestedNewAgent;
			var silentInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var sender = new CcsukInventoryMessageManager(Awb, new CcsukTransmissionMessageFunction.CUSCAR.FRC(), silentInitiator);
			return sender.SendToCommunity();
		}

		void SetAwbFromConsignmentNumber()
		{
			var airportAndShed = textLinesInbound[1];
			Awb = FindAwb(textLinesInbound[2], GetSplit(), airportAndShed, inboundEdiMessageForAuditing.Factory);
		}

		string GetNewAgent()
		{
			foreach (ZString line in textLinesInbound)
			{
				if (line.StartsWith("AGT"))
				{
					return line.Right(3);
				}
			}
			return "";
		}

		string GetSplit()
		{
			ZString lineThree = textLinesInbound[3];
			return lineThree.StartsWith("SPT") ? lineThree.Right(2) : ZString.Empty;
		}

		public ICcsukCusAwb Awb { get; private set; }

		void ICimParser.DoPrinting()
		{
		}

		readonly ZString airportOfArrival;
		readonly ZString cargoTerminalOperator;
		readonly ZString airWaybillNumber;
		readonly ZString houseAirWaybillNumber;
		readonly ZString splitNumber;
		readonly ZString agentCode;
		readonly List<string> textLinesInbound;
		readonly EDIMessage inboundEdiMessageForAuditing;
	}
}
