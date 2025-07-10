using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CAComponent)]
	[HumanReadableName(PropertyInfoBusinessObjectName = "Parent")]
	public class ComponentAddInfo : AutoComponentAddInfo
	{
		public ComponentAddInfo(ZPropertyInfo addInfoProperty) : base(addInfoProperty.BizObj.Factory)
		{
			Parent = addInfoProperty.BizObj;
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
