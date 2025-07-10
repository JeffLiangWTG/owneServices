using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Module
{
	public class EntryHeaderFilterBusinessObjectLookups : EntryHeaderFilterLookups
	{
		public EntryHeaderFilterBusinessObjectLookups(EntryHeaderFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, ZDateTime.Today);

		public CodeDescriptionPairList CustomsProcedureList => CNRefCusProcedure.GetRefCusProcedureList(Factory);

		public CodeDescriptionPairList CIQStatusList => GetEntryStatusList(Factory, RefCusCodeListAttributeTypes.Codes.IUpdateCIQStatus);

		public CodeDescriptionPairList GetEntryStatusList() => GetEntryStatusList(Factory, RefCusCodeListAttributeTypes.Codes.IUpdateCustomsStatus);

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<JobMessageStatusList>();

		CodeDescriptionPairList GetEntryStatusList(BusinessObjectFactory factory, string attributeName)
		{
			var countryCode = GlbCompany.CurrentCompany.Country.Code;
			var cacheKey = string.Format(CultureInfo.InvariantCulture, "{0}_RefCusCodeList_CustomsStatus_{1}", countryCode, attributeName);
			return factory.GetCachedValue(cacheKey, () =>
			{
				var result = new CodeDescriptionPairList();
				var attributeFilter = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(attributeName, JoinCondition.And) };
				result.AddRange(ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today, attributeFilter));
				result.Sort();
				return result;
			});
		}
	}
}
