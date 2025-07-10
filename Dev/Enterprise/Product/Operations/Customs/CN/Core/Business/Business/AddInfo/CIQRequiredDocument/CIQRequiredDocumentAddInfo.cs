using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CIQRequiredDocument)]
	[HumanReadableName(PropertyInfoBusinessObjectName = "Parent")]
	public class CIQRequiredDocumentAddInfo : AutoCIQRequiredDocumentAddInfo
	{
		public CIQRequiredDocumentAddInfo(ZPropertyInfo addInfoProperty) : base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
