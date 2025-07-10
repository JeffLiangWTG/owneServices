using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	static class CusCAeMHMasterSynchroniserExtension
	{
		public static ZString GetFirstLocoMapFromDischargePort(this Transport transport, CACustomsCodeType type, ZString transportMode)
		{
			var result = ZString.Empty;
			var port = transport?.DiscPort;
			if (port != null)
			{
				var refLocoMappings = CACustomsCodesResolver.GetMatchesForCodeType(type, port, transportMode);
				if (refLocoMappings.Count == 1)
				{
					result = refLocoMappings[0].RY_LocalPortCode.Left(4);
				}
			}
			return result;
		}

		public static ZString GetCusCodesFromCarrierAirCTO(this Transport transport, ZString cusCodeType)
		{
			var carrier = transport?.Carrier;
			if (carrier != null)
			{
				var dischargePort = transport.JW_RL_NKDiscPort;
				OrgCarrierAppointedAgentPorts port_AirCTO = null;
				foreach (var port in carrier.CarrierAppointedAgentPorts_AirCTO.Cast<OrgCarrierAppointedAgentPorts>())
				{
					if (port.O5_PortOrCountry == dischargePort)
					{
						port_AirCTO = port;
						break;
					}
					else if (port.O5_PortOrCountry == Core.Constants.CountryCodes.Canada)
					{
						port_AirCTO = port;
					}
				}
				var airCTO = port_AirCTO?.Organisation;
				if (airCTO != null)
				{
					var ccpCodes = airCTO.CustomsCodes.GetAllOrgCusCodesForCountryAndCodes(Core.Constants.CountryCodes.Canada, cusCodeType);

					if (ccpCodes.Length > 1)
					{
						foreach (var ccpCode in ccpCodes)
						{
							if (ccpCode.PremisesAddress != null && ccpCode.PremisesAddress.OA_RL_NKRelatedPortCode == dischargePort)
							{
								return ccpCode.OK_CustomsRegNo;
							}
						}
					}

					if (ccpCodes.Length > 0)
					{
						return ccpCodes[0].OK_CustomsRegNo;
					}
				}
			}
			return ZString.Empty;
		}

		public static ZString GetCusCodesFromOrgAddress(this OrgAddress address, ZString cusCodeType)
		{
			if (address != null)
			{
				return address.CustomsCodes.GetCustomsRegNo(cusCodeType, Core.Constants.CountryCodes.Canada);
			}
			return ZString.Empty;
		}

		public static ZBool IsCanadaPort(this ZString port)
		{
			return port.StartsWith(Core.Constants.CountryCodes.Canada);
		}
	}
}
