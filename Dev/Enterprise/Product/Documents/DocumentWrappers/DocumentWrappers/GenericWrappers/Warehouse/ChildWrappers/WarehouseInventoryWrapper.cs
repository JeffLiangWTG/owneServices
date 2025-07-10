using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseInventoryWrapper : WarehouseGenericLineWrapper
	{
		public WarehouseInventoryWrapper(WhsDocketLine docketLine, BusinessObjectFactory factoryToWrap)
			: base(docketLine ?? factoryToWrap.GetNull<WhsReceiveLine>(), factoryToWrap)
		{
			if (docketLine != null)
			{
				IncrementUnitValues(docketLine);
			}
		}

		#region CrossDockConsigneeAddress

		protected override AddressWrapper CrossDockConsigneeAddressCore
		{
			get { return new AddressWrapper(OrganisationUsageType.Consignee, (DocketLine as WhsReceiveLine)?.ConsigneeDocAddress, Factory); }
		}

		#endregion

		#region CrossDockOrderNumber

		protected override ZString CrossDockOrderNumberCore => DocketLine.WE_ReceiveCrossDockOrderNo;

		#endregion

		#region Product

		protected override ProductWrapper ProductCore => new ProductWrapper(DocketLine.SupplierPart, Factory);

		#endregion

		#region OrgPartRelation

		protected override OrgPartRelation OrgPartRelationCore
		{
			get => DocketLine.SupplierPart?.RelatedOrganisations.FindByOrganisationPKAndRelationship(DocketLine.Docket.WD_OH_Client, OrgPartRelation.RelationshipTypes.Owner);
		}

		#endregion

		#region Status

		protected override ZString StatusCore => DocketLine.WE_CurrentInventoryStatus;

		#endregion

		#region UnitsUQ

		protected override ZString UnitsUQCore => DocketLine != null ? DocketLine.ProductUQ : ZString.Empty;

		#endregion

		#region PacksUQ

		protected override ZString PacksUQCore => DocketLine.WE_F3_NKPackType;

		#endregion

		#region ProductCode

		protected override ZString ProductCodeCore => DocketLine.ProductCode;

		#endregion

		#region ProductCodeBarcodeNumberCore

		protected override ZString ProductCodeBarcodeNumberCore => ProductCodeTextBarcode.TextToEncode;

		#endregion

		#region ProductCodeBarcodeCore

		protected override ZString ProductCodeBarcodeCore => ProductCodeTextBarcode.TextAs128sFontString;

		#endregion

		#region ProductCodeTextBarcode

		TextBarcode ProductCodeTextBarcode
		{
			get
			{
				if (DocketLine.Product != null && DocketLine.Product.Parent != null)
				{
					var masterProductUnit = DocketLine.Product.Parent.OP_StockKeepingUnit;
					foreach (OrgSupplierPartBarcode thing in DocketLine.Product.Parent.PartBarcodes)
					{
						if (thing.PH_F3_NKPackType == masterProductUnit)
						{
							return new TextBarcode(thing.PH_Barcode);
						}
					}
				}

				return new TextBarcode(DocketLine.ProductCode);
			}
		}

		#endregion

		#region ProductDesc

		protected override ZString ProductDescriptionCore => DocketLine.SupplierPart?.OP_Desc ?? ZString.Empty;

		#endregion

		#region ProductBrandName

		protected override ZString ProductBrandNameCore => DocketLine.SupplierPart?.OP_Brand ?? ZString.Empty;

		#endregion

		#region ProductModel

		protected override ZString ProductModelCore => DocketLine.SupplierPart?.OP_Model ?? ZString.Empty;

		#endregion

		#region PartAttribute1

		protected override ZString PartAttribute1Core => DocketLine.WE_PartAttrib1;

		protected override MultilingualString PartAttribute1NameCore => DocketLine.Docket?.Client?.MiscServ?.OM_IMPartAttrib1NameMultilingual ?? (NoResString)ZString.Empty;

		protected override MultilingualString PartAttribute1WithLabelCore
		{
			get
			{
				return BuildAttribute(DocketLine.WE_PartAttrib1, PartAttribute1Name, ResString.GetMultilingualString("WarehouseInventoryWrapper|PartAttribute1WithLabelCore", "Attribute 1"));
			}
		}

		protected override ZString PartAttribute1BarcodeCore => new TextBarcode(PartAttribute1Core).TextAs128sFontString;

		#endregion

		#region PartAttribute2

		protected override ZString PartAttribute2Core => DocketLine.WE_PartAttrib2;

		protected override MultilingualString PartAttribute2NameCore => DocketLine.Docket?.Client?.MiscServ?.OM_IMPartAttrib2NameMultilingual ?? (NoResString)ZString.Empty;

		protected override MultilingualString PartAttribute2WithLabelCore
		{
			get
			{
				return BuildAttribute(DocketLine.WE_PartAttrib2, PartAttribute2Name, ResString.GetMultilingualString("b248f857-c476-46ad-8f0f-003c9b40c0ab", "Attribute 2"));
			}
		}

		protected override ZString PartAttribute2BarcodeCore => new TextBarcode(PartAttribute2Core).TextAs128sFontString;

		#endregion

		#region PartAttribute3

		protected override ZString PartAttribute3Core => DocketLine.WE_PartAttrib3;

		protected override MultilingualString PartAttribute3NameCore => DocketLine.Docket?.Client?.MiscServ?.OM_IMPartAttrib3NameMultilingual ?? (NoResString)ZString.Empty;

		protected override MultilingualString PartAttribute3WithLabelCore
		{
			get
			{
				return BuildAttribute(DocketLine.WE_PartAttrib3, PartAttribute3Name, ResString.GetMultilingualString("a03abc63-7d8a-4dbe-a104-654c8ec903fb", "Attribute 3"));
			}
		}

		protected override ZString PartAttribute3BarcodeCore => new TextBarcode(PartAttribute3Core).TextAs128sFontString;

		#endregion

		#region PartAttributeMultiple

		static ZString PartAttributeMultiple => Res.GetString("f0fda7b0-7c6d-465d-9094-8a61faad57f0", "Multiple");

		#endregion

		#region RFConfirm

		protected override ZString RFConfirmCore
		{
			get
			{
				var result = ZString.Empty;

				if (OrgPartRelation != null)
				{
					switch (OrgPartRelation.OU_RFAttributeConfirm)
					{
						case RFAttributeConfirmCode.Codes.PartAttribute1:
							result = PartAttribute1;
							break;
						case RFAttributeConfirmCode.Codes.PartAttribute2:
							result = PartAttribute2;
							break;
						case RFAttributeConfirmCode.Codes.PartAttribute3:
							result = PartAttribute3;
							break;
						case RFAttributeConfirmCode.Codes.SerialNumber:
							result = TrackedSerialNumber;
							break;
					}
				}

				return result;
			}
		}

		protected override ZString RFConfirmNameCore
		{
			get
			{
				var result = ZString.Empty;

				if (OrgPartRelation != null)
				{
					switch (OrgPartRelation.OU_RFAttributeConfirm)
					{
						case RFAttributeConfirmCode.Codes.PartAttribute1:
							result = PartAttribute1Name;
							break;
						case RFAttributeConfirmCode.Codes.PartAttribute2:
							result = PartAttribute2Name;
							break;
						case RFAttributeConfirmCode.Codes.PartAttribute3:
							result = PartAttribute3Name;
							break;
						case RFAttributeConfirmCode.Codes.SerialNumber:
							result = TrackedSerialLabel;
							break;
					}
				}

				return result;
			}
		}

		protected override ZString RFConfirmBarcodeCore => new TextBarcode(RFConfirm).TextAs128sFontString;

		#endregion

		#region LeftOverAttributes

		protected override MultilingualString LeftOverAttributesCore
		{
			get
			{
				var result = new ZStringBuilder();
				if (!PartAttribute1.IsEmpty)
				{
					result.Append(PartAttribute1Name + ": " + PartAttribute1);
				}

				if (!PartAttribute2.IsEmpty)
				{
					result.Append(PartAttribute2Name + ": " + PartAttribute2);
				}

				if (!PartAttribute3.IsEmpty)
				{
					result.Append(PartAttribute3Name + ": " + PartAttribute3);
				}

				if (!TrackedSerialNumber.IsEmpty)
				{
					result.Append(TrackedSerialLabel + ": " + TrackedSerialNumber);
				}

				return (NoResString)result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		#endregion

		#region TrackedSerialNumber

		protected override ZString TrackedSerialNumberCore => IsSerialRolledUp ? PartAttributeMultiple : DocketLine.WE_SerialNumber;

		protected override ZString TrackedSerialWithLabelCore
			=> BuildAttribute(TrackedSerialNumber, TrackedSerialLabel, string.Empty);

		protected override ZString TrackedSerialBarcodeCore
			=> new TextBarcode(TrackedSerialNumber).TextAs128sFontString;

		ZString TrackedSerialLabel => Res.GetString("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "Tracked Serial Number");

		public void MarkSerialRolledUp() => IsSerialRolledUp = true;
		bool IsSerialRolledUp;

		#endregion

		#region FirstDate

		protected override ZString FirstDateCore
		{
			get
			{
				if (!ExpiryDate.IsEmpty)
				{
					return Res.GetString("7d889aad-e5e2-46f7-953e-b91db2f204fe", "Expiry Date: {0}", ExpiryDate.ToShortDateString());
				}

				if (!PackingDate.IsEmpty)
				{
					return Res.GetString("bbed4f40-2bb6-48da-8da3-2dc925b2e9b6", "Packing Date: {0}", PackingDate.ToShortDateString());
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region SecondDate

		protected override ZString SecondDateCore
		{
			get
			{
				if (!ExpiryDate.IsEmpty && !PackingDate.IsEmpty)
				{
					return Res.GetString("15170737-7873-4aaf-8fc9-17c1226a143b", "Packing Date: {0}", PackingDate.ToShortDateString());
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region ArrivalDate

		protected override ZDateTime ArrivalDateCore => DocketLine.WE_AdjustmentArrivalDate.ToZDateTime();

		#endregion

		#region HoldCode

		protected override ZString HoldCodeCore => DocketLine.WE_WHC_NKCurrentInventoryHeldCode;

		#endregion

		#region HoldReason

		protected override ZString HoldReasonCore => DocketLine.WE_CurrentHoldReason;

		#endregion

		#region ReceivedQtyCore

		protected override ZString ReceivedQtyCore
		{
			get
			{
				if (DocketLine.SupplierPart != null)
				{
					return Utilities.Round(GroupedReceiveUnits, DocketLine.SupplierPart.OP_CountDecimalPlaces).ToString();
				}

				return GroupedReceiveUnits.ToString();
			}
		}

		#endregion

		#region InventoryQtyCore

		protected override ZString InventoryQtyCore
		{
			get
			{
				if (DocketLine.SupplierPart != null)
				{
					return Utilities.Round(GroupedInventoryUnits, DocketLine.SupplierPart.OP_CountDecimalPlaces).ToString();
				}

				return GroupedInventoryUnits.ToString();
			}
		}

		#endregion

		#region GroupedReceivedWeight

		protected override ZString GroupedReceivedWeightCore
		{
			get
			{
				if (DocketLine.Docket != null)
				{
					var part = DocketLine.SupplierPart;
					var weightUQ = DocketLine.Docket.WD_TotalWeightUnit;
					var weight = Utilities.Round(part.UnitConverter.Convert(part.OP_Weight, part.OP_WeightUQ, weightUQ) * GroupedReceiveUnits, 2);
					return new ZString(weight.ToString() + " " + weightUQ).TrimEnd();
				}

				return "0";
			}
		}

		#endregion

		#region LocationString

		protected override ZString LocationStringCore
		{
			get
			{
				ZString result = new();
				if (DocketLine != null)
				{
					result = DocketLine.LocationString;
					result = result.IsEmpty ? (ZString)"................." : result;
				}

				return result;
			}
		}

		#endregion

		#region PackingDate

		protected override ZDateTime PackingDateCore => DocketLine.WE_PackingDate;

		protected override ZString PackingDateWithLabelCore
		{
			get { return BuildAttribute(DocketLine.WE_PackingDate.ToShortDateString(), Res.GetString("7fbf528b-6bc1-411d-a91a-09c34f55a628", "Packing Date"), ""); }
		}

		protected override ZString PackingDateBarcodeCore
		{
			get
			{
				var dateFormat = OrgPartRelation != null && !OrgPartRelation.OU_PackingDateFormatString.IsEmpty ? OrgPartRelation.OU_PackingDateFormatString.ToString() : ZDateTime.ShortDateFormat;
				return new TextBarcode(PackingDate.ToString(dateFormat)).TextAs128sFontString;
			}
		}

		protected override ZString PackingDateFormattedCore
		{
			get { return PackingDate.ToString(PackingRegistry.Instance.ProductLabelDateFormat.Value); }
		}

		#endregion

		#region ExpiryDate

		protected override ZDateTime ExpiryDateCore => DocketLine.WE_ExpiryDate;

		protected override ZString ExpiryDateWithLabelCore
		{
			get { return BuildAttribute(DocketLine.WE_ExpiryDate.ToShortDateString(), Res.GetString("108d871f-cbe2-4004-a578-94e22d6daf89", "Expiry Date"), ""); }
		}

		protected override ZString ExpiryDateBarcodeCore
		{
			get
			{
				var dateFormat = OrgPartRelation != null && !OrgPartRelation.OU_ExpiryDateFormatString.IsEmpty ? OrgPartRelation.OU_ExpiryDateFormatString.ToString() : ZDateTime.ShortDateFormat;
				return new TextBarcode(ExpiryDate.ToString(dateFormat)).TextAs128sFontString;
			}
		}

		protected override ZString ExpiryDateFormattedCore
		{
			get { return ExpiryDate.ToString(PackingRegistry.Instance.ProductLabelDateFormat.Value); }
		}

		#endregion

		#region WeightUQ

		protected override ZString WeightUQCore => DocketLine.SupplierPart?.OP_WeightUQ ?? ZString.Empty;

		#endregion

		#region VolumeUQ

		protected override ZString VolumeUQCore => DocketLine.SupplierPart?.OP_CubicUQ ?? ZString.Empty;

		#endregion

		#region Volume

		protected override ZDecimal VolumeCore => DocketLine.WE_TransactionQuantity * (DocketLine.SupplierPart?.OP_Cubic ?? 0);

		#endregion

		#region PalletID

		protected override ZString PalletIDCore => DocketLine.WE_PalletID;

		#endregion

		#region PalletIDBarcode

		protected override ZString PalletIDBarcodeCore => new TextBarcode(DocketLine.WE_PalletID).TextAs128sFontString;

		#endregion

		#region LocationColumn

		protected override ZShort LocationColumnCore => DocketLine.Location?.WLV_Column ?? ZShort.Zero;

		#endregion

		#region LocationLevel

		protected override ZShort LocationLevelCore => DocketLine.Location?.WLV_Level ?? ZShort.Zero;

		#endregion

		#region Weight

		protected override ZDecimal WeightCore => DocketLine.WE_TransactionQuantity * (DocketLine.SupplierPart?.OP_Weight ?? ZDecimal.Zero);

		#endregion

		#region PackQty

		protected override ZDecimal PackQtyCore => DocketLine.WE_PackQuantity;

		#endregion

		#region ExpectedReceiptQuantity

		protected override ZDecimal ExpectedReceiptQuantityCore => DocketLine.WE_ClientOrderedUnits;

		#endregion

		#region VarianceCore

		protected override ZDecimal VarianceCore => Units - ExpectedReceiptQuantity;

		#endregion

		#region Units

		protected override ZDecimal UnitsCore => DocketLine.WE_TransactionQuantity;

		#endregion

		#region Pallets

		protected override ZDecimal PalletsCore
		{
			get
			{
				var result = ZDecimal.Zero;
				if (DocketLine.SupplierPart != null)
				{
					if (!DocketLine.SupplierPart.OP_StockKeepingUnitPerPallet.IsEmpty && DocketLine.SupplierPart.OP_StockKeepingUnitPerPallet != ZDecimal.Zero)
					{
						result = Utilities.Round(DocketLine.WE_TransactionQuantity / DocketLine.SupplierPart.OP_StockKeepingUnitPerPallet, 1);
					}
				}
				return result;
			}
		}

		#endregion

		#region GroupedUnits

		public override ZDecimal GroupedUnits
		{
			get { return groupedUnits; }
			set { groupedUnits = value; }
		}
		ZDecimal groupedUnits;

		#endregion

		#region Customs Fields

		protected override ZString CustomsEntryKeyCore
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

		protected override ZString CustomsEntryNoCore => DocketLine.CustomsData.WB_EntryKey;

		protected override ZShort CustomsEntryLineNoCore => DocketLine.CustomsData.WB_EntryLineNo;

		protected override ZString CustomsTariffItemCore => DocketLine.CustomsTariffItem;

		protected override ZDateTime CustomsEntryDateCore => DocketLine.CustomsData.WB_EntryDate;

		protected override ZDecimal CustomsQuantityCore => DocketLine.CustomsData.WB_CustomsQty;

		protected override ZString CustomsQuantityUQCore => DocketLine.CustomsData.WB_CustomsUnitOfQty;

		protected override ZDecimal CustomsVFDCore => DocketLine.CustomsData.WB_ValueForDuty;

		protected override ZDecimal CustomsTILVCore => DocketLine.CustomsData.WB_TILV;

		protected override ZString CustomsCtryOfOriginCore => DocketLine.CustomsData.WB_RN_NKCountryOfOrigin;

		protected override ZString CustomsAddInfoCore => DocketLine.CustomsData.WB_AddInfo;

		protected override ZDecimal CustomsSecondQuantityCore => DocketLine.CustomsData.WB_CustomsSecondQuantity;

		protected override ZString CustomsSecondUnitQtyCore => DocketLine.CustomsData.WB_CustomsSecondUnitQty;

		protected override ZString TariffCore => DocketLine.CustomsData.WB_Tariff;

		protected override ZString PrimaryPreferenceCore => DocketLine.CustomsData.WB_PrimaryPreference;

		protected override ZDecimal CustomsThirdQuantityCore => DocketLine.CustomsData.WB_CustomsThirdQuantity;

		protected override ZString CustomsThirdUnitQtyCore => DocketLine.CustomsData.WB_CustomsThirdUnitQty;

		#endregion

		#region Custom Attributes

		protected override ZString CustomAttrib1Core => DocketLine.WE_CustomAttrib1;

		protected override ZString CustomAttrib2Core => DocketLine.WE_CustomAttrib2;

		protected override ZString CustomAttrib3Core => DocketLine.WE_CustomAttrib3;

		protected override ZString CustomAttrib4Core => DocketLine.WE_CustomAttrib4;

		protected override ZString CustomAttrib5Core => DocketLine.WE_CustomAttrib5;

		protected override ZString CustomAttrib6Core => DocketLine.WE_CustomAttrib6;

		protected override ZDateTime CustomDate1Core => DocketLine.WE_CustomDate1;

		protected override ZDateTime CustomDate2Core => DocketLine.WE_CustomDate2;

		protected override ZDateTime CustomDate3Core => DocketLine.WE_CustomDate3;

		protected override ZDateTime CustomDate4Core => DocketLine.WE_CustomDate4;

		protected override ZDateTime CustomDate5Core => DocketLine.WE_CustomDate5;

		protected override ZDecimal CustomDecimal1Core => DocketLine.WE_CustomDecimal1;

		protected override ZDecimal CustomDecimal2Core => DocketLine.WE_CustomDecimal2;

		protected override ZDecimal CustomDecimal3Core => DocketLine.WE_CustomDecimal3;

		protected override ZDecimal CustomDecimal4Core => DocketLine.WE_CustomDecimal4;

		protected override ZDecimal CustomDecimal5Core => DocketLine.WE_CustomDecimal5;

		protected override ZBool CustomFlag1Core => DocketLine.WE_CustomFlag1;

		protected override ZBool CustomFlag2Core => DocketLine.WE_CustomFlag2;

		protected override ZBool CustomFlag3Core => DocketLine.WE_CustomFlag3;

		protected override ZBool CustomFlag4Core => DocketLine.WE_CustomFlag4;

		protected override ZBool CustomFlag5Core => DocketLine.WE_CustomFlag5;

		protected override ZString CustomTextBlob1Core => DocketLine.WE_CustomTextBlob1;

		#endregion

		#region LineNoCore

		protected override ZShort LineNoCore => DocketLine?.WE_LineNo ?? 0;

		#endregion

		#region Implementation

		WhsDocketLine DocketLine => (WhsDocketLine)WrappedObject;

		internal void IncrementUnitValues(WhsDocketLine docketLine)
		{
			GroupedReceiveUnits += docketLine.WE_TransactionQuantity; // change to properly calculate A.V
			GroupedInventoryUnits += docketLine.WE_StockOnHand;
		}

		#endregion
	}
}
