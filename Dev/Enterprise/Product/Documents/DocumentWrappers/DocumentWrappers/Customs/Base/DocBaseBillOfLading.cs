using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public class DocBaseBillOfLading : NonPersistentBusinessObject
	{
		public DocBaseBillOfLading(Bill billOfLading)
			: base(billOfLading.Factory)
		{
			this.billOfLading = billOfLading;
		}
		readonly Bill billOfLading;

		public ZString BillNumber
		{
			get
			{
				var result = ZString.Empty;
				if (billOfLading.IsMasterBill)
				{
					result = Res.GetString("0CE11FE9-06C0-4D58-8B2A-14EF16E798A1", "Master:{0}", GetBillNumberWithCodeCore(billOfLading));
				}
				else if (billOfLading.IsHouseBill)
				{
					result = (billOfLading.ParentBill != null ? Res.GetString("5F12385E-13F8-425D-BE8F-CF386F33ED00", "Master:{0}     ", GetBillNumberWithCodeCore(billOfLading.ParentBill)) : string.Empty) +
						Res.GetString("1D2CC2F0-D149-4970-BA0A-F2C053FC360C", "House:{0}", GetBillNumberWithCodeCore(billOfLading));
				}
				else if (billOfLading.IsSubHouseBill)
				{
					if (billOfLading.ParentBill != null)
					{
						if (billOfLading.ParentBill.ParentBill != null)
						{
							result += Res.GetString("5F12385E-13F8-425D-BE8F-CF386F33ED00", "Master:{0}     ", GetBillNumberWithCodeCore(billOfLading.ParentBill.ParentBill));
						}

						result += Res.GetString("D001896E-3917-4E4F-AD84-9716449CA4AF", "House:{0}     ", GetBillNumberWithCodeCore(billOfLading.ParentBill));
					}

					result += Res.GetString("148F33AF-678D-4782-A2CB-EA5B991774CE", "Sub House:{0}", GetBillNumberWithCodeCore(billOfLading));
				}

				return result + GetAdditionalNumber(billOfLading);
			}
		}

		protected virtual ZString GetAdditionalNumber(Bill bill)
		{
			return ZString.Empty;
		}

		protected virtual ZString GetBillNumberWithCodeCore(Bill bill)
		{
			return bill.CU_BillNum;
		}

		[DecimalPlaces("QuantityDecimalPlaces")]
		public ZDecimal ManifestQty
		{
			get { return GetManifestQtyCore(billOfLading); }
		}

		protected virtual int QuantityDecimalPlaces
		{
			get { return 0; }
		}

		protected virtual ZDecimal GetManifestQtyCore(Bill bill)
		{
			var result = bill.CU_NoOfPacks;

			if (result.IsEmpty)
			{
				foreach (Bill childBill in bill.ChildBills)
				{
					result += GetManifestQtyCore(childBill);
				}
			}

			return result;
		}

		public ZString UOM
		{
			get { return GetUniqueManifestUQ(billOfLading); }
		}

		protected virtual ZString GetUniqueManifestUQ(Bill bill)
		{
			var result = bill.CU_PackType;
			var hasDifferentUQ = false;

			if (result.IsEmpty)
			{
				foreach (Bill childBill in bill.ChildBills)
				{
					var childUQ = GetUniqueManifestUQ(childBill);

					if (!childUQ.IsEmpty)
					{
						if (!result.IsEmpty && result != childUQ)
						{
							hasDifferentUQ = true;
							break;
						}
						else if (result.IsEmpty)
						{
							result = childUQ;
						}
					}
				}
			}

			return !result.IsEmpty && hasDifferentUQ ? "PCS" : result.ToString();
		}
	}
}
