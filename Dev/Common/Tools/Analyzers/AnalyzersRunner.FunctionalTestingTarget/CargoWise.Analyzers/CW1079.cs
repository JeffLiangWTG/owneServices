using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1079
	{
		public void Method(Exception ex)
		{
			//CW1079:Do not compare on Exception.Message
			_ = ex.Message == "do not compare";
		}
	}
}
