using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class ContentInformationTypeLookups : CusCodeDataLookups
	{
		public ContentInformationTypeLookups(ContentInformationType officeCode) : base(officeCode)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<ContentInfoTypeList>();
	}
}
