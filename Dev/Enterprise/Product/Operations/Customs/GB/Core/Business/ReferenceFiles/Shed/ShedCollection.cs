using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class ShedCollection : NonPersistentBusinessObjectCollection<Shed>
	{
		public ShedCollection(BusinessObjectFactory factory, ZString dataGroupingCode)
			: this(factory, dataGroupingCode, ZString.Empty, ZString.Empty)
		{
		}

		public ShedCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString portCode, ZString shedCode, bool findOnlyShedsWithAnAirportNameAttribute = false, string transportModeForAttributeMatch = "", bool excludeShedsWithSiteCodeAttribute = false)
			: base(factory)
		{
			Load(factory, dataGroupingCode, portCode, shedCode, findOnlyShedsWithAnAirportNameAttribute, transportModeForAttributeMatch, excludeShedsWithSiteCodeAttribute);
		}

		void Load(BusinessObjectFactory factory, ZString dataGroupingCode, ZString portCode, ZString shedCode, bool findOnlyShedsWithAnAirportNameAttribute, string transportModeForAttributeMatch = "", bool excludeShedsWithSiteCodeAttribute = false)
		{
			var filter = Shed.GetFilter(factory, dataGroupingCode);
			if (!portCode.IsEmpty)
			{
				var portFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				portFilter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.StartsWith, portCode);

				var attributeQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
				attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, UniversalReferenceConstants.ShedAttributes.Chief);
				attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, SQLComparisonOperator.StartsWith, portCode);

				portFilter.AddSubQuery(attributeQuery, JoinCondition.Or);

				filter.AddToFilter(portFilter);
			}
			if (!shedCode.IsEmpty)
			{
				filter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.EndsWith, shedCode);
			}

			if (findOnlyShedsWithAnAirportNameAttribute)
			{
				var zzdFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				var attributeQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
				attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, UniversalReferenceConstants.ShedAttributes.AirportName);
				zzdFilter.AddSubQuery(attributeQuery, JoinCondition.And);
				filter.AddToFilter(zzdFilter);
			}

			if (excludeShedsWithSiteCodeAttribute) // List of sheds for CCSUK should not show those which have the CNS courier SITEID attribute
			{
				var zzdFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				var attributeQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, true);
				attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, UniversalReferenceConstants.ShedAttributes.SITECODE);
				zzdFilter.AddSubQuery(attributeQuery, JoinCondition.And);
				filter.AddToFilter(zzdFilter);
			}

			if (!string.IsNullOrEmpty(transportModeForAttributeMatch))
			{
				var zzdFilter = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				var attributeQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList);
				attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, UniversalReferenceConstants.PortAttributes.Type);
				attributeQuery.AddToFilter(ZZRefCusCodeListAttributeCombinedSchema.ZZE_Value, transportModeForAttributeMatch);
				zzdFilter.AddSubQuery(attributeQuery, JoinCondition.And);
				filter.AddToFilter(zzdFilter);
			}

			AddRange(Factory.Load<ZZRefCusCodeListCombined>(filter).Select(x => new Shed(x)));
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Shed(Factory.New<ZZRefCusCodeListCombined>());
		}

		#endregion
	}
}
