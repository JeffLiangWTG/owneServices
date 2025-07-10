using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	public interface IAuthentication
	{
		string ApplicationUser { get; }
		string ApplicationPwd { get; }
		ICodeDescriptionPairList AlternativeCredentials { get; }
		Dictionary<string, string> GetDigestAuthorizationRequestInfo(string authorizationString);
		string GetDigestHash(Dictionary<string, string> requestInfo, string httpMethod, string userPwd);
		string GetChallengeString(bool isNonceStale);
		bool IsValidNonce(string nonce);
		string GetCurrentNonce();
#if DEBUG
		bool UseProductionDateTimeForTest { get; set; }
#endif
	}

	public class Authentication : IAuthentication
	{
		Authentication()
		{
		}

		[System.Runtime.CompilerServices.SpecialName] // Using this so that Static Sniffer doesn't pick this up without us requiring a reference to Enterprise.Core.
		public static readonly IAuthentication Instance = new Authentication();

		#region User and Pwd

		public string ApplicationUser
		{
			get
			{
				if (applicationUser == null)
				{
					lock (credentialsLock)
					{
						using (DbConnection connection = Db.NewExtraConnectionToMainDb())
						{
							applicationUser = RegistryData.WebServiceUsername(connection);
						}
					}
				}
				else
				{
					CheckCredentialsDataExpired();
				}

				return applicationUser;
			}
		}
		string applicationUser;

		public string ApplicationPwd
		{
			get
			{
				if (applicationPwd == null)
				{
					lock (credentialsLock)
					{
						using (DbConnection connection = Db.NewExtraConnectionToMainDb())
						{
							applicationPwd = RegistryData.WebServicePassword(connection);
						}
					}
				}
				else
				{
					CheckCredentialsDataExpired();
				}

				return applicationPwd;
			}
		}
		string applicationPwd;

		public ICodeDescriptionPairList AlternativeCredentials
		{
			get
			{
				if (alternativeCredentials == null)
				{
					lock (credentialsLock)
					{
						using (var connection = Db.NewExtraConnectionToMainDb())
						{
							alternativeCredentials = RegistryData.WebServiceAlternativeCredentials(connection);
						}
					}
				}
				else
				{
					CheckCredentialsDataExpired();
				}

				return alternativeCredentials;
			}
		}
		ICodeDescriptionPairList alternativeCredentials;

		void CheckCredentialsDataExpired()
		{
			if (WebServiceCredentialsCacheTimeInMinutes <= 0)
			{
				return;
			}

			if (!cacheTimer.IsRunning)
			{
				lock (credentialsLock)
				{
					if (!cacheTimer.IsRunning)
					{
						cacheTimer.Start();
					}
				}
				return;
			}

			if (cacheTimer.Elapsed.TotalMinutes > WebServiceCredentialsCacheTimeInMinutes)
			{
				lock (credentialsLock)
				{
					using (var connection = Db.NewExtraConnectionToMainDb())
					{
						if (applicationUser != null)
						{
							applicationUser = RegistryData.WebServiceUsername(connection);
						}
						if (applicationPwd != null)
						{
							applicationPwd = RegistryData.WebServicePassword(connection);
						}
						if (alternativeCredentials != null)
						{
							alternativeCredentials = RegistryData.WebServiceAlternativeCredentials(connection);
						}
					}

					cacheTimer.Restart();
				}
			}
		}

		int WebServiceCredentialsCacheTimeInMinutes
		{
			get
			{
				if (!webServiceCredentialsCacheTimeInMinutes.HasValue)
				{
					lock (credentialsLock)
					{
						using (var connection = Db.NewExtraConnectionToMainDb())
						{
							webServiceCredentialsCacheTimeInMinutes = RegistryData.WebServiceCredentialsCacheTime(connection);
						}
					}
				}
				else
				{
					if (!intervalTimer.IsRunning)
					{
						lock (credentialsLock)
						{
							if (!intervalTimer.IsRunning)
							{
								intervalTimer.Start();
							}
						}
					}

					if (intervalTimer.Elapsed.TotalMinutes > MinutesToRefreshCacheTimeValue)
					{
						lock (credentialsLock)
						{
							using (var connection = Db.NewExtraConnectionToMainDb())
							{
								webServiceCredentialsCacheTimeInMinutes = RegistryData.WebServiceCredentialsCacheTime(connection);
							}
						}

						intervalTimer.Restart();
					}
				}

				return webServiceCredentialsCacheTimeInMinutes.Value;
			}
		}
		int? webServiceCredentialsCacheTimeInMinutes;

		readonly Stopwatch cacheTimer = new Stopwatch();
		readonly Stopwatch intervalTimer = new Stopwatch();
		readonly object credentialsLock = new object();

		const int MinutesToRefreshCacheTimeValue = 15;

		public const string SupportUserPrefix = "CWSupport-";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string UserRole = "user";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string SupportRole = "support";

		#endregion

		public Dictionary<string, string> GetDigestAuthorizationRequestInfo(string authorizationString)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			if (authorizationString != null && authorizationString.Length > 0)
			{
				authorizationString = authorizationString.Trim();

				// If it does not start with Digest, it is not a Digest Authorization Header
				if (authorizationString.IndexOf("Digest") == 0 && authorizationString.Length >= 7)
				{
					authorizationString = authorizationString.Substring(7);
					string[] elems = authorizationString.Split(new char[] { ',' });

					foreach (string elem in elems)
					{
						// form key="value"
						string[] parts = elem.Split(new char[] { '=' }, 2);
						string key = parts[0].Trim(new char[] { ' ', '\"' });
						string val = parts[1].Trim(new char[] { ' ', '\"' });

						result.Add(key, val);
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		public string GetDigestHash(Dictionary<string, string> requestInfo, string httpMethod, string userPwd)
		{
			Argument.NotNull(requestInfo, nameof(requestInfo));

			string username = requestInfo["username"];
			string uri = requestInfo["uri"];
			string nonce = requestInfo["nonce"];

			// A1 = unq(username-value) ":" unq(realm-value) ":" passwd
			string a1 = String.Format("{0}:{1}:{2}", username, authenticationRealm, userPwd);

			// H(A1) = MD5(A1)
			string hA1 = GetMD5HashBinHex(a1);

			// A2 = Method ":" digest-uri-value
			string a2 = String.Format("{0}:{1}", httpMethod, uri);

			// H(A2)
			string hA2 = GetMD5HashBinHex(a2);

			// KD(secret, data) = H(concat(secret, ":", data))
			// if qop == auth:
			// request-digest  = <"> < KD ( H(A1),     unq(nonce-value)
			//                              ":" nc-value
			//                              ":" unq(cnonce-value)
			//                              ":" unq(qop-value)
			//                              ":" H(A2)
			//                            ) <">
			// if qop is missing,
			// request-digest  = <"> < KD ( H(A1), unq(nonce-value) ":" H(A2) ) > <">

			string unhashedDigest;

			if (requestInfo.ContainsKey("qop"))
			{
				unhashedDigest = String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
					hA1,
					nonce,
					requestInfo["nc"],
					requestInfo["cnonce"],
					requestInfo["qop"],
					hA2);
			}
			else
			{
				unhashedDigest = String.Format("{0}:{1}:{2}",
					hA1,
					nonce,
					hA2);
			}

			string hashedDigest = GetMD5HashBinHex(unhashedDigest);
			return hashedDigest;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string GetChallengeString(bool isNonceStale)
		{
			string nonce = GetCurrentNonce();

			StringBuilder challenge = new StringBuilder("Digest");
			challenge.Append(" realm=\"");
			challenge.Append(authenticationRealm);
			challenge.Append("\"");
			challenge.Append(", nonce=\"");
			challenge.Append(nonce);
			challenge.Append("\"");
			challenge.Append(", opaque=\"0000000000000000\"");
			challenge.Append(", stale=");
			challenge.Append(isNonceStale ? "true" : "false");
			challenge.Append(", algorithm=MD5");
			challenge.Append(", qop=\"auth\"");
			string challengeString = challenge.ToString();

			return challengeString;
		}

		public bool IsValidNonce(string nonce)
		{
			Argument.NotNull(nonce, nameof(nonce));

			DateTime expireTime;

			// pad nonce on the right with '=' until length is a multiple of 4
			int numPadChars = nonce.Length % 4;

			if (numPadChars > 0)
			{
				numPadChars = 4 - numPadChars;
			}

			string newNonce = nonce.PadRight(nonce.Length + numPadChars, '=');

			try
			{
				byte[] decodedBytes = Convert.FromBase64String(newNonce);
				string expireStr = new ASCIIEncoding().GetString(decodedBytes);
				expireTime = DateTime.Parse(expireStr);
			}
			catch (FormatException)
			{
				return false;
			}

			return (GetCurrentTimeForNonce() <= expireTime);
		}

		public static bool IsSupportUser(string userName)
		{
			return userName.StartsWith(SupportUserPrefix, StringComparison.OrdinalIgnoreCase);
		}

		public static string GetStaffCodeFromSupportUser(string userName)
		{
			string staffCode = string.Empty;
			if (IsSupportUser(userName))
			{
				staffCode = userName.Substring(SupportUserPrefix.Length);
			}
			return staffCode;
		}

		string GetMD5HashBinHex(string val)
		{
			Argument.NotNull(val, nameof(val));

			Encoding enc = new ASCIIEncoding();
			var md5 = MD5.Create();
			byte[] bHA1 = md5.ComputeHash(enc.GetBytes(val));
			string hA1 = "";
			for (int i = 0; i < 16; i++)
			{
				hA1 += String.Format("{0:x02}", bHA1[i]);
			}

			return hA1;
		}

		/// <summary>
		/// This implementation will create a nonce which is
		/// the text representation of the current time, plus one minute.
		/// The nonce will be valid for this one minute.
		/// </summary>
		/// <returns></returns>
		public string GetCurrentNonce()
		{
			var nonceTime = GetCurrentTimeForNonce() + TimeSpan.FromMinutes(1);
			string expireStr = nonceTime.ToString("G");

			Encoding enc = new ASCIIEncoding();
			byte[] expireBytes = enc.GetBytes(expireStr);
			string nonce = Convert.ToBase64String(expireBytes);

			// nonce can't end in '=', so trim them from the end
			nonce = nonce.TrimEnd(new Char[] { '=' });
			return nonce;
		}

		DateTime GetCurrentTimeForNonce()
		{
#if DEBUG
			if (Enterprise.ZArchitecture.Environment.Globals.IsTest && !UseProductionDateTimeForTest)
			{
				return ZDateTime.UtcNow.ToDateTime();
			}
#endif

			// DateTime.Now is calculated from DateTime.UtcNow, so use UtcNow to remove unnecessary calculations.
			return DateTime.UtcNow;
		}

#if DEBUG
		public bool UseProductionDateTimeForTest { get; set; }
#endif

		readonly string authenticationRealm = "RemotePrinting";
	}
}
