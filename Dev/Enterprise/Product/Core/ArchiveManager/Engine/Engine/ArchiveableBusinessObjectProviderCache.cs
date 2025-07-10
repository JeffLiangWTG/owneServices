using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Caches ArchiveableBusinessObjectProviders system-wide
	/// </summary>
	public class ArchiveableBusinessObjectProviderCache : IArchiveableBusinessObjectProviderCache
	{
		public void RegisterProvider(IArchiveableBusinessObjectProvider provider)
		{
			_ = Argument.NotNull(provider, "provider");

			foreach (string tableName in provider.TableNamesSupported)
			{
				if (HasProvider(tableName))
				{
					throw new InvalidOperationException("Cannot register two providers for the same table name: " + tableName);
				}
				else
				{
					providerDictionary.Add(tableName, provider);
				}
			}
		}

		#region IArchiveableBusinessObjectProviderCache Members

		public IArchiveableBusinessObjectProvider GetProvider(string tableName)
			=> providerDictionary[tableName];

		public bool HasProvider(string tableName)
			=> providerDictionary.ContainsKey(tableName);

#if NETFRAMEWORK
		public IEnumerable<ReferenceKeyType> GetAllReferenceKeyTypesSupported()
			=> providerDictionary
				.SelectMany(kv => kv.Value.ReferenceKeyTypesSupported)
				.DistinctBy(rt => rt.Code);
#else
		public IEnumerable<ReferenceKeyType> GetAllReferenceKeyTypesSupported()
			=> Enumerable.DistinctBy(
				providerDictionary.SelectMany(kv => kv.Value.ReferenceKeyTypesSupported), rt => rt.Code);
#endif
		#endregion

		readonly Dictionary<string, IArchiveableBusinessObjectProvider> providerDictionary = new Dictionary<string, IArchiveableBusinessObjectProvider>();
	}
}
