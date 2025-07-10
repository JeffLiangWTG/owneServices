#if DEBUG
using System.Collections.Generic;
#endif

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Loaders
{
	public interface IDefinitionLoader
	{
		bool HasDefinition(string entitySetName);
		byte[] Load(string entitySetName);
		bool SetAssemblies(string[] assemblyNames);
		#if DEBUG
		IEnumerable<string> assemblyNames { get; }
		#endif
	}
}