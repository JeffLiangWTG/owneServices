using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface ISystemMinimumFees
	{
		IEnumerable<SystemMinimumFee> GetSystemMinimumFeesByDatabase(ZGuid dbPK, ZDateTime periodStart);
	}

	public class SystemMinimumFees : ISystemMinimumFees
	{
		readonly Dictionary<ZGuid, SystemMinimumFee[]> DatabaseMinimumFeeMap;

		public SystemMinimumFees(SystemBill[] systemBills)
		{
			DatabaseMinimumFeeMap = systemBills.OfType<ISystemMinimumFeeContributionBill>()
				.SelectMany(x => x.CalculateMinimumFeeContribution()).GroupBy(x => x.DatabasePk)
				.ToDictionary(group => group.Key, group => group.ToArray());
		}

		public IEnumerable<SystemMinimumFee> GetSystemMinimumFeesByDatabase(ZGuid dbPK, ZDateTime periodStart)
		{
			SystemMinimumFee[] fees = null;
			if (DatabaseMinimumFeeMap.TryGetValue(dbPK, out fees))
			{
				return fees.Where(x => x.PeriodStart == periodStart);
			}
			else
			{
				return Enumerable.Empty<SystemMinimumFee>();
			}
		}
	}
}

