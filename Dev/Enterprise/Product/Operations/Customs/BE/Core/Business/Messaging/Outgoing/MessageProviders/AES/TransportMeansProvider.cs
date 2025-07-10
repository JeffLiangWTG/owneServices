using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class TransportMeansProvider : ITransportMeans
{
	readonly JobDeclaration declaration;
	public TransportMeansProvider(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	public int TypeOfIdentification => ZInt.ParseSafe(declaration.ZG_BorderTransportMeans, ZInt.Zero);

	public string IdentificationNumber
	{
		get
		{
			var returnValue = string.Empty;

			switch (TypeOfIdentification)
			{
				case 10:
					returnValue = declaration.Vessel?.RV_LloydsNumber;
					break;
				case 11:
				case 21:
				case 30:
				case 80:
				case 81:
					returnValue = declaration.JE_VesselName;
					break;
				case 40:
				case 41:
					returnValue = declaration.JE_VoyageFlightNo;
					break;
			}

			return returnValue;
		}
	}

	public string Nationality => declaration.JE_RN_NKTransportNationality;
}
