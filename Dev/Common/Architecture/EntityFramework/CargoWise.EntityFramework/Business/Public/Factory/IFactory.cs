using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IFactory : IBusinessObjectFactoryInternals
	{
		BusinessObject New(Type bizOType);
		void Save();

		T Load<T>(ZGuid pK) where T : class;

		/// <summary>
		/// Loads a BusinessObject with a given TablePrefix and PK. Returns null if the PK was not found.
		/// *** YOUR TABLEPREFIX MUST EXIST IN BusinessObjectPrefixTypesConfiguration.xml. ***
		/// </summary>
		T Load<T>(string tablePrefix, ZGuid pk) where T : class;
		T[] Load<T>(ZQuery query) where T : class;

		BusinessObject Load(Type bizOType, ZGuid pK);
		BusinessObject[] Load(Type bizOType, ZQuery sQLFilter);
		BusinessObject LoadTop1(Type bizOType, ZQuery sQLFilter);

		BusinessObject LoadFromNaturalKey(Type bizOType, SchemaColumn column, ZString naturalKeyValue);
		BusinessObject LoadFromUniqueKey(Type bizOType, SchemaColumn uniqueKeyColumn, IZType uniqueKeyValue);

		void ClearQueryCache();
		bool RefreshEnabled { get; set; }

		IFactoryChangeSet GetChanges();

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Do not want to refactor all upstream calls of IFactory.ReloadAll yet")]
		void ReloadAll<T>() where T : BusinessObject;
	}
}
