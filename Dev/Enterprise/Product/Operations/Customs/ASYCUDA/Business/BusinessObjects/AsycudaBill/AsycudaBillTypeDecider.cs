using System;
using CargoWise.Application;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaBillTypeDecider : ManifestBase.AsycudaBillTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaBill);
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill>();
		}
	}
}
