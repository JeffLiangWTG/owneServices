using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class CountrySpecificTypeDecider : TypeDecider
	{
		#region struct CountrySpecificType

		/// <remarks>
		/// It should be a class, not a struct to keep lazy cache of type object.
		/// </remarks>
		[WTG.StaticAnalysis.Annotation.Immutable]
		public sealed class CountrySpecificType
		{
			public CountrySpecificType(ZString countryCode, Func<Type> businessObjectTypeGetter)
			{
				this.CountryCode = countryCode;
				businessObjectType = new Lazy<Type>(businessObjectTypeGetter);
			}

			public Type BusinessObjectType
			{
				get
				{
					return businessObjectType.Value;
				}
			}

			public readonly ZString CountryCode;
			readonly Lazy<Type> businessObjectType;
		}

		#endregion

		protected CountrySpecificTypeDecider()
		{
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return GetTypeForCountryCode(CurrentCountryCode);
		}

		public override Type GetTypeForBinding()
		{
			return GetTypeForCountryCode(CurrentCountryCode);
		}

		protected override Type GetTypeForNewCore(ITypeDeciderContext context)
		{
			var country = context?.Country;
			return !string.IsNullOrEmpty(country) ? GetTypeForCountryCode(country) : GetTypeForNew();
		}

		public sealed override Type GetTypeForNew()
		{
			return GetTypeForCountryCode(CurrentCountryCode);
		}

		public IEnumerable<CountrySpecificType> CountrySpecificTypes => countrySpecificTypes ?? (countrySpecificTypes = CountrySpecificTypesCore);

		protected abstract IEnumerable<CountrySpecificType> CountrySpecificTypesCore { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Used in the above CountrySpecificTypes, configured in subclasses")]
		IEnumerable<CountrySpecificType> countrySpecificTypes;

		protected virtual Type DefaultTypeForEuCountry
		{
			get { return DefaultTypeForUnsupportedCountry; }
		}

		protected abstract Type DefaultTypeForUnsupportedCountry { get; }

		public Type GetTypeForCountryCode(ZString countryCode)
		{
			if (CountrySpecificTypes.FirstOrDefault(x => x.CountryCode == countryCode) is { } countrySpecificType)
			{
				return countrySpecificType.BusinessObjectType;
			}

			var customsCountryCode = Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);

			if (customsCountryCode != countryCode && CountrySpecificTypes.FirstOrDefault(x => x.CountryCode == customsCountryCode) is { } customsCountrySpecificType)
			{
				return customsCountrySpecificType.BusinessObjectType;
			}

			if (DataGroupSepcificTypes.FirstOrDefault(x => x.Key(countryCode)).Value is { } dataGroupSpecificType)
			{
				return dataGroupSpecificType();
			}

			if (customsCountryCode != countryCode && DataGroupSepcificTypes.FirstOrDefault(x => x.Key(customsCountryCode)).Value is { } customsDataGroupSpecificType)
			{
				return customsDataGroupSpecificType();
			}

			return DefaultTypeForUnsupportedCountry;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				yield return new KeyValuePair<Func<ZString, bool>, Func<Type>>(countryCode => ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode), () => DefaultTypeForEuCountry);
			}
		}

#if DEBUG
		virtual
#endif
 protected IGlbCompany CurrentCompany
		{
			get { return StaticCurrentFetcher.Instance.CurrentCompany; }
		}

		protected ZString CurrentCountryCode => CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty;
	}
}
