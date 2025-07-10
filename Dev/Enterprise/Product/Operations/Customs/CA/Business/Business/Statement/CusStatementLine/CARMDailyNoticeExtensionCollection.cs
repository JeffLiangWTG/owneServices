using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CARMDailyNoticeExtensionCollection : Customs.Business.CusSupportingInfoCollection<CARMDailyNoticeExtension>
	{
		public CARMDailyNoticeExtensionCollection(BusinessObject parent) : base(parent, Common.CA.CusSupportingInfoTypeList.Codes.CarmDailyNoticeExtension)
		{
		}
	}
}
