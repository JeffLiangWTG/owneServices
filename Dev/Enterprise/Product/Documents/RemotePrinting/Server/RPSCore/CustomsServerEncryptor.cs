using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server.RPSCore;

sealed class CustomsServerEncryptor
{
	public static string Encrypt(IAuthentication authentication, string data)
	{
		return Encrypt(HttpContext.Current?.User?.Identity, authentication, data);
	}

	public static string Encrypt(IIdentity identity, IAuthentication authentication, string data)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			return string.Empty;
		}

		return Encrypt(identity, authentication, Encoding.UTF8.GetBytes(data));
	}

	public static string Encrypt(IIdentity identity, IAuthentication authentication, byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			return string.Empty;
		}

		var userName = identity?.Name ?? string.Empty;

		if (string.IsNullOrWhiteSpace(userName))
		{
			return Convert.ToBase64String(data);
		}

		var credentials = GetPasswordOrToken(userName, authentication);
		if (string.IsNullOrWhiteSpace(credentials.PasswordOrToken))
		{
			return Convert.ToBase64String(data);
		}

		var rgbKey = GetRGBValue(userName);
		var rgbIV = GetRGBValue(credentials.PasswordOrToken);

		using var algorithm = Aes.Create();
		algorithm.Padding = PaddingMode.Zeros;
		using var encryptor = algorithm.CreateEncryptor(rgbKey, rgbIV);
		using var transformationStream = new MemoryStream();
		using var encryptStream = new CryptoStream(transformationStream, encryptor, CryptoStreamMode.Write);
		encryptStream.Write(data, 0, data.Length);
		encryptStream.FlushFinalBlock();

		return credentials.Prefix + Convert.ToBase64String(transformationStream.ToArray());
	}

	static (string Prefix, string PasswordOrToken) GetPasswordOrToken(string userName, IAuthentication authentication)
	{
		if (userName == authentication.ApplicationUser)
		{
			return ("#", authentication.ApplicationPwd);
		}
		else if (authentication.AlternativeCredentials?.ContainsCode(userName) ?? false)
		{
			return ("*", authentication.AlternativeCredentials.GetDescriptionFromCode(userName));
		}
		else if (userName.StartsWith(Authentication.SupportUserPrefix, StringComparison.InvariantCultureIgnoreCase))
		{
			return ("@", userName.Substring(Authentication.SupportUserPrefix.Length));
		}

		return (string.Empty, string.Empty);
	}

	static byte[] GetRGBValue(string source)
	{
		return RGBValues.GetOrAdd(source, s =>
		{
			using var md5 = MD5.Create();
			var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(s));
			var hashString = BitConverter.ToString(hash).ToLower().Replace("-", "").Substring(5, 16);

			return Encoding.UTF8.GetBytes(hashString);
		});
	}

	[ThreadSafe]
	static readonly ConcurrentDictionary<string, byte[]> RGBValues = new ConcurrentDictionary<string, byte[]>();
}
