using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BuildTools;
using Mono.Cecil;
using Mono.Cecil.Pdb;

[assembly: AssemblyTitle("CargoWise.NGenRoot.Generator")]

namespace CargoWise.NGenRoot.Generator
{
	class Program
	{
		static void Main(string[] args)
		{
#if NETFRAMEWORK
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#else
			var binPath = AppContext.BaseDirectory;
#endif
			var targetAssemblyFile = Path.Combine(binPath, "CargoWise.NGenRoot.dll");

			var assemblyReaderParameters = new ReaderParameters();
			assemblyReaderParameters.ReadSymbols = true;
			assemblyReaderParameters.SymbolReaderProvider = new PdbReaderProvider();
			assemblyReaderParameters.ReadWrite = true;
			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(targetAssemblyFile, assemblyReaderParameters))
			{
				((DefaultAssemblyResolver)assemblyDefinition.MainModule.AssemblyResolver).AddSearchDirectory(Path.GetDirectoryName(targetAssemblyFile));

				foreach (var assemblyName in Directory.GetFiles(binPath).Where(file => ShouldNGen(file)).Select(file => AsDotNetAssembly(file)).Where(item => item != null))
				{
					assemblyDefinition.MainModule.AssemblyReferences.Add(AssemblyNameReference.Parse(assemblyName.FullName));
				}

#if NETFRAMEWORK
				string keyPairFilePath = Path.Combine(Path.GetDirectoryName(binPath), "EDI-Release-private.snk");
				var writeParameters = new WriterParameters() { StrongNameKeyPair = new StrongNameKeyPair(File.ReadAllBytes(keyPairFilePath)) };
#else
				var writeParameters = new WriterParameters();
#endif

				writeParameters.WriteSymbols = true;
				writeParameters.SymbolWriterProvider = new PdbWriterProvider();
				assemblyDefinition.Write(writeParameters);
			}
		}

		static bool ShouldNGen(string file)
		{
			return BuildXml.Instance.NGen(Path.GetFileName(file).Replace(".XmlSerializers", ""));
		}

		static AssemblyName AsDotNetAssembly(string file)
		{
			try
			{
				var extension = Path.GetExtension(file);
				if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase) || extension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
				{
					return AssemblyName.GetAssemblyName(file);
				}
				else
				{
					return null;
				}
			}
			catch (BadImageFormatException)
			{
				return null;
			}
		}
	}
}
