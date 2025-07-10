using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class DeclarationBorderTransportMeansItineraryWrapper : IDeclarationBorderTransportMeansItinerary
	{
		DeclarationBorderTransportMeansItineraryWrapper(ZString countryCode)
		{
			this.countryCode = Argument.NotNull(countryCode, nameof(countryCode));
		}

		internal static DeclarationBorderTransportMeansItineraryWrapper NewOrNull(ZString countryCode)
		=> countryCode.IsEmpty ? null : new DeclarationBorderTransportMeansItineraryWrapper(countryCode);

		public ICodeType RoutingCountryCode => routingCountryCode ?? (routingCountryCode = CodeTypeWrapper.NewOrNull(countryCode));
		ICodeType routingCountryCode;

		readonly ZString countryCode;
	}
}
