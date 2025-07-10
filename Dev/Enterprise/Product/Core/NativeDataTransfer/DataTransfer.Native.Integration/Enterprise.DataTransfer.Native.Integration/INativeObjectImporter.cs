using System.IO;

namespace Enterprise.DataTransfer.Native.Integration
{
	public interface INativeObjectImporter
	{
		bool AlwaysUseProvidedPKs { get; set; }
		void Import(Stream stream, out string errorLog);
		void Import(Stream stream, out string errorLog, string[] assemblyNames);
	}
}