using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Common
{
	public class CusEntryNumLookups : AutoCusEntryNumLookups, Integration.Customs.ICusEntryNumLookups
	{
		public CusEntryNumLookups(AutoCusEntryNum parent)
			: base(parent)
		{
		}

		public new CusEntryNumber Parent
		{
			get { return (CusEntryNumber)base.Parent; }
		}

		public const string IMR = "IMR";

		public static string HIR => CustomsReferenceNumberType.eHubInterchangeReference.HIR;

		#region Additional Reference Number Types

		public virtual CodeDescriptionPairList AdditionalReferenceNumberTypes
		{
			get
			{
				if (Parent == null || Parent.IsDeleted)
				{
					return new CodeDescriptionPairList();
				}
				else
				{
					return GetAdditionalReferenceNumberTypes(Parent.Parent, Parent.Factory, Parent.CE_Category, Parent.CE_RN_NKCountryCode);
				}
			}
		}

		public static CodeDescriptionPairList GetAdditionalReferenceNumberTypes(BusinessObject parent, BusinessObjectFactory factory, ZString category, ZString countryCode)
		{
			CodeDescriptionPairList result = null;
			var additionalReferenceNumberTypeProvider = parent as IAdditionalReferenceNumberTypeProvider;
			if (additionalReferenceNumberTypeProvider != null)
			{
				result = additionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(category, countryCode);
			}
			else
			{
				var additionalRefNumberSupporter = parent as IAdditionalReferenceNumberSupporter;
				result = GetAdditionalReferenceNumberTypes(factory, countryCode, additionalRefNumberSupporter != null && additionalRefNumberSupporter.IncludeSpecialCustomsInstructionsItems);
			}
			return result;
		}

		public static CodeDescriptionPairList GetAdditionalReferenceNumberTypes(BusinessObjectFactory factory, ZString countryCode, bool includeSpecialCustomsInstructionsItems)
		{
			return GetAdditionalReferenceNumberTypes(factory, new AdditionalReferenceNumberTypesParameters(countryCode), includeSpecialCustomsInstructionsItems);
		}

		public static CodeDescriptionPairList GetAdditionalReferenceNumberTypes(BusinessObjectFactory factory, AdditionalReferenceNumberTypesParameters additionalReferenceNumberTypesParameters, bool includeSpecialCustomsInstructionsItems)
		{
			return factory.GetCachedValue(string.Format("AdditionalReferenceNumberTypes_{0}_{1}", additionalReferenceNumberTypesParameters.Key, includeSpecialCustomsInstructionsItems),
				delegate
				{
					var list = GetAdditionalReferenceNumberTypes(additionalReferenceNumberTypesParameters);

					if (includeSpecialCustomsInstructionsItems)
					{
						if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsMemberOfEU(additionalReferenceNumberTypesParameters.CountryCode))
						{
							list.AddRange(CusEntryNumberTypes.EU.EUCustomsEntryTypeList);
						}
					}
					return list;
				});
		}

		public static CodeDescriptionPairList GetAdditionalReferenceNumberTypes(ZString countryCode)
		{
			return GetAdditionalReferenceNumberTypes(new AdditionalReferenceNumberTypesParameters(countryCode));
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static CodeDescriptionPairList GetAdditionalReferenceNumberTypes(AdditionalReferenceNumberTypesParameters additionalReferenceNumberTypesParameters)
		{
			var result = new CodeDescriptionPairList();

			switch (additionalReferenceNumberTypesParameters.CountryCode)
			{
				case CountryCodes.Iceland:
					result.AddRange(new IcelandAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.UnitedStates:
					result.AddRange(new UnitedStatesAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.UnitedArabEmirates:
					result.AddRange(new UnitedArabEmiratesAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.Canada:
					result.AddRange(new CanadaAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.China:
				case CountryCodes.HongKong:
				case CountryCodes.Taiwan:
					result.AddRange(new ChinaAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.Germany:
					result.AddRange(new GermanyAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.Brazil:
					result.AddRange(new BrazilAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.Italy:
					result.AddRange(new ItalyAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.France:
					result.AddRange(new FranceAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.KoreaSouth:
					result.AddRange(new KRAdditionalReferenceNumberTypes());
					break;

				case CountryCodes.Israel:
					if (additionalReferenceNumberTypesParameters.Parent is ICommonConsol)
					{
						if (additionalReferenceNumberTypesParameters.DischargeCountryCode == CountryCodes.Israel)
						{
							result.AddRange(new IsraelConsolAdditionalReferenceNumberTypes());
						}
					}
					else if (additionalReferenceNumberTypesParameters.Parent is ICommonShipment)
					{
						if (additionalReferenceNumberTypesParameters.DischargeCountryCode == CountryCodes.Israel)
						{
							result.AddRange(new IsraelShipmentAdditionalReferenceNumberTypes());
						}
					}
					else
					{
						result.AddRange(new IsraelConsolAdditionalReferenceNumberTypes());
						result.AddRange(new IsraelShipmentAdditionalReferenceNumberTypes());
					}
					break;
			}

			result.AddPairsIfNotExist(FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.Value);

			return result;
		}

		#endregion

		#region ICusEntryNumLookups members

		ICodeDescriptionPairList Integration.Customs.ICusEntryNumLookups.AdditionalReferenceNumberTypes
		{
			get { return AdditionalReferenceNumberTypes; }
		}

		IBusinessObjectCollection Integration.Customs.ICusEntryNumLookups.Countries
		{
			get { return Countries; }
		}

		Integration.Customs.ICusEntryNumber Integration.Customs.ICusEntryNumLookups.Parent
		{
			get { return Parent; }
		}

		public ICodeDescriptionPairList CountriesAsCodeDescriptionList
		{
			get
			{
				if (Parent == null || Parent.IsDeleted)
				{
					return new CodeDescriptionPairList();
				}
				else
				{
					var list = new CodeDescriptionPairList();
					list.AddRange(Countries);

					return list;
				}
			}
		}

		#endregion
	}
}
