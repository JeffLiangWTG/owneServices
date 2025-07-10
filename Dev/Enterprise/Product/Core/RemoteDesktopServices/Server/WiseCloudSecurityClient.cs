using System;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.Security;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class WiseCloudSecurityClient : IWiseCloudSecurityClient
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public string GetClientIPAddress(string license, string username)
		{
			if (EnterpriseChannel.Instance == null)
			{
				throw new IOException($"{ClientVersion.ProductName} is not enabled.");
			}

			var message = new GetClientIPAddressMessage() { domain = GetDomain(), timestamp = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture), token = license + ";" + username + ";" + Guid.NewGuid().ToString() };
			var result = EnterpriseChannel.Instance.SendMessage<GetClientIPAddressMessage, GetClientIPAddressResultMessage>(EnterpriseChannelMessageTypes.GetClientIPAddress, message);

			using (var wcaSecurityProviderCypto = new RSACryptoServiceProvider())
			using (var hashAlgorithm = SHA256.Create())
			{
				wcaSecurityProviderCypto.FromXmlString(WCASecurityProviderPublicKey);
				if (!wcaSecurityProviderCypto.VerifyData(Encoding.UTF8.GetBytes(result.clientIPAddress + "\t" + message.timestamp + "\t" + message.token), hashAlgorithm, result.signature))
				{
					throw new IOException("WCA message signature failed validation");
				}
			}

			return result.clientIPAddress;
		}

		protected virtual string GetDomain()
		{
			return Domain.GetComputerDomain().Name;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual string WCASecurityProviderPublicKey
		{
			get { return "<RSAKeyValue><Modulus>pFEPXoJID+/SOarZKfTig21IMrHu2JDz7kDskKYIO+/g70PpGZDbS/w/tjxmAH41BlOnr2TVT571mBS82QF6hyyI0q22GQkzGczmu4JSADM/yfIl+aHjqScWHQ5PDqfY8D392SKFdy8Q2rTgc04+8zoM5UNwoPFICIawMbb2SWU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>"; }
		}
	}
}
