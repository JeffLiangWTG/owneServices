using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class CACusRulingLookupsExtension
	{
		public static CodeDescriptionPairList GetRulingTypeList(BusinessObjectFactory factory, CodeDescriptionPairList originalTypeList)
		{
			return factory.GetCachedValue("CACusRulingLookups|RulingTypeList", delegate()
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(originalTypeList);
				result.AddPairIfNotExist(CalculationMethods.Codes.WarrantyRepairsRemission, CalculationMethods.Descriptions.WarrantyRepairsRemission);
				result.AddPairIfNotExist(CalculationMethods.Codes.DutyDeferral, CalculationMethods.Descriptions.DutyDeferral);
				result.AddPairIfNotExist(CalculationMethods.Codes.GiftsUpTo60, CalculationMethods.Descriptions.GiftsUpTo60);
				result.AddPairIfNotExist(CalculationMethods.Codes.SoftwareRemission, CalculationMethods.Descriptions.SoftwareRemission);
				result.AddPairIfNotExist(CalculationMethods.Codes.SpiritsRemission, CalculationMethods.Descriptions.SpiritsRemission);
				result.Sort();
				return result;
			});
		}
	}
}
