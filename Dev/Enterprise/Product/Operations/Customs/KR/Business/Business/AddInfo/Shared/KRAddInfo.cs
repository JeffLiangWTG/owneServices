using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public abstract class KRAddInfo : AutoKRAddInfo
	{
		protected KRAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
