using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalInfoCollection : CusSupportingInfoCollection<AdditionalInfo>
	{
		public AdditionalInfoCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo)
		{
			MaxCountValidationEnable(99);
		}
	}
}
