using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.OrgMatching;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.ProductEntityMatching
{
	public class ProductParamsByWhsAndClientHelper
	{
		#region Constructor

		public ProductParamsByWhsAndClientHelper(BusinessObjectFactory factory, IEntity xmlProduct)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			XmlProduct = Argument.NotNull(xmlProduct, nameof(xmlProduct));
			ProductParamsByWhsAndClientListCache = new Lazy<IEnumerable<IEntity>>(() => FindWhsProductParamsByWhsAndClientDirectlyBelowOrgSupplierPart(xmlProduct));
		}

		readonly BusinessObjectFactory Factory;
		readonly IEntity XmlProduct;
		Lazy<IEnumerable<IEntity>> ProductParamsByWhsAndClientListCache { get; }
		IEnumerable<IEntity> ProductParamsByWhsAndClientList => ProductParamsByWhsAndClientListCache.Value;

		#endregion

		#region FailIfInvalidProductParams

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public void FailIfInvalidProductParams(OrgSupplierPart partLoadedFromEnterprise)
		{
			if (ProductParamsByWhsAndClientList.Any())
			{
				foreach (var xmlProductParams in ProductParamsByWhsAndClientList)
				{
					var productNumberFromXml = XmlProduct.GetPropertyOrBlankString("PartNum");
					var organisationFromProductParams = GetOrganisationEntity(xmlProductParams);
					if (organisationFromProductParams == null)
					{
						Fail(string.Format(CultureInfo.InvariantCulture, "Invalid XML for WhsProductParamsByWhsAndClient under product {0}. The Organisation is not specified.", productNumberFromXml));
					}

					var orgToMatchAgainst = new EntityToOrgMatchingConverter(Factory).Convert(organisationFromProductParams);
					var org = new OrganisationMatcher(Factory, IfUnmatched.ReturnUnmatchedOrganisation).GetMatchingOrganization(orgToMatchAgainst);
					if (org != null)
					{
						organisationFromProductParams.InternalPK = org.PK.ToGuid();
					}
					else
					{
						Fail(string.Format(CultureInfo.InvariantCulture, "Invalid XML for WhsProductParamsByWhsAndClient under product {0}. The organisation specified is not valid under this context. Code: {1}.", productNumberFromXml, orgToMatchAgainst.OH_Code));
					}

					var warehouseFromProductParams = GetWarehouseEntity(xmlProductParams);
					if (warehouseFromProductParams == null)
					{
						Fail(string.Format(CultureInfo.InvariantCulture, "Invalid XML for WhsProductParamsByWhsAndClient under product {0}. The warehouse is not specified.", productNumberFromXml));
					}
					else
					{
						var warehouseCode = warehouseFromProductParams.GetPropertyOrBlankString("WarehouseCode");
						if (!warehouseCode.IsNullOrEmpty())
						{
							var query = new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, warehouseCode);
							query.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);

							var warehouse = Factory.Load<IWhsWarehouse>(query).SingleOrDefault();
							if (warehouse != null)
							{
								warehouseFromProductParams.InternalPK = warehouse.PK.ToGuid();
							}
							else
							{
								Fail(string.Format(CultureInfo.InvariantCulture, "Invalid XML for WhsProductParamsByWhsAndClient under product {0}. The warehouse specified is not valid under this context. Code: {1}.", productNumberFromXml, warehouseCode));
							}
						}

						if (xmlProductParams.Action != EntityAction.DELETE)
						{
							var validXmlOrganisationRelationships = ProductEntityMatchingInterceptor.FindOrganisationalRelationshipsDirectlyBelowOrgSupplierPart(XmlProduct)
																		.Where(r => r.Action != EntityAction.DELETE
																			&& (r.GetPropertyOrBlankString("Relationship") == OrgPartRelation.RelationshipTypes.Both || r.GetPropertyOrBlankString("Relationship") == OrgPartRelation.RelationshipTypes.Owner));
							FailIfNoMatchedOrganisationInXmlRelationships(partLoadedFromEnterprise, validXmlOrganisationRelationships, org);

							var productParam = GetProductParams(xmlProductParams.InternalPK, partLoadedFromEnterprise?.PK ?? ZGuid.Empty, org.PK, warehouseFromProductParams.InternalPK);
							FailIfInvalidMaximumShelfLife(productParam, partLoadedFromEnterprise, xmlProductParams, validXmlOrganisationRelationships);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfNoMatchedOrganisationInXmlRelationships(OrgSupplierPart partLoadedFromEnterprise, IEnumerable<IEntity> validXmlOrganisationRelationships, OrgHeader client)
		{
			var hasMatchedClient = false;
			foreach (var xmlRelation in validXmlOrganisationRelationships)
			{
				var organisationFromRelation = GetOrganisationEntity(xmlRelation);
				if (organisationFromRelation != null && organisationFromRelation.InternalPK == client.PK.ToGuid())
				{
					hasMatchedClient = true;
					break;
				}
			}

			if (!hasMatchedClient && partLoadedFromEnterprise != null)
			{
				hasMatchedClient = partLoadedFromEnterprise.RelatedOrganisations.Cast<OrgPartRelation>().Any(r => r.OU_OH == client.PK.ToGuid() && (r.OU_Relationship == OrgPartRelation.RelationshipTypes.Both || r.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner));
			}

			if (!hasMatchedClient)
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Invalid XML for WhsProductParamsByWhsAndClient. Cannot find out matched Organisation from dbo.OrgPartRelation with the specified organisation with code {0} under this context.", client.OH_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void FailIfInvalidMaximumShelfLife(IWhsProductParamsByWhsAndClient productParams, OrgSupplierPart partLoadedFromEnterprise, IEntity xmlProductParams, IEnumerable<IEntity> validXmlOrganisationRelationships)
		{
			var maximumShelfLife = GetMaximumShelfLife(xmlProductParams);
			if (maximumShelfLife < 0)
			{
				Fail(string.Format(CultureInfo.InvariantCulture, ("Maximum Shelf Life Accepted '{0}' is not valid."), maximumShelfLife));
			}

			var helper = ObjectFactory.Get<IWhsProductParamsByWhsAndClientValidationHelper>();
			var organisationFromProductParams = GetOrganisationEntity(xmlProductParams);
			if (organisationFromProductParams != null)
			{
				var matchedOrgPKInXml = Guid.Empty;
				foreach (var xmlRelation in validXmlOrganisationRelationships)
				{
					var organisationFromRelation = GetOrganisationEntity(xmlRelation);
					if (organisationFromRelation != null && organisationFromProductParams.InternalPK == organisationFromRelation.InternalPK)
					{
						var consigneeMinShelfLifeAccepted = GetConsigneeMinShelfLifeAccepted(xmlRelation);
						FailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAccepted(helper, maximumShelfLife, consigneeMinShelfLifeAccepted);

						matchedOrgPKInXml = organisationFromRelation.InternalPK;
						break;
					}
				}

				if (matchedOrgPKInXml == Guid.Empty && partLoadedFromEnterprise != null)
				{
					var matchedRelationInDB = partLoadedFromEnterprise.RelatedOrganisations.FindByOrganisationPKAndRelationship(organisationFromProductParams.InternalPK, OrgPartRelation.RelationshipTypes.Owner);
					if (matchedRelationInDB != null)
					{
						FailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAccepted(helper, maximumShelfLife, matchedRelationInDB.OU_ConsigneeMinShelfLifeAccepted);
					}
				}

				if (productParams != null)
				{
					var errorMsgForMaximumShelfLifeIsValidWhenHasStock = helper.CheckMaximumShelfLifeIsValidWhenHasStock(productParams, maximumShelfLife);
					if (!errorMsgForMaximumShelfLifeIsValidWhenHasStock.IsEmpty)
					{
						Fail(errorMsgForMaximumShelfLifeIsValidWhenHasStock);
					}
				}
			}
		}

		void FailIfMaximumShelfLifeLessThanConsigneeMinShelfLifeAccepted(IWhsProductParamsByWhsAndClientValidationHelper helper, ZShort maximumShelfLife, ZShort consigneeMinShelfLifeAccepted)
		{
			var errorMsgForIsLessThanConsigneeMinShelfLifeAccepted = helper.CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(consigneeMinShelfLifeAccepted, maximumShelfLife);
			if (!errorMsgForIsLessThanConsigneeMinShelfLifeAccepted.IsEmpty)
			{
				Fail(errorMsgForIsLessThanConsigneeMinShelfLifeAccepted);
			}
		}

		IWhsProductParamsByWhsAndClient GetProductParams(ZGuid pk, ZGuid partPK, ZGuid clientPK, ZGuid warehousePK)
		{
			IWhsProductParamsByWhsAndClient productParams = null;
			if (!pk.IsEmpty)
			{
				productParams = Factory.Load<IWhsProductParamsByWhsAndClient>(pk);
			}
			else
			{
				var query = new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, partPK);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_OH, clientPK);
				query.AddToFilter(WhsProductParamsByWhsAndClientSchema.W3_WW, warehousePK);

				productParams = Factory.Load<IWhsProductParamsByWhsAndClient>(query).SingleOrDefault();
			}

			return productParams;
		}

		static IEnumerable<IEntity> FindWhsProductParamsByWhsAndClientDirectlyBelowOrgSupplierPart(IEntity xmlProduct)
		{
			return xmlProduct.ChildrenCollection.Where(entity => entity.EntityName == "WhsProductParamsByWhsAndClient").ToArray();
		}

		static ZShort GetMaximumShelfLife(IEntity xmlProductParams)
		{
			var maximumShelfLifeString = xmlProductParams.GetPropertyOrBlankString("MaximumShelfLife");
			return ZShort.ParseSafe(maximumShelfLifeString, 0);
		}

		ZShort GetConsigneeMinShelfLifeAccepted(IEntity xmlRelation)
		{
			var consigneeMinShelfLifeAcceptedString = xmlRelation.GetPropertyOrBlankString("ConsigneeMinShelfLifeAccepted");
			return ZShort.ParseSafe(consigneeMinShelfLifeAcceptedString, 0);
		}

		void Fail(string p)
		{
			throw new NativeXMLUserVisibleException(p);
		}

		#endregion

		#region GetBestMatchProductParamsByOrganisation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public static IEntity GetBestMatchProductParamsByOrganisation(IEntity xmlProduct, IEntity organisationInRelationship)
		{
			var xmlProductParams = FindWhsProductParamsByWhsAndClientDirectlyBelowOrgSupplierPart(xmlProduct).ToArray();
			var matchedProductParams = xmlProductParams.Where(p => GetOrganisationEntity(p).GetPropertyOrBlankString("Code") == organisationInRelationship.GetPropertyOrBlankString("Code"))
									.Select(p => new { ProductParams = p, MaximumShelfLifeStr = p.GetPropertyOrBlankString("MaximumShelfLife") })
									.Where(v => !v.MaximumShelfLifeStr.IsNullOrEmpty() && ZShort.TryParse(v.MaximumShelfLifeStr, out ZShort maxShelfLife))
									.OrderBy(v => ZShort.ParseSafe(v.MaximumShelfLifeStr, 0)).FirstOrDefault();

			return matchedProductParams?.ProductParams;
		}

		#endregion

		#region GetEntity

		static IEntity GetOrganisationEntity(IEntity entity)
		{
			return GetEntityByEntityName(entity, "OrgHeader");
		}

		static IEntity GetWarehouseEntity(IEntity entity)
		{
			return GetEntityByEntityName(entity, "WhsWarehouse");
		}

		static IEntity GetEntityByEntityName(IEntity entity, string entityName)
		{
			if (entity.Parents.Any(p => p.EntityName == entityName))
			{
				var result = entity.Parents.First(p => p.EntityName == entityName);
				return result;
			}

			return null;
		}

		#endregion
	}
}
