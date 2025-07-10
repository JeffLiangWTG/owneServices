using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CustomsCodesDefaulter
	{
		public delegate IEnumerable<OrgAddress> GetRelatedAddresses();
		public delegate IEnumerable<Transport> GetTransports();
		public delegate ZString GetTransportMode();

		public CustomsCodesDefaulter(BusinessObjectFactory factory, ZPropertyInfo customsCodeInfo, GetRelatedAddresses getAddresses, GetTransports getTransports, GetTransportMode getTransportMode, ZString orgCusCodeType, ZBool fallbackUNLOCO)
		{
			this.factory = factory;
			this.customsCodeInfo = customsCodeInfo;
			this.getAddresses = getAddresses;
			this.getTransports = getTransports;
			this.getTransportMode = getTransportMode;
			this.orgCusCodeType = orgCusCodeType;
			this.fallbackUNLOCO = fallbackUNLOCO;
		}

		readonly BusinessObjectFactory factory;
		readonly ZPropertyInfo customsCodeInfo;
		readonly GetRelatedAddresses getAddresses;
		readonly GetTransports getTransports;
		readonly GetTransportMode getTransportMode;
		readonly ZString orgCusCodeType;
		readonly ZBool fallbackUNLOCO;

		public void DefaultCustomsCode(bool onlyIsEmpty = true)
		{
			if (!IsDefaultingInProgress && (!onlyIsEmpty || customsCodeInfo.Value.IsEmpty))
			{
				var defaultCode = GetDefaultCustomsCode();
				if (!defaultCode.IsEmpty && defaultCode.Length <= customsCodeInfo.MaxLength)
				{
					using (new DefaulterLock(this))
					{
						customsCodeInfo.Value = defaultCode;
					}
				}
			}
		}

		public ZString GetDefaultCustomsCode()
		{
			var custPort = ZString.Empty;
			if (getTransports != null && getTransportMode != null)
			{
				var transports = getTransports().ToList();
				var sourceTransportsCount = transports.Count;
				var transportMode = getTransportMode();
				if (sourceTransportsCount <= 1 || (sourceTransportsCount > 1 && transportMode != Core.Constants.TransportModes.Sea))
				{
					custPort = GetPortFromOrganization(orgCusCodeType);
				}
				else if (fallbackUNLOCO && sourceTransportsCount > 1 && transportMode == Core.Constants.TransportModes.Sea)
				{
					var lsatTransport = transports.Last();
					if (lsatTransport != null)
					{
						var refLocoMappings = CACustomsCodesResolver.GetMatchesForCodeType(CACustomsCodeType.Office, lsatTransport.JW_RL_NKDiscPort, Core.Constants.TransportModes.Sea, factory);
						custPort = GetFirstLocoMap(refLocoMappings);
					}
				}
			}
			return custPort.SubstringSafe(0, 4);
		}

		static ZString GetFirstLocoMap(List<RefLocoMap> locoMaps)
		{
			if (locoMaps.Count == 1)
			{
				return locoMaps[0].RY_LocalPortCode;
			}
			return ZString.Empty;
		}

		ZString GetPortFromOrganization(ZString codeType)
		{
			if (getAddresses != null)
			{
				var addresses = getAddresses();
				foreach (var address in addresses)
				{
					var officeCode = address.Header?.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.Canada, address.PK) ?? ZString.Empty;
					if (!officeCode.IsEmpty)
					{
						return officeCode;
					}
				}
			}
			return ZString.Empty;
		}

		bool IsDefaultingInProgress
		{
			get { return defaultIndex > 0; }
		}

		class DefaulterLock : IDisposable
		{
			public DefaulterLock(CustomsCodesDefaulter defaulter)
			{
				this.defaulter = defaulter;
				defaulter.defaultIndex++;
			}

			readonly CustomsCodesDefaulter defaulter;

			void IDisposable.Dispose()
			{
				defaulter.defaultIndex--;
			}
		}
		int defaultIndex;
	}
}
