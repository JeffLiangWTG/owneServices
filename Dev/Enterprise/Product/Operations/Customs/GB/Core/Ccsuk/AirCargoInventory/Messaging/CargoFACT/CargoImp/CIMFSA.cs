using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public partial class CIMFSA : CargoImpBase
	{
		protected CIMFSA(ICcsukCusAwb awb, ZString optionalOtherServiceInformation, ErrorCollector ec)
			: base(ec)
		{
			if (awb != null)
			{
				formattedMawbNumber = awb.ReferenceNumber;
				originAirport = awb.AirportOfOrigin.Right(3);
				destinationAirport = awb.AirportOfDestination.Right(3);
				partialPieces = awb.NumberOfPiecesReceived;
				totalPieces = awb.NumberOfPiecesExpected;
				Awb = awb;
			}
			this.optionalOtherServiceInformation = optionalOtherServiceInformation;
			DemandFieldsNotEmpty(new ZString[] { "Master Number", formattedMawbNumber });
		}

		protected CIMFSA(ZString formattedMawbNumber, ZString optionalOtherServiceInformation, ErrorCollector ec)
			: base(ec)
		{
			this.formattedMawbNumber = formattedMawbNumber;
			this.optionalOtherServiceInformation = optionalOtherServiceInformation;

			DemandFieldsNotEmpty(new ZString[] { "Master Number", this.formattedMawbNumber });
			DemandFieldsNotEmpty(new ZString[] { "Other Service Information", this.optionalOtherServiceInformation });
		}

		public override ZString CargoImpCode
		{
			get { return CcsukTransmissionMessageFunction.CIM.FSA.SubCode; }
		}

		protected override ZInt CargoImpVersionCore
		{
			get { return 2; }
		}

		protected override string[] CargoImpLinesWithoutType
		{
			get
			{
				var result = new List<string>();
				result.Add(
							formattedMawbNumber +  // 2.1
							originAirport + destinationAirport +  // 2.2
							GetHeaderPartialAndTotalPieces() // 2.3
						);
				var mainStatusList = GetStatusCodeLine();
				if (mainStatusList.Count > 0)
				{
					result.AddRange(mainStatusList);
				}

				var osi = GetLineOrTwoLinesOfOSI();
				if (osi.Count > 0)
				{
					result.AddRange(osi.ToArray());
				}

				return result.ToArray();
			}
		}

		protected string GetHeaderPartialAndTotalPieces()
		{
			return partialPieces.IsEmpty ?
										(!totalPieces.IsEmpty ? string.Format("/T{0}", totalPieces) : "")
										: string.Format("/P{0}T{1}", partialPieces, totalPieces);
		}

		List<string> GetLineOrTwoLinesOfOSI()
		{
			var result = new List<string>();
			optionalOtherServiceInformation = optionalOtherServiceInformation.Replace(slash, "");
			if (optionalOtherServiceInformation.Length > 65)
			{
				result.Add("OSI" + slash + optionalOtherServiceInformation.Left(65));
				result.Add(slash + optionalOtherServiceInformation.SubstringSafe(65, 65));
			}
			else if (optionalOtherServiceInformation.Length > 0)
			{
				result.Add("OSI" + slash + optionalOtherServiceInformation);
			}
			return result;
		}

		public override ZString MessageInterpretation
		{
			get
			{
				var inboundInterpretation = MessageInterpretationInbound;
				return inboundInterpretation.IsEmpty ? MessageInterpretationOutbound : inboundInterpretation;
			}
		}

		ZString MessageInterpretationOutbound
		{
			get { return string.Format("{0} <h3>Freight Status Answer (outbound) - {1}</h3>", MessagePrettierCss.CSS, formattedMawbNumber); }
		}

		public string PartialAndTotalPiecesLine()
		{
			return (!partialPieces.IsEmpty ? "P" + partialPieces.ToString() : (!totalPieces.IsEmpty ? "T" + totalPieces.ToString() : ""));
		}

		protected virtual List<string> GetStatusCodeLine()
		{
			return new List<string>();
		}
		protected ZString formattedMawbNumber;
		protected ZString originAirport;
		protected ZString destinationAirport;
		protected ZInt partialPieces;
		protected ZInt totalPieces;
		ZString optionalOtherServiceInformation;
	}
}
