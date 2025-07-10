using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class OrderHeaderStatusCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return ObjectFactory.Get<Enterprise.Freight.Integration.Forwarding.IOrderStatusListProvider>().GetOrderStatusList();
		}
	}
}
