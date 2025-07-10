using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public class IEChildTariffViewCollection : ChildTariffViewCollection
	{
		public IEChildTariffViewCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] tariffTypes, ZDateTime effectiveValuationDate) : base(factory, dataGroupingCode, string.Empty, effectiveValuationDate, null)
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.TariffCode, "Property", ZString.Empty, true));
			if (effectiveValuationDate.IsValid)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.EffectiveDate, "Property1", effectiveValuationDate));
			}

			RelationshipFilter.AddToFilter(GetAdditionalFilter(factory, dataGroupingCode, tariffTypes));
		}

		static ZQuery GetAdditionalFilter(BusinessObjectFactory factory, ZString dataGroupingCode, ZString[] tariffTypes)
		{
			var result = new ZDBOnlyQuery(typeof(TariffView));
			if (tariffTypes?.Length > 0)
			{
				result.AddToFilter(
					new ZQuery().AddToFilter(JoinCondition.Or, TariffViewSchema.ZZ1_ZZI_NKTariffType, SQLComparisonOperator.StartsWith, tariffTypes),
					JoinCondition.And
				);
			}
			return result;
		}
	}
}
