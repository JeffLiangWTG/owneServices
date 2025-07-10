using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace CargoWise.Common
{
	public static class AssemblyAccessor
	{
		public static Stream GetManifestResourceStreamFromAssemblyFile(string assemblyFilePath, string resourceName)
		{
#if NET
			// in NetCore, exe is just a way to run code in dll, so we need to read the dll for resources
			if (assemblyFilePath.EndsWith(".exe", System.StringComparison.InvariantCultureIgnoreCase))
			{
				assemblyFilePath = assemblyFilePath.Substring(0, assemblyFilePath.Length - 4) + ".dll";
			}
#endif

			using (var fs = File.OpenRead(assemblyFilePath))
			using (var per = new PEReader(fs))
			{
				var mr = per.GetMetadataReader();
				foreach (var resHandle in mr.ManifestResources)
				{
					var res = mr.GetManifestResource(resHandle);
					if (mr.StringComparer.Equals(res.Name, resourceName))
					{
						var resourceDirectory = per.GetSectionData(per.PEHeaders.CorHeader.ResourcesDirectory.RelativeVirtualAddress);
						var reader = resourceDirectory.GetReader((int)res.Offset, resourceDirectory.Length - (int)res.Offset);
						var size = reader.ReadUInt32();
						return new MemoryStream(reader.ReadBytes((int)size));
					}
				}

				return null;
			}
		}
	}
}
