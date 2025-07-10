using System.IO;
using System.Reflection;
using CargoWise.BuildTools;

[assembly: AssemblyTitle("Copy DocumentXmls")]

namespace CopyDocumentXmls
{
	class Program
	{
		static void Main()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			ImportableBuild.CopyDocumentXmlToBin(binPath);
		}
	}
}
