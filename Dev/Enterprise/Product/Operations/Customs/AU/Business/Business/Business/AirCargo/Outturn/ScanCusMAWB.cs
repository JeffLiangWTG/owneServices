using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ScanCusMAWB : ScanMasterBill
	{
		public ScanCusMAWB(CusMAWB hostedBO)
			: base(hostedBO)
		{
			this.hostedBO = Argument.NotNull(hostedBO, "hostedBO");
		}

		public CusUnderbond[] GetSentUnderbonds()
		{
			return hostedBO.GetSentUnderbonds();
		}

		public override string Validate()
		{
			var errorBuilder = new ZStringBuilder(base.Validate());

			if (!IsStandAlone)
			{
				if (SelectedShipments != null)
				{
					foreach (CusHAWB cusHAWB in SelectedShipments)
					{
						if (cusHAWB.IsHVLVShipment())
						{
							var airCargo = hostedBO.FindStandAloneAirCargo(cusHAWB.CS_HAWB);
							if (airCargo == null)
							{
								errorBuilder.AppendLine(Res.GetString("22ec7290-87bd-48c9-89fe-ee7d3c81ee41", "Create Standalone AirCargo for House Bill '{0}'.", cusHAWB.CS_HAWB));
							}
						}
					}
				}
			}

			return errorBuilder.ToString();
		}

		public override string ValidateSelectedUnderbond()
		{
			var errorBuilder = new ZStringBuilder(base.ValidateSelectedUnderbond());
			if (errorBuilder.IsEmpty && string.IsNullOrEmpty(SelectedUnderbond.C4_FlightNo))
			{
				errorBuilder.AppendLine(Res.GetString("22ec7290-97bd-48c9-89fe-ee7d3c81ee42", "Flight Number for Underbond '{0}' is empty. Please enter a Flight Number.", SelectedUnderbond.C4_SendersMessageReference));
			}
			return errorBuilder.ToString();
		}

		public string ValidateStandAloneUnderbonds()
		{
			var errorBuilder = new StringBuilder();

			if (SelectedUnderbond == null)
			{
				throw new InvalidOperationException("Select Underbond first");
			}

			if (!IsStandAlone)
			{
				if (SelectedShipments != null)
				{
					foreach (CusHAWB cusHAWB in SelectedShipments)
					{
						if (cusHAWB.IsHVLVShipment())
						{
							var airCargo = hostedBO.FindStandAloneAirCargo(cusHAWB.CS_HAWB);
							if (airCargo == null)
							{
								errorBuilder.AppendLine(
									Res.GetString("1799D011-8612-46F3-BD24-C81E4874F3A0", "Standalone AirCargo with House Bill '{0}' was not found, has it been renamed?,",
									cusHAWB.CS_HAWB));
							}
							else
							{
								var underbonds = airCargo.GetUnderbonds();
								CusUnderbond[] matchUnderbonds = null;
								if (underbonds != null)
								{
									matchUnderbonds = underbonds.Where
									(
										u => u.C4_FlightNo == SelectedUnderbond.C4_FlightNo
										&& u.C4_ArrivalDate == SelectedUnderbond.C4_ArrivalDate
										&& u.C4_OriginPremiseID == SelectedUnderbond.C4_OriginPremiseID
										&& u.C4_DestinationPremiseID == SelectedUnderbond.C4_DestinationPremiseID
									).ToArray();
								}

								if (matchUnderbonds == null || matchUnderbonds.Length == 0)
								{
									errorBuilder.AppendLine(
										Res.GetString("22ec7290-87bd-48c9-89fe-ee7d3c81ee42", "Create Underbond for Standalone AirCargo with House Bill '{0}' and with Flight No '{1}', Arrival Date '{2}'. Origin Premise ID '{3}' and Destination Premise ID '{4}'.",
										cusHAWB.CS_HAWB,
										SelectedUnderbond.C4_FlightNo,
										SelectedUnderbond.C4_ArrivalDate,
										SelectedUnderbond.C4_OriginPremiseID,
										SelectedUnderbond.C4_DestinationPremiseID));
								}

								if (matchUnderbonds != null && matchUnderbonds.Length > 1)
								{
									errorBuilder.AppendLine(
										Res.GetString("22ec7290-87bd-48c9-89fe-ee7d3c81ee43", "There are multiple Underbond for Standalone AirCargo with House Bill '{0}' and same Flight No '{1}', Arrival Date '{2}'. Origin Premise ID '{3}' and Destination Premise ID '{4}'. Delete all except one.",
										cusHAWB.CS_HAWB,
										SelectedUnderbond.C4_FlightNo,
										SelectedUnderbond.C4_ArrivalDate,
										SelectedUnderbond.C4_OriginPremiseID,
										SelectedUnderbond.C4_DestinationPremiseID));
								}
							}
						}
					}
				}
			}

			return errorBuilder.ToString();
		}

		public ScanCusMAWB GetStandAloneAirCargo(ZString houseBillNumber)
		{
			if (IsStandAlone)
			{
				return null;
			}

			var airCargo = hostedBO.FindStandAloneAirCargo(houseBillNumber)
				?? throw new InvalidOperationException();

			var standAloneCargo = new ScanCusMAWB(airCargo);
			standAloneCargo.SelectedUnderbond = this.SelectedUnderbond;
			return standAloneCargo;
		}

		public ScanCusMAWB GetAirCargoToAddSurplusConsignment()
		{
			if (IsStandAlone)
			{
				return this;
			}

			if (SelectedShipments == null)
			{
				return null;
			}

			foreach (CusHAWB cusHAWB in SelectedShipments)
			{
				if (cusHAWB.Shipment != null)
				{
					if (cusHAWB.IsHVLVShipment())
					{
						var airCargo = hostedBO.FindStandAloneAirCargo(cusHAWB.CS_HAWB);
						if (airCargo == null)
						{
							continue;
						}

						var standAloneCargo = new ScanCusMAWB(airCargo);
						standAloneCargo.SelectedUnderbond = this.SelectedUnderbond;
						return standAloneCargo;
					}
				}
			}

			return null;
		}

		public CusUnderbond GetUnderbond()
		{
			return hostedBO.GetMatchedToSelectedUnderbond(SelectedUnderbond);
		}

		public CusMAWB hostedBO { get; private set; }
	}
}
