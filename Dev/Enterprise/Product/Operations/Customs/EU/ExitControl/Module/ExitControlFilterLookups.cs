using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public class ExitControlFilterLookups : CommonFilterLookups
	{
		public ExitControlFilterLookups(ExitControlFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		protected new ExitControlFilterBusinessObject FilterBizObj => (ExitControlFilterBusinessObject)base.FilterBizObj;

		public CodeDescriptionPairList StatusCodesList => StatusCodesListCore;

		protected virtual CodeDescriptionPairList StatusCodesListCore
		{
			get
			{
				return Factory.GetCachedValue("StatusCodesList_EU", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDate.Today)
						.OrderBy(x => x.ZZD_Code).ToArray());
					return result;
				});
			}
		}

		public CodeDescriptionPairList TransportModeList => Factory.GetCachedValue<TransportTypeList>();
	}
}
