using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.Accounting.Business.CreditStatus;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public static class ARBalance
	{
		public static Dictionary<Guid, decimal> GetOutstandingBalance(IEnumerable<Guid> orgPks, Guid companyPk, DateTime maxDueDateExclusive)
		{
			string sqlBalance = ARAPDataAccessor.GetOutstandingBalanceSql()
				.Replace("@Ledger", "'" + LedgerTypes.AccountsReceivable + "'")
				.Replace("@Company", "'" + companyPk + "'")
				.Replace("SELECT", "SELECT AH_OH, ")
				.Replace("@OH", "'" + string.Join("', '", orgPks) + "'")
				;

			string sql = @"
select AH_OH, -Amount
from
(
" + sqlBalance + @"
AND (AH_DueDate is null or AH_DueDate < '" + SqlFormatInfo.ToSqlDateString(maxDueDateExclusive) + @"') group by AH_OH ) a ";

			var result = new Dictionary<Guid, decimal>();

			using (var cmd = Db.Connection.Command(sql))
			{
				using (var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
				{
					while (reader.Read())
					{
						var orgPk = reader.GetGuid(0);
						var amount = reader.GetDecimal(1);

						result.Add(orgPk, amount);
					}
				}
			}

			return result;
		}

		public static decimal GetOutstandingBalance(Guid orgPk, Guid companyPk, DateTime maxDueDateExclusive)
		{
			return ARBalance.GetOutstandingBalance(new[] { orgPk }, companyPk, maxDueDateExclusive).Values.FirstOrDefault();
		}
	}
}

