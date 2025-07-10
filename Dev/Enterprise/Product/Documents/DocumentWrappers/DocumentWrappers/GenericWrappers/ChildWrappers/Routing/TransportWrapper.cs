using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Reference")]
	public class TransportWrapper : GenericWrapper
	{
		public TransportWrapper(ZString transportMode, ZString vessel, ZString voyageFlight, ZDateTime eTD, ZDateTime aTD, BusinessObjectFactory factory, bool forceShowVesselVoyage = false)
			: base(null, factory)
		{
			fTransportModeCode = transportMode;
			fVessel = vessel;
			fVoyageFlight = voyageFlight;
			fETD = eTD;
			fATD = aTD;
			this.forceShowVesselVoyage = forceShowVesselVoyage;
		}

		public ZString Reference
		{
			get
			{
				ZString result = ZString.Empty;
				switch (fTransportModeCode)
				{
					case TransportModeList.Codes.Airfreight:
						result = FlightAndDate;
						break;

					case TransportModeList.Codes.Seafreight:
						result = GetCombinedValue(GetCombinedValue(fVessel, fVoyageFlight, " / "), LloydsNo, " / ");
						break;

					default:
						result = GetCombinedValue(GetCombinedValue(fVessel, fVoyageFlight, " / "), DepartureDateAsShortDateString, " / ");
						break;
				}

				return result;
			}
		}

		public ZString ReferenceLabel
		{
			get
			{
				ZString result = ZString.Empty;
				if (forceShowVesselVoyage)
				{
					result = Res.GetString("4aea32af-5215-4b60-9da4-d2501e1177c2", "Vessel / Voyage");
				}
				else
				{
					switch (fTransportModeCode)
					{
						case TransportModeList.Codes.Airfreight:
							result = Res.GetString("70033e5a-23b0-4b34-a952-67d2ee87da17", "Flight / Date");
							break;
						case TransportModeList.Codes.Seafreight:
							result = Res.GetString("60bef0b4-f053-4e42-aae4-c50db36be8e6", "Vessel / Voyage / IMO(Lloyds)");
							break;
						case TransportModeList.Codes.Road:
							result = Res.GetString("a2edfead-c6de-4276-920d-a8e5de7cbb5c", "Road Reference");
							break;
						case TransportModeList.Codes.Rail:
							result = Res.GetString("c2a7fe72-32c6-4ce2-85d7-2c1c9ef95d37", "Rail Reference");
							break;
					}
				}
				return result;
			}
		}

		public CodeAndDescriptionWrapper Mode
		{
			get
			{
				if (fMode == null)
				{
					fMode = new CodeAndDescriptionWrapper(fTransportModeCode, new TransportModeList(), Factory);
				}
				return fMode;
			}
		}

		public ZString LloydsNo
		{
			get
			{
				if (!fVessel.IsEmpty)
				{
					RefVesselCollection result = new RefVesselCollection(Factory, new ZQuery(ZArchitecture.Schema.RefVesselSchema.RV_Code, fVessel));
					if (result.Count > 0 && !result[0].RV_LloydsNumber.IsEmpty)
					{
						return result[0].RV_LloydsNumber;
					}
				}
				return ZString.Empty;
			}
		}

		public ZString VesselName
		{
			get { return (IsSea || forceShowVesselVoyage ? fVessel : ZString.Empty); }
		}

		public ZString VoyageNo
		{
			get { return (IsSea || forceShowVesselVoyage ? fVoyageFlight : ZString.Empty); }
		}

		public ZDateTime VoyageDate // This should be removed, VoyageDate is not a standard means of identifying a means of Transport, it's related to a Route.
		{
			get { return (IsSea ? DepartureDate : ZDateTime.Empty); }
		}

		public ZString FlightNo
		{
			get { return (IsAir ? fVoyageFlight : ZString.Empty); }
		}

		public ZDateTime FlightDate
		{
			get { return (IsAir ? DepartureDate : ZDateTime.Empty); }
		}

		public ZString VoyageFlight
		{
			get { return fVoyageFlight; }
		}

		#region Implementation
		readonly ZBool forceShowVesselVoyage;
		readonly ZString fTransportModeCode;
		CodeAndDescriptionWrapper fMode;
		readonly ZString fVessel;
		readonly ZString fVoyageFlight;
		readonly ZDateTime fETD;
		readonly ZDateTime fATD;

		bool IsSea
		{
			get { return fTransportModeCode == TransportModeList.Codes.Seafreight; }
		}

		bool IsAir
		{
			get { return fTransportModeCode == TransportModeList.Codes.Airfreight; }
		}

		ZDateTime DepartureDate
		{
			get { return fATD.IsEmpty ? fETD : fATD; }
		}

		ZString DepartureDateAsShortDateString
		{
			get
			{
				var departureDate = DepartureDate;
				return departureDate.IsEmpty ? ZString.Empty : new ZString(departureDate.ToString("dd-MMM"));
			}
		}

		ZString FlightAndDate
		{
			get { return GetCombinedValue(fVoyageFlight, DepartureDateAsShortDateString, " / "); }
		}
		#endregion
	}
}
