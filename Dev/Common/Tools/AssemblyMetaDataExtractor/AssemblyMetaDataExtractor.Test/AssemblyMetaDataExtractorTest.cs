using System;
using Mono.Cecil;
using NUnit.Framework;

namespace AssemblyMetaDataExtractor.Test
{
	sealed class AssemblyMetaDataExtractorTest : TestCase
	{
		public void TestGenericToArrayConvertor()
		{
			var arrayAttrib =
				new CustomAttributeArgument(
					new TypeReference("a", "b", ModuleDefinition.CreateModule("a", ModuleKind.Dll), new AssemblyNameDefinition("a", new Version())),
					new[] { new CustomAttributeArgument(), new CustomAttributeArgument(), new CustomAttributeArgument() });

			var result = AssemblyMetaDataExtractor.Program.ToArray<object>(arrayAttrib);
			AssertEquals(3, result.Length);
		}
	}
}
