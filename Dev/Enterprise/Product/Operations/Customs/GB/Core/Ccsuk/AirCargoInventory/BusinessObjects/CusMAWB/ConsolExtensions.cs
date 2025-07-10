using System;
using System.Linq;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public static class ConsolExtensions
	{
		public static bool IsValidForCcsukForPlugin(this ForwardingConsol consol)
		{
			return
				LicenceAndPimaHelper.AgentEnabled &&
				consol != null
				&& consol.JK_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.OrdinalIgnoreCase)
				&& TransportModeIsSuitable(consol)
				&& !consol.IsDomestic()
				&& !consol.JK_MasterBillNum.IsEmpty
				&& Env.Security.AirCcsukMaster.IsAllowed;
		}

		static bool TransportModeIsSuitable(ForwardingConsol consol)
		{
			return (consol.JK_TransportMode == Enterprise.Core.Constants.TransportModes.Air)
				||
				(
					consol.Shipments.OfType<ForwardingShipment>().Any(s => s.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.OrdinalIgnoreCase)
																		&& s.TransportMode == Enterprise.Core.Constants.TransportModes.Air)
					&& GBCustomsDataRegistry.Instance.CcsukAllowConsolPluginForNonAirModesWhenAtLeastOneGbAirShipment.Value
				);
		}
	}
}
