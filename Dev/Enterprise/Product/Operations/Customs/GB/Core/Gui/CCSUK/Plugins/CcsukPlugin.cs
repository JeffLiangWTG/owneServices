using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public abstract class CcsukPlugin : CustomsCargoManifestPlugin
	{
		public CcsukPlugin(IManifestProvider hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		ZGlobalMutex fMutexForConsol;
		public ZGlobalMutex MutexForConsol
		{
			get
			{
				if (fMutexForConsol == null && Consol != null)
				{
					fMutexForConsol = CusMAWB.CreateMutexForConsol(Consol.PK, Core.Constants.CountryCodes.UnitedKingdom);
				}
				return fMutexForConsol;
			}
		}

		#region Implementation

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AirCcsukBase; }
		}

		protected abstract ForwardingConsol Consol
		{
			get;
		}

		#endregion

		protected override CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			throw new NotSupportedException("GetCustomsManifestStatus is not for GB");
		}
	}
}
