using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUOrgImpAddInfo : AutoAUOrgImpAddInfo, Integration.Customs.AU.IOrgImpAddInfo
	{
		public AUOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public AUOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public ZBool IsDutyDeferred
		{
			get => base.ZO_IsDutyDeferred;
			set => base.ZO_IsDutyDeferred = value;
		}
	}
}
