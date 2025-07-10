using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportAddInfoJobComInvoiceLineLookups : AddInfoJobComInvoiceLineLookups
	{
		public ExportAddInfoJobComInvoiceLineLookups(EU.Business.Declaration.AddInfoJobComInvoiceLine parent) : base(parent)
		{
		}

		protected override ICollection GetCountriesOfDestinationCore() => GetNewCountriesList();

		public ICodeDescriptionPairList CountryOfDestinationCL063 => Factory.GetCachedValue(
			CountryOfDestinationKey + UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL063,
			() => GetCountryOfDestinationList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL063, Enumerable.Empty<ZString>(), CountryOfDestination.B1871.ExcludeFromCL063)
		);

		public ICodeDescriptionPairList CountryOfDestinationCL140 => Factory.GetCachedValue(
			CountryOfDestinationKey + UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL140,
			() => GetCountryOfDestinationList(UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL140, CountryOfDestination.B1872.ExtraToCL140, CountryOfDestination.B1872.ExcludeFromCL140)
		);

		ICodeDescriptionPairList GetCountryOfDestinationList(string codeType, IEnumerable<ZString> extraToRefList, IEnumerable<ZString> excludeFromRefList)
		{
			var result = new CodeDescriptionPairList();

			var refList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ireland, codeType, ZDateTime.Today);
			refList.Load();
			var refCodeDescription = refList.Cast<ICodeDescription>();

			if (extraToRefList.Any())
			{
				var extraCountryList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
						factory: Factory,
						additionalFilter: new ZQuery(RefCusCodeListSchema.ZZD_Code, extraToRefList.ToArray()),
						dataGroupingCode: Core.Constants.CountryCodes.Ireland,
						codeTypes: new[] { (ZString)UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country },
						date: ZDateTime.Today,
						attributeFilters: null,
						includeParentDataGroupings: false
				);
				extraCountryList.Load();
				var extraCountryCodeDescription = extraCountryList.Cast<ICodeDescription>();
				refCodeDescription = refCodeDescription.Union(extraCountryCodeDescription);
			}

			var excludeDict = new HashSet<ZString>(excludeFromRefList);
			var pairList = refCodeDescription.Where(item => !excludeDict.Contains(item.Code));
			pairList.ForEach(cusCode => result.Add(cusCode));

			return result;
		}

		const string CountryOfDestinationKey = "IE.InvoiceLine.CountryOfDestinationList|";
	}
}
