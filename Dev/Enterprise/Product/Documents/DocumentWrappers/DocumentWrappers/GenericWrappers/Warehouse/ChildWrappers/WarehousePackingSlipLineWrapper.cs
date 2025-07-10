using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePackingSlipLineWrapper : WarehouseDocketLineWrapper, IPackingSlipWrapper
	{
		public WarehousePackingSlipLineWrapper(WhsReleaseLine packingLineBO, WhsPickableDocketLine orderLine, BusinessObjectFactory factory)
			: base(packingLineBO, factory)
		{
			OrderLine = orderLine;
		}

		readonly WhsPickableDocketLine OrderLine;

		#region LineNo

		protected override ZShort LineNoCore
		{
			get
			{
				var result = OrderLine != null ? OrderLine.WE_LineNo : (ZShort)0;

				foreach (var line in RolledUpParents)
				{
					if (result > line.WE_LineNo)
					{
						result = line.WE_LineNo;
					}
				}

				return result;
			}
		}

		#endregion

		#region Product related

		#region ProductCodeCore

		protected override ZString ProductCodeCore
		{
			get
			{
				ZString result = "";
				if (PackingLineBO != null && SupplierPart != null)
				{
					result = BOMIndentation + SupplierPart.GetLocalPartNumForWarehouseConsignee(OrderBO.Consignee);
				}
				return result;
			}
		}

		#endregion

		protected override ZString ProductDescriptionCore
		{
			get
			{
				ZString result = ZString.Empty;
				if (PackingLineBO != null && SupplierPart != null)
				{
					result = BOMIndentation + SupplierPart.GetLocalPartDescForWarehouseConsignee(OrderBO.Consignee);
				}
				return result;
			}
		}

		#endregion

		#region Attribute Related

		#region AttributeUnits

		protected override ZString AttributeUnitsCore
		{
			get
			{
				ZString result = "";

				if (PackingLineBO != null)
				{
					var client = PackingLineBO.Client;

					bool isMultipleAttributesMet = (PackingLineBO.ParentCollection.Count > 1);
					bool isAttributeNeutralRollUp = (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(PackingLineBO.SupplierPart));

					if (isMultipleAttributesMet && !isAttributeNeutralRollUp)
					{
						result = PackingLineBO.Quantity.ToString("0.###");
					}
				}

				return result;
			}
		}

		#endregion

		#region ExpiryDateCore

		protected override ZDateTime ExpiryDateCore
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (PackingLineBO != null)
				{
					var client = PackingLineBO.Client;
					result = (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(PackingLineBO.SupplierPart))
						? PackingLineBO.OrderedExpiryDate
						: PackingLineBO.ExpiryDate;
				}

				return result;
			}
		}

		#endregion

		#region PackingDateCore

		protected override ZDateTime PackingDateCore
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (PackingLineBO != null)
				{
					var client = PackingLineBO.Client;
					result = (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(PackingLineBO.SupplierPart))
						? PackingLineBO.OrderedPackingDate
						: PackingLineBO.PackingDate;
				}

				return result;
			}
		}

		#endregion

		#region PartAttribute1Core

		protected override ZString PartAttribute1Core
		{
			get
			{
				ZString result = "";

				if (PackingLineBO != null)
				{
					var client = PackingLineBO.Client;
					result = (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(PackingLineBO.SupplierPart))
						? PackingLineBO.OrderedPartAttribute1
						: PackingLineBO.PartAttribute1;
				}

				return result;
			}
		}

		#endregion

		#region PartAttribute2Core

		protected override ZString PartAttribute2Core
		{
			get
			{
				ZString result = "";

				if (PackingLineBO != null)
				{
					var client = PackingLineBO.Client;
					result = (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(PackingLineBO.SupplierPart))
						? PackingLineBO.OrderedPartAttribute2
						: PackingLineBO.PartAttribute2;
				}

				return result;
			}
		}

		#endregion

		#region PartAttribute3Core

		protected override ZString PartAttribute3Core
		{
			get
			{
				ZString result = "";

				if (PackingLineBO != null)
				{
					var client = PackingLineBO.Client;
					result = (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(PackingLineBO.SupplierPart))
						? PackingLineBO.OrderedPartAttribute3
						: PackingLineBO.PartAttribute3;
				}

				return result;
			}
		}

		#endregion

		#region TrackedSerialNumberCore

		protected override ZString TrackedSerialNumberCore
		{
			get
			{
				ZString result = "";

				if (PackingLineBO != null)
				{
					var client = PackingLineBO.Client;
					result = (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(PackingLineBO.SupplierPart))
						? PackingLineBO.OrderedSerialNumber
						: PackingLineBO.SerialNumber;
				}

				return result;
			}
		}

		#endregion

		#region Additional Information

		public override UNDGSubstanceWrapper DangerousGoodsSubstance
		{
			get
			{
				UNDGSubstanceWrapper result = null;

				var part = SupplierPart;
				if (part != null && part.UNDGs.Count > 0 && part.UNDGs[0].Substance != null)
				{
					result = new UNDGSubstanceWrapper(part.UNDGs[0], Factory);
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region NMFC

		public override DocRefNMFC NMFC
		{
			get { return IsNMFCParticipantSecurityCheckPassed ? NMFC_NoSecurityCheck : null; }
		}

		DocRefNMFC NMFC_NoSecurityCheck
		{
			get
			{
				if (nmfcWrapper == null)
				{
					var nmfc = SupplierPart != null && SupplierPart.CommodityCode != null ? SupplierPart.CommodityCode.NMFC : null;
					return nmfcWrapper ?? (nmfcWrapper = DocRefNMFC.New(nmfc, Factory));
				}
				return nmfcWrapper;
			}
		}
		DocRefNMFC nmfcWrapper;

		#endregion

		#region Money

		protected override MoneyWrapper ExtendedLinePriceCore
		{
			get
			{
				return OrderLine != null
					? new MoneyWrapper(TotaledExtendedLinePrices, Factory)
					: null;
			}
		}

		Money TotaledExtendedLinePrices
		{
			get
			{
				Money result;
				var total = 0m;
				var uniqueCurrencies = new HashSet<ICurrency>();

				foreach (var line in RolledUpParents)
				{
					total += line.WE_ExtendedLinePrice;
					uniqueCurrencies.Add(line.UnitPriceCurrency);
				}

				if (uniqueCurrencies.Count == 1)
				{
					result = new Money(total, uniqueCurrencies.SingleOrDefault());
				}
				else
				{
					result = new Money(total, null);
				}

				return result;
			}
		}

		protected override LabelValuePairWrapper RecommendedUnitPriceCore
		{
			get
			{
				if (OrderLine != null)
				{
					return new LabelValuePairWrapper(Res.GetString("a7dda2e2-a143-4a3c-be42-54620bccd2bb", "Rec Unit Price"),
						new LabelValuePairWrapper
						(
							new MoneyWrapper(new Money(OrderLine.WE_RecommendedUnitPrice, OrderLine.UnitPriceCurrency), Factory).ToString(),
							OrderLine.WE_RecommendedUnitPrice,
							Factory
						), Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		protected override LabelValuePairWrapper UnitDiscountAmountCore
		{
			get
			{
				if (OrderLine != null)
				{
					return new LabelValuePairWrapper(Res.GetString("e5d9d170-74b2-4aae-937a-f4267df43cdd", "Unit Disc"),
						new LabelValuePairWrapper
						(
							new MoneyWrapper(new Money(OrderLine.WE_UnitDiscountAmount, OrderLine.UnitPriceCurrency), Factory).ToString(),
							OrderLine.WE_UnitDiscountAmount,
							Factory
						), Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		protected override LabelValuePairWrapper UnitDiscountPercentCore
		{
			get
			{
				if (OrderLine != null)
				{
					return new LabelValuePairWrapper(Res.GetString("84fe967a-aa91-44f9-8c2c-1b6cef386b0c", "Unit Disc %"),
						new LabelValuePairWrapper(OrderLine.WE_UnitDiscountPercent.ToString("0.00"), OrderLine.WE_UnitDiscountPercent, Factory), Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		public override LabelValuePairWrapper UnitPriceAfterDiscount
		{
			get
			{
				if (OrderLine != null)
				{
					return new LabelValuePairWrapper(Res.GetString("f360bbe6-b4e7-4a0e-92fb-c2709fc6b718", "Unit Price After Disc"),
						new LabelValuePairWrapper
						(
							new MoneyWrapper(new Money(OrderLine.WE_UnitPriceAfterDiscount, OrderLine.UnitPriceCurrency), Factory).ToString(),
							OrderLine.WE_UnitPriceAfterDiscount,
							Factory
						), Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		protected override ZString AdditionalMoneysCore
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				result.AppendIfNotEmpty(RecommendedUnitPrice.LabelAndValue);
				result.AppendIfNotEmpty(UnitDiscountAmount.LabelAndValue);
				result.AppendIfNotEmpty(UnitDiscountPercent.LabelAndValue);
				result.AppendIfNotEmpty(UnitPriceAfterDiscount.LabelAndValue);
				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		#endregion

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

					var parent = OrderLine;
					if (parent != null)
					{
						LinePK = parent.PK;
						rolledUpParents.Add(parent);
					}
				}

				return rolledUpParents;
			}
		}

		List<WhsPickableDocketLine> rolledUpParents;

		#endregion

		#region UnitsMet

		protected override LabelValuePairWrapper UnitsMetCore
		{
			get
			{
				if (OrderLine != null)
				{
					return new LabelValuePairWrapper(Res.GetString("8b64869d-fdd7-4c9d-a1a6-ab18e4ea8771", "Units Met"),
						new LabelValuePairWrapper(BOMLevel.IsEmpty ? LineUnitsMet.ToString() : String.Empty, LineUnitsMet, Factory), Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		ZDecimal LineUnitsMet
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

		#region GroupedLineUnitsMet

		protected override ZDecimal GroupedLineUnitsMetCore
		{
			get { return groupedLineUnitsMet; }
			set { groupedLineUnitsMet = value; }
		}
		ZDecimal groupedLineUnitsMet;

		#endregion

		#region CurrencySymbol

		protected override ZString CurrencySymbolCore
		{
			get
			{
				var result = ZString.Empty;

				var unitPriceCurrency = OrderLine?.UnitPriceCurrency;
				if (unitPriceCurrency != null)
				{
					result = unitPriceCurrency.RX_Symbol;
				}
				else
				{
					var docket = PackingLineBO.PickableDocket;
					if (docket?.Client?.Country?.LocalCurrency != null)
					{
						result = docket.Client.Country.LocalCurrency.RX_Symbol;
					}
				}
				return result;
			}
		}

		#endregion

		#region UnitsOrdered

		protected override LabelValuePairWrapper UnitsOrderedCore
		{
			get
			{
				if (OrderLine != null)
				{
					return new LabelValuePairWrapper(Res.GetString("3063902a-809f-44d4-bd58-5570a89fd16a", "Units Ordered"),
						new LabelValuePairWrapper(BOMLevel.IsEmpty ? LineUnitsOrdered.ToString() : String.Empty, LineUnitsOrdered, Factory), Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		ZDecimal LineUnitsOrdered
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

		#region UnitsUQ

		protected override ZString UnitsUQCore
		{
			get { return PackingLineBO != null && BOMLevel == 0 ? PackingLineBO.UnitsUQ : ZString.Empty; }
		}

		#endregion

		#region CustomFields

		protected override CustomLabelsProviderAndBizO GetCustomLabelsProvider()
		{
			var docket = PackingLineBO != null ? PackingLineBO.PickableDocket : null;
			return docket != null ? new CustomLabelsProviderAndBizO(OrderLine, new WhsDocketLine.CustomLabelsProvider(docket)) : null;
		}

		#endregion

		#region ExtendedLinePriceForTotalCore

		protected override ZDecimal ExtendedLinePriceForTotalCore
		{
			get
			{
				var totalUnitsMet = LineUnitsMet;
				var packingLineUnits = IsMultipledRolledUpOrderLines ? totalUnitsMet : PackingLineBO.Quantity;

				return totalUnitsMet != ZDecimal.Zero
					? TotaledExtendedLinePrices.Amount * (packingLineUnits / totalUnitsMet)
					: 0m;
			}
		}

		bool IsMultipledRolledUpOrderLines => RolledUpParents.Count > 1;

		#endregion

		#region Implementation

		WhsReleaseLine PackingLineBO
		{
			get { return (WhsReleaseLine)WrappedBO; }
		}

		protected override WhsDocket DocketBO
		{
			get { return OrderBO; }
		}

		protected override WhsDocketLine DocketLineBO
		{
			get { return OrderLine; }
		}

		WhsPickableDocket OrderBO
		{
			get { return PackingLineBO?.PickableDocket; }
		}

		protected ZBool IsNMFCParticipantSecurityCheckPassed
		{
			get
			{
				var result = false;
				if (NMFC_NoSecurityCheck != null && NMFC_NoSecurityCheck.IsActive)
				{
					var docket = PackingLineBO.PickableDocket;
					var address = docket != null ? docket.TransportCoDocAddress : null;
					var org = address != null ? address.Organisation : null;
					result = org != null && org.IsNMFCParticipant;
				}
				return result;
			}
		}

		#endregion
	}
}
