using System.Collections.Generic;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	sealed class WebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "ZClientWebEDI";
			yield return "CargoWise.ComponentModel";
			yield return "Enterprise.ZArchitecture.Core";
			yield return "Enterprise.ZArchitecture.GUI";
			yield return "Enterprise.ZArchitecture.Business";
			yield return "ZClientEDI.Business";
			yield return "CargoWise.Integration";
			yield return "Enterprise.ZArchitecture.Modules";
			yield return "Enterprise.ZArchitecture.Web.GUI";
			yield return "Enterprise.ZArchitecture.Web.GUI.Ajax";
			yield return "Enterprise.Tracking.Web";
			yield return "System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
		}

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "ClientExtensions", "EDI", "ZClientWebEDI", "ZClientWebEDI", "Web.config");

		protected override string DebugCompilationAssembliesXPath => "location/system.web/compilation/assemblies";
	}
}
