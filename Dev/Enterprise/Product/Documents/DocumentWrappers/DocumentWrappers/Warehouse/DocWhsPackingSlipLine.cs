using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	[AllowNoStaticNew]
	public class DocWhsPackingSlipLine : DocBaseWrapper, IPackingSlipWrapper
	{
		protected DocWhsPackingSlipLine(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, BusinessObjectFactory factoryToWrap)
			: base(releaseLine, factoryToWrap)
		{
			OrderLineBizO = orderLine;
		}

		readonly WhsPickableDocketLine OrderLineBizO;

		public static DocWhsPackingSlipLine New(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, BusinessObjectFactory factoryToWrap)
		{
			DocWhsPackingSlipLine result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(releaseLine, orderLine, factoryToWrap);
			}
			else if (releaseLine != null)
			{
				result = new DocWhsPackingSlipLine(releaseLine, orderLine, factoryToWrap);
			}

			return result;
		}

		protected delegate DocWhsPackingSlipLine NewDelegate(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine, BusinessObjectFactory factoryToWrap);

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region Related Business Objects

		WhsReleaseLine ReleaseLine
		{
			get { return (WhsReleaseLine)WrappedObject; }
		}

		WhsPickableDocket Order
		{
			get { return ReleaseLine.PickableDocket; }
		}

		OrgSupplierPart SupplierPart
		{
			get { return ReleaseLine.SupplierPart; }
		}

		#region RolledUpParents

		public void AddParentToRollUp(WhsPickableDocketLine parent)
		{
			if (!RolledUpParents.Contains(parent))
			{
				RolledUpParents.Add(parent);
			}
		}

		public bool ContainsRolledUpParent(WhsPickableDocketLine parent)
		{
			foreach (var line in RolledUpParents)
			{
				if (line.PK == parent.PK)
				{
					return true;
				}
			}
			return false;
		}

		List<WhsPickableDocketLine> RolledUpParents
		{
			get
			{
				if (rolledUpParents == null)
				{
					rolledUpParents = new List<WhsPickableDocketLine>();

					var parent = OrderLineBizO;
					if (parent != null)
					{
						rolledUpParents.Add(parent);
					}
				}

				return rolledUpParents;
			}
		}

		List<WhsPickableDocketLine> rolledUpParents;

		#endregion

		#endregion

		#region Wrapper Fields

		public DocWhsPickableDocketLine OrderLine
		{
			get { return DocWhsPickableDocketLine.New(OrderLineBizO, Factory); }
		}

		public DocOrgSupplierPart Product
		{
			get { return DocOrgSupplierPart.New(SupplierPart, Factory); }
		}

		public UNDGSubstanceWrapper DGSubstance
		{
			get { return SupplierPart != null && SupplierPart.UNDGs.Count > 0 && SupplierPart.UNDGs[0].Substance != null ? new UNDGSubstanceWrapper(SupplierPart.UNDGs[0], Factory) : null; }
		}

		public DocRefNMFC NMFC
		{
			get { return SupplierPart != null && SupplierPart.CommodityCode != null ? DocRefNMFC.New(SupplierPart.CommodityCode.NMFC, Factory) : null; }
		}

		#endregion

		#region ZGuid Fields

		public ZGuid LinePK
		{
			get { return OrderLineBizO.PK; }
		}

		#endregion

		#region ZDateTime Fields

		#region PackingDate

		public ZDateTime PackingDate
		{
			get
			{
				var client = ReleaseLine.Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(ReleaseLine.SupplierPart)
					? ReleaseLine.OrderedPackingDate
					: ReleaseLine.PackingDate;
			}
		}

		#endregion

		#region ExpiryDate

		public ZDateTime ExpiryDate
		{
			get
			{
				var client = ReleaseLine.Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(ReleaseLine.SupplierPart)
					? ReleaseLine.OrderedExpiryDate
					: ReleaseLine.ExpiryDate;
			}
		}

		#endregion

		#endregion

		#region ZString Fields

		public ZString CurrencySymbol
		{
			get
			{
				ZString result = ZString.Empty;

				var unitPriceCurrency = OrderLineBizO != null ? OrderLineBizO.UnitPriceCurrency : null;
				if (unitPriceCurrency != null)
				{
					result = unitPriceCurrency.RX_Symbol;
				}
				else
				{
					var docket = ReleaseLine.PickableDocket;
					if (docket != null
						&& docket.Client != null
						&& docket.Client.Country != null
						&& docket.Client.Country.LocalCurrency != null)
					{
						result = docket.Client.Country.LocalCurrency.RX_Symbol;
					}
				}

				return result;
			}
		}

		public ZString UnitPriceCurrency
		{
			get { return OrderLineBizO != null ? OrderLineBizO.WE_RX_NKUnitPriceCurrency : ZString.Empty; }
		}

		public ZString RecommendedUnitPriceWithSymbol
		{
			get { return CurrencySymbol + RecommendedUnitPrice.ToString("0.00"); }
		}

		public ZString UnitDiscountAmountWithSymbol
		{
			get { return CurrencySymbol + UnitDiscountAmount.ToString("0.00"); }
		}

		public ZString UnitPriceAfterDiscountWithSymbol
		{
			get { return CurrencySymbol + UnitPriceAfterDiscount.ToString("0.00"); }
		}

		public ZString ExtendedLinePriceWithSymbol
		{
			get
			{
				ZString result = ZString.Empty;
				if (ExtendedLinePrice != ZDecimal.Zero)
				{
					result = CurrencySymbol + ExtendedLinePrice.ToString("0.00");
				}
				return result;
			}
		}

		public ZString UnitsUQ
		{
			get { return BOMLevel == 0 ? ReleaseLine.UnitsUQ : ZString.Empty; }
		}

		#region PartAttrib1

		public ZString PartAttrib1
		{
			get
			{
				var client = ReleaseLine.Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(ReleaseLine.SupplierPart)
					? ReleaseLine.OrderedPartAttribute1
					: ReleaseLine.PartAttribute1;
			}
		}

		#endregion

		#region PartAttrib2

		public ZString PartAttrib2
		{
			get
			{
				var client = ReleaseLine.Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(ReleaseLine.SupplierPart)
					? ReleaseLine.OrderedPartAttribute2
					: ReleaseLine.PartAttribute2;
			}
		}

		#endregion

		#region PartAttrib3

		public ZString PartAttrib3
		{
			get
			{
				var client = ReleaseLine.Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(ReleaseLine.SupplierPart)
					? ReleaseLine.OrderedPartAttribute3
					: ReleaseLine.PartAttribute3;
			}
		}

		#endregion

		#region TrackedSerialNumber

		public ZString TrackedSerialNumber
		{
			get
			{
				var client = ReleaseLine.Client;
				return client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(ReleaseLine.SupplierPart)
					? ReleaseLine.OrderedSerialNumber
					: ReleaseLine.SerialNumber;
			}
		}

		#endregion

		public ZBool IsWorkOrder
		{
			get
			{
				WhsOrder order = this.Order as WhsOrder;
				WhsWorkOrder workOrder = this.Order as WhsWorkOrder;

				if (workOrder != null)
				{
					return true;
				}
				else if (order != null)
				{
					return false;
				}
				else
				{
					throw new ArgumentException("PickableDocket was not a WhsWorkOrder or WhsOrder and is not supported by the DocWhsPickableDocket wrapper.");
				}
			}
		}

		protected ZInt BOMLevel
		{
			get
			{
				if (IsWorkOrder && OrderLineBizO != null)
				{
					var workorderline = (WhsWorkOrderLine)OrderLineBizO;
					return workorderline.WE_Level.ToZInt();
				}
				else
				{
					return ZInt.Zero;
				}
			}
		}

		protected ZString GetBlankSpaces
		{
			get
			{
				ZString blankSpaces = "";
				blankSpaces = blankSpaces.PadLeft(10 * BOMLevel, ' ');
				return blankSpaces;
			}
		}

		public ZString ProductCode
		{
			get { return SupplierPart != null ? (ZString)(GetBlankSpaces + SupplierPart.GetLocalPartNumForWarehouseConsignee(Order.Consignee)) : ZString.Empty; }
		}

		public ZString ProductDescription
		{
			get { return SupplierPart != null ? (ZString)(GetBlankSpaces + SupplierPart.GetLocalPartDescForWarehouseConsignee(Order.Consignee)) : ZString.Empty; }
		}

		public ZDecimal TopLevelUnitsOrdered
		{
			get { return BOMLevel == ZInt.Zero || !IsWorkOrder ? ReleaseLine.OrderedQuantity : ZDecimal.Zero; }
		}

		public ZDecimal TopLevelUnitsMet
		{
			get { return BOMLevel == ZInt.Zero || !IsWorkOrder ? ReleaseLine.ParentCollection.SumOfUnitsMet : ZDecimal.Zero; }
		}

		public ZString Class
		{
			get { return DGSubstance != null ? DGSubstance.IMOClass : ZString.Empty; }
		}

		public ZString IsDG
		{
			get { return DGSubstance != null ? new ZString("X") : ZString.Empty; }
		}

		public ZString UNNumber
		{
			get { return DGSubstance != null ? DGSubstance.UNNumber : ZString.Empty; }
		}

		public ZString PackingGroup
		{
			get { return DGSubstance != null ? DGSubstance.PackingGroup : ZString.Empty; }
		}

		public ZString ProperShippingName
		{
			get { return DGSubstance != null ? DGSubstance.ProperShippingName : ZString.Empty; }
		}

		public ZString HazMatString
		{
			get
			{
				ZString result = ZString.Empty;
				ZString seperator = "  ";
				if (!UNNumber.IsEmpty) { result = InsertString(result, UNNumber, Res.GetString("8ef03d20-2488-4126-add4-1be6b03a7039", "UN Number:") + " ", seperator); }
				if (!Class.IsEmpty) { result = InsertString(result, Class, Res.GetString("37716d21-9a4a-489b-9687-170c5cc55220", "DG Class:") + " ", seperator); }
				if (!ProperShippingName.IsEmpty) { result = InsertString(result, ProperShippingName, Res.GetString("c15785d3-111a-4429-86db-cc0f1b52239d", "Proper Shipping Name:") + " ", seperator); }
				if (!PackingGroup.IsEmpty) { result = InsertString(result, PackingGroup, Res.GetString("6d3cd011-44e8-443d-af0b-4a3b240dc223", "Packing Group:") + " ", seperator); }
				return result;
			}
		}

		public ZString PricingString
		{
			get
			{
				ZString result = ZString.Empty;
				ZString seperator = ", ";
				if (!RecommendedUnitPrice.IsEmpty) { result = InsertString(result, RecommendedUnitPriceWithSymbol, Res.GetString("81107e7a-fc72-4ec7-93e0-e5501155d0b1", "Rec Unit Price:") + " ", seperator); }
				if (!UnitDiscountAmount.IsEmpty) { result = InsertString(result, UnitDiscountAmountWithSymbol, Res.GetString("f0adfdce-103c-4a82-9547-64bd4697e28f", "Unit Disc:") + " ", seperator); }
				if (!UnitDiscountPercent.IsEmpty) { result = InsertString(result, UnitDiscountPercent.ToString("0.00"), Res.GetString("289c0a99-e7a4-4435-94f4-c646349e7868", "Unit Disc %:") + " ", seperator); }
				if (!UnitPriceAfterDiscount.IsEmpty) { result = InsertString(result, UnitPriceAfterDiscountWithSymbol, Res.GetString("1066c861-8f4c-4d03-b77f-991242f51d86", "Unit Price After Disc:") + " ", seperator); }
				if (!UnitPriceCurrency.IsEmpty) { result = InsertString(result, UnitPriceCurrency, Res.GetString("d717fd8f-2161-412b-b7f7-9d3c89c2a873", "Currency:") + " ", seperator); }
				return result;
			}
		}

		public ZString NMFCClass
		{
			get
			{
				return IsNMFCParticipantSecurityCheckPassed ? NMFC.Class : ZString.Empty;
			}
		}

		public ZString NMFCItemNo
		{
			get
			{
				return IsNMFCParticipantSecurityCheckPassed ? NMFC.ItemNo : ZString.Empty;
			}
		}

		public ZString NMFCDescription
		{
			get
			{
				return IsNMFCParticipantSecurityCheckPassed ? NMFC.Description : ZString.Empty;
			}
		}

		public ZString GroupedLineUnitsMetString
		{
			get { return Utilities.Round(GroupedLineUnitsMet, 0).ToString(); }
		}

		public ZString GroupedLineUnitsWeightString
		{
			get { return ReleaseLine.SupplierPart != null ? GroupedLineUnitsWeight.ToString() : ""; }
		}

		#region PositionAfterSorting

		public ZString PositionAfterSorting
		{
			get;
			set;
		}

		#endregion

		#endregion

		#region Number Fields

		#region AttributeUnits

		public ZString AttributeUnits
		{
			get
			{
				ZString result = "";

				var client = ReleaseLine.Client;

				bool isMultipleAttributesMet = (ReleaseLine.ParentCollection.Count > 1);
				bool isAttributeNeutralRollUp = client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(ReleaseLine.SupplierPart);

				if (isMultipleAttributesMet && !isAttributeNeutralRollUp)
				{
					result = ReleaseLine.Quantity.ToString("0.###");
				}

				return result;
			}
		}

		#endregion

		#region LineUnitsMet

		public ZDecimal LineUnitsMet
		{
			get
			{
				ZDecimal result = 0m;

				foreach (var line in RolledUpParents)
				{
					result += line.SumOfUnitsMet;
				}

				return result;
			}
		}

		#endregion

		#region LineUnitsOrdered

		public ZDecimal LineUnitsOrdered
		{
			get
			{
				ZDecimal result = 0m;

				foreach (var line in RolledUpParents)
				{
					result += line.WE_TransactionQuantity;
				}

				return result;
			}
		}

		#endregion

		public ZString SLineUnitsMet
		{
			get { return BOMLevel == 0 ? LineUnitsMet.ToString() : ""; }
		}

		public ZString SLineUnitsOrdered
		{
			get { return BOMLevel == 0 ? LineUnitsOrdered.ToString() : ""; }
		}

		public ZDecimal RecommendedUnitPrice
		{
			get { return OrderLineBizO != null ? OrderLineBizO.WE_RecommendedUnitPrice : ZDecimal.Zero; }
		}

		public ZDecimal UnitDiscountAmount
		{
			get { return OrderLineBizO != null ? OrderLineBizO.WE_UnitDiscountAmount : ZDecimal.Zero; }
		}

		public ZDecimal UnitDiscountPercent
		{
			get { return OrderLineBizO != null ? OrderLineBizO.WE_UnitDiscountPercent : ZDecimal.Zero; }
		}

		public ZDecimal UnitPriceAfterDiscount
		{
			get { return OrderLineBizO != null ? OrderLineBizO.WE_UnitPriceAfterDiscount : ZDecimal.Zero; }
		}

		public ZDecimal ExtendedLinePrice
		{
			get { return OrderLineBizO != null ? OrderLineBizO.WE_ExtendedLinePrice : ZDecimal.Zero; }
		}

		public ZDecimal LineUnitsWeight
		{
			get
			{
				var part = ReleaseLine.SupplierPart;
				return part != null ? LineUnitsMet * part.OP_Weight : 0m;
			}
		}

		public ZDecimal GroupedLineUnitsMet
		{
			get { return groupedLineUnitsMet; }
			set { groupedLineUnitsMet = value; }
		}
		ZDecimal groupedLineUnitsMet;

		public ZDecimal GroupedLineUnitsWeight
		{
			get
			{
				var part = ReleaseLine.SupplierPart;
				return part != null ? Utilities.Round(GroupedLineUnitsMet * part.OP_Weight, 2) : 0m;
			}
		}

		public ZDecimal CurrentRow
		{
			get
			{
				ZDecimal result = 0;
				if (ParentCollections.Count > 0)
				{
					foreach (DocWhsPackingSlipLine packingSlipLine in ParentCollections.First())
					{
						if (packingSlipLine == this)
						{
							break;
						}
						result++;
					}
				}
				return result;
			}
		}

		public ZString LineNo
		{
			get
			{
				ZInt result = OrderLineBizO != null ? OrderLineBizO.WE_LineNo : 0;

				foreach (var line in RolledUpParents)
				{
					if (result > line.WE_LineNo)
					{
						result = line.WE_LineNo;
					}
				}

				return result.ToString("00000");
			}
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
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_EntryKey : ZString.Empty; }
		}

		public ZShort CustomsEntryLineNo
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_EntryLineNo : ZShort.Zero; }
		}

		public ZString CustomsTariffItem
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsTariffItem : ZString.Empty; }
		}

		public ZDateTime CustomsEntryDate
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_EntryDate : ZDateTime.Empty; }
		}

		public ZDecimal CustomsQuantity
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_CustomsQty : ZDecimal.Zero; }
		}

		public ZString CustomsQuantityUQ
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_CustomsUnitOfQty : ZString.Empty; }
		}

		public ZDecimal CustomsVFD
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_ValueForDuty : ZDecimal.Zero; }
		}

		public ZDecimal CustomsTILV
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_TILV : ZDecimal.Zero; }
		}

		public ZString CustomsCtryOfOrigin
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_RN_NKCountryOfOrigin : ZString.Empty; }
		}

		public ZString CustomsAddInfo
		{
			get { return OrderLineBizO != null ? OrderLineBizO.CustomsData.WB_AddInfo : ZString.Empty; }
		}

		#endregion

		#region Implementation

		protected ZBool IsNMFCParticipantSecurityCheckPassed
		{
			get
			{
				var docket = ReleaseLine.PickableDocket;
				return NMFC != null && NMFC.IsActive &&
					docket != null && docket.TransportCoDocAddress.Organisation != null &&
					docket.TransportCoDocAddress.Organisation.IsNMFCParticipant;
			}
		}

		ZString InsertString(ZString originalString, ZString appendedString, ZString insertedString, ZString seperator)
		{
			ZString result = ZString.Empty;
			if (originalString.IsEmpty)
			{
				result = insertedString.TrimStart() + appendedString;
			}
			else
			{
				result = originalString + seperator + insertedString + appendedString;
			}

			return result;
		}

		#endregion
	}
}
