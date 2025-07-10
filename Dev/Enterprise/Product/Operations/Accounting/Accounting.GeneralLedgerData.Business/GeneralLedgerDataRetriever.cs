using System;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public static class GeneralLedgerDataRetriever
	{
		#region TransactionHeader

		public static TransactionHeaderInfo GetTransactionHeaderInfo(ReadOnlyBusinessObjectFactory factory, Guid headerPK)
		{
			TransactionHeaderInfo headerInfo = null;
			if (TransactionHeaderInfoDict.TryGetValue(headerPK, out headerInfo))
			{
				return headerInfo;
			}
			else
			{
				var header = factory.Load<AccTransactionHeader>(headerPK);
				if (header != null)
				{
					headerInfo = new TransactionHeaderInfo
					{
						Ledger = header.AH_Ledger,
						TransactionType = header.AH_TransactionType,
						TransactionNum = header.AH_TransactionNum,
						OrgHeaderPK = header.AH_OH,
						PostDate = header.AH_PostDate,
						DueDate = header.AH_DueDate,
					};
					TransactionHeaderInfoDict.Add(headerPK, headerInfo);
				}
			}

			return headerInfo;
		}

		static readonly LRUCache<ZGuid, TransactionHeaderInfo> TransactionHeaderInfoDict = new LRUCache<ZGuid, TransactionHeaderInfo>();

		public class TransactionHeaderInfo
		{
			public ZString Ledger { get; set; }
			public ZString TransactionType { get; set; }
			public ZString TransactionNum { get; set; }
			public ZGuid OrgHeaderPK { get; set; }
			public ZDateTime PostDate { get; set; }
			public ZDateTime DueDate { get; set; }
		}

		#endregion

		#region CompanyInfo

		public static CompanyInfo GetCompanyInfo(ReadOnlyBusinessObjectFactory factory, Guid companyPK)
		{
			CompanyInfo companyInfo;
			if (CompanyInfoDict.TryGetValue(companyPK, out companyInfo))
			{
				return companyInfo;
			}
			else
			{
				var company = factory.Load<GlbCompany>(companyPK);
				companyInfo = new CompanyInfo
				{
					Company = company,
					PeriodCalculator = new AccountingPeriodCalculator(factory, company)
				};

				CompanyInfoDict.Add(companyPK, companyInfo);
			}

			return companyInfo;
		}

		public static void ClearCompanyInfoCache()
		{
			CompanyInfoDict.Clear();
		}

		static readonly LRUCache<ZGuid, CompanyInfo> CompanyInfoDict = new LRUCache<ZGuid, CompanyInfo>();

		public class CompanyInfo
		{
			public GlbCompany Company { get; set; }
			public AccountingPeriodCalculator PeriodCalculator { get; set; }
		}

		#endregion
	}
}
