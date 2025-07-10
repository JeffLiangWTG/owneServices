using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollection : ZZRefCusCodeListCombinedCollection
	{
		public ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> attributeFilters, IEnumerable<RefCusCodeListAttributeFilter> parentDataGroupFilters)
			: base(factory, dataGroupingCode, new[] { codeType }, date, attributeFilters, true)
		{
			InitializeLazy(date, parentDataGroupFilters);
		}

		public override void Load(ZQuery filter)
		{
			ParentRefCusCodeListCombinedCollection.Load(filter);

			UpdateFilterToIgnoreParentLevelData(filter);
			base.Load(filter);

			AddCodesFromParentRefCusCodeListCombined();
		}

		#region Implementation

		void UpdateFilterToIgnoreParentLevelData(ZQuery filter)
		{
			var dataGroupingCode = DataGroupingCodes.FirstOrDefault();
			var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(Factory, dataGroupingCode);
			if (!parentDataGroupingCode.IsEmpty)
			{
				filter.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, SQLComparisonOperator.NotEqual, parentDataGroupingCode);
			}
		}

		void InitializeLazy(ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> parentDataGroupFilters)
		{
			lazyParentRefCusCodeListCombinedCollection = new Lazy<ZZRefCusCodeListCombinedCollection>(() => CreateParentRefCusCodeListCombinedCollection(date, parentDataGroupFilters));
		}

		ZZRefCusCodeListCombinedCollection CreateParentRefCusCodeListCombinedCollection(ZDateTime date, IEnumerable<RefCusCodeListAttributeFilter> parentDataGroupFilters)
			=> new ZZRefCusCodeListCombinedCollection(Factory, DataGroupingCodes.ToArray(), CodeTypes.ToArray(), date, parentDataGroupFilters, includeParentDataGroupings: true);

		void AddCodesFromParentRefCusCodeListCombined()
		{
			var codes = Select(c => c.ZZD_Code).ToHashSet();
			foreach (var parentCusCode in ParentRefCusCodeListCombinedCollection.Cast<ZZRefCusCodeListCombined>())
			{
				if (codes.Contains(parentCusCode.ZZD_Code))
				{
					continue;
				}

				Add(parentCusCode);
			}
		}

		#endregion

		ZZRefCusCodeListCombinedCollection ParentRefCusCodeListCombinedCollection => lazyParentRefCusCodeListCombinedCollection.Value;
		Lazy<ZZRefCusCodeListCombinedCollection> lazyParentRefCusCodeListCombinedCollection;
	}
}
