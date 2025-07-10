using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseGroupedLinesForVarianceWrapper : GenericWrapper
	{
		public WarehouseGroupedLinesForVarianceWrapper(WhsDocketLine docketLine, BusinessObjectFactory factory)
			: base(docketLine ?? factory.GetNull<WhsReceiveLine>(), factory)
		{
			if (docketLine != null)
			{
				IncrementTotalValues(docketLine);
			}
		}

		public ZString ProductCode => DocketLine.ProductCode;

		public ZString ProductDescription => DocketLine.ProductDesc;

		public ZString PartAttribute1 => DocketLine.WE_PartAttrib1;

		public ZString PartAttribute2WithLabel => WarehouseGenericLineWrapper.BuildAttribute(
			DocketLine.WE_PartAttrib2,
			DocketLine?.Docket?.Client?.MiscServ.OM_IMPartAttrib2NameMultilingual ?? (NoResString)ZString.Empty,
			ResString.GetMultilingualString("cfcfd119-6fe5-4402-860a-a9196f391350", "Attribute 2"));

		public ZString PartAttribute3WithLabel => WarehouseGenericLineWrapper.BuildAttribute(
			DocketLine.WE_PartAttrib3,
			DocketLine?.Docket?.Client?.MiscServ.OM_IMPartAttrib3NameMultilingual ?? (NoResString)ZString.Empty,
			ResString.GetMultilingualString("68bdec78-c8c9-48fd-9629-a8384e6d4720", "Attribute 3"));

		public ZString TrackedSerialWithLabel => WarehouseGenericLineWrapper.BuildAttribute(
			DocketLine.WE_SerialNumber,
			ResString.GetMultilingualString("fd598626-e39e-41c5-8d60-099ca7dabee3", "Tracked Serial Number"),
			"");

		public ZString PackingDateWithLabel => WarehouseGenericLineWrapper.BuildAttribute(
			DocketLine.WE_PackingDate.ToShortDateString(),
			ResString.GetMultilingualString("f37751d1-16b3-45ba-9fcc-b0f1cc73e65d", "Packing Date"),
			"");

		public ZString ExpiryDateWithLabel => WarehouseGenericLineWrapper.BuildAttribute(
			DocketLine.WE_ExpiryDate.ToShortDateString(),
			ResString.GetMultilingualString("f113e113-8f64-41a1-9b37-3f20cbc929f5", "Expiry Date"),
			"");

		public ZDecimal TotalExpectedReceiptQuantity { get; private set; }

		public ZString UnitsUQ => DocketLine.ProductUQ;

		public ZDecimal TotalUnits { get; private set; }

		public ZDecimal TotalVariance { get; private set; }

		public ZDecimal TotalWeight { get; private set; }

		public ZString WeightUQ => DocketLine.SupplierPart?.OP_WeightUQ ?? ZString.Empty;

		public ZString Status => DocketLine.WE_CurrentInventoryStatus;

		#region Customs Fields

		public ZString CustomsEntryKey
		{
			get
			{
				var customsEntryKey = DocketLine.CustomsData.WB_EntryKey + " / " + DocketLine.CustomsData.WB_EntryLineNo;
				if (customsEntryKey == " / 0")
				{
					customsEntryKey = "";
				}

				return customsEntryKey;
			}
		}

		public ZDateTime CustomsEntryDate => DocketLine.CustomsData.WB_EntryDate;

		public ZDecimal CustomsQuantity => DocketLine.CustomsData.WB_CustomsQty;

		public ZString CustomsQuantityUQ => DocketLine.CustomsData.WB_CustomsUnitOfQty;

		public ZString CustomsCtryOfOrigin => DocketLine.CustomsData.WB_RN_NKCountryOfOrigin;

		public ZDecimal CustomsVFD => DocketLine.CustomsData.WB_ValueForDuty;

		public ZDecimal CustomsTILV => DocketLine.CustomsData.WB_TILV;

		public ZString CustomsAddInfo => DocketLine.CustomsData.WB_AddInfo;

		#endregion

		internal void IncrementTotalValues(WhsDocketLine docketLine)
		{
			TotalVariance += GetVariance(docketLine);
			TotalWeight += GetWeight(docketLine);
			TotalExpectedReceiptQuantity += docketLine.WE_ClientOrderedUnits;
			TotalUnits += docketLine.WE_TransactionQuantity;
		}

		ZDecimal GetVariance(WhsDocketLine docketLine)
		{
			return docketLine.WE_TransactionQuantity - docketLine.WE_ClientOrderedUnits;
		}

		ZDecimal GetWeight(WhsDocketLine docketLine)
		{
			var weight = 0m;
			var part = docketLine.SupplierPart;
			if (part != null)
			{
				weight = docketLine.WE_TransactionQuantity * part.OP_Weight;
			}

			return weight;
		}

		#region Implementation

		WhsDocketLine DocketLine => (WhsDocketLine)WrappedObject;

		#endregion
	}
}
