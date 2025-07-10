using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public class ITBox18IdentityOfTransportAtDepartureBuilder
{
	public ITBox18IdentityOfTransportAtDepartureBuilder(JobDeclaration jobDeclaration)
	{
		declaration = Argument.NotNull(jobDeclaration, nameof(jobDeclaration));
	}

	readonly JobDeclaration declaration;

	public ZString GetBox18IdentityOfTransportAtDepartureFormatted()
	{
		if (declaration.IsUCC6AndIsExport)
		{
			var transportIDInland = declaration.JE_TransportIDInland;

			if (declaration.IsAirInland)
			{
				var aircraftRegistrationInland = declaration.JE_AircraftRegistrationInland;

				return !aircraftRegistrationInland.IsEmpty ? aircraftRegistrationInland : transportIDInland;
			}
			else if (declaration.IsRailInland)
			{
				return !transportIDInland.IsEmpty ? transportIDInland : declaration.JE_Trailer1RegNo;
			}
			else
			{
				return transportIDInland;
			}
		}
		else
		{
			return declaration.ZG_Box18TransportID;
		}
	}
}
