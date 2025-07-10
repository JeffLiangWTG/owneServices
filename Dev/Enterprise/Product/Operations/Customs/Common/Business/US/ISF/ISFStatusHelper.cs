using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.US.ISF
{
	public class ISFBillData
	{
		public ZString BillNumber
		{
			get;
			internal set;
		}

		public ZString Status
		{
			get;
			internal set;
		}

		public ZString StatusDescription
		{
			get;
			internal set;
		}
	}

	public static class ISFStatusHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant")]
		public const string Multiple = "Multiple";

		public static ISFBillData GetISFBillData(BusinessObjectFactory factory, bool isMasterBill, ZString billNumber, ZDateTime createTime)
		{
			return GetISFBillData(factory, isMasterBill, new ZString[] { billNumber }, createTime);
		}

		public static ZQuery GetISFBillDataQuery(bool isMasterBill, ZString[] billNumbers)
		{
			var billQuery = new ZQuery(CusISFBillSchema.BB_BillNum, billNumbers);
			if (isMasterBill)
			{
				billQuery.AddToFilter(CusISFBillSchema.BB_BillType, BillTypeList.Codes.OceanBillOfLading);
			}
			else
			{
				billQuery.AddToFilter(CusISFBillSchema.BB_BillType, new ZString[] { BillTypeList.Codes.OceanBillOfLading, BillTypeList.Codes.HouseBillOfLading });
			}
			return billQuery;
		}
		public const int TimeFrame6MonthsForSearching = 6;

		public static ISFBillData GetISFBillData(BusinessObjectFactory factory, bool isMasterBill, ZString[] billNumbers, ZDateTime createTime)
		{
			ISFBillData result = null;

			if (createTime.IsValid)
			{
				var bills = factory.Load<Integration.Customs.US.ISF.ICusISFBill>(GetISFBillDataQuery(isMasterBill, billNumbers));
				if (bills.Any())
				{
					var headerQuery = new ZQuery(CusISFHeaderSchema.PK, bills.Select(x => x.BB_BF).Distinct());
					var fromDate = createTime.AddMonths(-TimeFrame6MonthsForSearching);
					var toDate = createTime.AddMonths(TimeFrame6MonthsForSearching);
					var headerPKs = factory.Load<Integration.Customs.US.ISF.ICusISFHeader>(headerQuery).Where(x => x.BF_SystemCreateTimeUtc >= fromDate && x.BF_SystemCreateTimeUtc <= toDate).Select(x => x.PK).ToList();
					if (headerPKs.Any())
					{
						var validBills = bills.Where(x => headerPKs.Contains(x.BB_BF)).OrderBy(x => x.BB_BillNum);
						var count = validBills.Take(2).Count();
						if (count > 1)
						{
							result = new ISFBillData()
							{
								BillNumber = new ZStringBuilder(validBills.Select(x => x.BB_BillNum)).ToStringWithDelimiterBetweenAppends(", "),
								Status = ISFStatusHelper.Multiple,
								StatusDescription = ISFStatusHelper.BillFoundOnMultipleISF
							};
						}
						else if (count == 1)
						{
							var isfData = validBills.First();
							result = new ISFBillData()
							{
								BillNumber = isfData.BB_BillNum,
								Status = isfData.BB_CustomsStatus
							};
							if (!result.Status.IsEmpty)
							{
								result.StatusDescription = GetISFBillStatusDescription(factory, result.Status);
							}
						}
					}
				}
			}
			return result;
		}

		public static ZString GetISFBillStatusDescription(BusinessObjectFactory factory, string billStatus)
		{
			ZString result;
			if (billStatus == Multiple)
			{
				result = BillFoundOnMultipleISF;
			}
			else
			{
				result = factory.GetCachedValue<DispositionCodeList>().GetDescriptionFromCode(billStatus);
			}

			return result;
		}

		public static string BillFoundOnMultipleISF
		{
			get { return Res.GetString("ISFStatusHelper|Multiple", "The bill appears on multiple ISF jobs"); }
		}
	}
}

