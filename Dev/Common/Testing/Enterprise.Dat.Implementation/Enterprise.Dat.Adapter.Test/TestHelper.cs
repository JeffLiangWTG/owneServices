using System;

namespace Enterprise.Dat.Adapter.Test
{
	public static class TestHelper
	{
		public static string GetSimpleSolutionFileContents(string projectName = "Test", string projectFilePath = @"Test\Test.csproj", Guid? projectGuid = null, Guid? projectReferenceGuid = null)
		{
			if (projectGuid == null)
			{
				projectGuid = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
			}

			if (projectReferenceGuid == null)
			{
				projectReferenceGuid = Guid.Parse("21982A2B-458A-415A-8BF7-A0CAEA2BAEC3");
			}

			return $@"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 14
VisualStudioVersion = 14.0.24720.0
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{{projectGuid.Value}}}"") = ""{projectName}"", ""{projectFilePath}"", ""{{{projectReferenceGuid.Value}}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{{projectReferenceGuid.Value}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{{projectReferenceGuid.Value}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{{projectReferenceGuid.Value}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{{projectReferenceGuid.Value}}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
";
		}
	}
}
