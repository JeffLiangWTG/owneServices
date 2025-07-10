using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public abstract class AddInfo : AutoCNAddInfo
	{
		protected AddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}
	}
}
