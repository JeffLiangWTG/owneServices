using System.Data;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1108
	{
		public void Method()
		{
			//CW1108:Do Not Use DataSet
			_ = new DataSet();
		}
	}
}
