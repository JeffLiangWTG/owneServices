using System;
using System.Threading.Tasks;
using CargoWise.Licensing;
using Enterprise.ProductRegistration.Common;

namespace CargoWise.ProductRegistration.Service
{
	public interface IRegistrationRepository : IDisposable
	{
		Task<DbStatus> GetStatus(string productKey);
		Task<DbRegisterResult> Register(RegisterRequest request, string passwordHash);
		Task<DbRegisterResult> Verify(int databaseNumber, string passwordHash, VerifyRequest request);
		Task<int> Unregister(int databaseNumber, string passwordHash);
		Task<DatabaseUniqueKey> GetDatabaseUniqueKey(int databaseNumber);

		IConnectionManager Connection { get; }
	}

	public class DbStatus
	{
		public int DatabaseNumber { get; set; }
		public string Status { get; set; }
		public string Product { get; set; }
	}

	public class DbRegisterResult
	{
		public int ReturnCode { get; set; }

		public int DatabaseNumber { get; set; }
		public string DatabaseType { get; set; }
		public string HostedLocation { get; set; }
		public string DatabaseSecurityMode { get; set; }
		public string EnterpriseCode { get; set; }
		public string ServerCode { get; set; }
		public DateTime? ManualLicenceExpiry { get; set; }
		public string CustomExpiredMessage { get; set; }
		public string CustomExpiryWeekMessage { get; set; }
		public string CustomExpiryMonthMessage { get; set; }
		public DateTime UtcNow { get; set; }
		public double CurrentBillingTimeZoneUtcOffset { get; set; }
		public double NextBillingTimeZoneUtcOffset { get; set; }
		public DateTime NextUtcOffsetEffectiveTimeUtc { get; set; }
		public string BillingModel { get; set; }
		public bool IsInternalSystem { get; set; }
	}
}
