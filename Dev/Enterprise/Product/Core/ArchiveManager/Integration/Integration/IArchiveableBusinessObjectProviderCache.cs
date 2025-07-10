using System.Collections.Generic;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveableBusinessObjectProviderCache
	{
		IArchiveableBusinessObjectProvider GetProvider(string tableName);
		IEnumerable<ReferenceKeyType> GetAllReferenceKeyTypesSupported();
		bool HasProvider(string tableName);
	}
}
