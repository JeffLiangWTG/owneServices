using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business.Ncts.Common
{
	sealed class NctsPhase5RuleR0474_1Validation
	{
		internal NctsPhase5RuleR0474_1Validation(IDepartureTransportMeansProvider departureTransportMeansProvider, NctsHeader header)
		{
			this.departureTransportMeansProvider = Argument.NotNull(departureTransportMeansProvider, nameof(departureTransportMeansProvider));
			this.header = Argument.NotNull(header, nameof(header));
		}

		public void ValidateTransportAtDeparture(ZPropertyInfo targetInfo, INctsBillDeparturePhase5ValidationDecider billValidationDecider)
		{
			if (billValidationDecider is { IsRuleR0474_1Active: true }
				&& departureTransportMeansProvider.InlandTransportModeAtDeparture == ModeOfTransportList.Codes._3_RoadTransport
				&& departureTransportMeansProvider.TransportAtDeparture.IsEmpty
				&& (!departureTransportMeansProvider.Trailer1IDAtDeparture.IsEmpty || !departureTransportMeansProvider.Trailer2IDAtDeparture.IsEmpty))
			{
				targetInfo.AddMessageError(header.Configuration.ValidationRuleConfiguration.Messages.R0474_1Message);
			}
		}

		readonly NctsHeader header;
		readonly IDepartureTransportMeansProvider departureTransportMeansProvider;
	}
}
