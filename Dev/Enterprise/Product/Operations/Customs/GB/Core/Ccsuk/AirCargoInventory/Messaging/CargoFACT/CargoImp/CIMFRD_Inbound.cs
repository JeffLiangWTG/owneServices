using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CIMFRD : CIMFCS, ICimParser
	{
		public CIMFRD(List<string> textLines, EDIMessage inboundEdiMessageForAuditing, CargoFactMessage receivedCargoFact)
			: base(new ErrorCollector())
		{
			this.textLinesInbound = textLines;
			this.inboundEdiMessageForAuditing = inboundEdiMessageForAuditing;
			this.receivedCargoFact = receivedCargoFact;
		}

		bool ICimParser.DoAllProcessingBeforePrinting()
		{
			SetAwbFromConsignmentNumber();
			if (Awb != null)
			{
				return PerformAnyAutoSplit();
			}
			else
			{
				SendContrl("Consignment not found.");
			}
			return false;
		}

		bool PerformAnyAutoSplit()
		{
			requestedSplitsAndFlightData = ParseInboundFrdToGetSplitsRequested();
			if (requestedSplitsAndFlightData != null && requestedSplitsAndFlightData.SplitLines != null)
			{
				var splitsCountIsOk = CheckSplitsCountMatchesOrIsCurrentlyZero();
				if (splitsCountIsOk.IsEmpty)
				{
					if (CanPerformAutoSplitDueToRegoOptionAndFewerThanTwoReceiptRows())
					{
						if ((LicenceAndPimaHelper.IsFullShed(Awb) || LicenceAndPimaHelper.IsFallbackShed(Awb)))
						{
							var result = AutoSplitUsingOrchestrator(requestedSplitsAndFlightData);
							if (!result.IsEmpty)
							{
								Log(LogType.Warning, "Could not auto-split using FRD. " + result);
							}
							return true;
						}
						else
						{
							SendContrl("Cannot split, this consignment is not registered as a shed job.");
							return false;
						}
					}
					else
					{
						SendEmailNotifyingOfSplitRequest(requestedSplitsAndFlightData.SplitLines);
						return true;
					}
				}
				else
				{
					SendContrl(splitsCountIsOk);
					return false;
				}
			}
			else
			{
				SendContrl("Request rejected, no split lines present in FRD message or error parsing message.");
			}
			return false;
		}

		bool CanPerformAutoSplitDueToRegoOptionAndFewerThanTwoReceiptRows()
		{
			return GBCustomsDataRegistry.Instance.CcsukAllowShedAutoSplitFromFrd.Value
					&& (
						(Awb.NumberOfPiecesReceived == 0 && Awb.OutTurns.Count == 0)  // no pieces ...
						||
						(!Awb.Status1Date.IsEmpty && Awb.OutTurns.Count == 1 && Awb.OutTurns[0].C5_PackagesOutturned == Awb.NumberOfPiecesExpected)  // or all pieces in one location
					  );
		}

		ZString CheckSplitsCountMatchesOrIsCurrentlyZero()
		{
			if (Awb.Splits == null || Awb.Splits.Count == 0)
			{
				return ""; // OK
			}
			else if (Awb.Splits != null && Awb.Splits.Count > 0 && Awb.Splits.Count < requestedSplitsAndFlightData.SplitLines.Count)
			{
				return string.Format("Changes to existing splits rejected. Number of splits in FRD ({0}) greater than splits on file ({1}).\n\nTo fix correct the number of splits OR do not send flight data.", requestedSplitsAndFlightData.SplitLines.Count, Awb.Splits.Count);
			}
			var dictionaryOfDatabaseSplitsAndInboundRequestedSplits = new Dictionary<SplitConsignment, NonPersistentSplitLine>();
			var requestedSplitNumbersNotFoundInDatabase = new List<string>();
			foreach (NonPersistentSplitLine requestedSplit in requestedSplitsAndFlightData.SplitLines)
			{
				var splitInDatabase = Awb.Splits[requestedSplit.SplitNumber];
				if (splitInDatabase != null)
				{
					dictionaryOfDatabaseSplitsAndInboundRequestedSplits.Add(splitInDatabase, requestedSplit);
				}
				else
				{
					requestedSplitNumbersNotFoundInDatabase.Add(requestedSplit.SplitNumber);
				}
			}
			if (requestedSplitNumbersNotFoundInDatabase.Count == 0)
			{
				return "";  // OK
			}
			else
			{
				return string.Format("Cannot update NPR on split the following splits.  Not found in local database. No records updated. Split number(s) {0}.", String.Join(", ", requestedSplitNumbersNotFoundInDatabase.ToArray()));
			}
		}

		void SendEmailNotifyingOfSplitRequest(NonPersistentSplitLineCollection splitsRequested)
		{
			var splitsHumanReadable = splitsRequested.FormatForGenral();
			var body = ZString.Format("{0} <h2>Request to split {1}</h3> <p>An inbound FRD message was received. Auto-splitting is not possible (some but not all pieces are received, or are spread over multiple receipt rows). Please create an FCS message to split as per the agent's request. Agent from message text={2}, PIMA of message sender={3}</p> <pre>{4}</pre>",
										MessagePrettierCss.CSS, Awb.ReferenceNumber, agentBadge, inboundEdiMessageForAuditing.Interchange.EI_From, splitsHumanReadable);
			var emailer = new CcsukEmailSender(inboundEdiMessageForAuditing.Factory, CcsukEmailSender.ToWhom.StaffAndOrCustomsGroupBasedOnRegistry, (BusinessObject)Awb, Awb.UserInChargeOfJob);
			emailer.SendEmail(Awb.HumanReadableName + " - request to split", body, GBCustomsDataRegistry.Instance.NotificationCcsukCargoFactCimFrd, "", Awb.Branch.Company.PK.ToGuid(), Awb.Branch.PK.ToGuid(), Guid.Empty);
			inboundEdiMessageForAuditing.EM_MessageInterpretation = body;
		}

		void SendContrl(ZString text)
		{
			Log(LogType.Warning, text);
			if (Awb != null)
			{
				text += " Contact shed operator on " + Awb.Branch.GB_Phone;
			}
			var ccsukTransmissionMessageFunction = new CcsukTransmissionMessageFunction.CONTRL(inboundEdiMessageForAuditing, receivedCargoFact.UNH[0], "4", "6", text);
			new CcsukInventoryMessageManager(inboundEdiMessageForAuditing, ccsukTransmissionMessageFunction, new SendsMessagesToCustomsShutterUpperer()).SendToCommunity();
		}

		ZString AutoSplitUsingOrchestrator(NonPersistentSplitsAndFlightData splitsAndFlightDataRequested)
		{
			if (Awb.OutTurns.Count == 1)
			{
				foreach (NonPersistentSplitLine npbo in splitsAndFlightDataRequested.SplitLines)
				{
					npbo.WarehouseLocationID = Awb.OutTurns[0].WarehouseLocationID;
				}
			}
			var orchestrator = new NonPersistentSplitLineOrchestrator(Awb, splitsAndFlightDataRequested);
			return orchestrator.PerformSplit_FCS(new SendsMessagesToCustomsShutterUpperer(false)); // Does the shutter upper support amendment detection? 
		}

		public CancellationTokenSource CancellationTokenSource => cancellationTokenSource ?? ResetCancellationTokenSource();
		public CancellationTokenSource ResetCancellationTokenSource() => cancellationTokenSource = new CancellationTokenSource();
		CancellationTokenSource cancellationTokenSource;

		internal NonPersistentSplitsAndFlightData ParseInboundFrdToGetSplitsRequested()
		{
			var npSplitsAndFlightData = new NonPersistentSplitsAndFlightData(inboundEdiMessageForAuditing.Factory);
			for (int lineNumber = 2; lineNumber < textLinesInbound.Count; lineNumber++)
			{
				var line = textLinesInbound[lineNumber];
				if (line.StartsWith("ARR"))
				{
					//ARR/BA123/25MAY
					var arrivalInfo = line.Split('/');
					npSplitsAndFlightData.FlightNumber = arrivalInfo[1];
					var tempDate = ZDateTime.Empty;
					ZDateTime.TryParseExact(arrivalInfo[2], out tempDate, "ddMMM");
					npSplitsAndFlightData.FlightArrivalDate = tempDate;
					continue;
				}
				if (line.StartsWith("AGT"))
				{
					agentBadge = new ZString(line).Right(3);
					continue;
				}
				if (line.StartsWith("SPT"))
				{
					try
					{
						// e.g. SPT/03P20K10/BOX SPARE PARTS - 21 THRU 40
						ParseOneSplitLine(npSplitsAndFlightData.SplitLines, lineNumber);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("BGB-CUK-CIMFRD-Split", "Could not split inbound CIMFRD message, text parsing error", ex);
						return null;
					}
				}
			}
			return npSplitsAndFlightData;
		}

		void ParseOneSplitLine(NonPersistentSplitLineCollection splitsCollection, int lineNumber)
		{
			var elementsOfSplitLine = Regex.Split(textLinesInbound[lineNumber], slash);   // {SPT}, {03P20K10}, {BOX SPARE PARTS - 21 THRU 40}
			ZString splitAndWeightAndPieces = elementsOfSplitLine[1];  //03P20K10
			var handlingDetail = elementsOfSplitLine.Length > 2 ? elementsOfSplitLine[2] : "";  //BOX SPARE PARTS - 21 THRU 40
			var splitNumber = splitAndWeightAndPieces.Left(2);  //03
			var sdc = splitAndWeightAndPieces.SubstringSafe(2, 1); //P
			var weightInKilosAndPieces = splitAndWeightAndPieces.SubstringSafe(3).Split('K'); // {20},{10}
			var weight = ZDecimal.Parse(weightInKilosAndPieces[1]); //10 
			var numberOfPieces = ZInt.ParseEmptyAsZero(weightInKilosAndPieces[0]); //20					
			var npbo = new NonPersistentSplitLine(splitNumber, "KG", numberOfPieces, weight, handlingDetail, "", Awb);
			npbo.NumberOfPieces = numberOfPieces; // now that AWB is set, can trigger NoP-->NPR setting
			splitsCollection.Add(npbo);
		}

		void SetAwbFromConsignmentNumber()
		{
			var airportAndShed = textLinesInbound[1];
			Awb = FindAwb(textLinesInbound[2], "", airportAndShed, inboundEdiMessageForAuditing.Factory);
			if (Awb != null)
			{
				airWaybillNumber = Awb.ReferenceNumber;
			}
		}

		public ICcsukCusAwb Awb { get; set; }

		void ICimParser.DoPrinting()
		{
		}

		public override ZString MessageInterpretation
		{
			get { return Awb != null && requestedSplitsAndFlightData != null && requestedSplitsAndFlightData.SplitLines != null ? base.MessageInterpretation : new ZString("Split request"); }
		}

		readonly List<string> textLinesInbound;
		readonly EDIMessage inboundEdiMessageForAuditing;
		readonly CargoFactMessage receivedCargoFact;
	}
}
