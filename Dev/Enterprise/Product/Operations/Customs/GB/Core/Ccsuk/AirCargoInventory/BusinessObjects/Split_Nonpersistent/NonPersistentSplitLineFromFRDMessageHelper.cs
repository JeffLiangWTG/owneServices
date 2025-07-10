using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class NonPersistentSplitLineFromFRDMessageHelper
	{
		public NonPersistentSplitLineFromFRDMessageHelper(ICcsukCusAwb awb, NonPersistentSplitsAndFlightData splitsAndFlightData)
		{
			this.awb = awb;
			this.splitsAndFlightData = splitsAndFlightData;
		}

		internal ZString ParseFrdAndUpdateNpboSplitsCollection()
		{
			var result = "";
			var lastReceivedFrd = (from EDIMessage m in awb.Messages
								   where m.EM_ReceiveTransmit == EDIMessage.Direction.Receive
									&& m.EM_MessageType == CcsukTransmissionMessageFunction.CIM.Code
									&& m.EM_MessageSubType == CcsukTransmissionMessageFunction.CIM.FRD.SubCode
								   orderby m.EM_SystemCreateTimeUtc
								   select m).LastOrDefault();
			if (lastReceivedFrd != null)
			{
				var edifactObject = lastReceivedFrd.GetAutoEdifactMessageUsingNamedFactory(CcsukEdifactMessageFactory.Factory, lastReceivedFrd.CharacterSet);
				var cargoFactMessage = edifactObject as CargoFactMessage;
				if (cargoFactMessage != null)
				{
					var processor = new CargoFactMessageProcessor(cargoFactMessage, lastReceivedFrd, null);
					var iCimProcessor = processor.GetCargoImpFromCargoFact();
					var frdProcessor = iCimProcessor as CIMFRD;
					if (frdProcessor != null)
					{
						frdProcessor.Awb = awb;
						var npSplitLinesRequested = frdProcessor.ParseInboundFrdToGetSplitsRequested();
						if (npSplitLinesRequested != null)
						{
							if (npSplitLinesRequested.FlightNumber.IsEmpty)
							{
								// Splits don't yet exsit or want to update those that do exist - just load NPX details from FRD
								splitsAndFlightData.SplitLines.RemoveAndDeleteAll();
								foreach (NonPersistentSplitLine requestedLine in npSplitLinesRequested.SplitLines)
								{
									requestedLine.Awb = awb;
									splitsAndFlightData.SplitLines.Add(requestedLine);
								}
							}
							else
							{
								if (splitsAndFlightData.SplitLines.Count > 0 && awb.HasSplits && npSplitLinesRequested.SplitLines.Count == awb.Splits.Count)
								{
									// Splits already exist and FRD message contains flight info, meaning that it is allocating NPR
									// Update NPR on existing splits with NoP from FRD. Don't touch NPX. 
									foreach (NonPersistentSplitLine requestedNpbo in npSplitLinesRequested.SplitLines)
									{
										var existingNpbo = (from NonPersistentSplitLine existingLine in splitsAndFlightData.SplitLines where existingLine.SplitNumber == requestedNpbo.SplitNumber select existingLine).FirstOrDefault();
										if (existingNpbo != null)
										{
											existingNpbo.NumberOfPiecesReceived = (ZShort)requestedNpbo.NumberOfPieces;
											existingNpbo.HandlingDetail = requestedNpbo.HandlingDetail;
										}
									}
								}
								else
								{
									result = "FRD flight number is set, but the number of splits in the messages does not match those which already exist, or none already exist.";
								}
							}
							splitsAndFlightData.SplitLines.RefreshBinding();
						}
						else
						{
							result = "FRD processor could not parse message to obtain lines.";
						}
					}
					else
					{
						result = "FRD processor could not be created.";
					}
				}
				else
				{
					result = "FRD message could not be loaded.";
				}
			}
			else
			{
				result = "No inbound FRD message was found.";
			}

			return result;
		}
		readonly ICcsukCusAwb awb;
		readonly NonPersistentSplitsAndFlightData splitsAndFlightData;
	}
}
