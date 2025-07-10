using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class UNLOCODefaulter
	{
		public delegate ZPropertyInfo GetPropertyInfo();
		public delegate ZString GetTransportMode();

		public UNLOCODefaulter(BusinessObjectFactory factory, GetPropertyInfo getUNLOCOInfo, GetTransportMode getTransportMode)
		{
			this.factory = factory;
			this.getUNLOCOInfo = getUNLOCOInfo;
			this.getTransportMode = getTransportMode;
		}
		readonly BusinessObjectFactory factory;
		readonly GetPropertyInfo getUNLOCOInfo;
		readonly GetTransportMode getTransportMode;

		public void DefaultUNLOCOCode(ZString customsCode, bool onlyIsEmpty = true)
		{
			if (!IsDefaultingInProgress && !customsCode.IsEmpty && getUNLOCOInfo != null)
			{
				var unlocoInfo = getUNLOCOInfo();
				if (unlocoInfo != null && (!onlyIsEmpty || unlocoInfo.Value.IsEmpty))
				{
					using (new DefaulterLock(this))
					{
						unlocoInfo.Value = CACustomsCodesResolver.MatchingUNLOCO(customsCode, factory);
					}
				}
			}
		}

		public void DefaultCustomsCode(GetPropertyInfo getCustomsCodeInfo, CACustomsCodeType codeType, bool onlyIsEmpty = true)
		{
			if (!IsDefaultingInProgress && CACustomsDataRegistry.Instance.ShouldDefaultCustomsCodes.Value && getUNLOCOInfo != null && getCustomsCodeInfo != null)
			{
				var customsCodeInfo = getCustomsCodeInfo();
				var unlocoInfo = getUNLOCOInfo();
				var unlocoCode = unlocoInfo == null ? ZString.Empty : (ZString)unlocoInfo.Value;

				if (customsCodeInfo != null && !unlocoCode.IsEmpty && (!onlyIsEmpty || customsCodeInfo.Value.IsEmpty))
				{
					var defaultCode = GetFirstLocoMap(GetCustomsPortOfClearanceMappings(codeType, unlocoCode));
					if (!defaultCode.IsEmpty && defaultCode.Length <= customsCodeInfo.MaxLength)
					{
						using (new DefaulterLock(this))
						{
							customsCodeInfo.Value = defaultCode;
						}
					}
				}
			}
		}

		List<RefLocoMap> GetCustomsPortOfClearanceMappings(CACustomsCodeType codeType, ZString unlocoCode)
		{
			var transportMode = getTransportMode != null ? getTransportMode() : ZString.Empty;
			return CACustomsCodesResolver.GetMatchesForCodeType(codeType, unlocoCode, transportMode, factory);
		}

		static ZString GetFirstLocoMap(List<RefLocoMap> locoMaps)
		{
			if (locoMaps.Count == 1)
			{
				return locoMaps[0].RY_LocalPortCode;
			}
			return ZString.Empty;
		}

		bool IsDefaultingInProgress
		{
			get { return defaultIndex > 0; }
		}

		class DefaulterLock : IDisposable
		{
			public DefaulterLock(UNLOCODefaulter defaulter)
			{
				this.defaulter = defaulter;
				defaulter.defaultIndex++;
			}

			readonly UNLOCODefaulter defaulter;

			void IDisposable.Dispose()
			{
				defaulter.defaultIndex--;
			}
		}
		int defaultIndex;
	}
}
