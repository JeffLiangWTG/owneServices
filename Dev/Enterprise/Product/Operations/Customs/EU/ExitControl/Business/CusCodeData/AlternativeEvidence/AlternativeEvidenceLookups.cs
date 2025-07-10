using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class AlternativeEvidenceLookups : CusCodeDataLookups
	{
		public AlternativeEvidenceLookups(AlternativeEvidence parent) : base(parent)
		{
		}

		AlternativeEvidence parent => (AlternativeEvidence)base.Parent;

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				var countryCode = parent.ExitReport.Header?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var date = ZDateTime.Today;
				return Factory.GetCachedValue($"EU.ExitControl.AlternativeEvidenceLookups.CY_CodeList.{countryCode}|{date.ToShortDateString()}", () =>
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, countryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170, date));
						return result;
					});
			}
		}
	}
}
