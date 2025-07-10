using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CIMFSA : CargoImpBase, ICimParser
	{
		public CIMFSA(List<string> textLines, EDIMessage inboundEdiMessageForAuditing, string commonAccessReference)
			: base(new ErrorCollector())
		{
			this.textLinesInbound = textLines;
			this.inboundEdiMessageForAuditing = inboundEdiMessageForAuditing;
			this.commonAccessReferenceMaybeCrap = commonAccessReference;
		}

		bool ICimParser.DoAllProcessingBeforePrinting()
		{
			var mawbNumber = new ZString(textLinesInbound[1]).Left(12);  // some responses are 130-62012002ATLLHR/P9T10, others are just 130-62012002 
			Awb = FindAwbFromCommonAccessReference();
			if (Awb == null)
			{
				var senderPima = inboundEdiMessageForAuditing.Interchange != null ? inboundEdiMessageForAuditing.Interchange.EI_From : ZString.Empty;
				Awb = FindAwb(mawbNumber, "", senderPima.Right(6), inboundEdiMessageForAuditing.Factory);  // Try using message's interchange's sender PIMA, whcih will be the shed who's answering us
				if (Awb == null)
				{
					Awb = FindAwb(mawbNumber, "", "", inboundEdiMessageForAuditing.Factory);   // otherwise try the first awb hit
				}
			}
			return (Awb != null);
		}

		ICcsukCusAwb FindAwbFromCommonAccessReference()
		{
			ICcsukCusAwb result = null;
			if (!commonAccessReferenceMaybeCrap.IsEmpty)
			{
				var message = TryLoadOutboundMessageFromCommonAccessReference(commonAccessReferenceMaybeCrap, inboundEdiMessageForAuditing.Factory);
				if (message != null)
				{
					var hawb = message.EM_LinkedObject as CusHAWB;
					if (hawb != null)
					{
						result = hawb.CS_IsMasterHouse ? hawb.MAWB : hawb;
					}
				}
			}
			return result;
		}

		void ICimParser.DoPrinting()
		{ }

		protected ZString MessageInterpretationInbound
		{
			get
			{
				ZString result = "";
				if (textLinesInbound != null)
				{
					// Inbound FSA
					var table = new HtmlTableCreator(new string[] { "CargoIMP", "Explanation" });
					for (int i = 1; i < textLinesInbound.Count; i++)
					{
						var rawCargoImp = textLinesInbound[i];
						var explained = ZString.Empty;
						try
						{
							explained = i == 1 ? GetExplanationOfMawbAndAirportsLine(rawCargoImp) : GetExplanationOfAllOtherLines(rawCargoImp);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ErrorReporter.ReportOnce("BGB-CUK-FSAParser", string.Format("Could not parse line {0} of inbound FSA with text {1}", i, inboundEdiMessageForAuditing.EM_MessageText), ex); // Column name used in error message, not key
						}
						table.WriteRow(rawCargoImp, explained);
					}

					result = string.Format(" {0} <h3>{1} - {2}</h3> {3}",
											MessagePrettierCss.CSS,
											"Freight Status Answer (FSA)", (Awb == null ? new ZString("(unknown)") : Awb.ReferenceNumber),
											table.ToHtml()
											);
				}
				return result;
			}
		}

		string GetExplanationOfMawbAndAirportsLine(ZString rawCargoImp)
		{
			// e.g. 057-12345675YMXHEL/P2T3
			var mawb = rawCargoImp.SubstringSafe(0, 12);  // eg 057-12345675
			var origin = rawCargoImp.SubstringSafe(12, 3); // e.g. YMX
			var destination = rawCargoImp.SubstringSafe(15, 3);  //e.g HEL
			var piecesRaw = rawCargoImp.SubstringSafe(19); // e.g. P2T3 or T3 with optional K10 afterwards
			ZString piecesExplained = ExplainPiecesAndMass(piecesRaw);
			var overallExplanation = "MAWB " + mawb;
			if (!origin.IsEmpty)
			{
				overallExplanation += " from " + origin;
			}
			if (!destination.IsEmpty)
			{
				overallExplanation += " to " + destination;
			}
			if (!piecesExplained.IsEmpty)
			{
				overallExplanation += " ; " + piecesExplained;
			}
			return overallExplanation;
		}

		internal static ZString ExplainPiecesAndMass(ZString piecesRaw)
		{
			//piecesRaw:   with P2T3 or P2 or T3, then with optional K10 or L10 afterwards for mass
			var piecesExplained = ZString.Empty;
			var partialPieces = ZString.Empty;
			var totalPieces = ZString.Empty;
			var mass = ZString.Empty;
			var massUnit = string.Empty;
			var partialPiecesRegex = new Regex(@"P([0-9]{1,4})"); // P and 1-4 digits, capture digits
			var totalPiecesRegex = new Regex(@"T([0-9]{1,4})");  // T and 1-4 digits, capture digits
			var massRegex = new Regex(@"([KL])([0-9]{1,7}(\.[0-9]{1,5})?)");  // K or L followed by 7-character decimal; ie K or L plus 1-7 digits with optional (point and 1-5 digits); capture unit and all numbers

			var partialMatches = partialPiecesRegex.Matches(piecesRaw);
			if (partialMatches != null && partialMatches.Count > 0)
			{
				partialPieces = partialMatches[0].Groups[1].Value;
			}
			var totalMatches = totalPiecesRegex.Matches(piecesRaw);
			if (totalMatches != null && totalMatches.Count > 0)
			{
				totalPieces = totalMatches[0].Groups[1].Value;
			}
			var massMatches = massRegex.Matches(piecesRaw);
			if (massMatches != null && massMatches.Count > 0)
			{
				massUnit = massMatches[0].Groups[1].Value;
				mass = massMatches[0].Groups[2].Value;
				massUnit = massUnit == "K" ? "kilo" : (massUnit == "L" ? "pound" : massUnit);
				massUnit = mass != "1" ? massUnit + "s" : massUnit;
			}

			if (partialPieces.IsEmpty && !totalPieces.IsEmpty)
			{
				piecesExplained = "Total " + totalPieces + " pieces";
			}
			else if (!partialPieces.IsEmpty && totalPieces.IsEmpty)
			{
				piecesExplained = string.Format("Partial {0} pieces", partialPieces);
			}
			else if (!partialPieces.IsEmpty && !totalPieces.IsEmpty)
			{
				piecesExplained = string.Format("Partial {0} of {1} total pieces", partialPieces, totalPieces);
			}

			if (!mass.IsEmpty)
			{
				piecesExplained += string.Format("; {0} {1}", mass, massUnit);
			}
			return piecesExplained;
		}

		string GetExplanationOfAllOtherLines(ZString rawCargoImp)
		{
			var lineIdentifier = rawCargoImp.Left(3);
			switch (lineIdentifier)
			{
				case CargoIMPFSUStatusList.Codes.RCF:
					return CIMFSA_RCF.ExplainInboundLine(rawCargoImp);
				case CargoIMPFSUStatusList.Codes.TFD:
					return CIMFSA_TFD.ExplainInboundLine(rawCargoImp);
				case CargoIMPFSUStatusList.Codes.DLV:
					return CIMFSA_DLV.ExplainInboundLine(rawCargoImp);
				case CcsukTransmissionMessageFunction.CIM.FSA.OSI.FsaSubCode:
					return CIMFSA_OSI.ExplainInboundLine(rawCargoImp);
				default:
					return new CargoIMPFSUStatusList().GetDescriptionFromCode(lineIdentifier);
			}
		}

		public ICcsukCusAwb Awb { get; private set; }
		readonly List<string> textLinesInbound;
		readonly EDIMessage inboundEdiMessageForAuditing;
		readonly ZString commonAccessReferenceMaybeCrap; // the shed may pass back nothing, or a good value, or a totally made-up value. 
	}
}
