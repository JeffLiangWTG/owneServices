using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public partial class NonPersistentSplitLineOrchestrator
	{
		public NonPersistentSplitLineOrchestrator(ICcsukCusAwb awb)
		{
			this.Awb = awb;
			SplitsAndFlightData = GetNonPersistentSplitsAndFlightData();
		}

		public NonPersistentSplitLineOrchestrator(ICcsukCusAwb awb, NonPersistentSplitsAndFlightData splitsAndFlightData)
		{
			this.Awb = awb;
			SplitsAndFlightData = splitsAndFlightData;
		}

		public NonPersistentSplitsAndFlightData SplitsAndFlightData { get; set; }

		//Agents
		public ZString PerformSplit_FRD(ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem initiator)
		{
			var result = "";
			if (IsValid)
			{
				var manager = new CcsukInventoryMessageManager(Awb, new CcsukTransmissionMessageFunction.CIM.FRD(SplitsAndFlightData), initiator);
				if (!manager.SendToCommunity())
				{
					result = "Could not send FRD";
				}
			}
			else
			{
				result = "Please fix the validation errors first";
			}
			return result;
		}

		// Sheds
		public ZString PerformSplit_FCS(ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem guiInitiator)
		{
			var result = "";
			if (IsValid)
			{
				if ((LicenceAndPimaHelper.IsFullShed(Awb) || LicenceAndPimaHelper.IsFallbackShed(Awb)))
				{
					if (!SendFcsToSplitOnNetwork(guiInitiator)) // saves FCS message then shows popup
					{
						result = "Could not send FCS";
					}
					SplitImmediatelyLocally(guiInitiator);  // shows user popups about FRC edit messages, then saves					
				}
				else
				{
					result = "Agent roles may not perform this function.  Use the GENRAL or FRD options instead, or select a badge with a non-agent PIMA";
				}
			}
			else
			{
				result = "Please fix the validation errors first";
			}
			return result;
		}

		//Agent
		public ZString PerformSplit_Genral(ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem initiator)
		{
			if (Awb == null || Awb.AgentBadge.IsEmpty || Awb.Branch == null || Awb.CargoTerminalOperatorAirport.IsEmpty || Awb.CargoTerminalOperator.IsEmpty)
			{
				return "Cannot send, job is has insufficient data. Check agent, airport/shed, serial number";
			}

			// NB do not allow any of the lines in the mask, once placeholders have been replaces with real valuwes, to exceed 70 chars.  
			string maskEachLineMustBeNoMoreThan70CharsLong = @"-- Split Request --
Request from agent {0} to split consignment at shed {1}/{2}
Consignment identifier: {3}
Requested division of consignment:
{4}
Agent details:
  Operator company: {5}
  Operator name: {6}
  Operator phone: {7}";

			if (IsValid)
			{
				var realBadge = Awb.AgentBadge;
				var cred = CredentialsSetting.GetCredentialsForBadge(Awb.AgentBadge, Awb.Branch.Company.PK);
				if (cred != null)
				{ realBadge = cred.Company; }
				var textMessage = string.Format(CultureInfo.InvariantCulture, maskEachLineMustBeNoMoreThan70CharsLong, realBadge, Awb.CargoTerminalOperatorAirport, Awb.CargoTerminalOperator,
						Awb.ReferenceNumber,
						SplitsAndFlightData.SplitLines.FormatForGenral(),
						Awb.Branch.Company.GC_Name.Left(70 - 21),
						new ZString(EnvProxy.Instance.CurrentUser.FullName).Left(70 - 17),
						Awb.Branch.GB_Phone.Left(70 - 18)
						);
				var shedPima = string.Format(CultureInfo.InvariantCulture, "CUKAIR98{0}{1}", Awb.CargoTerminalOperatorAirport, Awb.CargoTerminalOperator);

				var linesOfTextMessage = Regex.Split(textMessage, System.Environment.NewLine);  // This assumes that each line, when formatted, is not longer than 70 chars

				if (linesOfTextMessage.Length > 20)
				{
					// Send payload over several messages
					foreach (var partMessage in GenralEdiMessage.SplitLargePayloadIntoManageableFractions(linesOfTextMessage))
					{
						var genral = GenralEdiMessage.MakeNewOutboundFromPayload(string.Join(System.Environment.NewLine, partMessage), shedPima, Awb.Factory, Awb.AgentBadge, true);
						Awb.Messages.Add(genral);
						genral.EM_GB = Awb.Branch.PK;
					}
				}
				else
				{
					var genral = GenralEdiMessage.MakeNewOutboundFromPayload(textMessage, shedPima, Awb.Factory, Awb.AgentBadge, true);
					Awb.Messages.Add(genral);
					genral.EM_GB = Awb.Branch.PK;
				}
				Awb.Factory.Save();
				return "";
			}
			else
			{
				return "Please fix the validation errors first";
			}
		}

		public ZString PerformRemoveAllSplitstNoMessage() // This just updates NPXs... still need another call to send the message
		{
			var result = "";
			var anySplitsReadOnly = (from NonPersistentSplitLine s in SplitsAndFlightData.SplitLines where s.ReadOnly select s).Any();
			if (!anySplitsReadOnly)
			{
				foreach (NonPersistentSplitLine splitLine in SplitsAndFlightData.SplitLines)
				{
					if (splitLine.SplitNumber == "01")
					{
						splitLine.NumberOfPieces = Awb.NumberOfPiecesExpected;
						if (Awb is CusMAWB)
						{
							splitLine.Weight = (Awb as CusMAWB).Weight;
						}
						else if (Awb is CusHAWB)
						{
							splitLine.Weight = (Awb as CusHAWB).CS_Weight;
						}
					}
					else
					{
						splitLine.NumberOfPieces = 0;
						splitLine.Weight = 0;
					}
				}
			}
			else
			{
				result = "Cannot delete all splits, at least one has a locking customs action code";
			}
			return result;
		}

		public NonPersistentSplitsAndFlightData GetNonPersistentSplitsAndFlightData()
		{
			var npSplits = new NonPersistentSplitLineCollection(Awb);
			if (Awb.HasSplits)
			{
				foreach (SplitConsignment split in Awb.Splits)
				{
					var npSplit = new NonPersistentSplitLine(split.SplitReference, split.WeightCode, split.CG_PiecesManifested, split.Weight, split.HandlingInformation, split.CustomsActionCode, Awb);
					npSplit.NumberOfPiecesReceived = split.NumberOfPiecesReceived;
					GetAggregateWarehouseIdAndMarksNumbers(split, npSplit);
					npSplits.Add(npSplit);
				}
			}

			NonPersistentSplitsAndFlightData npSplitsAndFlightData;
			if (Awb is CusMAWB)
			{
				var mawb = (Awb as CusMAWB);
				npSplitsAndFlightData = new NonPersistentSplitsAndFlightData(mawb.CM_FlightNo, mawb.CM_ArrivalDate, npSplits, mawb);
			}
			else
			{
				var hawb = (Awb as CusHAWB);
				npSplitsAndFlightData = new NonPersistentSplitsAndFlightData(hawb.MAWB.CM_FlightNo, hawb.MAWB.CM_ArrivalDate, npSplits, hawb);
			}
			return npSplitsAndFlightData;
		}

		bool SendFcsToSplitOnNetwork(ISendsMessagesToCustoms initiator)
		{
			var fcsManager = new CcsukInventoryMessageManager(Awb, new CcsukTransmissionMessageFunction.CUSCAR.FCS(SplitsAndFlightData.SplitLines), initiator);
			return fcsManager.SendToCommunity(true);  // Do not save or show popup... with a save of FCS and then a popup, the FCS message to our OWN agent will be sent, received and processed; the agent FCS processor may see no splits (if being added) and will add them in its own factory, in our factory in a moment we will then add locally (ignorant of the back-end processing) and we end up with two of each row. Gash. No popup, but commit on final save.  
		}

		bool SplitImmediatelyLocally(ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem amendmentSender)
		{
			var result = true;
			var previousDivisionOfNpr = GetDistributionOfNprToSeeIfFrcIsReallyNeeded(Awb);
			new ConsignmentSplitter(Awb).Split(SplitsAndFlightData.SplitLines);
			if (WipeExistingReceiptsAndUpdateWithNewDetailsAndSendFRCs(Awb, SplitsAndFlightData.SplitLines, amendmentSender, previousDivisionOfNpr) == ContinueWithSave.Yes)
			{
				Awb.Factory.Save();
			}
			return result;
		}

		bool IsValid
		{
			get
			{
				var hasErrors = SplitsAndFlightData.HasErrors || (from NonPersistentSplitLine l in SplitsAndFlightData.SplitLines where l.HasErrors select l).Any();
				return !hasErrors;
			}
		}

		public ICcsukCusAwb Awb { get; private set; }
	}
}
