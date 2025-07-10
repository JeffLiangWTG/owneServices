using System;
using CargoWise.Application;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTaxTypeDecider : ManifestBase.AsycudaTaxTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaTax);
		}

		public override Type GetTypeForNew()
		{
			return ObjectFactory.GetType<Integration.Customs.ASYCUDA.IAsycudaTax>();
		}
	}
}
