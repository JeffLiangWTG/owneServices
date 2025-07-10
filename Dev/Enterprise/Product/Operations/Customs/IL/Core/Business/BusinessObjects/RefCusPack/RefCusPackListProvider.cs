using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.IL;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public sealed class RefCusPackListProvider : MasterFiles.Business.RefCusPackListProvider, IRefCusPackListProvider
	{
		public RefCusPackListProvider()
		{
		}

		public override CodeDescriptionPairList GetCustomsPackList(BusinessObjectFactory factory, ZString type, ZString country)
		{
			return factory.GetPackagesTypesList();
		}
	}
}
