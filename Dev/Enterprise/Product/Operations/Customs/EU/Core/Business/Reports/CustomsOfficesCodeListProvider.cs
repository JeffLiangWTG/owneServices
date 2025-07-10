using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Reports
{
	public abstract class CustomsOfficesCodeListProvider : Integration.Customs.EU.ICustomsOfficesProvider, DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var factory = new BusinessObjectFactory();
			return factory.GetCachedValue(FormattableString.Invariant($"Enterprise.Customs.{CountryCode}.Business.Reports.CustomsOfficesCodeListProvider"), () => GetCustomsOffices(factory));
		}

		protected abstract ZString CountryCode { get; }

		ReadOnlyCodeDescriptionPairList GetCustomsOffices(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			var customsOfficeCodeCollection = CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(factory, CountryCode);
			customsOfficeCodeCollection.Load();
			customsOfficeCodeCollection.Sort(ZZRefCusCodeListCombinedSchema.ZZD_Code.Name);

			foreach (ICodeDescription item in customsOfficeCodeCollection)
			{
				result.AddPair(item.Code, ZString.Format("{0} - {1}", item.Code, item.Description));
			}

			return result;
		}
	}
}
