using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Enterprise.Upgrades.UnitTestHelper
{
	public static class RuntimeCompilerHelper
	{
		public static IEnumerable<string> CreateTestAssembly(Func<string[]> referenceAssemblies, Func<string> code, string assemblyPath, bool generateExecutable = false)
		{
			using (var provider = CodeDomProvider.CreateProvider("CSharp"))
			{
				var options = new CompilerParameters();

				foreach (var refernenceAssembly in referenceAssemblies())
				{
					options.ReferencedAssemblies.Add(refernenceAssembly);
				}

				options.GenerateInMemory = false;
				options.IncludeDebugInformation = true;
				options.GenerateExecutable = generateExecutable;
				options.OutputAssembly = assemblyPath;

				var result = provider.CompileAssemblyFromSource(options, code());
				var output = new string[result.Output.Count];

				result.Output.CopyTo(output, 0);

				return output;
			}
		}
	}
}
