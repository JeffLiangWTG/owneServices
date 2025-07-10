using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public static class AdditionalBillCollectionExtensions
	{
		/// <summary>
		/// Get the Parent Additional Bill of the Additional Bill which match BillNumber and BillType
		/// </summary>
		/// <param name="billNumber">Bill Number</param>
		/// <param name="billType">Bill Type</param>
		/// <returns></returns>
		public static AdditionalBill GetParentAdditionalBill(this IEnumerable<AdditionalBill> bills, ZString billNumber, ZString billType, Func<AdditionalBill, bool> additionalMatch = null)
		{
			AdditionalBill result = null;
			if (bills != null && billType != WayBillTypeList.Codes.Master)
			{
				var additionalBill = bills.GetAdditionalBill(billNumber, billType, additionalMatch);
				if (additionalBill != null)
				{
					var parentBillNumber = additionalBill.ParentBillNumber.GetValueOrDefault();
					if (!parentBillNumber.IsEmpty)
					{
						var parentBillType = billType == WayBillTypeList.Codes.SubHouse ? WayBillTypeList.Codes.House : WayBillTypeList.Codes.Master;
						foreach (var bill in bills)
						{
							if (bill != null && bill != additionalBill && bill.BillNumber.GetValueOrDefault() == parentBillNumber && bill.BillType.GetCodeAsUpperCase() == parentBillType && (additionalMatch == null || additionalMatch(bill)))
							{
								result = bill;
								break;
							}
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Get the Children Additional Bill of the Additional Bill which match BillNumber and BillType
		/// </summary>
		/// <param name="billNumber">Bill Number</param>
		/// <param name="billType">Bill Type</param>
		/// <returns></returns>
		public static IEnumerable<AdditionalBill> GetChildrenAdditionalBill(this IEnumerable<AdditionalBill> bills, ZString billNumber, ZString billType, Func<AdditionalBill, bool> additionalMatch = null)
		{
			if (bills != null && billType != WayBillTypeList.Codes.SubHouse)
			{
				var childBillType = billType == WayBillTypeList.Codes.Master ? WayBillTypeList.Codes.House : WayBillTypeList.Codes.SubHouse;
				foreach (var bill in bills)
				{
					if (bill != null && bill.ParentBillNumber.GetValueOrDefault() == billNumber && bill.BillType.GetCodeAsUpperCase() == childBillType && (additionalMatch == null || additionalMatch(bill)))
					{
						yield return bill;
					}
				}
			}
		}

		/// <summary>
		/// Get AdditionalBill matching BillNumber and BillType
		/// </summary>
		/// <param name="billNumber">Bill Number</param>
		/// <param name="billType">Bill Type</param>
		/// <returns></returns>
		public static AdditionalBill GetAdditionalBill(this IEnumerable<AdditionalBill> bills, ZString billNumber, ZString billType, Func<AdditionalBill, bool> additionalMatch = null)
		{
			AdditionalBill result = null;
			if (bills != null)
			{
				foreach (var bill in bills)
				{
					if (bill != null && bill.BillNumber.GetValueOrDefault() == billNumber && bill.BillType.GetCodeAsUpperCase() == billType && (additionalMatch == null || additionalMatch(bill)))
					{
						result = bill;
						break;
					}
				}
			}
			return result;
		}
	}
}

