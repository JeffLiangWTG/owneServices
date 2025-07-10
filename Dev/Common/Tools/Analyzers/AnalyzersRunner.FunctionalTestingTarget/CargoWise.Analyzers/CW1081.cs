using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1081
	{
		public void Method()
		{
			//CW1081:IsSubClassOf Typeof BusinessObjectCollection Rule
			_ = GetType().IsSubclassOf(typeof(BusinessObjectCollection));
		}
	}
}
