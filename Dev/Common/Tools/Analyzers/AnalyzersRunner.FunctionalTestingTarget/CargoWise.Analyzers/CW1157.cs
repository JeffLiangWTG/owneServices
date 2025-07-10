using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1157
	{
		public void BadCode()
		{
			// CW1157 Do not use System.AppDomain. It is not supported in .NET Core, and will be replaced.
			var appDomain = AppDomain.CreateDomain("");
		}
	}
}
