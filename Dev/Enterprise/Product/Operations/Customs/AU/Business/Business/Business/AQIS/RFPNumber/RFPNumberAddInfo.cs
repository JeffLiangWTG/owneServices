using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.AURFPNumber)]
	public class RFPNumberAddInfo : AutoRFPNumberAddInfo
	{
		public RFPNumberAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
