using System.Collections.Generic;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture
{
	public interface IModuleFactory
	{
		string[] GetCountryOverridesRegisteredForModule(ModuleIdentifier iD);
		ModuleIdentifier GetRegisteredIdentifierByName(string iD);
		ModuleIdentifier GetRegisteredIdentifierByTableName(string tableName, string countryCode = null);
		IEnumerable<ModuleIdentifier> GetRegisteredIdentifiersByTableName(string tableName, string countryCode = null);
		ModuleIdentifier GetRegisteredIdentifierByColumnNamePrefix(string prefix, string countryCode = null);
		IEnumerable<ModuleIdentifier> GetRegisteredIdentifiersByColumnNamePrefix(string prefix, string countryCode = null);
		IZModule Create(ModuleIdentifier iD);
		IZModule Create(ModuleIdentifier iD, string countryOverride);
		ModuleInfo GetRegisteredModuleInfo(ModuleIdentifier iD, string countryCode, bool useFallbackIfRegistrationForCountryNotFound);
	}
}
