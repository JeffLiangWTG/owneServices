using System.Collections.Generic;
using System.IO;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.PowerBiSync;
using CargoWise.BuildTools.Testing;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing
{
	class PowerBiSynchroniserTest : TestCase
	{
		public void TestAddReportLayoutFilesToRegistrationProject()
		{
			using (var temp = new TempDirectory())
			{
				try
				{
					MockSourceControl.Setup();

					var projectFilePath = Path.Combine(temp.DirectoryName, "project.csproj");
					var actual = "<!--Report Layout Files-->blahblah<!--Report Layout Files-->";
					BiFiles.SaveFile(projectFilePath, actual);

					var embeddedResourceTags = new List<string>() { "<Tag1>", "<Tag2>", "<Tag3>" };
					PowerBiSynchroniser.AddLayoutTagsToRegistrationsProject(embeddedResourceTags, projectFilePath);
					actual = File.ReadAllText(projectFilePath);

					var expected = @"<!--Report Layout Files-->
	<ItemGroup Condition=""$(Configuration) == 'Debug'""><Tag1><Tag2><Tag3>
	</ItemGroup>
	<!--Report Layout Files-->";
					AssertEquals("New registrations project file should include report layout files as embedded resources", expected, actual);
				}
				finally
				{
					MockSourceControl.TearDown();
				}
			}
		}
	}
}
