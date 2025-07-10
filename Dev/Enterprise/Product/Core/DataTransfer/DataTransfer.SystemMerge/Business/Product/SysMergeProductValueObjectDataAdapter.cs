using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Cus = Enterprise.Customs.Business;
using Res = Enterprise.DataTransfer.SystemMerge.Business.Res;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	public class SysMergeProductValueObjectDataAdapter : ValueObjectDataAdapter<OrgSupplierPart, Xsd.OrgSupplierPart>
	{
		public override string RootCollectionElementName
		{
			get { return "OrgSupplierParts"; }
		}

		public override string RootElementName
		{
			get { return "OrgSupplierPart"; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		protected override OrgSupplierPart FindBusinessObject(Xsd.OrgSupplierPart value, IValueObjectImportContext context)
		{
			return FindProductByPk(value, context);
		}

		OrgSupplierPart FindProductByPk(Xsd.OrgSupplierPart value, IValueObjectImportContext context)
		{
			ZGuid pkTofind = new ZGuid(value.PK);
			return context.Factory.Load<OrgSupplierPart>(pkTofind);
		}

		#region ImportFromValueObject

		protected override OrgSupplierPart NewBusinessObject(Xsd.OrgSupplierPart value, IValueObjectImportContext context)
		{
			return context.Factory.NewWithPrimaryKey<OrgSupplierPart>(new Guid(value.PK));
		}

		protected override bool ConfirmUpdateOfExistingBusinessObject(OrgSupplierPart obj, INotifications notifications)
		{
			return false;
		}

		protected override void OnUserDeclinedImport(OrgSupplierPart bizObj, Xsd.OrgSupplierPart value, IValueObjectImportContext context)
		{
			string message = Res.GetString("3a740e1e-c0ff-4489-a71e-7cf484135dcd", "Import of product [({0}) - {1} - {2}] skipped. Reason: Product already exists.", bizObj.PK.ToString(), bizObj.OP_PartNum, bizObj.OP_Desc) + "\r\n";
			context.Notify(new InfoNotification(message));
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// Don't notify here. It will be notified if save succeeds.
		}

		protected override void ImportFromValueObjectCore(OrgSupplierPart bizObj, Xsd.OrgSupplierPart value, IValueObjectImportContext context)
		{
			var errorContext = Res.GetString("7c0fb2ed-d59d-404d-862f-db1ef6532ad2", "[Product: {0} - {1}]", value.PartNum, value.Desc);

			ImportOrgSupplierPart(bizObj, value, context);

			ImportOrgPartRelations(bizObj, value, context);
			ImportOrgPartLocations(bizObj, value);
			ImportOrgPartUnits(bizObj, value);
			ImportOrgSupplierPartBarcodes(bizObj, value);
			ImportCusClassifications(bizObj, value, context);
			ImportWhsPickFaces(bizObj, value, errorContext);
			ImportWhsProductParamsByWhsAndClient(bizObj, value, errorContext);

			SysMergeValueObjectHelper.ImportCPDecAnswers(bizObj.Factory, value.CPDecAnswers, bizObj.PK, OrgSupplierPartSchema.Constants.Prefix);

			AddImportEvent(bizObj);
		}

		void ImportOrgSupplierPart(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct, IValueObjectImportContext context)
		{
			if (!xsdProduct.DG_Code.IsEmpty)
			{
				var dgSubstances = UNDGSubstanceLoader.LoadSubstances(product.Factory, xsdProduct.DG_Code.SubstringSafe(0, 4), xsdProduct.DG_Code.SubstringSafe(4, 2), xsdProduct.DG_Standard).ToArray();

				if (dgSubstances != null && dgSubstances.Length == 1)
				{
					product.UNDGs.AddNew().DI_DG = dgSubstances[0].PK;
				}
				else
				{
					context.AddWarning(Res.GetString("08336EC4-9E61-4548-98AC-C165B09A47CD", "There is none or more than one matches [Dangerous Goods: {0} - {1}]", xsdProduct.DG_Code, xsdProduct.DG_Standard));
				}
			}

			product.OP_AutoPrintAssemblyInstructions = xsdProduct.AutoPrintAssemblyInstructions;
			product.OP_Brand = xsdProduct.Brand;
			product.OP_CanDisassembleKit = xsdProduct.CanDisassembleKit;
			product.OP_CanResell = xsdProduct.CanResell;
			product.OP_KitIsAutoReplenished = xsdProduct.KitIsAutoReplenished;
			product.OP_CountDecimalPlaces = xsdProduct.CountDecimalPlaces;
			product.OP_Cubic = xsdProduct.Cubic;
			product.OP_CubicUQ = xsdProduct.CubicUQ;
			product.OP_CustomAttrib1 = xsdProduct.CustomAttrib1;
			product.OP_CustomAttrib2 = xsdProduct.CustomAttrib2;
			product.OP_CustomAttrib3 = xsdProduct.CustomAttrib3;
			product.OP_CustomAttrib4 = xsdProduct.CustomAttrib4;
			product.OP_CustomAttrib5 = xsdProduct.CustomAttrib5;
			product.OP_CustomDate1 = xsdProduct.CustomDate1;
			product.OP_CustomDate2 = xsdProduct.CustomDate2;
			product.OP_CustomDate3 = xsdProduct.CustomDate3;
			product.OP_CustomDate4 = xsdProduct.CustomDate4;
			product.OP_CustomDate5 = xsdProduct.CustomDate5;
			product.OP_CustomDecimal1 = xsdProduct.CustomDecimal1;
			product.OP_CustomDecimal2 = xsdProduct.CustomDecimal2;
			product.OP_CustomDecimal3 = xsdProduct.CustomDecimal3;
			product.OP_CustomDecimal4 = xsdProduct.CustomDecimal4;
			product.OP_CustomDecimal5 = xsdProduct.CustomDecimal5;
			product.OP_CustomFlag1 = xsdProduct.CustomFlag1;
			product.OP_CustomFlag2 = xsdProduct.CustomFlag2;
			product.OP_CustomFlag3 = xsdProduct.CustomFlag3;
			product.OP_CustomFlag4 = xsdProduct.CustomFlag4;
			product.OP_CustomFlag5 = xsdProduct.CustomFlag5;
			product.OP_Department = xsdProduct.Department;
			product.OP_Depth = xsdProduct.Depth;
			if (xsdProduct.Desc.Length > product.OP_DescInfo.MaxLength)
			{
				product.OP_Desc = xsdProduct.Desc.Left(product.OP_DescInfo.MaxLength);
				context.AddWarning(Res.GetString("369e35d4-214e-45dc-952b-d8783c6d6762", "Value {0} was too large ({1} characters entered, maximum length = {2}). It has been truncated as a result.", xsdProduct.Desc, xsdProduct.Desc.Length, product.OP_DescInfo.MaxLength));
			}
			else
			{
				product.OP_Desc = xsdProduct.Desc;
			}
			product.OP_Division = xsdProduct.Division;
			product.OP_Height = xsdProduct.Height;
			product.OP_IsActive = xsdProduct.IsActive;
			product.OP_LastCost = xsdProduct.LastCost;
			product.OP_MeasureUQ = xsdProduct.MeasureUQ;
			product.OP_Model = xsdProduct.Model;
			product.OP_NetWeight = xsdProduct.NetWeight;
			product.OP_OrderMultipleQty = xsdProduct.OrderMultipleQty;
			product.OP_OrderMultipleUnit = xsdProduct.OrderMultipleUnit;
			product.OP_PartNum = xsdProduct.PartNum;
			product.OP_QtyInStock = xsdProduct.QtyInStock;
			product.OP_RH_NKCommodityCode = xsdProduct.RH_NKCommodityCode;
			product.OP_StockKeepingUnit = xsdProduct.StockKeepingUnit;
			product.OP_VendorPackQty = xsdProduct.VendorPackQty;
			product.OP_F3_NKPackType = xsdProduct.VendorPackUnit;
			product.OP_Weight = xsdProduct.Weight;
			product.OP_WeightedCost = xsdProduct.WeightedCost;
			product.OP_WeightUQ = xsdProduct.WeightUQ;
			product.OP_Width = xsdProduct.Width;

			product.IsImportedFromXML = true;
		}

		#region ImportOrgPartRelations

		void ImportOrgPartRelations(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct, IValueObjectImportContext context)
		{
			foreach (Xsd.OrgPartRelation xsdRelation in xsdProduct.OrgPartRelations)
			{
				CheckRelatedOrganisationExists(product, xsdRelation);

				OrgPartRelation relation = product.RelatedOrganisations.AddNew();
				relation.OU_OH = new ZGuid(xsdRelation.OrgHeaderPK);
				relation.OU_Relationship = xsdRelation.Relationship;
				relation.OU_LocalPartNumber = xsdRelation.LocalPartNumber;
				relation.OU_Hi = xsdRelation.Hi;
				relation.OU_Ti = xsdRelation.Ti;
				relation.OU_LandedCostMarginPercent1 = xsdRelation.LandedCostMarginPercent1;
				relation.OU_LandedCostMarginPercent2 = xsdRelation.LandedCostMarginPercent2;
				relation.OU_LandedCostMarginPercent3 = xsdRelation.LandedCostMarginPercent3;
				relation.OU_FormLayoutController = xsdRelation.FormLayoutController;
				relation.OU_LocalPartDescription = xsdRelation.LocalPartDescription;
				relation.OU_ClientUQ = xsdRelation.ClientUQ;
				relation.OU_RoyaltyPercent = xsdRelation.RoyaltyPercent;
				relation.OU_RoyaltyFlatAmount = xsdRelation.RoyaltyFlatAmount;
				relation.OU_RX_NKRoyaltyCurrency = xsdRelation.RX_NKRoyaltyCurrency;
				relation.OU_ConsigneeMinShelfLifeAccepted = xsdRelation.ConsigneeMinShelfLifeAccepted;

				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_RFAttributeConfirmInfo, xsdRelation.RFAttributeConfirm);
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_UsePartAttrib1Info, xsdRelation.UsePartAttrib1.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_UsePartAttrib2Info, xsdRelation.UsePartAttrib2.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_UsePartAttrib3Info, xsdRelation.UsePartAttrib3.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_UseExpiryDateInfo, xsdRelation.UseExpiryDate.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_UsePackingDateInfo, xsdRelation.UsePackingDate.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_PickModeInfo, xsdRelation.PickMode);
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_CompletePalletPickingInfo, xsdRelation.CompletePalletPicking.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_RollUpAttributesOnDocumentsInfo, xsdRelation.RollUpAttributesOnDocuments.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_ExpiryDateFormatStringInfo, xsdRelation.ExpiryDateFormatString);
				context.SetPropertyInfoValueIfValueNotEmpty(relation.OU_PackingDateFormatStringInfo, xsdRelation.PackingDateFormatString);
			}
		}

		void CheckRelatedOrganisationExists(OrgSupplierPart product, Xsd.OrgPartRelation xsdRelation)
		{
			var orgPk = new ZGuid(xsdRelation.OrgHeaderPK);
			var org = product.Factory.Load<OrgHeader>(orgPk);

			if (org == null)
			{
				string message = Res.GetString("c7be9230-c369-434c-afa1-f33300a74d4d", "[Product: {0} - {1}]\r\nCould not find Organization with PK = [{2}].\r\nPlease import all Organizations related to this product then retry the import operation.",
					product.OP_PartNum,
					product.OP_Desc,
					orgPk.ToString()) + "\r\n";

				throw new Exception(message);
			}
		}

		#endregion

		void ImportOrgPartLocations(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct)
		{
			foreach (Xsd.OrgPartLocation xsdLocation in xsdProduct.OrgPartLocations)
			{
				var location = product.Locations.AddNew();
				location.OR_Warehouse = xsdLocation.Warehouse;
				location.OR_BinLocation = xsdLocation.BinLocation;
				location.OR_Hi = xsdLocation.Hi;
				location.OR_Ti = xsdLocation.Ti;
				location.OR_InStock = xsdLocation.InStock;
				location.OR_StockTakeCount = xsdLocation.StockTakeCount;
				location.OR_WeightCostThisLocation = xsdLocation.WeightCostThisLocation;
			}
		}

		void ImportOrgPartUnits(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct)
		{
			foreach (Xsd.OrgPartUnit xsdUnit in xsdProduct.OrgPartUnits)
			{
				var unit = product.PartUnits.GetUnitConversion(xsdUnit.ParentPackage, xsdUnit.Package);
				if (unit == null)
				{
					unit = product.PartUnits.AddNew();
					unit.OF_PackType = xsdUnit.Package;
					unit.OF_ParentPackType = xsdUnit.ParentPackage;
				}
				unit.OF_Cubic = xsdUnit.Cubic;
				unit.OF_Depth = xsdUnit.Depth;
				unit.OF_Height = xsdUnit.Height;
				unit.OF_Width = xsdUnit.Width;
				unit.OF_Weight = xsdUnit.Weight;
				unit.OF_NoOfSKUsInThisPack = xsdUnit.NoOfSKUsInThisPack;
				unit.OF_QuantityInParent = xsdUnit.QuantityInParent;
			}
		}

		void ImportOrgSupplierPartBarcodes(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct)
		{
			foreach (Xsd.OrgSupplierPartBarcode xsdBarcode in xsdProduct.OrgSupplierPartBarcodes)
			{
				var barcode = product.PartBarcodes.AddNew();
				barcode.PH_Barcode = xsdBarcode.Barcode;
				barcode.PH_F3_NKPackType = xsdBarcode.PackType;
			}
		}

		#region ImportCusClassifications

		void ImportCusClassifications(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct, IValueObjectImportContext context)
		{
			var dataAdapter = new SysMergeClassificationValueObjectDataAdapter(product);
			var classificationImportContext = new ValueObjectImportContext(product.Factory, context);
			foreach (Xsd.CusClassification xsdClassification in xsdProduct.CusClassifications)
			{
				dataAdapter.CreateOrUpdateFromValueObject(xsdClassification, classificationImportContext);
			}
		}

		#endregion

		#region ImportWhsPickFaces

		void ImportWhsPickFaces(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct, string errorContext)
		{
			var existingOrganisations = LoadExistingOrganisations(product.Factory, xsdProduct.WhsPickFaces);
			var existingLocations = LoadExistingLocations(product.Factory, xsdProduct.WhsPickFaces);
			foreach (Xsd.WhsPickFace xsdPickFace in xsdProduct.WhsPickFaces)
			{
				var pickFaceBizOType = ObjectFactory.GetType<IWhsPickFace>();
				var pickFace = product.Factory.New(pickFaceBizOType, new Guid(xsdPickFace.PK));

				ValidateAndImportClient(product, existingOrganisations, xsdPickFace, pickFace, errorContext);
				ValidateAndImportLocation(product, existingLocations, xsdPickFace, pickFace, errorContext);

				pickFace[WhsPickFaceSchema.WF_OP] = product.PK;
				pickFace[WhsPickFaceSchema.WF_ReplenishMinimum] = xsdPickFace.ReplenishMinimum;
				pickFace[WhsPickFaceSchema.WF_ReplenishMaximum] = xsdPickFace.ReplenishMaximum;
			}
		}

		#region LoadExistingOrganisations

		IEnumerable<ZGuid> LoadExistingOrganisations(BusinessObjectFactory factory, Xsd.WhsPickFaceCollection whsPickFaceCollection)
		{
			var orgsToFind = whsPickFaceCollection.Cast<Xsd.WhsPickFace>().Select(p => new ZGuid(p.ClientPK));
			return factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgsToFind)).Select(o => o.PK);
		}

		#endregion

		#region LoadExistingLocations

		IEnumerable<ZGuid> LoadExistingLocations(BusinessObjectFactory factory, Xsd.WhsPickFaceCollection whsPickFaceCollection)
		{
			var locationsToFind = whsPickFaceCollection.Cast<Xsd.WhsPickFace>().Select(l => new ZGuid(l.LocationPK));
			return factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.PK, locationsToFind)).Cast<BusinessObject>().Select(l => l.PK);
		}

		IEnumerable<ZGuid> LoadExistingLocations(BusinessObjectFactory factory, Xsd.WhsProductParamByWhsAndClientCollection whsProductParamsByWhsAndClientCollection)
		{
			var locationsToFind = whsProductParamsByWhsAndClientCollection.Cast<Xsd.WhsProductParamByWhsAndClient>().Select(l => new ZGuid(l.StagingLocationBOMPK));
			return factory.Load<IWhsLocation>(new ZQuery(WhsLocationViewSchema.PK, locationsToFind)).Cast<BusinessObject>().Select(l => l.PK);
		}

		#endregion

		#region ValidateAndImportClient

		void ValidateAndImportClient(OrgSupplierPart product, IEnumerable<ZGuid> existingOrganisations, Xsd.WhsPickFace xsdPickFace, BusinessObject pickFace, string errorContext)
		{
			var clientPK = new ZGuid(xsdPickFace.ClientPK);
			if (existingOrganisations.Contains(clientPK))
			{
				pickFace[WhsPickFaceSchema.WF_OH_Client] = new ZGuid(xsdPickFace.ClientPK);
			}
			else
			{
				string message = Res.GetString("a9a8b270-6b0a-49d2-bed6-7c8ea5d3728a", "{0}\r\nCould not find Organization with PK = ({1}).\r\nPlease import it first and then retry the import operation.",
					errorContext, clientPK) + "\r\n";
				product.Delete(); // delete dbo.OrgSupplierPart to not allow DataTransfer architecture to save it to DB.
				throw new InvalidOperationException(message);
			}
		}

		#endregion

		#region ValidateAndImportLocation

		void ValidateAndImportLocation(OrgSupplierPart product, IEnumerable<ZGuid> existingLocations, Xsd.WhsPickFace xsdPickFace, BusinessObject pickFace, string errorContext)
		{
			var locationPK = new ZGuid(xsdPickFace.LocationPK);
			if (existingLocations.Contains(locationPK))
			{
				pickFace[WhsPickFaceSchema.WF_WL] = new ZGuid(xsdPickFace.LocationPK);
			}
			else
			{
				var message = GetLocationErrorMessage(errorContext, locationPK);
				product.Delete(); // delete dbo.OrgSupplierPart to not allow DataTransfer architecture to save it to DB.
				throw new InvalidOperationException(message);
			}
		}

		void ValidateAndImportLocation(OrgSupplierPart product, IEnumerable<ZGuid> existingLocations, Xsd.WhsProductParamByWhsAndClient xsdProductParams, BusinessObject productParam, string errorContext)
		{
			var locationPK = new ZGuid(xsdProductParams.StagingLocationBOMPK);
			if (!locationPK.IsEmpty)
			{
				if (existingLocations.Contains(locationPK))
				{
					productParam[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM] = new ZGuid(xsdProductParams.StagingLocationBOMPK);
				}
				else
				{
					var message = GetLocationErrorMessage(errorContext, locationPK);
					product.Delete(); // delete dbo.OrgSupplierPart to not allow DataTransfer architecture to save it to DB.
					throw new InvalidOperationException(message);
				}
			}
		}

		static string GetLocationErrorMessage(string errorContext, ZGuid locationPK)
		{
			return Res.GetString("2114043d-9ced-49d1-a55c-db54a91b2f4c",
				"{0}\r\nCould not find Warehouse Location with PK = ({1}).\r\nPlease import it first and then retry the import operation.",
				errorContext, locationPK) + "\r\n";
		}

		#endregion

		#endregion

		#region ImportWhsProductParamsByWhsAndClient

		void ImportWhsProductParamsByWhsAndClient(OrgSupplierPart product, Xsd.OrgSupplierPart xsdProduct, string errorContext)
		{
			var existingOrganisations = LoadExistingOrganisations(product.Factory, xsdProduct.WhsProductParamsByWhsAndClient);
			var existingWarehouses = LoadExistingWarehouses(product.Factory, xsdProduct.WhsProductParamsByWhsAndClient);
			var existingAreas = LoadExistingAreas(product.Factory, existingWarehouses);
			var existingLocations = LoadExistingLocations(product.Factory, xsdProduct.WhsProductParamsByWhsAndClient);
			foreach (Xsd.WhsProductParamByWhsAndClient xsdProductParam in xsdProduct.WhsProductParamsByWhsAndClient)
			{
				var productParamBizOType = ObjectFactory.GetType<IWhsProductParamsByWhsAndClient>();
				var productParam = product.Factory.New(productParamBizOType, new Guid(xsdProductParam.PK));

				ValidateAndImportClient(product, existingOrganisations, xsdProductParam, productParam, errorContext);
				ValidateAndImportWarehouse(product, existingWarehouses, xsdProductParam, productParam, errorContext);
				ValidateAndImportLocation(product, existingLocations, xsdProductParam, productParam, errorContext);

				productParam[WhsProductParamsByWhsAndClientSchema.W3_OP] = product.PK;
				productParam[WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity] = xsdProductParam.EconomicQuantity;
				productParam[WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod] = xsdProductParam.ExpiryNotificationPeriod;
				productParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReceivedPackType] = xsdProductParam.F3_NKReceivedPackType;
				productParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReleasedPackType] = xsdProductParam.F3_NKReleasedPackType;
				productParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum] = xsdProductParam.ReplenishmentMinimum;
				productParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMultiple] = xsdProductParam.ReplenishmentMultiple;
				productParam[WhsProductParamsByWhsAndClientSchema.W3_StockTakeCycle] = xsdProductParam.StockTakeCycle;
			}
		}

		#region LoadExistingOrganisations

		IEnumerable<ZGuid> LoadExistingOrganisations(BusinessObjectFactory factory, Xsd.WhsProductParamByWhsAndClientCollection whsProductParamByWhsAndClientCollection)
		{
			var orgsToFind = whsProductParamByWhsAndClientCollection.Cast<Xsd.WhsProductParamByWhsAndClient>().Select(o => new ZGuid(o.ClientPK));
			return factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgsToFind)).Select(o => o.PK);
		}

		#endregion

		#region LoadExistingWarehouses

		IEnumerable<ZGuid> LoadExistingWarehouses(BusinessObjectFactory factory, Xsd.WhsProductParamByWhsAndClientCollection whsProductParamByWhsAndClientCollection)
		{
			var warehousesToFind = whsProductParamByWhsAndClientCollection.Cast<Xsd.WhsProductParamByWhsAndClient>().Select(w => new ZGuid(w.WarehousePK));
			return factory.Load<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, warehousesToFind)).Cast<BusinessObject>().Select(w => w.PK);
		}

		#endregion

		#region LoadExistingAreas

		IEnumerable<ZGuid> LoadExistingAreas(BusinessObjectFactory factory, IEnumerable<ZGuid> warehousesToLoadAreasFor)
		{
			return factory.Load<IWhsArea>(new ZQuery(WhsAreaSchema.WA_WW_Whs, warehousesToLoadAreasFor)).Cast<BusinessObject>().Select(p => p.PK);
		}

		#endregion

		#region ValidateAndImportClient

		void ValidateAndImportClient(OrgSupplierPart product, IEnumerable<ZGuid> existingOrganisations, Xsd.WhsProductParamByWhsAndClient xsdProductParam, BusinessObject productParam, string errorContext)
		{
			var clientPK = new ZGuid(xsdProductParam.ClientPK);
			if (existingOrganisations.Contains(clientPK))
			{
				productParam[WhsProductParamsByWhsAndClientSchema.W3_OH] = clientPK;
			}
			else
			{
				string message = Res.GetString("a9a8b270-6b0a-49d2-bed6-7c8ea5d3728a", "{0}\r\nCould not find Organization with PK = ({1}).\r\nPlease import it first and then retry the import operation.",
					errorContext, xsdProductParam.ClientPK) + "\r\n";
				product.Delete(); // delete dbo.OrgSupplierPart to not allow DataTransfer architecture to save it to DB.
				throw new InvalidOperationException(message);
			}
		}

		#endregion

		#region ValidateAndImportWarehouse

		void ValidateAndImportWarehouse(OrgSupplierPart product, IEnumerable<ZGuid> existingWarehouses, Xsd.WhsProductParamByWhsAndClient xsdProductParam, BusinessObject productParam, string errorContext)
		{
			var warehousePK = new ZGuid(xsdProductParam.WarehousePK);
			if (existingWarehouses.Contains(warehousePK))
			{
				productParam[WhsProductParamsByWhsAndClientSchema.W3_WW] = warehousePK;
			}
			else
			{
				string message = Res.GetString("af96fb28-c294-4c81-a0b9-11b6999b4eb4", "{0}\r\nCould not find Warehouse with PK = ({1}).\r\nPlease import it first and then retry the import operation.",
					errorContext, xsdProductParam.WarehousePK) + "\r\n";
				product.Delete(); // delete dbo.OrgSupplierPart to not allow DataTransfer architecture to save it to DB.
				throw new InvalidOperationException(message);
			}
		}

		#endregion

		#endregion

		#endregion

		#region ExportToValueObject

		protected override void ExportToValueObjectCore(OrgSupplierPart bizObj, Xsd.OrgSupplierPart constructedValueObject, IValueObjectExportContext context)
		{
			ExportOrgSupplierPart(bizObj, constructedValueObject);

			ExportOrgPartRelations(bizObj, constructedValueObject.OrgPartRelations);
			ExportOrgPartLocations(bizObj, constructedValueObject.OrgPartLocations);
			ExportOrgPartUnits(bizObj, constructedValueObject.OrgPartUnits);
			ExportOrgSupplierPartBarcodes(bizObj, constructedValueObject.OrgSupplierPartBarcodes);
			ExportCusClassifications(bizObj, constructedValueObject.CusClassifications, context);
			ExportWhsPickFaces(bizObj, constructedValueObject.WhsPickFaces);
			ExportWhsProductParamsByWhsAndClient(bizObj, constructedValueObject.WhsProductParamsByWhsAndClient);

			SysMergeValueObjectHelper.ExportCPDecAnswers(bizObj.Factory, constructedValueObject.CPDecAnswers, bizObj.PK, OrgSupplierPartSchema.Constants.Prefix);
		}

		void ExportOrgSupplierPart(OrgSupplierPart product, Xsd.OrgSupplierPart xsdObject)
		{
			xsdObject.PK = product.PK.ToString();

			if (product.UNDGs.Count > 0)
			{
				if (product.UNDGs[0].Substance != null)
				{
					var subs = product.UNDGs[0].Substance;
					xsdObject.DG_Code = subs.DG_Code;
					xsdObject.DG_Standard = subs.DG_Standard;
				}
			}

			xsdObject.AutoPrintAssemblyInstructions = product.OP_AutoPrintAssemblyInstructions;
			xsdObject.Brand = product.OP_Brand;
			xsdObject.CanDisassembleKit = product.OP_CanDisassembleKit;
			xsdObject.CanResell = product.OP_CanResell;
			xsdObject.KitIsAutoReplenished = product.OP_KitIsAutoReplenished;
			xsdObject.CountDecimalPlaces = product.OP_CountDecimalPlaces;
			xsdObject.Cubic = product.OP_Cubic;
			xsdObject.CubicUQ = product.OP_CubicUQ;
			xsdObject.CustomAttrib1 = product.OP_CustomAttrib1;
			xsdObject.CustomAttrib2 = product.OP_CustomAttrib2;
			xsdObject.CustomAttrib3 = product.OP_CustomAttrib3;
			xsdObject.CustomAttrib4 = product.OP_CustomAttrib4;
			xsdObject.CustomAttrib5 = product.OP_CustomAttrib5;
			xsdObject.CustomDecimal1 = product.OP_CustomDecimal1;
			xsdObject.CustomDecimal2 = product.OP_CustomDecimal2;
			xsdObject.CustomDecimal3 = product.OP_CustomDecimal3;
			xsdObject.CustomDecimal4 = product.OP_CustomDecimal4;
			xsdObject.CustomDecimal5 = product.OP_CustomDecimal5;
			xsdObject.CustomFlag1 = product.OP_CustomFlag1;
			xsdObject.CustomFlag2 = product.OP_CustomFlag2;
			xsdObject.CustomFlag3 = product.OP_CustomFlag3;
			xsdObject.CustomFlag4 = product.OP_CustomFlag4;
			xsdObject.CustomFlag5 = product.OP_CustomFlag5;
			xsdObject.Department = product.OP_Department;
			xsdObject.Depth = product.OP_Depth;
			xsdObject.Desc = product.OP_Desc;
			xsdObject.Division = product.OP_Division;
			xsdObject.Height = product.OP_Height;
			xsdObject.IsActive = product.OP_IsActive;
			xsdObject.LastCost = product.OP_LastCost;
			xsdObject.MeasureUQ = product.OP_MeasureUQ;
			xsdObject.Model = product.OP_Model;
			xsdObject.NetWeight = product.OP_NetWeight;
			xsdObject.OrderMultipleQty = product.OP_OrderMultipleQty;
			xsdObject.OrderMultipleUnit = product.OP_OrderMultipleUnit;
			xsdObject.PartNum = product.OP_PartNum;
			xsdObject.QtyInStock = product.OP_QtyInStock;
			xsdObject.RH_NKCommodityCode = product.OP_RH_NKCommodityCode;
			xsdObject.StockKeepingUnit = product.OP_StockKeepingUnit;
			xsdObject.VendorPackQty = product.OP_VendorPackQty;
			xsdObject.VendorPackUnit = product.OP_F3_NKPackType;
			xsdObject.Weight = product.OP_Weight;
			xsdObject.WeightedCost = product.OP_WeightedCost;
			xsdObject.WeightUQ = product.OP_WeightUQ;
			xsdObject.Width = product.OP_Width;

			if (!product.OP_CustomDate1.IsEmpty)
			{
				xsdObject.CustomDate1 = product.OP_CustomDate1.ToDateTime();
				xsdObject.CustomDate1Specified = true;
			}

			if (!product.OP_CustomDate2.IsEmpty)
			{
				xsdObject.CustomDate2 = product.OP_CustomDate2.ToDateTime();
				xsdObject.CustomDate2Specified = true;
			}

			if (!product.OP_CustomDate3.IsEmpty)
			{
				xsdObject.CustomDate3 = product.OP_CustomDate3.ToDateTime();
				xsdObject.CustomDate3Specified = true;
			}

			if (!product.OP_CustomDate4.IsEmpty)
			{
				xsdObject.CustomDate4 = product.OP_CustomDate4.ToDateTime();
				xsdObject.CustomDate4Specified = true;
			}

			if (!product.OP_CustomDate5.IsEmpty)
			{
				xsdObject.CustomDate5 = product.OP_CustomDate5.ToDateTime();
				xsdObject.CustomDate5Specified = true;
			}

			// Set Specified flag for boolean/numeric fields so they get serialised to XML
			// Boolean
			xsdObject.AutoPrintAssemblyInstructionsSpecified = true;
			xsdObject.CanDisassembleKitSpecified = true;
			xsdObject.CanResellSpecified = true;
			xsdObject.KitIsAutoReplenishedSpecified = true;
			xsdObject.CustomFlag1Specified = true;
			xsdObject.CustomFlag2Specified = true;
			xsdObject.CustomFlag3Specified = true;
			xsdObject.CustomFlag4Specified = true;
			xsdObject.CustomFlag5Specified = true;
			xsdObject.IsActiveSpecified = true;
			// Numeric
			xsdObject.CountDecimalPlacesSpecified = true;
			xsdObject.CubicSpecified = true;
			xsdObject.CustomDecimal1Specified = true;
			xsdObject.CustomDecimal2Specified = true;
			xsdObject.CustomDecimal3Specified = true;
			xsdObject.CustomDecimal4Specified = true;
			xsdObject.CustomDecimal5Specified = true;
			xsdObject.DepthSpecified = true;
			xsdObject.HeightSpecified = true;
			xsdObject.LastCostSpecified = true;
			xsdObject.NetWeightSpecified = true;
			xsdObject.OrderMultipleQtySpecified = true;
			xsdObject.QtyInStockSpecified = true;
			xsdObject.VendorPackQtySpecified = true;
			xsdObject.WeightSpecified = true;
			xsdObject.WeightedCostSpecified = true;
			xsdObject.WidthSpecified = true;
		}

		void ExportOrgPartRelations(OrgSupplierPart product, Xsd.OrgPartRelationCollection xsdRelations)
		{
			foreach (OrgPartRelation relation in product.RelatedOrganisations)
			{
				Xsd.OrgPartRelation xsdRelation = xsdRelations.AddNew();

				xsdRelation.OrgHeaderPK = relation.OU_OH.ToString();
				xsdRelation.Relationship = relation.OU_Relationship;
				xsdRelation.LocalPartNumber = relation.OU_LocalPartNumber;
				xsdRelation.Hi = relation.OU_Hi;
				xsdRelation.Ti = relation.OU_Ti;
				xsdRelation.LandedCostMarginPercent1 = relation.OU_LandedCostMarginPercent1;
				xsdRelation.LandedCostMarginPercent2 = relation.OU_LandedCostMarginPercent2;
				xsdRelation.LandedCostMarginPercent3 = relation.OU_LandedCostMarginPercent3;
				xsdRelation.FormLayoutController = relation.OU_FormLayoutController;
				xsdRelation.LocalPartDescription = relation.OU_LocalPartDescription;
				xsdRelation.ClientUQ = relation.OU_ClientUQ;
				xsdRelation.RoyaltyPercent = relation.OU_RoyaltyPercent;
				xsdRelation.RoyaltyFlatAmount = relation.OU_RoyaltyFlatAmount;
				xsdRelation.RX_NKRoyaltyCurrency = relation.OU_RX_NKRoyaltyCurrency;
				xsdRelation.RFAttributeConfirm = relation.OU_RFAttributeConfirm;
				xsdRelation.PickMode = relation.OU_PickMode;
				xsdRelation.ExpiryDateFormatString = relation.OU_ExpiryDateFormatString;
				xsdRelation.PackingDateFormatString = relation.OU_PackingDateFormatString;
				xsdRelation.ConsigneeMinShelfLifeAccepted = relation.OU_ConsigneeMinShelfLifeAccepted;

				if (relation.OU_UsePartAttrib1)
				{
					xsdRelation.UsePartAttrib1 = xsdRelation.UsePartAttrib1Specified = true;
				}
				if (relation.OU_UsePartAttrib2)
				{
					xsdRelation.UsePartAttrib2 = xsdRelation.UsePartAttrib2Specified = true;
				}
				if (relation.OU_UsePartAttrib3)
				{
					xsdRelation.UsePartAttrib3 = xsdRelation.UsePartAttrib3Specified = true;
				}
				if (relation.OU_UseExpiryDate)
				{
					xsdRelation.UseExpiryDate = xsdRelation.UseExpiryDateSpecified = true;
				}
				if (relation.OU_UsePackingDate)
				{
					xsdRelation.UsePackingDate = xsdRelation.UsePackingDateSpecified = true;
				}
				if (relation.OU_CompletePalletPicking)
				{
					xsdRelation.CompletePalletPicking = xsdRelation.CompletePalletPickingSpecified = true;
				}
				if (relation.OU_RollUpAttributesOnDocuments)
				{
					xsdRelation.RollUpAttributesOnDocuments = xsdRelation.RollUpAttributesOnDocumentsSpecified = true;
				}

				// Set Specified flag for boolean/numeric fields so they get serialised to XML
				// Boolean
				xsdRelation.FormLayoutControllerSpecified = true;
				// Numeric
				xsdRelation.HiSpecified = true;
				xsdRelation.TiSpecified = true;
				xsdRelation.LandedCostMarginPercent1Specified = true;
				xsdRelation.LandedCostMarginPercent2Specified = true;
				xsdRelation.LandedCostMarginPercent3Specified = true;
				xsdRelation.RoyaltyPercentSpecified = true;
				xsdRelation.RoyaltyFlatAmountSpecified = true;
			}
		}

		void ExportOrgPartLocations(OrgSupplierPart product, Xsd.OrgPartLocationCollection xsdLocations)
		{
			foreach (OrgPartLocation location in product.Locations)
			{
				Xsd.OrgPartLocation xsdLocation = xsdLocations.AddNew();

				xsdLocation.Warehouse = location.OR_Warehouse;
				xsdLocation.BinLocation = location.OR_BinLocation;
				xsdLocation.Hi = location.OR_Hi;
				xsdLocation.Ti = location.OR_Ti;
				xsdLocation.InStock = location.OR_InStock;
				xsdLocation.StockTakeCount = location.OR_StockTakeCount;
				xsdLocation.WeightCostThisLocation = location.OR_WeightCostThisLocation;

				// Set Specified flag for boolean/numeric fields so they get serialised to XML
				// Numeric
				xsdLocation.HiSpecified = true;
				xsdLocation.TiSpecified = true;
				xsdLocation.InStockSpecified = true;
				xsdLocation.StockTakeCountSpecified = true;
				xsdLocation.WeightCostThisLocationSpecified = true;
			}
		}

		void ExportOrgPartUnits(OrgSupplierPart product, Xsd.OrgPartUnitCollection xsdUnits)
		{
			foreach (OrgPartUnit unit in product.PartUnits)
			{
				Xsd.OrgPartUnit xsdUnit = xsdUnits.AddNew();

				xsdUnit.Package = unit.OF_PackType;
				xsdUnit.ParentPackage = unit.OF_ParentPackType;
				xsdUnit.Cubic = unit.OF_Cubic;
				xsdUnit.Depth = unit.OF_Depth;
				xsdUnit.Height = unit.OF_Height;
				xsdUnit.Width = unit.OF_Width;
				xsdUnit.Weight = unit.OF_Weight;
				xsdUnit.NoOfSKUsInThisPack = unit.OF_NoOfSKUsInThisPack;
				xsdUnit.QuantityInParent = unit.OF_QuantityInParent;

				// Set Specified flag for boolean/numeric fields so they get serialised to XML
				// Numeric
				xsdUnit.CubicSpecified = true;
				xsdUnit.DepthSpecified = true;
				xsdUnit.HeightSpecified = true;
				xsdUnit.WidthSpecified = true;
				xsdUnit.WeightSpecified = true;
				xsdUnit.NoOfSKUsInThisPackSpecified = true;
				xsdUnit.QuantityInParentSpecified = true;
			}
		}

		void ExportOrgSupplierPartBarcodes(OrgSupplierPart product, Xsd.OrgSupplierPartBarcodeCollection xsdBarcodes)
		{
			foreach (OrgSupplierPartBarcode barcode in product.PartBarcodes)
			{
				Xsd.OrgSupplierPartBarcode xsdBarcode = xsdBarcodes.AddNew();
				xsdBarcode.Barcode = barcode.PH_Barcode;
				xsdBarcode.PackType = barcode.PH_F3_NKPackType;
			}
		}

		void ExportCusClassifications(OrgSupplierPart product, Xsd.CusClassificationCollection xsdClassifications, IValueObjectExportContext context)
		{
			ZQuery query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			Cus.BaseCusClassPartPivot[] classPivots = product.Factory.Load<Cus.BaseCusClassPartPivot>(query);

			var dataAdapter = new SysMergeClassificationValueObjectDataAdapter(product);

			foreach (Cus.BaseCusClassPartPivot classPivot in classPivots)
			{
				Xsd.CusClassification xsdClassification = xsdClassifications.AddNew();
				dataAdapter.ExportToValueObject(classPivot, xsdClassification, context);
			}
		}

		void ExportWhsPickFaces(OrgSupplierPart product, Xsd.WhsPickFaceCollection xsdWhsPickFaces)
		{
			var allPickFaces = product.Factory.Load<IWhsPickFace>(new ZQuery(WhsPickFaceSchema.WF_OP, product.PK));
			foreach (BusinessObject pickFace in allPickFaces)
			{
				var xsdPickFace = xsdWhsPickFaces.AddNew();
				xsdPickFace.PK = pickFace[WhsPickFaceSchema.PK].ToString();
				xsdPickFace.ClientPK = pickFace[WhsPickFaceSchema.WF_OH_Client].ToString();
				xsdPickFace.LocationPK = pickFace[WhsPickFaceSchema.WF_WL].ToString();

				xsdPickFace.ReplenishMinimum = (ZDecimal)pickFace[WhsPickFaceSchema.WF_ReplenishMinimum];
				xsdPickFace.ReplenishMaximum = (ZDecimal)pickFace[WhsPickFaceSchema.WF_ReplenishMaximum];
			}
		}

		void ExportWhsProductParamsByWhsAndClient(OrgSupplierPart product, Xsd.WhsProductParamByWhsAndClientCollection xsdWhsProductParamsByWhsAndClient)
		{
			var allWhsProductParamsByWhsAndClient = product.Factory.Load<IWhsProductParamsByWhsAndClient>(new ZQuery(WhsProductParamsByWhsAndClientSchema.W3_OP, product.PK));
			foreach (BusinessObject productParam in allWhsProductParamsByWhsAndClient)
			{
				var xsdProductParam = xsdWhsProductParamsByWhsAndClient.AddNew();

				xsdProductParam.PK = productParam[WhsProductParamsByWhsAndClientSchema.PK].ToString();
				xsdProductParam.ClientPK = productParam[WhsProductParamsByWhsAndClientSchema.W3_OH].ToString();
				xsdProductParam.WarehousePK = productParam[WhsProductParamsByWhsAndClientSchema.W3_WW].ToString();
				xsdProductParam.StagingLocationBOMPK = productParam[WhsProductParamsByWhsAndClientSchema.W3_WL_StagingLocationBOM].ToString();
				xsdProductParam.EconomicQuantity = (ZDecimal)productParam[WhsProductParamsByWhsAndClientSchema.W3_EconomicQuantity];
				xsdProductParam.ExpiryNotificationPeriod = (ZInt)productParam[WhsProductParamsByWhsAndClientSchema.W3_ExpiryNotificationPeriod];
				xsdProductParam.F3_NKReceivedPackType = productParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReceivedPackType].ToString();
				xsdProductParam.F3_NKReleasedPackType = productParam[WhsProductParamsByWhsAndClientSchema.W3_F3_NKReleasedPackType].ToString();
				xsdProductParam.ReplenishmentMinimum = (ZDecimal)productParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMinimum];
				xsdProductParam.ReplenishmentMultiple = (ZDecimal)productParam[WhsProductParamsByWhsAndClientSchema.W3_ReplenishmentMultiple];
				xsdProductParam.StockTakeCycle = productParam[WhsProductParamsByWhsAndClientSchema.W3_StockTakeCycle].ToString();
			}
		}

		#endregion
	}
}
