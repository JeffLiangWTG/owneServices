using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CO2eEmissionWrapper : GenericWrapper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description Zstring")]
		public CO2eEmissionWrapper(ICO2eLegBasedSupporter supporter, ICO2eLegProvider leg, BusinessObjectFactory factory)
			: base(supporter as BusinessObject, factory)
		{
			Argument.NotNull(supporter, nameof(supporter));
			Argument.NotNull(leg, nameof(leg));

			Weight = supporter.Weight;
			TransportMode = leg.TransportMode;
			RouteText = (leg.LoadPort.IsEmpty || leg.DiscPort.IsEmpty) ? ZString.Empty : new ZString(leg.LoadPort + " to " + leg.DiscPort);

			using (leg.WithTempCurrentCO2eCalcSupporter(supporter))
			{
				if (leg.GetCO2eStatus() == CO2eStatusList.Codes.Current)
				{
					FormattedCO2e = CO2eHelper.GetFormattedCO2e(leg.DynamicTotalCO2e);
				}
			}
		}

		public ZDecimal Weight { get; private set; }
		public ZString TransportMode { get; private set; }
		public ZString RouteText { get; private set; }
		public ZString FormattedCO2e { get; private set; }
	}
}
