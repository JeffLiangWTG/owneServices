using System;
using CargoWise.Application;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaContainerBillOrPackageLinkDecider : ManifestBase.AsycudaContainerBillOrPackageLinkTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaContainerBillOrPackageLink);
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainerBillOrPackageLink>();
		}
	}
}
