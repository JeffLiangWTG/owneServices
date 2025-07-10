using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.BarcodeParsing.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackedItemWrapperFromWhsReleaseLine : PackedItemWrapper
	{
		public PackedItemWrapperFromWhsReleaseLine(WhsReleaseLine releaseLine, PkgPackageItemDivotsWrapper[] packedItems, BusinessObjectFactory factory)
			: base(releaseLine, packedItems, factory)
		{
		}

		#region Properties

		#region LocalCode

		protected override ZString GetLocalCode()
		{
			var consigneeRelation = ConsigneePartRelation;
			return consigneeRelation != null ? consigneeRelation.OU_LocalPartNumber : ZString.Empty;
		}

		#endregion

		#region LocalCodeWithFallback

		protected override ZString GetLocalCodeWithFallback()
		{
			var result = LocalCode;
			return !result.IsEmpty ? result : Code;
		}

		#endregion

		#region LocalDescription

		protected override ZString GetLocalDescription()
		{
			var consigneeRelation = ConsigneePartRelation;
			return consigneeRelation != null ? consigneeRelation.OU_LocalPartDescription : ZString.Empty;
		}

		#endregion

		#region LocalDescriptionWithFallback

		protected override ZString GetLocalDescriptionWithFallback()
		{
			var result = LocalDescription;
			return !result.IsEmpty ? result : Description;
		}

		#endregion

		#region Expiry

		protected override ZBool GetIsExpiryUsed()
		{
			var client = Client;
			return client != null && client.PartAttributeManager.IsExpiryDateUsedByProduct(Part);
		}

		protected override ZString GetExpiryLabel()
		{
			var result = "";

			switch (OrgRelationExpiryIdentifier)
			{
				case "15":
					result = Res.GetString("c183214e-a8e7-4476-ab6b-12b5e2a45649", "BEST BEFORE");
					break;
				case "17":
					result = Res.GetString("da1d8ce7-5bc4-42ff-92b2-0398bb069b88", "USE BY");
					break;
				default:
					result = Res.GetString("97e10e3f-01ee-4e35-887d-514a726ce849", "EXPIRY DATE");
					break;
			}

			var dateFormatForLabel = ProductLabelDateFormat.Replace(".", "").Replace("-", "").Replace("/", "").ToLower();
			return result + " (" + dateFormatForLabel + ")";
		}

		protected override ZString GetExpiry()
		{
			return ReleaseLine?.ExpiryDate.ToString(ProductLabelDateFormat, Culture.Current) ?? ZString.Empty;
		}

		ZString ExpiryForBarcode
		{
			get { return ReleaseLine?.ExpiryDate.ToString("yyMMdd", Culture.Current) ?? ZString.Empty; }
		}

		ZString ExpiryIdentifier
		{
			get
			{
				var result = OrgRelationExpiryIdentifier;
				if (result != "15")
				{
					result = "17";
				}

				return result;
			}
		}

		ZString OrgRelationExpiryIdentifier
		{
			get
			{
				var result = "";
				var orgPartRelation = OrgPartRelation;
				if (orgPartRelation != null && orgPartRelation.OU_UseExpiryDate)
				{
					result = GetApplicationIdentifier(orgPartRelation, WarehouseTargetFields.Codes.ExpiryDate);
				}

				return result;
			}
		}

		ZString GetApplicationIdentifier(OrgPartRelation orgPartRelation, string targetFieldCode)
		{
			var result = ZString.Empty;
			var rulesMatched = BarcodeRule.LoadMatchingRules(Factory, BarcodeModuleTypes.Codes.Warehouse, orgPartRelation.OU_OH, ZGuid.Empty, orgPartRelation.OU_OP, true);
			if (rulesMatched != null)
			{
				var rule = rulesMatched.FirstOrDefault(r => r.IsGS1 && r.IsPartialRule);
				if (rule != null)
				{
					var component = rule.Components.FirstOrDefault(c => c.BRC_TargetField == targetFieldCode);
					if (component != null)
					{
						result = component.BRC_ApplicationID;
					}
				}
			}
			return result;
		}

		#endregion

		#region PackingDate

		protected override ZBool GetIsPackingDateUsed()
		{
			var client = Client;
			return client != null && client.PartAttributeManager.IsPackingDateUsedByProduct(Part);
		}

		protected override ZString GetPackingDate()
		{
			return ReleaseLine?.PackingDate.ToString(ProductLabelDateFormat, Culture.Current) ?? base.GetPackingDate();
		}

		#endregion

		#region PartAttributes

		protected override ZBool GetIsPartAttrib1Used()
		{
			var client = Client;
			return client != null && client.PartAttributeManager.IsPartAttributeUsedByProduct(Part, 1);
		}

		protected override ZBool GetIsPartAttrib2Used()
		{
			var client = Client;
			return client != null && client.PartAttributeManager.IsPartAttributeUsedByProduct(Part, 2);
		}

		protected override ZBool GetIsPartAttrib3Used()
		{
			var client = Client;
			return client != null && client.PartAttributeManager.IsPartAttributeUsedByProduct(Part, 3);
		}

		protected override ZString GetPartAttrib1Name()
		{
			var client = Client;
			return client != null ? client.PartAttributeManager.PartAttributeName1 : ZString.Empty;
		}

		protected override ZString GetPartAttrib2Name()
		{
			var client = Client;
			return client != null ? client.PartAttributeManager.PartAttributeName2 : ZString.Empty;
		}

		protected override ZString GetPartAttrib3Name()
		{
			var client = Client;
			return client != null ? client.PartAttributeManager.PartAttributeName3 : ZString.Empty;
		}

		protected override ZBool GetIsTrackedSerialUsed()
		{
			var client = Client;
			return client != null && client.PartAttributeManager.IsSerialNumberUsedByProduct(Part);
		}

		#endregion

		#region Batch

		const string GS1BatchAppId = "10";

		protected override ZString GetBatch()
		{
			var result = ZString.Empty;

			var releaseLine = ReleaseLine;
			var orgPartRelation = OrgPartRelation;
			if (orgPartRelation != null && releaseLine != null)
			{
				if (orgPartRelation.OU_UsePartAttrib1 && GetApplicationIdentifier(orgPartRelation, WarehouseTargetFields.Codes.PartAttrib1) == GS1BatchAppId)
				{
					result = releaseLine.PartAttribute1;
				}
				else if (orgPartRelation.OU_UsePartAttrib2 && GetApplicationIdentifier(orgPartRelation, WarehouseTargetFields.Codes.PartAttrib2) == GS1BatchAppId)
				{
					result = releaseLine.PartAttribute2;
				}
				else if (orgPartRelation.OU_UsePartAttrib3 && GetApplicationIdentifier(orgPartRelation, WarehouseTargetFields.Codes.PartAttrib3) == GS1BatchAppId)
				{
					result = releaseLine.PartAttribute3;
				}
			}

			if (result.IsEmpty)
			{
				var miscServ = Client?.MiscServ;
				if (miscServ != null)
				{
					if (miscServ.OM_IMPartAttrib1Type == PartAttributeTypeList.Codes.BatchNumber)
					{
						result = releaseLine.PartAttribute1;
					}
					else if (miscServ.OM_IMPartAttrib2Type == PartAttributeTypeList.Codes.BatchNumber)
					{
						result = ReleaseLine.PartAttribute2;
					}
					else if (miscServ.OM_IMPartAttrib3Type == PartAttributeTypeList.Codes.BatchNumber)
					{
						result = ReleaseLine.PartAttribute3;
					}
				}
			}

			return result;
		}

		#endregion

		#region Product Stock

		protected override ZString GetProductCodeStockUnitBarcodeNumber()
		{
			var part = Part;
			var barcode = part?.PartBarcodes?.FindUseForDocumentsPartBarcodeByPackage(part.OP_StockKeepingUnit);
			return barcode != null && !barcode.PH_Barcode.IsEmpty ? barcode.PH_Barcode : Description;
		}

		protected override ZString GetProductBarcode()
		{
			var variableParts = new List<ZString>();

			var variableLength1 = new ZStringBuilder();
			if (!ProductCodeStockUnitBarcodeNumber.IsEmpty)
			{
				variableLength1.Append("02" + ProductCodeStockUnitBarcodeNumber);
			}

			if (!PackedQty.Value.IsEmpty)
			{
				variableLength1.Append("37" + PackedQty.Value.ToString(0));
			}

			if (!variableLength1.IsEmpty)
			{
				variableParts.Add(variableLength1.ToString());
			}

			var variableLength2 = new ZStringBuilder();
			if (!ExpiryForBarcode.IsEmpty)
			{
				variableLength2.Append(ExpiryIdentifier + ExpiryForBarcode);
			}

			if (!Batch.IsEmpty)
			{
				variableLength2.Append(GS1BatchAppId + Batch);
			}

			if (!variableLength2.IsEmpty)
			{
				variableParts.Add(variableLength2.ToString());
			}

			const bool USE_OPTIMISED_ENCODING = true;
			const bool IS_GS1_128_BarCode = true;

			return new TextBarcode(variableParts.ToArray(), USE_OPTIMISED_ENCODING, IS_GS1_128_BarCode).TextAs128sFontString;
		}

		protected override ZString GetProductBarcodeWithPrefixes()
		{
			var result = new ZStringBuilder();
			if (!ProductCodeStockUnitBarcodeNumber.IsEmpty)
			{
				result.Append("(02)" + ProductCodeStockUnitBarcodeNumber);
			}

			if (!PackedQty.Value.IsEmpty)
			{
				result.Append("(37)" + PackedQty.Value.ToString(0));
			}

			if (!ExpiryForBarcode.IsEmpty)
			{
				result.Append("(" + ExpiryIdentifier + ")" + ExpiryForBarcode);
			}

			if (!Batch.IsEmpty)
			{
				result.AppendFormat("({0}){1}", GS1BatchAppId, Batch);
			}

			return result.ToString();
		}

		#endregion

		protected override OrgPartRelation GetOrgPartRelation()
		{
			return Part != null && Client != null
				? Part.RelatedOrganisations.FindByOrganisationAndRelationship(Client, OrgPartRelation.RelationshipTypes.Owner)
				: null;
		}

		#region CommonCurrency

		protected override CurrencyWrapper GetCommonCurrency()
		{
			if (fCurrency == null)
			{
				var releaseLine = ReleaseLine;
				if (releaseLine != null)
				{
					var docket = releaseLine.PickableDocket;
					fCurrency = new CurrencyWrapper(RefCurrency.LoadFromCurrencyCode(Factory, docket.GetCommonValidLineCurrency()), Factory);
				}
			}

			return fCurrency;
		}
		CurrencyWrapper fCurrency;

		#endregion

		#region GetLinePrice

		protected override ZDecimal GetLinePrice()
		{
			var result = 0m;
			if (!string.IsNullOrWhiteSpace(CommonCurrency.Code))
			{
				var releaseLine = ReleaseLine;
				if (releaseLine != null)
				{
					result = releaseLine.UnitPriceAfterDiscount * PackedItems.Sum(d => d.PackedQty);
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Related Objects

		#region CustomFields

		protected override CustomLabelsProviderAndBizO GetCustomLabelsProvider()
		{
			CustomLabelsProviderAndBizO result = null;

			var releaseLine = ReleaseLine;
			if (releaseLine != null)
			{
				var docket = releaseLine.PickableDocket;
				result = docket != null ? new CustomLabelsProviderAndBizO(releaseLine.CustomFieldsForOrderLineAccessor, new WhsDocketLine.CustomLabelsProvider(docket)) : null;
			}

			return result ?? base.GetCustomLabelsProvider();
		}

		#endregion

		#region Product

		protected override GenericWrapper GetProduct()
		{
			return new ProductWrapper(Part, Factory);
		}

		#endregion

		#region ReleaseLine

		WhsReleaseLine ReleaseLine
		{
			get { return (WhsReleaseLine)PackableItemParent; }
		}

		#endregion

		#region Client

		OrgHeader Client
		{
			get { return ReleaseLine?.Client; }
		}

		#endregion

		#region Consignee

		OrgHeader Consignee
		{
			get { return ReleaseLine?.PickableDocket?.Consignee; }
		}

		#endregion

		#region Part

		OrgSupplierPart Part
		{
			get { return ReleaseLine?.SupplierPart; }
		}

		#endregion

		#region ConsigneePartRelation

		OrgPartRelation ConsigneePartRelation
		{
			get
			{
				var part = Part;
				var consignee = Consignee;
				return part != null && consignee != null ? part.RelatedOrganisations.FindByOrganisationAndRelationship(consignee, OrgPartRelation.RelationshipTypes.WarehouseConsignee) : null;
			}
		}

		#endregion

		#endregion
	}
}
