using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickLine : DocBaseWrapper, IPickingSlipLineWrapper
	{
		#region Constructors

		protected DocWhsPickLine(WhsPickLine pickLine, BusinessObjectFactory factoryToWrap)
			: base(pickLine, factoryToWrap)
		{
			units = pickLine.WZ_Units;
		}

		#endregion

		#region Static

		public static DocWhsPickLine New(WhsPickLine pickLine, BusinessObjectFactory factoryToWrap)
		{
			return (pickLine == null) ? null : new DocWhsPickLine(pickLine, factoryToWrap);
		}

		#endregion

		#region Related Business Objects

		WhsPickLine PickLine
		{
			get { return (WhsPickLine)WrappedObject; }
		}

		OrgHeader Client
		{
			get { return PickLine.InventoryLine.Docket.Client; }
		}

		OrgSupplierPart SupplierPart
		{
			get { return PickLine.SupplierPart; }
		}

		WhsLocation Location
		{
			get { return PickLine.InventoryLineForAvailableInventory.Location; }
		}

		#endregion

		#region Properties

		public ZString UnitsUQ
		{
			get { return PickLine.InventoryLine.ProductUQ; }
		}

		#region Part Attributes

		#region PackingDate

		public ZDateTime PackingDate
		{
			get
			{
				OrgHeader client = Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(SupplierPart)
					 ? PickLine.DocketLine.WE_PackingDate
					 : PickLine.InventoryLine.WE_PackingDate;
			}
		}

		#endregion

		#region ExpiryDate

		public ZDateTime ExpiryDate
		{
			get
			{
				OrgHeader client = Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(SupplierPart)
					? PickLine.DocketLine.WE_ExpiryDate
					: PickLine.InventoryLine.WE_ExpiryDate;
			}
		}

		#endregion

		#region PackingDateWithLabel

		public ZString PackingDateWithLabel
		{
			get { return BuildAttribute(PackingDate.ToShortDateString(), Res.GetString("56646db1-f80d-4cd5-9ec1-b811f9bfc560", "Packing Date"), ""); }
		}

		#endregion

		#region ExpiryDateWithLabel

		public ZString ExpiryDateWithLabel
		{
			get { return BuildAttribute(ExpiryDate.ToShortDateString(), Res.GetString("cb308ca9-7467-4ff7-b902-c522c4758d85", "Expiry Date"), ""); }
		}

		#endregion

		#region PartAttribute1

		public ZString PartAttribute1
		{
			get
			{
				var client = Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(SupplierPart)
					? PickLine.DocketLine.WE_PartAttrib1
					: PickLine.InventoryLine.WE_PartAttrib1;
			}
		}

		#endregion

		#region PartAttribute2

		public ZString PartAttribute2
		{
			get
			{
				var client = Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(SupplierPart)
					? PickLine.DocketLine.WE_PartAttrib2
					: PickLine.InventoryLine.WE_PartAttrib2;
			}
		}

		#endregion

		#region PartAttribute3

		public ZString PartAttribute3
		{
			get
			{
				var client = Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(SupplierPart)
					? PickLine.DocketLine.WE_PartAttrib3
					: PickLine.InventoryLine.WE_PartAttrib3;
			}
		}

		#endregion

		#region TrackedSerialNumber

		public ZString TrackedSerialNumber
		{
			get
			{
				var client = Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(SupplierPart)
					? PickLine.DocketLine.WE_SerialNumber
					: PickLine.InventoryLine.WE_SerialNumber;
			}
		}

		#endregion

		#region PartAttribute1WithLabel

		public ZString PartAttribute1WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (Client != null)
				{
					if (Client.MiscServ != null)
					{
						attributeName = Client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute1, attributeName, Res.GetString("af06d5ad-a114-47a3-a18c-aba50e8eddf6", "Attribute 1"));
			}
		}

		#endregion

		#region PartAttribute2WithLabel

		public ZString PartAttribute2WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (Client != null)
				{
					if (Client.MiscServ != null)
					{
						attributeName = Client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute2, attributeName, Res.GetString("dd9ea523-88b6-44a6-8e27-d43c4ab2e054", "Attribute 2"));
			}
		}

		#endregion

		#region PartAttribute3WithLabel

		public ZString PartAttribute3WithLabel
		{
			get
			{
				ZString attributeName = ZString.Empty;
				if (Client != null)
				{
					if (Client.MiscServ != null)
					{
						attributeName = Client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute3, attributeName, Res.GetString("c050abfe-6f08-498f-aecf-9023a03d102f", "Attribute 3"));
			}
		}

		#endregion

		#region TrackedSerialWithLabel 

		public ZString TrackedSerialWithLabel => BuildAttribute(TrackedSerialNumber, Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number"), string.Empty);

		#endregion

		#endregion

		public ZString LocationString
		{
			get { return PickLine.InventoryLineForAvailableInventory.LocationString; }
		}

		#region ProductCode

		public ZString ProductCode
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_PartNum : ZString.Empty;
			}
		}

		#endregion

		#region ProductDesc

		public ZString ProductDesc
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_Desc : ZString.Empty;
			}
		}

		#endregion

		#region ProductDesc2

		public ZString ProductDesc2
		{
			get
			{
				var supplierPart = this.SupplierPart;

				ZString result = ZString.Empty;
				if (supplierPart != null)
				{
					OrgPartRelation partRelation = supplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(Client.PK, OrgPartRelation.RelationshipTypes.Owner);
					result = partRelation != null ? partRelation.OU_LocalPartDescription : result;
				}
				return result;
			}
		}

		#endregion

		#region ProductBrandName

		public ZString ProductBrandName
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_Brand : ZString.Empty;
			}
		}

		#endregion

		#region ProductModel

		public ZString ProductModel
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_Model : ZString.Empty;
			}
		}

		#endregion

		public ZString ExtraDetails
		{
			get
			{
				ZString releaseUnitsAndUQ = ReleaseUnitsAndUQ;
				return
					Weight.ToString() + " " + WeightUQ + "; " +
					Volume.ToString() + " " + VolumeUQ + "; " +
					(releaseUnitsAndUQ.IsEmpty ? "" : releaseUnitsAndUQ + "; ") +
					Res.GetString("b9f97381-1a9c-46cf-b1ad-763e257f6d7e", "{0} PLT", Pallets.ToString());
			}
		}

		#region WeightUQ

		public ZString WeightUQ
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_WeightUQ : ZString.Empty;
			}
		}

		#endregion

		#region VolumeUQ

		public ZString VolumeUQ
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? supplierPart.OP_CubicUQ : ZString.Empty;
			}
		}

		#endregion

		public ZString ClientName
		{
			get { return Client != null ? Client.OH_FullName : ZString.Empty; }
		}

		public ZString ClientCode
		{
			get { return Client != null ? Client.OH_Code : ZString.Empty; }
		}

		public ZString PickMethod
		{
			get
			{
				var location = Location;
				return location?.PickMethods.GetDescriptionFromCode(location.WLV_PickMethod) ?? "";
			}
		}

		public ZString PalletID
		{
			get { return PickLine.InventoryLineForAvailableInventory.WE_PalletID; }
		}

		#region Weight

		public ZDecimal Weight
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? ZArchitecture.Core.Utilities.Round(Units * supplierPart.OP_Weight, 2) : 0m;
			}
		}

		#endregion

		#region Volume

		public ZDecimal Volume
		{
			get
			{
				var supplierPart = this.SupplierPart;
				return supplierPart != null ? ZArchitecture.Core.Utilities.Round(Units * supplierPart.OP_Cubic, 4) : 0m;
			}
		}

		#endregion

		#region Pallets

		public ZDecimal Pallets
		{
			get
			{
				ZDecimal result = 0m;
				var supplierPart = this.SupplierPart;
				if (supplierPart != null)
				{
					if (!supplierPart.OP_StockKeepingUnitPerPallet.IsEmpty && supplierPart.OP_StockKeepingUnitPerPallet != 0m)
					{
						result = ZArchitecture.Core.Utilities.Round(Units / supplierPart.OP_StockKeepingUnitPerPallet, 1);
					}
				}
				return result;
			}
		}

		#endregion

		#region Units

		public ZDecimal Units
		{
			get { return units; }
			set { units = value; }
		}

		#endregion

		#region ReleaseUnitsAndUQ

		public ZString ReleaseUnitsAndUQ
		{
			get
			{
				ZString result = "";

				var supplierPart = SupplierPart;
				var product = PickLine.InventoryLine.Product;
				var orderLine = PickLine.DocketLine;

				if (supplierPart != null && product != null && orderLine != null)
				{
					var docket = orderLine.Docket;
					var productParams = product.GetParamsByWhsAndClient(docket.Warehouse, docket.Client);
					if (productParams != null && !productParams.W3_F3_NKReleasedPackType.IsEmpty)
					{
						var convertedUnits = supplierPart.UnitConverter.Convert(Units, supplierPart.OP_StockKeepingUnit, productParams.W3_F3_NKReleasedPackType);
						var unitOfQty = (convertedUnits > 1) ? Grammar.Instance.Pluralize(productParams.W3_F3_NKReleasedPackType) : productParams.W3_F3_NKReleasedPackType.ToString();

						result = convertedUnits.Round(1) + " " + unitOfQty;
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Implementation

		ZString BuildAttribute(ZString attributeValue, ZString attributeName, ZString alternateAttributeName)
		{
			if (attributeName.IsEmpty)
			{
				attributeName = alternateAttributeName;
			}

			return attributeValue.IsEmpty ? ZString.Empty : new ZString(attributeName.ToString() + ": " + attributeValue.ToString());
		}

		ZDecimal units;

		#endregion
	}
}
