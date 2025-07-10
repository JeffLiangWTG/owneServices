using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CNVINData)]
	[HumanReadableName(PropertyInfoBusinessObjectName = "Parent")]
	public class CNVINDataAddInfo : AutoCNVINDataAddInfo
	{
		public CNVINDataAddInfo(ZPropertyInfo addInfoProperty) : base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
