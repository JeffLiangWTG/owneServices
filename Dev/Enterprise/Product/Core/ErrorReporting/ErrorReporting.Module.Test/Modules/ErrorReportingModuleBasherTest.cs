using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ErrorReporting.Module.Test
{
	[TestedType(typeof(ErrorReportingModule))]
	public class ErrorReportingModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ErrorReporting;
		}
	}
}
