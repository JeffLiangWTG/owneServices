namespace Enterprise.Client.EDI.Billing.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;

	public interface IEnterpriseUsage
	{
		ZDateTime FirstOdplChargeableUsagePeriod(ZGuid licEnterprisePk, string moduleCode);
	}

	/// <summary>
	/// Licence module usage across an enterprise.
	/// </summary>
	public class EnterpriseUsage : IEnterpriseUsage
	{
		public static IEnterpriseUsage SharedCache
		{
			get { return sharedCache ?? (sharedCache = new Cache()); }
		}

		public static void ClearCache()
		{
			sharedCache = null;
		}

		[ThreadStatic]
		static IEnterpriseUsage sharedCache;

		/// <summary>
		/// Determine the month of the first chargeable usage of the given module
		/// across all production databases of the given enterprise.
		/// </summary>
		/// <returns>Day 1 of first month with usage, or an empty date if no usage</returns>
		public ZDateTime FirstOdplChargeableUsagePeriod(ZGuid licEnterprisePk, string moduleCode)
		{
			//select MIN(U1_PeriodStart)
			//from dbo.ClientChargeableUsage
			//where 
			//    U1_Code = 'ODM'
			//    and U1_SubCode = <moduleCode>
			//    and 
			//    U1_LD in
			//    (
			//        select LD_PK from dbo.LicenceDatabase where LD_LicenceType = 'PRD' and LD_LE = <licEnterprisePk>
			//    )
			var query = new ZDBOnlyQuery(typeof(ClientChargeableUsage));
			query.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.ODM);
			query.AddToFilter(ClientChargeableUsageSchema.U1_SubCode, moduleCode);

			var dbQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), ClientChargeableUsageSchema.U1_LD);
			dbQuery.AddToFilter(LicenceDatabaseSchema.LD_LicenceType, DatabaseTypes.Codes.Production);
			dbQuery.AddToFilter(LicenceDatabaseSchema.LD_LE, licEnterprisePk);

			query.AddSubQuery(dbQuery, JoinCondition.And);

			string sql = "SELECT MIN(" + ClientChargeableUsageSchema.Constants.U1_PeriodStart +
				") FROM " + ClientChargeableUsageSchema.Constants.TableName +
				" WHERE " + query.LiteralTextSqlFormatted;

			ZDateTime result = ZDateTime.Empty;

			using (var cmd = Db.Connection.Command(sql))
			{
				var scalar = cmd.ExecuteScalar();
				if (scalar is DateTime)
				{
					result = (DateTime)scalar;
				}
			}

			return result;
		}

		public class Cache : IEnterpriseUsage
		{
			public Cache(IEnterpriseUsage usage = null)
			{
				this.usage = usage ?? new EnterpriseUsage();
			}

			readonly IEnterpriseUsage usage;

			public ZDateTime FirstOdplChargeableUsagePeriod(ZGuid licEnterprisePk, string moduleCode)
			{
				Dictionary<string, ZDateTime> codeMap;

				if (!cache.TryGetValue(licEnterprisePk, out codeMap))
				{
					codeMap = new Dictionary<string, ZDateTime>();
					cache.Add(licEnterprisePk, codeMap);
				}

				ZDateTime result;
				if (!codeMap.TryGetValue(moduleCode, out result))
				{
					result = usage.FirstOdplChargeableUsagePeriod(licEnterprisePk, moduleCode);
					codeMap.Add(moduleCode, result);
				}

				return result;
			}

			readonly Dictionary<ZGuid, Dictionary<string, ZDateTime>> cache = new Dictionary<ZGuid, Dictionary<string, ZDateTime>>();
		}
	}
}

