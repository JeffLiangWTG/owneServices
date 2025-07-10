using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class CIMFCS : CargoImpBase
	{
		public CIMFCS(ZString airportOfArrival, ZString cargoTerminalOperator, ZString airWaybillNumberFormatted, ZString houseAirWaybillNumber, NonPersistentSplitLineCollection splits, ErrorCollector ec)
			: base(ec)
		{
			this.airportOfArrival = airportOfArrival;
			this.airWaybillNumber = airWaybillNumberFormatted;
			this.cargoTerminalOperator = cargoTerminalOperator;
			this.houseAirWaybillNumber = houseAirWaybillNumber;
			this.requestedSplitsAndFlightData = new NonPersistentSplitsAndFlightData(splits.Factory);
			this.requestedSplitsAndFlightData.SplitLines = splits;
			DemandFieldsNotEmpty("Airport", this.airportOfArrival,
								"AWB number", airWaybillNumber,
								"Shed code", this.cargoTerminalOperator);
			if (splits.Count == 0)
			{
				errorCollector.AddError("number of splits", new ErrorInfo("", "mandatory"));
			}
		}

		public CIMFCS(ErrorCollector ec)
			: base(ec)
		{ }

		public override ZString CargoImpCode
		{
			get { return "FCS"; }
		}

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

				AddFlightArrivalDetails(result);
				AddAgentBadge(result);

				foreach (NonPersistentSplitLine split in requestedSplitsAndFlightData.SplitLines)
				{
					foreach (var errorNotification in (from INotification n in split.Notifications where n.Type.IsFatal select n))
					{
						errorCollector.AddError(errorNotification.Message, new ErrorInfo("", "", false));
					}

					result.Add(string.Format("SPT/{0}P{1}{2}{3}{4}", split.SplitNumber, split.NumberOfPieces, "K", GetWeightInKilos(split), GetHandlingDetail(split)));
				}
				return result.ToArray();
			}
		}

		string GetWeightInKilos(NonPersistentSplitLine split)
		{
			string unit = new List<string> { "K", "KG", "KGM" }.Contains(split.WeightUQ) ? Constants.Weight.Kilograms : split.WeightUQ.ToString();
			var weight = new ZWeight(split.Weight, unit.ToUpper());
			if (weight.IsValid)
			{
				return weight.InKilograms > 9999m ? weight.InKilograms.ToStringTrimZeros("#######") : weight.InKilograms.ToStringTrimZeros("####.##");
			}
			else
			{
				return "";
			}
		}

		protected virtual void AddFlightArrivalDetails(List<string> result)
		{
		}

		protected virtual void AddAgentBadge(List<string> result)
		{
		}

		protected virtual ZString GetHandlingDetail(NonPersistentSplitLine split)
		{
			return ZString.Empty;
		}

		public override ZString MessageInterpretation
		{
			get
			{
				return string.Format(
					@"
<p><b>Request splitting of consignment </b></p>
<p>MAWB / HAWB:   {0} {1}  </p>
<p>Splits requested: <pre>{2}</pre>  </p> 
",
					airWaybillNumber, houseAirWaybillNumber,
					requestedSplitsAndFlightData.SplitLines.FormatForGenral());
			}
		}

		protected ZString airportOfArrival;
		protected ZString cargoTerminalOperator;
		protected ZString airWaybillNumber;
		protected ZString houseAirWaybillNumber;
		protected NonPersistentSplitsAndFlightData requestedSplitsAndFlightData;
	}
}
