using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching
{
	class ProductComponentMatchingHelper
	{
		public ProductComponentMatchingHelper(BusinessObjectFactory factory, IEntity xmlProduct, ProductEntityMatchingOrgCache productEntityMatchingOrgCache, OrgSupplierPart partLoadedFromEnterprise)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			XmlProduct = Argument.NotNull(xmlProduct, nameof(xmlProduct));
			ProductEntityMatchingOrgCache = Argument.NotNull(productEntityMatchingOrgCache, nameof(productEntityMatchingOrgCache));
			XmlOrganisationalRelationships = xmlProduct.Children.Where(e => e.EntityName == "OrgPartRelation").ToArray();
			PartLoadedFromEnterprise = partLoadedFromEnterprise;
			OrgPartBOMModelsCache = new Lazy<Dictionary<string, OrgPartBOMModel>>(() => LoadExistingOrgPartBOMModels(PartLoadedFromEnterprise));
		}

		BusinessObjectFactory Factory { get; }
		IEntity XmlProduct { get; }
		IEntity[] XmlOrganisationalRelationships { get; }
		OrgSupplierPart PartLoadedFromEnterprise { get; }
		ProductEntityMatchingOrgCache ProductEntityMatchingOrgCache { get; }
		Lazy<Dictionary<string, OrgPartBOMModel>> OrgPartBOMModelsCache { get; }

		#region LoadExistingOrgPartBOMModels

		Dictionary<string, OrgPartBOMModel> LoadExistingOrgPartBOMModels(OrgSupplierPart partLoadedFromEnterprise)
		{
			var orgPartBomModels = new Dictionary<string, OrgPartBOMModel>();
			if (partLoadedFromEnterprise != null)
			{
				var existingBoms = Factory.Load<OrgPartBOM>(new ZQuery(OrgPartBOMSchema.OE_OP_MainProduct, partLoadedFromEnterprise.PK));
				existingBoms.ForEach(bom =>
				{
					var model = new OrgPartBOMModel(bom);
					orgPartBomModels[model.ToKey()] = model;
				});
			}
			return orgPartBomModels;
		}

		#endregion

		#region FailIfInvalidOrgPartOrSecondaryPartBOMs

		public void FailIfInvalidOrgPartOrSecondaryPartBOMs()
		{
			FailIfInvalidOrgPartBOMs();
			FailIfInvalidOrgSecondaryPartBOMs();
		}

		#endregion

		#region FailIfInvalidOrgPartBOMs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidOrgPartBOMs()
		{
			var orgPartBOMCollection = XmlProduct.Children.Where(e => e.EntityName == XmlConstants.EntityNames.OrgPartBOM);

			var isBOMProductPickedOnSalesOrder = false;
			if (PartLoadedFromEnterprise != null)
			{
				isBOMProductPickedOnSalesOrder = OrgPartBOMValidation.IsKitBuiltOnSalesOrder(Factory, PartLoadedFromEnterprise.PK);
			}

			foreach (var orgPartBOMEntity in orgPartBOMCollection)
			{
				OrgPartBOM orgPartBOMLoadedFromEnterprise = null;
				if (orgPartBOMEntity.InternalPK != Guid.Empty)
				{
					orgPartBOMLoadedFromEnterprise = Factory.Load<OrgPartBOM>(orgPartBOMEntity.InternalPK);
					if (orgPartBOMLoadedFromEnterprise == null)
					{
						Fail($"Cannot finding matching OrgPartBOM with PK '{orgPartBOMEntity.InternalPK}'.");
					}

					UpdateModelDictionary(orgPartBOMEntity, orgPartBOMLoadedFromEnterprise.Component, orgPartBOMLoadedFromEnterprise.PackType);
				}

				var componentEntity = orgPartBOMEntity.Parents.FirstOrDefault(e => e.EntityName == XmlConstants.EntityNames.Component);
				if (componentEntity != null)
				{
					var component = MatchAndValidateOrgSupplierPartWithCommonOwner(componentEntity);
					var refPackType = MatchAndValidateRefPackType(orgPartBOMEntity);

					if (isBOMProductPickedOnSalesOrder)
					{
						FailIfCriticalOrgPartBOMFieldsModifiedWhenPickedOnSalesOrder(orgPartBOMEntity, orgPartBOMLoadedFromEnterprise, component, refPackType);
					}

					if (orgPartBOMEntity.Action == EntityAction.INSERT && PartLoadedFromEnterprise != null)
					{
						FailIfInvalidInsertionOfBOMProduct(PartLoadedFromEnterprise, component);
					}

					if (orgPartBOMEntity.InternalPK == Guid.Empty)
					{
						orgPartBOMEntity.InternalPK = Guid.NewGuid(); // To allow matching with OrgSecondaryPartBOMPivot
					}

					UpdateModelDictionary(orgPartBOMEntity, component, refPackType);
				}
				else if (orgPartBOMEntity.InternalPK == Guid.Empty)
				{
					Fail("Cannot match OrgPartBOM without PK or Component definition.");
				}
			}

			void UpdateModelDictionary(IEntity orgPartBOMEntity, OrgSupplierPart component, RefPackType packType)
			{
				var qtyXML = orgPartBOMEntity.GetPropertyOrBlankString("ComponentQty");
				var componentQty = 0m;
				decimal.TryParse(qtyXML, out componentQty);

				var orgPartBomModel = new OrgPartBOMModel(orgPartBOMEntity.InternalPK, component, packType, componentQty);
				if (orgPartBOMEntity.Action != EntityAction.DELETE)
				{
					OrgPartBOMModelsCache.Value[orgPartBomModel.ToKey()] = orgPartBomModel;
				}
				else
				{
					OrgPartBOMModelsCache.Value.Remove(orgPartBomModel.ToKey());
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		OrgSupplierPart MatchAndValidateOrgSupplierPartWithCommonOwner(IEntity orgSupplierPartEntity)
		{
			OrgSupplierPart matchedPart;

			var partPK = orgSupplierPartEntity.InternalPK;
			if (partPK == Guid.Empty)
			{
				var partNum = orgSupplierPartEntity.GetPropertyOrBlankString("PartNum");
				if (string.IsNullOrWhiteSpace(partNum))
				{
					Fail("Unable to match OrgSupplierPart without PK or Product Partnum/Owner pair defined.");
				}

				var ownerRelationEntity = GetAndValidateComponentAsOwnerRelation(orgSupplierPartEntity);
				(_, var org) = ProductEntityMatchingOrgCache.GetOrganisationFromEntityParent(ownerRelationEntity);
				if (org == null)
				{
					Fail("OrgSupplierPart OrgPartRelation must include a valid OrgHeader.");
				}

				FailIfOwnerIsNotSharedBetweenProductAndComponent(partNum, org);

				var partQueryWithOwner = new ZDBOnlyQuery(typeof(OrgSupplierPart));
				partQueryWithOwner.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNum);

				var relatedOrganisationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				relatedOrganisationSubQuery.AddToFilter(ProductEntityMatchingInterceptor.GetAdditionalRelationshipQuery(org.PK, new[] { OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.Owner }));
				partQueryWithOwner.AddSubQuery(relatedOrganisationSubQuery, JoinCondition.And);

				matchedPart = Factory.LoadTop1<OrgSupplierPart>(partQueryWithOwner);
				if (matchedPart == null)
				{
					Fail($"Cannot import product as OrgSupplierPart {partNum} cannot be matched with existing definitions.");
				}
				orgSupplierPartEntity.InternalPK = matchedPart?.PK.ToGuid() ?? Guid.Empty;
			}
			else
			{
				matchedPart = Factory.Load<OrgSupplierPart>(partPK);
				if (matchedPart == null)
				{
					Fail($"Cannot import product as OrgSupplierPart with PK {partPK} cannot be matched with existing definitions.");
				}
			}

			return matchedPart;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		RefPackType MatchAndValidateRefPackType(IEntity orgPartBOMEntity)
		{
			RefPackType refPackType = null;
			var refPackTypeEntity = orgPartBOMEntity.Parents.FirstOrDefault(p => p.EntityName == XmlConstants.PropertyNames.PackType);
			if (refPackTypeEntity != null)
			{
				if (refPackTypeEntity.InternalPK != Guid.Empty)
				{
					refPackType = Factory.Load<RefPackType>(refPackTypeEntity.InternalPK);
					if (refPackType == null)
					{
						Fail($"Cannot import product as Pack Type with PK {refPackTypeEntity.InternalPK} cannot be matched with existing definitions.");
					}
				}
				else
				{
					var packTypeCode = refPackTypeEntity.GetPropertyOrBlankString(XmlConstants.PropertyNames.Code);
					refPackType = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, packTypeCode));
					if (refPackType == null)
					{
						Fail($"Cannot import product as Pack Type with PackTypeCode {packTypeCode} cannot be matched with existing definitions.");
					}
				}
			}
			else if (orgPartBOMEntity.InternalPK == Guid.Empty)
			{
				Fail("Unable to match OrgPartBom without PK or Component/Pack Type pair.");
			}

			return refPackType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		IEntity GetAndValidateComponentAsOwnerRelation(IEntity component)
		{
			var relationCollection = component.Children.Where(e => e.TableName == XmlConstants.EntityNames.OrgPartRelation);
			var ownerRelationEntity = relationCollection.FirstOrDefault();
			var remainder = relationCollection.Skip(1);
			if (remainder.Any())
			{
				Fail("Cannot match OrgSupplierPart with multiple owner OrgPartRelations defined.");
			}
			else if (ownerRelationEntity == null)
			{
				Fail("Cannot match OrgSupplierPart without owner OrgPartRelation defined.");
			}

			var relationshipType = ownerRelationEntity.GetPropertyOrBlankString("Relationship");
			if (string.IsNullOrEmpty(relationshipType))
			{
				Fail("OrgPartRelation must specify Relationship.");
			}
			else if (relationshipType != OrgPartRelation.RelationshipTypes.Both
				&& relationshipType != OrgPartRelation.RelationshipTypes.Owner)
			{
				Fail("OrgPartRelation must be an Owner.");
			}

			return ownerRelationEntity;
		}

		void FailIfOwnerIsNotSharedBetweenProductAndComponent(string componentPartNum, OrgHeader componentOwner)
		{
			if (PartLoadedFromEnterprise != null)
			{
				var productRelationships = PartLoadedFromEnterprise.RelatedOrganisations;
				var existingRelation = productRelationships.FindByOrganisationPKAndRelationship(componentOwner.PK, OrgPartRelation.RelationshipTypes.Owner);

				if (existingRelation == null
					|| (existingRelation.OU_Relationship != OrgPartRelation.RelationshipTypes.Both
					&& existingRelation.OU_Relationship != OrgPartRelation.RelationshipTypes.Owner))
				{
					Fail($"Cannot import product as Component owner is not an owner of {PartLoadedFromEnterprise.OP_PartNum}.");
				}
			}
			else
			{
				FailIfOwnerIsNotSharedBetweenProductAndComponentDuringInsert(componentPartNum, componentOwner);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfOwnerIsNotSharedBetweenProductAndComponentDuringInsert(string componentPartNum, OrgHeader componentOwner)
		{
			var hasMatchingOwner = false;
			foreach (var relation in XmlOrganisationalRelationships)
			{
				var relationship = relation.GetPropertyOrBlankString("Relationship");
				if (relationship != OrgPartRelation.RelationshipTypes.Both
					&& relationship != OrgPartRelation.RelationshipTypes.Owner)
				{
					continue;
				}

				(_, var org) = ProductEntityMatchingOrgCache.GetOrganisationFromEntityParent(relation);
				if (org.PK == componentOwner.PK)
				{
					hasMatchingOwner = true;
					break;
				}
			}

			if (!hasMatchingOwner)
			{
				Fail($"Cannot import OrgPartBOM as Component {componentPartNum} does not share an owner with Product.");
			}
		}

		void FailIfCriticalOrgPartBOMFieldsModifiedWhenPickedOnSalesOrder(IEntity orgPartBOMEntity, OrgPartBOM orgPartBOMLoadedFromEnterprise, OrgSupplierPart orgPartBOMComponent, RefPackType packType)
		{
			if (orgPartBOMEntity.InternalPK == Guid.Empty
				|| orgPartBOMEntity.Action == EntityAction.INSERT
				|| orgPartBOMEntity.Action == EntityAction.DELETE)
			{
				Fail(OrgPartBOMValidation.PickOnSalesOrderDetected);
			}

			var componentQtyProperty = orgPartBOMEntity.GetPropertyOrBlankString("ComponentQty");
			if (ZDecimal.TryParse(componentQtyProperty, out var componentQty) && componentQty != orgPartBOMLoadedFromEnterprise?.OE_ComponentQty)
			{
				Fail(OrgPartBOMValidation.PickOnSalesOrderDetected);
			}

			if (!string.Equals(packType?.F3_Code, orgPartBOMLoadedFromEnterprise.OE_F3_NKPackType, StringComparison.OrdinalIgnoreCase)
				|| orgPartBOMLoadedFromEnterprise.OE_OP_Component != orgPartBOMComponent.PK)
			{
				Fail(OrgPartBOMValidation.PickOnSalesOrderDetected);
			}
		}

		void FailIfInvalidInsertionOfBOMProduct(OrgSupplierPart partLoadedFromEnterprise, OrgSupplierPart orgPartBOMComponent)
		{
			if (OrgPartBOMValidationHelper.IsProductComponentOnMainProductWithIsPickOnOrder(partLoadedFromEnterprise))
			{
				Fail(OrgPartBOMValidation.ParentHasIsPickOnOrderParent);
			}

			if (partLoadedFromEnterprise.OP_IsComponentPickedOnSalesOrder && orgPartBOMComponent.BillOfMaterials.Count > 0)
			{
				Fail(OrgPartBOMValidation.CannotAddThisProductAsComponent);
			}
		}

		#endregion

		#region FailIfInvalidOrgSecondaryPartBOMs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidOrgSecondaryPartBOMs()
		{
			var orgSecondaryPartBOMCollection = XmlProduct.Children.Where(e => e.EntityName == XmlConstants.EntityNames.OrgSecondaryPartBOM);

			var mainProductPartNum = PartLoadedFromEnterprise?.OP_PartNum.ToString() ?? XmlProduct.GetPropertyOrBlankString("PartNum");
			var mainProductHasBOM = OrgPartBOMModelsCache.Value.Count > 0;
			var mainProductSetToPickWithWorkOrder = IsMainProductSetToPickWithWorkOrder();

			var includedSecondaryParts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (PartLoadedFromEnterprise != null)
			{
				includedSecondaryParts.UnionWith(LoadExistingSecondaryPartsFromDB());
			}

			foreach (var orgSecondaryPartBOMEntity in orgSecondaryPartBOMCollection)
			{
				var secondaryPart = MatchAndValidateSecondaryPartDefinition(orgSecondaryPartBOMEntity, mainProductPartNum, includedSecondaryParts);

				if (orgSecondaryPartBOMEntity.Action != EntityAction.DELETE)
				{
					if (!mainProductHasBOM)
					{
						Fail(OrgSecondaryPartBOMValidation.MainProductDoesNotHaveBillOfMaterials);
					}
					else if (mainProductSetToPickWithWorkOrder)
					{
						Fail(OrgSecondaryPartBOMValidation.MainProductSetToPickWithWorkOrder);
					}
					else if (secondaryPart != null)
					{
						includedSecondaryParts.Add(secondaryPart.OP_PartNum);
					}
				}

				var orgSecondaryPartBOMLoadedFromEnterprise =
					orgSecondaryPartBOMEntity.InternalPK != Guid.Empty
						? Factory.Load<OrgSecondaryPartBOM>(orgSecondaryPartBOMEntity.InternalPK)
						: null;

				var orgSecondaryPartBOMPivotEntities = orgSecondaryPartBOMEntity.Children.Where(e => e.EntityName == XmlConstants.EntityNames.OrgSecondaryPartBOMPivot);
				MatchAndValidateOrgSecondaryPartBOMPivotsDefinition(orgSecondaryPartBOMPivotEntities, orgSecondaryPartBOMLoadedFromEnterprise);
			}

			foreach (var model in OrgPartBOMModelsCache.Value.Values)
			{
				var totalStockQty = model.TotalComponentStockQuantity;
				if (totalStockQty > 0 && model.TotalComponentsInUse > totalStockQty)
				{
					Fail(OrgSecondaryPartBOMPivotValidation.TotalComponentQtyCannotExceedTotalComponentStockQty(new ZDecimal(model.TotalComponentsInUse), new ZDecimal(totalStockQty)));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		OrgSupplierPart MatchAndValidateSecondaryPartDefinition(IEntity orgSecondaryPartBOMEntity, string mainProductPartNum, HashSet<string> includedSecondaryParts)
		{
			OrgSupplierPart secondaryPart = null;

			var secondaryPartEntity = orgSecondaryPartBOMEntity.Parents.FirstOrDefault(e => e.EntityName == "SecondaryPart");
			if (secondaryPartEntity != null)
			{
				secondaryPart = MatchAndValidateOrgSupplierPartWithCommonOwner(secondaryPartEntity);

				FailIfInvalidSecondaryPart(secondaryPart, orgSecondaryPartBOMEntity, includedSecondaryParts, mainProductPartNum);
			}
			else if (orgSecondaryPartBOMEntity.Action == EntityAction.INSERT)
			{
				Fail("Cannot create new OrgSecondaryPartBOM without Secondary Part definition.");
			}

			return secondaryPart;
		}

		void FailIfInvalidSecondaryPart(OrgSupplierPart secondaryPart, IEntity orgSecondaryPartBOMEntity, HashSet<string> seenParts, string mainProductPartNum)
		{
			if (!seenParts.Add(secondaryPart.OP_PartNum) && orgSecondaryPartBOMEntity.Action == EntityAction.INSERT)
			{
				Fail(OrgSecondaryPartBOMValidation.SecondaryPartCannotBeAddedTwice);
			}
			else if (mainProductPartNum.Equals(secondaryPart.OP_PartNum, StringComparison.OrdinalIgnoreCase))
			{
				Fail(OrgSecondaryPartBOMValidation.SecondaryPartCannotBeMainProduct);
			}
			else if (secondaryPart.BillOfMaterials.Count > 0)
			{
				Fail(OrgSecondaryPartBOMValidation.SecondaryPartHasBillOfMaterials);
			}
			else if (OrgSecondaryPartBOMValidation.IsProductWithIsPickOnOrderEnabled(secondaryPart))
			{
				Fail(OrgSecondaryPartBOMValidation.SecondaryProductSetToPickWithWorkOrder);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void MatchAndValidateOrgSecondaryPartBOMPivotsDefinition(IEnumerable<IEntity> orgSecondaryPartBOMPivotEntities, OrgSecondaryPartBOM secondaryPartBOMLoadedFromEnterprise)
		{
			var includedPivots = new HashSet<Guid>();
			if (secondaryPartBOMLoadedFromEnterprise != null)
			{
				includedPivots.UnionWith(LoadExistingSecondaryPartPivotsFromDB(secondaryPartBOMLoadedFromEnterprise, OrgPartBOMModelsCache.Value.Values.Select(m => m.PK)));
			}

			foreach (var pivotEntity in orgSecondaryPartBOMPivotEntities)
			{
				if (pivotEntity.InternalPK == Guid.Empty)
				{
					if (!IsInsertionOrDeletionAction(pivotEntity.Action))
					{
						Fail("Unable to update dbo.OrgSecondaryPartBOMPivot without PK.");
					}
					var orgPartBOMEntity = pivotEntity.Parents.FirstOrDefault(e => e.EntityName == "ComponentOrgPartBOM");
					if (orgPartBOMEntity == null)
					{
						Fail("Unable to import an OrgSecondaryPartBOM without a PK or ComponentOrgPartBOM definition.");
					}

					if (orgPartBOMEntity.InternalPK == Guid.Empty)
					{
						var orgPartBOMModel = MatchAndValidateOrgPartBOMByFields(orgPartBOMEntity);
						UpdateTotalComponentsInUse(pivotEntity, orgPartBOMModel);
					}
					else
					{
						var bom = Factory.Load<OrgPartBOM>(orgPartBOMEntity.InternalPK);
						if (bom != null && OrgPartBOMModelsCache.Value.TryGetValue(OrgPartBOMModel.ToKey(bom.Component, bom.PackType), out var matchingBomModel))
						{
							UpdateTotalComponentsInUse(pivotEntity, matchingBomModel);
						}
					}

					if (!includedPivots.Add(orgPartBOMEntity.InternalPK))
					{
						Fail(OrgSecondaryPartBOMPivotValidation.CannotSelectSameBOMComponentTwice);
					}
				}
				else
				{
					var pivotBizo = Factory.Load<OrgSecondaryPartBOMPivot>(pivotEntity.InternalPK);
					var bom = pivotBizo.Component;
					if (pivotBizo != null && OrgPartBOMModelsCache.Value.TryGetValue(OrgPartBOMModel.ToKey(bom.Component, bom.PackType), out var matchingBomModel))
					{
						UpdateTotalComponentsInUse(pivotEntity, matchingBomModel);
					}
				}
			}

			void UpdateTotalComponentsInUse(IEntity pivotEntity, OrgPartBOMModel bomModel)
			{
				var pivotQtyXml = pivotEntity.GetPropertyOrBlankString("ComponentQuantity");
				if (decimal.TryParse(pivotQtyXml, out var componentQty))
				{
					if (pivotEntity.Action == EntityAction.DELETE)
					{
						bomModel.TotalComponentsInUse -= componentQty;
					}
					else
					{
						bomModel.TotalComponentsInUse += componentQty;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		OrgPartBOMModel MatchAndValidateOrgPartBOMByFields(IEntity orgPartBOMEntity)
		{
			var componentEntity = orgPartBOMEntity.Parents.FirstOrDefault(e => e.EntityName == XmlConstants.EntityNames.Component);
			if (componentEntity == null)
			{
				Fail("Unable to match OrgPartBom without Component/Pack Type pair.");
			}

			var component = MatchAndValidateOrgSupplierPartWithCommonOwner(componentEntity);
			var refPackType = MatchAndValidateRefPackType(orgPartBOMEntity);

			if (!OrgPartBOMModelsCache.Value.TryGetValue(OrgPartBOMModel.ToKey(component, refPackType), out var matchingBomModel))
			{
				Fail($"Unable to match OrgPartBom with provided Component '{component.OP_PartNum}' and Pack Type '{refPackType.F3_Code}'.");
			}
			orgPartBOMEntity.InternalPK = matchingBomModel.PK;

			return matchingBomModel;
		}

		bool IsMainProductSetToPickWithWorkOrder()
		{
			var mainProductHasPickOnOrderEnabled = OrgSecondaryPartBOMValidation.IsProductWithIsPickOnOrderEnabled(PartLoadedFromEnterprise);
			var mainProductHasPickOnOrderEnabledXml = XmlProduct.GetPropertyOrBlankString("IsComponentPickedOnSalesOrder");
			if (mainProductHasPickOnOrderEnabledXml != null)
			{
				if (bool.TryParse(mainProductHasPickOnOrderEnabledXml, out var parsedXml))
				{
					mainProductHasPickOnOrderEnabled = mainProductHasPickOnOrderEnabled || parsedXml;
				}
			}

			return mainProductHasPickOnOrderEnabled;
		}

		IEnumerable<string> LoadExistingSecondaryPartsFromDB()
		{
			var partsQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var secondaryPartsSubQuery = new ZDBOnlySubQuery(typeof(OrgSecondaryPartBOM), OrgSecondaryPartBOMSchema.OSB_OP_SecondaryProduct);
			secondaryPartsSubQuery.AddToFilter(OrgSecondaryPartBOMSchema.OSB_OP_MainProduct, PartLoadedFromEnterprise.PK);
			partsQuery.AddSubQuery(OrgSupplierPartSchema.PK, secondaryPartsSubQuery, JoinCondition.And);

			var secondaryParts = Factory.Load<OrgSupplierPart>(partsQuery);

			return secondaryParts.Select(p => p.OP_PartNum.ToString());
		}

		IEnumerable<Guid> LoadExistingSecondaryPartPivotsFromDB(OrgSecondaryPartBOM secondaryPartBOM, IEnumerable<Guid> associatedBOMs)
		{
			var existingSecondaryPartPivotsQuery = new ZQuery();
			existingSecondaryPartPivotsQuery.AddToFilter(OrgSecondaryPartBOMPivotSchema.OPP_OE_Component, associatedBOMs);
			existingSecondaryPartPivotsQuery.AddToFilter(OrgSecondaryPartBOMPivotSchema.OPP_OSB_SecondaryPart, secondaryPartBOM.PK);

			var secondaryPartPivots = Factory.Load<OrgSecondaryPartBOMPivot>(existingSecondaryPartPivotsQuery);

			return secondaryPartPivots.Select(p => p.OPP_OE_Component.ToGuid());
		}

		#endregion

		#region FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder

		public void FailIfCriticalOrgPartUnitFieldsModifiedWhenIsComponentOfPickedOnSalesOrder()
		{
			var isComponentUsedToBuiltKitOnSalesOrder = PartLoadedFromEnterprise != null && OrgPartBOMValidation.IsComponentUsedToBuiltKitOnSalesOrder(Factory, PartLoadedFromEnterprise.PK);

			var orgPartUnitCollection = XmlProduct.ChildrenCollection.Where(e => e.EntityName == XmlConstants.EntityNames.OrgPartUnit);
			foreach (var orgPartUnitEntity in orgPartUnitCollection)
			{
				if (isComponentUsedToBuiltKitOnSalesOrder)
				{
					if (orgPartUnitEntity.InternalPK == Guid.Empty || orgPartUnitEntity.Action == EntityAction.INSERT || orgPartUnitEntity.Action == EntityAction.DELETE)
					{
						Fail(OrgPartUnitValidation.PickOnSalesOrderDetected);
					}

					OrgPartUnit orgPartUnitEntityLoadedFromEnterprise = null;
					if (orgPartUnitEntity.InternalPK != Guid.Empty)
					{
						orgPartUnitEntityLoadedFromEnterprise = Factory.Load<OrgPartUnit>(orgPartUnitEntity.InternalPK);
						if (orgPartUnitEntityLoadedFromEnterprise == null)
						{
							Fail(OrgPartUnitValidation.PickOnSalesOrderDetected);
						}
					}

					var quantityInParentProperty = orgPartUnitEntity.GetPropertyOrBlankString("QuantityInParent");
					if (ZDecimal.TryParse(quantityInParentProperty, out var quantityInParent) && quantityInParent != orgPartUnitEntityLoadedFromEnterprise?.OF_QuantityInParent
						|| !string.Equals(orgPartUnitEntity.GetPropertyOrBlankString("PackType"), orgPartUnitEntityLoadedFromEnterprise?.OF_PackType, StringComparison.OrdinalIgnoreCase)
						|| !string.Equals(orgPartUnitEntity.GetPropertyOrBlankString("ParentPackType"), orgPartUnitEntityLoadedFromEnterprise?.OF_ParentPackType, StringComparison.OrdinalIgnoreCase))
					{
						Fail(OrgPartUnitValidation.PickOnSalesOrderDetected);
					}
				}
			}
		}

		#endregion

		void Fail(string reason)
		{
			throw new NativeXMLUserVisibleException(reason);
		}

		static bool IsInsertionOrDeletionAction(EntityAction action) =>
			action == EntityAction.INSERT
			|| action == EntityAction.MERGE
			|| action == EntityAction.DELETE;
	}
}
