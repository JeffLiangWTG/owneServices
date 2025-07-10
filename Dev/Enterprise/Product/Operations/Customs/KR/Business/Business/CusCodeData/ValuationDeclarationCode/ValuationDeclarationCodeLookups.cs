using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationCodeLookups : CusCodeDataLookups
	{
		public ValuationDeclarationCodeLookups(AutoCusCodeData parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PriceDeclarationItemCodeList => Factory.GetCachedValue<PriceDeclarationItemCodeList>();
	}
}
