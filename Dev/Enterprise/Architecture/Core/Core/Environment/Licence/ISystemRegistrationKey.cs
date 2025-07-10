using System;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public interface ISystemRegistrationKey
	{
		string DatabaseName { get; }
		string DatabaseType { get; }
		string DbInstanceName { get; }
		string DbSecurityMode { get; }
		Guid ServerSid { get; }
		DateTime SystemExpiryDate { get; }

		/// <summary>
		/// Globally unique system ID issued from ediProd
		/// </summary>
		string SystemId { get; }

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

		string BillingModel { get; }

		double CurrentBillingTimeZoneUtcOffset { get; }
		double NextBillingTimeZoneUtcOffset { get; }
		DateTime NextUtcOffsetEffectiveTimeUtc { get; }

		string ToEncryptedKeyString();
	}

	[Serializable]
	public class SystemRegistrationKeyMissingInformationException : ArgumentException
	{
		public SystemRegistrationKeyMissingInformationException(string parameterName)
			: base(NotEnoughInformationToGenerateKey, parameterName)
		{
		}

#if NETFRAMEWORK
		protected SystemRegistrationKeyMissingInformationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		internal static string NotEnoughInformationToGenerateKey
		{
			get { return Res.GetString("e449ea50-3598-4987-8f78-cfacfe4ed722", "Not enough information to generate a system key."); }
		}
	}

	[Serializable]
	public class SystemRegistrationInvalidKeyException : InvalidOperationException
	{
		public SystemRegistrationInvalidKeyException(Exception innerException)
			: base(EncryptedKeyWasCorrupt, innerException)
		{
		}

#if NETFRAMEWORK
		protected SystemRegistrationInvalidKeyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		internal static string EncryptedKeyWasCorrupt
		{
			get { return Res.GetString("ac3c5d6d-70af-4c37-ab28-fecfcc0ecbd4", "The Encrypted System Key was invalid or corrupt"); }
		}
	}
}
