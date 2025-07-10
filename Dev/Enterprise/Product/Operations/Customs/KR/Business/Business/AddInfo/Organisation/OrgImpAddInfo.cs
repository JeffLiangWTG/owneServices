using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class OrgImpAddInfo : AutoKROrgImpAddInfo
	{
		public OrgImpAddInfo(ZPropertyInfoString parentPropertyInfo) : base(parentPropertyInfo.BizObj.Factory)
		{
			this.ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}
	}
}
