using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository
{
	/// <summary>
	/// Object Pool for EntitySet Definitions - caches all entityset definitions
	/// </summary>
	public class EntitySetDefinitionCache : IDefinitionCache
	{
		#region Construction

		[ThreadSafe]
		static Lazy<EntitySetDefinitionCache> instance = new Lazy<EntitySetDefinitionCache>(() => new EntitySetDefinitionCache(), LazyThreadSafetyMode.PublicationOnly);

		public static EntitySetDefinitionCache GetInstance()
		{
			return instance.Value;
		}

		EntitySetDefinitionCache()
		{
			definitionPool = new ConcurrentDictionary<string, EntitySetDefinition>();
		}

		readonly ConcurrentDictionary<string, EntitySetDefinition> definitionPool;

		#endregion

		#region Special code for setting Alternate DefinitionLoader for Testing

#if DEBUG
		public static void ResetStaticCacheForTesting()
		{
			instance = new Lazy<EntitySetDefinitionCache>(() => new EntitySetDefinitionCache(), LazyThreadSafetyMode.PublicationOnly);
		}

		public static EntitySetDefinitionCache SetAlternateDefinitionLoaderForTesting(IDefinitionLoader loader)
		{
			var cache = GetInstance();
			cache.DefinitionLocator = loader;
			return cache;
		}
#endif

		#endregion

#if DEBUG
		public
		#endif
		IDefinitionLoader DefinitionLocator = new DefinitionAssemblyLoader();

		public IEnumerable<EntitySetDefinition> Definitions
		{
			get { return definitionPool.Values; }
		}

		public bool HasDefinition(string definitionName)
		{
			return definitionPool.ContainsKey(definitionName) || DefinitionLocator.HasDefinition(definitionName);
		}

		public EntitySetDefinition Find(string definitionName)
		{
			return definitionPool.GetOrAdd(definitionName, LoadDefinition);
		}

		public void SetAssemblies(string[] assemblyNames)
		{
			if (DefinitionLocator.SetAssemblies(assemblyNames))
			{
				definitionPool.Clear();
			}
		}

		public void Reset()
		{
		}

		EntitySetDefinition LoadDefinition(string definitionName)
		{
			var doc = DefinitionLocator.Load(definitionName);
			return CreateDefinition(doc);
		}

		EntitySetDefinition CreateDefinition(byte[] definitionData)
		{
			return new EntitySetDefinitionBuilder(definitionData).GetEntitySetDefinition();
		}
	}
}
