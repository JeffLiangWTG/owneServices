using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.JP.AFR.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.JPAFRNewVesselVoyage)]
	public class VesselVoyageAddInfo : AutoVesselVoyageAddInfo
	{
		public VesselVoyageAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
