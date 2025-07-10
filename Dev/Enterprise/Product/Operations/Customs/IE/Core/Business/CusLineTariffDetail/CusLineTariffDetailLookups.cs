using System.Collections;
using System.Linq;
using System.Reflection;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business
{
	public class CusLineTariffDetailLookups : EU.Business.CusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(EU.Business.CusLineTariffDetail parent) : base(parent)
		{
		}

		public override ICodeDescriptionPairList TariffTypeList => Factory.GetCachedValue($"{Parent.CustomsCountryCode}.CusLineTariffDetailLookups.GetAdditionalDutiesTariffTypeList", () => new AdditionalDutiesTariffTypeList(Factory, Parent.CustomsCountryCode));

		public ChildTariffViewCollection TariffList
		{
			get
			{
				return Factory.GetCachedValue($"{Parent.CustomsCountryCode}.{Parent.BZ_Type}.{Parent.EffectiveAssessmentDate}.CusLineTariffDetailLookups.TariffList", () =>
				{
					ChildTariffViewCollection childTariffViewCollection;
					if (!Parent.BZ_Type.IsEmpty)
					{
						childTariffViewCollection = ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Ireland, Parent.BZ_Type, Parent.EffectiveAssessmentDate, ZString.Empty, ZString.Empty);
					}
					else
					{
						//Get all the contant values defined in TaxOrFeeType.ExciseTaxes. This way if any new tax types are added in the future, they will be automatically included in the list.
						var tariffTypes = typeof(TaxOrFeeType.ExciseTaxes)
							.GetFields(BindingFlags.Public | BindingFlags.Static)
							.Where(f => f.FieldType == typeof(string))
							.Select(f => new ZString((string)f.GetValue(null)))
							.ToArray();

						childTariffViewCollection = new IEChildTariffViewCollection(Factory, Core.Constants.CountryCodes.Ireland, tariffTypes, Parent.EffectiveAssessmentDate);
					}
					return childTariffViewCollection;
				});
			}
		}

		public override ICollection QuantityUnitList => Customs.Universal.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Parent.EffectiveAssessmentDate);
	}
}
