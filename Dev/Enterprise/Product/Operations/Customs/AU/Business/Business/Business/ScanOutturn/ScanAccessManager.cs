using Enterprise.Environment;
using Enterprise.Licensing;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ScanAccessManager
	{
		public static LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.HVLVClearance; }
		}

		public bool HasAccess
		{
			get { return Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance; }
		}

		public string LicenceName
		{
			get
			{
				return Env.Licence.HVLVClearance.DisplayName;
			}
		}
	}
}
