using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrgOpportunityFilterBusinessObject : OrgOpportunityFilterBusinessObject
	{
		#region Value Types

		public override CodeDescriptionPairList ValueTypes
		{
			get { return valueTypes ?? (valueTypes = new CodeDescriptionPairList(new EDIOrgOpportunityValueLookups(null).ValueTypes)); }
		}

		CodeDescriptionPairList valueTypes;

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();
			var currentFilter = filters.AddNumberRangeFilter("Global Reach", GetGlobalReachQuery);
			currentFilter.MultilingualDescription = ResString.GetMultilingualString("b500ec71-0a4d-4114-860b-cd8de2ed619f", "Global Reach");
			currentFilter.Decimals = 0;
			currentFilter.MinValue = 0;
			var lifetimeFilter = filters.AddNumberRangeFilter("Lifetime (3 CLV)", GetLifetimeQuery);
			lifetimeFilter.MultilingualDescription = ResString.GetMultilingualString("d0976ef4-ce8e-4875-b2b2-1a5edc379f5a", "Lifetime (3 CLV)");
			lifetimeFilter.MinValue = 0;
			var currencyFilter = filters.AddNkFilter("Contract Currency", OrgOpportunitySchema.P8_RX_NKEstimatedValueCurrency, ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory));
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("140c8445-2214-41b5-a8d4-be35a22226bd", "Contract Currency");
			currencyFilter.Category = FilterCategories.Other;
			SetCurrentAndPotentialCaptionsToEDISpecificDisplayNames(filters);
			return filters;
		}

		ZQuery GetGlobalReachQuery(INumericZType value1, INumericZType value2)
		{
			ZInt intValue1;
			ZInt intValue2;

			try
			{
				intValue1 = value1.ToZInt();
			}
			catch (OverflowException)
			{
				intValue1 = Int32.MinValue;
			}

			try
			{
				intValue2 = value2.ToZInt();
			}
			catch (OverflowException)
			{
				intValue2 = Int32.MaxValue;
			}

			return GetEdiOrgOpportunityExQuery(intValue1 == 0, intValue1, intValue2, EdiOrgOpportunityExSchema.EOM_GlobalPotential);
		}

		ZQuery GetLifetimeQuery(INumericZType value1, INumericZType value2)
		{
			ZDecimal decimalValue1;
			ZDecimal decimalValue2;

			try
			{
				decimalValue1 = (ZDecimal)value1;
			}
			catch (OverflowException)
			{
				decimalValue1 = decimal.MinValue;
			}

			try
			{
				decimalValue2 = (ZDecimal)value2;
			}
			catch (OverflowException)
			{
				decimalValue2 = decimal.MaxValue;
			}

			return GetEdiOrgOpportunityExQuery(decimalValue1 == 0M, decimalValue1, decimalValue2, EdiOrgOpportunityExSchema.EOM_LifetimeValueOver3Years);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Culture invariant string in sql")]
		ZDBOnlyQuery GetEdiOrgOpportunityExQuery(bool includeNonEDI, INumericZType value1, INumericZType value2, SchemaColumn column)
		{
			var query = new ZDBOnlyQuery(typeof(OrgOpportunity));

			var filter = ModuleNumberRangeFilter.AddToFilters(new ZQuery(), column, value1, value2);
			var exSubQuery = new ZDBOnlySubQuery(typeof(EdiOrgOpportunityEx), EdiOrgOpportunityExSchema.EOM_P8);
			exSubQuery.AddToFilter(filter, JoinCondition.And);

			query.AddSubQuery(exSubQuery, JoinCondition.And);

			if (includeNonEDI)
			{
				var queryInner = new ZDBOnlySubQuery(typeof(EdiOrgOpportunityEx), EdiOrgOpportunityExSchema.EOM_P8, true);

				query.AddSubQuery(queryInner, JoinCondition.Or);
			}

			return query;
		}

		void SetCurrentAndPotentialCaptionsToEDISpecificDisplayNames(ModuleFilterCollection filters)
		{
			foreach (var filter in filters)
			{
				if (filter.Description == "Current")
				{
					filter.MultilingualDescription = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
				}
				else if (filter.Description == "Potential")
				{
					filter.MultilingualDescription = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
				}
				else if (filter.Description == "Total Estimated Value (p.a)")
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("467dc53d-73ad-4948-a674-1a78f629300a", "Contract (p.a)");
				}
			}
		}

		#endregion
	}
}
