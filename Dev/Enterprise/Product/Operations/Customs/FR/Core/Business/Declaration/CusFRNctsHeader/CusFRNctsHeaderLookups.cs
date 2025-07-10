using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class CusFRNctsHeaderLookups : AutoCusFRNctsHeaderLookups
	{
		public CusFRNctsHeaderLookups(AutoCusFRNctsHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList NatureOfSealsList => Factory.GetCachedValue<NatureOfSealsList>();
	}
}
