using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class CountryRelatedFilter : ModuleCodeFilter
	{
		public CountryRelatedFilter(ZString description, GetCodeQuery queryDelegate, BusinessObjectFactory factory,
			FieldType property2FieldType, int property2MaxLength, Func<string, BusinessObjectFactory, CodeDescriptionPairList> property2ListGetter)
			: base(description, queryDelegate, DummyGetList, DummyGetList)
		{
			this.factory = factory;
			Property2FieldType = property2FieldType;
			Property2MaxLength = property2MaxLength;
			Property2ListGetter = property2ListGetter;
			Property2Validation = ListValidation.WarnIfInvalidCode;
		}

		readonly BusinessObjectFactory factory;

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			throw new NotSupportedException(GetType().Name + " does not support 'Common'.");
		}

		protected override FilterCategory DefaultCategory => FilterCategories.TextSearch;

		protected override bool IsEmptyCore => Property2.IsEmpty;

		static GetList DummyGetList => () => null;

		public override ZString Property1
		{
			get => GetFilterCountry();
		}

		public FieldType Property2FieldType { get; }
		public int Property2MaxLength { get; }
		Func<string, BusinessObjectFactory, CodeDescriptionPairList> Property2ListGetter { get; }

		[MaxLength("Property2MaxLength")]
		[List(nameof(Property2List))]
		public override ZString Property2
		{
			get => base.Property2;
			set
			{
				if (Property2 != value)
				{
					InvalidateCachedQuery();
				}
				base.Property2 = value;
			}
		}

		public ICodeDescriptionPairList Property2List
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (Property2ListGetter != null)
				{
					var countryCode = GetFilterCountry();
					if (!countryCode.IsEmpty)
					{
						result = factory.GetCachedValue(ZString.Format("Enterprise.Customs.ASYCUDA.Module.CountryRelatedFilter.{0}|{1}", Description, countryCode), () => Property2ListGetter(countryCode, factory));
					}
				}

				return result;
			}
		}

		ZString GetFilterCountry()
		{
			var result = new ZString();

			var countryFilters = FilterBusinessObject?.ActiveModuleFilters?.OfType<ModuleNkFilter>().Where(x => x.OriginalCode == AsycudaFilterStrip.FilterConstants.Country && !x.Property.IsEmpty).ToArray();
			var countryFilterCount = countryFilters?.Length ?? 0;
			if (countryFilterCount == 0)
			{
				result = Environment.Env.CurrentCompany.Country.Code;
			}
			else if (countryFilterCount == 1)
			{
				result = countryFilters[0].Property;
			}

			return result;
		}
	}
}
