using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.AUCLR)]
	public class TravelDocAddInfo : AutoAUTravelDocAddInfo
	{
		public TravelDocAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public TravelDocument TravelDocument
		{
			get;
			set;
		}
	}
}
