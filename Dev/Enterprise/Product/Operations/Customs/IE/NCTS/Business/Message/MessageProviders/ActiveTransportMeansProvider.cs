using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class ActiveTransportMeansProvider : IActiveTransportMeans
	{
		public ActiveTransportMeansProvider(NctsDepartureMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}
		readonly NctsDepartureMovementHeader movementHeader;

		public string CustomsOfficeAtBorderReferenceNumber => movementHeader.BM_CustomsOfficeAtBorder;

		public string IdentificationType => movementHeader.BM_ActiveBorderIdentificationType;

		public string IdentificationNumber => movementHeader.BM_TOLCarrierID.ToUpperInvariant();

		public string Nationality => movementHeader.BM_RN_NKTOLCarrierNationality;

		public string ConveyanceReferenceNumber => movementHeader.BM_ConveyanceNumber;

		public IReadOnlyCollection<IActiveTransportMeans> AsReadOnlyCollection()
		{
			if (transports.IsNullOrEmpty())
			{
				transports = new List<IActiveTransportMeans>();
				transports.Add(this);
				foreach (var additionalTransport in movementHeader.AdditionalTransportAtBorderList)
				{
					transports.Add(new AdditionalActiveTransportMeansProvider(additionalTransport));
				}
			}
			return transports;
		}
		List<IActiveTransportMeans> transports;

		class AdditionalActiveTransportMeansProvider : IActiveTransportMeans
		{
			readonly DepartureCusTransportMeans transport;
			public AdditionalActiveTransportMeansProvider(DepartureCusTransportMeans transport)
			{
				this.transport = Argument.NotNull(transport, nameof(transport));
			}
			public string CustomsOfficeAtBorderReferenceNumber => transport.TPM_CustomsOffice;

			public string IdentificationType => transport.TPM_TypeOfIdentification;

			public string IdentificationNumber => transport.TPM_IdentificationNumber;

			public string Nationality => transport.TPM_RN_NKTransportNationality;

			public string ConveyanceReferenceNumber => transport.TPM_ReferenceNumber;
		}
	}
}
