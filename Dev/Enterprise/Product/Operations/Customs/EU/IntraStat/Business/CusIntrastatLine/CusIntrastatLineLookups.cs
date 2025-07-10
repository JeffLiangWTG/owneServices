using System.Collections;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatLineLookups : AutoCusIntrastatLineLookups
	{
		public CusIntrastatLineLookups(AutoCusIntrastatLine parent) : base(parent)
		{
		}

		new CusIntrastatLine Parent => (CusIntrastatLine)base.Parent;

		public ICollection MassInKilogramsUnits => new CodeDescriptionPairList();

		public ICodeDescriptionPairList Regions => GetRegions();

		protected virtual ICodeDescriptionPairList GetRegions() => new CodeDescriptionPairList();

		public CodeDescriptionPairList CustomsUQList => GetCachedRefCusCodeList(CustomsUQListType, CustomsUQListIncludeParentDataGrouping);

		protected virtual ZString CustomsUQListType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ;

		protected virtual bool CustomsUQListIncludeParentDataGrouping => false;

		CodeDescriptionPairList GetCachedRefCusCodeList(ZString codeType, bool includeParentDataGrouping = false) => RefCusCodeListTypes.GetCachedList(Factory, GetDefaultDataGroupingCode(), codeType, GetDateOfValuation(), includeParentDataGrouping: includeParentDataGrouping);

		protected ZDateTime GetDateOfValuation() => !Parent.Header.CIH_TransactionDate.IsEmpty ? Parent.Header.CIH_TransactionDate : ZDateTime.Today;
		protected ZString GetDefaultDataGroupingCode() => !Parent.Header.CountryCode.IsEmpty ? Parent.Header.CountryCode : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
