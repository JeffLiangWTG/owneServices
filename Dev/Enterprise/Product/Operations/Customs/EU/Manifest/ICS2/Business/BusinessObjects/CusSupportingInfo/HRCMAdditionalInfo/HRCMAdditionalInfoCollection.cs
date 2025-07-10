using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class HRCMAdditionalInfoCollection : CusSupportingInfoCollection<HRCMAdditionalInfo>
	{
		public HRCMAdditionalInfoCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo)
		{
		}
	}
}
