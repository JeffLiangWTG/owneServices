using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerLookups : Customs.Business.CusSCAContainerLookups
	{
		public CusSCAContainerLookups(Customs.Business.BaseCusSCAContainer parent)
			: base(parent)
		{
		}

		public CMRContainerTypes TypesOfContainer
		{
			get { return Factory.GetCachedValue<CMRContainerTypes>(); }
		}

		public CMRContainerSizes ContainerSizes
		{
			get { return Factory.GetCachedValue<CMRContainerSizes>(); }
		}

		public CMRImportCargoTypes CargoTypes
		{
			get { return Factory.GetCachedValue<CMRImportCargoTypes>(); }
		}
	}
}
