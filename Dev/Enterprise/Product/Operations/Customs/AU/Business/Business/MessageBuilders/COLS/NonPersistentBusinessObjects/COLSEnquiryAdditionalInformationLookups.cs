using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class COLSEnquiryAdditionalInformationLookups : ZLookups
	{
		public COLSEnquiryAdditionalInformationLookups(COLSEnquiryAdditionalInformation parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList EnquiryType => Factory.GetCachedValue<COLSEnquiryTypeList>();
	}
}
