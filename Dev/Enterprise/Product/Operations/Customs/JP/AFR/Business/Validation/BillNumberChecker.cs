using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public static class BillNumberChecker
	{
		const int NVOCCHouseBillMaxLimit = 99;
		const int VOCCBillMaxLimit = 9999;

		public static bool IsMBOLNumDuplicate(this JPAFRHeader header)
		{
			var query = new ZQuery();
			query.AddToFilter(JPAFRHeaderSchema.JPH_MasterBillNumber, header.JPH_MasterBillNumber);
			query.AddToFilter(JPAFRHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.PK);
			var loadedHeaders = header.Factory.Load<JPAFRHeader>(query);
			return loadedHeaders != null && loadedHeaders.Length > 0;
		}

		public static bool IsHBOLNumDuplicateWithinThisHeader(this JPAFRBills currentBill)
		{
			var result = false;
			var billHeader = currentBill != null ? currentBill.Header : null;
			if (billHeader != null)
			{
				result = billHeader.Bills.Any(bill => bill.JPB_BillNumber == currentBill.JPB_BillNumber && bill != currentBill);
			}
			return result;
		}

		public static bool HasHBOLReachedMaxAllowedWithinThisHeader(this JPAFRBills currentBill)
		{
			var result = false;
			var billHeader = currentBill != null ? currentBill.Header : null;
			if (billHeader != null)
			{
				result = billHeader.Bills.Count > (billHeader.JPH_IsShippingLineEntry ? VOCCBillMaxLimit : NVOCCHouseBillMaxLimit);
			}
			return result;
		}
	}
}
