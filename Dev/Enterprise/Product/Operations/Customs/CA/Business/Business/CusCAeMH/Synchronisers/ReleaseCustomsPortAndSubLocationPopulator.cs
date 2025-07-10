using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class ReleaseCustomsPortAndSubLocationPopulator
	{
		public ReleaseCustomsPortAndSubLocationPopulator(ForwardingShipment source, CusCAeMHHouse destination)
		{
			this.source = source;
			this.destination = destination;
		}

		readonly ForwardingShipment source;
		readonly CusCAeMHHouse destination;

		#region Release Customs Port & Sub Location

		ZString GetCustomsCodeFromCFS(string customsCode)
		{
			return source.ImportReleaseDepot.GetCusCodesFromOrgAddress(customsCode);
		}

		ZString GetCustomsCodeFromConsolCFS(string customsCode)
		{
			var result = ZString.Empty;
			if (source.ArrivalConsol is ForwardingConsol consol)
			{
				result = consol.UnpackDepotAddress.GetCusCodesFromOrgAddress(customsCode);
			}
			return result;
		}

		ZString GetCustomsCodeFromConsolCTO(string customsCode)
		{
			var result = ZString.Empty;
			if (source.ArrivalConsol is ForwardingConsol consol)
			{
				result = consol.ArrivalCTOAddress.GetCusCodesFromOrgAddress(customsCode);
			}
			return result;
		}

		public IZType GetReleaseCustomsPort()
		{
			var result = ZString.Empty;
			var transportMode = source.JS_TransportMode;
			if (transportMode == TransportTypeList.Codes.Air || transportMode == TransportTypeList.Codes.Sea)
			{
				var transports = GetTransports().ToList();
				if (transports.Count > 0 && !transports[0].JW_RL_NKLoadPort.IsCanadaPort())
				{
					var caDiscPorts = transports.Where(x => x.JW_RL_NKDiscPort.IsCanadaPort()).ToArray();
					var caDiscPortCount = caDiscPorts.Length;
					if (caDiscPortCount >= 1)
					{
						if (transportMode == TransportTypeList.Codes.Air)
						{
							var lastTransport = transports.Last();
							if (lastTransport.JW_RL_NKDiscPort.IsCanadaPort())
							{
								result = GetCustomsCodeFromCFS(OrgCusCode.CACodeTypes.CustomsOfficeCode);
								if (result.IsEmpty && caDiscPortCount == 1)
								{
									result = GetCustomsCodeFromConsolCFS(OrgCusCode.CACodeTypes.CustomsOfficeCode);
								}
							}
							else
							{
								result = caDiscPorts.Last().GetFirstLocoMapFromDischargePort(CACustomsCodeType.Office, transportMode);
							}

							if (result.IsEmpty && destination.MasterBill is CusCAeMHMaster masterBill && CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.GetFallBackValueAtAllLevels(masterBill.Company.PK.ToGuid(), (masterBill.Branch?.PK ?? ZGuid.Empty).ToGuid(), Guid.Empty))
							{
								var lastDescCAPort = caDiscPorts.Last();
								result = lastDescCAPort.GetFirstLocoMapFromDischargePort(CACustomsCodeType.Office, transportMode);
							}
						}
						else if (transportMode == TransportTypeList.Codes.Sea)
						{
							var lastTransport = transports.Last();
							var packMode = source.JS_PackingMode;
							if (lastTransport.JW_RL_NKDiscPort.IsCanadaPort())
							{
								if (packMode == Core.Constants.ContainerModes.LCL)
								{
									result = GetCustomsCodeFromCFS(OrgCusCode.CACodeTypes.CustomsOfficeCode);
								}
								else if (packMode == Core.Constants.ContainerModes.FCL)
								{
									result = GetCustomsCodeFromConsolCTO(OrgCusCode.CACodeTypes.CustomsOfficeCode);
								}
							}
							else if (caDiscPortCount == 1)
							{
								if (packMode == Core.Constants.ContainerModes.LCL)
								{
									result = GetCustomsCodeFromCFS(OrgCusCode.CACodeTypes.CustomsOfficeCode);
								}
								else if (packMode == Core.Constants.ContainerModes.FCL)
								{
									var lastCAport = caDiscPorts.Last();
									if (lastCAport != null)
									{
										var arrivalAtOrg = lastCAport.ArrivalLocation;
										result = arrivalAtOrg.GetCusCodesFromOrgAddress(OrgCusCode.CACodeTypes.CustomsOfficeCode);
									}
								}
							}
							else
							{
								var depFromOrg = transports.LastOrDefault(x => x.JW_RL_NKLoadPort.IsCanadaPort())?.DepartureLocation;
								if (depFromOrg != null)
								{
									result = depFromOrg.GetCusCodesFromOrgAddress(OrgCusCode.CACodeTypes.CustomsOfficeCode);
								}
							}
						}
					}
				}
			}
			else
			{
				result = GetCustomsCodeFromCFS(OrgCusCode.CACodeTypes.CustomsOfficeCode);
				if (result.IsEmpty)
				{
					result = GetCustomsCodeFromConsolCFS(OrgCusCode.CACodeTypes.CustomsOfficeCode);
				}
				if (result.IsEmpty)
				{
					result = destination.BW_CBSAReleasePort;
				}
			}

			return result;
		}

		public IZType GetReleaseSubLocation()
		{
			var result = ZString.Empty;
			var transportMode = source.JS_TransportMode;
			if (transportMode == TransportTypeList.Codes.Air || transportMode == TransportTypeList.Codes.Sea)
			{
				var transports = GetTransports().ToList();
				if (transports.Count > 0 && !transports[0].JW_RL_NKLoadPort.IsCanadaPort())
				{
					var caDiscPorts = transports.Where(x => x.JW_RL_NKDiscPort.IsCanadaPort()).ToArray();
					var caDiscPortCount = caDiscPorts.Length;
					if (caDiscPortCount >= 1)
					{
						if (transportMode == TransportTypeList.Codes.Air)
						{
							var lastTransport = transports.Last();
							if (lastTransport.JW_RL_NKDiscPort.IsCanadaPort())
							{
								result = GetCustomsCodeFromCFS(OrgCusCode.CodeTypes.ControlledPremisesID);
								if (result.IsEmpty && caDiscPortCount == 1)
								{
									result = GetCustomsCodeFromConsolCFS(OrgCusCode.CodeTypes.ControlledPremisesID);
								}
							}
							else
							{
								var lastDescCAPort = caDiscPorts.Last();
								result = lastDescCAPort.GetCusCodesFromCarrierAirCTO(OrgCusCode.CodeTypes.ControlledPremisesID);
							}
						}
						else if (transportMode == TransportTypeList.Codes.Sea)
						{
							var packMode = source.JS_PackingMode;
							var lastTransport = transports.Last();
							if (lastTransport.JW_RL_NKDiscPort.IsCanadaPort())
							{
								if (packMode == Core.Constants.ContainerModes.LCL)
								{
									result = GetCustomsCodeFromCFS(OrgCusCode.CodeTypes.ControlledPremisesID);
								}
								else if (packMode == Core.Constants.ContainerModes.FCL)
								{
									result = GetCustomsCodeFromConsolCTO(OrgCusCode.CodeTypes.ControlledPremisesID);
								}
							}
							else if (caDiscPortCount == 1)
							{
								if (packMode == Core.Constants.ContainerModes.LCL)
								{
									result = GetCustomsCodeFromCFS(OrgCusCode.CodeTypes.ControlledPremisesID);
								}
								else if (packMode == Core.Constants.ContainerModes.FCL)
								{
									var lastCAport = caDiscPorts.Last();
									if (lastCAport != null)
									{
										var arrivalAtOrg = lastCAport.ArrivalLocation;
										result = arrivalAtOrg.GetCusCodesFromOrgAddress(OrgCusCode.CodeTypes.ControlledPremisesID);
									}
								}
							}
							else
							{
								var depFromOrg = transports.LastOrDefault(x => x.JW_RL_NKLoadPort.IsCanadaPort())?.DepartureLocation;
								if (depFromOrg != null)
								{
									result = depFromOrg.GetCusCodesFromOrgAddress(OrgCusCode.CodeTypes.ControlledPremisesID);
								}
							}
						}
					}
				}
			}
			else
			{
				result = GetCustomsCodeFromCFS(OrgCusCode.CodeTypes.ControlledPremisesID);
				if (result.IsEmpty)
				{
					result = GetCustomsCodeFromConsolCFS(OrgCusCode.CodeTypes.ControlledPremisesID);
				}
				if (result.IsEmpty)
				{
					result = destination.BW_CBSAReleaseSubLocation;
				}
			}
			return result;
		}

		IEnumerable<Transport> GetTransports()
		{
			return source.ArrivalConsol?.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder) ?? Enumerable.Empty<Transport>();
		}

		#endregion
	}
}
