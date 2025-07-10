using System;
using System.Threading;

namespace Enterprise.Integration.Licensing
{
	public enum ProductRegistrationVerifyResult
	{
		OK,
		Unregistered,
		Fail,
		Timeout,
		Error,
		NotFound
	}

	public enum ProductRegistrationRegisterResult
	{
		OK,
		ProductKeyNotFound,
		ProductKeyUnavailable,
		Fail,
		Timeout,
		Error
	}

	public enum ProductRegistrationUnregisterResult
	{
		OK,
		NotRegistered,
		Timeout,
		Error
	}

	public enum ProductRegistrationProduct
	{
		CargoWiseOne,
		CargoWiseNext
	}

	public interface IProductRegistrationKey
	{
		/// <summary>
		/// Globally unique system ID issued from ediProd
		/// </summary>
		string SystemId { get; }
		int DatabaseNumber { get; }

		/// <summary>
		/// Enterprise code of the parent organization in our records.
		/// Usually used as part of a nine character code identifying a company on a database, i.e. 
		///		{enterprise code}{company code}{server code}
		/// Not invariant since organizations can change their enterprise code.
		/// </summary>
		string EnterpriseCode { get; }

		/// <summary>
		/// Server code for the database.
		/// Uniquely identifies the database when combined with the enterprise code.
		/// </summary>
		string ServerCode { get; }

		string DatabaseType { get; }
		string DbSecurityMode { get; }

		DateTime SystemExpiryDate { get; }

		/// <summary>
		/// Hosted location. Possible values:
		/// empty - unknown (key comes from a time when hosted location was not included)
		/// Core.Constants.LicenceConstants.NotHostedWithCargoWise - not hosted with cargowise
		/// SYD/CHI/LON/etc - hosted location code
		/// </summary>
		string HostedLocation { get; }

		/// <summary>
		/// Message shown when system has expired
		/// </summary>
		string ExpiredMessage { get; }

		/// <summary>
		/// Message shown when system is within a week of expiry
		/// </summary>
		string ExpiryWeekMessage { get; }

		/// <summary>
		/// Message shown when system is within a month and over a week of expiry.
		/// </summary>
		string ExpiryMonthMessage { get; }

		/// <summary>
		/// Password generated for this registration.
		/// Used when calling other services.
		/// </summary>
		string Password { get; }

		/// <summary>
		/// Name of registered server.
		/// </summary>
		string ServerName { get; }

		/// <summary>
		/// Name of registered database.
		/// </summary>
		string DatabaseName { get; }

		/// <summary>
		/// True if this system need only verify the connection server name and database name.
		/// The database creation date and/or availability group ID is ignored.
		/// Typically used for WiseTech managed systems such as hosted customers or internal systems
		/// where we need to migrate databases to different physical servers while using the same DNS connection name.
		/// </summary>
		bool? CanVerifyByNameOnly { get; }

		/// <summary>
		/// True if this is one of our internal systems like a UAT or ediProd, and not a customer system.
		/// </summary>
		bool? IsInternalSystem { get; }

		double CurrentBillingTimeZoneUtcOffset { get; }
		double NextBillingTimeZoneUtcOffset { get; }
		DateTime NextUtcOffsetEffectiveTimeUtc { get; }

		string BillingModel { get; }
	}

#if DEBUG
	public interface IProductRegistrationKeyForTest : IProductRegistrationKey
	{
		string SystemIdForTest { get; set; }
		int DatabaseNumberForTest { get; set; }
		string EnterpriseCodeForTest { get; set; }
		string ServerCodeForTest { get; set; }
		string DatabaseTypeForTest { get; set; }
		string DbSecurityModeForTest { get; set; }
		DateTime SystemExpiryDateForTest { get; set; }
		string HostedLocationForTest { get; set; }
		string ExpiredMessageForTest { get; set; }
		string ExpiryWeekMessageForTest { get; set; }
		string ExpiryMonthMessageForTest { get; set; }
		string PasswordForTest { get; set; }
		bool? CanVerifyByNameOnlyForTest { get; set; }
		bool? IsInternalSystemForTest { get; set; }
		double CurrentBillingTimeZoneUtcOffsetForTest { get; set; }
		double NextBillingTimeZoneUtcOffsetForTest { get; set; }
		DateTime NextUtcOffsetEffectiveTimeUtcForTest { get; set; }
		string BillingModelForTest { get; set; }
	}
#endif

	public interface IProductRegistration
	{
		IProductRegistrationKey Key { get; }

#if DEBUG
		/// <summary>
		/// An editable key for unit tests.
		/// Any changes will be automatically cleared at the end of the test (by ZEnvironmentListener.AfterEachTest).
		/// </summary>
		IProductRegistrationKeyForTest KeyForTest { get; }

		/// <summary>
		/// Call this if the key needs to be reset in the middle of test.
		/// </summary>
		void ResetKeyToDefault();
#endif

		ProductRegistrationRegisterResult Register(string productKey, CancellationToken cancelToken, int timeoutMs = 20000);

		ProductRegistrationVerifyResult Verify(CancellationToken cancelToken, int timeoutMs = 20000);

		/// <summary>
		/// Verify the registration as fully as possible.
		/// Does the same checks as Verify, but if the registration is locally invalid, also does a remote check
		/// to determine if the installation has been unregistered by support.
		/// Should only be called from the Register UI.
		/// </summary>
		ProductRegistrationVerifyResult FullVerify(CancellationToken cancelToken, int timeoutMs = 20000);

		ProductRegistrationUnregisterResult Unregister(CancellationToken cancelToken, int timeoutMs = 20000);

		/// <summary>
		/// Verify the registration as far as possible without contacting the remote web service.
		/// Checks that the registration key is present and its database key matches the current database.
		/// </summary>
		ProductRegistrationVerifyResult LocalVerify();

		ProductRegistrationRegisterResult TryAutoRegisterAfterUpgrade();

		ProductRegistrationProduct GetProduct();

		string LastError { get; }

		bool IsWiseTechGlobalInternalSystem();

		bool IsWiseTechGlobalInternalProdSystem();

		bool IsWiseTechGlobalInternalUATSystem();

		bool IsWiseTechGlobalInternalDeveloperSystem();

		bool IsWiseTechGlobalInternalTrainingSystem();

		bool IsWiseTechGlobalInternalEDISystem();
	}
}
