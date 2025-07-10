using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	class CodeSmellsTest : TestCase
	{
		public void TestAllTypesAreInGuiNameSpace()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(StmPrintJobForm));
			ZStringBuilder builder = new ZStringBuilder();
			string errorMessage = "[{0}] Not in Enterprise.DocumentEngine.GUI namespace.";

			foreach (Type type in assembly.GetTypes())
			{
				if (!type.FullName.StartsWith("<PrivateImplementationDetails>")
					&& !type.ToString().Contains("Enterprise.DocumentEngine.GUI")
					&& type.Name != "CommonAssemblyInfo"
					&& !IsCompilerGenerated(type))
				{
					builder.Append(string.Format(errorMessage, type.ToString()));
				}
			}

			if (builder.Length > 0)
			{
				Assert(builder.ToStringWithNewLineBetweenAppends(), false);
			}
			else
			{
				Assert(true);
			}
		}

		static bool IsCompilerGenerated(Type type)
		{
			return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute))
				|| type.Name.StartsWith("<>");
		}
	}
}
