using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.EntityRepositories;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching
{
	class ProductEntityMatchingInterceptor : MIDOrgMatchingInterceptor
	{
		public ProductEntityMatchingInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			productEntityMatchingOrgCache = new ProductEntityMatchingOrgCache(factory);
		}

		public ProductEntityMatchingSetting Setting
		{
			get { return (ProductEntityMatchingSetting)InterceptorSetting; }
		}

		public override void Invoke(IEntitySet entitySet)
		{
			var product = entitySet.Root;
			product.DisableNativeEnginesOwnNaturalKeyMatch = true;
			try
			{
				entityToDataRowMapping = new Dictionary<string, DataRow>();
				productEntityMatchingOrgCache.ClearCache();
				pivotAttributesDictionary = new Dictionary<string, (string[] attribute1, string[] attribute2, string[] attribute3)>();
				entityRepository = new EntityRepository(this.Setting.Context, sessionServices);
				entityRepository.OpenSession();
				MatchParts(product);
			}
			finally
			{
				entityRepository = null;
			}
			Function(entitySet);
		}

		Dictionary<string, DataRow> entityToDataRowMapping;
		EntityRepository entityRepository;
		readonly ProductEntityMatchingOrgCache productEntityMatchingOrgCache;

		protected static bool IsEntityEmpty(IEntity entity)
		{
			var result = entity == null;
			if (!result)
			{
				result = entity.InternalPK == Guid.Empty && entity.PropertyCount == 0 && !entity.Parents.Any();
			}
			return result;
		}

		DataRow FindRow(IEntity entity)
		{
			var key = $"{entity.EntityName}-{GetInstanceKey(entity)}";
			if (!entityToDataRowMapping.TryGetValue(key, out var result))
			{
				result = IsEntityEmpty(entity) ? null : entityRepository.FindRow(entity, throwExceptionOnNotFound: false);
				if (result != null && entity.InternalPK == Guid.Empty)
				{
					var table = result.Table;
					var primaryKeys = table.PrimaryKey;
					if (primaryKeys.Length == 1)
					{
						var pkName = primaryKeys[0];
						if (pkName.DataType == typeof(Guid))
						{
							var index = table.Columns.IndexOf(pkName);
							entity.InternalPK = (Guid)result.ItemArray[index];
						}
					}
				}
				entityToDataRowMapping.Add(key, result);
			}
			return result;
		}

		class RelatedPartyWithType : RelatedParty
		{
			public readonly string RelatedOrganisationCode;

			public RelatedPartyWithType(string relationshipCode, string relatedOrganisationCode, ZGuid relatedOrganisationPK) : base(relationshipCode, relatedOrganisationPK)
			{
				RelatedOrganisationCode = relatedOrganisationCode;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void MatchParts(IEntity xmlProduct)
		{
			CombineDupliatedRelationships(xmlProduct);

			var productNumberFromXml = xmlProduct.GetPropertyOrBlankString("PartNum");
			if (string.IsNullOrEmpty(productNumberFromXml))
			{
				Fail("Illegal XML: the OrgPartRelation requires a part number");
			}

			var productDefinition = xmlProduct.Definition;
			var parentPivotDefinition = productDefinition.Children.First(x => x.EntityName == XmlConstants.EntityNames.CusClassPartPivot);
			var parentPivotOrgHeaderDefinition = parentPivotDefinition.Parents.First(p => p.EntityName == XmlConstants.EntityNames.OrgHeader);
			var parentPivotColumnsToCompareHonouringExclusions = parentPivotDefinition.PropertyDefinitions.Where(d => !parentPivotDefinition.UniqueCriteriaExclusionsContains(d.ColumnDef.Name)).ToArray();
			var parentPivotCusCAClassificationDefinition = parentPivotDefinition.Children.First(p => p.EntityName == XmlConstants.EntityNames.CusCAClassification);
			var parentPivotCusCNClassificationDefinition = parentPivotDefinition.Children.First(p => p.EntityName == XmlConstants.EntityNames.CusCNClassification);
			var parentPivotCusUSClassificationDefinition = parentPivotDefinition.Children.First(p => p.EntityName == XmlConstants.EntityNames.CusUSClassification);
			var componentCusClassPartPivotDefinition = parentPivotDefinition.Children.First(p => p.EntityName == XmlConstants.EntityNames.ComponentCusClassPartPivot);
			var componentCusClassPartPivotDefinitionCusUSClassificationDefinition = componentCusClassPartPivotDefinition.Children.First(p => p.EntityName == XmlConstants.EntityNames.CusUSClassification);
			var parentPivotColumnsWithoutPKToCompareHonouringExclusions = parentPivotColumnsToCompareHonouringExclusions.Where(d => d.ColumnDef.Name != CusClassPartPivotSchema.Constants.PK).ToArray();
			var xmlPivots = xmlProduct.Children.Where(e => e.EntityName == XmlConstants.EntityNames.CusClassPartPivot).ToArray();
			var listOfFailures = new List<string>();
			var htiPivots = new List<IEntity>();
			var mergePivots = new List<IEntity>();
			foreach (var xmlPivot in xmlPivots)
			{
				var parentPivotCountryCode = string.Empty;
				var parentPivotCountry = xmlPivot.GetParentEntity(XmlConstants.EntityNames.Country);
				if (parentPivotCountry != null)
				{
					parentPivotCountryCode = parentPivotCountry.GetPropertyOrBlankString(XmlConstants.PropertyNames.Code);
					var parentPivotCountryRow = FindRow(parentPivotCountry);
					if (parentPivotCountryRow != null)
					{
						var parentPivotCountryRowCode = (string)parentPivotCountryRow[RefCountrySchema.Constants.RN_Code];
						if (parentPivotCountryRowCode != parentPivotCountryCode)
						{
							parentPivotCountryCode = parentPivotCountryRowCode;
							parentPivotCountry[XmlConstants.PropertyNames.Code] = parentPivotCountryRowCode;
						}
					}
				}

				if (string.IsNullOrEmpty(parentPivotCountryCode))
				{
					Fail("All CusClassPartPivot rows must have a country specified");
				}
				else
				{
					var parentCusClassification = xmlPivot.GetParentEntity(XmlConstants.EntityNames.CusClassification);
					if (parentCusClassification != null)
					{
						var parentCusClassificationLookUpCode = parentCusClassification.GetPropertyOrBlankString(XmlConstants.PropertyNames.LookupCode);
						if (!string.IsNullOrEmpty(parentCusClassificationLookUpCode))
						{
							var parentCusClassificationCountry = parentCusClassification.GetParentEntity(XmlConstants.EntityNames.CountryCodeExternal);
							if (parentCusClassificationCountry != null)
							{
								var parentCusClassificationCountryCode = parentCusClassificationCountry.GetPropertyOrBlankString(XmlConstants.PropertyNames.Code);
								if (parentPivotCountryCode != parentCusClassificationCountryCode)
								{
									Fail("CusClassPartPivot and its CusClassification's countries mismatch.");
								}
							}
							else
							{
								Fail("CusClassPartPivot's CusClassification rows must have a country specified.");
							}
						}
					}
				}

				if (MatchingHelper.IsChildTypeHTI(xmlPivot))
				{
					htiPivots.Add(xmlPivot);
				}

				if (xmlPivot.Action == EntityAction.MERGE)
				{
					mergePivots.Add(xmlPivot);
				}
				var parentPivotOrgHeaders = xmlPivot.Parents.Where(e => e.EntityName == XmlConstants.EntityNames.OrgHeader).Take(2).ToArray();
				FailIfAnyPivotsContainMultipleOrgHeaders(parentPivotOrgHeaders);
				switch (parentPivotCountryCode)
				{
					case Core.Constants.CountryCodes.Canada:
						InsertEmptyCusCountrySpecificClassificationUnderPivotIfMissing(xmlPivot, parentPivotCusCAClassificationDefinition);
						break;
					case Core.Constants.CountryCodes.China:
						InsertEmptyCusCountrySpecificClassificationUnderPivotIfMissing(xmlPivot, parentPivotCusCNClassificationDefinition);
						break;
					case Core.Constants.CountryCodes.UnitedStates:
						InsertEmptyCusCountrySpecificClassificationUnderPivotIfMissing(xmlPivot, parentPivotCusUSClassificationDefinition);
						break;
				}

				var componentPivots = xmlPivot.Children.Where(e => e.EntityName == XmlConstants.EntityNames.ComponentCusClassPartPivot).ToArray();
				var parentTariff = xmlPivot.GetPropertyOrBlankString(XmlConstants.PropertyNames.TariffNum);
				InsertEmptyFieldForParentPivotPropertiesNotGivenAndOhToo(xmlPivot, parentPivotOrgHeaderDefinition, parentPivotColumnsWithoutPKToCompareHonouringExclusions);
				var sequencePairs = new List<string>();
				var isUSPivot = parentPivotCountryCode == Core.Constants.CountryCodes.UnitedStates;
				foreach (var componentPivot in componentPivots)
				{
					FailIfDuplicateChildSequencesInComponentTariff(componentPivot, parentTariff, listOfFailures, sequencePairs);
					AddCountryIfMissingOnComponentPivot(componentPivot, parentPivotCountryCode);
					if (isUSPivot)
					{
						InsertEmptyCusCountrySpecificClassificationUnderPivotIfMissing(componentPivot, componentCusClassPartPivotDefinitionCusUSClassificationDefinition);
					}
				}
			}
			if (listOfFailures.Any())
			{
				Fail("Child pivots must have different sequences. Failures:\r\n" + string.Join("; ", listOfFailures.ToArray()));
			}
			FailIfHITPivotsAttributesInvalid(htiPivots.ToArray());
			FailIfDuplicateParentPivots(mergePivots.ToArray(), parentPivotColumnsToCompareHonouringExclusions);

			var pkOfSupposedExistingRowInXml = (ZGuid)xmlProduct.InternalPK;
			sessionServices.Logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Importing Product: {0}", productNumberFromXml));

			var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			partQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productNumberFromXml);
			var partQueryWithRelOrgInsert = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			partQueryWithRelOrgInsert.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productNumberFromXml);

			var xmlOrganisationalRelationships = FindOrganisationalRelationshipsDirectlyBelowOrgSupplierPart(xmlProduct).ToArray();
			var listXmlOrgTypesByCode = new List<RelatedPartyWithType>();
			var listXmlOrgTypesByCodeExceptDeletes = new List<RelatedPartyWithType>();
			var listXmlOrgTypesByCodeInserts = new List<RelatedPartyWithType>();
			var owners = new List<ZGuid>();

			OrgSupplierPart partLoadedFromEnterprise = null;
			if (xmlOrganisationalRelationships.Length > 0)
			{
				CheckXmlHasBareMinimumWeNeedRegardingOrgsAndRels(xmlOrganisationalRelationships);
				var numberOfRelationshipsInXml = 0;
				var numberOfRelationshipsInXmlWithoutInsert = 0;
				foreach (var xmlRelation in xmlOrganisationalRelationships)
				{
					var relationshipType = xmlRelation.GetPropertyOrBlankString("Relationship");
					if (string.IsNullOrEmpty(relationshipType))
					{
						Fail("Illegal XML: the OrgPartRelation requires a relationship type");
					}

					(var organisation, var org) = productEntityMatchingOrgCache.GetOrganisationFromEntityParent(xmlRelation);
					if (organisation != null)
					{
						if (relationshipType == OrgPartRelation.RelationshipTypes.Both
							|| relationshipType == OrgPartRelation.RelationshipTypes.Owner
							|| relationshipType == OrgPartRelation.RelationshipTypes.Supplier
							|| relationshipType == OrgPartRelation.RelationshipTypes.ClassificationOrganization) // Else the relationship is not consequential
						{
							numberOfRelationshipsInXml++;
							var relatedOrganisationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
							if (org != null)
							{
								if (!owners.Contains(org.PK) && xmlRelation.Action != EntityAction.DELETE)
								{
									var isOrgBecomingOwner = relationshipType == OrgPartRelation.RelationshipTypes.Both || relationshipType == OrgPartRelation.RelationshipTypes.Owner;
									if (isOrgBecomingOwner || IsOwnerInDB(org.PK.ToGuid(), pkOfSupposedExistingRowInXml))
									{
										owners.Add(org.PK);
									}
								}

								relatedOrganisationSubQuery.AddToFilter(GetAdditionalRelationshipQuery(organisation.InternalPK, new[] { relationshipType }));
								partQueryWithRelOrgInsert.AddSubQuery(relatedOrganisationSubQuery, JoinCondition.And);

								if (xmlRelation.Action != EntityAction.INSERT)
								{
									numberOfRelationshipsInXmlWithoutInsert++;
									partQuery.AddSubQuery(relatedOrganisationSubQuery, JoinCondition.And);
								}

								if (!listXmlOrgTypesByCode.Any(x => x.RelatedOrganisationCode == org.OH_Code && x.RelationshipCode == relationshipType))
								{
									var relationship = new RelatedPartyWithType(relationshipType, org.OH_Code, org.PK);
									listXmlOrgTypesByCode.Add(relationship);

									if (xmlRelation.Action != EntityAction.DELETE)
									{
										listXmlOrgTypesByCodeExceptDeletes.Add(relationship);
									}

									if (xmlRelation.Action == EntityAction.INSERT)
									{
										listXmlOrgTypesByCodeInserts.Add(relationship);
									}
								}
								else
								{
									Fail(string.Format("Invalid XML for product {0}. The combination of relationship type and organisation code {1} is already specified", productNumberFromXml, org.OH_Code));
								}
							}
							else
							{
								Fail(string.Format("Invalid XML for product {0}. The organisation specified is not valid under this context. Code: {1}.", productNumberFromXml, organisation.GetPropertyOrBlankString(XmlConstants.PropertyNames.Code)));
							}
						}
					}
					else
					{
						Fail("Every OrgPartRelation must have an OrgHeader.");  // It is not possible to save (and thence export to XML) a product that has an OU without an OH, but the user could create XML that has a missing OH.
					}
				}

				if (!pkOfSupposedExistingRowInXml.IsEmpty)
				{
					// try PK
					partLoadedFromEnterprise = factory.Load<OrgSupplierPart>(pkOfSupposedExistingRowInXml);
				}

				if (partLoadedFromEnterprise == null)
				{
					var relOrgCount = numberOfRelationshipsInXml;
					var loadedParts = factory.Load<OrgSupplierPart>(partQueryWithRelOrgInsert);
					if (numberOfRelationshipsInXmlWithoutInsert > 0 && loadedParts.Length == 0 && numberOfRelationshipsInXml != numberOfRelationshipsInXmlWithoutInsert)
					{
						relOrgCount = numberOfRelationshipsInXmlWithoutInsert;
						loadedParts = factory.Load<OrgSupplierPart>(partQuery);
					}
					// try NK
					partLoadedFromEnterprise = (from OrgSupplierPart databasePart in loadedParts where databasePart.RelatedOrganisations.Count == relOrgCount select databasePart).OrderByDescending(p => p.OP_IsActive).FirstOrDefault();
				}

				ProcessAfterSeekingExistingRow(xmlProduct, productNumberFromXml, xmlPivots, parentPivotDefinition, listXmlOrgTypesByCodeExceptDeletes, listXmlOrgTypesByCodeInserts, partLoadedFromEnterprise);

				if (partLoadedFromEnterprise != null)
				{
					FailIfAnyRelationshipsAreNowInvalid(partLoadedFromEnterprise, xmlOrganisationalRelationships);
				}
			}
			else
			{
				// No org releations exist
				if (pkOfSupposedExistingRowInXml.IsEmpty)
				{
					// No relations and no PK --> puke
					Fail("Illegal XML: No relationships between Product and Organisation are defined in the XML, please check to ensure at least one exists.");
				}
				else
				{
					// Have PK
					partLoadedFromEnterprise = factory.Load<OrgSupplierPart>(pkOfSupposedExistingRowInXml);
					if (partLoadedFromEnterprise != null)
					{
						ProcessAfterSeekingExistingRow(xmlProduct, productNumberFromXml, xmlPivots, parentPivotDefinition, listXmlOrgTypesByCodeExceptDeletes, listXmlOrgTypesByCodeInserts, partLoadedFromEnterprise, dontTouchExistingOrgsBecauseNoneWereInXml: true);
					}
					else
					{
						Fail("Could not find match using primary key and insufficient data present to attempt a natural key match (no OrgPartRelations exist)");
					}
				}
			}

			FailIfInvalidOrgPartRelationAttributeConfig(partLoadedFromEnterprise, xmlOrganisationalRelationships);
			FailIfDuplicateBarcode(productNumberFromXml, owners, xmlProduct);
			FailIfInvalidPartBarcode(xmlProduct);
			FailIfInvalidConsigneeMinShelfLifeAcceptedInPartRelation(partLoadedFromEnterprise, xmlProduct, xmlOrganisationalRelationships);
			FailIfInvalidUnitConversionPackTypes(xmlProduct);

			var componentMatchingHelper = new ProductComponentMatchingHelper(factory, xmlProduct, productEntityMatchingOrgCache, partLoadedFromEnterprise);
			componentMatchingHelper.FailIfInvalidOrgPartOrSecondaryPartBOMs();
			componentMatchingHelper.FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder();

			var productParamsHelper = new ProductParamsByWhsAndClientHelper(factory, xmlProduct);
			productParamsHelper.FailIfInvalidProductParams(partLoadedFromEnterprise);

			var productStyleHelper = new ProductEntityMatchingProductStyleHelper(xmlProduct);
			productStyleHelper.FailIfInvalidProductColourAndSizeStyles(partLoadedFromEnterprise);
			productStyleHelper.AddSequenceNumberToProductStyleSizeIfNotSpecified(factory);
		}

		bool IsOwnerInDB(Guid orgPK, ZGuid productPK)
		{
			var query = new ZDBOnlyQuery(typeof(OrgPartRelation));
			query.AddToFilter(OrgPartRelationSchema.OU_OH, orgPK);
			query.AddToFilter(OrgPartRelationSchema.OU_Relationship, new[] { OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner });
			query.AddToFilter(OrgPartRelationSchema.OU_OP, productPK);

			return factory.ExistsInDatabase(OrgPartRelationSchema.Constants.TableName, query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void CombineDupliatedRelationships(IEntity xmlProduct)
		{
			var xmlOrganisationalRelationships = FindOrganisationalRelationshipsDirectlyBelowOrgSupplierPart(xmlProduct).ToArray();
			if (xmlOrganisationalRelationships != null && xmlOrganisationalRelationships.Any(x => x.GetPropertyOrBlankString("Relationship") == OrgPartRelation.RelationshipTypes.Owner)
				&& xmlOrganisationalRelationships.Any(x => x.GetPropertyOrBlankString("Relationship") == OrgPartRelation.RelationshipTypes.Supplier))
			{
				var childCollection = xmlOrganisationalRelationships.First().Parent.ChildrenCollection;
				var dic = new Dictionary<ZGuid, List<IEntity>>();

				foreach (var xmlRelation in xmlOrganisationalRelationships)
				{
					var relationshipType = xmlRelation.GetPropertyOrBlankString("Relationship");
					if (relationshipType == OrgPartRelation.RelationshipTypes.Owner || relationshipType == OrgPartRelation.RelationshipTypes.Supplier)
					{
						(var organisation, var org) = productEntityMatchingOrgCache.GetOrganisationFromEntityParent(xmlRelation);
						if (org != null)
						{
							if (!dic.ContainsKey(org.PK))
							{
								dic.Add(org.PK, new List<IEntity>() { xmlRelation });
							}
							else
							{
								dic[org.PK].Add(xmlRelation);
							}
						}
					}
				}

				foreach (var d in dic.Where(x => x.Value.Count > 1))
				{
					var index = 0;
					foreach (var e in d.Value)
					{
						var entity = e;
						childCollection.Remove(e);
						if (index == 0)
						{
							entity["Relationship"] = OrgPartRelation.RelationshipTypes.Both;
							childCollection.Add(entity);
						}
						index++;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfAnyRelationshipsAreNowInvalid(OrgSupplierPart partLoadedFromEnterprise, IEntity[] xmlRelationships)
		{
			foreach (var xmlRelation in xmlRelationships)
			{
				(var organisation, var org) = productEntityMatchingOrgCache.GetOrganisationFromEntityParent(xmlRelation);
				if (organisation != null)
				{
					var relationship = partLoadedFromEnterprise.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation.InternalPK, xmlRelation.GetPropertyOrBlankString("Relationship"));
					if (relationship != null)
					{
						FailIfChangingIsReleaseCapturedWithStockOnHand(relationship, xmlRelation);
						FailIfIsPartAttributeChangedWithStockOnHand(relationship, partLoadedFromEnterprise, xmlRelation);
						FailIfDeletingOrgPartRelationWithUnfinalisedASNLines(relationship, xmlRelation);
						FailIfChangingAttributeWithUnfinalisedASNLines(relationship, xmlRelation);
						FailIfUnitPriceChangedWithoutCurrency(relationship, partLoadedFromEnterprise, xmlRelation);
					}
					else
					{
						// Entering this branch indicates the changing of client code/relationship type in original relationships
						FailIfChangingOrgPartRelationWithUnfinalisedASNLines(partLoadedFromEnterprise.RelatedOrganisations, org);
					}
				}
			}
		}

		bool IsBooleanSetAndDifferentToPreviousValue(bool? entityValue, bool previousValue) => entityValue != null && entityValue != previousValue;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfRelationshipDoesNotMatchClientSetup(OrgHeader client, IEntity xmlRelation)
		{
			var partAttributeManager = client.PartAttributeManager;

			if (
				((GetBooleanFieldValueOrNull(xmlRelation, "UseExpiryDate") ?? false) && !partAttributeManager.IsExpiryDateUsedByOrganisation) ||
				((GetBooleanFieldValueOrNull(xmlRelation, "UsePackingDate") ?? false) && !partAttributeManager.IsPackingDateUsedByOrganisation) ||
				((GetBooleanFieldValueOrNull(xmlRelation, "UsePartAttrib1") ?? false) && !partAttributeManager.IsPartAttributeUsedByOrganisation(1)) ||
				((GetBooleanFieldValueOrNull(xmlRelation, "UsePartAttrib2") ?? false) && !partAttributeManager.IsPartAttributeUsedByOrganisation(2)) ||
				((GetBooleanFieldValueOrNull(xmlRelation, "UsePartAttrib3") ?? false) && !partAttributeManager.IsPartAttributeUsedByOrganisation(3)) ||
				((GetBooleanFieldValueOrNull(xmlRelation, "UseSerialNumber") ?? false) && !partAttributeManager.IsSerialNumberUsedByOrganisation))
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Attribute setup does not match Client = {0}.", client.OH_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfIsReleaseCapturedIsNotConsistentWithPartAttributeUsage(OrgHeader client, OrgPartRelation relationship, IEntity xmlRelation)
		{
			if (IsReleaseCapturedInconsistentWithPartAttributeUsage(xmlRelation, "IsPartAttrib1ReleaseCaptured", "UsePartAttrib1", relationship?.OU_IsPartAttrib1ReleaseCaptured ?? false, relationship?.OU_UsePartAttrib1 ?? false) ||
				IsReleaseCapturedInconsistentWithPartAttributeUsage(xmlRelation, "IsPartAttrib2ReleaseCaptured", "UsePartAttrib2", relationship?.OU_IsPartAttrib2ReleaseCaptured ?? false, relationship?.OU_UsePartAttrib2 ?? false) ||
				IsReleaseCapturedInconsistentWithPartAttributeUsage(xmlRelation, "IsPartAttrib3ReleaseCaptured", "UsePartAttrib3", relationship?.OU_IsPartAttrib3ReleaseCaptured ?? false, relationship?.OU_UsePartAttrib3 ?? false) ||
				IsReleaseCapturedInconsistentWithPartAttributeUsage(xmlRelation, "IsSerialNumberReleaseCaptured", "UseSerialNumber", relationship?.OU_IsSerialNumberReleaseCaptured ?? false, relationship?.OU_UseSerialNumber ?? false))
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Cannot ReleaseCapture unused PartAttributes. Client = {0}.", client.OH_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool IsReleaseCapturedInconsistentWithPartAttributeUsage(IEntity xmlRelation, string rcField, string usePartAttribField, bool relationRcValue, bool relationUsePartAttribValue)
		{
			var entityRcValue = GetBooleanFieldValueOrNull(xmlRelation, rcField);
			var entityUsePartAttribValue = GetBooleanFieldValueOrNull(xmlRelation, usePartAttribField);

			// If neither value set in XML, dont care
			return (entityRcValue ?? entityUsePartAttribValue) != null &&
				// if attribute is rc and part attrib is not used, that's an error. We use the value from the database if xml value not specified
				(entityRcValue ?? relationRcValue) && !(entityUsePartAttribValue ?? relationUsePartAttribValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfChangingIsReleaseCapturedWithStockOnHand(OrgPartRelation relationship, IEntity xmlRelation)
		{
			var client = relationship.Organisation;
			if (relationship.HasCurrentStockIncludingInTransit() &&
					(IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsPartAttrib1ReleaseCaptured"), relationship.OU_IsPartAttrib1ReleaseCaptured)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsPartAttrib2ReleaseCaptured"), relationship.OU_IsPartAttrib2ReleaseCaptured)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsPartAttrib3ReleaseCaptured"), relationship.OU_IsPartAttrib3ReleaseCaptured)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsSerialNumberReleaseCaptured"), relationship.OU_IsSerialNumberReleaseCaptured)))
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Cannot change ReleaseCaptured settings as there is current stock in the warehouse. Client = {0}.", client.OH_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfIsPartAttributeChangedWithStockOnHand(OrgPartRelation relationship, OrgSupplierPart part, IEntity xmlRelation)
		{
			var client = relationship.Organisation;
			if (IsPartAttribChangedWithStockOnHand(xmlRelation, "UseExpiryDate", client, part, WhsDocketLineSchema.WE_ExpiryDate)
					|| IsPartAttribChangedWithStockOnHand(xmlRelation, "UsePackingDate", client, part, WhsDocketLineSchema.WE_PackingDate)
					|| IsPartAttribChangedWithStockOnHand(xmlRelation, "UsePartAttrib1", client, part, WhsDocketLineSchema.WE_PartAttrib1)
					|| IsPartAttribChangedWithStockOnHand(xmlRelation, "UsePartAttrib2", client, part, WhsDocketLineSchema.WE_PartAttrib2)
					|| IsPartAttribChangedWithStockOnHand(xmlRelation, "UsePartAttrib3", client, part, WhsDocketLineSchema.WE_PartAttrib3)
					|| IsPartAttribChangedWithStockOnHand(xmlRelation, "UseSerialNumber", client, part, WhsDocketLineSchema.WE_SerialNumber))
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Inventory exists that is incompatible with the PartAttribute setup. Client = {0}.", client.OH_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfDeletingOrgPartRelationWithUnfinalisedASNLines(OrgPartRelation relationship, IEntity xmlRelation)
		{
			if (xmlRelation.Action == EntityAction.DELETE && relationship.HasAsnLineOnUnfinalisedReceive)
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Cannot delete Part RelationShip(s) as there are current ASN line(s) in the warehouse. Client = {0}.", relationship.Organisation.OH_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfChangingOrgPartRelationWithUnfinalisedASNLines(OrgPartRelationCollection relationships, OrgHeader org)
		{
			foreach (OrgPartRelation relationship in relationships)
			{
				// skip the case that org.PK != relationship.OU_OH, because it can't confirm they are the same relationship and will fail for other reason.
				if (org.PK == relationship.OU_OH && relationship.HasAsnLineOnUnfinalisedReceive)
				{
					Fail(string.Format(CultureInfo.InvariantCulture, "Cannot change Part RelationShip(s) as there are current ASN line(s) in the warehouse. Client = {0}.", relationship.Organisation.OH_Code));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfChangingAttributeWithUnfinalisedASNLines(OrgPartRelation relationship, IEntity xmlRelation)
		{
			var client = relationship.Organisation;
			if ((IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsPartAttrib1ReleaseCaptured"), relationship.OU_IsPartAttrib1ReleaseCaptured)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsPartAttrib2ReleaseCaptured"), relationship.OU_IsPartAttrib2ReleaseCaptured)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsPartAttrib3ReleaseCaptured"), relationship.OU_IsPartAttrib3ReleaseCaptured)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "IsSerialNumberReleaseCaptured"), relationship.OU_IsSerialNumberReleaseCaptured)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "UsePartAttrib1"), relationship.OU_UsePartAttrib1)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "UsePartAttrib2"), relationship.OU_UsePartAttrib2)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "UsePartAttrib3"), relationship.OU_UsePartAttrib3)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "UseExpiryDate"), relationship.OU_UseExpiryDate)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "UsePackingDate"), relationship.OU_UsePackingDate)
					 || IsBooleanSetAndDifferentToPreviousValue(GetBooleanFieldValueOrNull(xmlRelation, "UseSerialNumber"), relationship.OU_UseSerialNumber)) && relationship.HasAsnLineOnUnfinalisedReceive)
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Cannot change Attribute settings as there are current ASN line(s) in the warehouse. Client = {0}.", client.OH_Code));
			}
		}

		bool IsPartAttribChangedWithStockOnHand(IEntity xmlRelation, string entityField, OrgHeader client, OrgSupplierPart part, SchemaColumn inventoryColumn)
		{
			var entityValue = GetBooleanFieldValueOrNull(xmlRelation, entityField);
			return entityValue != null && PartAttributeValidation.CheckAttributeDefinitionForPartHasErrorIfChanged(part.Factory, client, part, inventoryColumn, entityValue.Value);
		}

		bool? GetBooleanFieldValueOrNull(IEntity entity, string name)
		{
			bool? result = null;

			if (bool.TryParse(entity.GetPropertyOrBlankString(name), out bool value))
			{
				result = value;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfUnitPriceChangedWithoutCurrency(OrgPartRelation relationship, OrgSupplierPart partLoadedFromEnterprise, IEntity xmlRelation)
		{
			var client = relationship.Organisation;
			var unitPriceCurrencyEntity = xmlRelation.Parents?.FirstOrDefault(x => x.EntityName == "UnitPriceCurrency");
			var xmlUnitPriceCurrency = unitPriceCurrencyEntity?.GetPropertyOrBlankString("Code");
			var priceAfterUpdate = GetDecimalFieldValueOrNull(xmlRelation, "UnitPrice") ?? relationship.OU_UnitPrice;
			var currencyAfterUpdate = xmlUnitPriceCurrency ?? relationship.OU_RX_NKUnitPriceCurrency;

			if (xmlRelation.Action != EntityAction.DELETE
					&& priceAfterUpdate > 0m
					&& string.IsNullOrEmpty(currencyAfterUpdate))
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Attempted to create a situation for Client: {0}, Part: {1} where the Unit Price is without a Unit Price Currency. ", client.OH_Code, partLoadedFromEnterprise.OP_PartNum));
			}
		}

		decimal? GetDecimalFieldValueOrNull(IEntity entity, string name)
		{
			decimal? result = null;

			if (decimal.TryParse(entity.GetPropertyOrBlankString(name), out decimal value))
			{
				result = value;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfAnyPivotsContainMultipleOrgHeaders(IEntity[] orgHeaders)
		{
			if (orgHeaders.Length > 1)
			{
				Fail("OrgHeader may be specified at most once on CusClassPartPivot.");
			}
		}

		void InsertEmptyFieldForParentPivotPropertiesNotGivenAndOhToo(IEntity parentPivot, IEntityDefinition cusClassPartPivotOrgHeaderDefinition, IPropertyDef[] columnsToCompareHonouringExclusions)
		{
			/* If XML contains this:
			 *	Product
			 *		Pivot
			 *			B=2
			 *		Pivot
			 *			A=1
			 *			B=2
			 *		Pivot
			 *			A=1
			 Then the engine will insert the first pivot and second pivot, but when it looks to see if any pivots already exist during the processing of the third, the query accidentally finds the middle one.
			 * That's because the query is "select * from dbo.cusclasspartPivot where A=1".  It finds the middle one 'cos it's not looking at field B. The result is that two rows are inserted instead of three.
			 * Instead insert a blank element for the missing fields, so what is processed is really:
			 *	Product
			 *		Pivot
			 *			A=blank
			 *			B=2			 *
			 *		Pivot
			 *			A=1
			 *			B=2
			 *		Pivot
			 *			A=1
			 *			B=blank
			 *	The query to find a row when processing the third row is then "select * from dbo.cusclasspartPivot where A=1 and B=blank", no row is found, and one is inserted.
			*/
			foreach (var def in columnsToCompareHonouringExclusions)
			{
				if (!parentPivot.HasProperty(def.PropertyName))
				{
					var schemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(def.ColumnDef.Name, CusClassPartPivotSchema.Constants.TableName);
					parentPivot[def.PropertyName] = schemaColumn.SqlDbDefault;
				}
			}
			var oh = parentPivot.GetParentEntity(XmlConstants.EntityNames.OrgHeader);
			if (oh == null)
			{
				// Add a blank OrgHeader entity so that it can be considered as part of the lookup of the pivot
				oh = new Entity(cusClassPartPivotOrgHeaderDefinition, sessionServices);
				//oh.Action = EntityAction.MERGE;
				((Entity)parentPivot).ParentCollection.Add(oh);
			}

			var org = productEntityMatchingOrgCache.GetOrgHeader(oh);
			if (org != null)
			{
				oh[XmlConstants.PropertyNames.Code] = org.OH_Code;
			}
			else
			{
				var orgAddress = CreateMIDOrganizationIfNecessary(oh);
				if (orgAddress != null)
				{
					productEntityMatchingOrgCache.UpdateOrgHeader(oh, orgAddress.Header);
				}
			}
		}

		void InsertEmptyCusCountrySpecificClassificationUnderPivotIfMissing(IEntity pivot, IEntityDefinition cusCountrySpecificClassificationDefinition)
		{
			string entityName = cusCountrySpecificClassificationDefinition.EntityName;
			if (pivot.Action != EntityAction.DELETE && !pivot.Children.Any(e => e.EntityName == entityName))
			{
				var mergeNewClassification = new Entity(cusCountrySpecificClassificationDefinition, sessionServices)
				{
					Action = EntityAction.MERGE
				};
				pivot.ChildrenCollection.Add(mergeNewClassification);
				mergeNewClassification.Parent = pivot;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void ProcessAfterSeekingExistingRow(IEntity xmlProduct, string productNumberFromXml, IEntity[] xmlPivots, IEntityDefinition pivotDefinition, List<RelatedPartyWithType> listXmlOrgTypesByCodeExceptDeletes, List<RelatedPartyWithType> listXmlOrgTypesByCodeInserts, OrgSupplierPart partLoadedFromEnterprise, bool dontTouchExistingOrgsBecauseNoneWereInXml = false)
		{
			if (partLoadedFromEnterprise != null)
			{
				//Exact match... OK for everything except insert.
				if (xmlProduct.Action == EntityAction.INSERT)
				{
					var listOfOrgs = new ZStringBuilder();
					foreach (OrgPartRelation rel in partLoadedFromEnterprise.RelatedOrganisations)
					{
						listOfOrgs.Append(rel.CodeAndRelationship);
					}
					Fail(ZString.Format("Cannot insert, product '{0}' with related organisations {1} already exists.", productNumberFromXml, listOfOrgs.ToStringWithDelimiterBetweenAppends(", ")));
				}
				else
				{
					xmlProduct.InternalPK = partLoadedFromEnterprise.PK.ToGuid();  // Native engine will use this fellow... but check that any proposed update is OK
					ValidateProposedChanges(productNumberFromXml, listXmlOrgTypesByCodeExceptDeletes, listXmlOrgTypesByCodeInserts, partLoadedFromEnterprise, dontTouchExistingOrgsBecauseNoneWereInXml);
				}
				DeleteAllExistingComponentPivotsAndTheirChildrenOnFoundParentPivotIfPivotFound(xmlProduct, partLoadedFromEnterprise, xmlPivots, pivotDefinition);
			}
			else
			{
				ValidateProposedChanges(productNumberFromXml, listXmlOrgTypesByCodeExceptDeletes, listXmlOrgTypesByCodeInserts, partLoadedFromEnterprise);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void ValidateProposedChanges(string productNumber, IEnumerable<RelatedPartyWithType> listXmlOrgTypesByCodeExceptDeletes, IEnumerable<RelatedPartyWithType> listXmlOrgTypesByCodeInserts, OrgSupplierPart partLoadedFromEnterprise, bool dontTouchExistingOrgsBecauseNoneWereInXml = false)
		{
			var pkOfFoundRow = partLoadedFromEnterprise?.PK ?? ZGuid.Empty;
			var relationsWithCode = new List<RelatedPartyWithCode>();
			if (!dontTouchExistingOrgsBecauseNoneWereInXml)
			{
				foreach (var rel in listXmlOrgTypesByCodeExceptDeletes)
				{
					relationsWithCode.Add(new RelatedPartyWithCode(rel.RelationshipCode, rel.RelatedOrganisationCode));
				}
			}

			var validator = new DuplicateProductDetectorNoBizO(pkOfFoundRow, productNumber, true, relationsWithCode, null);  // This last arg allows us to pass over any existing parts that are not yet in the DB to allow the validator to consider them too. This interceptor does not know about anything except the one part currently being considered even if loads exist in the source XML. We must assume here that each product is saved independently.
			validator.Validate();

			if (validator.HasError)
			{
				Fail(ZString.Format("Cannot process {0}. {1}", productNumber, validator.ErrorMessage));
			}

			foreach (var x in listXmlOrgTypesByCodeExceptDeletes)
			{
				var indexerAsIEnumerable = new RelatedParty[] { x };
				var listExceptIndexer = listXmlOrgTypesByCodeExceptDeletes.Except(indexerAsIEnumerable);
				if (OrgPartRelationValidationHelper.HasDuplicateRelationship(listExceptIndexer, x.RelatedOrganisationPK, x.RelationshipCode))
				{
					Fail(ZString.Format("Cannot process {0}. Related organisation {1}: {2}", productNumber, x.RelatedOrganisationCode, OrgPartRelation.DuplicateRelationshipError));
				}

				if (OrgPartRelationValidationHelper.HasSameSupplierAndOwner(listExceptIndexer, x.RelatedOrganisationPK, x.RelationshipCode))
				{
					Fail(ZString.Format("Cannot process {0}. Related organisation {1}: {2}", productNumber, x.RelatedOrganisationCode, OrgPartRelation.SameOrgAsOwnerAndSupplier));
				}
			}

			if (partLoadedFromEnterprise != null)
			{
				foreach (var x in listXmlOrgTypesByCodeInserts)
				{
					if (partLoadedFromEnterprise.RelatedOrganisations.Cast<OrgPartRelation>().Any(r => r.OU_Relationship == x.RelationshipCode && r.OU_OH == x.RelatedOrganisationPK))
					{
						Fail(ZString.Format("Cannot process {0}. Related organisation {1}: {2}", productNumber, x.RelatedOrganisationCode, OrgPartRelation.DuplicateRelationshipError));
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void CheckXmlHasBareMinimumWeNeedRegardingOrgsAndRels(IEnumerable<IEntity> xmlOrganisationalRelationships)
		{
			foreach (var node in (from IEntity e in xmlOrganisationalRelationships where e.Properties.Any(c => c.Name == "Relationship") select e))
			{
				if (node != null)
				{
					var orgInXml = node.Parents.FirstOrDefault(c => c.EntityName == XmlConstants.EntityNames.OrgHeader);
					if (orgInXml != null)
					{
						var firstCodeNode = orgInXml.Properties.FirstOrDefault(p => p.Name == XmlConstants.PropertyNames.Code);
						if (firstCodeNode == null || string.IsNullOrWhiteSpace(firstCodeNode.Value.ToString()))
						{
							Fail("Illegal XML; the Relationship requires a Code for the Organisation");
						}
					}
					else
					{
						Fail("Illegal XML; the Relationship requires an Organisation");
					}
				}
				else
				{
					// Should already have been handled
				}
			}
		}

		void Fail(string p)
		{
			throw new NativeXMLUserVisibleException(p);
		}

		public static ZQuery GetAdditionalRelationshipQuery(ZGuid organisationPK, string[] relationshipTypes)
		{
			var filter = new ZQuery(OrgPartRelationSchema.OU_Relationship, relationshipTypes);
			filter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_OH, organisationPK);
			return filter;
		}

		public static IEnumerable<IEntity> FindOrganisationalRelationshipsDirectlyBelowOrgSupplierPart(IEntity product)
		{
			return product.Children.Where(e => e.EntityName == "OrgPartRelation");
		}

		public static int GetInstanceKey(IEntity entity) => RuntimeHelpers.GetHashCode(entity);

		(string[] attribute1, string[] attribute2, string[] attribute3) GetPivotAttributes(IEntity pivot, bool allowDuplicate)
		{
			var key = $"{allowDuplicate}-{GetInstanceKey(pivot)}";
			if (!pivotAttributesDictionary.TryGetValue(key, out var data))
			{
				var attrs1 = MatchingHelper.GetAttributes(pivot, AttributeNames.AT1);
				var attrs2 = MatchingHelper.GetAttributes(pivot, AttributeNames.AT2);
				var attrs3 = MatchingHelper.GetAttributes(pivot, AttributeNames.AT3);
				if (allowDuplicate)
				{
					data = (attrs1.ToArray(), attrs2.ToArray(), attrs3.ToArray());
				}
				else
				{
					data = (attrs1.Distinct().ToArray(), attrs2.Distinct().ToArray(), attrs3.Distinct().ToArray());
				}

				pivotAttributesDictionary.Add(key, data);
			}
			return data;
		}
		Dictionary<string, (string[] attribute1, string[] attribute2, string[] attribute3)> pivotAttributesDictionary;

		void DeleteChildrenByInsertingActionDeleteNodesIntoXml(RowFactory rowFactory, ChildEntityMatchingInfo childEntityMatchingInfo, IEntity deletingParentEntity, DataRow[] childrenRowToDelete)
		{
			string childTablePKColumnName = childEntityMatchingInfo.TableSchema.PK.Name;
			var childDefinition = childEntityMatchingInfo.ChildEntityDefinition;
			var grandChildEntityMatchingInfos = childEntityMatchingInfo.ChildrenEntityMatchingInfos.ToArray();
			foreach (var childRow in childrenRowToDelete)
			{
				var deleteChild = new Entity(childDefinition, sessionServices);
				var childPk = (Guid)childRow[childTablePKColumnName];
				deleteChild.InternalPK = childPk;
				deleteChild.Action = EntityAction.DELETE;
				deletingParentEntity.ChildrenCollection.AddAtStart(deleteChild);
				foreach (var grandChildEntityMatchingInfo in grandChildEntityMatchingInfos)
				{
					DeleteChildrenByInsertingActionDeleteNodesIntoXml(rowFactory, grandChildEntityMatchingInfo, deleteChild, rowFactory.Load(grandChildEntityMatchingInfo.TableSchema.TableName, grandChildEntityMatchingInfo.GetParentQuery(childRow)));
				}
			}
		}

		class CusClassPartPivotEntityMatchingInfo : ChildEntityMatchingInfo
		{
			public CusClassPartPivotEntityMatchingInfo(Func<IEntity, DataRow> findRow, IEntityDefinition childEntityDefinition,
				Func<IEntity, string> getIdentificationMessage,
				Func<DataRow, ZQuery> getParentQuery,
				Func<IEntity, (string[] attribute1, string[] attribute2, string[] attribute3)> getPivotAttributes,
				Func<IEntity, (IEntity entity, OrgHeader org)> getOrganisationEntity,
				bool isParentPivot,
				bool deleteOnlyIfExistsInXml = false)
				: base(findRow, childEntityDefinition, getIdentificationMessage,
					GetColumnsToCompare(childEntityDefinition, IsValidForComponentMatch),
					getParentQuery,
					deleteOnlyIfExistsInXml: deleteOnlyIfExistsInXml)
			{
				this.getPivotAttributes = getPivotAttributes;
				this.getOrganisationEntity = getOrganisationEntity;
				this.isParentPivot = isParentPivot;
			}

			static bool IsValidForComponentMatch(string columnName)
			{
				switch (columnName)
				{
					case CusClassPartPivotSchema.Constants.PK:
					case CusClassPartPivotSchema.Constants.CI_LastAuditedDate:
					case CusClassPartPivotSchema.Constants.CI_LastAuditedUser:
					case CusClassPartPivotSchema.Constants.CI_AddInfo:
					case CusClassPartPivotSchema.Constants.CI_NAddInfo:
						return false;
					default:
						return true;
				}
			}

			protected override ZQuery GetAdditionalQueryCore(IEntity entity, string identificationMessage, bool treatAbsentXmlElementAsEmpty, Action<string> reportFail)
			{
				return isParentPivot
					? GetParentPivotQuery(entity, identificationMessage, treatAbsentXmlElementAsEmpty, reportFail)
					: GetComponentPivotQuery(entity, identificationMessage, treatAbsentXmlElementAsEmpty, reportFail);
			}

			ZQuery GetComponentPivotQuery(IEntity entity, string identificationMessage, bool treatAbsentXmlElementAsEmpty, Action<string> reportFail)
			{
				var query = base.GetAdditionalQueryCore(entity, identificationMessage, treatAbsentXmlElementAsEmpty, reportFail);
				foreach (var parentDefinition in ChildEntityDefinition.Parents)
				{
					var parentEntityName = parentDefinition.EntityName;
					switch (parentEntityName)
					{
						case XmlConstants.EntityNames.Country:
							query.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, entity.GetParentEntity(XmlConstants.EntityNames.Country)[XmlConstants.PropertyNames.Code]);
							break;
						case XmlConstants.EntityNames.CountryOfExport:
							query.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountryOfExport, GetValueFromMatchedParentEntityOrBlankString(entity, XmlConstants.EntityNames.CountryOfExport, RefCountrySchema.Constants.RN_Code));
							break;
						case XmlConstants.EntityNames.CountryOfOrigin:
							query.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountryOfOrigin, GetValueFromMatchedParentEntityOrBlankString(entity, XmlConstants.EntityNames.CountryOfOrigin, RefCountrySchema.Constants.RN_Code));
							break;
						case XmlConstants.EntityNames.OriginState:
							query.AddToFilter(CusClassPartPivotSchema.CI_RW_NKOriginState, GetValueFromMatchedParentEntityOrBlankString(entity, XmlConstants.EntityNames.OriginState, RefCountryStatesSchema.Constants.RW_Code));
							break;
						case XmlConstants.EntityNames.TaxType:
							query.AddToFilter(CusClassPartPivotSchema.CI_ZZF_NKTaxType, GetValueFromMatchedParentEntityOrBlankString(entity, XmlConstants.EntityNames.TaxType, RefCusTaxOrFeeSchema.Constants.ZZF_Code));
							break;
						case XmlConstants.EntityNames.OrgHeader:
						case XmlConstants.EntityNames.CusClassification:
						case XmlConstants.EntityNames.CusClassPartPivot:
						case XmlConstants.EntityNames.OrgSupplierPart:
							// already handle via GetParentQuery
							break;
						default:
							ErrorReporter.ReportOnce($"Missing support for parent '{parentEntityName}' of {entity.EntityName}");
							break;
					}
				}

				return query;
			}

			ZQuery GetParentPivotQuery(IEntity entity, string identificationMessage, bool treatAbsentXmlElementAsEmpty, Action<string> reportFail)
			{
				var result = new ZQuery(CusClassPartPivotSchema.CI_ChildType, entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.ChildType));
				result.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, entity.GetParentEntity(XmlConstants.EntityNames.Country)[XmlConstants.PropertyNames.Code]);
				(var orgXmlEntity, var org) = getOrganisationEntity(entity);
				if (IsEntityEmpty(orgXmlEntity))
				{
					result.AddToFilter(CusClassPartPivotSchema.CI_OH, null);
				}
				else
				{
					result.AddToFilter(CusClassPartPivotSchema.CI_OH, org?.PK ?? ZGuid.Invalid);
				}
				AddColumnFilter(result, entity, identificationMessage, treatAbsentXmlElementAsEmpty, CusClassPartPivotSchema.CI_DateStart, XmlConstants.PropertyNames.DateStart, reportFail);
				AddColumnFilter(result, entity, identificationMessage, treatAbsentXmlElementAsEmpty, CusClassPartPivotSchema.CI_DateEnd, XmlConstants.PropertyNames.DateEnd, reportFail);
				return result;
			}

			protected override DataRow[] AdditionalMatchingCore(RowFactory rowFactory, IEntity pivotEntity, DataRow[] matchingPivotRows)
			{
				var result = matchingPivotRows;
				if (result.Length > 0)
				{
					result = isParentPivot ? MatchAttributesIfNeeded(rowFactory, pivotEntity, result) : MatchAddInfo(pivotEntity, result, AddInfoPropertyDefinition, NAddInfoPropertyDefinition);
				}
				return result;
			}

			IPropertyDef AddInfoPropertyDefinition => addInfoPropertyDefinition ?? (addInfoPropertyDefinition = ChildEntityDefinition.PropertyDefinitions[XmlConstants.PropertyNames.AddInfo]);
			IPropertyDef addInfoPropertyDefinition;

			IPropertyDef NAddInfoPropertyDefinition => nAddInfoPropertyDefinition ?? (nAddInfoPropertyDefinition = ChildEntityDefinition.PropertyDefinitions[XmlConstants.PropertyNames.NAddInfo]);
			IPropertyDef nAddInfoPropertyDefinition;

			DataRow[] MatchAttributesIfNeeded(RowFactory rowFactory, IEntity pivotEntity, DataRow[] matchingPivotRows)
			{
				DataRow[] result = matchingPivotRows;
				if (MatchingHelper.IsChildTypeHTI(pivotEntity))
				{
					var matched = new List<DataRow>();
					(var attrib1s, var attrib2s, var attrib3s) = getPivotAttributes(pivotEntity);
					foreach (var matchingPivotRow in matchingPivotRows)
					{
						ZGuid rowPK = ZGuid.Empty;
						if (IsAttributesEqualToEntityAttributes(rowFactory, rowPK, attrib1s, AttributeNames.AT1) &&
							IsAttributesEqualToEntityAttributes(rowFactory, rowPK, attrib2s, AttributeNames.AT2) &&
							IsAttributesEqualToEntityAttributes(rowFactory, rowPK, attrib3s, AttributeNames.AT3))
						{
							matched.Add(matchingPivotRow);
						}
					}

					result = matched.ToArray();
				}

				return result;
			}

			bool IsAttributesEqualToEntityAttributes(RowFactory rowFactory, ZGuid rowPK, IEnumerable<string> entityAttributes, ZString attributeName)
			{
				var query = new ZQuery(CusAttributeFilterSchema.BG_CI, rowPK);
				query.AddToFilter(CusAttributeFilterSchema.BG_AttributeName, attributeName);
				var foundAttrs = rowFactory.Load(CusAttributeFilterSchema.Constants.TableName, query)
					.Select(x => x[CusAttributeFilterSchema.Constants.BG_AttributeValue1].ToString()).Distinct();
				return (new HashSet<string>(entityAttributes)).SetEquals(foundAttrs);
			}
			readonly Func<IEntity, (string[] attribute1, string[] attribute2, string[] attribute3)> getPivotAttributes;
			readonly Func<IEntity, (IEntity entity, OrgHeader org)> getOrganisationEntity;
			readonly bool isParentPivot;
		}

		static DataRow[] MatchAddInfo(IEntity entity, DataRow[] matchingPivotRows, IPropertyDef addInfoPropertyDefinition, IPropertyDef nAddInfoPropertyDefinition = null)
		{
			var result = MatchAddInfo(entity.GetPropertyOrBlankString(addInfoPropertyDefinition.PropertyName), matchingPivotRows, addInfoPropertyDefinition);
			if (nAddInfoPropertyDefinition != null)
			{
				result = MatchAddInfo(entity.GetPropertyOrBlankString(nAddInfoPropertyDefinition.PropertyName), result.ToArray(), nAddInfoPropertyDefinition);
			}
			return result.ToArray();
		}

		static IEnumerable<DataRow> MatchAddInfo(string addInfoValue, IEnumerable<DataRow> matchingPivotRows, IPropertyDef propertyDef)
		{
			return matchingPivotRows.Where(r => MatchAddInfo(r[propertyDef.ColumnDef.Name].ToString(), addInfoValue));
		}

		static bool MatchAddInfo(string rowAddInfo, string xmlAddInfo)
		{
			return xmlAddInfo.Equals(rowAddInfo) || xmlAddInfo.Equals(GetSortedAddInfoData(rowAddInfo));
		}

		static string GetSortedAddInfoData(string rowAddInfo) => AddInfoParser.CreateDictionaryWithAddInfoString(rowAddInfo).CondenseKeyValuePairsIntoSortedOneString();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		class ChildEntityMatchingInfo
		{
			public ChildEntityMatchingInfo(Func<IEntity, DataRow> findRow, IEntityDefinition childEntityDefinition, Func<IEntity, string> getIdentificationMessage, IPropertyDef[] columnsToCompare, Func<DataRow, ZQuery> getParentQuery, bool deleteOnlyIfExistsInXml = false)
			{
				this.FindRow = findRow;
				ChildEntityDefinition = childEntityDefinition;
				TableSchema = EnterpriseSchema.GetTableSchema(childEntityDefinition.TableName);
				GetIdentificationMessage = getIdentificationMessage;
				GetParentQuery = getParentQuery;
				DeleteOnlyIfExistsInXml = deleteOnlyIfExistsInXml;
				childrenEntityMatchingInfos = new List<ChildEntityMatchingInfo>();
				this.columnsToCompare = columnsToCompare;
			}

			public IEntityDefinition ChildEntityDefinition { get; private set; }
			public ITableSchema TableSchema { get; private set; }
			public Func<IEntity, string> GetIdentificationMessage { get; private set; }
			public Func<DataRow, ZQuery> GetParentQuery { get; protected set; }
			public ZQuery GetAdditionalQuery(IEntity entity, string identificationMessage, bool treatAbsentXmlElementAsEmpty, Action<string> reportFail) => GetAdditionalQueryCore(entity, identificationMessage, treatAbsentXmlElementAsEmpty, reportFail);
			protected virtual ZQuery GetAdditionalQueryCore(IEntity entity, string identificationMessage, bool treatAbsentXmlElementAsEmpty, Action<string> reportFail)
			{
				var result = new ZQuery();
				foreach (var column in columnsToCompare)
				{
					AddColumnFilter(result, entity, identificationMessage, treatAbsentXmlElementAsEmpty, TableSchema.GetSchemaColumn(column.ColumnDef.Name), column.PropertyName, reportFail);
				}
				return result;
			}

			protected void AddColumnFilter(ZQuery result, IEntity entity, string identificationMessage, bool treatAbsentXmlElementAsEmpty,
				SchemaColumn schemaColumn, string propertyName, Action<string> reportFail)
			{
				if (entity.HasProperty(propertyName))
				{
					var propertyValue = entity[propertyName];
					try
					{
						var zTypeValue = PropertyConverter.ObjectToZType(schemaColumn.GetEquivalentZType(), propertyValue);
						result.AddToFilter(schemaColumn, zTypeValue);
					}
					catch (ZTypeValueException typeValueException)
					{
						reportFail(string.Format("Cannot process {0}.  The value of {1} is invalid.  {2}", identificationMessage,
							propertyName, typeValueException.Message));
					}
					catch (ArgumentException)
					{
						// The exception message will be something like: See WI00114690 for details. ParameterName='', SchemaColumn='CI_DateStart', Value='' , which is confusing for the user.
						reportFail(string.Format("Cannot process {0}.  The value '{1}' for {2} is not valid or out of range.",
							identificationMessage, propertyValue.ToString().Trim(), propertyName));
					}
				}
				else if (treatAbsentXmlElementAsEmpty)
				{
					var zTypeValue = ZDataType.ZTypeToEmptyValue(schemaColumn.GetEquivalentZType());
					result.AddToFilter(schemaColumn, zTypeValue);
				}
			}

			protected Func<IEntity, DataRow> FindRow { get; private set; }

			protected string GetValueFromMatchedParentEntityOrBlankString(IEntity entity, string parentEntityName, string column)
			{
				var result = string.Empty;
				var parentEntity = entity.GetParentEntity(parentEntityName);
				if (parentEntity != null && FindRow(parentEntity) is DataRow dataRow)
				{
					result = (string)dataRow[column];
				}
				return result;
			}

			public bool DeleteOnlyIfExistsInXml { get; private set; }
			public DataRow[] AdditionalMatching(RowFactory rowFactory, IEntity pivotEntity, DataRow[] matchingPivotRows) => AdditionalMatchingCore(rowFactory, pivotEntity, matchingPivotRows);
			protected virtual DataRow[] AdditionalMatchingCore(RowFactory rowFactory, IEntity pivotEntity, DataRow[] matchingPivotRows) => matchingPivotRows;

			public void AddChildEntityMatchingInfo(ChildEntityMatchingInfo childEntityMatchingInfo)
			{
				childrenEntityMatchingInfos.Add(childEntityMatchingInfo);
			}

			public IEnumerable<ChildEntityMatchingInfo> ChildrenEntityMatchingInfos => childrenEntityMatchingInfos.Cast<ChildEntityMatchingInfo>();
			readonly List<ChildEntityMatchingInfo> childrenEntityMatchingInfos;
			readonly IPropertyDef[] columnsToCompare;
		}

		CusClassPartPivotEntityMatchingInfo GetCusClassPartPivotMatchingInfo(IEntityDefinition cusClassPartPivotDefinition, Guid partPK, bool isParentPivot)
		{
			var entityName = cusClassPartPivotDefinition.EntityName;
			(IEntityDefinition cusAttributeFilterDefinition, IEntityDefinition cusCodeDataDefinition, IEntityDefinition cusAddInfoDefinition, IEntityDefinition componentCusClassPartPivotDefinition, IEntityDefinition cusUSClassificationDefinition, IEntityDefinition cusLineTariffDetailDefinition) = GetCusClassPartPivotChildrenDefinition(cusClassPartPivotDefinition, isParentPivot, entityName);
			var result = new CusClassPartPivotEntityMatchingInfo(FindRow, cusClassPartPivotDefinition, (entity) =>
			{
				var childType = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.ChildType);
				var countryCode = entity.GetParentEntity(XmlConstants.EntityNames.Country)[XmlConstants.PropertyNames.Code];// Country Code should already be fixed in precheck
				var tariffNum = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.TariffNum);
				return $"{entityName}(ChildType='{childType}', Country='{countryCode}', TariffNum='{tariffNum}')";
			},
				(parentRow) =>
				{
					var query = new ZQuery(CusClassPartPivotSchema.CI_OP, partPK);
					if (isParentPivot)
					{
						query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, null);
					}
					else
					{
						query.AddToFilter(CusClassPartPivotSchema.CI_CI_Parent, parentRow[CusClassPartPivotSchema.Constants.PK]);
					}
					return query;
				}, getPivotAttributes: (x) => GetPivotAttributes(x, allowDuplicate: false), productEntityMatchingOrgCache.GetOrganisationFromEntityParent, isParentPivot);
			var tableSchema = result.TableSchema;
			result.AddChildEntityMatchingInfo(GetCusCodeDataMatchingInfo(cusCodeDataDefinition, tableSchema));
			result.AddChildEntityMatchingInfo(GetAdditionalInformationChildMatchingInfo(cusAddInfoDefinition, isParentPivot));
			if (isParentPivot)
			{
				result.AddChildEntityMatchingInfo(GetCusAttributeFilterMatchingInfo(cusAttributeFilterDefinition));
				result.AddChildEntityMatchingInfo(GetCusClassPartPivotMatchingInfo(componentCusClassPartPivotDefinition, partPK, isParentPivot: false));
			}
			else
			{
				result.AddChildEntityMatchingInfo(GetCusUSClassificationMatchingInfo(cusUSClassificationDefinition));
			}

			if (cusLineTariffDetailDefinition != null)
			{
				result.AddChildEntityMatchingInfo(GetCusLineTariffDetailChildMatchingInfo(cusLineTariffDetailDefinition, tableSchema));
			}

			return result;
		}

		(IEntityDefinition cusAttributeFilterDefinition, IEntityDefinition cusCodeDataDefinition, IEntityDefinition cusAddInfoDefinition,
			IEntityDefinition componentCusClassPartPivotDefinition, IEntityDefinition cusUSClassificationDefinition, IEntityDefinition cusLineTariffDetailDefinition) GetCusClassPartPivotChildrenDefinition(IEntityDefinition cusClassPartPivotDefinition,
			bool isParentPivot, string entityName)
		{
			IEntityDefinition cusCodeDataDefinition = null;
			IEntityDefinition cusAttributeFilterDefinition = null;
			IEntityDefinition cusAddInfoDefinition = null;
			IEntityDefinition componentCusClassPartPivotDefinition = null;
			IEntityDefinition cusUSClassificationDefinition = null;
			IEntityDefinition cusLineTariffDetail = null;
			foreach (var childDefinition in cusClassPartPivotDefinition.Children)
			{
				var childEntityName = childDefinition.EntityName;
				switch (childEntityName)
				{
					case XmlConstants.EntityNames.CusCAClassification:
					case XmlConstants.EntityNames.CusCNClassification:
					case XmlConstants.EntityNames.CusSupportingInfo:
					case XmlConstants.EntityNames.StmNote:
						// do nothing as we don't delete them currently
						break;
					case XmlConstants.EntityNames.CusUSClassification:
						if (!isParentPivot)
						{
							cusUSClassificationDefinition = childDefinition; // We only delete on component pivots
						}

						break;
					case XmlConstants.EntityNames.CusCodeDataCensus:
						cusCodeDataDefinition = childDefinition;
						break;
					case XmlConstants.EntityNames.CusAttributeFilter when (isParentPivot):
						cusAttributeFilterDefinition = childDefinition;
						break;
					case XmlConstants.EntityNames.AdditionalInformationChild:
						cusAddInfoDefinition = childDefinition;
						break;
					case XmlConstants.EntityNames.ComponentCusClassPartPivot when (isParentPivot):
						componentCusClassPartPivotDefinition = childDefinition;
						break;
					case XmlConstants.EntityNames.CusLineTariffDetail:
						cusLineTariffDetail = childDefinition;
						break;
					default:
						ErrorReporter.ReportOnce($"Missing support for child '{childEntityName}' of {entityName}");
						break;
				}
			}

			return (cusAttributeFilterDefinition, cusCodeDataDefinition, cusAddInfoDefinition, componentCusClassPartPivotDefinition, cusUSClassificationDefinition, cusLineTariffDetail);
		}

		ChildEntityMatchingInfo GetAdditionalInformationChildMatchingInfo(IEntityDefinition additionalInformationChild, bool deleteOnlyIfExistsInXml)
		{
			var entityName = additionalInformationChild.EntityName;
			IEntityDefinition cusCodeDataFdaAffirmationDefinition = null;
			IEntityDefinition additionalInformationGrandChildDefinition = null;
			IEntityDefinition lpcoDefinition = null;
			foreach (var childDefinition in additionalInformationChild.Children)
			{
				var childEntityName = childDefinition.EntityName;
				switch (childEntityName)
				{
					case XmlConstants.EntityNames.CusCodeDataFdaAffirmation:
						cusCodeDataFdaAffirmationDefinition = childDefinition;
						break;
					case XmlConstants.EntityNames.AdditionalInformationGrandChild:
						additionalInformationGrandChildDefinition = childDefinition;
						break;
					case XmlConstants.EntityNames.CusCALPCO:
						lpcoDefinition = childDefinition;
						break;
					default:
						ErrorReporter.ReportOnce($"Missing support for child '{childEntityName}' of {entityName}");
						break;
				}
			}

			var result = new CusAddInfoMatchingInfo(FindRow, additionalInformationChild, CusClassPartPivotSchema.Instance, deleteOnlyIfExistsInXml);
			result.AddChildEntityMatchingInfo(GetCusCodeDataMatchingInfo(cusCodeDataFdaAffirmationDefinition, result.TableSchema));
			result.AddChildEntityMatchingInfo(GetAdditionalInformationGrandChildMatchingInfo(additionalInformationGrandChildDefinition));
			result.AddChildEntityMatchingInfo(GetCusCALPCOMatchingInfo(lpcoDefinition, result.TableSchema));
			return result;
		}

		ChildEntityMatchingInfo GetCusLineTariffDetailChildMatchingInfo(IEntityDefinition cusLineTariffDetailDefinition, ITableSchema parenTableSchema)
		{
			return new ChildEntityMatchingInfo(FindRow, cusLineTariffDetailDefinition, (entity) =>
			{
				var type = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.Type);
				return $"{entity.EntityName}(Type='{type}')";
			},
				GetColumnsToCompare(cusLineTariffDetailDefinition, IsValidForCusLineTariffDetailMatch),
				(pivotRow) =>
				{
					var queryForCusLineTariffDetail = new ZQuery(CusLineTariffDetailSchema.BZ_ParentID, pivotRow[parenTableSchema.PK.Name]);
					queryForCusLineTariffDetail.AddToFilter(CusLineTariffDetailSchema.BZ_ParentTableCode, TableNameHelper.GetPrefixFromTableName(parenTableSchema.TableName));
					return queryForCusLineTariffDetail;
				});
		}

		static bool IsValidForCusLineTariffDetailMatch(string columnName)
		{
			switch (columnName)
			{
				case CusLineTariffDetailSchema.Constants.PK:
				case CusLineTariffDetailSchema.Constants.BZ_NAddInfo:
					return false;
				default:
					return true;
			}
		}

		ChildEntityMatchingInfo GetAdditionalInformationGrandChildMatchingInfo(IEntityDefinition additionalInformationGrandChildDefinition)
		{
			var entityName = additionalInformationGrandChildDefinition.EntityName;
			IEntityDefinition additionalInformationGreatGrandChildDefinition = null;
			foreach (var childDefinition in additionalInformationGrandChildDefinition.Children)
			{
				var childEntityName = childDefinition.EntityName;
				switch (childEntityName)
				{
					case XmlConstants.EntityNames.AdditionalInformationGreatGrandChild:
						additionalInformationGreatGrandChildDefinition = childDefinition;
						break;
					default:
						ErrorReporter.ReportOnce($"Missing support for child '{childEntityName}' of {entityName}");
						break;
				}
			}

			var tableSchema = CusAddInfoSchema.Instance;
			var result = new CusAddInfoMatchingInfo(FindRow, additionalInformationGrandChildDefinition, tableSchema, false);
			result.AddChildEntityMatchingInfo(new CusAddInfoMatchingInfo(FindRow, additionalInformationGreatGrandChildDefinition, tableSchema, false));
			return result;
		}

		class CusAddInfoMatchingInfo : ChildEntityMatchingInfo
		{
			public CusAddInfoMatchingInfo(Func<IEntity, DataRow> findRow, IEntityDefinition cusAddInfoDefinition, ITableSchema parentTableSchema,
				bool deleteOnlyIfExistsInXml)
				: base(findRow, cusAddInfoDefinition, GetCusAddInfoIdentificationMessage,
					GetColumnsToCompare(cusAddInfoDefinition, IsValidForCusAddInfoMatch), null,
					deleteOnlyIfExistsInXml: deleteOnlyIfExistsInXml)
			{
				this.parentPKSchemaName = parentTableSchema.PK.Name;
				this.parentTableCode = TableNameHelper.GetPrefixFromTableName(parentTableSchema.TableName);
				GetParentQuery = (parentRow) =>
				{
					var queryForCusAddInfo = new ZQuery(CusAddInfoSchema.B7_ParentID, parentRow[parentPKSchemaName]);
					queryForCusAddInfo.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, parentTableCode);
					return queryForCusAddInfo;
				};
			}
			readonly string parentPKSchemaName;
			readonly string parentTableCode;

			static string GetCusAddInfoIdentificationMessage(IEntity entity)
			{
				var type = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.Type);
				return $"{entity.EntityName}(Type='{type}')";
			}

			IPropertyDef AddInfoPropertyDefinition => addInfoPropertyDefinition ?? (addInfoPropertyDefinition = ChildEntityDefinition.PropertyDefinitions[XmlConstants.PropertyNames.AddInfoData]);
			IPropertyDef addInfoPropertyDefinition;

			IPropertyDef NAddInfoPropertyDefinition => nAddInfoPropertyDefinition ?? (nAddInfoPropertyDefinition = ChildEntityDefinition.PropertyDefinitions[XmlConstants.PropertyNames.NAddInfoData]);
			IPropertyDef nAddInfoPropertyDefinition;

			protected override DataRow[] AdditionalMatchingCore(RowFactory rowFactory, IEntity pivotEntity, DataRow[] matchingPivotRows)
			{
				return MatchAddInfo(pivotEntity, matchingPivotRows, AddInfoPropertyDefinition, NAddInfoPropertyDefinition);
			}
		}

		static IPropertyDef[] GetColumnsToCompare(IEntityDefinition definition, Func<string, bool> isValidForMatch) => definition.PropertyDefinitions.Where(d => isValidForMatch(d.ColumnDef.Name)).ToArray();

		static bool IsValidForCusAddInfoMatch(string columnName)
		{
			switch (columnName)
			{
				case CusAddInfoSchema.Constants.PK:
				case CusAddInfoSchema.Constants.B7_AddInfoData:
				case CusAddInfoSchema.Constants.B7_NAddInfoData:
					return false;
				default:
					return true;
			}
		}

		ChildEntityMatchingInfo GetCusUSClassificationMatchingInfo(IEntityDefinition cusUSClassificationDefinition)
		{
			return new ChildEntityMatchingInfo(FindRow, cusUSClassificationDefinition, (entity) => XmlConstants.EntityNames.CusUSClassification,
				Array.Empty<IPropertyDef>(),
				(pivotRow) =>
				{
					var query = new ZQuery(CusUSClassificationSchema.CD_ParentID, pivotRow[CusClassPartPivotSchema.Constants.PK]);
					query.AddToFilter(CusUSClassificationSchema.CD_ParentTableCode, CusClassPartPivotSchema.Constants.Prefix);
					return query;
				});
		}

		ChildEntityMatchingInfo GetCusAttributeFilterMatchingInfo(IEntityDefinition cusAttributeFilterDefinition)
		{
			return new ChildEntityMatchingInfo(FindRow, cusAttributeFilterDefinition,
				(entity) =>
				{
					var attributeName = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.AttributeName);
					return $"CusAttributeFilter(Name='{attributeName}')";
				},
				GetColumnsToCompare(cusAttributeFilterDefinition, IsValidForCusAttributeFilterMatch),
				(pivotRow) => new ZQuery(CusAttributeFilterSchema.BG_CI, pivotRow[CusClassPartPivotSchema.Constants.PK]));
		}

		bool IsValidForCusAttributeFilterMatch(string columnName)
		{
			switch (columnName)
			{
				case CusAttributeFilterSchema.Constants.PK:
					return false;
				default:
					return true;
			}
		}

		ChildEntityMatchingInfo GetCusCodeDataMatchingInfo(IEntityDefinition cusCodeDataDefinition, ITableSchema parenTableSchema)
		{
			return new ChildEntityMatchingInfo(FindRow, cusCodeDataDefinition, (entity) =>
			{
				var type = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.Type);
				var code = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.Code);
				var order = entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.Order);
				return $"{entity.EntityName}(Type='{type}', Code='{code}', Order='{order}')";
			},
				GetColumnsToCompare(cusCodeDataDefinition, IsValidForCusCodeDataMatch),
				(pivotRow) =>
				{
					var queryForCusCodeData = new ZQuery(CusCodeDataSchema.CY_ParentID, pivotRow[parenTableSchema.PK.Name]);
					queryForCusCodeData.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, TableNameHelper.GetPrefixFromTableName(parenTableSchema.TableName));
					return queryForCusCodeData;
				});
		}

		ChildEntityMatchingInfo GetCusCALPCOMatchingInfo(IEntityDefinition lpcoDefinition, ITableSchema parenTableSchema)
		{
			return new ChildEntityMatchingInfo(FindRow, lpcoDefinition, (entity) => XmlConstants.EntityNames.CusCALPCO,
				Array.Empty<IPropertyDef>(),
				(pivotRow) =>
				{
					var query = new ZQuery(CusCALPCOSchema.CLP_ParentID, pivotRow[parenTableSchema.PK.Name]);
					query.AddToFilter(CusCALPCOSchema.CLP_ParentTableCode, TableNameHelper.GetPrefixFromTableName(parenTableSchema.TableName));
					return query;
				});
		}

		bool IsValidForCusCodeDataMatch(string columnName)
		{
			switch (columnName)
			{
				case CusCodeDataSchema.Constants.PK:
					return false;
				default:
					return true;
			}
		}

		class AttributeNames
		{
			public const string AT1 = "AT1";
			public const string AT2 = "AT2";
			public const string AT3 = "AT3";
		}

		void DeleteAllExistingComponentPivotsAndTheirChildrenOnFoundParentPivotIfPivotFound(IEntity xmlProduct, OrgSupplierPart existingPart, IEntity[] parentPivotsInXml, IEntityDefinition cusClassPartPivotDefinition)
		{
			if (parentPivotsInXml.Length > 0)
			{
				var cusClassPartPivotMatchingInfo = GetCusClassPartPivotMatchingInfo(cusClassPartPivotDefinition, existingPart.PK.ToGuid(), isParentPivot: true);
				var rowFactory = new RowFactory(Db.Connection, null);
				var partRow = ((INeedRow)existingPart).Row;
				var partNum = existingPart.OP_PartNum;
				foreach (var parentXmlPivot in parentPivotsInXml)
				{
					(var foundDbParentPivot, var parentPivotIdentificationMessage) = FindDataRow(rowFactory, cusClassPartPivotMatchingInfo, parentXmlPivot, partRow, partNum, treatAbsentXmlElementAsEmpty: false);
					if (foundDbParentPivot != null)
					{
						DeleteChildrenByInsertingActionDeleteNodesIntoXml(rowFactory, foundDbParentPivot, parentPivotIdentificationMessage, parentXmlPivot, cusClassPartPivotMatchingInfo);
					}
				}
			}
		}

		void DeleteChildrenByInsertingActionDeleteNodesIntoXml(RowFactory rowFactory, DataRow parentRow, string parentPivotIdentificationMessage, IEntity parentXmlPivot, ChildEntityMatchingInfo childEntityMatchingInfo)
		{
			var childrenDictionary = parentXmlPivot.Children.ToKeyListDictionary(x => x.Definition);
			foreach (var childMatchingInfo in childEntityMatchingInfo.ChildrenEntityMatchingInfos)
			{
				if (!childrenDictionary.TryGetValue(childMatchingInfo.ChildEntityDefinition, out var childXmlList))
				{
					if (childMatchingInfo.DeleteOnlyIfExistsInXml)
					{
						continue;
					}

					childXmlList = new List<IEntity>();
				}

				var grandChildMatchingInfos = childMatchingInfo.ChildrenEntityMatchingInfos.ToArray();
				var needToProcessMatched = grandChildMatchingInfos.Length > 0;
				(var nonMatchedChildrenData, var matchedChildrenData) = SplitMatchedDataRows(parentRow, parentPivotIdentificationMessage, childXmlList, rowFactory, childMatchingInfo, needToProcessMatched);
				if (nonMatchedChildrenData.Length > 0)
				{
					DeleteChildrenByInsertingActionDeleteNodesIntoXml(rowFactory, childMatchingInfo, parentXmlPivot, nonMatchedChildrenData);
				}

				if (needToProcessMatched)
				{
					foreach (var matchedChildData in matchedChildrenData)
					{
						var matchedChildEntity = matchedChildData.Value;
						var matchedChildEntityIdentificationMessage = parentPivotIdentificationMessage + " - " + childMatchingInfo.GetIdentificationMessage(matchedChildEntity);
						DeleteChildrenByInsertingActionDeleteNodesIntoXml(rowFactory, matchedChildData.Key, matchedChildEntityIdentificationMessage, matchedChildEntity, childMatchingInfo);
					}
				}
			}
		}

		(DataRow[] nonMatchedBizObjs, IEnumerable<KeyValuePair<DataRow, IEntity>> matchedBizObjsData) SplitMatchedDataRows(DataRow parentRow, string parentIdentificationMessage, IEnumerable<IEntity> entitiesInXml, RowFactory rowFactory, ChildEntityMatchingInfo childEntityMatchingInfo, bool needToProcessMatched)
		{
			var removedData = new Dictionary<DataRow, IEntity>();
			var tableSchema = childEntityMatchingInfo.TableSchema;
			var result = rowFactory.Load(tableSchema.TableName, childEntityMatchingInfo.GetParentQuery(parentRow)).ToList();
			if (result.Count > 0)
			{
				foreach (var entityInXml in entitiesInXml)
				{
					(var bizObj, var identificationMessage) = FindDataRow(rowFactory, childEntityMatchingInfo, entityInXml, parentRow, parentIdentificationMessage, treatAbsentXmlElementAsEmpty: true);
					if (bizObj != null)
					{
						if (entityInXml.InternalPK == Guid.Empty)
						{
							entityInXml.InternalPK = (Guid)bizObj[tableSchema.PK.Name];
						}

						if (result.Remove(bizObj) && needToProcessMatched)
						{
							removedData.Add(bizObj, entityInXml);
						}
					}
				}
			}
			return (result.ToArray() ?? Array.Empty<DataRow>(), removedData);
		}

		(DataRow row, string identificationMessage) FindDataRow(RowFactory rowFactory, ChildEntityMatchingInfo childEntityMatchingInfo, IEntity entity, DataRow parentRow, string identificationMessage, bool treatAbsentXmlElementAsEmpty)
		{
			// NB we cannot use the native engine here to find the pivot, because it caches the row we find.  It then would later
			// treat the cached DB row as if it were the XML row, pfff. This is true even if we use a new repository, new factory, new context... lame.
			// So use good ole' fashioned ZQuery.
			// ..... but make sure we consider many fields in the DB, not just (a) those given in XML and/or (b) those columns that are foreign keys and so appear in the pivot's Children or Parents collections (instead of in its Properties collection)
			DataRow result = null;
			var additionalIdentificationMessage = childEntityMatchingInfo.GetIdentificationMessage(entity);
			if (!string.IsNullOrEmpty(additionalIdentificationMessage))
			{
				identificationMessage += " - " + additionalIdentificationMessage;
			}

			var tableSchema = childEntityMatchingInfo.TableSchema;
			var tableName = tableSchema.TableName;
			if (entity.InternalPK != Guid.Empty)
			{
				result = rowFactory.LoadFromPK(tableName, entity.InternalPK);
			}
			if (result == null)
			{
				var query = childEntityMatchingInfo.GetParentQuery(parentRow);
				var additionalQuery = childEntityMatchingInfo.GetAdditionalQuery(entity, identificationMessage, treatAbsentXmlElementAsEmpty, Fail);
				if (!additionalQuery.IsEmpty)
				{
					query.AddToFilter(additionalQuery);
				}
				var rowsFromDb = childEntityMatchingInfo.AdditionalMatching(rowFactory, entity, rowFactory.Load(tableName, query));
				if (rowsFromDb.Length == 1)
				{
					result = rowsFromDb[0];
				}
				if (result != null)
				{
					entity.InternalPK = (Guid)result[childEntityMatchingInfo.TableSchema.PK.Name];
				}
			}
			return (result, identificationMessage);
		}

		void AddCountryIfMissingOnComponentPivot(IEntity componentPivot, ZString defaultCountry)
		{
			var countryCode = string.Empty;
			var country = componentPivot.GetParentEntity(XmlConstants.EntityNames.Country);
			if (country == null)
			{
				var countryDefintiion = componentPivot.Definition.Parents.First(e => e.EntityName == XmlConstants.EntityNames.Country);
				country = new Entity(countryDefintiion, sessionServices);
				country[XmlConstants.PropertyNames.Code] = defaultCountry;
				((Entity)componentPivot).ParentCollection.Add(country);
			}
			else
			{
				countryCode = country.GetPropertyOrBlankString(XmlConstants.PropertyNames.Code);
				var dataRow = FindRow(country);
				if (dataRow != null)
				{
					var dataRowCountry = (string)dataRow[RefCountrySchema.Constants.RN_Code];
					if (countryCode != dataRowCountry)
					{
						countryCode = dataRowCountry;
						country[XmlConstants.PropertyNames.Code] = countryCode;
					}
				}
			}

			if (string.IsNullOrEmpty(countryCode))
			{
				country[XmlConstants.PropertyNames.Code] = defaultCountry;
			}
			else if (countryCode != defaultCountry)
			{
				Fail($"Component Pivot's Country ({countryCode}) must match Pivot's Country ({defaultCountry})");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfDuplicateBarcode(ZString productNumberFromXml, IEnumerable<ZGuid> owners, IEntity xmlProduct)
		{
			var pkOfSupposedExistingRowInXml = (ZGuid)xmlProduct.InternalPK;
			var xmlBarcodes = xmlProduct.Children.Where(e => e.EntityName == "OrgSupplierPartBarcode").ToArray();
			if (xmlBarcodes.Length > 0)
			{
				var barcodesToImport = xmlBarcodes.Where(b => b.Properties.Any(p => p.Name == "Barcode")).ToArray().Select(p => new ZString(p["Barcode"])).Distinct();
				if (barcodesToImport.Contains(productNumberFromXml))
				{
					Fail(string.Format(CultureInfo.InvariantCulture, "Barcode cannot be the same as the Product Code '{0}'.", productNumberFromXml));
				}
				else
				{
					var barcodesAndProductCode = barcodesToImport.Union(new[] { productNumberFromXml });
					var productsWithSameBarcodeOrPartNum = OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(factory, pkOfSupposedExistingRowInXml, owners, barcodesAndProductCode);
					if (productsWithSameBarcodeOrPartNum.Any())
					{
						Fail(string.Format(CultureInfo.InvariantCulture, "The barcode(s) or Product Code '{0}' has already been used on Product(s) '{1}' by the same owner.", string.Join(",", barcodesAndProductCode), string.Join(",", productsWithSameBarcodeOrPartNum)));
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidPartBarcode(IEntity xmlProduct)
		{
			var xmlBarcodes = xmlProduct.Children.Where(e => e.EntityName == "OrgSupplierPartBarcode").ToArray();
			foreach (var xmlbarcode in xmlBarcodes.Where(barcode => IsUpdatingBarcodeAction(barcode.Action)))
			{
				var barcodeHasValue = xmlbarcode.Properties.Any(p => (p.Name == "Barcode") && !((string)p.Value).Trim().IsNullOrEmpty());
				var packTypeEntity = xmlbarcode.Parents.FirstOrDefault(p => p.EntityName == "PackType");
				var packTypeHasValue = packTypeEntity != null && packTypeEntity.Properties.Any(p => (p.Name == XmlConstants.PropertyNames.Code) && !((string)p.Value).Trim().IsNullOrEmpty());
				if (!barcodeHasValue || !packTypeHasValue)
				{
					Fail("This Product has an OrgSupplierPartBarcode with an empty Barcode and/or PackType.");
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidOrgPartRelationAttributeConfig(OrgSupplierPart part, IEntity[] xmlRelationships)
		{
			foreach (var xmlRelation in xmlRelationships.Where(r => r.Action != EntityAction.DELETE))
			{
				(var organisation, var org) = productEntityMatchingOrgCache.GetOrganisationFromEntityParent(xmlRelation);
				if (organisation != null)
				{
					FailIfRelationshipDoesNotMatchClientSetup(org, xmlRelation);

					var relationship = part?.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation.InternalPK, xmlRelation.GetPropertyOrBlankString("Relationship"));
					FailIfIsReleaseCapturedIsNotConsistentWithPartAttributeUsage(org, relationship, xmlRelation);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidConsigneeMinShelfLifeAcceptedInPartRelation(OrgSupplierPart part, IEntity xmlProduct, IEntity[] xmlRelationships)
		{
			foreach (var xmlRelation in xmlRelationships.Where(r => r.Action != EntityAction.DELETE))
			{
				ZShort consigneeMinShelfLifeAccepted = 0;
				var consigneeMinShelfLifeAcceptedString = xmlRelation.GetPropertyOrBlankString("ConsigneeMinShelfLifeAccepted");
				if (!consigneeMinShelfLifeAcceptedString.IsNullOrEmpty() && (!ZShort.TryParse(consigneeMinShelfLifeAcceptedString, out consigneeMinShelfLifeAccepted) || (consigneeMinShelfLifeAccepted < 0)))
				{
					Fail(string.Format(CultureInfo.InvariantCulture, ("Consignee Minimum Shelf Life Accepted '{0}' is not valid."), consigneeMinShelfLifeAcceptedString));
				}

				var relationshipType = xmlRelation.GetPropertyOrBlankString("Relationship");
				var isOwner = relationshipType == OrgPartRelation.RelationshipTypes.Both || relationshipType == OrgPartRelation.RelationshipTypes.Owner;
				if (isOwner || relationshipType == OrgPartRelation.RelationshipTypes.WarehouseConsignee)
				{
					var (organisation, org) = productEntityMatchingOrgCache.GetOrganisationFromEntityParent(xmlRelation);
					if (!(organisation is null))
					{
						var matchedProductParams = isOwner ? ProductParamsByWhsAndClientHelper.GetBestMatchProductParamsByOrganisation(xmlProduct, organisation) : null;
						var maximumShelfLifeStr = matchedProductParams?.GetPropertyOrBlankString("MaximumShelfLife");
						if (!(matchedProductParams is null) && matchedProductParams.Action != EntityAction.DELETE
							&& (ZShort.TryParse(maximumShelfLifeStr, out var maximumShelfLife) && maximumShelfLife > 0 && consigneeMinShelfLifeAccepted > maximumShelfLife))
						{
							Fail(OrgPartRelationValidationHelper.GetConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLifeErrorMessage(consigneeMinShelfLifeAccepted, maximumShelfLife));
						}

						if (!(part is null) && (xmlRelation.Action == EntityAction.MERGE || xmlRelation.Action == EntityAction.UPDATE))
						{
							var relationship = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation.InternalPK, xmlRelation.GetPropertyOrBlankString("Relationship"));
							if (!(relationship is null))
							{
								if (maximumShelfLifeStr.IsNullOrEmpty())
								{
									var errorMsgForCannotGreaterThanMaximumShelfLife = OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(relationship, consigneeMinShelfLifeAccepted);
									if (!errorMsgForCannotGreaterThanMaximumShelfLife.IsEmpty)
									{
										Fail(errorMsgForCannotGreaterThanMaximumShelfLife);
									}
								}

								var errorMsgForCannotBeIncreasedIfPickIsInProgress = OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(relationship, consigneeMinShelfLifeAccepted);
								if (!errorMsgForCannotBeIncreasedIfPickIsInProgress.IsEmpty)
								{
									Fail(errorMsgForCannotBeIncreasedIfPickIsInProgress);
								}
							}
						}
					}
				}
			}
		}

		void FailIfInvalidUnitConversionPackTypes(IEntity xmlProduct)
		{
			OrgPartUnitValidationHelper.ValidatePackTypes(
				factory,
				xmlProduct,
				p => p.ChildrenCollection.Where(e => e.EntityName == XmlConstants.EntityNames.OrgPartUnit),
				c => c.GetPropertyOrBlankString(XmlConstants.PropertyNames.PackType),
				c => c.GetPropertyOrBlankString(XmlConstants.PropertyNames.ParentPackType),
				(p, message) => Fail(message));
		}

		static bool IsUpdatingBarcodeAction(EntityAction action) =>
			action == EntityAction.INSERT
			|| action == EntityAction.MERGE
			|| action == EntityAction.UPDATE;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfDuplicateParentPivots(IEntity[] pivotsToMerge, IPropertyDef[] parentPivotColumnsToCompareHonouringExclusions)
		{
			// The engine is set to exclude CI_TariffNum when looking for existing pivots to update.  If you have an OP file with two parent pivots (no component pivots)
			// which differ only by CI_Tariff, the engine will insert one row and then re-use it for the second node.  You'll get one insert and one update.
			// I think that the exclusion of CI_Tariff is the culprit and that we should again consider this field in lookups (i.e. remove it from the list of excluded fields),
			// but if we keep it excuded then the only way to proceed is to bork when we see such files.

			var totalMergePivotsCount = pivotsToMerge.Length;
			var numberOfUniqueMergePivots = SelectUniquePivotsHonouringExcludedColumns(pivotsToMerge, parentPivotColumnsToCompareHonouringExclusions).Count();
			if (numberOfUniqueMergePivots < totalMergePivotsCount && totalMergePivotsCount > 0)
			{
				Fail("This Part has duplicate pivots when comparing using a limited number of fields. Pivots should be action INSERT or should be sufficiently different.");
			}
		}

		void FailIfHITPivotsAttributesInvalid(IEntity[] htiPivots)
		{
			var pivotGroupsByCountry = htiPivots.GroupBy(x => x.GetParentEntity(XmlConstants.EntityNames.Country)[XmlConstants.PropertyNames.Code]);
			foreach (var pivotGroupByCountry in pivotGroupsByCountry)
			{
				var country = pivotGroupByCountry.Key;
				var pivotGroups = pivotGroupByCountry.GroupBy(x => x.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgHeader)?.GetPropertyOrBlankString(XmlConstants.PropertyNames.Code) ?? string.Empty);
				foreach (var group in pivotGroups)
				{
					if (group.Any())
					{
						var orgCode = group.Key;
						var uniqueEntities = new List<IEntity>();
						var index = 0;
						var isAT1Specified = false;
						var isAT2Specified = false;
						var isAT3Specified = false;
						foreach (var pivot in group)
						{
							(var attrs1, var attrs2, var attrs3) = GetPivotAttributes(pivot, allowDuplicate: true);
							if (MatchingHelper.IsContainingEmptyOrDuplicatedAttributes(attrs1) ||
								MatchingHelper.IsContainingEmptyOrDuplicatedAttributes(attrs2) ||
								MatchingHelper.IsContainingEmptyOrDuplicatedAttributes(attrs3))
							{
								Fail($"This Part has invalid HTI (Country='{country}', Party='{orgCode}') attributes. Some attribute values are empty or duplicated on a single pivot.");
								break;
							}
							if (index == 0)
							{
								isAT1Specified = attrs1.Any();
								isAT2Specified = attrs2.Any();
								isAT3Specified = attrs3.Any();
							}
							else if (isAT1Specified != attrs1.Any() || isAT2Specified != attrs2.Any() || isAT3Specified != attrs3.Any())
							{
								Fail($"This Part has invalid HTI (Country='{country}', Party='{orgCode}') attributes. Attributes have been specified on other pivots and therefore must be specified on all pivots.");
								break;
							}
							if (uniqueEntities.Contains(pivot, new HTIAttributesComparer((x) => GetPivotAttributes(x, allowDuplicate: false))))
							{
								Fail($"This Part has invalid HTI (Country='{country}', Party='{orgCode}') attributes. Attribute type cannot be duplicated between pivots, type must be unique across pivots.");
								break;
							}
							else
							{
								uniqueEntities.Add(pivot);
							}
							index++;
						}
					}
				}
			}
		}

		IEnumerable<IEntity> SelectUniquePivotsHonouringExcludedColumns(IEntity[] mergePivots, IPropertyDef[] propertyDefsToCompare)
		{
			var uniqueValues = new List<IEntity>();
			foreach (var e in mergePivots)
			{
				if (!uniqueValues.Contains(e, new EntityComparer(propertyDefsToCompare, (x) => GetPivotAttributes(x, allowDuplicate: false))))
				{
					uniqueValues.Add(e);
				}
			}
			return uniqueValues.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfDuplicateChildSequencesInComponentTariff(IEntity componentPivot, string parentTariff, List<string> listOfFailures, List<string> sequencePairs)
		{
			// Child sequence must be unique among brother component tariffs under on parent pivot.
			// Multiple parents on one Part each must have unique children, but cousins can have the same sequence.
			if (componentPivot.Action != EntityAction.DELETE)
			{
				var childSeq = componentPivot.GetPropertyOrBlankString(XmlConstants.PropertyNames.ChildListOrder);
				var componentTariff = componentPivot.GetPropertyOrBlankString(XmlConstants.PropertyNames.TariffNum);
				if (!sequencePairs.Contains(childSeq))
				{
					sequencePairs.Add(childSeq);
				}
				else
				{
					listOfFailures.Add(string.Format(@"Tariff {0}	Component {1}	Sequence {2}", parentTariff, componentTariff, childSeq));
				}
			}
		}

		class EntityComparer : IEqualityComparer<IEntity>
		{
			public EntityComparer(IPropertyDef[] propertyDefsToCompare, Func<IEntity, (string[] attribute1, string[] attribute2, string[] attribute3)> getPivotAttributes)
			{
				this.propertyDefsToCompare = propertyDefsToCompare;
				this.getPivotAttributes = getPivotAttributes;
			}
			readonly IPropertyDef[] propertyDefsToCompare;
			readonly Func<IEntity, (string[] attribute1, string[] attribute2, string[] attribute3)> getPivotAttributes;

			public bool Equals(IEntity x, IEntity y)
			{
				Argument.NotNull(x, nameof(x));
				Argument.NotNull(y, nameof(y));
				var allEqualSoFar = true;
				foreach (var propertyDefToCompare in propertyDefsToCompare)
				{
					if (!MatchingHelper.ArePropertiesEqual(x, y, propertyDefToCompare.PropertyName))
					{
						allEqualSoFar = false;
						break;
					}
				}
				if (allEqualSoFar)
				{
					var countryX = x.GetParentEntity(XmlConstants.EntityNames.Country);
					var countryY = y.GetParentEntity(XmlConstants.EntityNames.Country);
					allEqualSoFar = MatchingHelper.ArePropertiesEqual(countryX, countryY, XmlConstants.PropertyNames.Code);
				}
				if (allEqualSoFar)
				{
					var relX = x.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgPartRelation);
					var relY = y.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgPartRelation);
					allEqualSoFar = MatchingHelper.ArePropertiesEqual(relX, relY, XmlConstants.PropertyNames.Relationship, () =>
					{
						var orgX = relX.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgHeader);
						var orgY = relY.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgHeader);
						return MatchingHelper.ArePropertiesEqual(orgX, orgY, XmlConstants.PropertyNames.Code) && MatchingHelper.ArePropertiesEqual(orgX, orgY, XmlConstants.PropertyNames.PK);
					});
				}

				if (allEqualSoFar && MatchingHelper.IsChildTypeHTI(x) && MatchingHelper.IsChildTypeHTI(y))
				{
					var orgX = x.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgHeader);
					var orgY = y.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.EntityNames.OrgHeader);
					if (!MatchingHelper.ArePropertiesEqual(orgX, orgY, XmlConstants.PropertyNames.Code) || !MatchingHelper.ArePropertiesEqual(orgX, orgY, XmlConstants.PropertyNames.PK))
					{
						allEqualSoFar = false;
					}
					else
					{
						var xData = getPivotAttributes(x);
						var yData = getPivotAttributes(y);
						if (!MatchingHelper.AreAttributesEqual(xData.attribute1, yData.attribute1) ||
							!MatchingHelper.AreAttributesEqual(xData.attribute2, yData.attribute2) ||
							!MatchingHelper.AreAttributesEqual(xData.attribute3, yData.attribute3))
						{
							allEqualSoFar = false;
						}
					}
				}

				return allEqualSoFar;
			}

			public int GetHashCode(IEntity obj)
			{
				return obj.GetHashCode();
			}
		}

		class HTIAttributesComparer : IEqualityComparer<IEntity>
		{
			public HTIAttributesComparer(Func<IEntity, (string[] attribute1, string[] attribute2, string[] attribute3)> getPivotAttributes)
			{
				this.getPivotAttributes = getPivotAttributes;
			}
			readonly Func<IEntity, (string[] attribute1, string[] attribute2, string[] attribute3)> getPivotAttributes;

			public bool Equals(IEntity x, IEntity y)
			{
				(var attrs1X, var attrs2X, var attrs3X) = getPivotAttributes(x);
				(var attrs1Y, var attrs2Y, var attrs3Y) = getPivotAttributes(y);

				return !(attrs1X.IsCountEqualTo(0) &&
					attrs1Y.IsCountEqualTo(0) &&
					attrs2X.IsCountEqualTo(0) &&
					attrs2Y.IsCountEqualTo(0) &&
					attrs3X.IsCountEqualTo(0) &&
					attrs3Y.IsCountEqualTo(0)) &&
					(HavingSameAttributesOrBothHaveNoAttributes(x, y, AttributeNames.AT1) &&
					HavingSameAttributesOrBothHaveNoAttributes(x, y, AttributeNames.AT2) &&
					HavingSameAttributesOrBothHaveNoAttributes(x, y, AttributeNames.AT3));
			}

			public int GetHashCode(IEntity obj)
			{
				return obj.GetHashCode();
			}

			bool HavingSameAttributesOrBothHaveNoAttributes(IEntity x, IEntity y, string attributeName)
			{
				var attrsX = MatchingHelper.GetAttributes(x, attributeName);
				var attrsY = MatchingHelper.GetAttributes(y, attributeName);
				return (attrsX.IsCountEqualTo(0) && attrsY.IsCountEqualTo(0)) || HavingSameAttributes(attrsX, attrsY);
			}

			bool HavingSameAttributes(IEnumerable<string> attrsX, IEnumerable<string> attrsY) =>
				attrsX.IsCountMoreThan(0) && attrsY.IsCountMoreThan(0) && attrsX.Intersect(attrsY).IsCountMoreThan(0);
		}

		class MatchingHelper
		{
			public static bool IsChildTypeHTI(IEntity entity) => entity.GetPropertyOrBlankString(XmlConstants.PropertyNames.ChildType) == "HTI";

			public static bool ArePropertiesEqual(IEntity x, IEntity y, ZString propertyName, Func<bool> additionalComparison = null)
			{
				var result = true;
				if ((x != null && y == null && !x.GetPropertyOrBlankString(propertyName).IsNullOrEmpty()) || (x == null && y != null && !y.GetPropertyOrBlankString(propertyName).IsNullOrEmpty()))
				{
					result = false;
				}
				else if (x != null && y != null)
				{
					if (x.GetPropertyOrBlankString(propertyName) != y.GetPropertyOrBlankString(propertyName))
					{
						result = false;
					}
					else if (additionalComparison != null)
					{
						result = additionalComparison();
					}
				}
				return result;
			}

			public static bool AreAttributesEqual(string[] attributesX, string[] attributesY)
			{
				return new HashSet<string>(attributesX).SetEquals(attributesY);
			}

			public static IEnumerable<string> GetAttributes(IEntity obj, ZString attributeName) => obj.Children
				.Where(elem => elem.GetPropertyOrBlankString(XmlConstants.PropertyNames.AttributeName) == attributeName)
				.Select(elem => elem.GetPropertyOrBlankString(XmlConstants.PropertyNames.AttributeValue1));

			public static bool IsContainingEmptyOrDuplicatedAttributes(IEnumerable<string> attrs) =>
				!(new HashSet<string>(attrs.Where(x => { return !x.Trim().IsNullOrEmpty(); }).Distinct()).SequenceEqual(attrs));
		}
	}
}
