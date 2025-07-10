using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ProductRegistrationPeriodicChecker : IProductRegistrationPeriodicChecker
	{
		public ProductRegistrationPeriodicChecker(IMemoryCache memoryCache, IHostLogger hostLogger, IProductRegistration productRegistration)
		{
			this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.productRegistration = productRegistration ?? throw new ArgumentNullException(nameof(productRegistration));
		}

		bool? IsProductRegistered(bool localOnly)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var status = localOnly
						? productRegistration.LocalVerify()
						: productRegistration.Verify(CancellationToken.None);

					switch (status)
					{
						case ProductRegistrationVerifyResult.OK:
							return true;
						case ProductRegistrationVerifyResult.Fail:
						case ProductRegistrationVerifyResult.Unregistered:
						case ProductRegistrationVerifyResult.NotFound:
							return false;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}

			return null;
		}

		bool RetrieveOrUpdateItem(bool localOnly, bool defaultValue)
		{
			return ItemData(
				localOnly ? MemoryCacheLocalReference : MemoryCacheRemoteReference,
				localOnly ? MemoryCacheLocalExpiration : MemoryCacheRemoteExpiration);

			bool ItemData(string memoryCacheReference, TimeSpan registrationExpireTimeSpan)
			{
				return memoryCache.AddOrGetExisting(
					memoryCacheReference,
					() =>
					{
						var result = IsProductRegistered(localOnly) ?? defaultValue;
						registrationExpireTimeSpan = result ? registrationExpireTimeSpan : MemoryCacheUnregisteredProductExpiration;
						return result;
					},
					() => registrationExpireTimeSpan);
			}
		}

		public bool IsProductRegisteredAsNonTrialSystemOrUnknown()
		{
			var localCheck = RetrieveOrUpdateItem(true, false);
			var remoteCheck = RetrieveOrUpdateItem(false, true);
			var licenceType = productRegistration.Key?.DatabaseType;
			var trialSystem = string.Equals(licenceType, DatabaseTypes.Codes.WisecloudTrial, StringComparison.OrdinalIgnoreCase);

			bool isRegistered = remoteCheck
				&& localCheck
				&& !trialSystem;

			if (!isRegistered)
			{
				LogIfTimerExpired();

				void LogIfTimerExpired()
				{
					memoryCache.AddOrGetExisting(
						MemoryCacheUnregisteredProductLogReference,
						() =>
						{
							hostLogger.Log(LogLevel.Warning, $"System is unregistered (local registration: [{localCheck}], remote registration: [{remoteCheck}], type: [{licenceType}])");
							isUnRegisteredProductWarningLogged = true;
							return new object();
						},
						MemoryCacheUnregisteredProductExpiration);
				}
			}
			else
			{
				if (isUnRegisteredProductWarningLogged)
				{
					isUnRegisteredProductWarningLogged = false;
					hostLogger.Log(LogLevel.Information, $"System is registered (local registration: [{localCheck}], remote registration: [{remoteCheck}], type: [{licenceType}])");
				}
			}

			return isRegistered;
		}

		internal static readonly string MemoryCacheLocalReference = $"{nameof(ProductRegistrationPeriodicChecker)}_LocalVerification";
		internal static readonly string MemoryCacheRemoteReference = $"{nameof(ProductRegistrationPeriodicChecker)}_RemoteVerification";
		internal static readonly string MemoryCacheUnregisteredProductLogReference = $"{nameof(ProductRegistrationPeriodicChecker)}_UnRegisteredProductLogVerification";
		internal static readonly TimeSpan MemoryCacheLocalExpiration = TimeSpan.FromMinutes(10);
		internal static readonly TimeSpan MemoryCacheRemoteExpiration = TimeSpan.FromMinutes(30);
		internal static readonly TimeSpan MemoryCacheUnregisteredProductExpiration = TimeSpan.FromMinutes(1);

		readonly IMemoryCache memoryCache;
		readonly IProductRegistration productRegistration;
		readonly IHostLogger hostLogger;
		bool isUnRegisteredProductWarningLogged;
	}
}
