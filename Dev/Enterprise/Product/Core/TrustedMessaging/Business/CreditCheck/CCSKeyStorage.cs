using System.Collections.Generic;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;

namespace Enterprise.TrustedMessaging.Business.CreditCheck
{
	public class CCSKeyStorage : ISecretKeyStorage
	{
		readonly Dictionary<(string product, string systemId), string> keys = new Dictionary<(string product, string systemId), string>();
		public SecretKey LoadSecretKey(string product, string systemId)
		{
			return new SecretKey()
			{
				Product = product,
				RefId = systemId,
				Key = keys.ContainsKey((product, systemId)) ? keys[(product, systemId)] : null
			};
		}

		public void SaveSecretKey(SecretKey key)
		{
			var product = key.Product;
			var refId = key.RefId;
			if (keys.ContainsKey((product, refId)))
			{
				keys[(product, refId)] = key.Key;
			}
			else
			{
				keys.Add((product, refId), key.Key);
			}
		}
	}
}
