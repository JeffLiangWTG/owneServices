using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestLineLookups : Customs.Business.CusMiscRequestLineLookups
	{
		public CusMiscRequestLineLookups(AutoCusMiscRequestLine parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ReferenceNumberTypeList => Factory.GetCachedValue<ReferenceNumberTypeList>();
	}
}
