using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class ArrivalMeansOfTransportWrapper : IArrivalMeansOfTransport
{
	ArrivalMeansOfTransportWrapper(JobDeclaration declaration)
	{
		this.declaration = declaration;

		lazyType = new Lazy<int>(GetTransportModeInlandType);
	}

	readonly JobDeclaration declaration;

	public static ArrivalMeansOfTransportWrapper NewOrNull(JobDeclaration declaration)
	{
		Argument.NotNull(declaration, nameof(declaration));

		if (declaration.ZG_Box18TransportID.IsEmpty && declaration.JE_TransportMeans.IsEmpty)
		{
			return null;
		}
		return new ArrivalMeansOfTransportWrapper(declaration);
	}

	string IArrivalMeansOfTransport.IdentificationNumber => declaration.ZG_Box18TransportID;

	int IArrivalMeansOfTransport.TransportMode => lazyType.Value;
	readonly Lazy<int> lazyType;

	#region Implementation

	int GetTransportModeInlandType()
	{
		if (int.TryParse(declaration.JE_TransportMeans, out var result))
		{
			return result;
		}

		return 0;
	}

	#endregion
}
