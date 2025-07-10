using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public class UNLOCO_CNPortsDefaulter
	{
		public UNLOCO_CNPortsDefaulter(BusinessObjectFactory factory, ZPropertyInfo port, ZPropertyInfo unloco, bool useCHN000ForChinaPort = false)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.port = Argument.NotNull(port, nameof(port));
			this.unloco = Argument.NotNull(unloco, nameof(unloco));
			this.useCHN000ForChinaPort = useCHN000ForChinaPort;
		}
		readonly BusinessObjectFactory factory;
		readonly bool useCHN000ForChinaPort;
		readonly ZPropertyInfo port;
		readonly ZPropertyInfo unloco;

		public void DefaultUNLOCO()
		{
			if (!port.Value.IsEmpty && !IsDefaultingInProgress)
			{
				using (new DefaulterLock(this))
				{
					var result = CNScheduleResolver.GetMatchedUNLOCO((ZString)port.Value, factory);
					if (!result.IsEmpty)
					{
						unloco.Value = result;
					}
				}
			}
		}

		public void DefaultPort()
		{
			var uNLOCOValue = new ZString(unloco.Value);
			if (uNLOCOValue.Length > 1 && !IsDefaultingInProgress)
			{
				using (new DefaulterLock(this))
				{
					ZString result = ZString.Empty;
					var countryCode = ((ZString)unloco.Value).Left(2);
					var refLocoMapping = useCHN000ForChinaPort && countryCode == Core.Constants.CountryCodes.China ? null : CNScheduleResolver.GetMatchedLocoMap(uNLOCOValue, factory);
					if (refLocoMapping != null)
					{
						result = refLocoMapping.RY_LocalPortCode;
					}
					else
					{
						var isoAlpha3Code = RefCountry.LoadFromCountryCode(factory, countryCode).GetCNCountryCode();
						if (!isoAlpha3Code.IsEmpty)
						{
							result = isoAlpha3Code + "000";
						}
					}

					if (!result.IsEmpty)
					{
						port.Value = result.Left(port.MaxLength);
					}
				}
			}
		}

		bool IsDefaultingInProgress => defaultIndex > 0;
		int defaultIndex;

		class DefaulterLock : IDisposable
		{
			public DefaulterLock(UNLOCO_CNPortsDefaulter defaulter)
			{
				this.defaulter = defaulter;
				defaulter.defaultIndex++;
			}

			readonly UNLOCO_CNPortsDefaulter defaulter;

			void IDisposable.Dispose()
			{
				defaulter.defaultIndex--;
			}
		}
	}
}
