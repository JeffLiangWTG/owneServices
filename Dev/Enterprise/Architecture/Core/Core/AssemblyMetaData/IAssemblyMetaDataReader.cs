using System.Collections.Generic;
using CargoWise.Definitions;

namespace Enterprise.ZArchitecture
{
	public interface IAssemblyMetaDataReader
	{
		IEnumerable<TAssemblyMetaDataAttribute> GetAttributes<TAssemblyMetaDataAttribute>(bool retrieveForAllClients) where TAssemblyMetaDataAttribute : AssemblyMetaDataAttribute;

		string[] AssemblyMetaDataFiles { get; }

		bool FilesExist { get; }
	}
}
