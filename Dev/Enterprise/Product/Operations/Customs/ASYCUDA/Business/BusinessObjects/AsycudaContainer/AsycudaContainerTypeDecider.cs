using System;
using CargoWise.Application;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaContainerTypeDecider : ManifestBase.AsycudaContainerTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaContainer);
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainer>();
		}
	}
}
