using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class DEOrgImpAddInfo : AutoDEOrgImpAddInfo, Integration.Customs.DE.IOrgImpAddInfo
	{
		public DEOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public DEOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		[List(nameof(Lookups) + "." + nameof(DEOrgImpAddInfoLookups.VATClaimBackList))]
		public override ZString ZO_VATClaimBack
		{
			get => base.ZO_VATClaimBack;
			set => base.ZO_VATClaimBack = value;
		}

		public static DEOrgImpAddInfo Get(OrgHeader organisation) => (DEOrgImpAddInfo)organisation?.GetCountryData(Core.Constants.CountryCodes.Germany).ImpAddInfo;
	}
}
