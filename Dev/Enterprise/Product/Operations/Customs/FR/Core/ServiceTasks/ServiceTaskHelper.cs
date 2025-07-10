using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.ServiceTasks
{
	public static class ServiceTaskHelper
	{
		public static IEnumerable<GlbBranch> GetOneActiveBranchPerCompanies()
		{
			var factory = new BusinessObjectFactory();

			var result = new List<GlbBranch>();
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				result.AddRange(GlbBranch.GetOneActiveBranchPerCompany(country, factory));
			}

			return result;
		}

		public static CusEntryNumber[] GetFallbackEntryNumbersInStatus(BusinessObjectFactory factory, ZString entryStatus, ZString country)
		{
			var entryNumQuery = new ZQuery();
			AddFallbackEntryNumFilter(entryNumQuery, entryStatus, country);
			return factory.Load<CusEntryNumber>(entryNumQuery);
		}

		public static void AddFallbackEntryNumFilter(ZQuery entryNumQuery, ZString entryStatus, ZString country)
		{
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, entryStatus);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeader.Schema.FallbackEntryType);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, country);
		}

		public static int IfZero(int origrinalInt, int ifZeroInt)
		{
			if (origrinalInt == 0)
			{
				origrinalInt = ifZeroInt;
			}
			return origrinalInt;
		}
	}
}
