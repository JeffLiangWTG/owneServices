using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class CusLineTariffDetailLookups : EU.Business.CusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(CusLineTariffDetail parent)
		: base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		public override ICodeDescriptionPairList TariffTypeList => GetAdditionalDutiesTariffTypeList(Factory);

		public override ICollection QuantityUnitList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today, includeParentDataGrouping: false);

		public ChildTariffViewCollection TariffCollection
		{
			get
			{
				var tariffDetailParent = Parent.Parent;
				return ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Germany, Parent.BZ_Type, tariffDetailParent?.EffectiveAssessmentDate ?? ZDateTime.Today, GetApplicableTariffDetail(tariffDetailParent));
			}
		}

		KeyValuePair<ZString, ZString>[] GetApplicableTariffDetail(ICusLineTariffDetailParent tariffDetailParent)
		{
			KeyValuePair<ZString, ZString>[] validTariffDetail = null;

			var tariff = tariffDetailParent?.Tariff ?? ZString.Empty;
			if (!tariff.IsEmpty)
			{
				validTariffDetail = new[] { new KeyValuePair<ZString, ZString>(Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, tariff) };
			}

			return validTariffDetail ?? System.Array.Empty<KeyValuePair<ZString, ZString>>();
		}

		static AdditionalDutiesTariffTypeList GetAdditionalDutiesTariffTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("DE.CusLineTariffDetailLookups.GetAdditionalDutiesTariffTypeList", () => new AdditionalDutiesTariffTypeList(factory, Core.Constants.CountryCodes.Germany));
		}
	}
}
