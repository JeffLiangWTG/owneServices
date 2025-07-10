using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Client;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer")]
sealed class CustomsClientDecryptor
{
	public CustomsClientDecryptor(ICustomsMessageController controller, ICustomseHubClientSetting clientSetting)
	{
		this.controller = controller;
		this.clientSetting = clientSetting;
	}

	readonly ICustomsMessageController controller;
	readonly ICustomseHubClientSetting clientSetting;

	public void DecryptIfNeeded()
	{
		controller.ShowInformation($"Decrypt credential on [{clientSetting.GetHashCode()}] {clientSetting.GetType().Name}.");

		switch (clientSetting)
		{
			case CNSWClientApplicationSettingWrapper cnWrapper:
				{
					var instance = cnWrapper.Instance;
					if (!instance.IsDecrypted)
					{
						instance.DecryptedEHubClientPassword = Decrypt(instance.EHubClientPassword);
						instance.IsDecrypted = true;
					}
					break;
				}

			case CLSMSClientApplicationSettingWrapper clWrapper:
				{
					var instance = clWrapper.Instance;
					if (!instance.IsDecrypted)
					{
						instance.DecryptedApplicationNodePassword = Decrypt(instance.ApplicationNodePassword);
						instance.IsDecrypted = true;
					}
					break;
				}

			case JPNACCSClientApplicationSettingWrapper jpWrapper:
				{
					var instance = jpWrapper.Instance;
					if (!instance.IsDecrypted)
					{
						instance.DecryptedxTPassword = Decrypt(instance.xTPassword);

						foreach (var mailbox in instance.MailBoxInfos ?? [])
						{
							mailbox.DecryptedMailBoxPassword = Decrypt(mailbox.MailBoxPassword);
						}

						instance.IsDecrypted = true;
					}
					break;
				}

			case TWNCATKClientApplicationSettingWrapper twWrapper:
				{
					var instance = twWrapper.Instance;
					if (!instance.IsDecrypted)
					{
						instance.DecryptedEHubClientPassword = Decrypt(instance.EHubClientPassword);
						instance.IsDecrypted = true;
					}
					break;
				}
		}
	}

	string Decrypt(string encryptedString)
	{
		if (string.IsNullOrWhiteSpace(encryptedString))
		{
			return encryptedString;
		}

		var isBase64Text = false;
		byte[] encryptedData = null;

		try
		{
			encryptedData = Convert.FromBase64String(SafePrefixes.Any(c => encryptedString.StartsWith(c.Prefix)) ? encryptedString.Substring(1) : encryptedString);
			isBase64Text = true;
		}
		catch
		{
			controller.ShowInformation("Encrypted string in unexpected format.");
			encryptedData = Encoding.UTF8.GetBytes(encryptedString);
		}

		var (user, password) = GetAccountDetailsSafe(encryptedString);

		if (!isBase64Text || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
		{
			return Encoding.UTF8.GetString(encryptedData);
		}

		password = ProtectedDataHelper.Unprotect(password);

		var rgbKey = GetRGBValue(user);
		var rgbIV = GetRGBValue(password);

		var sourceBuffer = new ArraySegment<byte>(encryptedData);
		var targetBuffer = new ArraySegment<byte>(new byte[encryptedString.Length]);

		return DecryptCore(sourceBuffer, targetBuffer, rgbKey, rgbIV);
	}

	(string User, string Password) GetAccountDetailsSafe(string encryptedString)
	{
		(string User, string Password) LoadUserNameAndPassword(bool reload)
		{
			if (reload)
			{
				controller.ResetConfigSetting();
			}

			try
			{
				var configuration = controller.ConfigSetting;
				return (configuration.WebServiceUser, configuration.WebServicePwd);
			}
			catch
			{
				controller.ShowInformation("Failed to load config setting.");
				return (string.Empty, string.Empty);
			}
		}

		var result = LoadUserNameAndPassword(false);

		var prefixInfo = SafePrefixes.FirstOrDefault(c => encryptedString.StartsWith(c.Prefix));

		if (!string.IsNullOrWhiteSpace(prefixInfo.Prefix))
		{
			if (string.IsNullOrWhiteSpace(result.User) || string.IsNullOrWhiteSpace(result.Password))
			{
				LoadUserNameAndPassword(true);

				if (string.IsNullOrWhiteSpace(result.User) || string.IsNullOrWhiteSpace(result.Password))
				{
					throw new InvalidOperationException($@"Incorrect encryption password with unregistered {prefixInfo.AccountType} account details for ""{clientSetting.MachineName}"", please check the account name and password for connecting to the Service in the current configuration.");
				}
			}
		}
		else if (!string.IsNullOrWhiteSpace(result.User) && !string.IsNullOrWhiteSpace(result.Password))
		{
			LoadUserNameAndPassword(true);

			if (!string.IsNullOrWhiteSpace(result.User) && !string.IsNullOrWhiteSpace(result.Password))
			{
				throw new InvalidOperationException($@"Incorrect encryption password with registered account details for ""{clientSetting.MachineName}"", please check the account name and password for connecting to the Service in the current configuration.");
			}
		}

		return result;
	}

	[ThreadSafe]
	static readonly (string Prefix, string AccountType)[] SafePrefixes = [("*", "alternative"), ("#", "application"), ("@", "support")];

	string DecryptCore(ArraySegment<byte> sourceBuffer, ArraySegment<byte> targetBuffer, byte[] rgbKey, byte[] rgbIV)
	{
		int bytesRead;
		var totalBytesRead = 0;

		try
		{
			using var algorithm = Aes.Create();
			algorithm.Padding = PaddingMode.Zeros;
			using var decryptor = algorithm.CreateDecryptor(rgbKey, rgbIV);
			using var transformationStream = new MemoryStream(sourceBuffer.Array, sourceBuffer.Offset, sourceBuffer.Count);
			using var decryptStream = new CryptoStream(transformationStream, decryptor, CryptoStreamMode.Read);
			do
			{
				bytesRead = decryptStream.Read(targetBuffer.Array, targetBuffer.Offset + totalBytesRead, targetBuffer.Count - totalBytesRead);
				totalBytesRead += bytesRead;
			}
			while (bytesRead > 0);

			decryptStream.Clear();
			decryptStream.Dispose();
		}
		catch (IndexOutOfRangeException)
		{
		}

		var decryptResult = Encoding.UTF8.GetString(targetBuffer.Array, targetBuffer.Offset, totalBytesRead);
		return Regex.Replace(decryptResult, @"[^\t\r\n -~]", string.Empty);
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
