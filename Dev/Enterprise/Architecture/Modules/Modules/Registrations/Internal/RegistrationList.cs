using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Modules
{
	public class RegistrationList<TRegistrationIdentifier, TRegistrationInfo>
		where TRegistrationIdentifier : RegistrationIdentifier
		where TRegistrationInfo : RegistrationInfo
	{
		public string[] GetCountryOverridesRegistered(TRegistrationIdentifier iD)
		{
			List<string> result = new List<string>();
			Dictionary<string, TRegistrationInfo> hashByCountry;
			if (HashByID.TryGetValue(iD.ToString(), out hashByCountry))
			{
				result.AddRange(hashByCountry.Keys);
			}
			return result.ToArray();
		}

		protected void Add(TRegistrationInfo[] infos)
		{
			foreach (TRegistrationInfo info in infos)
			{
				Add(info);
			}
		}

		protected virtual void Add(TRegistrationInfo info)
		{
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(info.ID, nameof(info.ID));
			Argument.NotNull(info.CountryCode, nameof(info.CountryCode));
			Dictionary<string, TRegistrationInfo> hashByCountry;
			if (!HashByID.TryGetValue(info.ID.ToString(), out hashByCountry))
			{
				hashByCountry = new Dictionary<string, TRegistrationInfo>();
				HashByID[info.ID.ToString()] = hashByCountry;
			}

			if (hashByCountry.ContainsKey(info.CountryCode))
			{
				if (info.IsClientOverride && info.Equals(hashByCountry[info.CountryCode]) && !info.CanOverrideForClientWithNoCountryWhenOtherCountryOverridesExist)
				{
					var applicationException = new ApplicationException(
						"Sorry, you cannot have a country-specific and a non-country-specific identifier with a " +
						"non-country-specific client override identifier");

					applicationException.Data.Add(nameof(info.ID), info.ID.Name);
					applicationException.Data.Add(nameof(info.CountryCode), info.CountryCode);
					applicationException.Data.Add(nameof(info.TypePath), info.TypePath);
					applicationException.Data.Add("PreviousStackTrace", previousStackTrace);

					throw applicationException;
				}
				throw new ApplicationException("Sorry, can't register second entry with ID '"
					+ info.ID.ToString() + "' and CountryCode '" + info.CountryCode + "'");
			}

			hashByCountry[info.CountryCode] = info;
			if (info.ID.ToString() == ModuleIDs.JobConsol.ToString() && string.IsNullOrEmpty(info.CountryCode))
			{
				previousStackTrace = Environment.StackTrace;
			}
		}

		public TRegistrationIdentifier GetRegisteredIdentifierByName(string identifier)
		{
			Dictionary<string, TRegistrationInfo> registrationInfos = null;
			HashByID.TryGetValue(identifier, out registrationInfos);

			TRegistrationIdentifier result = null;
			if (registrationInfos != null)
			{
				List<TRegistrationInfo> registrationInfosForAllCountries = new List<TRegistrationInfo>(registrationInfos.Values);
				if (registrationInfosForAllCountries.Count > 0)
				{
					result = (TRegistrationIdentifier)registrationInfosForAllCountries[0].ID;
				}
			}
			return result;
		}

		public TRegistrationInfo this[TRegistrationIdentifier iD, string countryCode]
		{
			get { return this[iD, countryCode, true]; }
		}

		public TRegistrationInfo this[TRegistrationIdentifier iD, string countryCode, bool useFallbackIfRegistrationForCountryNotFound]
		{
			get
			{
				TRegistrationInfo result = null;
				countryCode = countryCode ?? "";

				if (iD == null)
				{
					throw new ArgumentNullException(nameof(iD), String.Format("(CountryCode: '{0}')", countryCode));
				}

				Dictionary<string, TRegistrationInfo> hashByCountry;
				if (HashByID.TryGetValue(iD.ToString(), out hashByCountry))
				{
					if (hashByCountry == null)
					{
						throw new InvalidOperationException(String.Format("HashByID returns HashByCountry that is null (ID: {0}, CountryCode: '{1}')", iD.ToString(), countryCode));
					}

					hashByCountry.TryGetValue(countryCode, out result);
					if (result == null && useFallbackIfRegistrationForCountryNotFound)
					{
						hashByCountry.TryGetValue("", out result);
					}
				}

				return result;
			}
		}

		#region Implementation

		readonly Dictionary<string, Dictionary<string, TRegistrationInfo>> HashByID = new Dictionary<string, Dictionary<string, TRegistrationInfo>>();
		string previousStackTrace = string.Empty;

		internal ClientHook RegistrationOverrides
		{
			get { return ClientHookLoader.Instance.ClientHook; }
		}

		#endregion

		public IEnumerable<TRegistrationInfo> All
		{
			get { return HashByID.Values.SelectMany(value => value.Values); }
		}

#if DEBUG
		public void ClearForTesting()
		{
			HashByID.Clear();
		}
#endif
	}
}
