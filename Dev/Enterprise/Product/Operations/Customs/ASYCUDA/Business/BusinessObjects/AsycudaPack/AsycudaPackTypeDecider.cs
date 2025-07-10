using System;
using CargoWise.Application;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackTypeDecider : ManifestBase.AsycudaPackTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaPack);
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaPack>();
		}
	}
}
