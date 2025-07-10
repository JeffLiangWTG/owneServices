using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#pragma warning disable IDE0005
using System.Linq;
#pragma warning restore IDE0005
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Authentication.Glow.Client;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using WTG.Foundation.Cryptography;
using Rfc2898DeriveBytes = WTG.Foundation.Cryptography.Algorithms.Rfc2898DeriveBytes;

namespace Enterprise.ZArchitecture.GlowInterop
{
	class UserEnv
	{
		public Guid UserPK { get; set; }
		public Guid Branch { get; set; }
		public Guid Department { get; set; }
	}

	class PersistentSingleSignOnAuthenticationController : IAuthenticationController
	{
		public PersistentSingleSignOnAuthenticationController(IUserSession userSession, UserEnv userEnv, bool persistToFile)
		{
			this.userSession = userSession;
			this.persistToFile = persistToFile;
			this.userEnv = userEnv;
		}

		static readonly Mutex mutex = new();
		readonly IUserSession userSession;
		readonly bool persistToFile;
		readonly UserEnv userEnv;

		public string TestPath { get; set; }

		public async Task AuthenticateAsync(IClientSessionService clientSessionService)
		{
			using (Db.DisposableActionForDbConnection())
			{
				userSession.AuthenticationToken = ObjectFactory.Get<IGlowSingleSignOnTokenProvider>().CreateCaptiveLimitedToken(userEnv.UserPK, userEnv.Branch, userEnv.Department);
			}

			var details = await clientSessionService.BeginSessionWithSingleSignOnTokenAsync().ConfigureAwait(false);
			if (details.AuthenticationResult == AuthenticationResult.Success)
			{
				userSession.SessionId = details.SessionId;
			}
			else
			{
				throw new AuthorizationFailureException(details.AuthenticationResult);
			}
		}

		public Task<SessionAuthenticationDetails> LoadAuthenticationStateAsync()
		{
			SessionAuthenticationDetails result;

			if (!persistToFile)
			{
				result = default;
				return Task.FromResult(result);
			}

			var filePath = GetFilePath();
			if (mutex.WaitOne())
			{
				try
				{
					var toDecrypt = File.ReadAllBytes(filePath);
					if (AesTryDecrypt(toDecrypt, out var decryptedData))
					{
						var contents = Encoding.UTF8.GetString(decryptedData);
						result = ReadSessionDetails(contents);
					}
					else
					{
						result = default;
					}
				}
				catch (FileNotFoundException)
				{
					result = default;
				}
				catch (DirectoryNotFoundException)
				{
					result = default;
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
			else
			{
				result = default;
			}

			return Task.FromResult(result);
		}

		SessionAuthenticationDetails ReadSessionDetails(string contents)
		{
			var values = contents.Split(new[] { System.Environment.NewLine },
				StringSplitOptions.None);

			if (values.Length != 3)
			{
				return default;
			}

			if (Guid.TryParse(values[2], out var guid))
			{
				return new SessionAuthenticationDetails
				{
					AuthenticationToken = values[0],
					AuthenticationTicketHeader = values[1],
					SessionId = guid
				};
			}

			return default;
		}

		public Task SaveAuthenticationStateAsync(SessionAuthenticationDetails details)
		{
			if (!persistToFile)
			{
				return Task.CompletedTask;
			}

			var filePath = GetFilePath();

			var contents = string.Join(
				System.Environment.NewLine,
				details.AuthenticationToken,
				details.AuthenticationTicketHeader,
				details.SessionId.ToString()
			);

			var toEncrypt = Encoding.UTF8.GetBytes(contents);
			var encrypted = AesEncrypt(toEncrypt);

			if (mutex.WaitOne())
			{
				try
				{
					File.WriteAllBytes(filePath, encrypted);
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}

			return Task.CompletedTask;
		}

		[SuppressMessage("Enterprise", "EDI012", Justification = "Path names are exempt")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "file path, file name")]
		string GetFilePath()
		{
			var path = Path.Combine(
					System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
					"WiseTech Global", "CargoWise One",
					Db.ServerName,
					Db.DatabaseName
				);

			if (!TestPath.IsNullOrEmpty())
			{
				path = Path.Combine(TestPath,
					Db.ServerName,
					Db.DatabaseName);
			}

			Directory.CreateDirectory(path);

			return Path.Combine(path, "glowauth.dat");
		}

		byte[] AesEncrypt(byte[] dataToEncrypt)
		{
			var iv = GenerateCryptoRandomIV();
			var encryptedData = GetEncoder(iv, userEnv.UserPK).Encrypt(dataToEncrypt);

			// save the iv to the first 16 bytes of the file
			var result = new byte[ivLength + encryptedData.Length];
			iv.CopyTo(result, 0);
			encryptedData.CopyTo(result, ivLength);
			return result;
		}

		bool AesTryDecrypt(byte[] dataToDecrypt, out byte[] decryptedData)
		{
			decryptedData = null;

			if (dataToDecrypt.Length < ivLength)
			{
				return false;
			}

			// read the iv from the first 16 bytes of the encrypted file
			var iv = new ArraySegment<byte>(dataToDecrypt, 0, ivLength).ToArray();
			var encryptedSessionData = new ArraySegment<byte>(dataToDecrypt, ivLength, dataToDecrypt.Length - ivLength).ToArray();

			try
			{
				decryptedData = GetEncoder(iv, userEnv.UserPK).Decrypt(encryptedSessionData);
				return true;
			}
			catch (CryptographicException)
			{
				return false;
			}
		}

		byte[] GenerateCryptoRandomIV()
		{
			using (var rng = RandomNumberGenerator.Create())
			{
				var iv = new byte[ivLength];
				rng.GetNonZeroBytes(iv);
				return iv;
			}
		}

		static AESCryptographicProvider GetEncoder(byte[] iv, Guid userPK)
		{
			var key = new Rfc2898DeriveBytes(userPK.ToByteArray(), salt, 1000, HashAlgorithmName.SHA1);
			return new AESCryptographicProvider(key.GetBytes(32), iv);
		}

#pragma warning disable CW1021 // Static Fields Are Thread Static Rule - constant
		static readonly byte[] salt = new Guid("14573149-5A09-4D55-AFD1-9F7734031D54").ToByteArray();
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule - constant
		const int ivLength = 16;
	}
}
