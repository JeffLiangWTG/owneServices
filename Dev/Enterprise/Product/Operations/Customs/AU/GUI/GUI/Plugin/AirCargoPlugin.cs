using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public abstract class AirCargoPlugIn : CustomsManifestPlugIn
	{
		public AirCargoPlugIn(IManifestProvider hostBusinessEntity) : base(hostBusinessEntity)
		{
		}

		internal TriLockMutex MutexForConsol
		{
			get
			{
				if (fMutexForConsol == null && Consol != null)
				{
					fMutexForConsol = CusMAWB.CreateMutexForConsol(Consol.PK, Core.Constants.CountryCodes.Australia);
				}
				return fMutexForConsol;
			}
		}
		TriLockMutex fMutexForConsol;

		#region Implementation

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get
			{
				return Env.Licence.ImportManifest;
			}
		}

		protected abstract ForwardingConsol Consol
		{
			get;
		}

		#endregion
	}
}
