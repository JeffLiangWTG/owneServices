using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusDV1DetailLookups : AutoCusDV1DetailLookups
	{
		public CusDV1DetailLookups(AutoCusDV1Detail parent) : base(parent)
		{
		}
		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();
	}
}
