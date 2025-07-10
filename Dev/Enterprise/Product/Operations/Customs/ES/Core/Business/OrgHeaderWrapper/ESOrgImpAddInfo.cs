using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class ESOrgImpAddInfo : AutoESOrgImpAddInfo, Integration.Customs.ES.IOrgImpAddInfo
	{
		public ESOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public ESOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}

		public static ESOrgImpAddInfo Get(OrgHeader organisation) => (ESOrgImpAddInfo)organisation?.GetCountryData(Core.Constants.CountryCodes.Spain).ImpAddInfo;

		[List(nameof(Lookups) + "." + nameof(ESOrgImpAddInfoLookups.MethodOfPaymentList))]
		public override ZString ZO_MethodOfPayment { get => base.ZO_MethodOfPayment; set => base.ZO_MethodOfPayment = value; }

		[List(nameof(Lookups) + "." + nameof(ESOrgImpAddInfoLookups.MethodOfPaymentList))]
		public override ZString ZO_MethodOfPaymentCan { get => base.ZO_MethodOfPaymentCan; set => base.ZO_MethodOfPaymentCan = value; }
	}
}
