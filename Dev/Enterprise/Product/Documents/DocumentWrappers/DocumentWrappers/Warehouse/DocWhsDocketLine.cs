using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public abstract class DocWhsDocketLine : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsDocketLine(WhsDocketLine whsDocketLine, BusinessObjectFactory factoryToWrap)
			: base(whsDocketLine, factoryToWrap)
		{
		}

		protected WhsDocketLine WhsDocketLine
		{
			get { return (WhsDocketLine)WrappedObject; }
		}

		#endregion

		#region Related Business Objects

		public abstract DocWhsDocket Docket { get; }

		#endregion

		#region Business Objects Overrides

		public override string ToString()
		{
			return ProductCode;
		}

		#endregion

		#region Wrapper Fields

		public OrgPartRelation OrgPartRelation
		{
			get
			{
				return WhsDocketLine.SupplierPart != null && !WhsDocketLine.Docket.WD_OH_Client.IsEmpty
					? WhsDocketLine.SupplierPart.RelatedOrganisations.FindByOrganisationPKAndRelationship(WhsDocketLine.Docket.WD_OH_Client, OrgPartRelation.RelationshipTypes.Owner)
					: null;
			}
		}

		public DocOrgSupplierPart Product
		{
			get { return DocOrgSupplierPart.New(WhsDocketLine.SupplierPart, Factory); }
		}

		public UNDGSubstanceWrapper DGSubstance
		{
			get
			{
				var supplierPart = WhsDocketLine.SupplierPart;
				return supplierPart != null && supplierPart.UNDGs.Count > 0 && supplierPart.UNDGs[0].Substance != null ? new UNDGSubstanceWrapper(supplierPart.UNDGs[0], Factory) : null;
			}
		}

		#endregion

		#region Number Fields

		#region LineNo

		public ZInt LineNo
		{
			get { return LineNoCore; }
		}

		protected virtual ZInt LineNoCore
		{
			get { return WhsDocketLine.WE_LineNo; }
		}

		#endregion

		#region Units

		public ZDecimal Units
		{
			get { return UnitsCore; }
		}

		protected virtual ZDecimal UnitsCore
		{
			get { return WhsDocketLine.WE_TransactionQuantity; }
		}

		#endregion

		public ZDecimal Volume
		{
			get { return ZArchitecture.Core.Utilities.Round(WhsDocketLine.SupplierPart.UnitConverter.Convert(WhsDocketLine.SupplierPart.OP_Cubic, WhsDocketLine.SupplierPart.OP_CubicUQ, WhsDocketLine.Docket.WD_TotalCubicUnit) * Units, 4); }
		}

		public ZString VolumeUQ
		{
			get { return WhsDocketLine.SupplierPart.OP_CubicUQ; }
		}

		public ZDecimal Weight
		{
			get { return ZArchitecture.Core.Utilities.Round(WhsDocketLine.SupplierPart.UnitConverter.Convert(WhsDocketLine.SupplierPart.OP_Weight, WhsDocketLine.SupplierPart.OP_WeightUQ, WhsDocketLine.Docket.WD_TotalWeightUnit) * Units, 2); }
		}

		public ZString WeightUQ
		{
			get { return WhsDocketLine.SupplierPart.OP_WeightUQ; }
		}

		#region UnitsMet

		public ZDecimal UnitsMet
		{
			get { return UnitsMetCore; }
		}

		protected virtual ZDecimal UnitsMetCore
		{
			get { return WhsDocketLine.WE_TransactionQuantity; }
		}

		#endregion

		public ZDecimal CustomDecimal1
		{
			get { return WhsDocketLine.WE_CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return WhsDocketLine.WE_CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return WhsDocketLine.WE_CustomDecimal3; }
		}

		public ZDecimal CustomDecimal4
		{
			get { return WhsDocketLine.WE_CustomDecimal4; }
		}

		public ZDecimal CustomDecimal5
		{
			get { return WhsDocketLine.WE_CustomDecimal5; }
		}

		#region PackQty

		public ZDecimal PackQty
		{
			get { return PackQtyCore; }
		}

		protected virtual ZDecimal PackQtyCore
		{
			get { return WhsDocketLine.WE_PackQuantity; }
		}

		#endregion

		// Why there are 2 Packs Properties???
		public ZDecimal Packs
		{
			get { return PackQtyCore; }
		}

		#endregion

		#region ZString Fields

		#region UnitsBarcode

		public ZString UnitsBarcode
		{
			get
			{
				var barcode = new TextBarcode(Units.ToString(2));
				return barcode.TextAs128sFontString;
			}
		}

		#endregion

		#region UnitsMetBarcode

		public ZString UnitsMetBarcode
		{
			get
			{
				var barcode = new TextBarcode(UnitsMet.ToString(2));
				return barcode.TextAs128sFontString;
			}
		}

		#endregion

		#region ProductCode

		public ZString ProductCode
		{
			get
			{
				var supplierPart = WhsDocketLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_PartNum : ZString.Empty;
			}
		}

		#endregion

		#region ProductBrandName

		public ZString ProductBrandName
		{
			get
			{
				var supplierPart = WhsDocketLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Brand : ZString.Empty;
			}
		}

		#endregion

		#region ProductModel

		public ZString ProductModel
		{
			get
			{
				var supplierPart = WhsDocketLine.SupplierPart;
				return (supplierPart != null) ? supplierPart.OP_Model : ZString.Empty;
			}
		}

		#endregion

		public ZString CustomAttrib1
		{
			get { return WhsDocketLine.WE_CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return WhsDocketLine.WE_CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return WhsDocketLine.WE_CustomAttrib3; }
		}

		public ZString CustomAttrib4
		{
			get { return WhsDocketLine.WE_CustomAttrib4; }
		}

		public ZString CustomAttrib5
		{
			get { return WhsDocketLine.WE_CustomAttrib5; }
		}

		public ZString CustomAttrib6
		{
			get { return WhsDocketLine.WE_CustomAttrib6; }
		}

		public ZString PackType
		{
			get { return WhsDocketLine.WE_F3_NKPackType; }
		}

		public ZString LineComment
		{
			get { return WhsDocketLine.WE_LineComment; }
		}

		public virtual ZString Location1
		{
			get { return (WhsDocketLine as WhsTransferLine)?.TransferFromLocationString ?? ZString.Empty; }
		}

		public virtual ZString Location2
		{
			get { return WhsDocketLine.LocationString; }
		}

		public virtual ZString UnitsUQ
		{
			get { return WhsDocketLine.ProductUQ; }
		}

		#region PartAttributes

		#region PartAttribute1Name

		public MultilingualString PartAttribute1Name
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";
				DocWhsDocket docket = Docket;
				if (docket != null)
				{
					DocOrganisation client = docket.Client;
					if (client != null && client.MiscServ != null)
					{
						attributeName = client.MiscServ.IMPartAttrib1Name;
					}
				}
				return attributeName;
			}
		}

		#endregion

		#region PartAttribute1Barcode

		public ZString PartAttribute1Barcode
		{
			get
			{
				var barcode = new TextBarcode(this.PartAttribute1);
				return barcode.TextAs128sFontString;
			}
		}

		#endregion

		#region PartAttribute1

		public ZString PartAttribute1
		{
			get { return WhsDocketLine.WE_PartAttrib1; }
		}

		#endregion

		#region PartAttribute2

		public ZString PartAttribute2
		{
			get { return WhsDocketLine.WE_PartAttrib2; }
		}

		#endregion

		#region PartAttribute3

		public ZString PartAttribute3
		{
			get { return WhsDocketLine.WE_PartAttrib3; }
		}

		#endregion

		#region TrackedSerialNumber

		public ZString TrackedSerialNumber
		{
			get { return WhsDocketLine.WE_SerialNumber; }
		}

		#endregion

		#region PartAttribute1WithLabel

		public MultilingualString PartAttribute1WithLabel
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";
				WhsDocket docket = WhsDocketLine.Docket;
				if (docket != null)
				{
					OrgHeader client = docket.Client;
					if (client != null && client.MiscServ != null)
					{
						attributeName = client.MiscServ.OM_IMPartAttrib1NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute1, attributeName, ResString.GetMultilingualString("4300cd27-d0c9-49d8-9aff-9bda042a123e", "Attribute 1"));
			}
		}

		#endregion

		#region PartAttribute2WithLabel

		public MultilingualString PartAttribute2WithLabel
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";
				WhsDocket docket = WhsDocketLine.Docket;
				if (docket != null)
				{
					OrgHeader client = docket.Client;
					if (client != null && client.MiscServ != null)
					{
						attributeName = client.MiscServ.OM_IMPartAttrib2NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute2, attributeName, ResString.GetMultilingualString("6121cba3-f0b1-4b6f-a0b4-c50ddd4d06fc", "Attribute 2"));
			}
		}

		#endregion

		#region PartAttribute3WithLabel

		public MultilingualString PartAttribute3WithLabel
		{
			get
			{
				MultilingualString attributeName = (NoResString)"";
				WhsDocket docket = WhsDocketLine.Docket;
				if (docket != null)
				{
					OrgHeader client = docket.Client;
					if (client != null && client.MiscServ != null)
					{
						attributeName = client.MiscServ.OM_IMPartAttrib3NameMultilingual;
					}
				}
				return BuildAttribute(PartAttribute3, attributeName, ResString.GetMultilingualString("7d6d15a5-29af-47f4-8585-ab630e9fe9cf", "Attribute 3"));
			}
		}

		#endregion

		#region TrackedSerialWithLabel

		public MultilingualString TrackedSerialWithLabel
		{
			get { return BuildAttribute(TrackedSerialNumber, ResString.GetMultilingualString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number"), (NoResString)""); }
		}

		#endregion

		#region PackingDateWithLabel

		public ZString PackingDateWithLabel
		{
			get { return BuildAttribute(WhsDocketLine.WE_PackingDate.ToShortDateString(), Res.GetString("662124ea-3f48-4f90-97d2-22a3216f6907", "Packing Date"), ""); }
		}

		#endregion

		#region ExpiryDateWithLabel

		public ZString ExpiryDateWithLabel
		{
			get { return BuildAttribute(WhsDocketLine.WE_ExpiryDate.ToShortDateString(), Res.GetString("4b1dc3a6-20e0-4634-a10b-82121099b7e5", "Expiry Date"), ""); }
		}

		#endregion

		#endregion

		#region PalletsSent

		public virtual ZShort PalletsSent
		{
			get { return WhsDocketLine.Docket.WD_PalletsSent; }
		}

		#endregion

		#region TotalPallets

		public ZShort TotalPallets
		{
			get { return WhsDocketLine.Docket.WD_TotalPallets; }
		}

		#endregion

		#region PackagesSent

		public virtual ZInt PackagesSent
		{
			get { return WhsDocketLine.Docket.WD_PackagesSent; }
		}

		#endregion

		#endregion

		#region ZDateTime Fields

		public ZDateTime CustomDate1
		{
			get { return WhsDocketLine.WE_CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return WhsDocketLine.WE_CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return WhsDocketLine.WE_CustomDate3; }
		}

		public ZDateTime CustomDate4
		{
			get { return WhsDocketLine.WE_CustomDate4; }
		}

		public ZDateTime CustomDate5
		{
			get { return WhsDocketLine.WE_CustomDate5; }
		}

		#region ExpiryDate

		public ZDateTime ExpiryDate
		{
			get { return WhsDocketLine.WE_ExpiryDate; }
		}

		#endregion

		#region PackingDate

		public ZDateTime PackingDate
		{
			get { return WhsDocketLine.WE_PackingDate; }
		}

		#endregion

		#endregion

		#region Bool Fields

		public ZBool CustomFlag1
		{
			get { return WhsDocketLine.WE_CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return WhsDocketLine.WE_CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return WhsDocketLine.WE_CustomFlag3; }
		}

		public ZBool CustomFlag4
		{
			get { return WhsDocketLine.WE_CustomFlag4; }
		}

		public ZBool CustomFlag5
		{
			get { return WhsDocketLine.WE_CustomFlag5; }
		}

		#endregion

		#region Customs Fields

		public ZString CustomsEntryKey
		{
			get
			{
				ZString result = CustomsEntryNo + " / " + CustomsEntryLineNo;
				if (result == " / 0")
				{
					result = "";
				}

				return result;
			}
		}

		public ZString CustomsEntryNo
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZString.Empty : customsData.WB_EntryKey;
			}
		}

		public ZShort CustomsEntryLineNo
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZShort.Zero : customsData.WB_EntryLineNo;
			}
		}

		public ZString CustomsTariffItem
		{
			get { return WhsDocketLine.CustomsTariffItem; }
		}

		public ZDateTime CustomsEntryDate
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZDateTime.Empty : customsData.WB_EntryDate;
			}
		}

		public ZDecimal CustomsQuantity
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZDecimal.Zero : customsData.WB_CustomsQty;
			}
		}

		public ZString CustomsQuantityUQ
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZString.Empty : customsData.WB_CustomsUnitOfQty;
			}
		}

		public ZDecimal CustomsVFD
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZDecimal.Zero : customsData.WB_ValueForDuty;
			}
		}

		public ZDecimal CustomsTILV
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZDecimal.Zero : customsData.WB_TILV;
			}
		}

		public ZString CustomsCtryOfOrigin
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZString.Empty : customsData.WB_RN_NKCountryOfOrigin;
			}
		}

		public ZString CustomsAddInfo
		{
			get
			{
				var customsData = WhsDocketLine.CustomsData;
				return customsData == null ? ZString.Empty : customsData.WB_AddInfo;
			}
		}

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

		MultilingualString BuildAttribute(ZString attributeValue, MultilingualString attributeName, MultilingualString alternateAttributeName)
		{
			if (attributeName.IsEmpty)
			{
				attributeName = alternateAttributeName;
			}

			return attributeValue.IsEmpty ? (NoResString)ZString.Empty : MultilingualString.Join(": ", attributeName, (NoResString)attributeValue);
		}

		#endregion
	}
}
