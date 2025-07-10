using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.AUROLL)]
	public class RollAddInfo : AutoAURollAddInfo
	{
		public RollAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public Roll Roll
		{
			get;
			set;
		}
	}
}
