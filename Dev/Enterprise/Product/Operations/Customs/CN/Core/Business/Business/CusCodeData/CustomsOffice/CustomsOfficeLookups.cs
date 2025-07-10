using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CustomsOfficeLookups : CusCodeDataLookups
	{
		public CustomsOfficeLookups(CusCodeData parent) : base(parent)
		{
		}

		public new CustomsOffice Parent => (CustomsOffice)base.Parent;

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<CustomsOfficeTypeList>();

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, Parent.Declaration?.DateOfValuation ?? ZDateTime.Today);
	}
}
