using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders
{
	public class DefinitionAssemblyLoader : IDefinitionLoader
	{
		const string DefinitionResourceSuffix = "SetDefinition.xml";

		#region Constructor

		public DefinitionAssemblyLoader()
		{
			this.assemblyNames = new[] { typeof(DefinitionAssemblyLoader).Assembly.FullName };
		}

		public DefinitionAssemblyLoader(string[] assemblyNames)
		{
			if (assemblyNames.IsEmpty())
			{
				this.assemblyNames = new[] { typeof(DefinitionAssemblyLoader).Assembly.FullName };
			}
			this.assemblyNames = assemblyNames;
		}

		#endregion

		#region IDefinitionLocator Members

		public bool HasDefinition(string entitySetName)
		{
			if (entitySetName.IsEmpty())
			{
				return false;
			}

			var resourceName = GetFileName(entitySetName);
			foreach (var assembly in Assemblies)
			{
				var names = assembly.GetManifestResourceNames();
				var resourceNames = names.Where(s => s.Contains(resourceName));
				return resourceNames.Any();
			}
			return false;
		}

		public byte[] Load(string entitySetName)
		{
			if (entitySetName.IsEmpty())
			{
				throw new NativeXMLUserVisibleException("Entity Set Name is Empty");
			}

			var resourceName = GetFileName(entitySetName);
			foreach (var assembly in Assemblies)
			{
				var names = assembly.GetManifestResourceNames();
				var resourceNames = names.Where(s => s.EndsWith("." + resourceName));
				if (resourceNames.Any())
				{
					resourceName = resourceNames.First();
					return LoadDefinition(resourceName, assembly);
				}
			}
			throw new NativeXMLUserVisibleException("EntitySet " + entitySetName + " not defined");
		}

		public bool SetAssemblies(string[] assemblyNames)
		{
			if (!this.assemblyNames.SequenceEqual(assemblyNames))
			{
				this.assemblyNames = assemblyNames;
				return true;
			}

			return false;
		}

		#endregion

		IEnumerable<Assembly> Assemblies
		{
			get
			{
				foreach (string assemblyName in assemblyNames)
				{
					yield return Assembly.Load(assemblyName);
				}
			}
		}
		#if DEBUG
		public
		#endif

		IEnumerable<string> assemblyNames { get; set; }

		byte[] LoadDefinition(string resourceName, Assembly assembly)
		{
			var resourceRetriver = new EmbeddedResourceRetriever(assembly);
			return resourceRetriver.GetBytes(resourceName);
		}

		internal string GetFileName(string name)
		{
			return name + DefinitionResourceSuffix;
		}
	}
}