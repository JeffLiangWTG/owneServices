using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin
{
	public class TransportDetailWrapper : ITransportDetail
	{
		public TransportDetailWrapper(JobDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		protected JobDeclaration Declaration { get; }

		ZString ITransportDetail.Voyage => voyage ?? (voyage = GetVoyage()).ToUpperInvariant();
		string voyage;

		protected virtual ZString GetVoyage()
		{
			switch (Declaration.TransportMode)
			{
				case Core.Constants.TransportModes.Sea:
					return FormattableString.Invariant($"{Declaration.JE_VesselName} {Declaration.JE_VoyageFlightNo}").Trim();

				case Core.Constants.TransportModes.Air:
					return Declaration.JE_VoyageFlightNo;

				default:
					return Declaration.JE_VesselName;
			}
		}
	}
}
