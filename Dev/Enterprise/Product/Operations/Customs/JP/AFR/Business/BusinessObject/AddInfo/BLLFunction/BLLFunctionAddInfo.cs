using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.JP.AFR.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction)]
	public class BLLFunctionAddInfo : AutoBLLFunctionAddInfo
	{
		public BLLFunctionAddInfo(ZPropertyInfo addInfoProperty) : base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
