using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.eHubMessaging.Business.Interfaces;

namespace Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck
{
	public class FailedEDIInterchangeHealthCheckResult : IHealthCheckResult
	{
		public FailedEDIInterchangeHealthCheckResult()
		{
			FailedEDIInterchangesCountInPeriodPerCompany = new Dictionary<Guid, int>();
		}

		public FailedEDIInterchangeHealthCheckResult(string data)
		{
			FailedEDIInterchangesCountInPeriodPerCompany = Deserialize(data);
		}

		public static FailedEDIInterchangeHealthCheckResult Empty => new FailedEDIInterchangeHealthCheckResult();

		public bool FoundError()
		{
			return FailedEDIInterchangesCountInPeriodPerCompany.Any();
		}

		public void Add(Guid companyPK, int count)
		{
			if (FailedEDIInterchangesCountInPeriodPerCompany.ContainsKey(companyPK))
			{
				FailedEDIInterchangesCountInPeriodPerCompany[companyPK] += count;
			}
			else
			{
				FailedEDIInterchangesCountInPeriodPerCompany[companyPK] = count;
			}
		}

		public string Serialize()
		{
			return Serialize(FailedEDIInterchangesCountInPeriodPerCompany);
		}

		static string Serialize(Dictionary<Guid, int> failedEDIInterchangesCountInPeriod)
		{
			return string.Join(",", failedEDIInterchangesCountInPeriod.Select(m => m.Key + "=" + m.Value).ToArray());
		}

		static Dictionary<Guid, int> Deserialize(string data)
		{
			return data.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Split('='))
				.Where(x => x.Length == 2 && Guid.TryParse(x[0], out var companyPK))
				.ToDictionary(x => Guid.Parse(x[0]), x => int.Parse(x[1], CultureInfo.InvariantCulture));
		}

		public Dictionary<Guid, int> FailedEDIInterchangesCountInPeriodPerCompany { get; }
	}
}
