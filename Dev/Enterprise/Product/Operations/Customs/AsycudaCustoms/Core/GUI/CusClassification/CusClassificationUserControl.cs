using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class CusClassificationUserControl : Customs.GUI.GeneralCountryClassificationUserControl
	{
		public BaseApplicationBusinessProvider ApplicationBusinessProvider => BaseApplicationBusinessProvider.GetApplicationBusinessProvider(GlbCompany.CurrentCompany.Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		protected override ZString UniversalTariffType => ApplicationBusinessProvider?.UniversalTariffType ?? base.UniversalTariffType;
	}
}
