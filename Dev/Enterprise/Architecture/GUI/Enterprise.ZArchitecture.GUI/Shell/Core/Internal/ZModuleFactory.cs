using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Core.Modules;

namespace Enterprise.ZArchitecture.Modules
{
	public sealed class ZModuleFactory : IDCountryFactory<ModuleIdentifier, ModuleInfo, ModuleList>, IModuleFactory
	{
		ZModuleFactory() { }

		static readonly ZModuleFactory instance = new ZModuleFactory();

		public static ZModuleFactory Instance
		{
			get { return instance; }
		}

		/// <summary>
		/// You must Dispose any module that you create.
		/// </summary>
		public ZModule Create(ModuleIdentifier iD)
		{
			return (ZModule)CreateNew(iD);
		}

		public ModuleInfo GetRegisteredModuleInfo(ModuleIdentifier iD, string countryCode, bool useFallbackIfRegistrationForCountryNotFound)
		{
			return RegistrationList[iD, countryCode, useFallbackIfRegistrationForCountryNotFound];
		}

		public ModuleIdentifier GetRegisteredIdentifierByTableName(string tableName, string countryCode = null)
			=> GetRegisteredIdentifiersByTableName(tableName, countryCode)?.FirstOrDefault();

		public IEnumerable<ModuleIdentifier> GetRegisteredIdentifiersByTableName(string tableName, string countryCode = null)
			=> RegistrationList.GetRegisteredIdentifiersByTableName(tableName, countryCode);

		public ModuleIdentifier GetRegisteredIdentifierByColumnNamePrefix(string prefix, string countryCode = null)
			=> GetRegisteredIdentifiersByColumnNamePrefix(prefix, countryCode)?.FirstOrDefault();

		public IEnumerable<ModuleIdentifier> GetRegisteredIdentifiersByColumnNamePrefix(string prefix, string countryCode = null)
			=> RegistrationList.GetRegisteredIdentifiersByColumnNamePrefix(prefix, countryCode);

		public ZModule Create(ModuleIdentifier iD, string countryOverride)
		{
			return (ZModule)CreateNewWithCountry(iD, countryOverride);
		}

		IZModule IModuleFactory.Create(ModuleIdentifier iD)
		{
			return Create(iD);
		}

		IZModule IModuleFactory.Create(ModuleIdentifier iD, string countryOverride)
		{
			return Create(iD, countryOverride);
		}

		public string[] GetCountryOverridesRegisteredForModule(ModuleIdentifier iD)
		{
			return GetCountryOverridesRegistered(iD);
		}

		public bool IsZFilterModule(ModuleIdentifier id)
		{
			return IsModuleOfType<ZFilterModule>(id);
		}

		public bool IsZFilterGridModule(ModuleIdentifier id)
		{
			return IsModuleOfType<ZFilterGridModule>(id);
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The generic type can't be inferred, and this is a helpful method that should be public.")]
		public bool IsModuleOfType<T>(ModuleIdentifier id) where T : ZModule
		{
			var createType = GetType(id);
			return createType != null && typeof(T).IsAssignableFrom(createType);
		}

		public bool IsZPopupModuleNonSingleton(ModuleIdentifier id)
		{
			var createType = GetType(id);
			var result = (createType != null) && typeof(ZPopupModule).IsAssignableFrom(createType);
			if (result)
			{
				using (var module = (ZPopupModule)Create(id))
				{
					result = !module.IsSingletonModule;
				}
			}
			return result;
		}

		Type GetType(ModuleIdentifier id)
		{
			var info = RegistrationList[id, CountryCode];
			return (info != null) ? Type.GetType(info.TypePath) : null;
		}
	}
}
