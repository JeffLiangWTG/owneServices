using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	// functionality is tested in BarcodeRuleTest and BarcodeValidationRuleTest 
	public static class LoadMatchingRulesHelper
	{
		public static IEnumerable<T> LoadMatchingRules<T>(
			Func<ZDBOnlySubQuery, LoadMatchingRulesParameters, ZDBOnlyQuery> buildLoadMatchingRulesQuery,
			BusinessObjectFactory factory,
			ZString moduleCode,
			ZString buyerCode,
			ZString supplierCode,
			ZGuid relatedEntityPK,
			bool isGS1) where T : BusinessObject
		{
			var buyer = buyerCode.IsEmpty ? null : factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, buyerCode);
			var supplier = supplierCode.IsEmpty ? null : factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, supplierCode);
			var buyerPK = buyer?.PK ?? ZGuid.Empty;
			var supplierPK = supplier?.PK ?? ZGuid.Empty;

			return LoadMatchingRules<T>(buildLoadMatchingRulesQuery, factory, moduleCode, buyerPK, supplierPK, relatedEntityPK, isGS1);
		}

		public static IEnumerable<T> LoadMatchingRules<T>(
			Func<ZDBOnlySubQuery, LoadMatchingRulesParameters, ZDBOnlyQuery> buildLoadMatchingRulesQuery,
			BusinessObjectFactory factory,
			ZString moduleCode,
			ZGuid buyerPK,
			ZGuid supplierPK,
			ZGuid relatedEntityPK,
			bool isGS1) where T : BusinessObject
		{
			if (!BarcodeParsingLookupHelper.ModuleTypes(factory).ContainsCode(moduleCode))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Module code '{0}' is not supported.", moduleCode), nameof(moduleCode));
			}

			var consumer = factory.GetBarcodeParsingConsumerFromModuleCode(moduleCode);
			var isRelatedEntityRequired = buyerPK.IsValid || !consumer.IsBuyerRequiredForRelatedEntity();
			var parameters = new LoadMatchingRulesParameters(
				moduleCode,
				buyerPK,
				supplierPK,
				relatedEntityPK: isRelatedEntityRequired ? relatedEntityPK : ZGuid.Empty,
				isBuyerRequiredForRelatedEntity: consumer.IsBuyerRequiredForRelatedEntity(),
				isGS1: isGS1);

			return LoadMatchingRulesCore<T>(buildLoadMatchingRulesQuery, factory, parameters);
		}

		static IEnumerable<T> LoadMatchingRulesCore<T>(
			Func<ZDBOnlySubQuery, LoadMatchingRulesParameters, ZDBOnlyQuery> buildLoadMatchingRulesQuery,
			BusinessObjectFactory factory,
			LoadMatchingRulesParameters parameters) where T : BusinessObject
		{
			var rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters);
			if (!rules.Any())
			{
				if (parameters.BuyerPK.IsValid && parameters.SupplierPK.IsValid)
				{
					rules = FallbackWithBuyerAndSupplier<T>(buildLoadMatchingRulesQuery, factory, parameters);
				}
				else if (parameters.RelatedEntityPK.IsValid)
				{
					rules = FallbackToRelatedEntity<T>(buildLoadMatchingRulesQuery, factory, parameters);
				}

				if (!rules.Any())
				{
					rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { BuyerPK = ZGuid.Empty, SupplierPK = ZGuid.Empty, RelatedEntityPK = ZGuid.Empty });
				}
			}

			return rules;
		}

		static IEnumerable<T> FallbackWithBuyerAndSupplier<T>(
			Func<ZDBOnlySubQuery, LoadMatchingRulesParameters, ZDBOnlyQuery> buildLoadMatchingRulesQuery,
			BusinessObjectFactory factory,
			LoadMatchingRulesParameters parameters) where T : BusinessObject
		{
			bool isBuyerRequiredForRelatedEntity = parameters.IsBuyerRequiredForRelatedEntity;
			var rules = Enumerable.Empty<T>();

			if (parameters.RelatedEntityPK.IsValid)
			{
				rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { RelatedEntityPK = ZGuid.Empty });
				if (!rules.Any())
				{
					rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { SupplierPK = ZGuid.Empty });
				}

				if (!isBuyerRequiredForRelatedEntity && !rules.Any())
				{
					rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { BuyerPK = ZGuid.Empty });
				}
			}

			if (!rules.Any())
			{
				rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { SupplierPK = ZGuid.Empty, RelatedEntityPK = ZGuid.Empty });

				if (!rules.Any())
				{
					rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { BuyerPK = ZGuid.Empty, RelatedEntityPK = ZGuid.Empty });
				}

				if (!isBuyerRequiredForRelatedEntity && !rules.Any())
				{
					rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { BuyerPK = ZGuid.Empty, SupplierPK = ZGuid.Empty });
				}
			}

			return rules;
		}

		static IEnumerable<T> FallbackToRelatedEntity<T>(
			Func<ZDBOnlySubQuery, LoadMatchingRulesParameters, ZDBOnlyQuery> buildLoadMatchingRulesQuery,
			BusinessObjectFactory factory,
			LoadMatchingRulesParameters parameters) where T : BusinessObject
		{
			bool isBuyerRequiredForRelatedEntity = parameters.IsBuyerRequiredForRelatedEntity;
			var rules = Enumerable.Empty<T>();

			if (parameters.BuyerPK.IsValid)
			{
				rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { SupplierPK = ZGuid.Empty, RelatedEntityPK = ZGuid.Empty });
			}
			else if (!isBuyerRequiredForRelatedEntity)
			{
				rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { BuyerPK = ZGuid.Empty, RelatedEntityPK = ZGuid.Empty });
			}

			if (!isBuyerRequiredForRelatedEntity && !rules.Any())
			{
				rules = LoadRulesWithParameters<T>(buildLoadMatchingRulesQuery, factory, parameters with { BuyerPK = ZGuid.Empty, SupplierPK = ZGuid.Empty });
			}

			return rules;
		}

		static IEnumerable<T> LoadRulesWithParameters<T>(
			Func<ZDBOnlySubQuery, LoadMatchingRulesParameters, ZDBOnlyQuery> buildLoadMatchingRulesQuery,
			BusinessObjectFactory factory,
			LoadMatchingRulesParameters parameters) where T : BusinessObject
		{
			var subQuery = new ZDBOnlySubQuery(typeof(BarcodeRuleSet), BarcodeRuleSetSchema.PK);
			subQuery.AddToFilter(BarcodeRuleSetSchema.BRS_Module, parameters.ModuleCode);
			subQuery.AddGuidFilterOrIsNullIfGuidIsEmpty(BarcodeRuleSetSchema.BRS_OH_Buyer, parameters.BuyerPK);
			subQuery.AddGuidFilterOrIsNullIfGuidIsEmpty(BarcodeRuleSetSchema.BRS_OH_Supplier, parameters.SupplierPK);
			subQuery.AddGuidFilterOrIsNullIfGuidIsEmpty(BarcodeRuleSetSchema.BRS_RelatedEntityId, parameters.RelatedEntityPK);

			var ruleQuery = buildLoadMatchingRulesQuery(subQuery, parameters);
			return factory.Load<T>(ruleQuery);
		}
	}
}
